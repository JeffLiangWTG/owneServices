using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.MonthlyClosing.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class IMonthlyClosingDecLineEqualityComparerTest : TestCase
	{
		public void TestGetHashCode() => AssertEquals(0, monthlyClosingDecLineEqualityComparer.GetHashCode(originalMonthlyClosingDecLine));

		public void TestEquals() => Assert(monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));

		public void TestEquals_SequenceNumber()
		{
			monthlyClosingDecLineMock.Setup(x => x.SequenceNumber).Returns(3);
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_ReferencedSequenceNumber()
		{
			monthlyClosingDecLineMock.Setup(x => x.ReferencedSequenceNumber).Returns(3);
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_MatterCode()
		{
			monthlyClosingDecLineMock.Setup(x => x.MatterCode).Returns("CM");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_ArticleNumber()
		{
			monthlyClosingDecLineMock.Setup(x => x.ArticleNumber).Returns("ARTICLE321");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_InvoiceAmount()
		{
			monthlyClosingDecLineMock.Setup(x => x.InvoiceAmount).Returns(3.21m);
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_DepartureCountry()
		{
			monthlyClosingDecLineMock.Setup(x => x.DepartureCountry).Returns("CH");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_CompleteDeclarationFlag()
		{
			monthlyClosingDecLineMock.Setup(x => x.CompleteDeclarationFlag).Returns(false);
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_ForeignTradeStatisticsGoodsStatus()
		{
			monthlyClosingDecLineMock.Setup(x => x.ForeignTradeStatisticsGoodsStatus).Returns("SG");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_ForeignTradeStatisticsTransactionType()
		{
			monthlyClosingDecLineMock.Setup(x => x.ForeignTradeStatisticsTransactionType).Returns("UU");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_ForeignTradeStatisticsDestinationCountry()
		{
			monthlyClosingDecLineMock.Setup(x => x.ForeignTradeStatisticsTransactionType).Returns("AU");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_ForeignTradeStatisticsDestinationFederalState()
		{
			monthlyClosingDecLineMock.Setup(x => x.ForeignTradeStatisticsDestinationFederalState).Returns("NRW");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_ForeignTradeStatisticsInlandTransportMode()
		{
			monthlyClosingDecLineMock.Setup(x => x.ForeignTradeStatisticsInlandTransportMode).Returns("SEA");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_ForeignTradeStatisticsAmount()
		{
			var amountMock = IAmountEqualityComparerTest.AmountMock;
			amountMock.Setup(x => x.Qualifier).Returns("A");
			monthlyClosingDecLineMock.Setup(x => x.ForeignTradeStatisticsAmount).Returns(amountMock.Object);
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_CustomsValue()
		{
			var importLineCustomsValueMock = CusReconBuildersTestHelper.ImportLineCustomsValueMock;
			importLineCustomsValueMock.Setup(x => x.CustomsValueDepartureAirport).Returns("BER");
			monthlyClosingDecLineMock.Setup(x => x.CustomsValue).Returns(importLineCustomsValueMock.Object);
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_BorderTransportMeansMode()
		{
			monthlyClosingDecLineMock.Setup(x => x.BorderTransportMeansMode).Returns("AIR");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_BorderTransportMeansType()
		{
			monthlyClosingDecLineMock.Setup(x => x.BorderTransportMeansType).Returns("TM");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_BorderTransportMeansInformation()
		{
			monthlyClosingDecLineMock.Setup(x => x.BorderTransportMeansInformation).Returns("GermanTruck");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_BorderTransportMeansNationality()
		{
			monthlyClosingDecLineMock.Setup(x => x.BorderTransportMeansNationality).Returns("AT");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_NetMassMeasure()
		{
			monthlyClosingDecLineMock.Setup(x => x.NetMassMeasure).Returns(1.2m);
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_OriginCountry()
		{
			monthlyClosingDecLineMock.Setup(x => x.OriginCountry).Returns("AT");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_SupplementaryInformation()
		{
			monthlyClosingDecLineMock.Setup(x => x.SupplementaryInformation).Returns("Modified");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_CommodityCode()
		{
			monthlyClosingDecLineMock.Setup(x => x.CommodityCode).Returns("Modified");
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_AdditionalProcedure()
		{
			monthlyClosingDecLineMock.Setup(x => x.AdditionalProcedure).Returns(new[] { "C11", "D35" });
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_SupplementaryCodes()
		{
			monthlyClosingDecLineMock.Setup(x => x.SupplementaryCodes).Returns(new[] { "A10", "A13" });
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_ForeignTradeStatisticsQuantity()
		{
			monthlyClosingDecLineMock.Setup(x => x.ForeignTradeStatisticsQuantity).Returns(1m);
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_ForeignTradeStatisticsGrossMassMeasure()
		{
			monthlyClosingDecLineMock.Setup(x => x.ForeignTradeStatisticsGrossMassMeasure).Returns(1m);
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_AssessmentCustomsValue()
		{
			monthlyClosingDecLineMock.Setup(x => x.AssessmentCustomsValue).Returns(1m);
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_AssessmentAmount()
		{
			monthlyClosingDecLineMock.Setup(x => x.AssessmentAmount).Returns(new[] { CusReconBuildersTestHelper.GetAmount("X", 18219, "NAR"), CusReconBuildersTestHelper.GetAmount("Y", 18219, "NAR") });
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_AssessmentSpecificRate()
		{
			monthlyClosingDecLineMock.Setup(x => x.AssessmentSpecificRate).Returns(new[] { CusReconBuildersTestHelper.GetImportSpecificRate("S", 10.02m), CusReconBuildersTestHelper.GetImportSpecificRate("T", 10.02m)	});
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_AssessmentContentInformation()
		{
			monthlyClosingDecLineMock.Setup(x => x.AssessmentContentInformation).Returns(new[] { CusReconBuildersTestHelper.GetContentInformation("L", 0.01m), CusReconBuildersTestHelper.GetContentInformation("K", 0.01m) });
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_ExciseDuty()
		{
			monthlyClosingDecLineMock.Setup(x => x.ExciseDuty).Returns(new[] { CusReconBuildersTestHelper.GetExciseDuty("A123", 0.02m, 1626.28m), CusReconBuildersTestHelper.GetExciseDuty("A456", 0.02m, 1626.28m) });
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		public void TestEquals_Documents()
		{
			monthlyClosingDecLineMock.Setup(x => x.Documents).Returns(new[] { CusReconBuildersTestHelper.GetImportLineDocument("4", "7HHF", "COSU6271657530", new DateTime(2021, 8, 12), "J"), CusReconBuildersTestHelper.GetImportLineDocument("5", "7HHF", "COSU6271657530", new DateTime(2021, 8, 12), "J") });
			AssertEquals(expected: false, monthlyClosingDecLineEqualityComparer.Equals(originalMonthlyClosingDecLine, monthlyClosingDecLineMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			monthlyClosingDecLineMock = CusReconBuildersTestHelper.GetMonthlyClosingDecLineMock<IMonthlyClosingDecLine>();
			monthlyClosingDecLineEqualityComparer = new IMonthlyClosingDecLineEqualityComparer();
		}
		IMonthlyClosingDecLineEqualityComparer monthlyClosingDecLineEqualityComparer;
		Mock<IMonthlyClosingDecLine> monthlyClosingDecLineMock;
		IMonthlyClosingDecLine originalMonthlyClosingDecLine => CusReconBuildersTestHelper.GetMonthlyClosingDecLineMock<IMonthlyClosingDecLine>().Object;
	}
}
