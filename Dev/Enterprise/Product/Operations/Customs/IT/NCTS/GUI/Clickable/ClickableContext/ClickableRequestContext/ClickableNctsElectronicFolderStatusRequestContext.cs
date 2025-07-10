using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.NCTS.GUI;

#if DEBUG
class ClickableNctsElectronicFolderStatusRequestContext : ClickableContext
#else
sealed class ClickableNctsElectronicFolderStatusRequestContext : ClickableContext
#endif
{
	public ClickableNctsElectronicFolderStatusRequestContext(NctsHeader header) : base(header)
	{
	}

	#region ClickableContext

	public override ResourceString Caption => ResString.GetMultilingualString("C3B3EF54-608E-476D-9CFA-5684334D949C", "EF Status Request");

	public override string Name => "EfStatusRequest";

	public override bool Visible => IsPhase5DepartureHeader();

	public override bool Enabled => !Header.MovementReferenceNumber.IsEmpty;

	public override void Execute(IClickableItem clickableItem)
	{
		var nctsHeader = Header;
		if (nctsHeader is null)
		{
			return;
		}

		if (!clickableItem.IsFormPreSaved(MovementHeader))
		{
			return;
		}

		if (GlbStaff.CurrentUser.HasValidAutomaticSignaturePassword()
			|| XadesCertificatePinHandler.HandleTokenPin() == XadesCertificatePinHandler.XadesCertificatePinHandlerResult.Completed)
		{
			SendEFStatusRequest(nctsHeader);
		}
	}

	#endregion

	#region Implementation

	void SendEFStatusRequest(NctsHeader nctsHeader)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var efStatusRequestContext = CreateEFStatusRequestContext(nctsHeader);
			var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new NctsElectronicFolderStatusRequestMessageCreationStrategy(factory, efStatusRequestContext);
			messageGenerator.GenerateMessage();
			factory.Save();

			Globals.Message.Show(EFStatusRequestSentMessage);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	ResourceString EFStatusRequestSentMessage => ResString.GetMultilingualString("AE3FD237-BD33-4390-863E-E7A2910E1799", "EF Status Request has been sent to customs.");

	XadesCertificatePinHandler XadesCertificatePinHandler => xadesCertificatePinHandler ??= new XadesCertificatePinHandler();
	XadesCertificatePinHandler xadesCertificatePinHandler;

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

	protected virtual IDocumentManagementServiceRequestContext<NctsHeader, NctsDepartureMovementHeader> CreateEFStatusRequestContext(NctsHeader nctsHeader)
#else
	IDocumentManagementServiceRequestContext<NctsHeader, NctsDepartureMovementHeader> CreateEFStatusRequestContext(NctsHeader nctsHeader)
#endif
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new NctsElectronicFolderStatusRequestContext(nctsHeader, glbCertificateProvider);
	}

	#endregion
}
