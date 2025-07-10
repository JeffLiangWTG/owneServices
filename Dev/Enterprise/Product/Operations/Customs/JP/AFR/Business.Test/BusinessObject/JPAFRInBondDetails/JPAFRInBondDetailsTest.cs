using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRInBondDetails))]
	class JPAFRInBondDetailsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_ParentId = consol1.PK;
			header1.JPH_ParentTableCode = consol1.TablePrefix;
			var consol2 = Factory.New<ForwardingConsol>();
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_ParentId = consol2.PK;
			header2.JPH_ParentTableCode = consol2.TablePrefix;
			var inbond = Factory.New<JPAFRInBondDetails>();
			inbond.JPI_JPH_Header = header1.PK;
			AssertEquals(header1, inbond.Header);

			inbond.JPI_JPH_Header = header2.PK;
			AssertEquals(header2, inbond.Header);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = "AFRTEST0001";
			var inbond = Factory.New<JPAFRInBondDetails>();
			inbond.JPI_JPH_Header = header.PK;
			return inbond;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = "AFRTEST0002";
			var inbond = Factory.New<JPAFRInBondDetails>();
			inbond.JPI_JPH_Header = header.PK;
			return inbond;
		}
	}
}
