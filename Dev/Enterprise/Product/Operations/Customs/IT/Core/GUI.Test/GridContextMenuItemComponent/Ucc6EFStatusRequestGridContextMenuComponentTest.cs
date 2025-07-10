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
using ICryptokiGlbExternalPassword = Enterprise.Customs.IT.Business.ICryptokiGlbExternalPassword;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(Ucc6EFStatusRequestGridContextMenuComponent))]
sealed class Ucc6EFStatusRequestGridContextMenuComponentTest : GridContextMenuItemComponentAbstractTest<CusEntryHeader>
{
	public void TestConstructor_ParentMenuItem()
	{
		using var entriesGrid = new EntriesGridForTest(declaration);
		AssertExceptionThrown<ArgumentNullException>("parentMenuItem parameter is required", () => new Ucc6EFStatusRequestGridContextMenuComponent(entriesGrid, null));
	}

	public void TestUcc6EFStatusRequestContextMenuItemVisibility()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new Ucc6EFStatusRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			AssertNotNull("MenuItem", menuItem);

			parentMenuItem.ShowPopupMenu();
			AssertEquals("Visibility", false, menuItem.Visible);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			parentMenuItem.ShowPopupMenu();
			AssertEquals("Visibility", true, menuItem.Visible);
		}
	}

	public void TestUcc6EFStatusRequestContextMenuItemEnable()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new Ucc6EFStatusRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			AssertNotNull("MenuItem", menuItem);

			parentMenuItem.ShowPopupMenu();
			AssertEquals("Enabled", false, menuItem.Enabled);

			entryHeader.MovementReferenceNumberSetter("REF1234433");
			parentMenuItem.ShowPopupMenu();
			AssertEquals("Enabled", true, menuItem.Visible);
		}
	}

	public void TestUcc6EFStatusRequestContextMenuItemCreateEFStatusRequestMessage()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		entryHeader.MovementReferenceNumberSetter("MRN12345");
		SetupCryptokiCertificate();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new Ucc6EFStatusRequestGridContextMenuComponentForTest(entriesGrid, "PQE123", parentMenuItem: parentMenuItem).Initialize();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			menuItem.PerformClick();
			AssertEquals("EF Status Request has been sent to customs.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestUcc6EFStatusRequestContextMenuItemCreateMessage_AskForPinIfNotFoundInMemory()
	{
		SetupCryptokiCertificate();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new Ucc6EFStatusRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			menuItem.PerformClick();

			AssertType<EnterCryptokiCertificatePinForm>("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
		}
	}

	public void TestUcc6EFStatusRequestContextMenuItemCreateMessage_DeclarationPendingChanges()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new Ucc6EFStatusRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();
			entryHeader.Declaration.HasChanges = true;

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			menuItem.PerformClick();

			AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestUcc6EFStatusRequestContextMenuItemCreateMessage_DoesNotAskPinWhenUserHasAutomaticSignature()
	{
		SetupCryptokiCertificate();
		var staffWrapper = GlbStaffWrapper.Get(GlbStaff.CurrentUser);
		staffWrapper.AutomaticSignaturePasswordCollection.AddNew().IsConfigurationActive = true;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new Ucc6EFStatusRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			menuItem.PerformClick();

			AssertEquals("EF Status Request has been sent to customs.", UnitTestUserNotification.Instance.LastMessage.Text);
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
		=> new Ucc6EFStatusRequestGridContextMenuComponent(grid, new MenuItem("Request to Customs"));

	protected override string MenuItemText => "EF Status Request";

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
}

sealed class Ucc6EFStatusRequestGridContextMenuComponentForTest : Ucc6EFStatusRequestGridContextMenuComponent
{
	public Ucc6EFStatusRequestGridContextMenuComponentForTest(ZGrid grid, string certificatePin = null, MenuItem parentMenuItem = null) : base(grid, parentMenuItem)
	{
		SetXadesCertificatePinHandler(new XadesCertificatePinHandlerForTest(null, "0123456789", new UserEnterableTokenPin { Pin = certificatePin }));
	}

	protected override IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateEFStatusRequestContext(CusEntryHeader entryHeader)
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
		contextMock.Setup(ctx => ctx.ServiceId).Returns("SERVICE_ID_EFQ");
		contextMock.Setup(ctx => ctx.XmlSigner).Returns(xmlSignerMock.Object);

		return contextMock.Object;
	}
}
