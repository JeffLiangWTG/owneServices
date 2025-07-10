using CargoWise.Integration;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business
{
	public class CusEntryPayInfoLookups : Customs.Business.CusEntryPayInfoLookups
	{
		public CusEntryPayInfoLookups(CusEntryPayInfo parent)
			: base(parent)
		{
		}

		public override ICodeDescriptionPairList TransactionTypeList => Factory.GetCachedValue<PaymentTransactionTypeList>();
		public override ICodeDescriptionPairList PaymentStatusList => Factory.GetCachedValue<PaymentStatusList>();
	}
}
