using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocAddressCollection))]
	sealed class DocAddressCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocAddressCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orgAddress = Factory.New<OrgAddress>();
			return DocAddress.New(orgAddress, Factory);
		}

		protected override DocAddressCollection GetCollectionToTest()
		{
			return new DocAddressCollection(Factory);
		}

		public void TestGetAddressesWithWareHousing()
		{
			var add1 = Factory.New<OrgAddress>();
			var add2 = Factory.New<OrgAddress>();

			DocAddress docAdd1 = DocAddress.New(add1, Factory);
			DocAddress docAdd2 = DocAddress.New(add2, Factory);

			DocAddressCollection addressesOnCartageAdvice = new DocAddressCollection(Factory);
			addressesOnCartageAdvice.Add(docAdd1);
			addressesOnCartageAdvice.Add(docAdd2);

			AssertEquals("None", 0, addressesOnCartageAdvice.GetAddressesWithWareHousing().Count);

			add1.OA_ForkLift = ZBool.True;
			AssertEquals("Collection should return 1", 1, addressesOnCartageAdvice.GetAddressesWithWareHousing().Count);

			add2.OA_ForkLift = ZBool.True;
			AssertEquals("Collection should return 2", 2, addressesOnCartageAdvice.GetAddressesWithWareHousing().Count);

			System.Collections.Hashtable hashtable = new System.Collections.Hashtable();

			foreach (DocAddress dAddress in addressesOnCartageAdvice.GetAddressesWithWareHousing())
			{
				hashtable.Add(((BusinessObject)dAddress.WrappedObject).PK, (BusinessObject)dAddress.WrappedObject);
			}
			Assert("Add1 is in", hashtable.ContainsKey(add1.PK));
			Assert("Add2 is in", hashtable.ContainsKey(add2.PK));

			addressesOnCartageAdvice.Add(docAdd2);
			AssertEquals("still 2", 2, addressesOnCartageAdvice.GetAddressesWithWareHousing().Count);
		}
	}
}
