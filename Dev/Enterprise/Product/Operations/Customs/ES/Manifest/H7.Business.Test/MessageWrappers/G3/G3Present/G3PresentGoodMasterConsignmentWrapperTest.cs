using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(G3PresentGoodMasterConsignmentWrapper))]
	public class G3PresentGoodMasterConsignmentWrapperTest : G3MasterConsignmentWrapperTest<G3PresentGoodMasterConsignmentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
#if NET
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
				"Value cannot be null. (Parameter 'bills')", () => new G3PresentGoodMasterConsignmentWrapper(null));

				var bill = Factory.New<AsycudaBill>();
				AssertExceptionThrown("Throws Exception if bills[0].Header is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'bills[0].Header')", () => new G3PresentGoodMasterConsignmentWrapper(new List<AsycudaBill> { bill }));
#else
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
				"Value cannot be null.\r\nParameter name: bills", () => new G3PresentGoodMasterConsignmentWrapper(null));

				var bill = Factory.New<AsycudaBill>();
				AssertExceptionThrown("Throws Exception if bills[0].Header is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: bills[0].Header", () => new G3PresentGoodMasterConsignmentWrapper(new List<AsycudaBill> { bill }));
#endif
			});
		}

		public void TestHouseConsignment()
		{
			CombineAssertions(() =>
			{
				var houseConsignment = Provider.HouseConsignment;
				AssertNotNull("Expected filled House Consignment", houseConsignment);
				AssertEquals("Expected two House Consignments", 2, houseConsignment.Count);
				AssertEquals("Expected filled Transport Document", "5025", houseConsignment.FirstOrDefault().TransportDocument.Name);

				AssertSame("Cached House Consignment", houseConsignment, Provider.HouseConsignment);
			});
		}

		protected override G3PresentGoodMasterConsignmentWrapper GetProviderCore()
		{
			return new G3PresentGoodMasterConsignmentWrapper(header.Bills);
		}
	}
}
