using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyRefundRequestMessageSendingObjectLookups : ZLookups
	{
		public PenaltyRefundRequestMessageSendingObjectLookups(PenaltyRefundRequestMessageSendingObject parent)
			: base(parent)
		{
		}
		public CodeDescriptionPairList RefundCauseCodeList => Factory.GetCachedValue<RefundCauseCodeList>();
		public CodeDescriptionPairList RefundReasonCodeList => Factory.GetCachedValue<RefundReasonCodeList>();
		public ZZRefCusCodeListCombinedCollection TaxOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.TaxOffice, ZDateTime.Today);
	}
}
