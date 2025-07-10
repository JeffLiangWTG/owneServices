using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgOpportunityValidation : OrgOpportunityValidation
	{
		public EDIOrgOpportunityValidation(AutoOrgOpportunity parent) : base(parent)
		{
		}

		protected override void CheckP8_DiscountAmount()
		{
			base.CheckP8_DiscountAmount();
			MandatoryValidation.CheckNotNegative(Parent.P8_DiscountAmountInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateP8_Calc_ContractValueLocalCurrency();
			ValidateP8_Calc_TotalLocalValueCurrency();
		}

		public new EDIOrgOpportunity Parent
		{
			get { return base.Parent as EDIOrgOpportunity; }
		}

		public void ValidateP8_Calc_ContractValueLocalCurrency()
		{
			ValidateCalculatedProperty(Parent.P8_Calc_ContractValueLocalCurrencyInfo);
		}

		protected void CheckP8_Calc_ContractValueLocalCurrency()
		{
			CheckOpportunityExchangeRateCompany(Parent.P8_Calc_ContractValueLocalCurrencyInfo, Parent.P8_Calc_ContractValueLocalCurrency);
		}

		public void ValidateP8_Calc_TotalLocalValueCurrency()
		{
			ValidateCalculatedProperty(Parent.P8_Calc_TotalLocalValueCurrencyInfo);
		}

		protected void CheckP8_Calc_TotalLocalValueCurrency()
		{
			CheckOpportunityExchangeRateCompany(Parent.P8_Calc_TotalLocalValueCurrencyInfo, Parent.P8_Calc_TotalLocalValueCurrency);
		}

		public static void CheckOpportunityExchangeRateCompany(ZPropertyInfo propertyInfo, ZString localCurrency)
		{
			var exchangeRateCompany = propertyInfo.BizObj.Factory.Load<GlbCompany>(EDIDataRegistry.Instance.OpportunityExchangeRateCompany.Value);

			if (exchangeRateCompany == null || exchangeRateCompany.GC_RX_NKLocalCurrency != localCurrency)
			{
				propertyInfo.AddWarning(Res.GetString("0798e595-e33f-421b-aef0-c3a3eea351da", "The Registry Item '{0}' is invalid, the exchange rate cannot be calculated.", EDIDataRegistry.Instance.OpportunityExchangeRateCompany.Caption));
			}
		}
	}
}


