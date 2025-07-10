using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CAeMHDocAddress))]
	sealed class CAeMHDocAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetAddressTypeList()
		{
			var addressTypes = CAeMHDocAddress.GetAddressTypeList(Factory);
			AssertEquals(10, addressTypes.Count);
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.ConsigneeDocumentaryAddress));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.ConsignorDocumentaryAddress));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.ConsigneePickupDeliveryAddress));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.NotifyParty));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.ImportBroker));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.ReceivingForwarderAddress));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.Carrier));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.Warehouse));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.PlaceOfConsolidation));
			Assert(addressTypes.ContainsCode(DocAddressTypes.Codes.Consolidator));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CAeMHDocAddress>();
		}

		#endregion
	}
}
