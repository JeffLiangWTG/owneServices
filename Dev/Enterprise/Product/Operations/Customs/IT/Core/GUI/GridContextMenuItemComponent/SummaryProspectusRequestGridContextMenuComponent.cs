using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class SummaryProspectusRequestGridContextMenuComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public SummaryProspectusRequestGridContextMenuComponent(ZGrid grid, MenuItem parentMenuItem) : base(grid, parentMenuItem)
	{
		Argument.NotNull(parentMenuItem, nameof(parentMenuItem));
	}

	protected override ZMenuItem GetMenuItem()
	{
		var menuItemText = ResString.GetMultilingualString("22BC86AE-6826-4C57-A07D-08CC84366AE1", "Summary Prospectus Request");
		return new ZMenuItem(menuItemText) { Name = "SummaryProspectusRequest" };
	}

	protected override bool IsMenuItemVisible(ZMenuItem menuItem) => base.IsMenuItemVisible(menuItem) && IsImportDeclaration();

	protected override bool IsMenuItemEnabled(ZMenuItem menuItem) => base.IsMenuItemEnabled(menuItem) && EntryHasMovementReferenceNumber();

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

		SendSummaryProspectusRequest(entryHeader);
	}

	protected virtual IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateSummaryProspectusRequestContext(CusEntryHeader entryHeader)
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new SummaryProspectusRequestContext(entryHeader, glbCertificateProvider);
	}

	bool IsImportDeclaration()
	{
		var declaration = EntryHeader?.Declaration;
		return declaration?.IsImport ?? false;
	}

	bool EntryHasMovementReferenceNumber() => EntryHeader is { MovementReferenceNumber.IsEmpty: false };

	void SendSummaryProspectusRequest(CusEntryHeader entryHeader)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var summaryProspectusRequestContext = CreateSummaryProspectusRequestContext(entryHeader);
			var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new SummaryProspectusRequestMessageCreationStrategy(factory, summaryProspectusRequestContext);
			messageGenerator.GenerateMessage();
			factory.Save();

			Globals.Message.Show(SummaryProspectusRequestSentMessageConfirmation);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	CusEntryHeader EntryHeader => DataContext;

	XadesCertificatePinHandler XadesCertificatePinHandler => xadesCertificatePinHandler ??= new XadesCertificatePinHandler();
	XadesCertificatePinHandler xadesCertificatePinHandler;

	ResourceString SummaryProspectusRequestSentMessageConfirmation => ResString.GetMultilingualString("869BD81C-B18A-4EFB-96E8-8C4804B8EB77", "Summary Prospectus Request has been sent to customs.");

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

#endif
}
