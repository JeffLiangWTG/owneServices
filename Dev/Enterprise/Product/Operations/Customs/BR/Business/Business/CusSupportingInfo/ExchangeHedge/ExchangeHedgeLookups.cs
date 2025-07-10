using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ExchangeHedgeLookups : CusSupportingInfoLookups
	{
		public ExchangeHedgeLookups(ExchangeHedge parent)
			: base(parent)
		{
		}

		public new ExchangeHedge Parent => base.Parent as ExchangeHedge;

		public CodeDescriptionPairList ExchangeHedgeList => Factory.GetCachedValue<ExchangeHedgeList>();

		public CodeDescriptionPairList FinancialInstitutionList => BRRefCusCodeListTypes.GetFinancialInstitutionList(Factory);

		public CodeDescriptionPairList ReasonTypeList => BRRefCusCodeListTypes.GetReasonTypeList(Factory);

		public CodeDescriptionPairList PaymentMethodList => BRRefCusCodeListTypes.GetExchangeHedgePaymentMethodList(Factory);
	}
}
