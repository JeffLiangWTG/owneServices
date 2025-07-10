using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Moq;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class IEnumerableExtensionsTest : TestCaseWithFactory
	{
		public void TestConcatenateWithPageDelimiter()
		{
			var listOfStrings = new ZString[] { "11111", "22222", "33333", "44444", "55555" };
			AssertEquals("11111*1*22222/33333*2*44444/55555", listOfStrings.ConcatenateWithPageDelimiter("/", 5, 11));
			AssertEquals("11111*1*22222*2*33333*3*44444*4*55555", listOfStrings.ConcatenateWithPageDelimiter("/", 4, 4));
			AssertEquals("11111/22222/33333/44444/55555", listOfStrings.ConcatenateWithPageDelimiter("/", 200, 200));
		}

		public void TestGetTotalAmounts()
		{
			var classifications = new List<IClassificationLine1>();
			classifications.Add(GetClassificationLine(MessageConstants.B3RecordIdentifiers.Positive, 1000m, 100m, "321", 30m));
			classifications.Add(GetClassificationLine(MessageConstants.B3RecordIdentifiers.Positive, 2100m, 120m, "322", 51m));
			classifications.Add(GetClassificationLine(MessageConstants.B3RecordIdentifiers.Negative, 20m, 30m, "321", 45m));
			classifications.Add(GetClassificationLine(MessageConstants.B3RecordIdentifiers.Negative, 0m, 15m, "322", 39m));

			var deposit = 1000m;
			var totalAmounts = classifications.GetTotalAmounts(MessageConstants.B3RecordIdentifiers.Positive, deposit);
			AssertEquals("Customs Duty", 100m + 200m + 100m + 200m + 1000m, totalAmounts.TotalCustomsDuty);
			AssertEquals("GST", 100m + 120m, totalAmounts.TotalGST);
			AssertEquals("Deposit", deposit, totalAmounts.Deposit);
			AssertEquals("Deposit included in Customs Duty", 600m, totalAmounts.TotalCustomsDuty - totalAmounts.Deposit);
			AssertEquals("Tax", 1000m + 2100m, totalAmounts.TotalExciseTax);
			AssertEquals("Sima", 30m, totalAmounts.TotalSIMAAssessment);

			totalAmounts = classifications.GetTotalAmounts(MessageConstants.B3RecordIdentifiers.Negative, 0.0m);
			AssertEquals("Tax", -20m, totalAmounts.TotalExciseTax);
			AssertEquals("GST", -45m, totalAmounts.TotalGST);
			AssertEquals("Customs Duty", -600m, totalAmounts.TotalCustomsDuty);
			AssertEquals("Sima", 0m, totalAmounts.TotalSIMAAssessment);
		}

		IClassificationLine1 GetClassificationLine(ZString identifier, ZDecimal excuseAmt, ZDecimal gstAmt, ZString simaCode, ZDecimal simaAss)
		{
			var mockLine = new Mock<IClassificationLine1>();
			mockLine.Setup(m => m.ExciseTaxAmount).Returns(excuseAmt);
			mockLine.Setup(m => m.GSTAmount).Returns(gstAmt);
			mockLine.Setup(m => m.SIMACode).Returns(simaCode);
			mockLine.Setup(m => m.SIMAAssessment).Returns(simaAss);
			mockLine.Setup(m => m.RecordIdentifier).Returns(identifier);
			mockLine.Setup(m => m.ClassificationLines).Returns(new[] { GetClassificationLine2(100m), GetClassificationLine2(200m) });
			return mockLine.Object;
		}

		IClassificationLine2 GetClassificationLine2(ZDecimal dutyAmt)
		{
			var mockLine = new Mock<IClassificationLine2>();
			mockLine.Setup(m => m.CustomsDutyAmount).Returns(dutyAmt);
			return mockLine.Object;
		}
	}
}
