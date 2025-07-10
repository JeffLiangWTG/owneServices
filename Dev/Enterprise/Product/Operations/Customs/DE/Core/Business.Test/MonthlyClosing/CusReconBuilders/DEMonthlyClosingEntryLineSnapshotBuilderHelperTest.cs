using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	sealed class DEMonthlyClosingEntryLineSnapshotBuilderHelperTest : TestCase
	{
		public void TestGetNetMassMeasure()
		{
			AssertEquals(2.2M, iMonthlyClosingEntryLineSnapshotObject.GetNetMassMeasure());
		}

		public void TestGetForeignTradeStatisticsGrossMassMeasure()
		{
			AssertEquals(4.3M, iMonthlyClosingEntryLineSnapshotObject.GetForeignTradeStatisticsGrossMassMeasure());
		}

		public void TestGetAssessmentCustomsValue()
		{
			AssertEquals(6.66M, iMonthlyClosingEntryLineSnapshotObject.GetAssessmentCustomsValue());
		}

		public void TestGetImportSpecificRateValue()
		{
			AssertEquals(2.11M, importSpecificRateObject.GetImportSpecificRateValue());
		}

		public void TestGetContentInformationDegreePercentage()
		{
			AssertEquals(2.23M, iContentInformationObject.GetContentInformationDegreePercentage());
		}

		public void TestGetExciseDutyDegreePercentage()
		{
			AssertEquals(2.28M, iExciseDutyObject.GetExciseDutyDegreePercentage());
		}

		public void TestGetExciseDutyValue()
		{
			AssertEquals(3.31M, iExciseDutyObject.GetExciseDutyValue());
		}

		public void TestGetImportLineDocumentIssuingDate()
		{
			AssertEquals(new DateTime(2021, 12, 29), iImportLineDocument.GetImportLineDocumentIssuingDate());
		}

		protected override void SetUp()
		{
			base.SetUp();

			var mock = new Mock<IMonthlyClosingEntryLineSnapshot>();
			mock.Setup(p => p.NetMassMeasure).Returns(2.2222M);
			mock.Setup(p => p.ForeignTradeStatisticsGrossMassMeasure).Returns(4.3334M);
			mock.Setup(p => p.AssessmentCustomsValue).Returns(6.65556M);
			iMonthlyClosingEntryLineSnapshotObject = mock.Object;

			var mock2 = new Mock<IImportSpecificRate>();
			mock2.Setup(p => p.Value).Returns(2.112M);
			importSpecificRateObject = mock2.Object;

			var mock3 = new Mock<IContentInformation>();
			mock3.Setup(p => p.DegreePercentage).Returns(2.2332M);
			iContentInformationObject = mock3.Object;

			var mock4 = new Mock<IExciseDuty>();
			mock4.Setup(p => p.DegreePercentage).Returns(2.2789M);
			mock4.Setup(p => p.Value).Returns(3.3123M);
			iExciseDutyObject = mock4.Object;

			var mock5 = new Mock<IImportLineDocument>();
			mock5.Setup(p => p.IssuingDate).Returns(new DateTime(2021, 12, 29));
			iImportLineDocument = mock5.Object;
		}
		IMonthlyClosingEntryLineSnapshot iMonthlyClosingEntryLineSnapshotObject;
		IImportSpecificRate importSpecificRateObject;
		IContentInformation iContentInformationObject;
		IExciseDuty iExciseDutyObject;
		IImportLineDocument iImportLineDocument;
	}
}
