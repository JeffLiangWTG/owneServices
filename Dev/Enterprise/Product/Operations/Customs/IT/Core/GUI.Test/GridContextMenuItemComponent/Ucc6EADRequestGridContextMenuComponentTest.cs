using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
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

[TestedType(typeof(Ucc6EADRequestGridContextMenuComponent))]
sealed class Ucc6EADRequestGridContextMenuComponentTest : GridContextMenuItemComponentAbstractTest<CusEntryHeader>
{
	public void TestConstructor_ParentMenuItem()
	{
		using var entriesGrid = new EntriesGridForTest(declaration);
		AssertExceptionThrown<ArgumentNullException>("parentMenuItem parameter is required", () => new Ucc6EADRequestGridContextMenuComponent(entriesGrid, null));
	}

	public void TestUcc6EADRequestContextMenuItemCaption()
	{
		using var entriesGrid = new EntriesGridForTest(declaration);

		var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
		new Ucc6EADRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

		var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
		AssertEquals("MenuItem Text", "EAD Request", menuItem.Text);
	}

	public void TestUcc6EADRequestContextMenuItemVisibility()
	{
		using var entriesGrid = new EntriesGridForTest(declaration);

		var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
		new Ucc6EADRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

		var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
		AssertNotNull("MenuItem", menuItem);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		parentMenuItem.ShowPopupMenu();
		AssertEquals("When declaration is Import, Visible", expected: false, menuItem.Visible);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			parentMenuItem.ShowPopupMenu();
			AssertEquals("When declaration is Export UCC5, Visible", expected: false, menuItem.Visible);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			parentMenuItem.ShowPopupMenu();
			AssertEquals("When declaration is Export UCC6, Visible", expected: true, menuItem.Visible);
		}
	}

	public void TestUcc6EADRequestContextMenuItemEnabled()
	{
		using var entriesGrid = new EntriesGridForTest(declaration);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
		new Ucc6EADRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

		var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
		AssertNotNull("MenuItem", menuItem);

		parentMenuItem.ShowPopupMenu();
		AssertEquals("Enabled", expected: false, menuItem.Enabled);

		entryHeader.MovementReferenceNumberSetter("REF1234433");
		parentMenuItem.ShowPopupMenu();
		AssertEquals("Enabled", expected: true, menuItem.Enabled);
	}

	public void TestUcc6EADRequestContextMenuItem_CreateEADRequestMessage()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		entryHeader.MovementReferenceNumberSetter("MRN12345");
		SetupCryptokiCertificate();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new Ucc6EADRequestGridContextMenuComponentForTest(entriesGrid, "PQE123", parentMenuItem: parentMenuItem).Initialize();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			menuItem.PerformClick();
			AssertEquals("EAD Request has been sent to customs.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestUcc6EADRequestContextMenuItemCreateMessage_AskForPinIfNotFoundInMemory()
	{
		SetupCryptokiCertificate();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new Ucc6EADRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			menuItem.PerformClick();

			AssertType<EnterCryptokiCertificatePinForm>("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
		}
	}

	public void TestUcc6EADRequestContextMenuItemCreateMessage_DeclarationPendingChanges()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			entryHeader.Declaration.HasChanges = true;

			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new Ucc6EADRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			menuItem.PerformClick();

			AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestUcc6EADRequestContextMenuItemCreateMessage_DoesNotAskPinWhenUserHasAutomaticSignature()
	{
		SetupCryptokiCertificate();
		var staffWrapper = GlbStaffWrapper.Get(GlbStaff.CurrentUser);
		staffWrapper.AutomaticSignaturePasswordCollection.AddNew().IsConfigurationActive = true;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new Ucc6EADRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			menuItem.PerformClick();

			AssertEquals("EAD Request has been sent to customs.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	void SetupCryptokiCertificate()
	{
		var currentStaff = GlbStaff.CurrentUser;
		var staffWrapper = GlbStaffWrapper.Get(currentStaff);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = ChipsetList.Codes.Bit4id;
		cryptokiCertificate.GP_CertificateSerialNumber = "0123456789";
	}

	protected override GridContextMenuItemComponent<CusEntryHeader> GetNewContextMenuItemComponent(ZGrid grid)
		=> new Ucc6EADRequestGridContextMenuComponent(grid, new MenuItem("Request to Customs"));

	protected override string MenuItemText => "EAD Request";

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
}

sealed class Ucc6EADRequestGridContextMenuComponentForTest : Ucc6EADRequestGridContextMenuComponent
{
	public Ucc6EADRequestGridContextMenuComponentForTest(ZGrid grid, string certificatePin = null, MenuItem parentMenuItem = null) : base(grid, parentMenuItem)
	{
		SetXadesCertificatePinHandler(new XadesCertificatePinHandlerForTest(null, "0123456789", new UserEnterableTokenPin { Pin = certificatePin }));
	}

	protected override IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateEADRequestContext(CusEntryHeader entryHeader)
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
		contextMock.Setup(ctx => ctx.ServiceId).Returns("SERVICE_ID_EAD");
		contextMock.Setup(ctx => ctx.XmlSigner).Returns(xmlSignerMock.Object);

		return contextMock.Object;
	}
}
