using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	[TestedType(typeof(RNSParentConsolWrapper))]
	sealed class RNSParentConsolWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetRNSRequestCollections()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var entryNumber1 = shipment1.Numbers.AddNew();
			entryNumber1.CE_EntryNum = "111";
			entryNumber1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			var shipment2 = consol.Shipments.AddNew();
			var entryNumber2 = shipment2.Numbers.AddNew();
			entryNumber2.CE_EntryNum = "222";
			entryNumber2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			var wrapper = RNSParentConsolWrapper.Load(consol);
			AssertSame("The ParentBusinessObject should be the consol", consol, wrapper.ParentBusinessObject);

			var rnsRequestCollections = wrapper.GetRNSRequestCollections();
			var rnsRequest1 = rnsRequestCollections.First(rnsRequest => rnsRequest.TopLevelBusinessObject == shipment1);
			var rnsRequest2 = rnsRequestCollections.First(rnsRequest => rnsRequest.TopLevelBusinessObject == shipment2);

			AssertNotNull("A RNS request should been created for shipment1", rnsRequest1);
			AssertEquals("The CargoControlNumber should be the CCN of shipment1", "111", rnsRequest1.CargoControlNumber);
			AssertNotNull("A RNS request should been created for shipment2", rnsRequest2);
			AssertEquals("The CargoControlNumber should be the CCN of shipment2", "222", rnsRequest2.CargoControlNumber);
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return RNSParentConsolWrapper.Load(Factory.New<ForwardingConsol>());
		}

		#endregion
	}
}
