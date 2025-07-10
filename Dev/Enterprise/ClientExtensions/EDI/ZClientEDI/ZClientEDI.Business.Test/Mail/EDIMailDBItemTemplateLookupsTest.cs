using Enterprise.MailManager.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	sealed class EDIMailDBItemTemplateLookupsTest : MailDBItemTemplateLookupsTest
	{
		protected override CodeDescriptionPairList GetExpectedMailTemplateCategoryList()
		{
			var list = base.GetExpectedMailTemplateCategoryList();
			list.AddRange(new EDIMailTemplateCategoryList());
			return list;
		}
	}
}