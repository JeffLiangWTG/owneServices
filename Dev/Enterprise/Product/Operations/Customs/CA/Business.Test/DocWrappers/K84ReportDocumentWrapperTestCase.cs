using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	abstract class K84ReportDocumentWrapperTestCase : TestCaseWithFactory
	{
		public abstract void TestProperties();

		protected static void AssertAmounts(K84AmountsWithPenaltyWrapper amounts, decimal duty, decimal sima, decimal excise, decimal gst, decimal total, decimal penalty, decimal allTotal)
		{
			AssertAmounts(amounts, duty, sima, excise, gst, total);
			AssertEquals("LateFilingPenalty", penalty, amounts.LateFilingPenalty);
			AssertEquals("TotalAmount", allTotal, amounts.TotalAmount);
		}

		protected static void AssertAmounts(K84AmountsWrapper amounts, decimal duty, decimal sima, decimal excise, decimal gst, decimal total)
		{
			AssertEquals("CustomsDuty", duty, amounts.CustomsDuty);
			AssertEquals("SIMAAssessment", sima, amounts.SIMAAssessment);
			AssertEquals("ExciseTax", excise, amounts.ExciseTax);
			AssertEquals("GST", gst, amounts.GST);
			AssertEquals("TotalDutiesAndTaxes", total, amounts.TotalDutiesAndTaxes);
		}

		protected static K84Message CreateMessageFromInterchangeString(BusinessObjectFactory factory, string interchangeString)
		{
			var interchange = EDIInterchange.CreateNewInterchangeFromString(factory, interchangeString.Replace("\r\n", ""), EDIMessage.ApplicationCodes.CAIMP, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			var message = (K84Message)interchange.ContainedMessages[0];
			message.EM_MessageSubType = BatchProcessorUtilities.GetK84MessageSubType(message.Interchange.EI_HeaderText);
			return message;
		}
	}
}
