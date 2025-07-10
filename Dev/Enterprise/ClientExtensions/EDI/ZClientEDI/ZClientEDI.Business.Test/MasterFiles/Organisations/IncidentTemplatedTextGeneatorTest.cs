using System.Collections.Generic;
using Enterprise.Client.EDI.IncidentManager.Business.Test;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class IncidentTemplatedTextGeneatorTest : TemplatedTextGeneratorTest<OrgTemplatedTextGenerator, EDIOrgHeader>
	{
		public void TestGenerateTemplatedText()
		{
			BizO.OH_Code = "1234";
			BizO.OH_RL_NKClosestPort = "AUSYD";
			BizO.OH_FullName = "Test client";

			string template = "<<HTMLLink>>\n<<OH_FullName>>\n<<OH_FullNameOriginalValue>>\n<<OH_Code>>\n<<OH_CodeOriginalValue>>\n<<UNLOCO>>";
			string result = TextGenerator.GenerateTemplatedText(template, "<a href='some.url'>1234</a>", "Some header", "Some footer");
			AssertEquals("GenerateTemplatedText", "<a href='some.url'>1234</a>\nTest client\nTest client\nTESCLISYD\nTESCLISYD\nAUSYD", result);
		}

		#region Implementation

		protected override OrgTemplatedTextGenerator GetTextGeneratorForTest(EDIOrgHeader bizO)
		{
			return new OrgTemplatedTextGenerator(BizO);
		}

		protected override List<string> ExpectedSupportedMacros
		{
			get
			{
				List<string> result = new List<string>();

				result.Add("<<OH_FullName>>");
				result.Add("<<OH_FullNameOriginalValue>>");
				result.Add("<<OH_Code>>");
				result.Add("<<OH_CodeOriginalValue>>");
				result.Add("<<UNLOCO>>");

				return result;
			}
		}

		protected override EDIOrgHeader GetBusinessObjectForTest()
		{
			return Factory.New<EDIOrgHeader>();
		}

		#endregion
	}
}