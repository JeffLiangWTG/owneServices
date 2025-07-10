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

public class AccountingSummaryRequestGridContextMenuComponent : GridContextMenuItemComponent<CusEntryHeader>
{
	public AccountingSummaryRequestGridContextMenuComponent(ZGrid grid, MenuItem parentMenuItem) : base(grid, parentMenuItem)
	{
		Argument.NotNull(parentMenuItem, nameof(parentMenuItem));
	}

	protected override ZMenuItem GetMenuItem()
	{
		var menuItemText = ResString.GetMultilingualString("5C72CD31-4008-4774-9C7B-143CC8C68EA5", "Accounting summary request");
		return new ZMenuItem(menuItemText) { Name = "AccountingSummaryRequest" };
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

		SendAccountingSummaryRequest(entryHeader);
	}

	protected virtual IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> CreateAccountingSummaryRequestContext(CusEntryHeader entryHeader)
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new AccountingSummaryRequestContext(entryHeader, glbCertificateProvider);
	}

	bool IsImportDeclaration()
	{
		var declaration = EntryHeader?.Declaration;
		return declaration?.IsImport ?? false;
	}

	bool EntryHasMovementReferenceNumber() => EntryHeader is { MovementReferenceNumber.IsEmpty: false };

	void SendAccountingSummaryRequest(CusEntryHeader entryHeader)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var accountingSummaryRequestContext = CreateAccountingSummaryRequestContext(entryHeader);
			var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new AccountingSummaryRequestMessageCreationStrategy(factory, accountingSummaryRequestContext);
			messageGenerator.GenerateMessage();
			factory.Save();

			Globals.Message.Show(AccountingSummaryRequestSentMessageConfirmation);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	CusEntryHeader EntryHeader => DataContext;

	XadesCertificatePinHandler XadesCertificatePinHandler => xadesCertificatePinHandler ??= new XadesCertificatePinHandler();
	XadesCertificatePinHandler xadesCertificatePinHandler;

	ResourceString AccountingSummaryRequestSentMessageConfirmation => ResString.GetMultilingualString("C1622655-AA3F-4650-A2B1-D58102B4CA89", "Accounting summary request has been sent to customs.");

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

#endif
}
