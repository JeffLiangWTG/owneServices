using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;
using GlbStaffWrapper = Enterprise.Customs.IT.Business.GlbStaffWrapper;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(Eur1RequestGridContextMenuComponent))]
sealed class Eur1RequestGridContextMenuComponentTest : GridContextMenuItemComponentAbstractTest<CusEntryHeader>
{
	public void TestConstructor_ParentMenuItem()
	{
		using var entriesGrid = new EntriesGridForTest(declaration);
		AssertExceptionThrown<ArgumentNullException>("parentMenuItem parameter is required", () => new Eur1RequestGridContextMenuComponent(entriesGrid, null));
	}

	public void TestEur1RequestContextMenuItemVisibility()
	{
		using var entriesGrid = new EntriesGridForTest(declaration);
		var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
		new Eur1RequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

		var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
		AssertNotNull("MenuItem", menuItem);

		parentMenuItem.ShowPopupMenu();
		AssertEquals("Visibility", expected: true, menuItem.Visible);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		parentMenuItem.ShowPopupMenu();
		AssertEquals("Visibility", expected: false, menuItem.Visible);
	}

	public void TestEur1RequestContextMenuItemEnable()
	{
		using var entriesGrid = new EntriesGridForTest(declaration);
		var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
		new Eur1RequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

		var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
		AssertNotNull("MenuItem", menuItem);

		entryHeader.MovementReferenceNumberSetter("MRN123434");
		parentMenuItem.ShowPopupMenu();
		AssertEquals("Enabled", expected: true, menuItem.Enabled);

		entryHeader.MovementReferenceNumberSetter(string.Empty);
		parentMenuItem.ShowPopupMenu();
		AssertEquals("Enabled", expected: false, menuItem.Enabled);
	}

	public void TestCreateEur1RequestMessage()
	{
		entryHeader.MovementReferenceNumberSetter("MRN123434");
		SetupCryptokiCertificate();

		using var entriesGrid = new EntriesGridForTest(declaration);
		var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
		new Eur1RequestGridContextMenuComponentForTest(entriesGrid, "PQE123", parentMenuItem: parentMenuItem).Initialize();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
		menuItem.PerformClick();
		AssertEquals("EUR1 Request has been sent to customs.", UnitTestUserNotification.Instance.LastMessage.Text);

		entryHeader.Messages.Reload(reLoadExistingRows: false);
		var eur1Message = entryHeader.Messages.GetLastMessageByType("EU1");
		AssertNotNull("EUR1 Message", eur1Message);
	}

	public void TestCreateEur1RequestMessage_AskForPinIfNotFoundInMemory()
	{
		entryHeader.MovementReferenceNumberSetter("MRN123434");
		SetupCryptokiCertificate();

		using var entriesGrid = new EntriesGridForTest(declaration);
		var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
		new Eur1RequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

		var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
		menuItem.PerformClick();

		AssertType<EnterCryptokiCertificatePinForm>("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
	}

	public void TestCreateEur1RequestMessage_DeclarationPendingChanges()
	{
		entryHeader.MovementReferenceNumberSetter("MRN123434");
		SetupCryptokiCertificate();

		using var entriesGrid = new EntriesGridForTest(declaration);
		var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
		new Eur1RequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

		var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

		AssertEquals("Precondition: declaration has changes", true, declaration.HasChanges);

		menuItem.PerformClick();

		AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestCreateEur1RequestMessage_DoesNotAskPinWhenUserHasAutomaticSignature()
	{
		entryHeader.MovementReferenceNumberSetter("MRN123434");
		SetupCryptokiCertificate();
		var staffWrapper = GlbStaffWrapper.Get(GlbStaff.CurrentUser);
		staffWrapper.AutomaticSignaturePasswordCollection.AddNew().IsConfigurationActive = true;

		using var entriesGrid = new EntriesGridForTest(declaration);
		var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
		new Eur1RequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

		var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
		menuItem.PerformClick();

		AssertEquals("EUR1 Request has been sent to customs.", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	void SetupCryptokiCertificate()
	{
		var currentStaff = GlbStaff.CurrentUser;
		var staffWrapper = GlbStaffWrapper.Get(currentStaff);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = ChipsetList.Codes.Bit4id;
		cryptokiCertificate.GP_CertificateSerialNumber = "0123456789";
	}

	protected override string MenuItemText => "EUR1 Request";

	protected override GridContextMenuItemComponent<CusEntryHeader> GetNewContextMenuItemComponent(ZGrid grid)
		=> new Eur1RequestGridContextMenuComponent(grid, new MenuItem("Request to Customs"));

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
}

sealed class Eur1RequestGridContextMenuComponentForTest : Eur1RequestGridContextMenuComponent
{
	public Eur1RequestGridContextMenuComponentForTest(ZGrid grid, string certificatePin = null, MenuItem parentMenuItem = null) : base(grid, parentMenuItem)
	{
		SetXadesCertificatePinHandler(new XadesCertificatePinHandlerForTest(null, "0123456789", new UserEnterableTokenPin { Pin = certificatePin }));
	}

	protected override IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateEur1RequestContext(CusEntryHeader entryHeader)
	{
		var provider = new GlbCertificateProvider();
		var mauCertificate = provider.GetMauCertificatePassword("1234");
		var cryptokeiCertificate = provider.GetCryptokiCertificate();

		var signedBytes = Encoding.UTF8.GetBytes("<soap></soap>");
		var xmlSignerMock = new Mock<IAidaXmlSigner>();
		xmlSignerMock
			.Setup(x => x.Sign(It.IsAny<byte[]>(), It.IsAny<ICryptokiGlbExternalPassword>(), It.IsAny<DateTime>()))
			.Returns(signedBytes);

		var contextMock = new Mock<IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader>>();
		contextMock.Setup(ctx => ctx.BusinessObject).Returns(entryHeader);
		contextMock.Setup(ctx => ctx.MessageParent).Returns(entryHeader);
		contextMock.Setup(ctx => ctx.CryptokiCertificate).Returns(cryptokeiCertificate);
		contextMock.Setup(ctx => ctx.MauCertificate).Returns(mauCertificate);
		contextMock.Setup(ctx => ctx.ServiceId).Returns("richiestaEur1");
		contextMock.Setup(ctx => ctx.XmlSigner).Returns(xmlSignerMock.Object);

		return contextMock.Object;
	}
}
