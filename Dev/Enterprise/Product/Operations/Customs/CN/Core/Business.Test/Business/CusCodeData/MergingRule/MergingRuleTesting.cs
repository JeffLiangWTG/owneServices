using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(MergingRule))]
	class MergingRuleTesting : Customs.Business.Testing.CusCodeDataTest<MergingRule>
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration.MergingRules.AddNew();
		}

		public void TestSupportsNotes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var mergingRule = declaration.MergingRules.AddNew();
			Assert("SupportsNotes should be false", !mergingRule.SupportsNotes);
		}
	}
}
