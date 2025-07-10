using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(KREntryLineDetailsView))]
	sealed class KREntryLineDetailsViewTest : EnterpriseBusinessObjectTestCase
	{
		// Not relevant for BizOs generated from views
		public override void TestSaveAndDeleteBusinessObject()
		{
		}
	}
}
