using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRInvoiceLineDetailsBuilder
	{
		public CMRInvoiceLineDetailsBuilder(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		protected void PopulateCSTSegment(int lineNumber)
		{
			string lineActionCode = "I";//null; //I=Insert, A = Replace, D = Delete Line //TODO:verify this is accepted
			string lineNum = (lineNumber + 1).ToString();
			if (lineNumber == -1)
			{
				lineActionCode = null;
				lineNum = "1";//trigger value
			}
			MessageUtilities.PopulateCST(CSTSection[0], lineNum, lineActionCode);
		}

		protected void PopulateAHECC()
		{
			if (!invoiceLine.JI_Tariff.IsEmpty)
			{
				MessageUtilities.PopulateRFF(GetNextRFFSegment(), ReferenceFunctionCodeQualifierList.HarmonisedSystemNumber, invoiceLine.JI_Tariff.Replace(".", ""), null);
			}
		}

		protected void PopulatePermits()
		{
			foreach (ZString permitNumber in invoiceLine.PermitsIncludingHeader)
			{
				if (!permitNumber.IsEmpty)
				{
					MessageUtilities.PopulateRFF(GetNextRFFSegment(), ReferenceFunctionCodeQualifierList.ExportPermitNumber, permitNumber, null);
				}
			}
		}

		protected void PopulateTemporaryImportNumber()
		{
			if (!invoiceLine.JI_TempImportNum.IsEmpty)
			{
				MessageUtilities.PopulateRFF(GetNextRFFSegment(), ReferenceFunctionCodeQualifierList.NumberOfTemporaryImportationDocument, invoiceLine.JI_TempImportNum, null);
			}
		}

		protected void PopulateOriginOfGoods()
		{
			if (!invoiceLine.JI_CountryOfOrigin.IsEmpty)
			{
				ZString countryOfOriginIfNotAustralia = invoiceLine.JI_CountryOfOrigin == Enterprise.Core.Constants.CountryCodes.Australia ? ZString.Empty : invoiceLine.JI_CountryOfOrigin;
				if (!countryOfOriginIfNotAustralia.IsEmpty || (!invoiceLine.JI_GoodsOriginCode.IsEmpty && invoiceLine.JI_GoodsOriginCode != JobComInvoiceLine.ForeignCountryCode))
				{
					MessageUtilities.PopulateLOC(LOCSection[0], LocationFunctionCodeQualifierList.CountryOfOrigin, countryOfOriginIfNotAustralia, CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization, invoiceLine.JI_GoodsOriginCode, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}
			}
		}

		protected void PopulateGoodsDescription()
		{
			if (!invoiceLine.JI_Description.IsEmpty)
			{
				MessageUtilities.PopulateFTX(FTXSection[0], TextSubjectCodeQualifierList.GoodsDescription, invoiceLine.JI_Description.Left(128));
			}
		}

		protected void PopulateMeasurements()
		{
			int mEANumber = -1;
			string totalWeight = GetStringWithoutInsignificantDecimalPoints(ZArchitecture.Core.Utilities.Round(invoiceLine.JI_Weight, 5).ToString());
			string netQuantity = GetStringWithoutInsignificantDecimalPoints(ZArchitecture.Core.Utilities.Round(invoiceLine.JI_CustomsQuantity, 5).ToString());
			ZString netQuantityUnit = invoiceLine.JI_CustomsUnitQty.Trim();
			if (!invoiceLine.JI_WeightUQ.IsEmpty)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.Weights, null, invoiceLine.JI_WeightUQ, totalWeight);
			}

			if (!netQuantityUnit.IsEmpty)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.UnitOfMeasureUsedForInvoicedQuantities, null, netQuantityUnit, netQuantity);
			}

			if (invoiceLine.AddInfo.ZA_AssayAG_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "AG", "GPT", ZArchitecture.Core.Utilities.Round(invoiceLine.AddInfo.ZA_AssayAG_Hidden, 5).ToString());
			}
			if (invoiceLine.AddInfo.ZA_AssayAU_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "AU", "GPT", ZArchitecture.Core.Utilities.Round(invoiceLine.AddInfo.ZA_AssayAU_Hidden, 5).ToString());
			}
			if (invoiceLine.AddInfo.ZA_AssayCU_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "CU", "PER", ZArchitecture.Core.Utilities.Round(invoiceLine.AddInfo.ZA_AssayCU_Hidden, 5).ToString());
			}
			if (invoiceLine.AddInfo.ZA_AssayNI_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "NI", "PER", ZArchitecture.Core.Utilities.Round(invoiceLine.AddInfo.ZA_AssayNI_Hidden, 5).ToString());
			}
			if (invoiceLine.AddInfo.ZA_AssayPB_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "PB", "PER", ZArchitecture.Core.Utilities.Round(invoiceLine.AddInfo.ZA_AssayPB_Hidden, 5).ToString());
			}
			if (invoiceLine.AddInfo.ZA_AssayPT_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "PT", "PER", ZArchitecture.Core.Utilities.Round(invoiceLine.AddInfo.ZA_AssayPT_Hidden, 5).ToString());
			}
			if (invoiceLine.AddInfo.ZA_AssaySN_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "SN", "PER", ZArchitecture.Core.Utilities.Round(invoiceLine.AddInfo.ZA_AssaySN_Hidden, 5).ToString());
			}
			if (invoiceLine.AddInfo.ZA_AssayWO_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "WO", "PER", ZArchitecture.Core.Utilities.Round(invoiceLine.AddInfo.ZA_AssayWO_Hidden, 5).ToString());
			}
			if (invoiceLine.AddInfo.ZA_AssayZN_Hidden != 0)
			{
				MessageUtilities.PopulateMEA(MEASection[++mEANumber], MeasurementAttributeCodeList.TestResult, "ZN", "PER", ZArchitecture.Core.Utilities.Round(invoiceLine.AddInfo.ZA_AssayZN_Hidden, 5).ToString());
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
				ZDecimal result = invoiceLine.JI_Calc_FOB;
				if (result > 0 && result < 1)
				{
					result = 1;
				}

				return (long)ZArchitecture.Core.Utilities.Round(result, 0);
			}
		}

		protected abstract CSTSegmentMessageSection CSTSection
		{
			get;
		}

		protected abstract FTXSegmentMessageSection FTXSection
		{
			get;
		}

		protected abstract LOCSegmentMessageSection LOCSection
		{
			get;
		}

		protected abstract MEASegmentMessageSection MEASection
		{
			get;
		}

		protected abstract MOASegmentMessageSection MOASection
		{
			get;
		}

		protected abstract RFFSegment GetNextRFFSegment();

		//TODO: unify this with Exit1 version
		protected ZString GetStringWithoutInsignificantDecimalPoints(ZString value)
		{
			return value.IndexOf('.') == -1 ? value : value.TrimEnd('0').TrimEnd('.');
		}

		#region Implementation

		protected JobComInvoiceLine invoiceLine;

		#endregion
	}
}
