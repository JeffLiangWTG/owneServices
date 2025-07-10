using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	[TestedType(typeof(StmUniversalJobLink))]
	sealed class StmUniversalJobLinkTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var universalJobLink = Factory.NewWithValidTestData<StmUniversalJobLink>();
			universalJobLink.UCL_ParentTableCode = "WRC";
			return universalJobLink;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var universalJobLink = factory.NewWithValidTestData<StmUniversalJobLink>();
			universalJobLink.UCL_ParentTableCode = "WRC";
			return universalJobLink;
		}
	}
}
