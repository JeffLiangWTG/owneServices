using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business
{
	public class JobComInvoiceHeaderValidation : AutoJPJobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected override void CheckJZ_ValuationDateOverride()
		{
			base.CheckJZ_ValuationDateOverride();
			var invoiceHeader = Parent;
			if (invoiceHeader.JobDeclaration is JobDeclaration declaration)
			{
				var declarationValuationDate = declaration.JE_ValuationDate;
				var invoiceHeaderValuationDateOverride = invoiceHeader.JZ_ValuationDateOverride;
				if (!declarationValuationDate.IsEmpty && !invoiceHeaderValuationDateOverride.IsEmpty && !invoiceHeaderValuationDateOverride.Date.Equals(declarationValuationDate))
				{
					invoiceHeader.JZ_ValuationDateOverrideInfo.AddMessageError(Res.GetString("3C028FE3-78A6-42A1-ABF2-A29D51B86BB9", "Invoice Header Valuation Date {0} must be the same as the Declaration Valuation Date {1}.", invoiceHeaderValuationDateOverride.Date.ToISO8601ShortDateString(), declarationValuationDate.ToISO8601ShortDateString()));
				}
			}
		}

		protected override void CheckJZ_ValuationCode()
		{
			base.CheckJZ_ValuationCode();
			var invoiceHeader = Parent;
			var valuationCode = invoiceHeader.JZ_ValuationCode;

			if (invoiceHeader.JobDeclaration is JobDeclaration declaration && !string.IsNullOrEmpty(valuationCode))
			{
				var targetInfo = invoiceHeader.JZ_ValuationCodeInfo;

				if (declaration.IsImport)
				{
					var declarationTypes = invoiceHeader.DeclarationTypes;
					if (declarationTypes.Contains(JPImportDeclarationTypeList.Codes.Y) ||
						declarationTypes.Contains(JPImportDeclarationTypeList.Codes.H) ||
						declarationTypes.Contains(JPImportDeclarationTypeList.Codes.N))
					{
						targetInfo.AddMessageError(Res.GetString("B00231F5-200D-4523-B4D0-565F0B585CAC", "Valuation Type cannot be entered when Shipment Type is IMP and Declaration Type contains Y, H, or N."));
					}
				}

				if (valuationCode != ValuationTypeCodeList.Codes.Five && valuationCode != ValuationTypeCodeList.Codes.Z && invoiceHeader.ComprehensiveValuations.Count > 0)
				{
					targetInfo.AddMessageError(Res.GetString("1985890C-1452-48E9-A564-C57AA28B0586", "The entered Valuation Type cannot be used when there are at least one Comprehensive Valuation Number."));
				}
			}
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();
			var invoiceHeader = Parent;
			var targetInfo = invoiceHeader.JZ_InvoiceAmountInfo;
			CompareValidation.CheckNumberNotNegative(Parent.JZ_InvoiceAmountInfo);
			if (!invoiceHeader.JZ_InvoiceAmount.IsEmpty && invoiceHeader.JZ_RX_NKInvoice_Currency == Core.Constants.CurrencyCodes.Japan && !invoiceHeader.JZ_InvoiceAmount.IsInteger)
			{
				targetInfo.AddMessageError(Res.GetString("36A6C17D-45A4-47BD-8E1F-66293D923F10", "Invoice Total must be a whole number when Invoice Currency is JPY."));
			}
		}

		protected override void CheckJZ_ComprehensiveInsuranceNumber()
		{
			base.CheckJZ_ComprehensiveInsuranceNumber();
			var invoiceHeader = Parent;
			if (invoiceHeader.JZ_InsuranceType == InsuranceTypes.Codes.B && invoiceHeader.JZ_ComprehensiveInsuranceNumber.IsEmpty)
			{
				invoiceHeader.JZ_ComprehensiveInsuranceNumberInfo.AddMessageError(Res.GetString("09C7909D-1A4B-424F-8D06-8DD3048B2535", "The Blanket Insurance Number is required if the value of the Insurance Type is 'B'."));
			}
		}

		protected override void CheckJZ_ElectronicInvoiceReceiptNumber()
		{
			base.CheckJZ_ElectronicInvoiceReceiptNumber();
			var invoiceHeader = Parent;
			var targetInfo = invoiceHeader.JZ_ElectronicInvoiceReceiptNumberInfo;
			if (Regex.Match(Parent.JZ_ElectronicInvoiceReceiptNumber, "[^A-Z0-9]").Captures.Count > 0)
			{
				targetInfo.AddMessageError(Res.GetString("9D6CA87F-5DE3-424A-B9EB-596AEC482573", "Only Numbers and Upper Case Letters will be accepted."));
			}

			if (invoiceHeader.JZ_InvoiceType == RepresentativeInvoiceTypes.Codes.C || invoiceHeader.JZ_InvoiceType == RepresentativeInvoiceTypes.Codes.D)
			{
				if (invoiceHeader.JZ_ElectronicInvoiceReceiptNumber.IsEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("7EF56D60-FE99-4CFD-BFC9-B17B3A2F6371", "Electronic Invoice Receipt Number cannot be empty when Invoice Type is C or D."));
				}
			}
			else if (!invoiceHeader.JZ_ElectronicInvoiceReceiptNumber.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("06640E27-B472-41D9-9AF8-661BAA289233", "Electronic Invoice Receipt Number must be empty when Invoice Type is not C nor D."));
			}
		}

		protected override void CheckJZ_InvoiceType()
		{
			base.CheckJZ_InvoiceType();

			var invoiceHeader = Parent;
			var targetInfo = invoiceHeader.JZ_InvoiceTypeInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			if (invoiceHeader.JZ_InvoiceType.IsEmpty && invoiceHeader.JobDeclaration is JobDeclaration declaration && declaration.IsImport)
			{
				var declarationTypes = invoiceHeader.DeclarationTypes;
				if ((declaration.IsAir && declarationTypes.Any(declarationType => RequiresDeclarationTypesOfInvoiceTypeForImportAir.Contains(declarationType))) ||
				(declaration.IsSea && declarationTypes.Any(declarationType => RequiresDeclarationTypesOfInvoiceTypeForImportSea.Contains(declarationType))))
				{
					targetInfo.AddMessageError(Res.GetString("73FC530A-3EB6-4939-AC64-039465C93B7F", "Invoice Type is required."));
				}
			}
		}

		protected override void CheckJZ_InvoiceAmountType()
		{
			base.CheckJZ_InvoiceAmountType();
			var invoiceHeader = Parent;
			ListValidation.MessageErrorIfInvalidCode(invoiceHeader.JZ_InvoiceAmountTypeInfo);
		}

		protected override void CheckJZ_InsuranceType()
		{
			base.CheckJZ_InsuranceType();
			var invoiceHeader = Parent;
			var targetInfo = invoiceHeader.JZ_InsuranceTypeInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			if (invoiceHeader.IsImport && !invoiceHeader.JZ_InsuranceType.IsEmpty && (invoiceHeader.JZ_IncoTerm == IncoTermList.Codes.CostAndInsurance || invoiceHeader.JZ_IncoTerm == Core.Constants.IncoTerms.CostInsuranceAndFreight))
			{
				targetInfo.AddMessageError(Res.GetString("9AA7AFCC-3548-4824-BA78-387CF0CE03D3", "Insurance Type must be empty when Incoterm is C&I or CIF."));
			}
		}

		protected override void CheckJZ_FreightType()
		{
			base.CheckJZ_FreightType();
			var invoiceHeader = Parent;
			var targetInfo = invoiceHeader.JZ_FreightTypeInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			if (invoiceHeader.JobDeclaration is JobDeclaration declaration && declaration.IsImport)
			{
				var hasOFTCharges = declaration.TopGroupInvoice.Charges.Any(x => x.J7_ChargeType == Core.Constants.Customs.CustomsCharges.Codes.OverseasFreight)
					|| invoiceHeader.Charges.Cast<InvoiceCharge>().Any(x => x.J7_ChargeType == Core.Constants.Customs.CustomsCharges.Codes.OverseasFreight);

				var freightType = invoiceHeader.JZ_FreightType;
				if (!hasOFTCharges)
				{
					switch (freightType)
					{
						case FreightRatesTypes.Codes.A:
						case FreightRatesTypes.Codes.B:
						case FreightRatesTypes.Codes.C:
						case FreightRatesTypes.Codes.E:
						case FreightRatesTypes.Codes.F:
						case FreightRatesTypes.Codes.G:
						case FreightRatesTypes.Codes.H:
						case FreightRatesTypes.Codes.J:
						case FreightRatesTypes.Codes.K:
						case FreightRatesTypes.Codes.L:
						case FreightRatesTypes.Codes.M:
						case FreightRatesTypes.Codes.N:
							targetInfo.AddMessageError(Res.GetString("28EA5967-B99A-4E58-8F65-C79C2D549AE5", "Freight Type {0} can only be used when the invoice has at least one Invoice Charge that is OFT - International Freight.",
								freightType));
							break;
					}
				}

				if (freightType == FreightRatesTypes.Codes.C && invoiceHeader.DeclarationTypes.Contains(JPImportDeclarationTypeList.Codes.Y))
				{
					targetInfo.AddMessageError(Res.GetString("6629E561-D38D-435B-A196-F0058D669A04", "Freight Type cannot be C when Declaration Type is Y."));
				}
			}
		}

		protected override void CheckJZ_Weight()
		{
			base.CheckJZ_Weight();
			CompareValidation.CheckNumberNotNegative(Parent.JZ_WeightInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JZ_WeightInfo, Parent.JZ_WeightUQInfo);
		}

		protected override void CheckJZ_WeightUQ()
		{
			base.CheckJZ_Weight();
			ListValidation.ErrorIfInvalidCode(Parent.JZ_WeightUQInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JZ_WeightUQInfo, Parent.JZ_WeightInfo);
		}

		protected override void CheckJZ_NetWeight()
		{
			base.CheckJZ_NetWeight();
			CompareValidation.CheckNumberNotNegative(Parent.JZ_NetWeightInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JZ_NetWeightInfo, Parent.JZ_NetWeightUQInfo);
		}

		protected override void CheckJZ_NetWeightUQ()
		{
			base.CheckJZ_NetWeightUQ();
			ListValidation.ErrorIfInvalidCode(Parent.JZ_NetWeightUQInfo);
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.JZ_NetWeightUQInfo, Parent.JZ_NetWeightInfo);
		}

		protected override void CheckJZ_AdvanceRulingOnValuation1()
		{
			base.CheckJZ_AdvanceRulingOnValuation1();

			ValidateAdvanceRulingOnValuation(Parent.JZ_AdvanceRulingOnValuation1, Parent.JZ_AdvanceRulingOnValuation1Info);
		}

		protected override void CheckJZ_AdvanceRulingOnValuation2()
		{
			base.CheckJZ_AdvanceRulingOnValuation2();

			ValidateAdvanceRulingOnValuation(Parent.JZ_AdvanceRulingOnValuation2, Parent.JZ_AdvanceRulingOnValuation2Info);
		}

		void ValidateAdvanceRulingOnValuation(ZString value, ZPropertyInfo info)
		{
			if (!string.IsNullOrEmpty(value) && value.Length != 7)
			{
				info.AddMessageError($"{info.HumanReadableName} must be exactly 7 characters long.");
			}
		}

		ZString[] RequiresDeclarationTypesOfInvoiceTypeForImportAir => new ZString[] {
			JPImportDeclarationTypeList.Codes.C,
			JPImportDeclarationTypeList.Codes.F,
			JPImportDeclarationTypeList.Codes.J,
			JPImportDeclarationTypeList.Codes.P,
			JPImportDeclarationTypeList.Codes.S,
			JPImportDeclarationTypeList.Codes.M,
			JPImportDeclarationTypeList.Codes.A,
			JPImportDeclarationTypeList.Codes.G,
			JPImportDeclarationTypeList.Codes.K,
			JPImportDeclarationTypeList.Codes.D,
			JPImportDeclarationTypeList.Codes.U,
			JPImportDeclarationTypeList.Codes.L,
			JPImportDeclarationTypeList.Codes.B,
			JPImportDeclarationTypeList.Codes.E,
			JPImportDeclarationTypeList.Codes.R
		};

		ZString[] RequiresDeclarationTypesOfInvoiceTypeForImportSea => new ZString[] {
			JPImportDeclarationTypeList.Codes.C,
			JPImportDeclarationTypeList.Codes.F,
			JPImportDeclarationTypeList.Codes.Y,
			JPImportDeclarationTypeList.Codes.J,
			JPImportDeclarationTypeList.Codes.P,
			JPImportDeclarationTypeList.Codes.S,
			JPImportDeclarationTypeList.Codes.M,
			JPImportDeclarationTypeList.Codes.A,
			JPImportDeclarationTypeList.Codes.G,
			JPImportDeclarationTypeList.Codes.K,
			JPImportDeclarationTypeList.Codes.D,
			JPImportDeclarationTypeList.Codes.U,
			JPImportDeclarationTypeList.Codes.L,
			JPImportDeclarationTypeList.Codes.B,
			JPImportDeclarationTypeList.Codes.E,
			JPImportDeclarationTypeList.Codes.R,
		};
	}
}
