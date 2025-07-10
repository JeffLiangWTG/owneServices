using NUnit.Framework;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	class ICSInterpretationPrettierTest : TestCase
	{
		public void TestCreatePrettyInterpretation()
		{
			const string errorResponse = @"<err:ErrorResponse SchemaVersion=""2.0"" xmlns:dsl=""http://decisionsoft.com/rim/errorExtension"" xmlns:err=""http://www.govtalk.gov.uk/CM/errorresponse""><err:Application><err:MessageCount>2</err:MessageCount></err:Application><err:Error><err:RaisedBy>HMRC</err:RaisedBy><err:Number>4065</err:Number><err:Type>schema</err:Type><err:Text>Invalid content was found starting with element 'IdeOfMeaOfTraCroHEA85'. One of '{TraModAtBorHEA76}' is expected.</err:Text><err:Location>/q1:CC315A[1]/HEAHEA[1]</err:Location></err:Error><err:Error><err:RaisedBy>HMRC</err:RaisedBy><err:Number>4066</err:Number><err:Type>schema</err:Type><err:Text>The content of element 'CUSOFFFENT730' is not complete. One of '{ExpDatOfArrFIRENT733}' is expected.</err:Text><err:Location>/q1:CC315A[1]/CUSOFFFENT730[1]</err:Location></err:Error></err:ErrorResponse>";
			const string expectedInterpretation = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><h3>Response Errors</h3><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>HMRC</td></tr><tr><td>Error Number</td><td>4065</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Invalid content was found starting with element 'IdeOfMeaOfTraCroHEA85'. One of '{TraModAtBorHEA76}' is expected.</td></tr><tr><td>Location</td><td>/q1:CC315A[1]/HEAHEA[1]</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>HMRC</td></tr><tr><td>Error Number</td><td>4066</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>The content of element 'CUSOFFFENT730' is not complete. One of '{ExpDatOfArrFIRENT733}' is expected.</td></tr><tr><td>Location</td><td>/q1:CC315A[1]/CUSOFFFENT730[1]</td></tr></table></ul><br>";

			var actualInterpretation = ICSInterpretationPrettier.CreatePrettyInterpretation(errorResponse);
			AssertEquals(expectedInterpretation, actualInterpretation);
		}
	}
}
