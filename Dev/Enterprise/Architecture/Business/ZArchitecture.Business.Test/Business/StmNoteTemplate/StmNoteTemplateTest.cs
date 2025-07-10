using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmNoteTemplate))]
	sealed class StmNoteTemplateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsAllCompanies_ReadOnly()
		{
			var template = Factory.New<StmNoteTemplate>();
			Assert(template.IsAllCompaniesInfo.ReadOnly);

			template.IsPublished = true;
			Assert(!template.IsAllCompaniesInfo.ReadOnly);
		}
	}
}
