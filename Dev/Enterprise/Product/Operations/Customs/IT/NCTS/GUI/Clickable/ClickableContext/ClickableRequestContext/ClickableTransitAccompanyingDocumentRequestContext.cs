using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.NCTS.GUI;

#if DEBUG
class ClickableTransitAccompanyingDocumentRequestContext : ClickableContext
#else
sealed class ClickableTransitAccompanyingDocumentRequestContext : ClickableContext
#endif
{
	public ClickableTransitAccompanyingDocumentRequestContext(NctsHeader header) : base(header)
	{
	}

	#region ClickableContext

	public override ResourceString Caption => ResString.GetMultilingualString("5D21D014-A5CA-47F1-BFCE-F2B6A218B718", "TAD Request");

	public override string Name => "TadRequest";

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
			SendTadRequest(nctsHeader);
		}
	}

	#endregion

	#region Implementation

	void SendTadRequest(NctsHeader nctsHeader)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var requestContext = CreateRequestContext(nctsHeader);
			IOutgoingCustomsMessageCreationStrategy messageGenerator = new EadTadRequestMessageCreationStrategy<NctsHeader, NctsDepartureMovementHeader>(factory, requestContext, EDIMessageTypeList.Codes.TadRequest);
			_ = messageGenerator.GenerateMessage();
			factory.Save();

			Globals.Message.Show(TadRequestSentMessage);
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	ResourceString TadRequestSentMessage => ResString.GetMultilingualString("34FD24A7-ADE2-47CB-A9AB-B7008993B268", "TAD Request has been sent to customs.");

	XadesCertificatePinHandler XadesCertificatePinHandler => xadesCertificatePinHandler ??= new XadesCertificatePinHandler();
	XadesCertificatePinHandler xadesCertificatePinHandler;

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

	protected virtual IDocumentManagementServiceRequestContext<NctsHeader, NctsDepartureMovementHeader> CreateRequestContext(NctsHeader nctsHeader)
#else
	IDocumentManagementServiceRequestContext<NctsHeader, NctsDepartureMovementHeader> CreateRequestContext(NctsHeader nctsHeader)
#endif
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new EadTadRequestContext<NctsHeader, NctsDepartureMovementHeader>(nctsHeader, nctsHeader.MovementHeader, glbCertificateProvider);
	}

	#endregion
}
