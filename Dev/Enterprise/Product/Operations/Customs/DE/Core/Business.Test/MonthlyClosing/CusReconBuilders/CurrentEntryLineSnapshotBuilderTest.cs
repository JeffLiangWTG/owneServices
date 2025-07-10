using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	sealed class CurrentEntryLineSnapshotBuilderTest : TestCaseWithFactory
	{
		public void TestUpdateDeMonthlyClosingEntryLineSnapshot()
		{
			providerMock.Setup(m => m.CessionManagementFlag).Returns("Muzha");
			providerMock.Setup(m => m.NetMassMeasure).Returns(1.2M);
			providerMock.Setup(m => m.CommodityCode).Returns("Nezha");
			providerMock.Setup(m => m.OriginCountry).Returns("CN");
			providerMock.Setup(m => m.PreferentialCountry).Returns("DE");
			providerMock.Setup(m => m.DepartureCountry).Returns("BE");
			providerMock.Setup(m => m.SupplementaryInformation).Returns("NJ000001");
			providerMock.Setup(m => m.ForeignTradeFlag).Returns("Y");

			var message = snapshotBuilder.GenerateMessage();

			CombineAssertions(() =>
			{
				AssertEquals("CessionManagementFlag", "Muzha", message.CessionManagementFlag);
				AssertEquals("NetMassMeasure", 1.2M, message.NetMassMeasure);
				AssertEquals("CommodityCode", "Nezha", message.CommodityCode);
				AssertEquals("OriginCountry", "CN", message.OriginCountry);
				AssertEquals("PreferentialCountry", "DE", message.PreferentialCountry);
				AssertEquals("DepartureCountry", "BE", message.DepartureCountry);
				AssertEquals("SupplementaryInformation", "NJ000001", message.SupplementaryInformation);
				AssertEquals("ForeignTradeFlag", "Y", message.ForeignTradeFlag);

				basicSnapshot.CessionManagementFlag = "Muzha";
				message = snapshotBuilder.GenerateMessage();
				AssertEquals("CessionManagementFlag should be write even same, Because the CommodityCode is different", "Muzha", message.CessionManagementFlag);
			});
		}

		public void TestUpdateCessionManagementFlagForSpecial()
		{
			providerMock.Setup(m => m.CommodityCode).Returns("TestTest");
			basicSnapshot.CommodityCode = "TestTest";

			CombineAssertions(() =>
			{
				basicSnapshot.CessionManagementFlag = "01";
				providerMock.Setup(m => m.CessionManagementFlag).Returns("");
				providerMock.Setup(m => m.NetMassMeasure).Returns(1.2M);

				var message = snapshotBuilder.GenerateMessage();
				AssertEquals("CommodityCode Should be Update", "TestTest", message.CommodityCode);
				Assert("NetMassMeasureSpecified Should be False", !message.NetMassMeasureSpecified);

				basicSnapshot.CessionManagementFlag = "CN";
				message = snapshotBuilder.GenerateMessage();
				AssertNull("CommodityCode Should be NULL", message.CommodityCode);
				Assert("NetMassMeasureSpecified Should be True", message.NetMassMeasureSpecified);
				AssertEquals("NetMassMeasure", 1.2M, message.NetMassMeasure);
			});
		}

		public void TestUpdateAdditionalProceduce()
		{
			CombineAssertions(() =>
			{
				AssertNull("AdditionalProcedure Should Be Null", snapshotBuilder.GenerateMessage().AdditionalProcedure);
				providerMock.Setup(m => m.AdditionalProcedure).Returns(new[] { "HUIJIAZHONGDI", "GOHOMETOFARM" });
				var actual = new List<string>();
				var message = snapshotBuilder.GenerateMessage();
				message.AdditionalProcedure.ForEach(x => actual.Add(x.Code));
				AssertContainsExactElementsInAnyOrder("AdditionalProcedure Changed", new[] { "HUIJIAZHONGDI", "GOHOMETOFARM" }, actual);
				AssertNotNull("CommodityCode", message.CommodityCode);
			});
		}

		public void TestUpdateAdditionalProceduceSpecial()
		{
			basicSnapshot.AdditionalProcedure = new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure[]
			{
				new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure() { Code = "INIT0001" },
				new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure() { Code = "INIT0002" }
			};
			providerMock.Setup(m => m.AdditionalProcedure).Returns(new[] { "INIT0001", "INIT0002" });

			CombineAssertions(() =>
			{
				var message = snapshotBuilder.GenerateMessage();
				AssertNull("AdditionalProcedure Should Be Null", message.AdditionalProcedure);

				providerMock.Setup(m => m.CommodityCode).Returns("Update");
				var acutal = new List<string>();
				snapshotBuilder.GenerateMessage().AdditionalProcedure.ForEach(x => acutal.Add(x.Code));
				AssertContainsExactElementsInAnyOrder(new[] { "INIT0001", "INIT0002" }, acutal);
			});
		}

		public void TestUpdateSupplementaryCodes()
		{
			CombineAssertions(() =>
			{
				AssertNull("SupplementaryCodes Should be null", snapshotBuilder.GenerateMessage().SupplementaryCodes);
				providerMock.Setup(m => m.SupplementaryCodes).Returns(new[] { "BIANBUXIAQULE", "TOHARDFORME" });
				var result = new List<string>();

				var message = snapshotBuilder.GenerateMessage();
				message.SupplementaryCodes.ForEach(x => result.Add(x.Code));
				AssertContainsExactElementsInAnyOrder("SupplementaryCodes Changed", new[] { "BIANBUXIAQULE", "TOHARDFORME" }, result);
				AssertNotNull("CommodityCode", message.CommodityCode);
			});
		}

		public void TestUpdateSupplementaryCodesSpecical()
		{
			basicSnapshot.SupplementaryCodes = new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes[]
			{
				new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes() { Code = "BIANBUXIAQULE" },
				new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes() { Code = "TOHARDFORME" }
			};
			providerMock.Setup(m => m.SupplementaryCodes).Returns(new[] { "BIANBUXIAQULE", "TOHARDFORME" });

			CombineAssertions(() =>
			{
				var message = snapshotBuilder.GenerateMessage();
				AssertNull("SupplementaryCodes Should Be Null", message.SupplementaryCodes);

				providerMock.Setup(m => m.CommodityCode).Returns("Update");
				var acutal = new List<string>();
				snapshotBuilder.GenerateMessage().SupplementaryCodes.ForEach(x => acutal.Add(x.Code));
				AssertContainsExactElementsInAnyOrder(new[] { "BIANBUXIAQULE", "TOHARDFORME" }, acutal);
			});
		}

		public void TestUpdateForeignTradeStatics()
		{
			providerMock.Setup(m => m.ForeignTradeStatisticsInlandTransportMode).Returns("BUHUIXIELE");
			providerMock.Setup(m => m.ForeignTradeStatisticsGrossMassMeasure).Returns(10.2M);
			var message = snapshotBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				AssertEquals("InlandTransportMode", "BUHUIXIELE", message.ForeignTradeStatistics.InlandTransportMode);
				AssertEquals("GrossMassMeasure", 10.2M, message.ForeignTradeStatistics.GrossMassMeasure);
				AssertEquals("GrossMassMeasureSpecified", true, message.ForeignTradeStatistics.GrossMassMeasureSpecified);
			});
		}

		public void TestUpdateInwardMovementAmount()
		{
			providerMock.Setup(m => m.InwardMovementAmount).Returns(CusReconBuildersTestHelper.GetAmount("H", 13.333M, "KGM"));
			var message = snapshotBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 13.333M, message.InwardMovementAmount.Quantity);
				AssertEquals("MeasurementUnit", "KGM", message.InwardMovementAmount.MeasurementUnit);
				AssertEquals("Qualifier", "H", message.InwardMovementAmount.Qualifier);
			});
		}

		public void TestUpdateUpdateCustomsValue()
		{
			providerMock.Setup(m => m.AssessmentCustomsValue).Returns(8.88M);
			CombineAssertions(() =>
			{
				AssertEquals("CustomsValue", 8.88M, snapshotBuilder.GenerateMessage().Assessment.CustomsValue);
				AssertEquals("CustomsValueSpecified", true, snapshotBuilder.GenerateMessage().Assessment.CustomsValueSpecified);
			});
		}

		public void TestUpdateAmount()
		{
			CombineAssertions(() =>
			{
				AssertNull("No Change", snapshotBuilder.GenerateMessage().Assessment?.Amount);

				providerMock.Setup(m => m.AssessmentAmount).Returns(new IAmount[] { CusReconBuildersTestHelper.GetAmount("A", 8.888M, "NAR"), CusReconBuildersTestHelper.GetAmount("C", 10.111M, "ABC") });

				AssertContainsExactElementsInAnyOrder("Collection", new AmountComparer(),
					new Amount[]
					{
						CusReconBuildersTestHelper.CreateAmountForBizo("A", 8.888M, "NAR"),
						CusReconBuildersTestHelper.CreateAmountForBizo("C", 10.111M, "ABC")
					}, snapshotBuilder.GenerateMessage().Assessment.Amount);
			});
		}

		public void TestUpdateSpecificRate()
		{
			CombineAssertions(() =>
			{
				AssertNull("No Change", snapshotBuilder.GenerateMessage().Assessment?.SpecificRate);

				providerMock.Setup(m => m.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { CusReconBuildersTestHelper.GetImportSpecificRate("M", 22.22M), CusReconBuildersTestHelper.GetImportSpecificRate("E", 11.33M) });

				var message = snapshotBuilder.GenerateMessage();
				AssertContainsExactElementsInAnyOrder("Collection", new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateComparer(),
					new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate[]
					{
						CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateForBizo("M", 22.22M),
						CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateForBizo("E", 11.33M),
					}, snapshotBuilder.GenerateMessage().Assessment.SpecificRate);
			});
		}

		public void TestUpdateContentInformation()
		{
			CombineAssertions(() =>
			{
				AssertNull("No Change", snapshotBuilder.GenerateMessage().Assessment?.ContentInformation);

				providerMock.Setup(m => m.AssessmentContentInformation).Returns(new IContentInformation[] { CusReconBuildersTestHelper.GetContentInformation("M", 11.22M), CusReconBuildersTestHelper.GetContentInformation("G", 11.55M) });

				AssertContainsExactElementsInAnyOrder("Collection", new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformationComparer(),
					new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation[]
					{
						CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotAssessmentContentInformationForBizo("M", 11.22M),
						CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotAssessmentContentInformationForBizo("G", 11.55M)
					}, snapshotBuilder.GenerateMessage().Assessment.ContentInformation);
			});
		}

		public void TestUpdateExciseDuty()
		{
			CombineAssertions(() =>
			{
				AssertNull("No Change", snapshotBuilder.GenerateMessage().ExciseDuty);

				providerMock.Setup(m => m.ExciseDuty).Returns(new IExciseDuty[] { CusReconBuildersTestHelper.GetExciseDuty("A123", 0.02M, 1626.28M), CusReconBuildersTestHelper.GetExciseDuty("N123", 0.02M, 1626.28M) });

				AssertContainsExactElementsInAnyOrder("Collection", new DEMonthlyClosingEntryLineSnapshotExciseDutyComparer(),
					new DEMonthlyClosingEntryLineSnapshotExciseDuty[]
					{
						CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotExciseDutyForBizo("A123", 0.02M, 1626.28M, CusReconBuildersTestHelper.CreateAmountForBizo("X", 11.111M, "NAR")),
						CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotExciseDutyForBizo("N123", 0.02M, 1626.28M, CusReconBuildersTestHelper.CreateAmountForBizo("X", 11.111M, "NAR"))
					}, snapshotBuilder.GenerateMessage().ExciseDuty);
			});
		}

		public void TestUpdateRequestedPreferentialTreatment()
		{
			CombineAssertions(() =>
			{
				AssertNull("No Change", snapshotBuilder.GenerateMessage().PreferentialTreatment);

				providerMock.Setup(m => m.PreferentialTreatment).Returns(CusReconBuildersTestHelper.GetPreferentialTreatment(CusReconBuildersTestHelper.GetAmount("M", 11M, "NAR"), "300"));
				var message = snapshotBuilder.GenerateMessage();
				Assert("RequestedPreferentialTreatment", message.PreferentialTreatment.RequestedPreferentialTreatment == "300");
			});
		}

		public void TestUpdateRequestedPreferentialTreatment_Null()
		{
			providerMock.Setup(m => m.PreferentialTreatment).Returns((ILinePreferentialTreatment)null);
			AssertNoExceptionThrown(() => snapshotBuilder.GenerateMessage());
		}

		public void TestUpdateDeclarationContingentAndSpecial()
		{
			basicSnapshot.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity
				= new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationPreferentialTreatmentQuantity() { Qualifier = "X", Quantity = 11M, MeasurementUnit = "NAR" };

			ILinePreferentialTreatment treatment = null;
			providerMock.Setup(m => m.PreferentialTreatment)
				.Returns(() =>
				{
					treatment = CusReconBuildersTestHelper.GetPreferentialTreatment(CusReconBuildersTestHelper.GetAmount("X", 11M, "NAR"));
					return treatment;
				});

			CombineAssertions(() =>
			{
				AssertNull("No Change", snapshotBuilder.GenerateMessage().PreferentialTreatment?.Declaration?.Contingent);
				AssertNull("No Change", snapshotBuilder.GenerateMessage().PreferentialTreatment?.Declaration?.PreferentialTreatmentQuantity);

				providerMock.Setup(m => m.PreferentialTreatment).Returns(CusReconBuildersTestHelper.GetPreferentialTreatment(treatment.Quantity, contingentNumber: new string[] { "abc", "xyz" }));

				var messageContingentList = new List<string>();
				var message = snapshotBuilder.GenerateMessage();
				message.PreferentialTreatment.Declaration.Contingent.ForEach(x => messageContingentList.Add(x.ContingentNumber));
				AssertContainsExactElementsInAnyOrder("Collection", new string[] { "abc", "xyz" }, messageContingentList);

				var preferentialTreatmentQuantity = message.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity;
				Assert("RequestedPreferentialTreatment", preferentialTreatmentQuantity.Qualifier == "X"
					&& preferentialTreatmentQuantity.Quantity == 11M && preferentialTreatmentQuantity.MeasurementUnit == "NAR");
			});
		}

		public void TestUpdateDeclarationContingent_Null()
		{
			providerMock.Setup(m => m.PreferentialTreatment).Returns((ILinePreferentialTreatment)null);
			AssertNoExceptionThrown(() => snapshotBuilder.GenerateMessage());
		}

		public void TestUpdateDeclarationPreferentialTreatmentQuantityQuantity()
		{
			CombineAssertions(() =>
			{
				AssertNull("No Change", snapshotBuilder.GenerateMessage().PreferentialTreatment?.Declaration?.PreferentialTreatmentQuantity);

				providerMock.Setup(m => m.PreferentialTreatment).Returns(CusReconBuildersTestHelper.GetPreferentialTreatment(CusReconBuildersTestHelper.GetAmount("J", 11M, "NAR")));
				var preferentialTreatmentQuantity = snapshotBuilder.GenerateMessage().PreferentialTreatment.Declaration.PreferentialTreatmentQuantity;
				Assert("RequestedPreferentialTreatment", preferentialTreatmentQuantity.Qualifier == "J"
					&& preferentialTreatmentQuantity.Quantity == 11M && preferentialTreatmentQuantity.MeasurementUnit == "NAR");
			});
		}

		public void TestUpdateDocument()
		{
			CombineAssertions(() =>
			{
				AssertNull("No Change", snapshotBuilder.GenerateMessage().Document);

				providerMock.Setup(m => m.Documents).Returns(new IImportLineDocument[] { CusReconBuildersTestHelper.GetImportLineDocument("1", "7HF", "COSU6271657530", new DateTime(2021, 08, 12), "J") });

				AssertContainsExactElementsInAnyOrder("Collection", new DEMonthlyClosingEntryLineSnapshotDocumentComparer(),
					new DEMonthlyClosingEntryLineSnapshotDocument[]
					{
						CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotDocumentForBizo("1", "7HF", "COSU6271657530", new ZDate(2021, 08, 12), "J", CusReconBuildersTestHelper.CreateAmountForBizo("Z", 11.888M, "NAR"))
					}, snapshotBuilder.GenerateMessage().Document);
			});
		}

		public void TestUpdateBorderTransportMeans()
		{
			CombineAssertions(() =>
			{
				providerMock.Setup(m => m.BorderTransportMeansType).Returns("B1");
				var message = snapshotBuilder.GenerateMessage();

				AssertEquals("Type", "B1", message.BorderTransportMeans.Type);
				AssertNullOrEmpty("Mode", message.BorderTransportMeans.Mode);
				AssertNullOrEmpty("Nationality", message.BorderTransportMeans.Nationality);
				AssertNullOrEmpty("Information", message.BorderTransportMeans.Information);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			providerMock = new Mock<IMonthlyClosingEntryLineSnapshot>();
			providerMock.Setup(p => p.CessionManagementFlag).Returns("C1");
			providerMock.Setup(p => p.NetMassMeasure).Returns(2.2M);
			providerMock.Setup(p => p.OriginCountry).Returns("DE");
			providerMock.Setup(p => p.PreferentialCountry).Returns("BE");
			providerMock.Setup(p => p.DepartureCountry).Returns("AU");
			providerMock.Setup(p => p.SupplementaryInformation).Returns("SI000001");
			providerMock.Setup(p => p.TobaccoRevenueStampNumber).Returns("TRSN0001");
			providerMock.Setup(p => p.CommodityCode).Returns("CC000002");
			providerMock.Setup(p => p.AdditionalProcedure).Returns(new[] { "AP000001", "AP000002" });
			providerMock.Setup(p => p.SupplementaryCodes).Returns(new[] { "SC000001", "SC000002" });
			providerMock.Setup(p => p.ForeignTradeStatisticsInlandTransportMode).Returns("FTSITM01");
			providerMock.Setup(p => p.ForeignTradeStatisticsGrossMassMeasure).Returns(4.4M);
			providerMock.Setup(p => p.AssessmentCustomsValue).Returns(6.66M);
			providerMock.Setup(p => p.AssessmentAmount).Returns(new IAmount[] { CusReconBuildersTestHelper.GetAmount("B", 9.999M, "TNE"), CusReconBuildersTestHelper.GetAmount("C", 10.111M, "ABC") });
			providerMock.Setup(p => p.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { CusReconBuildersTestHelper.GetImportSpecificRate("D", 11.22M), CusReconBuildersTestHelper.GetImportSpecificRate("E", 11.33M) });
			providerMock.Setup(p => p.AssessmentContentInformation).Returns(new IContentInformation[] { CusReconBuildersTestHelper.GetContentInformation("F", 11.44M), CusReconBuildersTestHelper.GetContentInformation("G", 11.55M) });
			providerMock.Setup(p => p.ExciseDuty).Returns(new IExciseDuty[] { CusReconBuildersTestHelper.GetExciseDuty("A123", 0.02M, 1626.28M) });
			providerMock.Setup(p => p.PreferentialTreatment).Returns(CusReconBuildersTestHelper.GetPreferentialTreatment(CusReconBuildersTestHelper.GetAmount("X", 11M, "NAR")));
			providerMock.Setup(p => p.Documents).Returns(new IImportLineDocument[] { CusReconBuildersTestHelper.GetImportLineDocument("4", "7HHF", "COSU6271657530", new DateTime(2020, 08, 12), "J") });
			providerMock.Setup(p => p.InwardMovementAmount).Returns(CusReconBuildersTestHelper.GetAmount("D", 13.333M, "KGM"));
			providerMock.Setup(p => p.BorderTransportMeansMode).Returns("M");
			providerMock.Setup(p => p.BorderTransportMeansType).Returns("T1");
			providerMock.Setup(p => p.BorderTransportMeansInformation).Returns("BTMI0001");
			providerMock.Setup(p => p.BorderTransportMeansNationality).Returns("US");
			providerMock.Setup(m => m.ForeignTradeFlag).Returns("F");

			basicSnapshot = new DEMonthlyClosingEntryLineSnapshot()
			{
				CessionManagementFlag = "C1",
				NetMassMeasure = 2.2M,
				NetMassMeasureSpecified = true,
				OriginCountry = "DE",
				PreferentialCountry = "BE",
				DepartureCountry = "AU",
				SupplementaryInformation = "SI000001",
				TobaccoRevenueStampNumber = "TRSN0001",
				CommodityCode = "CC000002",
				AdditionalProcedure = new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure[]
				{
					new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure() { Code = "AP000001" },
					new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure() { Code = "AP000002" }
				},
				SupplementaryCodes = new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes[]
				{
					new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes() { Code = "SC000001" },
					new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes() { Code = "SC000002" }
				},
				ForeignTradeStatistics = new DEMonthlyClosingEntryLineSnapshotForeignTradeStatistics() { GrossMassMeasure = 4.4M, GrossMassMeasureSpecified = true, InlandTransportMode = "FTSITM01" },
				Assessment = new DEMonthlyClosingEntryLineSnapshotAssessment()
				{
					Amount = new Amount[]
					{
						CusReconBuildersTestHelper.CreateAmountForBizo("B", 9.999M, "TNE"),
						CusReconBuildersTestHelper.CreateAmountForBizo("C", 10.111M, "ABC")
					},
					CustomsValue = 6.66M,
					CustomsValueSpecified = true,
					SpecificRate = new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate[]
					{
						CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateForBizo("D", 11.22M),
						CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateForBizo("E", 11.33M),
					},
					ContentInformation = new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation[]
					{
						CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotAssessmentContentInformationForBizo("F", 11.44M),
						CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotAssessmentContentInformationForBizo("G", 11.55M)
					}
				},
				ExciseDuty = new DEMonthlyClosingEntryLineSnapshotExciseDuty[]
				{
					CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotExciseDutyForBizo("A123", 0.02M, 1626.28M, CusReconBuildersTestHelper.CreateAmountForBizo("X", 11.111M, "NAR"))
				},
				PreferentialTreatment = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment()
				{
					RequestedPreferentialTreatment = "200",
					Declaration = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclaration()
					{
						Contingent = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent[] { new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent() { ContingentNumber = "1XYZ" } },
						PreferentialTreatmentQuantity = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationPreferentialTreatmentQuantity() { Qualifier = "X", Quantity = 11M, MeasurementUnit = "NAR" }
					}
				},
				Document = new DEMonthlyClosingEntryLineSnapshotDocument[]
				{
					CusReconBuildersTestHelper.CreateDEMonthlyClosingEntryLineSnapshotDocumentForBizo("4", "7HHF", "COSU6271657530", new ZDate(2020, 08, 12), "J", CusReconBuildersTestHelper.CreateAmountForBizo("Z", 11.888M, "NAR"))
				},
				InwardMovementAmount = CusReconBuildersTestHelper.CreateAmountForBizo("D", 13.333M, "KGM"),
				BorderTransportMeans = new DEMonthlyClosingEntryLineSnapshotBorderTransportMeans()
				{
					Mode = "M",
					Type = "T1",
					Information = "BTMI0001",
					Nationality = "US"
				},
				ForeignTradeFlag = "F"
			};

			snapshotBuilder = new CurrentEntryLineSnapshotBuilder(providerMock.Object, basicSnapshot);
		}

		Mock<IMonthlyClosingEntryLineSnapshot> providerMock;
		DEMonthlyClosingEntryLineSnapshot basicSnapshot;
		CurrentEntryLineSnapshotBuilder snapshotBuilder;
	}
}
