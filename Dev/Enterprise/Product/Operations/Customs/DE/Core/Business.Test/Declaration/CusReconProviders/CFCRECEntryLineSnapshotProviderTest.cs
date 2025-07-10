using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class CFCRECEntryLineSnapshotProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("Parameter line is null", () => new CFCRECEntryLineSnapshotProvider(null, header.Object));
				AssertExceptionThrown<ArgumentException>("Parameter header is null", () => new CFCRECEntryLineSnapshotProvider(line.Object, null));
			});
		}

		public void TestCessionManagementFlag()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.CessionManagementFlag).Returns("TestCessionManagementFlag");
			AssertEquals("TestCessionManagementFlag", dataProvider.CessionManagementFlag);
		}

		public void TestBorderTransportMeansInformation()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.BorderTransportMeansInformation).Returns("TestBorderTransportMeansInformation");
			AssertEquals("TestBorderTransportMeansInformation", dataProvider.BorderTransportMeansInformation);
		}

		public void TestTobaccoRevenueStampNumber()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.TobaccoRevenueStampNumber).Returns("TestTobaccoRevenueStampNumber");
			AssertEquals("TestTobaccoRevenueStampNumber", dataProvider.TobaccoRevenueStampNumber);
		}

		public void TestPreferentialTreatment()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			var tempObj = new Mock<ILinePreferentialTreatment>();
			line.Setup(l => l.PreferentialTreatment).Returns(tempObj.Object);
			AssertEquals(tempObj.Object, dataProvider.PreferentialTreatment);
		}

		public void TestForeignTradeStatisticsInlandTransportMode()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.ForeignTradeStatisticsInlandTransportMode).Returns("TestForeignTradeStatisticsInlandTransportMode");
			AssertEquals("TestForeignTradeStatisticsInlandTransportMode", dataProvider.ForeignTradeStatisticsInlandTransportMode);
		}

		public void TestBorderTransportMeansMode()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.BorderTransportMeansMode).Returns("TestBorderTransportMeansMode");
			AssertEquals("TestBorderTransportMeansMode", dataProvider.BorderTransportMeansMode);
		}

		public void TestBorderTransportMeansType()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.BorderTransportMeansType).Returns("TestBorderTransportMeansType");
			AssertEquals("TestBorderTransportMeansType", dataProvider.BorderTransportMeansType);
		}

		public void TestBorderTransportMeansNationality()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.BorderTransportMeansNationality).Returns("TestBorderTransportMeansNationality");
			AssertEquals("TestBorderTransportMeansNationality", dataProvider.BorderTransportMeansNationality);
		}

		public void TestSequenceNumber()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.SequenceNumber).Returns(1);
			AssertEquals(1, dataProvider.SequenceNumber);
		}

		public void TestNetMassMeasure()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.NetMassMeasure).Returns(1.0M);
			AssertEquals(1.0M, dataProvider.NetMassMeasure);
		}

		public void TestNetMassMeasureSpecified()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertEquals(true, dataProvider.NetMassMeasureSpecified);
		}

		public void TestOriginCountry()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.OriginCountry).Returns("TestOriginCountry");
			AssertEquals("TestOriginCountry", dataProvider.OriginCountry);
		}

		public void TestSupplementaryInformation()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.SupplementaryInformation).Returns("TestSupplementaryInformation");
			AssertEquals("TestSupplementaryInformation", dataProvider.SupplementaryInformation);
		}

		public void TestCommodityCode()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.CommodityCode).Returns("TestCommodityCode");
			AssertEquals("TestCommodityCode", dataProvider.CommodityCode);
		}

		public void TestAdditionalProcedure()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.AdditionalProcedure).Returns(new[] { "str1", "str2" });
			AssertEquals(2, dataProvider.AdditionalProcedure.Count);
		}

		public void TestSupplementaryCodes()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.SupplementaryCodes).Returns(new[] { "str1", "str2" });
			AssertEquals(2, dataProvider.SupplementaryCodes.Count);
		}

		public void TestForeignTradeStatisticsQuantity()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertEquals(decimal.Zero, dataProvider.ForeignTradeStatisticsQuantity);
		}

		public void TestForeignTradeStatisticsGrossMassMeasure()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.ForeignTradeStatisticsGrossMassMeasure).Returns(1.0M);
			AssertEquals(1.0M, dataProvider.ForeignTradeStatisticsGrossMassMeasure);
		}

		public void TestAssessmentCustomsValue()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.AssessmentCustomsValue).Returns(1.0M);
			AssertEquals(1.0M, dataProvider.AssessmentCustomsValue);
		}

		public void TestAssessmentAmount()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.AssessmentAmount).Returns(new[]
			{
				new Mock<IAmount>().Object,
				new Mock<IAmount>().Object
			});
			AssertEquals(2, dataProvider.AssessmentAmount.Count);
		}

		public void TestAssessmentSpecificRate()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.AssessmentSpecificRate).Returns(new[]
			{
				new Mock<IImportSpecificRate>().Object,
				new Mock<IImportSpecificRate>().Object
			});
			AssertEquals(2, dataProvider.AssessmentSpecificRate.Count);
		}

		public void TestAssessmentContentInformation()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.AssessmentContentInformation).Returns(new[]
			{
				new Mock<IContentInformation>().Object,
				new Mock<IContentInformation>().Object
			});
			AssertEquals(2, dataProvider.AssessmentContentInformation.Count);
		}

		public void TestExciseDuty()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.ExciseDuty).Returns(new[]
			{
				new Mock<IExciseDuty>().Object,
				new Mock<IExciseDuty>().Object
			});
			AssertEquals(2, dataProvider.ExciseDuty.Count);
		}

		public void TestDocuments()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.Documents).Returns(new[]
			{
				new Mock<IImportLineDocument>().Object,
				new Mock<IImportLineDocument>().Object
			});
			AssertEquals(2, dataProvider.Documents.Count);
		}

		public void TestInwardMovementAmount()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.InwardMovementAmount);
		}

		public void TestReferencedSequenceNumber()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.ReferencedSequenceNumber);
		}

		public void TestMatterCode()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.MatterCode);
		}

		public void TestArticleNumber()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.ArticleNumber);
		}

		public void TestInvoiceAmount()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.InvoiceAmount);
		}

		public void TestPreferentialCountry()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.PreferentialOriginCountry).Returns("AU");
			AssertEquals("AU", dataProvider.PreferentialCountry);
		}

		public void TestDepartureCountry()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.DepartureCountry).Returns("BE");
			AssertEquals("BE", dataProvider.DepartureCountry);
		}

		public void TestForeignTradeFlag()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.ForeignTradeFlag);
		}

		public void TestCompleteDeclarationFlag()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.CompleteDeclarationFlag);
		}

		public void TestForeignTradeStatisticsGoodsStatus()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.ForeignTradeStatisticsGoodsStatus);
		}

		public void TestForeignTradeStatisticsTransactionType()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.ForeignTradeStatisticsTransactionType);
		}

		public void TestForeignTradeStatisticsDestinationCountry()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.ForeignTradeStatisticsDestinationCountry);
		}

		public void TestForeignTradeStatisticsDestinationFederalState()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.ForeignTradeStatisticsDestinationFederalState);
		}

		public void TestForeignTradeStatisticsAmount()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.ForeignTradeStatisticsAmount);
		}

		public void TestCustomsValue()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.CustomsValue);
		}

		public void TestRequestedPreviousProcedure()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertEquals(ZString.Empty, dataProvider.RequestedPreviousProcedure);
		}

		public void TestGoodsDescription()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertEquals(ZString.Empty, dataProvider.GoodsDescription);
		}

		public void TestPackage()
		{
			var line = new Mock<ICFCRECLine>();
			var header = new Mock<ICFCRECHeader>();
			var dataProvider = new CFCRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(dataProvider.Package);
		}
	}
}
