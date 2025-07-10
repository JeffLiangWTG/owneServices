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

public class Ucc6EADRequestGridContextMenuComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public Ucc6EADRequestGridContextMenuComponent(ZGrid grid, MenuItem parentMenuItem) : base(grid, parentMenuItem)
	{
		Argument.NotNull(parentMenuItem, nameof(parentMenuItem));
	}

	protected override ZMenuItem GetMenuItem()
	{
		var menuItemText = ResString.GetMultilingualString("567488A7-DFA4-4389-8927-62712BD4F469", "EAD Request");
		return new ZMenuItem(menuItemText) { Name = "EADRequest" };
	}

	protected override void Execute(ZMenuItem menuItem)
	{
		var entryHeader = EntryHeader;
		if (entryHeader?.Declaration == null)
		{
			return;
		}

		if (!IsFormPreSaved(entryHeader, menuItem))
		{
			return;
		}

		if (GlbStaff.CurrentUser.HasValidAutomaticSignaturePassword()
			|| XadesCertificatePinHandler.HandleTokenPin() == XadesCertificatePinHandler.XadesCertificatePinHandlerResult.Completed)
		{
			SendEADRequest(entryHeader);
		}
	}

	protected override bool IsMenuItemVisible(ZMenuItem menuItem)
	{
		return base.IsMenuItemVisible(menuItem) && IsUcc6ExportDeclaration();
	}

	protected override bool IsMenuItemEnabled(ZMenuItem menuItem)
	{
		return base.IsMenuItemEnabled(menuItem) && EntryHeaderHasMovementReferenceNumber();
	}

	protected virtual IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateEADRequestContext(CusEntryHeader entryHeader)
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new EadTadRequestContext<CusEntryHeader, CusEntryHeader>(entryHeader, entryHeader, glbCertificateProvider);
	}

	void SendEADRequest(CusEntryHeader entryHeader)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var eadRequestContext = CreateEADRequestContext(entryHeader);
			IOutgoingCustomsMessageCreationStrategy messageGenerator = new EadTadRequestMessageCreationStrategy<CusEntryHeader, CusEntryHeader>(factory, eadRequestContext, EDIMessageTypeList.Codes.EadRequest);
			messageGenerator.GenerateMessage();
			factory.Save();

			Globals.Message.Show(EADRequestSentMessage);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	bool IsUcc6ExportDeclaration() => EntryHeader?.Declaration is { IsUCC6AndIsExport: true };

	bool EntryHeaderHasMovementReferenceNumber() => EntryHeader is { MovementReferenceNumber.IsEmpty: false };

	CusEntryHeader EntryHeader => DataContext;

	ResourceString EADRequestSentMessage
		=> ResString.GetMultilingualString("61ECB751-B64E-4F3A-831F-C5C0A3A8A661", "EAD Request has been sent to customs.");

	XadesCertificatePinHandler XadesCertificatePinHandler
		=> xadesCertificatePinHandler ??= new XadesCertificatePinHandler();
	XadesCertificatePinHandler xadesCertificatePinHandler;

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

#endif
}
