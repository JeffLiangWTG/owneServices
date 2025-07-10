using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

#if DEBUG
using Enterprise.Customs.Universal.Testing;
#endif

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DutyCalculationManager : IAUDutyCalculationManager
	{
		public void Calculate(IEnumerable<CusEntryHeader> entries)
		{
			CalculateCore(new TypedEnumerable<IHeaderFeeData>(entries));
		}

		#region IDutyCalculationManager Members

		void IDutyCalculationManager.Calculate(IEnumerable<IHeaderFeeData> entries)
		{
			CalculateCore(entries);
		}

		void CalculateCore(IEnumerable<IHeaderFeeData> entries)
		{
			ConsolidatedDeclaration consolidatedDeclaration = null;
			IDeclarationChargeProvider leadHeader = null;
			ZDecimal consolidatedWoodLevy = 0m;

			foreach (IDeclarationChargeProvider entry in entries)
			{
				JobDeclaration declaration = null;
				if (entry is CusEntryHeader auEntryHeader)
				{
					declaration = auEntryHeader.Declaration;

					if (consolidatedDeclaration == null)
					{
						consolidatedDeclaration = (ConsolidatedDeclaration)ConsolidatedDeclaration.GetConsolidatedDeclaration(declaration);
					}
					if (consolidatedDeclaration != null && consolidatedDeclaration.CRD_JE_LeadDeclaration == declaration.PK)
					{
						leadHeader = entry;
					}
				}

				var headerWoodLevy = CalculateLineFeesAndHeaderWoodLevy(entry);
				if (consolidatedDeclaration != null)
				{
					consolidatedWoodLevy += headerWoodLevy;
					headerWoodLevy = 0m;
				}
				entry.SetFeeResult(CusEntryChargeTypeList.Codes.Woodlevy, headerWoodLevy);

				CalculateHeaderFees(entry, declaration, consolidatedDeclaration, entry == leadHeader);
			}

			leadHeader?.SetFeeResult(CusEntryChargeTypeList.Codes.Woodlevy, consolidatedWoodLevy);
		}

		ZDecimal CalculateLineFeesAndHeaderWoodLevy(IHeaderFeeData entry)
		{
			ZDecimal headerWoodLevy = 0m;

			foreach (ICMRDutyData line in entry.Lines)
			{
				IDutyCalculator calculator = GetCalculator(line);

				line.SetDutyResult(calculator.Duty);
				line.SetFeeResult(CusEntryChargeTypeList.Codes.WetAmount, calculator.WET);
				line.SetFeeResult(CusEntryChargeTypeList.Codes.LCTAmount, calculator.LCT);
				line.SetFeeResult(!line.IsDutyAndTaxEstimatedForWH && line.IsGSTDeferred ? CusEntryChargeTypeList.Codes.GSTDeferred : CusEntryChargeTypeList.Codes.GSTAmount, calculator.GST);

				headerWoodLevy += calculator.WoodLevy;
			}

			return headerWoodLevy;
		}

		void CalculateHeaderFees(IDeclarationChargeProvider entry, JobDeclaration declaration, ConsolidatedDeclaration consolidatedDeclaration, bool isLeadEntryOfConsolidatedDeclaration)
		{
			var entryHeader = entry as CusEntryHeader;
			var isLodgedWithoutProcessingCharge = entryHeader != null && entryHeader.HasBeenLodgedAtCustoms && entryHeader.DeclarationProcessingCharge.IsEmpty;
			if (!entry.IsExemptedFromCustomsAndQuarantineFees && !isLodgedWithoutProcessingCharge)
			{
				IDeclarationChargeProvider feeCalculationEntryProvider = null;

				if (consolidatedDeclaration == null)
				{
					feeCalculationEntryProvider = entry;
				}
				else if (isLeadEntryOfConsolidatedDeclaration)
				{
					var aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration() as JobDeclaration;
					feeCalculationEntryProvider = aggregateDeclaration?.EntryHeader;
				}

				var aqisContainerCharge = ZDecimal.Zero;
				var aqisProcessingCharge = ZDecimal.Zero;
				var declarationProcessingCharge = ZDecimal.Zero;

				if (feeCalculationEntryProvider != null)
				{
					var headerChargeCalculator = new CMRDeclarationChargeCalculator(feeCalculationEntryProvider);
					aqisContainerCharge = headerChargeCalculator.AQISContainerCharge;
					aqisProcessingCharge = headerChargeCalculator.AQISProcessingCharge;

					if (declaration == null || !declaration.SettlementTypeSelected)
					{
						declarationProcessingCharge = headerChargeCalculator.DeclarationProcessingCharge;
					}
				}

				entry.SetFeeResult(CusEntryChargeTypeList.Codes.AQISContainerCharges, aqisContainerCharge);
				entry.SetFeeResult(CusEntryChargeTypeList.Codes.AQISProcessingCharge, aqisProcessingCharge);
				entry.SetFeeResult(CusEntryChargeTypeList.Codes.DeclarationProcessingCharge, declarationProcessingCharge);
			}

			if (entryHeader?.IsDutyDeferred ?? false)
			{
				entry.SetFeeResult(CusEntryChargeTypeList.Codes.DutyDeferredAmount, entryHeader.EstimatedDeferredDutyAndCharges);
			}
		}

		IDutyCalculator GetCalculator(ICMRDutyData line)
		{
			IDutyCalculator result = new CMRDutyCalculator(line);

#if DEBUG
			if (Globals.IsTest)
			{
				CusEntryLine entryLine = line as CusEntryLine;

				if (entryLine != null && entryLine.Declaration != null && !entryLine.Declaration.IsImportCMR)
				{
					result = new DutyCalculator(entryLine);
				}
			}
#endif
			return result;
		}

#if DEBUG
		void IAUDutyCalculationManager.CreateTaxOrFee()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);

			var tax1 = helper.CreateTaxOrFee("DAN", 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 6, 6));
			tax1.ZZF_Threshold = 10000m;
			helper.CreateTaxOrFee("DNW", 23.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 6, 6));
			var tax3 = helper.CreateTaxOrFee("DPN", 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 6, 6));
			tax3.ZZF_Threshold = 10000m;
			var tax4 = helper.CreateTaxOrFee("DSN", 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(2006, 5, 10), new ZDateTime(2079, 6, 6));
			tax4.ZZF_Threshold = 10000m;
			var tax5 = helper.CreateTaxOrFee("DAH", 152.0M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 6, 6));
			tax5.ZZF_Threshold = 10000m;
			var tax6 = helper.CreateTaxOrFee("DSH", 152.0M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 6, 6));
			tax6.ZZF_Threshold = 10000m;
			var tax7 = helper.CreateTaxOrFee("DPH", 152.0M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 6, 6));
			tax7.ZZF_Threshold = 10000m;

			var tax11 = helper.CreateTaxOrFee("DAN", 40.20M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(2006, 5, 10), new ZDateTime(2015, 12, 31, 23, 59, 59));
			tax11.ZZF_Threshold = 10000m;
			helper.CreateTaxOrFee("DNW", 23.20M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2006, 5, 10), new ZDateTime(2015, 12, 31, 23, 59, 59));
			var tax13 = helper.CreateTaxOrFee("DPN", 40.20M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(2006, 5, 10), new ZDateTime(2015, 12, 31, 23, 59, 59));
			tax13.ZZF_Threshold = 10000m;
			var tax8 = helper.CreateTaxOrFee("DAH", 122.10M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2014, 1, 1), new ZDateTime(2015, 12, 31, 23, 59, 59));
			tax8.ZZF_Threshold = 10000m;
			var tax9 = helper.CreateTaxOrFee("DSH", 152.60M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2014, 1, 1), new ZDateTime(2015, 12, 31, 23, 59, 59));
			tax9.ZZF_Threshold = 10000m;
			var tax10 = helper.CreateTaxOrFee("DPH", 122.10M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2014, 1, 1), new ZDateTime(2015, 12, 31, 23, 59, 59));
			tax10.ZZF_Threshold = 10000m;

			helper.CreateTaxOrFee("Q1A", 33M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2015, 12, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q1S", 42M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2015, 12, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q1A", 16M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2015, 11, 30, 23, 59, 59));
			helper.CreateTaxOrFee("Q1S", 15M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2015, 11, 30, 23, 59, 59));
			helper.CreateTaxOrFee("Q2A", 33M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2015, 12, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2S", 42M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2015, 12, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2A", 16M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2015, 11, 30, 23, 59, 59));
			helper.CreateTaxOrFee("Q2S", 15M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2015, 11, 30, 23, 59, 59));

			helper.CreateTaxOrFee("Q1F", 30M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2013, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2F", 30M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2013, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q1X", 30M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2013, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2X", 30M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2013, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q1L", 8M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2013, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2L", 8M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(2013, 1, 1), new ZDateTime(2079, 6, 6));

			factory.Save();
		}
#endif

		#endregion
	}
}
