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

public class Ucc6EFStatusRequestGridContextMenuComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public Ucc6EFStatusRequestGridContextMenuComponent(ZGrid grid, MenuItem parentMenuItem) : base(grid, parentMenuItem)
	{
		Argument.NotNull(parentMenuItem, nameof(parentMenuItem));
	}

	protected override ZMenuItem GetMenuItem()
	{
		var menuItemText = ResString.GetMultilingualString("F8156C68-2CDA-4B08-8A3E-5A24BC743235", "EF Status Request");
		return new ZMenuItem(menuItemText) { Name = "EFStatusRequest" };
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
			SendEFStatusRequest(entryHeader);
		}
	}

	protected override bool IsMenuItemVisible(ZMenuItem menuItem)
	{
		return base.IsMenuItemVisible(menuItem)
			&& (EntryHeader.Declaration?.IsUCC6 ?? false);
	}

	protected override bool IsMenuItemEnabled(ZMenuItem menuItem)
	{
		return base.IsMenuItemEnabled(menuItem) && (!EntryHeader?.MovementReferenceNumber.IsEmpty ?? false);
	}

	protected virtual IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateEFStatusRequestContext(CusEntryHeader entryHeader)
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new Ucc6ElectronicFolderStatusRequestContext(entryHeader, glbCertificateProvider);
	}

	#region Implementation

	void SendEFStatusRequest(CusEntryHeader entryHeader)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var efStatusRequestContext = CreateEFStatusRequestContext(entryHeader);
			var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new Ucc6EFStatusRequestMessageCreationStrategy(factory, efStatusRequestContext);
			messageGenerator.GenerateMessage();
			factory.Save();

			Globals.Message.Show(EFStatusRequestSentMessage);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	CusEntryHeader EntryHeader => DataContext;

	ResourceString EFStatusRequestSentMessage
		=> ResString.GetMultilingualString("A21FA9C5-DFD1-46E7-99D0-99B84D55534E", "EF Status Request has been sent to customs.");

	XadesCertificatePinHandler XadesCertificatePinHandler
		=> xadesCertificatePinHandler ?? (xadesCertificatePinHandler = new XadesCertificatePinHandler());
	XadesCertificatePinHandler xadesCertificatePinHandler;

	#endregion

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

#endif
}
