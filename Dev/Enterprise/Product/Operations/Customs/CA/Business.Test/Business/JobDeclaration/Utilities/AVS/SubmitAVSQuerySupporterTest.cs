using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class SubmitAVSQuerySupporterTest : TestCaseWithFactory
	{
		public void TestSubmitAVSQuerySupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var supporter = new SubmitAVSQuerySupporter(declaration);
			Assert("SupportSubmitAVSQuery", supporter.SupportSubmitAVSQuery);
			AssertEquals("NotSupportSubmitAVSQueryReason", "", supporter.NotSupportSubmitAVSQueryReason);
			AssertType<SubmitAVSQueryProcessor>("CreateSubmitAVSQueryProcessor", supporter.CreateSubmitAVSQueryProcessor());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			supporter = new SubmitAVSQuerySupporter(declaration);
			Assert("SupportSubmitAVSQuery", !supporter.SupportSubmitAVSQuery);
			AssertEquals("NotSupportSubmitAVSQueryReason", "Trigger action Submit AVS Query is valid only for IMP declaration.", supporter.NotSupportSubmitAVSQueryReason);
		}
	}
}
