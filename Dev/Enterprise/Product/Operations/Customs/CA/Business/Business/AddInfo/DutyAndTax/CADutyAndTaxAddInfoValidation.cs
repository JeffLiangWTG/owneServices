//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCADutyAndTaxAddInfoValidation
//
//    This class should be used for overriding validation in AutoCADutyAndTaxAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CADutyAndTaxAddInfoValidation : AutoCADutyAndTaxAddInfoValidation
	{
		public CADutyAndTaxAddInfoValidation(AutoCADutyAndTaxAddInfo parent)
			: base(parent)
		{
		}

		new DutyAndTax Parent
		{
			get { return (DutyAndTax)base.Parent.Parent; }
		}

		ZBool IsB3ValidationRequired
		{
			get
			{
				if (Parent.Parent != null)
				{
					return Parent.Parent.IsB3ValidationRequired;
				}
				else
				{
					return false;
				}
			}
		}

		#region CheckC1_ExemptCode

		protected override void CheckC1_ExemptCode()
		{
			base.CheckC1_ExemptCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.C1_ExemptCodeInfo, Parent.AddInfoLookups.ExemptCodes);

			if (DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(Parent.C1_TaxType) && IsB3ValidationRequired)
			{
				if (Parent.C1_ExemptCode.IsEmpty && Parent.C1_Amount > 0)
				{
					Parent.C1_ExemptCodeInfo.AddMessageError(Res.GetString("b00bcc60-fda1-4320-9864-8e9fca125250", "You have not entered a SIMA Code."));
				}

				if (Parent.Parent != null)
				{
					var hasOtherExemptCodePerSIMA = Parent.C1_TaxType != DutyAndTaxTypes.Codes.SUR && Parent.Parent.SIMADuties.Any(x => x != Parent && x.C1_ExemptCode != Parent.C1_ExemptCode && x.C1_TaxType != DutyAndTaxTypes.Codes.SUR);
					if (hasOtherExemptCodePerSIMA)
					{
						Parent.C1_ExemptCodeInfo.AddMessageError(Res.GetString("95f88b5b-fa10-48ae-9214-811a5a1bc90e", "All SIMA type rates must have the same SIMA Code."));
					}
				}
			}

			ValidateC1_Rate();
			ValidateC1_Amount();
		}

		#endregion

		#region CheckC1_TaxType

		protected override void CheckC1_TaxType()
		{
			base.CheckC1_TaxType();
			var parent = Parent;
			var invoiceLine = parent.Parent as JobComInvoiceLine;
			if (parent.C1_TaxType != DutyAndTaxTypes.Codes.SIMADuty)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.C1_TaxTypeInfo, parent.AddInfoLookups.Types);
			}
			else if (invoiceLine == null || !invoiceLine.CA_IsSeeded)
			{
				parent.C1_TaxTypeInfo.AddWarning(Res.GetString("F79395AF-1501-4F91-9DD5-F7B9650739BE", "The 'SIM' group has been replaced by it’s individual components, please select the specific additional duties or taxes required."));
			}

			if (invoiceLine != null)
			{
				invoiceLine.DutyAndTaxManager.ValidateDutyOrTaxCount(parent);

				if (invoiceLine.IsLuxuryTaxInvoiceLine && parent.IsGST && !invoiceLine.DutiesAndTaxes.Any(x => x.IsExciseTax))
				{
					parent.C1_TaxTypeInfo.AddMessageError(Res.GetString("2EF35F52-9E37-499A-8FB9-146692AB2E41", "Please create an Excise Tax when GST exists."));
				}
			}

			if (parent.C1_TaxType == DutyAndTaxTypes.Codes.CTA && parent.C1_RateType == RateTypes.Codes.AcceptX)
			{
				parent.C1_TaxTypeInfo.AddMessageError(Res.GetString("DBADBD22-B972-414A-8DF9-2C3B39248807", "No rate on file. Please calculate and enter provincial taxes manually, and add the mark-up dummy HS code line(s) manually."));
			}
		}

		#endregion

		#region CheckC1_Amount

		protected override void CheckC1_Amount()
		{
			base.CheckC1_Amount();
			var parent = Parent;
			var invoiceLine = parent.Parent as JobComInvoiceLine;
			if (!(invoiceLine?.IsLuxuryTaxInvoiceLine ?? false))
			{
				if (!parent.C1_Override && (parent.C1_RateType == RateTypes.Codes.AcceptT || parent.C1_RateType == RateTypes.Codes.AcceptX))
				{
					parent.C1_AmountInfo.AddMessageError(Res.GetString("0db4665b-d836-4dde-94a8-f6b923399a76", "{0} Rate Type {1} specified so you must override and enter the Rate and Amount manually",
																												new DutyAndTaxTypes().GetDescriptionFromCode(parent.C1_TaxType), parent.C1_RateType));
				}

				if (IsB3ValidationRequired && DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(parent.C1_TaxType))
				{
					if ((Regex.IsMatch(parent.C1_ExemptCode, "[12]$")) && parent.C1_Amount == 0 && parent.C1_NormalValuePerUnit.IsEmpty)
					{
						if ((parent.C1_TaxType.Contains(DutyAndTaxTypes.Codes.ADD) || parent.C1_TaxType.Contains(DutyAndTaxTypes.Codes.CVD)) && (invoiceLine.JI_Calc_SIMADutyAmount > 0))
						{
							parent.C1_AmountInfo.AddWarning(Res.GetString("04C8FB64-8DDF-47FB-B30A-9B4A7415E5AC", "No SIMA amount has been entered, please confirm this is correct prior to filing the entry"));
						}
						else
						{
							parent.C1_AmountInfo.AddMessageError(Res.GetString("0f451da5-6399-4fd9-9a9e-f93c32ef7909", "Amount is mandatory for SIMA Exempt Code {0}", parent.C1_ExemptCode));
						}
					}
					else if (parent.IsAmountNotRequiredForSIMA && parent.C1_Amount > 0)
					{
						parent.C1_AmountInfo.AddMessageError(Res.GetString("b05507c2-d2cb-4f34-be3c-8328337f1d42", "Amount must be zero for SIMA Code {0}", parent.C1_ExemptCode));
					}
				}
			}

			ValidateC1_ExemptCode();
			ValidateC1_Rate();
		}

		#endregion

		#region CheckC1_Code

		protected override void CheckC1_Code()
		{
			base.CheckC1_Code();

			var parent = Parent;
			if (parent.C1_TaxType == DutyAndTaxTypes.Codes.SUR)
			{
				if (parent.C1_Code.IsEmpty)
				{
					parent.C1_CodeInfo.AddMessageError(Res.GetString("5AA8B87C-9473-4C75-8B2E-A3B0A8BA8CC9", "Surtax Code required. The Surtax Code to be used can be found on the Customs Notice announcing the Surtax"));
				}
				return;
			}

			var rates = parent.AddInfoLookups.Rates;
			if (parent.C1_TaxType != DutyAndTaxTypes.Codes.ADD && parent.C1_TaxType != DutyAndTaxTypes.Codes.CVD)
			{
				if (parent.IsExciseTax || parent.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty)
				{
					ListValidation.WarnIfInvalidCode(parent.C1_CodeInfo, rates);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(parent.C1_CodeInfo, rates);
				}
			}

			if (parent.Parent != null && !parent.C1_Override && parent.IsTax && parent.C1_Code != "NO")
			{
				var shouldValidate = true;
				if (parent.IsGST)
				{
					shouldValidate = parent.CAGSTRateCode == null;
				}
				else
				{
					var exciseTaxRates = UniversalReferenceHelper.GetExciseTaxRates(parent.Factory, parent.Parent.EffectiveDutyDate, parent.Parent.ClassificationNumber);
					shouldValidate = !exciseTaxRates.Any(x => x.RateCode == parent.C1_Code);
				}

				if (shouldValidate)
				{
					if (parent.C1_Code.IsEmpty)
					{
						parent.C1_CodeInfo.AddMessageError(Res.GetString("BFF37585-FD30-41C1-83AD-A2F3FF1B2177", "Please select the appropriate Tax Code or select 'NO' if the tax does not apply"));
					}
					else if (parent.C1_TaxType != DutyAndTaxTypes.Codes.ExciseTax || CACustomsDataRegistry.Instance.DefaultToThisExciseTaxRateCodeWhenApplicable.Value != parent.C1_Code)
					{
						parent.C1_CodeInfo.AddMessageError(Res.GetString("bc562fc5-c0e4-4a83-944a-3fdd3bd87b2b", "No details were found for current tax so you have to override this line"));
					}
				}
			}

			if (UniversalReferenceConstants.SLFCALCEXSIsValid
				&& Parent.IsExciseTax
				&& UniversalReferenceConstants.ExciseTaxCodesRequireSelfCalculation.Contains<string>(Parent.C1_Code)
				&& Parent.C1_Amount == ZDecimal.Zero)
			{
				Parent.C1_CodeInfo.AddMessageError(Res.GetString("49BD7BC1-8E8B-440E-BAC1-BCC888AB1609", "Self calculated Amount is required for this excise code. Refer to CN24-35 for details."));
			}
		}

		#endregion

		#region CheckC1_Rate

		protected override void CheckC1_Rate()
		{
			base.CheckC1_Rate();
			if (!Parent.C1_Override && Parent.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty && !Parent.IsInDatabase && !Parent.IsInRefFiles && Parent.C1_RateType != RateTypes.Codes.Free)
			{
				Parent.C1_RateInfo.AddWarning(Res.GetString("d3f43073-a19a-4f33-982a-221c2108af7f", "No duty rate found so the default general rate of duty has been used"));
			}
			ValidateC1_ExemptCode();
			ValidateC1_Amount();

			var lineValidation = Parent.Validation as DutyAndTaxValidation;
			if (lineValidation != null)
			{
				lineValidation.ValidateQuantity();
			}
		}

		#endregion

		#region CheckC1_PreviousTranNumber

		protected override void CheckC1_PreviousTranNumber()
		{
			base.CheckC1_PreviousTranNumber();

			var superParent = Parent.Parent;
			if (superParent != null && superParent.IsWarehouseOrSupplementaryEntry)
			{
				if (Parent.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty && Parent.C1_PreviousTranNumber.IsEmpty)
				{
					Parent.C1_PreviousTranNumberInfo.AddMessageError(Res.GetString("dc2cd691-29e4-4dd1-83bb-c4f7fc9a438a", "Previous Tran. Number required."));
				}
			}
		}

		#endregion

		#region CheckC1_PreviousTranLine

		protected override void CheckC1_PreviousTranLine()
		{
			base.CheckC1_PreviousTranLine();

			var superParent = Parent.Parent;
			if (superParent != null && superParent.IsWarehouseOrSupplementaryEntry)
			{
				if (Parent.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty && Parent.C1_PreviousTranLine.IsEmpty)
				{
					Parent.C1_PreviousTranLineInfo.AddMessageError(Res.GetString("c935652d-d9f4-4afb-a695-52f1d7cecc21", "Previous Tran. Line Number required."));
				}
			}
		}

		#endregion

		#region CheckC1_NormalValueCurrency

		protected override void CheckC1_NormalValueCurrency()
		{
			base.CheckC1_NormalValueCurrency();
			ListValidation.MessageErrorIfInvalidCode(Parent.C1_NormalValueCurrencyInfo, Parent.AddInfoLookups.CurrencyList);

			if (!Parent.C1_NormalValuePerUnit.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.C1_NormalValueCurrencyInfo);
			}
		}

		#endregion

		#region CheckC1_UnitOfMeasure

		protected override void CheckC1_UnitOfMeasure()
		{
			base.CheckC1_UnitOfMeasure();
			ListValidation.MessageErrorIfInvalidCode(Parent.C1_UnitOfMeasureInfo, Parent.AddInfoLookups.CustomsUQList);

			if (!Parent.C1_NormalValuePerUnit.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.C1_UnitOfMeasureInfo);
			}

			var lineValidation = Parent.Validation as DutyAndTaxValidation;
			if (lineValidation != null)
			{
				lineValidation.ValidateQuantity();
			}
		}

		#endregion

		#region CheckC1_RateType

		protected override void CheckC1_RateType()
		{
			base.CheckC1_RateType();
			ListValidation.MessageErrorIfInvalidCode(Parent.C1_RateTypeInfo, Parent.AddInfoLookups.RateTypes);
		}

		#endregion

		#region ValidateOnOverrideValueChanged

		public void ValidateOnOverrideValueChanged()
		{
			ValidateC1_Code();
			ValidateC1_Amount();
			ValidateC1_Rate();
		}

		#endregion
	}
}
