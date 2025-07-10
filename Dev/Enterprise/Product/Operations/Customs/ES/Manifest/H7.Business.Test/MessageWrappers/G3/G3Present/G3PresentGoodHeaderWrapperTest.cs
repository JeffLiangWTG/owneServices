using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(G3PresentGoodHeaderWrapper))]
	public class G3PresentGoodHeaderWrapperTest : G3CommonHeaderWrapperTest<G3PresentGoodHeaderWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
#if NET
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
				"Value cannot be null. (Parameter 'bills')", () => new G3PresentGoodHeaderWrapper(null, "LRN"));

				AssertExceptionThrown("Throws Exception if localReferenceNumber is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'localReferenceNumber')", () => new G3PresentGoodHeaderWrapper(header.Bills, null));

				var bill = Factory.New<AsycudaBill>();
				AssertExceptionThrown("Throws Exception if bills[0].Header is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'bills[0].Header')", () => new G3PresentGoodHeaderWrapper(new List<AsycudaBill> { bill }, "LRN"));
#else
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
				"Value cannot be null.\r\nParameter name: bills", () => new G3PresentGoodHeaderWrapper(null, "LRN"));

				AssertExceptionThrown("Throws Exception if localReferenceNumber is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: localReferenceNumber", () => new G3PresentGoodHeaderWrapper(header.Bills, null));

				var bill = Factory.New<AsycudaBill>();
				AssertExceptionThrown("Throws Exception if bills[0].Header is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: bills[0].Header", () => new G3PresentGoodHeaderWrapper(new List<AsycudaBill> { bill }, "LRN"));
#endif
			});
		}

		public void TestMasterConsignment()
		{
			CombineAssertions(() =>
			{
				var masterConsignment = Provider.MasterConsignment;
				AssertNotNull("Expected filled Master Consignment", masterConsignment);
				AssertEquals("Expected one Master Consignment", 1, masterConsignment.Count);
				AssertEquals("Expected filled Previous Document", "337", masterConsignment.FirstOrDefault().PreviousDocument.FirstOrDefault().Name);

				AssertSame("Cached Master Consignment", masterConsignment, Provider.MasterConsignment);
			});
		}

		protected override G3PresentGoodHeaderWrapper GetProviderCore()
		{
			return new G3PresentGoodHeaderWrapper(header.Bills, localReferenceNumber);
		}
	}
}
