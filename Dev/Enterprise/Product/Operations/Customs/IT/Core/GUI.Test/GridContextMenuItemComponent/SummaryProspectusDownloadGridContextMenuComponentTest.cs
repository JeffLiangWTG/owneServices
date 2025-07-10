using System;
using System.Text;
using System.Windows.Forms;
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

[TestedType(typeof(SummaryProspectusDownloadGridContextMenuComponent))]
sealed class SummaryProspectusDownloadGridContextMenuComponentTest : GridContextMenuItemComponentAbstractTest<CusEntryHeader>
{
	public void TestSummaryProspectusDownloadContextMenuItem_Visible_Enable()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MergedLines.AddNew();

		using var entriesGrid = new EntriesGridForTest(declaration);
		new SummaryProspectusDownloadGridContextMenuComponent(entriesGrid).Initialize();

		Func<MenuItem> getMenuItem = () => GetSummaryProspectusDownloadMenuItem(entriesGrid);

		CombineAssertions("Summary Prospectus Download menu item for export declaration", () =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var menuItem = getMenuItem();

			Assert("Summary Prospectus Download menu item should not be visible", !menuItem.Visible);
			Assert("Summary Prospectus Download menu item should not be enabled", !menuItem.Enabled);
		});

		CombineAssertions("Summary Prospectus Download menu item for import declaration", () =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var menuItem = getMenuItem();

			Assert("Summary Prospectus Download menu item should be visible", menuItem.Visible);
			Assert("Summary Prospectus Download menu item should not be enabled", !menuItem.Enabled);
		});

		CombineAssertions("Summary Prospectus Download menu item for import declaration with processed summary prospectus request", () =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var message = entryHeader.Messages.AddNew();
			message.EM_MessageType = "SPR";
			message.EM_ReceiveTransmit = "RCV";

			var menuItem = getMenuItem();

			Assert("Summary Prospectus Download menu item should be visible", menuItem.Visible);
			Assert("Summary Prospectus Download menu item should be enabled", menuItem.Enabled);
		});
	}

	public void TestSummaryProspectusDownloadContextMenuItem_Click()
	{
		SetUpControlAndTestMenu(setupCert: true, setupAutoSig: false, pinInMem: true, rcvPrrMsg: true, saveForm: true, (ZGrid grid, CusEntryHeader entryHeader) =>
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var menuItem = GetSummaryProspectusDownloadMenuItem(grid);
			menuItem.PerformClick();

			AssertEquals("Summary Prospectus Download message has been sent to customs.", UnitTestUserNotification.Instance.LastMessage.Text);

			entryHeader.Messages.Reload(reLoadExistingRows: false);
			var summaryProspectusDownload = entryHeader.Messages.GetLastMessageByType("SPD");
			AssertNotNull("Summary Prospectus Download", summaryProspectusDownload);
		});
	}

	public void TestSummaryProspectusDownloadContextMenuItem_Click_AskForPinIfNotFoundInMemory()
	{
		SetUpControlAndTestMenu(setupCert: true, setupAutoSig: false, pinInMem: false, rcvPrrMsg: true, saveForm: true, (ZGrid grid, CusEntryHeader entryHeader) =>
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var menuItem = GetSummaryProspectusDownloadMenuItem(grid);
			menuItem.PerformClick();

			AssertType<EnterCryptokiCertificatePinForm>("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
		});
	}

	public void TestSummaryProspectusDownloadContextMenuItem_Click_DeclarationPendingChanges()
	{
		SetUpControlAndTestMenu(setupCert: true, setupAutoSig: false, pinInMem: true, rcvPrrMsg: true, saveForm: false, (ZGrid grid, CusEntryHeader entryHeader) =>
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var menuItem = GetSummaryProspectusDownloadMenuItem(grid);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			menuItem.PerformClick();

			AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	public void TestSummaryProspectusDownloadContextMenuItem_Click_DoesNotAskPinWhenUserHasAutomaticSignature()
	{
		SetUpControlAndTestMenu(setupCert: true, setupAutoSig: true, pinInMem: false, rcvPrrMsg: true, saveForm: true, (ZGrid grid, CusEntryHeader entryHeader) =>
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var menuItem = GetSummaryProspectusDownloadMenuItem(grid);
			menuItem.PerformClick();

			AssertEquals("Summary Prospectus Download message has been sent to customs.", UnitTestUserNotification.Instance.LastMessage.Text);

			entryHeader.Messages.Reload(reLoadExistingRows: false);
			var summaryProspectusDownload = entryHeader.Messages.GetLastMessageByType("SPD");
			AssertNotNull("Summary Prospectus Download", summaryProspectusDownload);
		});
	}

	protected override string MenuItemText => "Summary Prospectus Download";

	protected override GridContextMenuItemComponent<CusEntryHeader> GetNewContextMenuItemComponent(ZGrid grid) => new SummaryProspectusDownloadGridContextMenuComponent(grid);

	MenuItem GetSummaryProspectusDownloadMenuItem(ZGrid grid)
	{
		var ctxMenu = grid.ContextMenu;
		var menuItem = ctxMenu.MenuItems.FindByText(MenuItemText);
		AssertNotNull("Summary Prospectus Download should be available in menu", menuItem);

		ctxMenu.ShowPopupMenu();
		return menuItem;
	}

	void SetupCryptokiCertificate()
	{
		var currentStaff = GlbStaff.CurrentUser;
		var staffWrapper = GlbStaffWrapper.Get(currentStaff);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = ChipsetList.Codes.Bit4id;
		cryptokiCertificate.GP_CertificateSerialNumber = "0123456789";
	}

	void SetUpControlAndTestMenu(bool setupCert, bool setupAutoSig, bool pinInMem, bool rcvPrrMsg, bool saveForm, Action<ZGrid, CusEntryHeader> testMenu)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN123434");
		entryHeader.MergedLines.AddNew();

		if (setupCert)
		{
			SetupCryptokiCertificate();
		}

		if (setupAutoSig)
		{
			var staffWrapper = GlbStaffWrapper.Get(GlbStaff.CurrentUser);
			staffWrapper.AutomaticSignaturePasswordCollection.AddNew().IsConfigurationActive = true;
		}

		using var entriesGrid = new EntriesGridForTest(declaration);
		new SummaryProspectusDownloadGridContextMenuComponentForTest(entriesGrid, pinInMem ? "PQE123" : null).Initialize();

		if (rcvPrrMsg)
		{
			var message = entryHeader.Messages.AddNew();
			message.EM_MessageType = "SPR";
			message.EM_ReceiveTransmit = "RCV";
		}

		if (saveForm)
		{
			Factory.Save();
		}

		testMenu(entriesGrid, entryHeader);
	}
}

sealed class SummaryProspectusDownloadGridContextMenuComponentForTest : SummaryProspectusDownloadGridContextMenuComponent
{
	public SummaryProspectusDownloadGridContextMenuComponentForTest(ZGrid grid, string certificatePin = null) : base(grid)
	{
		SetXadesCertificatePinHandler(new XadesCertificatePinHandlerForTest(null, "0123456789", new UserEnterableTokenPin { Pin = certificatePin }));
	}

	protected override IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateSummaryProspectusDownloadContext(CusEntryHeader entryHeader)
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
		contextMock.Setup(ctx => ctx.ServiceId).Returns("download-ProspettoContabileSintesi");
		contextMock.Setup(ctx => ctx.XmlSigner).Returns(xmlSignerMock.Object);

		return contextMock.Object;
	}
}
