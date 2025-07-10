using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsCusAuthorizationUsage))]
	public class NctsCusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAGC_NumberReadOnly()
		{
			var movementHeader = Factory.New<NctsDepartureMovementHeader>();
			var cusAuthorizationUsage = movementHeader.CusAuthorizationUsages.AddNew();

			AssertEquals(false, cusAuthorizationUsage.AGC_NumberReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject() => GetCusAuthorizationUsage(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetCusAuthorizationUsage(factory);

		static NctsCusAuthorizationUsage GetCusAuthorizationUsage(BusinessObjectFactory factory)
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var cusAuthorizationUsage = factory.New<NctsHeader>().CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Number = "123";
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
			cusAuthorizationUsage.AGC_Code = "BOI";
			return cusAuthorizationUsage;
		}
	}
}
