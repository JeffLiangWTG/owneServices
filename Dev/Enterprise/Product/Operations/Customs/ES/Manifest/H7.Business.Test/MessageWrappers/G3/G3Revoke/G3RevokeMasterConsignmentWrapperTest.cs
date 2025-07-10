using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(G3RevokeMasterConsignmentWrapper))]
	public class G3RevokeMasterConsignmentWrapperTest : G3MasterConsignmentWrapperTest<G3RevokeMasterConsignmentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
#if NET
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
				"Value cannot be null. (Parameter 'bills')", () => new G3RevokeMasterConsignmentWrapper(null, revokeReasonDictionary));

				AssertExceptionThrown("Throws Exception if revokeReasonDictionary is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'revokeReasonDictionary')", () => new G3RevokeMasterConsignmentWrapper(header.Bills, null));

				var bill = Factory.New<AsycudaBill>();
				AssertExceptionThrown("Throws Exception if bills[0].Header is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'bills[0].Header')", () => new G3RevokeMasterConsignmentWrapper(new List<AsycudaBill> { bill }, revokeReasonDictionary));
#else
				AssertExceptionThrown("Throws Exception if bills is null", typeof(ArgumentNullException),
				"Value cannot be null.\r\nParameter name: bills", () => new G3RevokeMasterConsignmentWrapper(null, revokeReasonDictionary));

				AssertExceptionThrown("Throws Exception if revokeReasonDictionary is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: revokeReasonDictionary", () => new G3RevokeMasterConsignmentWrapper(header.Bills, null));

				var bill = Factory.New<AsycudaBill>();
				AssertExceptionThrown("Throws Exception if bills[0].Header is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: bills[0].Header", () => new G3RevokeMasterConsignmentWrapper(new List<AsycudaBill> { bill }, revokeReasonDictionary));
#endif
			});
		}

		public void TestHouseConsignment()
		{
			var houseConsignment = Provider.HouseConsignment;
			var houseConsignmentArray = houseConsignment.ToArray();

			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled House Consignment", houseConsignment);
				AssertEquals("Expected filled House Consignment items", 2, houseConsignmentArray.Length);

				AssertEquals("Expected filled Transport Document Name", "5025", houseConsignmentArray[0].TransportDocument.Name);
				AssertEquals("Expected filled Revoke Reason Code for bill 1", "G3001", houseConsignmentArray[0].AdditionalInformation.Number);
				AssertEquals("Expected filled Revoke Reason Text for bill 1", "Reason 1", houseConsignmentArray[0].AdditionalInformation.Name);

				AssertEquals("Expected filled Revoke Reason Code for bill 2", "G3002", houseConsignmentArray[1].AdditionalInformation.Number);
				AssertEquals("Expected filled Revoke Reason Text for bill 2", "Reason 2", houseConsignmentArray[1].AdditionalInformation.Name);

				AssertSame("Cached House Consignment", houseConsignment, Provider.HouseConsignment);

				var wrapper = new G3RevokeMasterConsignmentWrapper(header.Bills, new Dictionary<ZGuid, IDocumentsCommon>());
				AssertExceptionThrown<ArgumentNullException>("Throw Exception if any bill has no Revoke reason", () => { _ = wrapper.HouseConsignment; });
			});
		}

		public override void TestPreviousDocument()
		{
			var previousDocument = Provider.PreviousDocument;

			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Previous Document", previousDocument);
				AssertEquals("Expected two filled Previous Document items", 2, previousDocument.Count);
				AssertEquals("Expected filled Previous Document 1", "337", Provider.PreviousDocument.FirstOrDefault().Name);
				AssertEquals("Expected filled Previous Document 2", "MRN", Provider.PreviousDocument.LastOrDefault().Name);

				AssertSame("Cached Previous Document", previousDocument, Provider.PreviousDocument);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			revokeReasonDictionary = new Dictionary<ZGuid, IDocumentsCommon>()
			{
				{ header.Bills.First().PK, new DocumentCommonWrapper("Reason 1", "G3001") },
				{ header.Bills.Last().PK, new DocumentCommonWrapper("Reason 2", "G3002") }
			};
		}

		protected override G3RevokeMasterConsignmentWrapper GetProviderCore()
		{
			return new G3RevokeMasterConsignmentWrapper(header.Bills, revokeReasonDictionary);
		}

		Dictionary<ZGuid, IDocumentsCommon> revokeReasonDictionary;
	}
}
