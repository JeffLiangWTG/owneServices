using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentTemplatedTextGeneatorTest : TemplatedTextGeneratorTest<IncidentTemplatedTextGenerator, IncidentMainBase>
	{
		public void TestGenerateTemplatedText()
		{
			BizO.IM_IncidentNumber = "1234";
			BizO.IM_Priority = "PR1";
			BizO.IM_Description = "Test incident";

			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_Code = "CL1";
			client.OH_FullName = "Test client";
			BizO.IM_OH_Client = client.PK;

			string template = "<<HTMLLink>>\n<<IM_Priority>>\n<<IM_Description>>\n<<ClientCode>>\n<<ClientName>>";
			string result = TextGenerator.GenerateTemplatedText(template, "<a href='some.url'>1234</a>", "Some header", "Some footer");
			AssertEquals("GenerateTemplatedText", "<a href='some.url'>1234</a>\nPR1\nTest incident\nCL1\nTest client", result);
		}

		#region Implementation

		protected override IncidentTemplatedTextGenerator GetTextGeneratorForTest(IncidentMainBase bizO)
		{
			return new IncidentTemplatedTextGenerator(BizO);
		}

		protected override List<string> ExpectedSupportedMacros
		{
			get
			{
				List<string> result = new List<string>();

				result.Add("<<IncidentType>>");
				result.Add("<<IM_Priority>>");
				result.Add("<<ClientCode>>");
				result.Add("<<ClientName>>");
				result.Add("<<IM_Description>>");

				return result;
			}
		}

		protected override IncidentMainBase GetBusinessObjectForTest()
		{
			return Factory.New<ProfessionalServicesQuote>();
		}

		#endregion
	}
}