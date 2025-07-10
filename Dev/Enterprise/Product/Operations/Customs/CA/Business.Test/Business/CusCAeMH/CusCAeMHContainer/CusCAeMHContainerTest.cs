using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHContainer))]
	sealed class CusCAeMHContainerTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Master.Containers.AddNew();
		}

		CusCAeMHMaster Master
		{
			get { return master ?? (master = Factory.NewWithValidTestData<CusCAeMHMaster>()); }
		}
		CusCAeMHMaster master;
	}
}
