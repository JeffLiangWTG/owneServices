using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CSARevenueSummaryFormMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2020, 11, 11)]
		public void TestPopulateEdifactMessage_Orgiginal()
		{
			var wrapper = new CSARSFMessageWrapper(cSARevenueSummaryForm);
			wrapper.PreProcessBeforeSendMessage(MessageSubTypes.Create);
			var messages = new CSARevenueSummaryFormMessageBuilder(wrapper, MessageSubTypes.Create).PopulateMessages().GetBuilderResults();
			var message = messages.ToArray()[0];
			AssertEquals(createMessage, message.Message.EM_MessageText);
		}
		const string createMessage = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:S:99B:UN'BGM+335+<<MSGNO PLACEHOLDER>>+9'DTM+137:20201111:102'DTM+415:202010:610'DTM+90:20200919:102'DTM+91:20201018:102'RFF+ARA:ICID0001'MOA+43:000'UNS+D'DMS++84'LIN+49010+11'MOA+203:000'LIN+49010+9'MOA+203:000'LIN+49121+11'MOA+203:000'LIN+49121+9'MOA+203:000'LIN+49011'MOA+203:000'LIN+49475'MOA+203:000'LIN+49443'MOA+203:000'LIN+49555'MOA+203:000'LIN+49454'MOA+203:000'DMS++83'LIN+49010'MOA+203:000'LIN+49121'MOA+203:000'LIN+49443'MOA+203:000'LIN+49017'MOA+203:000'LIN+49018'MOA+203:000'LIN+49019'MOA+203:000'LIN+49409'MOA+203:000'LIN+49437'MOA+203:000'LIN+49555'MOA+203:000'DMS++66'LIN+49010'MOA+203:000'LIN+49121'MOA+203:000'DMS++70'LIN+B2-1'DOC+998'LIN+K23'MOA+203:000'LOC+127+5678'DOC+998'UNS+S'TAX+4'MOA+176:000'UNT+63+<<MSGNO PLACEHOLDER>>'";

		[TestDate(2020, 11, 11)]
		public void TestPopulateEdifactMessage_Adjustment()
		{
			var wrapper = new CSARSFMessageWrapper(cSARevenueSummaryForm);
			wrapper.PreProcessBeforeSendMessage(MessageSubTypes.Change);
			var messages = new CSARevenueSummaryFormMessageBuilder(wrapper, MessageSubTypes.Change).PopulateMessages().GetBuilderResults();
			var message = messages.ToArray()[0];
			AssertEquals(changeMessage, message.Message.EM_MessageText);
		}
		const string changeMessage = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:S:99B:UN'BGM+335+<<MSGNO PLACEHOLDER>>+4'DTM+137:20201111:102'DTM+415:202010:610'DTM+90:20200919:102'DTM+91:20201018:102'RFF+ARA:ICID0001'MOA+43:000'UNS+D'DMS++84'LIN+49010+11'MOA+203:000'LIN+49010+9'MOA+203:000'LIN+49121+11'MOA+203:000'LIN+49121+9'MOA+203:000'LIN+49011'MOA+203:000'LIN+49475'MOA+203:000'LIN+49443'MOA+203:000'LIN+49555'MOA+203:000'LIN+49454'MOA+203:000'DMS++83'LIN+49010'MOA+203:000'LIN+49121'MOA+203:000'LIN+49443'MOA+203:000'LIN+49017'MOA+203:000'LIN+49018'MOA+203:000'LIN+49019'MOA+203:000'LIN+49409'MOA+203:000'LIN+49437'MOA+203:000'LIN+49555'MOA+203:000'DMS++66'LIN+49010'MOA+203:000'LIN+49121'MOA+203:000'DMS++70'LIN+B2-1'DOC+998'LIN+K23'MOA+203:000'LOC+127+5678'DOC+998'UNS+S'TAX+4'MOA+176:000'UNT+63+<<MSGNO PLACEHOLDER>>'";

		protected override void SetUp()
		{
			base.SetUp();

			cSARevenueSummaryForm = Factory.New<CusStatementHeader>();
			cSARevenueSummaryForm.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			cSARevenueSummaryForm.B2_PrintDate = new ZDateTime(2020, 11, 11);
			cSARevenueSummaryForm.B2_PeriodEndDate = new ZDate(2020, 10, 18);
			cSARevenueSummaryForm.B2_PeriodStartDate = new ZDate(2020, 09, 19);
			cSARevenueSummaryForm.B2_ImporterCustomsID = "ICID0001";

			var customsAssessment = cSARevenueSummaryForm.CustomsAssessments.AddNew();
			customsAssessment.Type = CustomsAssessmentsCodes.Codes.B2Dash1;
			customsAssessment.PortCode = "1234";
			customsAssessment = cSARevenueSummaryForm.CustomsAssessments.AddNew();
			customsAssessment.Type = CustomsAssessmentsCodes.Codes.K23;
			customsAssessment.PortCode = "5678";
		}

		CusStatementHeader cSARevenueSummaryForm;
	}
}
