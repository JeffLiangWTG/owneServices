using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

public class NC123MessageManager : BasePassarExportDeclarationMessageManager
{
	public NC123MessageManager(ExportDeclarationMessageSendingObject messageSender) : base(messageSender)
	{
	}

	protected override bool ShouldUpdateStatus => true;

	protected override Event DeclarationSentEvent => Events.DeclarationActivationSent;

	protected override ZString DeclarationSentEventReference => SendingObject.MessageType;

	protected override void BeforeGenerateMessage(DeclarationMessageSendingObject sendingObject)
	{
		base.BeforeGenerateMessage(sendingObject);

		if (SendingObject is ExportDeclarationMessageSendingObject exportDeclarationMessageSendingObject)
		{
			var sendingObjectParent = exportDeclarationMessageSendingObject.SendingObjectParent;
			var declaration = sendingObjectParent.ParentDeclaration;
			var sendingDeclaration = sendingObjectParent.SendingDeclaration;
			declaration.JE_DeclarationLanguage = sendingDeclaration.JE_DeclarationLanguage;
			declaration.JE_LocationOfGoods = sendingDeclaration.JE_LocationOfGoods;
			declaration.JE_TransportMode = sendingDeclaration.JE_TransportMode;
			declaration.JE_TransportMeans = sendingDeclaration.JE_TransportMeans;
			declaration.JE_VesselName = sendingObjectParent.SendingDeclaration.JE_VesselName;
			declaration.JE_RN_NKTransportNationality = sendingDeclaration.JE_RN_NKTransportNationality;
			declaration.JE_MasterBill = sendingDeclaration.JE_MasterBill;
			declaration.JE_VoyageFlightNo = sendingDeclaration.JE_VoyageFlightNo;
			exportDeclarationMessageSendingObject.Header.EntryInstruction.CEI_NextProcedure = exportDeclarationMessageSendingObject.NextProcedure;
		}
	}
}
