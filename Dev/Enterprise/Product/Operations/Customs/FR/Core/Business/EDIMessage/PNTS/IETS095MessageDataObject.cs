using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS095;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS095MessageDataObject : PNTSMessageDataObject<Iets095>
	{
		public IETS095MessageDataObject(PNTSEDIMessage message) : base(message)
		{
		}

		protected override ZString GetCustomsStatusFromMessage() => MapCustomsStatusToCW1Status(ResponseMessage.Status);

		public ZString GetStatusReasonDescriptionFromMessage()
		{
			var statusReason = ResponseMessage.StatusReason;
			var statusReasonDescription = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, statusReason, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.TemporaryStorageStatusReason, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;
			return statusReasonDescription.IsEmpty ? statusReason : statusReasonDescription.ToString();
		}

		protected override FREDIMessagePrettier CreatePrettier()
		{
			return new IETS095MessagePrettier(this);
		}
	}
}
