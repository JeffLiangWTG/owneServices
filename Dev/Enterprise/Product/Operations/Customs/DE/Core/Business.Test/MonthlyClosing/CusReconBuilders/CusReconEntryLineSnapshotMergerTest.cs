using System;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class CusReconEntryLineSnapshotMergerTest : TestCase
	{
		public void TestCompareTwoSnapshotsEqual()
		{
			// this test is just to make sure, the comparison of 2 snapshot works
			var snapshot1 = CreateCurrentLineSnapshot();
			var snapshot2 = CreateCurrentLineSnapshot();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("DEMonthlyClosingEntryLineSnapshotAdditionalProcedureComparer", new DEMonthlyClosingEntryLineSnapshotAdditionalProcedureComparer(), snapshot2.AdditionalProcedure, snapshot1.AdditionalProcedure);
				AssertContainsExactElementsInAnyOrder("AmountComparer", new AmountComparer(), snapshot2.Assessment.Amount, snapshot1.Assessment.Amount);
				AssertContainsExactElementsInAnyOrder("DEMonthlyClosingEntryLineSnapshotAssessmentContentInformationComparer", new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformationComparer(), snapshot2.Assessment.ContentInformation, snapshot1.Assessment.ContentInformation);
				AssertEquals("CustomsValue", snapshot2.Assessment.CustomsValue, snapshot1.Assessment.CustomsValue);
				AssertEquals("CustomsValueSpecified", snapshot2.Assessment.CustomsValueSpecified, snapshot1.Assessment.CustomsValueSpecified);
				AssertContainsExactElementsInAnyOrder("DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateComparer", new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateComparer(), snapshot2.Assessment.SpecificRate, snapshot1.Assessment.SpecificRate);
				AssertEquals("BorderTransportMeans.Information", snapshot2.BorderTransportMeans.Information, snapshot1.BorderTransportMeans.Information);
				AssertEquals("BorderTransportMeans.Mode", snapshot2.BorderTransportMeans.Mode, snapshot1.BorderTransportMeans.Mode);
				AssertEquals("BorderTransportMeans.Nationality", snapshot2.BorderTransportMeans.Nationality, snapshot1.BorderTransportMeans.Nationality);
				AssertEquals("BorderTransportMeans.Type", snapshot2.BorderTransportMeans.Type, snapshot1.BorderTransportMeans.Type);
				AssertEquals("CessionManagementFlag", snapshot2.CessionManagementFlag, snapshot1.CessionManagementFlag);
				AssertEquals("CommodityCode", snapshot2.CommodityCode, snapshot1.CommodityCode);
				AssertEquals("DepartureCountry", snapshot2.DepartureCountry, snapshot1.DepartureCountry);
				AssertContainsExactElementsInAnyOrder("DEMonthlyClosingEntryLineSnapshotDocumentComparer", new DEMonthlyClosingEntryLineSnapshotDocumentComparer(), snapshot2.Document, snapshot1.Document);
				AssertContainsExactElementsInAnyOrder("DEMonthlyClosingEntryLineSnapshotExciseDutyComparer", new DEMonthlyClosingEntryLineSnapshotExciseDutyComparer(), snapshot2.ExciseDuty, snapshot1.ExciseDuty);
				AssertEquals("ForeignTradeFlag", snapshot2.ForeignTradeFlag, snapshot1.ForeignTradeFlag);
				AssertEquals("ForeignTradeStatistics.GrossMassMeasure", snapshot2.ForeignTradeStatistics.GrossMassMeasure, snapshot1.ForeignTradeStatistics.GrossMassMeasure);
				AssertEquals("ForeignTradeStatistics.GrossMassMeasureSpecified", snapshot2.ForeignTradeStatistics.GrossMassMeasureSpecified, snapshot1.ForeignTradeStatistics.GrossMassMeasureSpecified);
				AssertEquals("ForeignTradeStatistics.InlandTransportMode", snapshot2.ForeignTradeStatistics.InlandTransportMode, snapshot1.ForeignTradeStatistics.InlandTransportMode);
				AssertEquals("InwardMovementAmount.MeasurementUnit", snapshot2.InwardMovementAmount.MeasurementUnit, snapshot1.InwardMovementAmount.MeasurementUnit);
				AssertEquals("InwardMovementAmount.Qualifier", snapshot2.InwardMovementAmount.Qualifier, snapshot1.InwardMovementAmount.Qualifier);
				AssertEquals("InwardMovementAmount.Quantity", snapshot2.InwardMovementAmount.Quantity, snapshot1.InwardMovementAmount.Quantity);
				AssertEquals("NetMassMeasure", snapshot2.NetMassMeasure, snapshot1.NetMassMeasure);
				AssertEquals("NetMassMeasureSpecified", snapshot2.NetMassMeasureSpecified, snapshot1.NetMassMeasureSpecified);
				AssertEquals("OriginCountry", snapshot2.OriginCountry, snapshot1.OriginCountry);
				AssertEquals("PreferentialCountry", snapshot2.PreferentialCountry, snapshot1.PreferentialCountry);
				AssertContainsExactElementsInAnyOrder("DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingentComparer", new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingentComparer(), snapshot2.PreferentialTreatment.Declaration.Contingent, snapshot1.PreferentialTreatment.Declaration.Contingent);
				AssertEquals("PreferentialTreatmentQuantity.MeasurementUnit", snapshot2.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.MeasurementUnit, snapshot1.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.MeasurementUnit);
				AssertEquals("PreferentialTreatmentQuantity.Qualifier", snapshot2.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.Qualifier, snapshot1.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.Qualifier);
				AssertEquals("PreferentialTreatmentQuantity.Quantity", snapshot2.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.Quantity, snapshot1.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.Quantity);
				AssertEquals("RequestedPreferentialTreatment", snapshot2.PreferentialTreatment.RequestedPreferentialTreatment, snapshot1.PreferentialTreatment.RequestedPreferentialTreatment);
				AssertContainsExactElementsInAnyOrder("DEMonthlyClosingEntryLineSnapshotSupplementaryCodesComparer", new DEMonthlyClosingEntryLineSnapshotSupplementaryCodesComparer(), snapshot2.SupplementaryCodes, snapshot1.SupplementaryCodes);
				AssertEquals("SupplementaryInformation", snapshot2.SupplementaryInformation, snapshot1.SupplementaryInformation);
				AssertEquals("TobaccoRevenueStampNumber", snapshot2.TobaccoRevenueStampNumber, snapshot1.TobaccoRevenueStampNumber);
			});
		}

		public void TestAdditionalProcedure_Current()
		{
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotAdditionalProcedureComparer(), current.AdditionalProcedure, result.AdditionalProcedure);
		}

		public void TestAdditionalProcedure_NoCurrent()
		{
			current.AdditionalProcedure = null;
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotAdditionalProcedureComparer(), lodged.AdditionalProcedure, result.AdditionalProcedure);
		}

		public void TestAssessmentAmount_Current()
		{
			AssertContainsExactElementsInAnyOrder(new AmountComparer(), current.Assessment.Amount, result.Assessment.Amount);
		}

		public void TestAssessmentAmount_NoCurrent()
		{
			current.Assessment.Amount = null;
			AssertContainsExactElementsInAnyOrder(new AmountComparer(), lodged.Assessment.Amount, result.Assessment.Amount);
		}

		public void TestAssessmentContentInformation_Current()
		{
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformationComparer(), current.Assessment.ContentInformation, result.Assessment.ContentInformation);
		}

		public void TestAssessmentContentInformation_NoCurrent()
		{
			current.Assessment.ContentInformation = null;
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformationComparer(), lodged.Assessment.ContentInformation, result.Assessment.ContentInformation);
		}

		public void TestAssessmentCustomsValue_Current()
		{
			var mergedAssessment = result.Assessment;
			CombineAssertions(() =>
			{
				AssertEquals("CustomsValue", current.Assessment.CustomsValue, mergedAssessment.CustomsValue);
				AssertEquals("CustomsValueSpecified", current.Assessment.CustomsValueSpecified, mergedAssessment.CustomsValueSpecified);
			});
		}

		public void TestAssessmentCustomsValue_NoCurrent()
		{
			current.Assessment.CustomsValueSpecified = false;
			var mergedAssessment = result.Assessment;
			CombineAssertions(() =>
			{
				AssertEquals("CustomsValue", lodged.Assessment.CustomsValue, mergedAssessment.CustomsValue);
				AssertEquals("CustomsValueSpecified", lodged.Assessment.CustomsValueSpecified, mergedAssessment.CustomsValueSpecified);
			});
		}

		public void TestAssessmentSpecificRate_Current()
		{
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateComparer(), current.Assessment.SpecificRate, result.Assessment.SpecificRate);
		}

		public void TestAssessmentSpecificRate_NoCurrent()
		{
			current.Assessment.SpecificRate = null;
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateComparer(), lodged.Assessment.SpecificRate, result.Assessment.SpecificRate);
		}

		public void TestBorderTransportMeansInformation_Current()
		{
			AssertEquals(current.BorderTransportMeans.Information, result.BorderTransportMeans.Information);
		}

		public void TestBorderTransportMeansInformation_NoCurrent()
		{
			current.BorderTransportMeans.Information = null;
			AssertEquals(lodged.BorderTransportMeans.Information, result.BorderTransportMeans.Information);
		}

		public void TestBorderTransportMeansMode_Current()
		{
			AssertEquals(current.BorderTransportMeans.Mode, result.BorderTransportMeans.Mode);
		}

		public void TestBorderTransportMeansMode_NoCurrent()
		{
			current.BorderTransportMeans.Mode = null;
			AssertEquals(lodged.BorderTransportMeans.Mode, result.BorderTransportMeans.Mode);
		}

		public void TestBorderTransportMeansNationality_Current()
		{
			AssertEquals(current.BorderTransportMeans.Nationality, result.BorderTransportMeans.Nationality);
		}

		public void TestBorderTransportMeansNationality_NoCurrent()
		{
			current.BorderTransportMeans.Nationality = null;
			AssertEquals(lodged.BorderTransportMeans.Nationality, result.BorderTransportMeans.Nationality);
		}

		public void TestBorderTransportMeansType_Current()
		{
			AssertEquals(current.BorderTransportMeans.Type, result.BorderTransportMeans.Type);
		}

		public void TestBorderTransportMeansType_NoCurrent()
		{
			current.BorderTransportMeans.Type = null;
			AssertEquals(lodged.BorderTransportMeans.Type, result.BorderTransportMeans.Type);
		}

		public void TestCessionManagementFlag_Current()
		{
			AssertEquals(current.CessionManagementFlag, result.CessionManagementFlag);
		}

		public void TestCessionManagementFlag_NoCurrent()
		{
			current.CessionManagementFlag = null;
			AssertEquals(lodged.CessionManagementFlag, result.CessionManagementFlag);
		}

		public void TestCommodityCode_Current()
		{
			AssertEquals(current.CommodityCode, result.CommodityCode);
		}

		public void TestCommodityCode_NoCurrent()
		{
			current.CommodityCode = null;
			AssertEquals(lodged.CommodityCode, result.CommodityCode);
		}

		public void TestDepartureCountry_Current()
		{
			AssertEquals(current.DepartureCountry, result.DepartureCountry);
		}

		public void TestDepartureCountry_NoCurrent()
		{
			current.DepartureCountry = null;
			AssertEquals(lodged.DepartureCountry, result.DepartureCountry);
		}

		public void TestDocument_Current()
		{
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotDocumentComparer(), current.Document, result.Document);
		}

		public void TestDocument_NoCurrent()
		{
			current.Document = null;
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotDocumentComparer(), lodged.Document, result.Document);
		}

		public void TestExciseDuty_Current()
		{
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotExciseDutyComparer(), current.ExciseDuty, result.ExciseDuty);
		}

		public void TestExciseDuty_NoCurrent()
		{
			current.ExciseDuty = null;
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotExciseDutyComparer(), lodged.ExciseDuty, result.ExciseDuty);
		}

		public void TestForeignTradeFlag_Current()
		{
			AssertEquals(current.ForeignTradeFlag, result.ForeignTradeFlag);
		}

		public void TestForeignTradeFlag_NoCurrent()
		{
			current.ForeignTradeFlag = null;
			AssertEquals(lodged.ForeignTradeFlag, result.ForeignTradeFlag);
		}

		public void TestForeignTradeStatisticsGrossMassMeasure_Current()
		{
			var mergedForeignTradeStatistics = result.ForeignTradeStatistics;
			CombineAssertions(() =>
			{
				AssertEquals("GrossMassMeasure", current.ForeignTradeStatistics.GrossMassMeasure, mergedForeignTradeStatistics.GrossMassMeasure);
				AssertEquals("GrossMassMeasureSpecified", current.ForeignTradeStatistics.GrossMassMeasureSpecified, mergedForeignTradeStatistics.GrossMassMeasureSpecified);
			});
		}

		public void TestForeignTradeStatisticsGrossMassMeasure_NoCurrent()
		{
			current.ForeignTradeStatistics.GrossMassMeasureSpecified = false;
			var mergedForeignTradeStatistics = result.ForeignTradeStatistics;
			CombineAssertions(() =>
			{
				AssertEquals("GrossMassMeasure", lodged.ForeignTradeStatistics.GrossMassMeasure, mergedForeignTradeStatistics.GrossMassMeasure);
				AssertEquals("GrossMassMeasureSpecified", lodged.ForeignTradeStatistics.GrossMassMeasureSpecified, mergedForeignTradeStatistics.GrossMassMeasureSpecified);
			});
		}

		public void TestForeignTradeStatisticsInlandTransportMode_Current()
		{
			AssertEquals(current.ForeignTradeStatistics.InlandTransportMode, result.ForeignTradeStatistics.InlandTransportMode);
		}

		public void TestForeignTradeStatisticsInlandTransportMode_NoCurrent()
		{
			current.ForeignTradeStatistics.InlandTransportMode = null;
			AssertEquals(lodged.ForeignTradeStatistics.InlandTransportMode, result.ForeignTradeStatistics.InlandTransportMode);
		}

		public void TestInwardMovementAmount_Current()
		{
			var mergedInwardMovementAmount = result.InwardMovementAmount;
			CombineAssertions(() =>
			{
				AssertEquals("MeasurementUnit", current.InwardMovementAmount.MeasurementUnit, mergedInwardMovementAmount.MeasurementUnit);
				AssertEquals("Qualifier", current.InwardMovementAmount.Qualifier, mergedInwardMovementAmount.Qualifier);
				AssertEquals("Quantity", current.InwardMovementAmount.Quantity, mergedInwardMovementAmount.Quantity);
			});
		}

		public void TestInwardMovementAmount_NoCurrent()
		{
			current.InwardMovementAmount = null;
			var mergedInwardMovementAmount = result.InwardMovementAmount;
			CombineAssertions(() =>
			{
				AssertEquals("MeasurementUnit", lodged.InwardMovementAmount.MeasurementUnit, mergedInwardMovementAmount.MeasurementUnit);
				AssertEquals("Qualifier", lodged.InwardMovementAmount.Qualifier, mergedInwardMovementAmount.Qualifier);
				AssertEquals("Quantity", lodged.InwardMovementAmount.Quantity, mergedInwardMovementAmount.Quantity);
			});
		}

		public void TestNetMassMeasure_Current()
		{
			var mergedSnapshot = result;
			CombineAssertions(() =>
			{
				AssertEquals("NetMassMeasure", current.NetMassMeasure, mergedSnapshot.NetMassMeasure);
				AssertEquals("NetMassMeasureSpecified", current.NetMassMeasureSpecified, mergedSnapshot.NetMassMeasureSpecified);
			});
		}

		public void TestNetMassMeasure_NoCurrent()
		{
			current.NetMassMeasureSpecified = false;
			var mergedSnapshot = result;
			CombineAssertions(() =>
			{
				AssertEquals("NetMassMeasure", lodged.NetMassMeasure, mergedSnapshot.NetMassMeasure);
				AssertEquals("NetMassMeasureSpecified", lodged.NetMassMeasureSpecified, mergedSnapshot.NetMassMeasureSpecified);
			});
		}

		public void TestOriginCountry_Current()
		{
			AssertEquals(current.OriginCountry, result.OriginCountry);
		}

		public void TestOriginCountry_NoCurrent()
		{
			current.OriginCountry = null;
			AssertEquals(lodged.OriginCountry, result.OriginCountry);
		}

		public void TestPreferentialCountry_Current()
		{
			AssertEquals(current.PreferentialCountry, result.PreferentialCountry);
		}

		public void TestPreferentialCountry_NoCurrent()
		{
			current.PreferentialCountry = null;
			AssertEquals(lodged.PreferentialCountry, result.PreferentialCountry);
		}

		public void TestPreferentialTreatmentDeclarationContingent_Current()
		{
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingentComparer(), current.PreferentialTreatment.Declaration.Contingent, result.PreferentialTreatment.Declaration.Contingent);
		}

		public void TestPreferentialTreatmentDeclarationContingent_NoCurrent()
		{
			current.PreferentialTreatment.Declaration.Contingent = null;
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingentComparer(), lodged.PreferentialTreatment.Declaration.Contingent, result.PreferentialTreatment.Declaration.Contingent);
		}

		public void TestPreferentialTreatmentDeclarationPreferentialTreatmentQuantity_Current()
		{
			var mergedPreferentialTreatmentQuantity = result.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity;
			CombineAssertions(() =>
			{
				AssertEquals("MeasurementUnit", current.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.MeasurementUnit, mergedPreferentialTreatmentQuantity.MeasurementUnit);
				AssertEquals("Qualifier", current.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.Qualifier, mergedPreferentialTreatmentQuantity.Qualifier);
				AssertEquals("Quantity", current.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.Quantity, mergedPreferentialTreatmentQuantity.Quantity);
			});
		}

		public void TestPreferentialTreatmentDeclarationPreferentialTreatmentQuantity_NoCurrent()
		{
			current.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity = null;
			var mergedPreferentialTreatmentQuantity = result.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity;
			CombineAssertions(() =>
			{
				AssertEquals("MeasurementUnit", lodged.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.MeasurementUnit, mergedPreferentialTreatmentQuantity.MeasurementUnit);
				AssertEquals("Qualifier", lodged.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.Qualifier, mergedPreferentialTreatmentQuantity.Qualifier);
				AssertEquals("Quantity", lodged.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.Quantity, mergedPreferentialTreatmentQuantity.Quantity);
			});
		}

		public void TestPreferentialTreatmentRequestedPreferentialTreatment_Current()
		{
			AssertEquals(current.PreferentialTreatment.RequestedPreferentialTreatment, result.PreferentialTreatment.RequestedPreferentialTreatment);
		}

		public void TestPreferentialTreatmentRequestedPreferentialTreatment_NoCurrent()
		{
			current.PreferentialTreatment.RequestedPreferentialTreatment = null;
			AssertEquals(lodged.PreferentialTreatment.RequestedPreferentialTreatment, result.PreferentialTreatment.RequestedPreferentialTreatment);
		}

		public void TestSupplementaryCodes_Current()
		{
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotSupplementaryCodesComparer(), current.SupplementaryCodes, result.SupplementaryCodes);
		}

		public void TestSupplementaryCodes_NoCurrent()
		{
			current.SupplementaryCodes = null;
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntryLineSnapshotSupplementaryCodesComparer(), lodged.SupplementaryCodes, result.SupplementaryCodes);
		}

		public void TestSupplementaryInformation_Current()
		{
			AssertEquals(current.SupplementaryInformation, result.SupplementaryInformation);
		}

		public void TestSupplementaryInformation_NoCurrent()
		{
			current.SupplementaryInformation = null;
			AssertEquals(lodged.SupplementaryInformation, result.SupplementaryInformation);
		}

		public void TestTobaccoRevenueStampNumber_Current()
		{
			AssertEquals(current.TobaccoRevenueStampNumber, result.TobaccoRevenueStampNumber);
		}

		public void TestTobaccoRevenueStampNumber_NoCurrent()
		{
			current.TobaccoRevenueStampNumber = null;
			AssertEquals(lodged.TobaccoRevenueStampNumber, result.TobaccoRevenueStampNumber);
		}

		[TestDate(2022, 01, 25, 15, 41, 19)]
		public void TestLastUpdateTimeUtc()
		{
			var mergedSnapshot = result;
			CombineAssertions(() =>
			{
				AssertEquals("LastUpdateTimeUtc", new DateTime(2022, 01, 25, 15, 41, 19), mergedSnapshot.LastUpdateTimeUtc);
				AssertEquals("LastUpdateTimeUtcSpecified", true, mergedSnapshot.LastUpdateTimeUtcSpecified);
			});
		}

		static DEMonthlyClosingEntryLineSnapshot CreateLodgedLineSnapshot()
		{
			return new DEMonthlyClosingEntryLineSnapshot
			{
				AdditionalProcedure = new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure[]
				{
					new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure { Code = "PR3" },
					new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure { Code = "PR4" }
				},
				Assessment = new DEMonthlyClosingEntryLineSnapshotAssessment
				{
					Amount = new Amount[]
					{
						new Amount { MeasurementUnit = "KGM", Quantity = 10m, Qualifier = "C" },
					},
					ContentInformation = new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation[]
					{
						new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation { Type = "C", DegreePercentage = 55m },
						new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation { Type = "D", DegreePercentage = 66m }
					},
					CustomsValue = 0m,
					CustomsValueSpecified = false,
					SpecificRate = new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate[]
					{
						new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate { Type = "C", Value = 110m },
						new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate { Type = "D", Value = 120m }
					}
				},
				BorderTransportMeans = new DEMonthlyClosingEntryLineSnapshotBorderTransportMeans { Mode = "3", Type = "02", Nationality = "IT", Information = "informatione" },
				CessionManagementFlag = "2",
				CommodityCode = "87120123111",
				DepartureCountry = "ES",
				Document = new DEMonthlyClosingEntryLineSnapshotDocument[]
				{
					new DEMonthlyClosingEntryLineSnapshotDocument { Type = "N381", Division = DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item1, ReferenceNumber = "reference", AtHandFlag = "1", IssuingDate = new DateTime(2022, 2, 26), IssuingDateSpecified = true, WriteOff = new Amount { MeasurementUnit = "KGM", Quantity = 10m, Qualifier = "A" } },
					new DEMonthlyClosingEntryLineSnapshotDocument { Type = "N991", Division = DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item2, ReferenceNumber = "reference2", AtHandFlag = "0", IssuingDate = new DateTime(2021, 2, 26), IssuingDateSpecified = true, WriteOff = new Amount { MeasurementUnit = "LTR", Quantity = 10m, Qualifier = "B" } },
				},
				ExciseDuty = new DEMonthlyClosingEntryLineSnapshotExciseDuty[]
				{
					new DEMonthlyClosingEntryLineSnapshotExciseDuty
					{
						Amount = new Amount { Quantity = 10m, MeasurementUnit = "MTR", Qualifier = null },
						Code = "A",
						DegreePercentage = 120m,
						DegreePercentageSpecified = true,
						Value = 123m,
						ValueSpecified = true
					},
					new DEMonthlyClosingEntryLineSnapshotExciseDuty
					{
						Amount = new Amount { Quantity = 12m, MeasurementUnit = "GRM", Qualifier = null },
						Code = "B",
						DegreePercentage = 112m,
						DegreePercentageSpecified = true,
						Value = 345m,
						ValueSpecified = true
					}
				},
				ForeignTradeFlag = "N",
				ForeignTradeStatistics = new DEMonthlyClosingEntryLineSnapshotForeignTradeStatistics
				{
					GrossMassMeasure = 2m,
					GrossMassMeasureSpecified = true,
					InlandTransportMode = "3"
				},
				InwardMovementAmount = new Amount { Quantity = 13, MeasurementUnit = "MTR" },
				NetMassMeasure = 1m,
				NetMassMeasureSpecified = true,
				OriginCountry = "JO",
				PreferentialCountry = "FR",
				PreferentialTreatment = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment
				{
					Declaration = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclaration
					{
						Contingent = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent[]
						{
							new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent { ContingentNumber = "Cont3" },
						},
						PreferentialTreatmentQuantity = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationPreferentialTreatmentQuantity { Quantity = 112m, MeasurementUnit = "GRM", Qualifier = "B" }
					},
					RequestedPreferentialTreatment = "100"
				},
				SupplementaryCodes = new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes[]
				{
					new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes { Code = "A" },
					new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes { Code = "B" }
				},
				SupplementaryInformation = "supplemental information lodged",
				TobaccoRevenueStampNumber = "no tobacco",
				LastUpdateTimeUtc = new DateTime(2022, 01, 14, 21, 37, 56),
				LastUpdateTimeUtcSpecified = true
			};
		}

		static DEMonthlyClosingEntryLineSnapshot CreateCurrentLineSnapshot()
		{
			return new DEMonthlyClosingEntryLineSnapshot
			{
				AdditionalProcedure = new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure[]
				{
					new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure { Code = "PR1" },
					new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure { Code = "PR2" }
				},
				Assessment = new DEMonthlyClosingEntryLineSnapshotAssessment
				{
					Amount = new Amount[]
					{
						new Amount { MeasurementUnit = "KGM", Quantity = 10m, Qualifier = "A" },
						new Amount { MeasurementUnit = "LTR", Quantity = 12m, Qualifier = "B" },
						new Amount { MeasurementUnit = "KGM", Quantity = 14m }
					},
					ContentInformation = new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation[]
					{
						new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation { Type = "A", DegreePercentage = 33m },
						new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation { Type = "B", DegreePercentage = 44m }
					},
					CustomsValue = 123m,
					CustomsValueSpecified = true,
					SpecificRate = new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate[]
					{
						new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate { Type = "A", Value = 10m },
						new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate { Type = "B", Value = 20m }
					}
				},
				BorderTransportMeans = new DEMonthlyClosingEntryLineSnapshotBorderTransportMeans { Mode = "4", Type = "01", Nationality = "DE", Information = "information" },
				CessionManagementFlag = "1",
				CommodityCode = "87120123000",
				DepartureCountry = "TR",
				Document = new DEMonthlyClosingEntryLineSnapshotDocument[]
				{
					new DEMonthlyClosingEntryLineSnapshotDocument { Type = "N380", Division = DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item2, ReferenceNumber = "reference", AtHandFlag = "1", IssuingDate = new DateTime(2022, 1, 26), IssuingDateSpecified = true, WriteOff = new Amount { MeasurementUnit = "KGM", Quantity = 10m, Qualifier = "A" } },
					new DEMonthlyClosingEntryLineSnapshotDocument { Type = "N990", Division = DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item5, ReferenceNumber = "reference2", AtHandFlag = "0", IssuingDate = new DateTime(2021, 1, 26), IssuingDateSpecified = true, WriteOff = new Amount { MeasurementUnit = "LTR", Quantity = 10m, Qualifier = "B" } },
				},
				ExciseDuty = new DEMonthlyClosingEntryLineSnapshotExciseDuty[]
				{
					new DEMonthlyClosingEntryLineSnapshotExciseDuty
					{
						Amount = new Amount { Quantity = 10m, MeasurementUnit = "LTR", Qualifier = null },
						Code = "A",
						DegreePercentage = 10m,
						DegreePercentageSpecified = true,
						Value = 123m,
						ValueSpecified = true
					},
					new DEMonthlyClosingEntryLineSnapshotExciseDuty
					{
						Amount = new Amount { Quantity = 12m, MeasurementUnit = "KGM", Qualifier = null },
						Code = "B",
						DegreePercentage = 12m,
						DegreePercentageSpecified = true,
						Value = 345m,
						ValueSpecified = true
					}
				},
				ForeignTradeFlag = "J",
				ForeignTradeStatistics = new DEMonthlyClosingEntryLineSnapshotForeignTradeStatistics
				{
					GrossMassMeasure = 1m,
					GrossMassMeasureSpecified = true,
					InlandTransportMode = "2"
				},
				InwardMovementAmount = new Amount { Quantity = 12, MeasurementUnit = "LTR" },
				NetMassMeasure = 2m,
				NetMassMeasureSpecified = true,
				OriginCountry = "IR",
				PreferentialCountry = "DE",
				PreferentialTreatment = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment
				{
					Declaration = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclaration
					{
						Contingent = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent[]
						{
							new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent { ContingentNumber = "Cont1" },
							new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent { ContingentNumber = "Cont2" }
						},
						PreferentialTreatmentQuantity = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationPreferentialTreatmentQuantity { Quantity = 12m, MeasurementUnit = "KGM", Qualifier = "A" }
					},
					RequestedPreferentialTreatment = "200"
				},
				SupplementaryCodes = new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes[]
				{
					new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes { Code = "X" },
					new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes { Code = "Y" }
				},
				SupplementaryInformation = "supplemental information",
				TobaccoRevenueStampNumber = "tobacco"
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			current = CreateCurrentLineSnapshot();
			lodged = CreateLodgedLineSnapshot();
		}
		DEMonthlyClosingEntryLineSnapshot lodged;
		DEMonthlyClosingEntryLineSnapshot current;

		DEMonthlyClosingEntryLineSnapshot result => CusReconEntryLineSnapshotMerger.DoMerge(lodged, current);
	}
}
