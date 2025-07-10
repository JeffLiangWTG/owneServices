using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentWrappers.Customs.EU;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using ESJobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;
using ESUniversalReferenceConstants = Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public class ESDocSADHLineImport : ESDocSADHLine
	{
		public static ESDocSADHLineImport New(ESCusEntryLine entryLine, BusinessObjectFactory factory)
			=> entryLine == null ? null : new ESDocSADHLineImport(entryLine, factory);

		protected ESDocSADHLineImport(ESCusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
		}

		const string NoExciseExemptionCode = "0";
		const string DecimalFormatSpain = "N2";

		protected override ZString Box33CommodityCodeCore
		{
			get
			{
				var box33a = new ZStringBuilder();
				box33a.AppendIfNotEmpty(EntryLine.Tariff);
				box33a.AppendIfNotEmpty(EntryLine.SupplementaryCode1);
				return box33a.ToString();
			}
		}

		protected override ZString Box33ECSupplementCore => EntryLine.SupplementaryCode2;

		protected override ZString Box33ECSupplement2Core
		{
			get
			{
				var box33c = new ZStringBuilder();
				string exciseExemption = !EntryLine.RandomLine.ZG_ExciseExemption.IsEmpty ? EntryLine.RandomLine.ZG_ExciseExemption.ToString() : NoExciseExemptionCode;
				if (!EntryLine.RandomLine.ZG_ExciseCode.IsEmpty)
				{
					box33c.AppendIfNotEmpty(EntryLine.RandomLine.ZG_ExciseCode);
					box33c.AppendIfNotEmpty(exciseExemption);
				}
				return box33c.ToString();
			}
		}

		protected override int Box44MaxLength => EntryLine.CL_LineNumber == 1 ? EntryLine.ImportBox44MaxLength : EntryLine.ImportBox44BISPageMaxLength;

		protected override ZString GetBox45AdjustmentCore() => EntryLine.Box45CompleteText();

		protected override DocSADHLineTaxCollection GetBox47TaxesCore()
		{
			var lineTaxCollection = new ESDocSADHLineTaxCollectionImport(ESGetTaxBoxSupporterList(), Factory);
			SortBox47TaxesCore(lineTaxCollection);
			return lineTaxCollection;
		}

		protected override void SortBox47TaxesCore(DocSADHLineTaxCollection taxCollection)
		{
			((ESDocSADHLineTaxCollectionImport)taxCollection).Sort(new ESTaxSorterImport());
		}

		protected IEnumerable<IESDocSADHLineTaxBoxSupporter> ESGetTaxBoxSupporterList() => EntryLine.GetESTaxBoxSupporterList();

		#region TaxSorter

		class ESTaxSorterImport : TaxSorter
		{
			const string ChargeType3IG = "3IG";

			readonly IList<string> codes = new string[]
			{
				UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts,
				UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge,
				UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty,
				UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty,
				UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty,
				UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty,
				UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts,
				UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge,
				ChargeType3IG,
				ESUniversalReferenceConstants.RefCusRateCode.AIEM,
				ESUniversalReferenceConstants.RefCusRateCode.RetailerSurcharge,
				UniversalReferenceConstants.RefCusRateCodes.Vat,
				ChargeTypeD00, ChargeTypeB20, ChargeTypeD10,
				UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat
			};

			protected override IList<string> taxCodes => codes;
		}

		#endregion

		#region VatCalculation
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		const string VatBaseText = "B. IVA:";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
		const string VatBaseCanaryText = "B. IGIC:";
		const string AddText = "+";
		const string SubstractText = "-";
		const string EqualsText = "=";
		const string DutiesStartCode = "A";
		const string ExclusionType = "3";

		ZString GetVatBaseText => ((ESJobDeclaration)EntryLine?.Declaration).DestinationStateIsCanaryIsland ? VatBaseCanaryText : VatBaseText;

		ZDecimal GetSupportingDocumentsReferenceNumberSum(ESCusEntryLine entryLine, ZString docType)
		{
			return entryLine.SupportingDocuments.Where(d => d.CSI_Code == docType).Sum(d => Enterprise.Customs.ES.Business.DecimalHelper.DecimalParseToESFormat(d.CSI_ReferenceNumber));
		}

		ZDecimal GetFeeAmoutDutiesByStartType(ESCusEntryLine entryLine, ZString feeStartType)
		{
			return entryLine.Fees.Where(fee => fee.G4_Type.StartsWith(feeStartType)).Sum(fee => fee.CF_ChargeAmount);
		}

		ZDecimal GetFeeBaseByType(ESCusEntryLine entryLine, ZString feeType)
		{
			return entryLine.Fees.Where(fee => fee.G4_Type.Contains(feeType)).Sum(fee => fee.G4_BaseAmount);
		}

		ZDecimal GetFeeAmoutSpecialTaxes(ESCusEntryLine entryLine, ZString exclusionType)
		{
			if (entryLine.Fees.Count >= 3)
			{
				var ch = entryLine.Fees[2].G4_Type.SubstringSafe(0, 1);
				var bl = entryLine.Fees[2].G4_Type.SubstringSafe(0, 1).IsNumbersOnlyOrEmpty;
			}
			return entryLine.Fees.Where(fee => fee.G4_Type.SubstringSafe(0, 1).IsNumbersOnlyOrEmpty && !fee.G4_Type.StartsWith(exclusionType)).
				Sum(fee => fee.CF_ChargeAmount);
		}

		ZDecimal GetFeeAmountDuties(ESCusEntryLine entryLine) => GetFeeAmoutDutiesByStartType(entryLine, DutiesStartCode) + GetFeeAmoutSpecialTaxes(entryLine, ExclusionType);

		protected override ZString Box44VatInfoCalculationCore
		{
			get
			{
				var result = new ZStringBuilder();
				result.Append(GetVatBaseText);
				result.AppendIfNotEmpty(EntryLine.CL_CustomsValue != ZDecimal.Zero ? FormatDecimalDefault(EntryLine.CL_CustomsValue) : ZString.Empty);
				result.AppendIfNotEmpty(GetSupportingDocumentsReferenceNumberSum(EntryLine, SupportingDocumentType.TransformedRPP) != ZDecimal.Zero ? (ZString)SubstractText : ZString.Empty);
				result.AppendIfNotEmpty(GetSupportingDocumentsReferenceNumberSum(EntryLine, SupportingDocumentType.TransformedRPP) != ZDecimal.Zero ?
					FormatDecimalDefault(GetSupportingDocumentsReferenceNumberSum(EntryLine, SupportingDocumentType.TransformedRPP)) : ZString.Empty);
				result.AppendIfNotEmpty(GetSupportingDocumentsReferenceNumberSum(EntryLine, SupportingDocumentType.REARebate) != ZDecimal.Zero ? (ZString)SubstractText : ZString.Empty);
				result.AppendIfNotEmpty(GetSupportingDocumentsReferenceNumberSum(EntryLine, SupportingDocumentType.REARebate) != ZDecimal.Zero ?
					FormatDecimalDefault(GetSupportingDocumentsReferenceNumberSum(EntryLine, SupportingDocumentType.REARebate)) : ZString.Empty);
				result.Append(AddText);
				result.Append(FormatDecimalDefault(GetFeeAmountDuties(EntryLine)));
				result.Append(AddText);
				result.Append(FormatDecimalDefault(EntryLine.InvoiceLines.Sum(line => ((JobComInvoiceLine)line).JI_VAT_Additions)));
				result.Append(EqualsText);
				result.Append(FormatDecimalDefault(GetFeeBaseByType(EntryLine, "B00")));

				return result.ToStringWithDelimiterBetweenAppends(" ");
			}
		}
		#endregion

		protected override ZString FormatDecimalDefault(ZDecimal number) => number.ToStringTrimZeros(DecimalFormatSpain);

		protected override ZString SupplementaryUnitsDecimalFormatSpain => "N3";
	}
}
