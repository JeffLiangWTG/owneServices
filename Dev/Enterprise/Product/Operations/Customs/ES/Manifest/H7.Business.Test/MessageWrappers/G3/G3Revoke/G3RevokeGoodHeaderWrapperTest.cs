using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(G3RevokeGoodHeaderWrapper))]
	public class G3RevokeGoodHeaderWrapperTest : G3CommonHeaderWrapperTest<G3RevokeGoodHeaderWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
#if NET
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
				"Value cannot be null. (Parameter 'bills')", () => new G3RevokeGoodHeaderWrapper(null, revokeReasonDictionary, "LRN"));

				AssertExceptionThrown("Throws Exception if revokeReasonDictionary is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'revokeReasonDictionary')", () => new G3RevokeGoodHeaderWrapper(header.Bills, null, "LRN"));

				AssertExceptionThrown("Throws Exception if localReferenceNumber is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'localReferenceNumber')", () => new G3RevokeGoodHeaderWrapper(header.Bills, revokeReasonDictionary, null));

				var bill = Factory.New<AsycudaBill>();
				AssertExceptionThrown("Throws Exception if bills[0].Header is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'bills[0].Header')", () => new G3RevokeGoodHeaderWrapper(new List<AsycudaBill> { bill }, revokeReasonDictionary, "LRN"));
#else
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
				"Value cannot be null.\r\nParameter name: bills", () => new G3RevokeGoodHeaderWrapper(null, revokeReasonDictionary, "LRN"));

				AssertExceptionThrown("Throws Exception if revokeReasonDictionary is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: revokeReasonDictionary", () => new G3RevokeGoodHeaderWrapper(header.Bills, null, "LRN"));

				AssertExceptionThrown("Throws Exception if localReferenceNumber is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: localReferenceNumber", () => new G3RevokeGoodHeaderWrapper(header.Bills, revokeReasonDictionary, null));

				var bill = Factory.New<AsycudaBill>();
				AssertExceptionThrown("Throws Exception if bills[0].Header is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: bills[0].Header", () => new G3RevokeGoodHeaderWrapper(new List<AsycudaBill> { bill }, revokeReasonDictionary, "LRN"));
#endif
			});
		}

		public void TestMasterConsignment()
		{
			CombineAssertions(() =>
			{
				var masterConsignment = Provider.MasterConsignment;
				AssertNotNull("Expected filled Master Consignment", masterConsignment);
				AssertEquals("Expected filled Master Consignment Items", 1, masterConsignment.Count);
				AssertEquals("Expected filled Previous Document Name", "337", masterConsignment.FirstOrDefault().PreviousDocument.FirstOrDefault().Name);

				AssertSame("Cached Master Consignment", masterConsignment, Provider.MasterConsignment);
			});
		}

		protected override G3RevokeGoodHeaderWrapper GetProviderCore()
		{
			revokeReasonDictionary = header.Bills.ToDictionary(o => o.PK, o => new DocumentCommonWrapper("G001", "337") as IDocumentsCommon);
			return new G3RevokeGoodHeaderWrapper(header.Bills, revokeReasonDictionary, localReferenceNumber);
		}

		IDictionary<ZGuid, IDocumentsCommon> revokeReasonDictionary;
	}
}
