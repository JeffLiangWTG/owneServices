using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccTaxReturnLine : AutoAccTaxReturnLine, ICanApplyDataRefresh
	{
		public AccTaxReturnLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("TaxReturn")]
		public override ZGuid ARL_ATR_AccTaxReturn { get => base.ARL_ATR_AccTaxReturn; set => base.ARL_ATR_AccTaxReturn = value; }

		public virtual AccTaxReturn TaxReturn
		{
			get { return Factory.Load<AccTaxReturn>(ARL_ATR_AccTaxReturn); }
		}

		[ResourceStringData("AccTaxReturnLine|ARL_Address1", Caption = "Address Line 1")]
		public override ZString ARL_Address1 { get => base.ARL_Address1; set => base.ARL_Address1 = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_Address2", Caption = "Address Line 2")]
		public override ZString ARL_Address2 { get => base.ARL_Address2; set => base.ARL_Address2 = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_City", Caption = "City")]
		public override ZString ARL_City { get => base.ARL_City; set => base.ARL_City = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_State", Caption = "State")]
		public override ZString ARL_State { get => base.ARL_State; set => base.ARL_State = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_PostCode", Caption = "Post Code")]
		public override ZString ARL_PostCode { get => base.ARL_PostCode; set => base.ARL_PostCode = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_RN_NKCountryCode", Caption = "Country/Region")]
		public override ZString ARL_RN_NKCountryCode { get => base.ARL_RN_NKCountryCode; set => base.ARL_RN_NKCountryCode = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_TotalAmountIncludingTax", Caption = "Total Amount", FullDescription = "Total Amount including Tax")]
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal ARL_TotalAmountIncludingTax { get => base.ARL_TotalAmountIncludingTax; set => base.ARL_TotalAmountIncludingTax = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_OverriddenTotalAmountIncludingTax", Caption = "Ovr. Total Amt", FullDescription = "Overridden Total Amount including Tax")]
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal ARL_OverriddenTotalAmountIncludingTax { get => base.ARL_OverriddenTotalAmountIncludingTax; set => base.ARL_OverriddenTotalAmountIncludingTax = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_GSTAmount", Caption = "Tax Amount")]
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal ARL_GSTAmount { get => base.ARL_GSTAmount; set => base.ARL_GSTAmount = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_OverriddenGSTAmount", Caption = "Ovr. Tax Amt", FullDescription = "Overridden Tax Amount")]
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal ARL_OverriddenGSTAmount { get => base.ARL_OverriddenGSTAmount; set => base.ARL_OverriddenGSTAmount = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_PaymentsBasisWithholdingTaxAmount", Caption = "WHT Amount", FullDescription = "Payments Basis Withholding Tax Amount")]
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal ARL_PaymentsBasisWithholdingTaxAmount { get => base.ARL_PaymentsBasisWithholdingTaxAmount; set => base.ARL_PaymentsBasisWithholdingTaxAmount = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_OverriddenPaymentsBasisWithholdingTaxAmount", Caption = "Ovr. WHT Amt", FullDescription = "Overridden Payments Basis Withholding Tax Amount")]
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal ARL_OverriddenPaymentsBasisWithholdingTaxAmount { get => base.ARL_OverriddenPaymentsBasisWithholdingTaxAmount; set => base.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_Comment", Caption = "Comment", FullDescription = "Comment to describe reason for overriding amounts")]
		public override ZString ARL_Comment { get => base.ARL_Comment; set => base.ARL_Comment = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_SystemLastEditTimeUtc", Caption = "Last Edit UTC")]
		public override ZDateTime ARL_SystemLastEditTimeUtc { get => base.ARL_SystemLastEditTimeUtc; set => base.ARL_SystemLastEditTimeUtc = value; }

		[ResourceStringData("AccTaxReturnLine|ARL_SystemLastEditUser", Caption = "Last Edit User")]
		public override ZString ARL_SystemLastEditUser { get => base.ARL_SystemLastEditUser; set => base.ARL_SystemLastEditUser = value; }

		public int LocalCurrencyDecimals => TaxReturn.ComplianceReport.Company.GetLocalDecimals();

		public bool CanApplyDataRefresh(DataRefreshAction action, BusinessObject publisher) => false;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ARL_ATR_AccTaxReturn = Factory.NewWithValidTestData<AccTaxReturn>().PK;
		}
#endif
	}
}