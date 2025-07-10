using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ConsolidateLVXDeclarationSupporterTest : TestCaseWithFactory
	{
		public void TestConsolidateLVXDeclarationSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var supporter = new ConsolidateLVXDeclarationSupporter(declaration);
			Assert("SupportConsolidateLVXDeclaration", supporter.SupportConsolidateLVXDeclaration);
			AssertEquals("NotSupportConsolidateLVXDeclarationReason", "", supporter.NotSupportConsolidateLVXDeclarationReason);
			AssertType<ConsolidateLVXDeclarationProcessor>("CreateConsolidateLVXDeclarationProcessor", supporter.CreateConsolidateLVXDeclarationProcessor());

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			supporter = new ConsolidateLVXDeclarationSupporter(declaration);
			Assert("SupportConsolidateLVXDeclaration", !supporter.SupportConsolidateLVXDeclaration);
			AssertEquals("NotSupportConsolidateLVXDeclarationReason", "Trigger action Consolidate LVX Declaration is valid only for LVX declaration.", supporter.NotSupportConsolidateLVXDeclarationReason);
		}
	}
}
