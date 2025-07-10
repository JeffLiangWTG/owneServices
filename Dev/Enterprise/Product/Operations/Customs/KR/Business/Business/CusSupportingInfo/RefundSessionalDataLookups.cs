using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class RefundSessionalDataLookups : CusSupportingInfoLookups
	{
		public RefundSessionalDataLookups(RefundSessionalData parent) : base(parent)
		{
		}
		protected new RefundSessionalData Parent
		{
			get { return (RefundSessionalData)base.Parent; }
		}

		public CodeDescriptionPairList RefundCauseCodeList => Factory.GetCachedValue<RefundCauseCodeList>();
		public CodeDescriptionPairList RefundReasonCodeList => Factory.GetCachedValue<RefundReasonCodeList>();
		public CodeDescriptionPairList RefundTypeList => Factory.GetCachedValue<RefundTypeList>();
		public CodeDescriptionPairList YesNoList => Factory.GetCachedValue<YesNoList>();
		public CodeDescriptionPairList CustomsDisbursementBillNumberList
		{
			get
			{
				return Factory.GetCachedValue(Parent.Entry?.EntryNumber ?? Parent.PK.ToStringKey(), () =>
				{
					var result = new CodeDescriptionPairList();
					if (Parent.CustomsDisbursementBills != null)
					{
						foreach (var customsDisbursementBill in Parent.CustomsDisbursementBills)
						{
							result.AddPair(customsDisbursementBill.KEB_CustomsDisbursementBillNumber);
						}
					}
					return result;
				});
			}
		}
	}
}
