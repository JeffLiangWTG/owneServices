using System.Linq;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDSegmentGroup30EntryLineBuilder
	{
		public EXDSegmentGroup30EntryLineBuilder(CusEntryLine entryLine, SegmentGroup30 group30, int lineNumber)
		{
			this.entryLine = entryLine;
			this.group30 = group30;
			this.lineNumber = lineNumber;
		}

		protected int lineNumber;
		protected CusEntryLine entryLine;
		protected SegmentGroup30 group30;

		protected void PopulateCSTSegment(int lineNumber)
		{
			string lineActionCode = "I";
			string lineNum = (lineNumber + 1).ToString();
			if (lineNumber == -1)
			{
				lineActionCode = null;
				lineNum = "1";
			}
			MessageUtilities.PopulateCST(CSTSection[0], lineNum, lineActionCode);
		}

		protected void PopulateAHECC()
		{
			if (!entryLine.TariffNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(NextRFFSegment, ReferenceFunctionCodeQualifierList.HarmonisedSystemNumber, entryLine.TariffNumber, null);
			}
		}

		protected void PopulatePermits()
		{
			foreach (var permitNumber in entryLine.Permits)
			{
				if (!permitNumber.IsEmpty)
				{
					MessageUtilities.PopulateRFF(NextRFFSegment, ReferenceFunctionCodeQualifierList.ExportPermitNumber, permitNumber, null);
				}
			}
		}

		protected void PopulateTemporaryImportNumber()
		{
			if (!entryLine.TemporaryImportNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(NextRFFSegment, ReferenceFunctionCodeQualifierList.NumberOfTemporaryImportationDocument, entryLine.TemporaryImportNumber, null);
			}
		}

		protected void PopulateGoodsDescription()
		{
			if (!entryLine.CL_Description.IsEmpty)
			{
				MessageUtilities.PopulateFTX(FTXSection[0], TextSubjectCodeQualifierList.GoodsDescription, entryLine.CL_Description.Left(128));
			}
		}

		protected void PopulateMeasurements()
		{
			int mEANumber = -1;
			var totalWeight = GetStringWithoutInsignificantDecimalPoints(ZArchitecture.Core.Utilities.Round(entryLine.Weight, 5).ToString());
			var netQuantity = GetStringWithoutInsignificantDecimalPoints(ZArchitecture.Core.Utilities.Round(entryLine.Quantity, 5).ToString());
			if (!entryLine.UnitOfWeight.IsEmpty)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.Weights, null, entryLine.UnitOfWeight, totalWeight);
			}

			if (!entryLine.UnitOfQuantity.IsEmpty)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.UnitOfMeasureUsedForInvoicedQuantities, null, entryLine.UnitOfQuantity, netQuantity);
			}

			if (entryLine.AssayAG_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "AG", "GPT", ZArchitecture.Core.Utilities.Round(entryLine.AssayAG_Hidden, 5).ToString());
			}
			if (entryLine.AssayAU_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "AU", "GPT", ZArchitecture.Core.Utilities.Round(entryLine.AssayAU_Hidden, 5).ToString());
			}
			if (entryLine.AssayCU_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "CU", "PER", ZArchitecture.Core.Utilities.Round(entryLine.AssayCU_Hidden, 5).ToString());
			}
			if (entryLine.AssayNI_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "NI", "PER", ZArchitecture.Core.Utilities.Round(entryLine.AssayNI_Hidden, 5).ToString());
			}
			if (entryLine.AssayPB_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "PB", "PER", ZArchitecture.Core.Utilities.Round(entryLine.AssayPB_Hidden, 5).ToString());
			}
			if (entryLine.AssayPT_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "PT", "PER", ZArchitecture.Core.Utilities.Round(entryLine.AssayPT_Hidden, 5).ToString());
			}
			if (entryLine.AssaySN_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "SN", "PER", ZArchitecture.Core.Utilities.Round(entryLine.AssaySN_Hidden, 5).ToString());
			}
			if (entryLine.AssayWO_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "WO", "PER", ZArchitecture.Core.Utilities.Round(entryLine.AssayWO_Hidden, 5).ToString());
			}
			if (entryLine.AssayZN_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "ZN", "PER", ZArchitecture.Core.Utilities.Round(entryLine.AssayZN_Hidden, 5).ToString());
			}
		}

		protected void PopulateOriginOfGoods()
		{
			var countryOfOrigin = entryLine.CountryOfOrigin?.Code ?? ZString.Empty;
			if (!countryOfOrigin.IsEmpty)
			{
				var countryOfOriginIfNotAustralia = countryOfOrigin == Enterprise.Core.Constants.CountryCodes.Australia ? ZString.Empty : countryOfOrigin;
				if (!countryOfOriginIfNotAustralia.IsEmpty || (!entryLine.GoodsOriginCode.IsEmpty && entryLine.GoodsOriginCode != JobComInvoiceLine.ForeignCountryCode))
				{
					MessageUtilities.PopulateLOC(LOCSection[0], LocationFunctionCodeQualifierList.CountryOfOrigin, countryOfOriginIfNotAustralia, CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization, entryLine.GoodsOriginCode, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}
			}
		}

		protected void PopulateFOBValue()
		{
			MessageUtilities.PopulateMOA(MOASection[0], MonetaryAmountTypeCodeQualifierList.FobValue, RoundedFOBValue.ToString(), null);
		}

		public long RoundedFOBValue
		{
			get
			{
				ZDecimal result = 0;
				foreach (var line in entryLine.InvoiceLines.Cast<JobComInvoiceLine>())
				{
					result += line.JI_Calc_FOB;
				}
				if (result > 0 && result < 1)
				{
					result = 1;
				}

				return (long)ZArchitecture.Core.Utilities.Round(result, 0);
			}
		}

		public void PopulateSegment()
		{
			PopulateCSTSegment(lineNumber);
			PopulateAHECC();
			PopulatePermits();
			PopulateTemporaryImportNumber();
			PopulateGoodsDescription();
			PopulateMeasurements();
			PopulateOriginOfGoods();
			PopulateFOBValue();
		}

		protected ZString GetStringWithoutInsignificantDecimalPoints(ZString value)
		{
			return value.IndexOf('.') == -1 ? value : value.TrimEnd('0').TrimEnd('.');
		}

		protected CSTSegmentMessageSection CSTSection => group30.CST;

		protected FTXSegmentMessageSection FTXSection => group30.FTX;

		protected LOCSegmentMessageSection LOCSection => group30.LOC;

		protected MEASegmentMessageSection MEASection => group30.MEA;

		protected MOASegmentMessageSection MOASection => group30.Group33[0].MOA;

		protected RFFSegment NextRFFSegment => group30.Group35.InstantiateAChildAndAddItToChildrenCollection().RFF[0];
	}
}
