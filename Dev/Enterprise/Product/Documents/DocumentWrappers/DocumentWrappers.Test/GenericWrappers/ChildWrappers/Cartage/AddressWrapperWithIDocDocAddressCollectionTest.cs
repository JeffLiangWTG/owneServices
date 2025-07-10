using System.Linq;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(AddressWrapperWithIDocDocAddressCollection))]
	sealed class AddressWrapperWithIDocDocAddressCollectionTest : Base.Testing.GenericWrapperCollectionTest<AddressWrapperWithIDocDocAddressCollection>
	{
		#region TestGetAddressesWithWarehousing

		public void TestGetAddressesWithWarehousing()
		{
			var addressWithWarehousing = Factory.New<OrgAddress>();
			var addressWithConstraints = Factory.New<OrgAddress>();
			var addressWithout = Factory.New<OrgAddress>();

			addressWithWarehousing.OA_Address1 = "Address With Warehousing";
			addressWithConstraints.OA_Address1 = "Address With Constraints";
			addressWithout.OA_Address1 = "Address Without";

			addressWithWarehousing.OA_DockLeveler = true;
			addressWithConstraints.OA_LoadingUnloadingConstraints = "yo";

			var addressWrappers = new AddressWrapperWithIDocDocAddressCollection(Factory);
			addressWrappers.Add(new AddressWrapperWithIDocDocAddress(addressWithWarehousing, ContactType.All, Factory));
			addressWrappers.Add(new AddressWrapperWithIDocDocAddress(addressWithConstraints, ContactType.All, Factory));
			addressWrappers.Add(new AddressWrapperWithIDocDocAddress(addressWithout, ContactType.All, Factory));

			AssertEquals("Should contain the 3 org Addresses", 3, addressWrappers.Count);
			AssertContainsExactElementsInAnyOrder(new[] { addressWithWarehousing, addressWithConstraints }, addressWrappers.GetAddressesWithWarehousing().Cast<AddressWrapperWithIDocDocAddress>().Select(a => a.WrappedObject));
		}

		#endregion

		#region Implementation

		protected override AddressWrapperWithIDocDocAddressCollection GetNewDocumentWrapperCollection()
		{
			return new AddressWrapperWithIDocDocAddressCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new AddressWrapperWithIDocDocAddress(null, ContactType.All, Factory);
		}

		#endregion
	}
}
