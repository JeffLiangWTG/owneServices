using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	sealed class CusReconEntryLineSnapshotBuilderTest : TestCaseWithFactory
	{
		public void TestNetMassMeasure_Format()
		{
			providerMock.Setup(m => m.NetMassMeasure).Returns(1.15M);
			AssertEquals(1.2M, snapshotBuilder.GenerateMessage().NetMassMeasure);
		}

		public void TestNetMassMeasure_Specified()
		{
			providerMock.Setup(m => m.NetMassMeasure).Returns(decimal.Zero);
			AssertEquals(false, snapshotBuilder.GenerateMessage().NetMassMeasureSpecified);
		}

		public void TestForeignTradeStatistics_Specified()
		{
			providerMock.Setup(m => m.ForeignTradeStatisticsGrossMassMeasure).Returns(decimal.Zero);
			var message = snapshotBuilder.GenerateMessage();
			AssertEquals(false, message.ForeignTradeStatistics.GrossMassMeasureSpecified);
		}

		public void TestForeignTradeStatistics_Format()
		{
			providerMock.Setup(m => m.ForeignTradeStatisticsGrossMassMeasure).Returns(1.15M);
			var message = snapshotBuilder.GenerateMessage();
			AssertEquals("GrossMassMeasure", 1.2M, message.ForeignTradeStatistics.GrossMassMeasure);
		}

		public void TestForeignTradeFlag()
		{
			var message = snapshotBuilder.GenerateMessage();
			AssertEquals("X", message.ForeignTradeFlag);
		}

		public void TestAssessment_Format()
		{
			providerMock.Setup(m => m.AssessmentCustomsValue).Returns(1.115M);
			providerMock.Setup(m => m.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { CusReconBuildersTestHelper.GetImportSpecificRate("D", 4.445M) });
			providerMock.Setup(m => m.AssessmentContentInformation).Returns(new IContentInformation[] { CusReconBuildersTestHelper.GetContentInformation("F", 5.555M) });
			var assessment = snapshotBuilder.GenerateMessage().Assessment;
			CombineAssertions(() =>
			{
				AssertEquals("CustomsValue", 1.12M, assessment.CustomsValue);
				AssertEquals("SpecificRate.Value", 4.45M, assessment.SpecificRate.Single().Value);
				AssertEquals("ContentInformation.DegreePercentage", 5.56M, assessment.ContentInformation.Single().DegreePercentage);
			});
		}

		public void TestAssessment_Specified()
		{
			providerMock.Setup(m => m.AssessmentCustomsValue).Returns(decimal.Zero);
			var assessment = snapshotBuilder.GenerateMessage().Assessment;
			CombineAssertions(() =>
			{
				AssertEquals("CustomsValueSpecified", false, assessment.CustomsValueSpecified);
			});
		}

		public void TestExciseDuty_Format()
		{
			providerMock.Setup(m => m.ExciseDuty).Returns(new IExciseDuty[] { CusReconBuildersTestHelper.GetExciseDuty("A123", 1.115M, 2.225M) });
			var exciseDuty = snapshotBuilder.GenerateMessage().ExciseDuty.Single();
			CombineAssertions(() =>
			{
				AssertEquals("DegreePercentage", 1.12M, exciseDuty.DegreePercentage);
				AssertEquals("Value", 2.23M, exciseDuty.Value);
			});
		}

		public void TestExciseDuty_Specified()
		{
			providerMock.Setup(m => m.ExciseDuty).Returns(new IExciseDuty[] { CusReconBuildersTestHelper.GetExciseDuty("A123", decimal.Zero, decimal.Zero) });
			var exciseDuty = snapshotBuilder.GenerateMessage().ExciseDuty.Single();
			CombineAssertions(() =>
			{
				AssertEquals("DegreePercentageSpecified", false, exciseDuty.DegreePercentageSpecified);
				AssertEquals("ValueSpecified", false, exciseDuty.ValueSpecified);
			});
		}

		public void TestPreferentialTreatment_Null()
		{
			providerMock.Setup(m => m.PreferentialTreatment).Returns((ILinePreferentialTreatment)null);
			AssertNull(snapshotBuilder.GenerateMessage().PreferentialTreatment);
		}

		public void TestPreferentialTreatment_QuantityIsNull()
		{
			providerMock.Setup(m => m.PreferentialTreatment).Returns(CusReconBuildersTestHelper.GetLinePreferentialTreatment(null));
			AssertNull(snapshotBuilder.GenerateMessage().PreferentialTreatment.Declaration.PreferentialTreatmentQuantity);
		}

		public void TestPreferentialTreatment_Format()
		{
			providerMock.Setup(m => m.PreferentialTreatment).Returns(CusReconBuildersTestHelper.GetPreferentialTreatment(CusReconBuildersTestHelper.GetAmount("X", 1.1348M, "NAR")));
			AssertEquals(1.135M, snapshotBuilder.GenerateMessage().PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.Quantity);
		}

		public void TestDocument_Specified()
		{
			providerMock.Setup(m => m.Documents).Returns(new IImportLineDocument[] { CusReconBuildersTestHelper.GetImportLineDocument("1", "7HHF", "COSU6271657530", null, "J") });
			var document = snapshotBuilder.GenerateMessage().Document.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Division", DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item1, document.Division);
				AssertEquals("IssuingDateSpecified", false, document.IssuingDateSpecified);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 01, 20, 14, 20, 20)]
		public void TestCompleteSnapshot()
		{
			AssertASCIIFileSameAsString(CusReconBuildersTestHelper.TestFilesDirectory + @"\EntryLineSnapshot.xml", snapshotBuilder.GetXMLMessage());
		}

		[TestDate(2022, 01, 20, 14, 20, 20)]
		public void TestLastUpdateTimeUtc()
		{
			CombineAssertions(() =>
			{
				var snapshot = snapshotBuilder.GenerateMessage();
				AssertEquals("LastUpdateTimeUtc", new DateTime(2022, 01, 20, 14, 20, 20), snapshot.LastUpdateTimeUtc);
				AssertEquals("LastUpdateTimeUtcSpecified", true, snapshot.LastUpdateTimeUtcSpecified);
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
			providerMock.Setup(p => p.ForeignTradeFlag).Returns("X");
			providerMock.Setup(p => p.AssessmentCustomsValue).Returns(6.66M);
			providerMock.Setup(p => p.AssessmentAmount).Returns(new IAmount[] { CusReconBuildersTestHelper.GetAmount("B", 9.999M, "TNE"), CusReconBuildersTestHelper.GetAmount("C", 10.111M, "ABC") });
			providerMock.Setup(p => p.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { CusReconBuildersTestHelper.GetImportSpecificRate("D", 11.22M), CusReconBuildersTestHelper.GetImportSpecificRate("E", 11.33M) });
			providerMock.Setup(p => p.AssessmentContentInformation).Returns(new IContentInformation[] { CusReconBuildersTestHelper.GetContentInformation("F", 11.44M), CusReconBuildersTestHelper.GetContentInformation("G", 11.55M) });
			providerMock.Setup(p => p.ExciseDuty).Returns(new IExciseDuty[] { CusReconBuildersTestHelper.GetExciseDuty("A123", 0.02M, 1626.28M) });
			providerMock.Setup(p => p.PreferentialTreatment).Returns(CusReconBuildersTestHelper.GetPreferentialTreatment(CusReconBuildersTestHelper.GetAmount("X", 11, "NAR")));
			providerMock.Setup(p => p.Documents).Returns(new IImportLineDocument[] { CusReconBuildersTestHelper.GetImportLineDocument("3", "7HHF", "COSU6271657530", new DateTime(2020, 08, 12), "J") });
			providerMock.Setup(p => p.InwardMovementAmount).Returns(CusReconBuildersTestHelper.GetAmount("D", 13.333M, "KGM"));
			providerMock.Setup(p => p.BorderTransportMeansMode).Returns("M");
			providerMock.Setup(p => p.BorderTransportMeansType).Returns("T1");
			providerMock.Setup(p => p.BorderTransportMeansInformation).Returns("BTMI0001");
			providerMock.Setup(m => m.BorderTransportMeansNationality).Returns("US");
			snapshotBuilder = new CusReconEntryLineSnapshotBuilder(providerMock.Object);
		}
		Mock<IMonthlyClosingEntryLineSnapshot> providerMock;
		CusReconEntryLineSnapshotBuilder snapshotBuilder;
	}
}
