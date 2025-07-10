using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.IE.Business.Constants;
using EURateCodes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class DutyCalculatorStrategy : EU.Business.Declaration.DutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(JobDeclaration declaration) : base(declaration)
		{
		}

		new JobDeclaration declaration => (JobDeclaration)base.declaration;

		protected override void SetNationalType(EU.Business.Declaration.CusEntryLine entryLine, EU.Business.Declaration.CusEntryLineFee entryLineFee, RateView rateForCalculation)
		{
			base.SetNationalType(entryLine, entryLineFee, rateForCalculation);

			if (entryLine is CusEntryLine ieCusEntryLine && ieCusEntryLine.IsSecuritiesForEndUse
				&& (entryLineFee.CF_ChargeType == EURateCodes.CustomsDutyOnIndustrialProducts || entryLineFee.CF_ChargeType == EURateCodes.Vat)
			)
			{
				entryLineFee.NationalFeeTypeCode = UniversalReferenceConstants.IERefCusRateCodes.SecuritiesForEndUse;
			}
		}

		protected override void CalculateEntryLineVatFee(EU.Business.Declaration.CusEntryLine entryLine)
		{
			var shouldSkipVatFee = declaration.IsUCC5
				&& entryLine.RandomLine is JobComInvoiceLine invoiceLine
				&& !invoiceLine.JI_ZZF_NKTaxType.IsEmpty
				&& entryLine.Header.SupportingDocuments.Concat(entryLine.SupportingDocuments).Any(doc => SupportingDocumentCodes.CodesWithoutVat.Contains(doc.CSI_Code));

			if (!shouldSkipVatFee && !Add1B2Or1B3VatFeeIfNeeded(entryLine))
			{
				base.CalculateEntryLineVatFee(entryLine);
			}
		}

		bool Add1B2Or1B3VatFeeIfNeeded(EU.Business.Declaration.CusEntryLine entryLine)
		{
			var bIsEntryLineAdded = false;
			if (entryLine.RandomLine?.EntryInstruction is CusEntryInstruction instruction
				&& instruction.IsImport
				&& (instruction.CEI_Style == ImportDeclarationTypeList.Codes.H1 || instruction.CEI_Style == ImportDeclarationTypeList.Codes.H5))
			{
				if (ContainsSupportingDocType(SupportingDocumentCodes._1A05, entryLine))
				{
					AddEntryLineVatFee(entryLine, UniversalReferenceConstants.IERefCusRateCodes.DeferredVAT);
					bIsEntryLineAdded = true;
				}
				else if (ContainsSupportingDocType(SupportingDocumentCodes._1A06, entryLine))
				{
					AddEntryLineVatFee(entryLine, UniversalReferenceConstants.IERefCusRateCodes.SpecialArrangementsVAT);
					bIsEntryLineAdded = true;
				}
			}

			return bIsEntryLineAdded;
		}

		bool ContainsSupportingDocType(ZString supportingDocCode, EU.Business.Declaration.CusEntryLine entryLine)
		{
			var result = entryLine.Header?.SupportingDocuments
				.Concat(entryLine.SupportingDocuments)
				.Concat(entryLine.RandomLine?.EntryInstruction?.SupportingDocuments.Cast<SupportingDocument>())
				.Any(doc => doc.CSI_Code == supportingDocCode);
			if (result == true)
			{
				return true;
			}
			return false;
		}

		void AddEntryLineVatFee(EU.Business.Declaration.CusEntryLine entryLine, ZString ratecode)
		{
			if (!ShouldCalculateSystemFeeForThisCode(entryLine, ratecode) && !entryLine.HasAnyProcedureWithSuspendedVat)
			{
				var vatCalculator = entryLine.GetEntryLineVatCalculator();
				var calculatedVat = vatCalculator.CalculateVatFee();
				AddNewEntryLineFee(entryLine, ratecode, ZString.Empty, calculatedVat);
			}
		}

		protected override bool ShouldCalculateDutiesForEntryLine(EU.Business.Declaration.CusEntryLine entryLine)
		{
			if (declaration.IsUCC5AndIsImport && entryLine.RandomLine is JobComInvoiceLine invoiceLine && invoiceLine.EntryInstruction is CusEntryInstruction instruction)
			{
				return !(instruction.IsH3 || instruction.IsH4) || invoiceLine.IsSecuritiesForEndUse;
			}
			else
			{
				return true;
			}
		}

		protected override bool ShouldCalculateTaxesForEntryLine(EU.Business.Declaration.CusEntryLine entryLine) => ShouldCalculateDutiesForEntryLine(entryLine);
	}
}
