using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceLineTax
		: Customs.Business.JobComInvoiceLineTax
		, Integration.Customs.EU.IJobComInvoiceLineTax
		, IEuTax
	{
		public JobComInvoiceLineTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.JobComInvoiceLineTax.Schema
		{
			public const string HasEmptyTaxAmount = "HasEmptyTaxAmount";
			public const string JLT_Calc_RateDuty = "JLT_Calc_RateDuty";
			public const string JLT_Calc_RateSuspension = "JLT_Calc_RateSuspension";
			public const string JLT_Calc_CalculatedPercentage = "JLT_Calc_CalculatedPercentage";
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			yield return Schema.JLT_Amount;
			yield return Schema.JLT_BaseQuantity;
			yield return Schema.JLT_BaseValue;
		}

		protected override Customs.Business.JobComInvoiceLineTaxLookups GetNewLookups()
		{
			return new JobComInvoiceLineTaxLookups(this);
		}

		public new JobComInvoiceLineTaxValidation Validation => (JobComInvoiceLineTaxValidation)base.Validation;
		protected override Customs.Business.JobComInvoiceLineTaxValidation GetNewValidation()
		{
			return new JobComInvoiceLineTaxValidation(this);
		}

		public ZBool BaseQuantityUQListVisible => true;

		public new JobComInvoiceLineTaxLookups Lookups => (JobComInvoiceLineTaxLookups)base.Lookups;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.RateDutyList))]
		[MaxLength(3)]
		public virtual ZString JLT_Calc_RateDuty
		{
			get { return JLT_MethodOfCalculation.PadRight(4, ' ').Left(3).Trim(); }
			set
			{
				JLT_MethodOfCalculation = value.PadRight(3) + JLT_Calc_RateSuspension.PadRight(1);
				if (IsVatGst && RateDemandsNoPayment)
				{
					JLT_MethodOfPayment = ZString.Empty;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateJLT_Calc_RateDuty();
				}
				JLT_Calc_RateDutyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JLT_Calc_RateDutyInfo
		{
			get { return GetZPropertyInfo(Schema.JLT_Calc_RateDuty); }
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.RateSuspensionList))]
		public virtual ZString JLT_Calc_RateSuspension
		{
			get { return JLT_MethodOfCalculation.PadRight(4, ' ').Right(1).Trim(); }
			set
			{
				JLT_MethodOfCalculation = JLT_Calc_RateDuty.PadRight(3) + value.PadRight(1);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJLT_Calc_RateSuspension();
				}
				JLT_Calc_RateSuspensionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JLT_Calc_RateSuspensionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JLT_Calc_RateSuspension); }
		}

		public new JobComInvoiceLine InvoiceLine => Factory.Load<JobComInvoiceLine>(JLT_JI);

		public ZString CountryCode
		{
			get
			{
				var line = InvoiceLine;
				return line?.Declaration?.CountryCode ?? ZString.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.MOPList))]
		public override ZString JLT_MethodOfPayment
		{
			get { return base.JLT_MethodOfPayment; }
			set { base.JLT_MethodOfPayment = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.TypeList))]
		public override ZString JLT_Type
		{
			get { return base.JLT_Type; }
			set
			{
				base.JLT_Type = value;
				if (JLT_MethodOfPayment.IsEmpty && Lookups.MOPList.Count == 1)
				{
					JLT_MethodOfPayment = Lookups.MOPList[0].Code;
				}
				if (IsVatGst && RateDemandsNoPayment)
				{
					JLT_MethodOfPayment = ZString.Empty;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.RateOverrideList))]
		public override ZString JLT_RateOverrideReasonCode { get => base.JLT_RateOverrideReasonCode; set => base.JLT_RateOverrideReasonCode = value; }

		public ZBool HasEmptyTaxAmount
		{
			get { return JLT_Amount.IsEmpty; }
		}

		public ZPropertyInfo HasEmptyTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.HasEmptyTaxAmount); }
		}

		#region CalculatedPercentage

		public ZDecimal JLT_Calc_CalculatedPercentage
		{
			get { return JLT_BaseValue.IsEmpty ? ZDecimal.Zero : new ZDecimal(100 * JLT_Amount / JLT_BaseValue); }
		}

		public virtual ZPropertyInfo JLT_Calc_CalculatedPercentageInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.JLT_Calc_CalculatedPercentage); }
		}

		#endregion

		/// <summary>
		/// i.e. amount in *GBP* or *EUR*, rather than in the invoice currency of USD/HKD/AUD/etc
		/// </summary>
		public ZDecimal CalcAmountInDeclarationCurrency
		{
			get
			{
				return TaxStruct.ConvertToDeclarationCurrencyFromInvoiceCurrencyIfNecessary(JLT_Amount);
			}
		}

		bool IsVatGst
		{
			get { return JLT_Type == UniversalReferenceConstants.RefCusRateCodes.Vat; }
		}

		bool RateDemandsNoPayment
		{
			get { return JLT_Calc_RateDuty == TaxRateVATDutyListImport.Codes.VATTheGoodsAreExemptFromVAT || JLT_Calc_RateDuty == TaxRateVATDutyListImport.Codes.VATTheGoodsAreZeroRated; }
		}

		#region Backwards compatibility with CusAddInfo<Tax_CusAddInfoOnlyForPIVOT>

		// This bit is just so that we don't have an additional 400 edits in the shelf.  It offers {Tax biz o}.Data.G4_Whatever so that JobComINvoiceLineTax.Data.G4_Whatever looks like it did for CusAddInfo<Tax>.Data.G4_Whatever.
		public JobComInvoiceLineTax Data
		{
			get { return this; }
		}

		ZBool IEuTax.IsCopying => IsCopying;
		ICanBeImportOrExport IEuTax.ImportExportParent => InvoiceLine;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.MOPList))]
		[MaxLength(JobComInvoiceLineTax.Schema.JLT_MethodOfPaymentMaxLength)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("JLT_MethodOfPayment", Caption = "Method of Payment", ShortCaption = "MoP")]
		public ZString G4_MethodOfPayment { get => JLT_MethodOfPayment; set => JLT_MethodOfPayment = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.RateDutyList))]
		[MaxLength(3)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("JLT_Calc_RateDuty", Caption = "Rate", FullDescription = "Rate Code")]
		public ZString G4_RateDuty { get => JLT_Calc_RateDuty; set => JLT_Calc_RateDuty = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.RateOverrideList))]
		[MaxLength(JobComInvoiceLineTax.Schema.JLT_RateOverrideReasonCodeMaxLength)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("JLT_RateOverrideReasonCode", Caption = "Override", FullDescription = "Rate Override Reason Code")]
		public ZString G4_RateOverride { get => JLT_RateOverrideReasonCode; set => JLT_RateOverrideReasonCode = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.RateSuspensionList))]
		[MaxLength(1)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("JLT_Calc_RateSuspension", Caption = "Suspension", FullDescription = "Rate Suspension Code")]
		public ZString G4_RateSuspension { get => JLT_Calc_RateSuspension; set => JLT_Calc_RateSuspension = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.TypeList))]
		[MaxLength(JobComInvoiceLineTax.Schema.JLT_TypeMaxLength)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("JLT_Type", Caption = "Type", FullDescription = "Tax Type Code")]
		public ZString G4_Type { get => JLT_Type; set => JLT_Type = value; }

		[CargoWiseOne.ResourceStrings.ResourceStringData("JLT_Amount", Caption = "Amount", FullDescription = "Amount of Tax")]
		public ZString G4_Amount { get => JLT_Amount.IsEmpty ? "0.00" : JLT_Amount.ToString("#.00", CultureInfo.CurrentCulture); set => JLT_Amount = ZDecimal.ParseSafe(value, 0m); }

		[CargoWiseOne.ResourceStrings.ResourceStringData("JLT_BaseValue", Caption = "Base Amount", FullDescription = "Base Value/Amount on which the Tax Amount is due")]
		public ZDecimal G4_BaseAmount { get => JLT_BaseValue; set => JLT_BaseValue = value; }

		[CargoWiseOne.ResourceStrings.ResourceStringData("JLT_BaseQuantity", Caption = "Base Quantity", FullDescription = "Base Quantity on which the Tax Amount is due")]
		public ZDecimal G4_BaseQuantity { get => JLT_BaseQuantity; set => JLT_BaseQuantity = value; }

		public ZString G4_BaseQuantityUQ { get => JLT_BaseQuantityUQ; set => JLT_BaseQuantityUQ = value; }

		[CargoWiseOne.ResourceStrings.ResourceStringData("JLT_Calc_CalculatedPercentage", Caption = "%", FullDescription = "Percentage Calculated")]
		public ZDecimal G4_CalculatedPercentage { get => JLT_Calc_CalculatedPercentage; }

		public ZPropertyInfo G4_MethodOfPaymentInfo { get => JLT_MethodOfPaymentInfo; }
		public ZPropertyInfo G4_RateDutyInfo { get => JLT_Calc_RateDutyInfo; }
		public ZPropertyInfo G4_RateOverrideInfo { get => JLT_RateOverrideReasonCodeInfo; }
		public ZPropertyInfo G4_RateSuspensionInfo { get => JLT_Calc_RateSuspensionInfo; }
		public ZPropertyInfo G4_TypeInfo { get => JLT_TypeInfo; }
		public ZPropertyInfo G4_AmountInfo { get => JLT_AmountInfo; }
		public ZPropertyInfo G4_BaseAmountInfo { get => JLT_BaseValueInfo; }
		#endregion
	}
}
