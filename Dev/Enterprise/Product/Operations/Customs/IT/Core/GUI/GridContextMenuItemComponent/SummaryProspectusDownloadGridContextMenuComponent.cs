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

public class SummaryProspectusDownloadGridContextMenuComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public SummaryProspectusDownloadGridContextMenuComponent(ZGrid grid, MenuItem parentMenuItem = null) : base(grid, parentMenuItem)
	{
	}

	protected override ZMenuItem GetMenuItem()
	{
		var menuItemText = ResString.GetMultilingualString("F1F99ECC-6BA8-461B-BAA6-CB3425902678", "Summary Prospectus Download");
		return new ZMenuItem(menuItemText) { Name = "SummaryProspectusDownload" };
	}

	protected override bool IsMenuItemVisible(ZMenuItem menuItem) => base.IsMenuItemVisible(menuItem) && IsImportDeclaration();

	protected override bool IsMenuItemEnabled(ZMenuItem menuItem) => base.IsMenuItemEnabled(menuItem) && EntryReceivedSummaryProspectusRequest();

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

		GenerateSummaryProspectusDownloadRequest(entryHeader);
	}

	protected virtual IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateSummaryProspectusDownloadContext(CusEntryHeader entryHeader)
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new SummaryProspectusDownloadContext(entryHeader, glbCertificateProvider);
	}

	bool IsImportDeclaration() => EntryHeader?.Declaration?.IsImport ?? false;

	bool EntryReceivedSummaryProspectusRequest() => EntryHeader?.Messages?.Cast<EDIMessage>().Any(IsProcessedSummaryProspectusRequest) ?? false;

	bool IsProcessedSummaryProspectusRequest(EDIMessage msg) => msg.EM_MessageType == EDIMessageTypeList.Codes.SummaryProspectusRequest && !msg.IsTransmitMessage;

	void GenerateSummaryProspectusDownloadRequest(CusEntryHeader entryHeader)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var ctx = CreateSummaryProspectusDownloadContext(entryHeader);
			var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new SummaryProspectusDownloadMessageCreationStrategy(factory, ctx);
			messageGenerator.GenerateMessage();
			factory.Save();

			Globals.Message.Show(SummaryProspectusDownloadRequestGeneratedConfirmation);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	CusEntryHeader EntryHeader => DataContext;

	XadesCertificatePinHandler XadesCertificatePinHandler => xadesCertificatePinHandler ??= new XadesCertificatePinHandler();
	XadesCertificatePinHandler xadesCertificatePinHandler;

	ResourceString SummaryProspectusDownloadRequestGeneratedConfirmation => ResString.GetMultilingualString("6860E0B4-DEC1-475A-9EDC-A5B502C297E1", "Summary Prospectus Download message has been sent to customs.");

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

#endif
}

