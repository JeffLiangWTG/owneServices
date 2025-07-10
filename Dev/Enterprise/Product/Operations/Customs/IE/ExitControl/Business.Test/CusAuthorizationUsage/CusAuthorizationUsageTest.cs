using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusAuthorizationUsage))]
	sealed class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
	{
		public static (CusExitHeader header, CusExitConsignment consignment, CusAuthorizationUsage authorizationUsage) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			(var header, var consignment) = CusExitConsignmentTest.GetNewBusinessObject(factory);
			var authorizationUsage = consignment.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = "abc";
			authorizationUsage.AGC_Number = "001";
			authorizationUsage.AGC_OH_Owner = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			return (header, consignment, authorizationUsage);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).authorizationUsage;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
	}
}
