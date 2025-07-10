using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public abstract class PNTSMessageDataObject<TMessage> : MessageDataObject<TMessage>
		where TMessage : class
	{
		public PNTSMessageDataObject(PNTSEDIMessage message) : base(message)
		{
		}

		public new PNTSMessagePrettier<TMessage> Prettier => (PNTSMessagePrettier<TMessage>)base.Prettier;

		#region CustomsStatus

		public ZString CustomsStatus => GetCustomsStatusFromMessage();

		public ZString GetCustomsStatusDescriptionFromMessage()
		{
			var customsStatus = CustomsStatus;
			var cw1Description = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, customsStatus, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.TemporaryStorageCustomsStatus, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;
			return cw1Description.IsEmpty ? customsStatus : cw1Description;
		}

		protected abstract ZString GetCustomsStatusFromMessage();

		protected ZString MapCustomsStatusToCW1Status(ZString customsStatus)
		{
			return ZZRefCusMapCombined.MapCustomsCodeToCW1Code(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusMapTypeList.Codes.PNTSS, customsStatus, ZDateTime.Today);
		}
		#endregion
	}
}
