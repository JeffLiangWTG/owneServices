using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class AccountingSummaryDownloadGridContextMenuComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public AccountingSummaryDownloadGridContextMenuComponent(ZGrid grid, MenuItem parentMenuItem = null) : base(grid, parentMenuItem)
	{
	}

	protected override ZMenuItem GetMenuItem()
	{
		var menuItemText = ResString.GetMultilingualString("01FDAA75-1C0C-4ED5-845F-7827A68FA2FF", "Accounting Summary Download");
		return new ZMenuItem(menuItemText) { Name = "AccountingSummaryDownload" };
	}

	protected override bool IsMenuItemVisible(ZMenuItem menuItem) => base.IsMenuItemVisible(menuItem) && IsImportDeclaration();

	protected override bool IsMenuItemEnabled(ZMenuItem menuItem) => base.IsMenuItemEnabled(menuItem) && EntryReceivedAccountSummaryRequest();

	protected override void Execute(ZMenuItem menuItem)
	{
		var entryHeader = EntryHeader;
		if (entryHeader == null)
		{
			return;
		}

		if (!IsFormPreSaved(entryHeader, menuItem))
		{
			return;
		}

		if (!GlbStaff.CurrentUser.HasValidAutomaticSignaturePassword()
			&& XadesCertificatePinHandler.HandleTokenPin() != XadesCertificatePinHandler.XadesCertificatePinHandlerResult.Completed)
		{
			return;
		}

		GenerateAccountingSummaryDownloadRequest(entryHeader);
	}

	protected virtual IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateAccountingSummaryDownloadContext(CusEntryHeader entryHeader)
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new AccountingSummaryDownloadContext(entryHeader, glbCertificateProvider);
	}

	bool IsImportDeclaration() => EntryHeader?.Declaration?.IsImport ?? false;

	bool EntryReceivedAccountSummaryRequest() => EntryHeader?.Messages?.Cast<EDIMessage>().Any(IsProcessedAccountSummaryRequest) ?? false;

	bool IsProcessedAccountSummaryRequest(EDIMessage msg) => msg.EM_MessageType == EDIMessageTypeList.Codes.AccountingSummaryRequest && !msg.IsTransmitMessage;

	void GenerateAccountingSummaryDownloadRequest(CusEntryHeader entryHeader)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var ctx = CreateAccountingSummaryDownloadContext(entryHeader);
			var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new AccountingSummaryDownloadMessageCreationStrategy(factory, ctx);
			messageGenerator.GenerateMessage();
			factory.Save();

			Globals.Message.Show(AccountingSummaryDownloadRequestGeneratedConfirmation);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	CusEntryHeader EntryHeader => DataContext;

	XadesCertificatePinHandler XadesCertificatePinHandler => xadesCertificatePinHandler ??= new XadesCertificatePinHandler();
	XadesCertificatePinHandler xadesCertificatePinHandler;

	ResourceString AccountingSummaryDownloadRequestGeneratedConfirmation => ResString.GetMultilingualString("B5D02568-3871-4D63-8125-2B1D5AAD80D7", "Accounting summary download message has been sent to customs.");

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

#endif
}
