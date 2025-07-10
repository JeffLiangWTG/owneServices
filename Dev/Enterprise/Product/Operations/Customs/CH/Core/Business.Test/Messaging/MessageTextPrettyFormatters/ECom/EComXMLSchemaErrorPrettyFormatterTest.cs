using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class EComXMLSchemaErrorPrettyFormatterTest : TestCase
{
	#region Message Response

	ZString ResponseText => $@"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<edecComplaintResponse xmlns = ""http://www.e-dec.ch/xml/schema/edecComplaintResponse/v1""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.edec.ch/xml/schema/edecComplaintResponse/v1 edecComplaintResponse_v_1_0.xsd"" schemaVersion=""1.0"" >
<requestorTraderIdentificationNumber>not available</requestorTraderIdentificationNumber>
 <edecComplaintRejection>
<rejectionDate>2015-06-08</rejectionDate>
<rejectionTime>15:50:14</rejectionTime>
 <errors>
 <XMLSchemaErrors>
 <schema>
 <schemaLocation>http://www.ezv.admin.ch/pdf_linker.php?doc=edecComplaintResponse_v_1_0</schemaLocation>
 <namespace>http://www.e-dec.ch/xml/schema/edecComplaintResponse/v1</namespace>
<version>1.0</version>
 </schema>
 <parser>
 <name>Xerces-J 2.6.2</name>
 </parser>
 <error>
 <message>Parsing Error: ONLY FOR TEST PURPOSE.</message>
 </error>
 </XMLSchemaErrors>
 </errors>
 </edecComplaintRejection>
</edecComplaintResponse>";

	ZString FormattedText => $@"<h2>eCom Request has been rejected</h2>
<p>Rejection Date/Time: 2015-06-08 15:50:14</p>
<h3>Schema Errors</h3>
<p>Parsing Error: ONLY FOR TEST PURPOSE.</p>";

	ZString MessageIsEmpty => "<h2>Message is empty</h2>";

	#endregion

	public void TestGetFormattedText_SchemaErrorDetails()
	{
		var expectedMessageInterpretation = FormattedText;

		var interpretation = new EComXMLSchemaErrorPrettyFormatter(null).GetFormattedText();
		AssertEquals($"When MessageDetail is empty, EM_MessageInterpretation should return '{MessageIsEmpty}'", MessageIsEmpty, interpretation);

		interpretation = new EComXMLSchemaErrorPrettyFormatter((IEComXMLSchemaErrorsResponseDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
		var expectedText = expectedMessageInterpretation.Replace(System.Environment.NewLine, string.Empty);
		AssertEquals("EM_MessageInterpretation", expectedText, interpretation);
	}
}
