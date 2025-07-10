using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC037C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC037CMessageInterpreter))]
	class CC037CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC037CMessageInterpreter, CC037CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE037;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => GetExpectedInterpretationText();

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC037CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override CC037CProvider GetProvider(TextReader reader) => new CC037CProvider(new MailBoxItemProvider<Cc037CType>(reader).Message);

		public static ZString GetExpectedInterpretationText()
		{
			var expected = new ZStringBuilder();
			expected.Append("Response Query on Guarantee Message (IE037) has been received. NCTS has sent the response on Query on Guarantee for Job B00001000.");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr><td>Requester Identification Number</td><td>IN037</td></tr>");
			expected.Append("<tr><td>Requester Role</td><td>1</td></tr>");
			expected.Append("</table>");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("Guarantee Information:");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr><td>Guarantee Reference Sequence Number</td><td>1</td></tr>");
			expected.Append("<tr><td>Guarantee Reference GRN</td><td>12GRNCC055C012345A678901</td></tr>");
			expected.Append("<tr><td>Guarantee Monitoring code</td><td>7</td></tr>");
			expected.Append("</table>");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("Guarantee Query Information:");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr><td>Guarantee Query Identifier</td><td>1</td></tr>");
			expected.Append("<tr><td>Guarantee Query Period From Date</td><td>31-Jan-23</td></tr>");
			expected.Append("<tr><td>Guarantee Query Period To Date</td><td>28-Feb-23</td></tr>");
			expected.Append("</table>");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("Usage Information:");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr><td>Usage Sequence Number</td><td>1</td></tr>");
			expected.Append("<tr><td>Usage MRN</td><td>19MRNCC055C0123456</td></tr>");
			expected.Append("<tr><td>Usage Covered Amount</td><td>10 QWE</td></tr>");
			expected.Append("<tr><td>Usage Lock Date</td><td>21-Feb-23</td></tr>");
			expected.Append("<tr><td>Arrival Date &amp; Time</td><td>05-Feb-23 03:02</td></tr>");
			expected.Append("<tr><td>Release Date</td><td>19-Feb-23</td></tr>");
			expected.Append("</table>");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("Usage Information:");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr><td>Usage Sequence Number</td><td>2</td></tr>");
			expected.Append("<tr><td>Usage MRN</td><td>20MRNCC055C0123456</td></tr>");
			expected.Append("<tr><td>Usage Covered Amount</td><td>9 POI</td></tr>");
			expected.Append("<tr><td>Usage Lock Date</td><td>15-Mar-23</td></tr>");
			expected.Append("<tr><td>Arrival Date &amp; Time</td><td>15-Feb-23 04:03</td></tr>");
			expected.Append("<tr><td>Release Date</td><td>09-Apr-23</td></tr>");
			expected.Append("</table>");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("Exposure Information:");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr><td>Exposure</td><td>7</td></tr>");
			expected.Append("<tr><td>Exposure Counter</td><td>1</td></tr>");
			expected.Append("<tr><td>Balance</td><td>4 YGV</td></tr>");
			expected.Append("</table>");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("Guarantor Information:");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr><td>Identification number</td><td>LKJ321</td></tr>");
			expected.Append("<tr><td>Name</td><td>Guarantor 1</td></tr>");
			expected.Append("<tr><td>Street &amp; Number</td><td>123 WHERE ST</td></tr>");
			expected.Append("<tr><td>Postcode</td><td>2020</td></tr>");
			expected.Append("<tr><td>City</td><td>CITY</td></tr>");
			expected.Append("<tr><td>Country</td><td>IE</td></tr>");
			expected.Append("<tr><td>Contact Person Name</td><td>Contact Person 1</td></tr>");
			expected.Append("<tr><td>Phone Number</td><td>+123456789</td></tr>");
			expected.Append("<tr><td>Email Address</td><td>tes@email.com</td></tr>");
			expected.Append("</table>");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("Comprehensive Guarantee Information:");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr><td>Reference Amount</td><td>7</td></tr>");
			expected.Append("<tr><td>Percentage of Reference Amount</td><td>45</td></tr>");
			expected.Append("<tr><td>Guarantee Amount</td><td>12 USD</td></tr>");
			expected.Append("<tr><td>No. of certificates</td><td>3</td></tr>");
			expected.Append("<tr><td>Validity Start Date</td><td>11-Feb-23</td></tr>");
			expected.Append("<tr><td>Validity End Date</td><td>15-Feb-23</td></tr>");
			expected.Append("<tr><td>Invalidity Reason Code</td><td>AB8</td></tr>");
			expected.Append("<tr><td>Invalidity Reason Text</td><td>Some reason text</td></tr>");
			expected.Append("<tr><td>Liability liberation date</td><td>11-May-23</td></tr>");
			expected.Append("<tr><td>Restricted use for suspended goods</td><td>Yes</td></tr>");
			expected.Append("<tr><td>Validity Limitation Sequence Number</td><td>1</td></tr>");
			expected.Append("<tr><td>Guarantee Not Valid In</td><td>AB</td></tr>");
			expected.Append("<tr><td>Validity Limitation Sequence Number</td><td>2</td></tr>");
			expected.Append("<tr><td>Guarantee Not Valid In</td><td>DE</td></tr>");
			expected.Append("</table>");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("Individual Guarantee by Guarantor:");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr><td>Guarantee Amount</td><td>8 EUR</td></tr>");
			expected.Append("</table>");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("Individual Guarantee Voucher:");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr><td>Issue Date</td><td>24-Jan-23</td></tr>");
			expected.Append("<tr><td>Expiry Date</td><td>28-Jan-23</td></tr>");
			expected.Append("<tr><td>Copy Given</td><td>Yes</td></tr>");
			expected.Append("<tr><td>TIR Carnet</td><td>No</td></tr>");
			expected.Append("<tr><td>Voucher Amount</td><td>999 AUD</td></tr>");
			expected.Append("</table>");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("Guarantee Information:<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr>");
			expected.Append("<td>");
			expected.Append("Guarantee Reference Sequence Number</td>");
			expected.Append("<td>");
			expected.Append("1</td>");
			expected.Append("</tr>");
			expected.Append("<tr>");
			expected.Append("<td>");
			expected.Append("Guarantee Reference GRN</td>");
			expected.Append("<td>");
			expected.Append("12GRNCC055C012345A678901</td>");
			expected.Append("</tr>");
			expected.Append("<tr>");
			expected.Append("<td>");
			expected.Append("Guarantee Monitoring code</td>");
			expected.Append("<td>");
			expected.Append("7</td>");
			expected.Append("</tr>");
			expected.Append("</table>");
			expected.Append("<br />");
			expected.Append("<br />");
			expected.Append("Guarantee Query Information:<br />");
			expected.Append("<br />");
			expected.Append("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">");
			expected.Append("<tr>");
			expected.Append("<td>");
			expected.Append("Guarantee Query Identifier</td>");
			expected.Append("<td>");
			expected.Append("1</td>");
			expected.Append("</tr>");
			expected.Append("<tr>");
			expected.Append("<td>");
			expected.Append("Guarantee Query Period From Date</td>");
			expected.Append("<td>");
			expected.Append("31-Jan-23</td>");
			expected.Append("</tr>");
			expected.Append("<tr>");
			expected.Append("<td>");
			expected.Append("Guarantee Query Period To Date</td>");
			expected.Append("<td>");
			expected.Append("28-Feb-23</td>");
			expected.Append("</tr>");
			expected.Append("</table>");
			return expected.ToString();
		}
	}
}
