using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.eRouter.Business.Test
{
	[TestedType(typeof(eRouterEdiEnterpriseCommunicationCollection))]
	class eRouterEdiEnterpriseCommunicationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestOrgHeaderRelationshipFilter()
		{
			EDIOrgHeader orgHeader = Factory.New<EDIOrgHeader>();
			orgHeader.LicenceEnterpriseCode = "ZTZ";
			string identifier = "TST1234";
			eRouterEdiEnterpriseCommunication communication1 = Factory.New<eRouterEdiEnterpriseCommunication>();
			communication1.EC_EnterpriseCode = orgHeader.LicenceEnterpriseCode;
			communication1.EC_CompanyCode = "SYD";
			communication1.EC_Identifier = identifier;
			eRouterEdiEnterpriseCommunication communication2 = Factory.New<eRouterEdiEnterpriseCommunication>();
			communication2.EC_EnterpriseCode = "YTY";
			communication1.EC_CompanyCode = "SYD";
			communication2.EC_Identifier = identifier;
			eRouterEdiEnterpriseCommunication communication3 = Factory.New<eRouterEdiEnterpriseCommunication>();
			communication3.EC_EnterpriseCode = orgHeader.LicenceEnterpriseCode;
			communication1.EC_CompanyCode = "MEL";
			communication3.EC_Identifier = identifier;

			ZQuery query = new ZQuery(eRouterEdiEnterpriseCommunicationSchema.EC_Identifier, identifier);
			query.FetchOnlyFromLocalCache = true;
			eRouterEdiEnterpriseCommunicationCollection collection = new eRouterEdiEnterpriseCommunicationCollection(Factory);
			collection.Load(query);

			AssertEquals(3, collection.Count);
			AssertCollectionContains(communication1, collection);
			AssertCollectionContains(communication2, collection);
			AssertCollectionContains(communication3, collection);

			collection = new eRouterEdiEnterpriseCommunicationCollection(orgHeader);
			collection.Load(query);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(communication1, collection);
			AssertCollectionContains(communication3, collection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<eRouterEdiEnterpriseCommunication>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new eRouterEdiEnterpriseCommunicationCollection(Factory);
		}
	}
}
