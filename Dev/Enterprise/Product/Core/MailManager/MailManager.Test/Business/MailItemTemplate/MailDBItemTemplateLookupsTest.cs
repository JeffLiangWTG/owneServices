using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MailManager.Business.Testing
{
	public class MailDBItemTemplateLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMailTemplateCategories()
		{
			var lookups = MailDBItemTemplateLookups.New();
			AssertContainsExactElementsInAnyOrder(GetExpectedMailTemplateCategoryList(), lookups.MailTemplateCategories);
		}

		public void TestLanguages()
		{
			var lookups = MailDBItemTemplateLookups.New();
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Language), lookups.Languages);
		}

		protected virtual CodeDescriptionPairList GetExpectedMailTemplateCategoryList()
		{
			return new MailTemplateCategoryList();
		}
	}
}
