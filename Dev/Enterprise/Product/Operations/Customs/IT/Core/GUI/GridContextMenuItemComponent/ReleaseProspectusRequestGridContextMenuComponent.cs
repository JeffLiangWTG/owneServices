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

public class ReleaseProspectusRequestGridContextMenuComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public ReleaseProspectusRequestGridContextMenuComponent(ZGrid grid, MenuItem parentMenuItem) : base(grid, parentMenuItem)
	{
		Argument.NotNull(parentMenuItem, nameof(parentMenuItem));
	}

	protected override ZMenuItem GetMenuItem()
	{
		var menuItemText = ResString.GetMultilingualString("87AE3012-630A-4688-8B90-4AE259061F25", "Release prospectus");
		return new ZMenuItem(menuItemText) { Name = "ReleaseProspectus" };
	}

	protected override bool IsMenuItemVisible(ZMenuItem menuItem)
	{
		return base.IsMenuItemVisible(menuItem) && IsImportDeclaration();
	}

	protected override bool IsMenuItemEnabled(ZMenuItem menuItem)
	{
		return base.IsMenuItemEnabled(menuItem) && HasEntryNumber();
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

		SendReleaseProspectusRequest(entryHeader);
	}

	protected virtual IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateReleaseProspectusRequestContext(CusEntryHeader entryHeader)
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new ReleaseProspectusRequestContext(entryHeader, glbCertificateProvider);
	}

	bool IsImportDeclaration()
	{
		var declaration = EntryHeader?.Declaration;
		return declaration?.IsImport ?? false;
	}

	bool HasEntryNumber()
	{
		return !EntryHeader?.EntryNumbersProvider?.ReleaseInfo?.CE_EntryNum.IsEmpty ?? false;
	}

	void SendReleaseProspectusRequest(CusEntryHeader entryHeader)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var releaseProspectusRequestContext = CreateReleaseProspectusRequestContext(entryHeader);
			var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new ReleaseProspectusRequestMessageCreationStrategy(factory, releaseProspectusRequestContext);
			messageGenerator.GenerateMessage();
			factory.Save();

			Globals.Message.Show(ReleaseProspectusRequestSentMessageConfirmation);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	CusEntryHeader EntryHeader => DataContext;

	XadesCertificatePinHandler XadesCertificatePinHandler => xadesCertificatePinHandler ??= new XadesCertificatePinHandler();
	XadesCertificatePinHandler xadesCertificatePinHandler;

	ResourceString ReleaseProspectusRequestSentMessageConfirmation => ResString.GetMultilingualString("7E13273C-0465-4136-84E6-FAF96C688630", "Release Prospectus Request has been sent to customs.");

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

#endif
}
