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

public class Eur1RequestGridContextMenuComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public Eur1RequestGridContextMenuComponent(ZGrid grid, MenuItem parentMenuItem) : base(grid, parentMenuItem)
	{
		Argument.NotNull(parentMenuItem, nameof(parentMenuItem));
	}

	protected override ZMenuItem GetMenuItem()
	{
		var menuItemText = ResString.GetMultilingualString("C957F334-27EC-4035-B69E-AC70A00484B2", "EUR1 Request");
		return new ZMenuItem(menuItemText) { Name = "Eur1Request" };
	}

	protected override bool IsMenuItemVisible(ZMenuItem menuItem)
	{
		return base.IsMenuItemVisible(menuItem) && IsExportDeclaration();
	}

	protected override bool IsMenuItemEnabled(ZMenuItem menuItem)
	{
		return base.IsMenuItemEnabled(menuItem) && EntryHasMovementReferenceNumber();
	}

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

		SendEur1Request(entryHeader);
	}

	protected virtual IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateEur1RequestContext(CusEntryHeader entryHeader)
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new Eur1RequestContext(entryHeader, glbCertificateProvider);
	}

	bool IsExportDeclaration()
	{
		var declaration = EntryHeader?.Declaration;
		return declaration?.IsExport ?? false;
	}

	bool EntryHasMovementReferenceNumber()
		=> EntryHeader is CusEntryHeader entryHeader && !entryHeader.MovementReferenceNumber.IsEmpty;

	void SendEur1Request(CusEntryHeader entryHeader)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var efStatusRequestContext = CreateEur1RequestContext(entryHeader);
			var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new Eur1RequestMessageCreationStrategy(factory, efStatusRequestContext);
			messageGenerator.GenerateMessage();
			factory.Save();

			Globals.Message.Show(Eur1RequestSentMessageConfirmation);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	CusEntryHeader EntryHeader => DataContext;

	XadesCertificatePinHandler XadesCertificatePinHandler => xadesCertificatePinHandler ??= new XadesCertificatePinHandler();
	XadesCertificatePinHandler xadesCertificatePinHandler;

	ResourceString Eur1RequestSentMessageConfirmation => ResString.GetMultilingualString("36AD9536-9412-46C1-8D3F-85B07553743F", "EUR1 Request has been sent to customs.");

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

#endif
}
