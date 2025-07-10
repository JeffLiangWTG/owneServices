using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Accounting.Business.ComplianceReport.TPAR
{
	public class TparReportCreditorLine : AccTaxReturnLine
	{
		public TparReportCreditorLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool IsSavedByFactory => base.IsSavedByFactory && !IsDeleted && State != LineState.Added;

		public string GetPayeeDataRecord() => (TparReportHelper.RecordHeader + "DPAIVS" +
			TparReportHelper.FormatAustralianBusinessNumber(ARL_OrgRegNo) +
			TparReportHelper.FixWidth(string.Empty, 30) + //Ignoring Payee Last Name
			TparReportHelper.FixWidth(string.Empty, 15) + //Ignoring Payee First Name
			TparReportHelper.FixWidth(string.Empty, 15) + //Ignoring Payee Middle Name
			TparReportHelper.FixWidth(ARL_OrgName, 200) +
			TparReportHelper.FixWidth(string.Empty, 200) + //Ignoring Payee Trading Name
			TparReportHelper.FixWidth(ARL_Address1, 38) +
			TparReportHelper.FixWidth(ARL_Address2, 38) +
			TparReportHelper.FixWidth(ARL_City, 27) +
			TparReportHelper.FixWidth(ARL_State, 3) +
			TparReportHelper.FixWidth(ARL_PostCode, 4, padWithZeros: true) +
			TparReportHelper.FixWidth(PayeeCountryName, 20) +
			TparReportHelper.FixWidth(PayeeContactPhone, 15) +
			TparReportHelper.FixWidth(string.Empty, 6, padWithZeros: true) + //Ignoring Bank Details BSB
			TparReportHelper.FixWidth(string.Empty, 9, padWithZeros: true) + //Ignoring Bank Details Account
			TparReportHelper.FormatDecimal(ARL_OverriddenTotalAmountIncludingTax, 11) +
			TparReportHelper.FormatDecimal(ARL_OverriddenPaymentsBasisWithholdingTaxAmount, 11) +
			TparReportHelper.FormatDecimal(ARL_OverriddenGSTAmount, 11) +
			'P' + TparReportHelper.FixWidth(string.Empty, 8, padWithZeros: true) + //Ignoring Date of Grant
			TparReportHelper.FixWidth(string.Empty, 200) + //Ignoring Name of Grant
			TparReportHelper.FixWidth(PayeeEmail, 76) +
			StatementBySupplier +
			AmendmentIndicator).PadRight(TparReportHelper.RecordLength);

		public void SetLineDetails()
		{
			if (TaxReturn != null && !TaxReturn.IsSubmitted)
			{
				ARL_RN_NKCountryCode = Organisation.CountryCode;
				ARL_OrgRegNo = TparReportHelper.GetAustralianBusinessNumber(Organisation);
				ARL_OrgName = Organisation.OH_FullName;
				ARL_Address1 = Organisation.MainAddress.OA_Address1;
				ARL_Address2 = Organisation.MainAddress.OA_Address2;
				ARL_City = TparReportHelper.GetCity(Organisation.MainAddress.OA_City, Organisation.MainAddress.OA_PostCode, Organisation.MainAddress.OA_State, ARL_RN_NKCountryCode);
				ARL_State = TparReportHelper.GetState(Organisation.MainAddress.OA_State, ARL_RN_NKCountryCode);
				ARL_PostCode = TparReportHelper.GetPostCode(Organisation.MainAddress.OA_PostCode, ARL_RN_NKCountryCode);
			}
		}

		#region Report Details

		string StatementBySupplier => TaxReturn != null && TparReportHelper.GetStatementBySupplierProvidedByPayee(Organisation, TaxReturn.ComplianceReport) ? "Y" : "N";

		string AmendmentIndicator => "O";

		string PayeeCountryName => TparReportHelper.GetCountryName(Country);

		string PayeeContactPhone => Organisation.MainAddress.OA_Phone;

		string PayeeEmail => Organisation.MainAddress.OA_Email;

		#endregion

		public LineState State
		{
			get
			{
				var result = LineState.None;

				if (TaxReturn != null && TaxReturn.IsSubmitted)
				{
					result = LineState.Submitted;
				}
				else if (TaxReturn != null && TaxReturn.IsGenerated)
				{
					result = LineState.Generated;
				}
				else if (IsInDatabase)
				{
					result = HasChanges ? LineState.Modified : LineState.Saved;
				}
				else
				{
					result = IsOverridden ? LineState.Modified : LineState.Added;
				}

				return result;
			}
		}

		public bool IsTotalAmountOverridden => ARL_TotalAmountIncludingTax != ARL_OverriddenTotalAmountIncludingTax;
		public bool IsGSTAmountOverridden => ARL_GSTAmount != ARL_OverriddenGSTAmount;
		public bool IsPaymentsAmountOverridden => ARL_PaymentsBasisWithholdingTaxAmount != ARL_OverriddenPaymentsBasisWithholdingTaxAmount;

		bool IsOverridden => IsTotalAmountOverridden || IsGSTAmountOverridden || IsPaymentsAmountOverridden || !ARL_Comment.IsEmpty;

		protected override AccTaxReturnLineValidation GetNewValidation() => new TparReportCreditorLineValidation(this);

		#region ReadOnly properties

		protected bool ARL_OverriddenTotalAmountIncludingTax_ReadOnly => TaxReturn?.IsSubmitted ?? true;
		protected bool ARL_OverriddenGSTAmount_ReadOnly => TaxReturn?.IsSubmitted ?? true;
		protected bool ARL_OverriddenPaymentsBasisWithholdingTaxAmount_ReadOnly => TaxReturn?.IsSubmitted ?? true;
		protected bool ARL_Comment_ReadOnly => TaxReturn?.IsSubmitted ?? true;
		protected bool ARL_Address1_ReadOnly => true;
		protected bool ARL_Address2_ReadOnly => true;
		protected bool ARL_ATR_AccTaxReturn_ReadOnly => true;
		protected bool ARL_City_ReadOnly => true;
		protected bool ARL_GSTAmount_ReadOnly => true;
		protected bool ARL_OH_Organisation_ReadOnly => true;
		protected bool ARL_OrgMergeCounter_ReadOnly => true;
		protected bool ARL_OrgName_ReadOnly => true;
		protected bool ARL_OrgRegNo_ReadOnly => true;
		protected bool ARL_PaymentsBasisWithholdingTaxAmount_ReadOnly => true;
		protected bool ARL_PostCode_ReadOnly => true;
		protected bool ARL_RN_NKCountryCode_ReadOnly => true;
		protected bool ARL_State_ReadOnly => true;
		protected bool ARL_TotalAmountIncludingTax_ReadOnly => true;
		protected bool ARL_SystemLastEditTimeUtc_ReadOnly => true;
		protected bool ARL_SystemLastEditUser_ReadOnly => true;

		#endregion

		#region Overridden properties

		[ResourceStringData("TparReportCreditorLine|ARL_OH_Organisation", Caption = "Creditor", FullDescription = "Creditor Organization Code")]
		public override ZGuid ARL_OH_Organisation { get => base.ARL_OH_Organisation; set => base.ARL_OH_Organisation = value; }

		[ResourceStringData("TparReportCreditorLine|ARL_OrgName", Caption = "Creditor Name")]
		public override ZString ARL_OrgName { get => base.ARL_OrgName; set => base.ARL_OrgName = value; }

		[ResourceStringData("TparReportCreditorLine|ARL_OrgRegNo", Caption = "ABN")]
		public override ZString ARL_OrgRegNo { get => base.ARL_OrgRegNo; set => base.ARL_OrgRegNo = value; }

		[DecimalPlaces(0)]
		public override ZDecimal ARL_TotalAmountIncludingTax { get => base.ARL_TotalAmountIncludingTax; set => base.ARL_TotalAmountIncludingTax = value; }

		[DecimalPlaces(0)]
		public override ZDecimal ARL_OverriddenTotalAmountIncludingTax { get => base.ARL_OverriddenTotalAmountIncludingTax; set => base.ARL_OverriddenTotalAmountIncludingTax = value; }

		[DecimalPlaces(0)]
		public override ZDecimal ARL_GSTAmount { get => base.ARL_GSTAmount; set => base.ARL_GSTAmount = value; }

		[DecimalPlaces(0)]
		public override ZDecimal ARL_OverriddenGSTAmount { get => base.ARL_OverriddenGSTAmount; set => base.ARL_OverriddenGSTAmount = value; }

		[DecimalPlaces(0)]
		public override ZDecimal ARL_PaymentsBasisWithholdingTaxAmount { get => base.ARL_PaymentsBasisWithholdingTaxAmount; set => base.ARL_PaymentsBasisWithholdingTaxAmount = value; }

		[DecimalPlaces(0)]
		public override ZDecimal ARL_OverriddenPaymentsBasisWithholdingTaxAmount { get => base.ARL_OverriddenPaymentsBasisWithholdingTaxAmount; set => base.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = value; }

		#endregion

		#region New properties

		[ResourceStringData("8dc68a64-320c-4b9e-8b29-06b780e922ad", Caption = "Phone")]
		public ZString PhoneNumber => Organisation?.MainAddress?.OA_Phone ?? ZString.Empty;

		[ResourceStringData("da86dae7-520f-4e31-af2b-e212d9e8ff35", Caption = "Email")]
		public ZString Email => Organisation?.MainAddress?.OA_Email ?? ZString.Empty;

		#endregion

		public enum LineState
		{
			None = 0,
			Added = 1,
			Saved = 2,
			Modified = 3,
			Generated = 4,
			Submitted = 5
		}
	}
}
