using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(MonthlyClosingDecLineProvider))]
	sealed class MonthlyClosingDecLineProviderBaseOnlyTest : MonthlyClosingDecLineProviderAbstractTest<MonthlyClosingDecLineProvider, IImportDecHeader, IImportDecLine>
	{
		public void TestReferencedSequenceNumber()
		{
			AssertEquals(42, Provider.ReferencedSequenceNumber);
		}

		public void TestMatterCode()
		{
			invoiceLine.JI_CustomAttrib1 = "Matter";
			AssertEquals("Matter", Provider.MatterCode);
		}

		public void TestArticleNumber()
		{
			invoiceLine.JI_PartNo = "PARTNO1";
			AssertEquals("PARTNO1", Provider.ArticleNumber);
		}

		public void TestInvoiceAmount()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine.JI_LinePrice = 12.34;
			invoiceLine2.JI_LinePrice = 11.11;
			AssertEquals(23.45m, Provider.InvoiceAmount);
		}

		public void TestDepartureCountry_Provider()
		{
			isModificationMessage = true;
			Mock.Get(headerProvider).Setup(x => x.DepartureCountry).Returns("AU");
			AssertEquals("AU", Provider.DepartureCountry);
		}

		public void TestDepartureCountry_Snapshot()
		{
			isModificationMessage = false;
			snapshot.DepartureCountry = "NZ";
			AssertEquals("NZ", Provider.DepartureCountry);
		}

		public void TestDepartureCountry_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(string.Empty, Provider.DepartureCountry);
		}

		public void TestCompleteDeclarationFlag()
		{
			AssertEquals(true, Provider.CompleteDeclarationFlag);
		}

		public void TestForeignTradeStatisticsGoodsStatus()
		{
			declaration.JE_StatisticStatus = "1";
			AssertEquals("1", Provider.ForeignTradeStatisticsGoodsStatus);
		}

		public void TestForeignTradeStatisticsTransactionType()
		{
			invoice.JZ_ValuationCode = "X";
			AssertEquals("X", Provider.ForeignTradeStatisticsTransactionType);
		}

		public void TestForeignTradeStatisticsDestinationCountry()
		{
			declaration.JE_GoodsDestination = "DE";
			AssertEquals("DE", Provider.ForeignTradeStatisticsDestinationCountry);
		}

		public void TestForeignTradeStatisticsDestinationFederalState()
		{
			var loader = new RefUNLOCO.Loader(Factory);
			var berlin = loader.Load("DEBER");
			berlin.CountryStates.RW_Code = "HH";
			declaration.JE_RL_NKFinalDestination = berlin.Code;
			declaration.JE_GoodsDestination = CountryCodes.Germany;

			AssertEquals("02", Provider.ForeignTradeStatisticsDestinationFederalState);
		}

		public void TestForeignTradeStatisticsInlandTransportMode_Provider()
		{
			isModificationMessage = true;
			Mock.Get(headerProvider).Setup(x => x.ForeignTradeStatisticsInlandTransportMode).Returns("3");
			AssertEquals("3", Provider.ForeignTradeStatisticsInlandTransportMode);
		}

		public void TestForeignTradeStatisticsInlandTransportMode_Snapshot()
		{
			isModificationMessage = false;
			snapshot.ForeignTradeStatistics = new DEMonthlyClosingEntryLineSnapshotForeignTradeStatistics { InlandTransportMode = "3" };
			AssertEquals("3", Provider.ForeignTradeStatisticsInlandTransportMode);
		}

		public void TestForeignTradeStatisticsInlandTransportMode_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(string.Empty, Provider.ForeignTradeStatisticsInlandTransportMode);
		}

		public void TestForeignTradeStatisticsAmount()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine.JI_CustomsSecondQuantity = 12344.66m;
			invoiceLine.JI_CustomsSecondUnitQty = "KGM";
			invoiceLine2.JI_CustomsSecondQuantity = 22.11;
			invoiceLine2.JI_CustomsSecondUnitQty = "KGM";

			CombineAssertions(() =>
			{
				AssertEquals(12366.77m, Provider.ForeignTradeStatisticsAmount.Quantity);
				AssertEquals("KGM", Provider.ForeignTradeStatisticsAmount.MeasurementUnit);
				AssertEquals(string.Empty, Provider.ForeignTradeStatisticsAmount.Qualifier);
			});
		}

		public void TestForeignTradeStatisticsAmount0()
		{
			invoiceLine.JI_CustomsSecondQuantity = 0m;
			invoiceLine.JI_CustomsSecondUnitQty = "KGM";

			CombineAssertions(() =>
			{
				AssertNull(Provider.ForeignTradeStatisticsAmount);
			});
		}

		public void TestCustomsValue()
		{
			declaration.ZG_IsHighValueOvrd = false;

			CombineAssertions(() =>
			{
				AssertNull("Null", Provider.CustomsValue);

				var customsValue = Provider.CustomsValue;
				AssertEquals("Cached", customsValue, Provider.CustomsValue);
			});
		}

		public void TestCustomsValue_ConcessionInE01OrE02()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoiceLine.JI_Procedure = "7005E01";

			CombineAssertions(() =>
			{
				AssertNull("Null", Provider.CustomsValue);

				var customsValue = Provider.CustomsValue;
				AssertEquals("Cached", customsValue, Provider.CustomsValue);
			});
		}

		public void TestCustomsValue_ConcessionNotInE01OrE02()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoiceLine.JI_Procedure = "7005F01";

			var customsValue = Provider.CustomsValue;
			AssertEquals("Cached", customsValue, Provider.CustomsValue);
		}

		public void TestBorderTransportMeansMode_Provider()
		{
			isModificationMessage = true;
			Mock.Get(headerProvider).Setup(x => x.BorderTransportMeansMode).Returns("4");
			AssertEquals("4", Provider.BorderTransportMeansMode);
		}

		public void TestBorderTransportMeansMode_Snapshot()
		{
			isModificationMessage = false;
			snapshot.BorderTransportMeans = new DEMonthlyClosingEntryLineSnapshotBorderTransportMeans { Mode = "4" };
			AssertEquals("4", Provider.BorderTransportMeansMode);
		}

		public void TestBorderTransportMeansMode_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(string.Empty, Provider.BorderTransportMeansMode);
		}

		public void TestBorderTransportMeansType_Provider()
		{
			isModificationMessage = true;
			Mock.Get(headerProvider).Setup(x => x.BorderTransportMeansType).Returns("03");
			AssertEquals("03", Provider.BorderTransportMeansType);
		}

		public void TestBorderTransportMeansType_Snapshot()
		{
			isModificationMessage = false;
			snapshot.BorderTransportMeans = new DEMonthlyClosingEntryLineSnapshotBorderTransportMeans { Type = "03" };
			AssertEquals("03", Provider.BorderTransportMeansType);
		}

		public void TestBorderTransportMeansType_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(string.Empty, Provider.BorderTransportMeansType);
		}

		public void TestBorderTransportMeansInformation_Provider()
		{
			isModificationMessage = true;
			Mock.Get(headerProvider).Setup(x => x.BorderTransportMeansInformation).Returns("Information");
			AssertEquals("Information", Provider.BorderTransportMeansInformation);
		}

		public void TestBorderTransportMeansInformation_Snapshot()
		{
			isModificationMessage = false;
			snapshot.BorderTransportMeans = new DEMonthlyClosingEntryLineSnapshotBorderTransportMeans { Information = "Information" };
			AssertEquals("Information", Provider.BorderTransportMeansInformation);
		}

		public void TestBorderTransportMeansInformation_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(string.Empty, Provider.BorderTransportMeansInformation);
		}

		public void TestBorderTransportMeansNationality_Provider()
		{
			isModificationMessage = true;
			Mock.Get(headerProvider).Setup(x => x.BorderTransportMeansNationality).Returns("TR");
			AssertEquals("TR", Provider.BorderTransportMeansNationality);
		}

		public void TestBorderTransportMeansNationality_Snapshot()
		{
			isModificationMessage = false;
			snapshot.BorderTransportMeans = new DEMonthlyClosingEntryLineSnapshotBorderTransportMeans { Nationality = "TR" };
			AssertEquals("TR", Provider.BorderTransportMeansNationality);
		}

		public void TestBorderTransportMeansNationality_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(string.Empty, Provider.BorderTransportMeansNationality);
		}

		public void TestSequenceNumber()
		{
			reconEntryLine.CRL_LineNumber = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestRequestedPreviousProcedure()
		{
			AssertExceptionThrown<NotImplementedException>("property not used here", () => _ = Provider.RequestedPreviousProcedure);
		}

		public void TestGoodsDescription()
		{
			AssertExceptionThrown<NotImplementedException>("property not used here", () => _ = Provider.GoodsDescription);
		}

		public void TestNetMassMeasure_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.NetMassMeasure).Returns(12345.67m);
			AssertEquals(12345.67m, Provider.NetMassMeasure);
		}

		public void TestNetMassMeasure_Snapshot()
		{
			isModificationMessage = false;
			snapshot.NetMassMeasure = 12345.67m;
			AssertEquals(12345.67m, Provider.NetMassMeasure);
		}

		public void TestNetMassMeasure_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(0m, Provider.NetMassMeasure);
		}

		public void TestNetMassMeasureSpecified_Provider()
		{
			isModificationMessage = true;
			AssertEquals(true, Provider.NetMassMeasureSpecified);
		}

		public void TestNetMassMeasureSpecified_Snapshot()
		{
			isModificationMessage = false;
			snapshot.NetMassMeasureSpecified = true;
			AssertEquals(true, Provider.NetMassMeasureSpecified);
		}

		public void TestOriginCountry_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.OriginCountry).Returns("RU");
			AssertEquals("RU", Provider.OriginCountry);
		}

		public void TestOriginCountry_Snapshot()
		{
			isModificationMessage = false;
			snapshot.OriginCountry = "RU";
			AssertEquals("RU", Provider.OriginCountry);
		}

		public void TestOriginCountry_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(null, Provider.OriginCountry);
		}

		public void TestSupplementaryInformation_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.SupplementaryInformation).Returns("supplementary information text");
			AssertEquals("supplementary information text", Provider.SupplementaryInformation);
		}

		public void TestSupplementaryInformation_Snapshot()
		{
			isModificationMessage = false;
			snapshot.SupplementaryInformation = "supplementary information text";
			AssertEquals("supplementary information text", Provider.SupplementaryInformation);
		}

		public void TestSupplementaryInformation_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(null, Provider.SupplementaryInformation);
		}

		public void TestCommodityCode_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.CommodityCode).Returns("87120001099");
			AssertEquals("87120001099", Provider.CommodityCode);
		}

		public void TestCommodityCode_Snapshot()
		{
			isModificationMessage = false;
			snapshot.CommodityCode = "87120001099";
			AssertEquals("87120001099", Provider.CommodityCode);
		}

		public void TestCommodityCode_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(null, Provider.CommodityCode);
		}

		public void TestAdditionalProcedure_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.AdditionalProcedure).Returns(new[] { "04", "05" });
			AssertContainsExactElementsInAnyOrder(new[] { "05", "04" }, Provider.AdditionalProcedure);
		}

		public void TestAdditionalProcedure_Snapshot()
		{
			isModificationMessage = false;
			snapshot.AdditionalProcedure = new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure[]
			{
				new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure { Code = "04" },
				new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure { Code = "05" }
			};
			AssertContainsExactElementsInAnyOrder(new[] { "05", "04" }, Provider.AdditionalProcedure);
		}

		public void TestAdditionalProcedure_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(0, Provider.AdditionalProcedure.Count);
		}

		public void TestSupplementaryCodes_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.SupplementaryCodes).Returns(new[] { "E01", "E02" });
			AssertContainsExactElementsInAnyOrder(new[] { "E02", "E01" }, Provider.SupplementaryCodes);
		}

		public void TestSupplementaryCodes_Snapshot()
		{
			isModificationMessage = false;
			snapshot.SupplementaryCodes = new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes[]
			{
				new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes { Code = "E01" },
				new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes { Code = "E02" }
			};
			AssertContainsExactElementsInAnyOrder(new[] { "E02", "E01" }, Provider.SupplementaryCodes);
		}

		public void TestSupplementaryCodes_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(0, Provider.SupplementaryCodes.Count);
		}

		public void TestPackage()
		{
			AssertExceptionThrown<NotImplementedException>("property not used here", () => _ = Provider.Package);
		}

		public void TestForeignTradeStatisticsQuantity()
		{
			Mock.Get(lineProvider).Setup(x => x.ForeignTradeStatisticsQuantity).Returns(98765m);
			AssertEquals(98765m, Provider.ForeignTradeStatisticsQuantity);
		}

		public void TestForeignTradeStatisticsGrossMassMeasure_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.ForeignTradeStatisticsGrossMassMeasure).Returns(18765.43m);
			AssertEquals(18765.43m, Provider.ForeignTradeStatisticsGrossMassMeasure);
		}

		public void TestForeignTradeStatisticsGrossMassMeasure_Snapshot()
		{
			isModificationMessage = false;
			snapshot.ForeignTradeStatistics = new DEMonthlyClosingEntryLineSnapshotForeignTradeStatistics
			{
				GrossMassMeasure = 18765.43m
			};
			AssertEquals(18765.43m, Provider.ForeignTradeStatisticsGrossMassMeasure);
		}

		public void TestForeignTradeStatisticsGrossMassMeasure_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(0m, Provider.ForeignTradeStatisticsGrossMassMeasure);
		}

		public void TestAssessmentCustomsValue_IsDV1()
		{
			Mock.Get(lineProvider).Setup(x => x.AssessmentCustomsValue).Returns(28765.43m);
			declaration.ZG_IsHighValueOvrd = true;
			AssertEquals(decimal.Zero, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentCustomsValue_IsNotDV1()
		{
			Mock.Get(lineProvider).Setup(x => x.AssessmentCustomsValue).Returns(28765.43m);
			declaration.ZG_IsHighValueOvrd = false;
			AssertEquals(28765.43m, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentAmount_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.AssessmentAmount).Returns(new IAmount[]
			{
				Mock.Of<IAmount>(a => a.Quantity == 123.45m && a.MeasurementUnit == "KGM" && a.Qualifier == "A"),
				Mock.Of<IAmount>(a => a.Quantity == 123.46m && a.MeasurementUnit == "MTR" && a.Qualifier == string.Empty)
			});
			CombineAssertions(() =>
			{
				AssertEquals(123.45m, Provider.AssessmentAmount.First().Quantity);
				AssertEquals("KGM", Provider.AssessmentAmount.First().MeasurementUnit);
				AssertEquals("A", Provider.AssessmentAmount.First().Qualifier);
				AssertEquals(123.46m, Provider.AssessmentAmount.Last().Quantity);
				AssertEquals("MTR", Provider.AssessmentAmount.Last().MeasurementUnit);
				AssertEquals(string.Empty, Provider.AssessmentAmount.Last().Qualifier);
			});
		}

		public void TestAssessmentAmount_Snapshot()
		{
			isModificationMessage = false;
			snapshot.Assessment = new DEMonthlyClosingEntryLineSnapshotAssessment
			{
				Amount = new Amount[]
				{
					new Amount { Quantity = 123.45m, MeasurementUnit = "KGM", Qualifier = "A" },
					new Amount { Quantity = 123.46m, MeasurementUnit = "MTR" }
				}
			};
			CombineAssertions(() =>
			{
				AssertEquals(123.45m, Provider.AssessmentAmount.First().Quantity);
				AssertEquals("KGM", Provider.AssessmentAmount.First().MeasurementUnit);
				AssertEquals("A", Provider.AssessmentAmount.First().Qualifier);
				AssertEquals(123.46m, Provider.AssessmentAmount.Last().Quantity);
				AssertEquals("MTR", Provider.AssessmentAmount.Last().MeasurementUnit);
				AssertEquals(string.Empty, Provider.AssessmentAmount.Last().Qualifier);
			});
		}

		public void TestAssessmentAmount_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(0, Provider.AssessmentAmount.Count);
		}

		public void TestAssessmentSpecificRate_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.AssessmentSpecificRate).Returns(new IImportSpecificRate[]
			{
				Mock.Of<IImportSpecificRate>(r => r.Type == "X" && r.Value == 999.45m),
				Mock.Of<IImportSpecificRate>(r => r.Type == "Y" && r.Value == 123.45m)
			});

			CombineAssertions(() =>
			{
				AssertEquals("X", Provider.AssessmentSpecificRate.First().Type);
				AssertEquals(999.45m, Provider.AssessmentSpecificRate.First().Value);
				AssertEquals("Y", Provider.AssessmentSpecificRate.Last().Type);
				AssertEquals(123.45m, Provider.AssessmentSpecificRate.Last().Value);
			});
		}

		public void TestAssessmentSpecificRate_Snapshot()
		{
			isModificationMessage = false;
			snapshot.Assessment = new DEMonthlyClosingEntryLineSnapshotAssessment
			{
				SpecificRate = new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate[]
				{
					new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate { Type = "X", Value = 999.45m },
					new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate { Type = "Y", Value = 123.45m }
				}
			};
			CombineAssertions(() =>
			{
				AssertEquals("X", Provider.AssessmentSpecificRate.First().Type);
				AssertEquals(999.45m, Provider.AssessmentSpecificRate.First().Value);
				AssertEquals("Y", Provider.AssessmentSpecificRate.Last().Type);
				AssertEquals(123.45m, Provider.AssessmentSpecificRate.Last().Value);
			});
		}

		public void TestAssessmentSpecificRate_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(0, Provider.AssessmentSpecificRate.Count);
		}

		public void TestAssessmentContentInformation_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.AssessmentContentInformation).Returns(new IContentInformation[]
			{
				Mock.Of<IContentInformation>(c => c.ContentType == "A" && c.DegreePercentage == 123.45m),
				Mock.Of<IContentInformation>(c => c.ContentType == "B" && c.DegreePercentage == 999.99m)
			});
			CombineAssertions(() =>
			{
				AssertEquals("A", Provider.AssessmentContentInformation.First().ContentType);
				AssertEquals(123.45m, Provider.AssessmentContentInformation.First().DegreePercentage);
				AssertEquals("B", Provider.AssessmentContentInformation.Last().ContentType);
				AssertEquals(999.99m, Provider.AssessmentContentInformation.Last().DegreePercentage);
			});
		}

		public void TestAssessmentContentInformation_Snapshot()
		{
			isModificationMessage = false;
			snapshot.Assessment = new DEMonthlyClosingEntryLineSnapshotAssessment
			{
				ContentInformation = new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation[]
				{
					new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation { Type = "A", DegreePercentage = 123.45m },
					new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation { Type = "B", DegreePercentage = 999.99m }
				}
			};
			CombineAssertions(() =>
			{
				AssertEquals("A", Provider.AssessmentContentInformation.First().ContentType);
				AssertEquals(123.45m, Provider.AssessmentContentInformation.First().DegreePercentage);
				AssertEquals("B", Provider.AssessmentContentInformation.Last().ContentType);
				AssertEquals(999.99m, Provider.AssessmentContentInformation.Last().DegreePercentage);
			});
		}

		public void TestAssessmentContentInformation_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(0, Provider.AssessmentContentInformation.Count);
		}

		public void TestExciseDuty_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.ExciseDuty).Returns(new IExciseDuty[]
			{
				Mock.Of<IExciseDuty>(c => c.Code == "A" && c.DegreePercentage == 123.45m && c.Value == 88.88m),
				Mock.Of<IExciseDuty>(c => c.Code == "B" && c.Amount == Mock.Of<IAmount>(a => a.Quantity == 111.1m && a.MeasurementUnit == "LTR"))
			});
			CombineAssertions(() =>
			{
				AssertEquals("First.Code", "A", Provider.ExciseDuty.First().Code);
				AssertEquals("First.DegreePercentage", 123.45m, Provider.ExciseDuty.First().DegreePercentage);
				AssertEquals("First.Value", 88.88m, Provider.ExciseDuty.First().Value);
				AssertNull("First.Amount", Provider.ExciseDuty.First().Amount);
				AssertEquals("Last.Code", "B", Provider.ExciseDuty.Last().Code);
				AssertEquals("Last.DegreePercentage", decimal.Zero, Provider.ExciseDuty.Last().DegreePercentage);
				AssertEquals("Last.Value", decimal.Zero, Provider.ExciseDuty.Last().Value);
				AssertEquals("Last.Amount.Quantity", 111.1m, Provider.ExciseDuty.Last().Amount.Quantity);
				AssertEquals("Last.Amount.MeasurementUnit", "LTR", Provider.ExciseDuty.Last().Amount.MeasurementUnit);
			});
		}

		public void TestExciseDuty_Snapshot()
		{
			isModificationMessage = false;
			snapshot.ExciseDuty = new DEMonthlyClosingEntryLineSnapshotExciseDuty[]
			{
				new DEMonthlyClosingEntryLineSnapshotExciseDuty { Code = "A", DegreePercentage = 123.45m, Value = 88.88m },
				new DEMonthlyClosingEntryLineSnapshotExciseDuty { Code = "B", Amount = new Amount {
					Quantity = 111.1m, MeasurementUnit = "LTR" } }
			};
			CombineAssertions(() =>
			{
				AssertEquals("A", Provider.ExciseDuty.First().Code);
				AssertEquals(123.45m, Provider.ExciseDuty.First().DegreePercentage);
				AssertEquals(88.88m, Provider.ExciseDuty.First().Value);
				AssertNull(Provider.ExciseDuty.First().Amount);
				AssertEquals("B", Provider.ExciseDuty.Last().Code);
				AssertEquals(decimal.Zero, Provider.ExciseDuty.Last().DegreePercentage);
				AssertEquals(decimal.Zero, Provider.ExciseDuty.Last().Value);
				AssertEquals(111.1m, Provider.ExciseDuty.Last().Amount.Quantity);
				AssertEquals("LTR", Provider.ExciseDuty.Last().Amount.MeasurementUnit);
			});
		}

		public void TestExciseDuty_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(0, Provider.ExciseDuty.Count);
		}

		public void TestDocuments_Provider()
		{
			isModificationMessage = true;
			Mock.Get(lineProvider).Setup(x => x.Documents).Returns(new IImportLineDocument[]
			{
				Mock.Of<IImportLineDocument>(d => d.DocumentType == "N380" && d.ReferenceNumber == "Reference1" && d.IssuingDate == new DateTime(2022, 1, 20) && d.AtHandFlag == "J"),
				Mock.Of<IImportLineDocument>(d => d.DocumentType == "N990" && d.WriteOff == Mock.Of<IAmount>(a => a.Quantity == 123.45m && a.MeasurementUnit == "MTR")),
			});
			var firstDoc = Provider.Documents.First();
			var lastDoc = Provider.Documents.Last();
			CombineAssertions(() =>
			{
				AssertEquals("firstDoc.DocumentType", "N380", firstDoc.DocumentType);
				AssertEquals("firstDoc.ReferenceNumber", "Reference1", firstDoc.ReferenceNumber);
				AssertEquals("firstDoc.IssuingDate", new DateTime(2022, 1, 20), firstDoc.IssuingDate);
				AssertEquals("firstDoc.AtHandFlag", "J", firstDoc.AtHandFlag);
				AssertNull("firstDoc.WriteOff", firstDoc.WriteOff);

				AssertEquals("lastDoc.DocumentType", "N990", lastDoc.DocumentType);
				AssertNull("lastDoc.ReferenceNumber", lastDoc.ReferenceNumber);
				AssertNull("lastDoc.IssuingDate", lastDoc.IssuingDate);
				AssertNull("lastDoc.AtHandFlag", lastDoc.AtHandFlag);
				AssertEquals("lastDoc.WriteOff.Quantity", 123.45m, lastDoc.WriteOff.Quantity);
				AssertEquals("lastDoc.WriteOff.MeasurementUnit", "MTR", lastDoc.WriteOff.MeasurementUnit);
			});
		}

		public void TestDocuments_Snapshot()
		{
			isModificationMessage = false;
			snapshot.Document = new DEMonthlyClosingEntryLineSnapshotDocument[]
			{
				new DEMonthlyClosingEntryLineSnapshotDocument { Division = DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item1, Type = "N380", ReferenceNumber = "Reference1", IssuingDate = new DateTime(2022, 1, 20), IssuingDateSpecified = true, AtHandFlag = "J" },
				new DEMonthlyClosingEntryLineSnapshotDocument { Division = DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item2, Type = "N990", IssuingDateSpecified = false, WriteOff = new Amount { Quantity = 123.45m, MeasurementUnit = "MTR" } }
			};
			var firstDoc = Provider.Documents.First();
			var lastDoc = Provider.Documents.Last();
			CombineAssertions(() =>
			{
				AssertEquals("firstDoc.Division", "1", firstDoc.Division);
				AssertEquals("firstDoc.DocumentType", "N380", firstDoc.DocumentType);
				AssertEquals("firstDoc.ReferenceNumber", "Reference1", firstDoc.ReferenceNumber);
				AssertEquals("firstDoc.IssuingDate", new DateTime(2022, 1, 20), firstDoc.IssuingDate);
				AssertEquals("firstDoc.AtHandFlag", "J", firstDoc.AtHandFlag);
				AssertNull("firstDoc.WriteOff", firstDoc.WriteOff);

				AssertEquals("lastDoc.Division", "2", lastDoc.Division);
				AssertEquals("lastDoc.DocumentType", "N990", lastDoc.DocumentType);
				AssertNull("lastDoc.ReferenceNumber", lastDoc.ReferenceNumber);
				AssertNull("lastDoc.IssuingDate", lastDoc.IssuingDate);
				AssertNull("lastDoc.AtHandFlag", lastDoc.AtHandFlag);
				AssertEquals("lastDoc.WriteOff.Quantity", 123.45m, lastDoc.WriteOff.Quantity);
				AssertEquals("lastDoc.WriteOff.MeasurementUnit", "MTR", lastDoc.WriteOff.MeasurementUnit);
			});
		}

		public void TestDocuments_Snapshot_Null()
		{
			isModificationMessage = false;
			AssertEquals(0, Provider.Documents.Count);
		}

		protected override MonthlyClosingDecLineProvider GetProvider() => new MonthlyClosingDecLineProviderForTest(reconEntryLine, isModificationMessage, headerProvider, lineProvider, snapshot);

		sealed class MonthlyClosingDecLineProviderForTest : MonthlyClosingDecLineProvider
		{
			public MonthlyClosingDecLineProviderForTest(CusReconEntryLine reconEntryLine, bool isModificationMessage, IImportDecHeader headerProvider, IImportDecLine lineProvider, DEMonthlyClosingEntryLineSnapshot snapshot) : base(reconEntryLine, isModificationMessage)
			{
				this.snapshot = snapshot;
				this.lineProvider = lineProvider;
				this.headerProvider = headerProvider;
			}

			protected override IImportDecHeader GetHeaderProvider(CusEntryHeader entryHeader) => headerProvider;

			protected override IImportDecLine GetLineProvider(CusEntryLine entryLine) => lineProvider;

			protected override DEMonthlyClosingEntryLineSnapshot LoadCurrentSnapshot() => snapshot;

			readonly IImportDecHeader headerProvider;
			readonly IImportDecLine lineProvider;
			readonly DEMonthlyClosingEntryLineSnapshot snapshot;
		}
	}
}
