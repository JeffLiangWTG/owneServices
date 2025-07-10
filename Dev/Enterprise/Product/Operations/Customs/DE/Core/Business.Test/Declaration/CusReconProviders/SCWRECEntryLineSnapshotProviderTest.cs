using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class SCWRECEntryLineSnapshotProviderTest : TestCaseWithFactory
	{
		public void TestCessionManagementFlag()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.CessionManagementFlag);
		}

		public void TestTobaccoRevenueStampNumber()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.TobaccoRevenueStampNumber);
		}

		public void TestRequestedPreferentialTreatment()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.RequestedPreferentialTreatment).Returns("TestPreferentialTreatment");
			AssertEquals("TestPreferentialTreatment", provider.PreferentialTreatment.RequestedPreferentialTreatment);
		}

		public void TestInwardMovementAmount()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			var amount = new Mock<IAmount>();
			amount.Setup(a => a.Quantity).Returns(1M);
			amount.Setup(a => a.MeasurementUnit).Returns("test");
			amount.Setup(a => a.Qualifier).Returns("test");
			line.Setup(l => l.InwardMovementAmount).Returns(amount.Object);
			AssertEquals(amount.Object, provider.InwardMovementAmount);
		}

		public void TestReferencedSequenceNumber()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.ReferencedSequenceNumber);
		}

		public void TestMatterCode()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.MatterCode);
		}

		public void TestArticleNumber()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.ArticleNumber);
		}

		public void TestInvoiceAmount()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.InvoiceAmount);
		}

		public void TestPreferentialCountry()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.PreferentialCountry);
		}

		public void TestDepartureCountry()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.DepartureCountry).Returns("BE");
			AssertEquals("BE", provider.DepartureCountry);
		}

		public void TestForeignTradeFlag()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.ForeignTradeImportEarlyClearanceFlag).Returns("J");
			AssertEquals("J", provider.ForeignTradeFlag);
		}

		public void TestCompleteDeclarationFlag()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.CompleteDeclarationFlag);
		}

		public void TestForeignTradeStatisticsGoodsStatus()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.ForeignTradeStatisticsGoodsStatus);
		}

		public void TestForeignTradeStatisticsTransactionType()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.ForeignTradeStatisticsTransactionType);
		}

		public void TestForeignTradeStatisticsDestinationCountry()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.ForeignTradeStatisticsDestinationCountry);
		}

		public void TestForeignTradeStatisticsDestinationFederalState()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.ForeignTradeStatisticsDestinationFederalState);
		}

		public void TestForeignTradeStatisticsInlandTransportMode()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.ForeignTradeStatisticsInlandTransportMode).Returns("TestForeignTradeStatisticsInlandTransportMode");
			AssertEquals("TestForeignTradeStatisticsInlandTransportMode", provider.ForeignTradeStatisticsInlandTransportMode);
		}

		public void TestForeignTradeStatisticsAmount()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.ForeignTradeStatisticsAmount);
		}

		public void TestCustomsValue()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.CustomsValue);
		}

		public void TestBorderTransportMeansMode()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.BorderTransportMeansMode).Returns("TestBorderTransportMeansMode");
			AssertEquals("TestBorderTransportMeansMode", provider.BorderTransportMeansMode);
		}

		public void TestBorderTransportMeansType()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.BorderTransportMeansType).Returns("TestBorderTransportMeansType");
			AssertEquals("TestBorderTransportMeansType", provider.BorderTransportMeansType);
		}

		public void TestBorderTransportMeansInformation()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.BorderTransportMeansInformation).Returns("TestBorderTransportMeansInformation");
			AssertEquals("TestBorderTransportMeansInformation", provider.BorderTransportMeansInformation);
		}

		public void TestBorderTransportMeansNationality()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			header.Setup(h => h.BorderTransportMeansNationality).Returns("TestBorderTransportMeansNationality");
			AssertEquals("TestBorderTransportMeansNationality", provider.BorderTransportMeansNationality);
		}

		public void TestSequenceNumber()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.SequenceNumber).Returns(1);
			AssertEquals(1, provider.SequenceNumber);
		}

		public void TestRequestedPreviousProcedure()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertEquals(ZString.Empty, provider.RequestedPreviousProcedure);
		}

		public void TestGoodsDescription()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertEquals(ZString.Empty, provider.GoodsDescription);
		}

		public void TestNetMassMeasure()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.NetMassMeasure).Returns(1.0M);
			AssertEquals(1.0M, provider.NetMassMeasure);
		}

		public void TestNetMassMeasureSpecified()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertEquals(true, provider.NetMassMeasureSpecified);
		}

		public void TestOriginCountry()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.OriginCountry).Returns("TestOriginCountry");
			AssertEquals("TestOriginCountry", provider.OriginCountry);
		}

		public void TestSupplementaryInformation()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.SupplementaryInformation).Returns("TestSupplementaryInformation");
			AssertEquals("TestSupplementaryInformation", provider.SupplementaryInformation);
		}

		public void TestCommodityCode()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.CommodityCode).Returns("TestCommodityCode");
			AssertEquals("TestCommodityCode", provider.CommodityCode);
		}

		public void TestAdditionalProcedure()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.AdditionalProcedure).Returns(new[] { "str1", "str2" });
			AssertEquals(2, provider.AdditionalProcedure.Count);
		}

		public void TestSupplementaryCodes()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.SupplementaryCodes).Returns(new[] { "str1", "str2" });
			AssertEquals(2, provider.SupplementaryCodes.Count);
		}

		public void TestPackage()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertNull(provider.Package);
		}

		public void TestForeignTradeStatisticsQuantity()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			AssertEquals(decimal.Zero, provider.ForeignTradeStatisticsQuantity);
		}

		public void TestForeignTradeStatisticsGrossMassMeasure()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.ForeignTradeStatisticsGrossMassMeasure).Returns(1.0M);
			AssertEquals(1.0M, provider.ForeignTradeStatisticsGrossMassMeasure);
		}

		public void TestAssessmentCustomsValue()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.AssessmentCustomsValue).Returns(1.0M);
			AssertEquals(1.0M, provider.AssessmentCustomsValue);
		}

		public void TestAssessmentAmount()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.AssessmentAmount).Returns(new IAmount[] { new Mock<IAmount>().Object, new Mock<IAmount>().Object });
			AssertEquals(2, provider.AssessmentAmount.Count);
		}

		public void TestAssessmentSpecificRate()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { new Mock<IImportSpecificRate>().Object, new Mock<IImportSpecificRate>().Object });
			AssertEquals(2, provider.AssessmentSpecificRate.Count);
		}

		public void TestAssessmentContentInformation()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.AssessmentContentInformation).Returns(new IContentInformation[] { new Mock<IContentInformation>().Object, new Mock<IContentInformation>().Object });
			AssertEquals(2, provider.AssessmentContentInformation.Count);
		}

		public void TestExciseDuty()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.ExciseDuty).Returns(new IExciseDuty[] { new Mock<IExciseDuty>().Object, new Mock<IExciseDuty>().Object });
			AssertEquals(2, provider.ExciseDuty.Count);
		}

		public void TestDocuments()
		{
			var line = new Mock<ISCWRECLine>();
			var header = new Mock<ISCWRECHeader>();
			var provider = new SCWRECEntryLineSnapshotProvider(line.Object, header.Object);
			line.Setup(l => l.Documents).Returns(new IImportLineDocument[] { new Mock<IImportLineDocument>().Object, new Mock<IImportLineDocument>().Object });
			AssertEquals(2, provider.Documents.Count);
		}
	}
}
