using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocDocAddressCollection))]
	sealed class DocDocAddressCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocDocAddressCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orgAddress = Factory.New<OrgAddress>();
			return DocDocAddress.New(orgAddress, Factory);
		}

		protected override DocDocAddressCollection GetCollectionToTest()
		{
			return new DocDocAddressCollection(Factory);
		}

		public void TestGetAddressesWithWareHousing()
		{
			var add1 = Factory.New<OrgAddress>();
			var add2 = Factory.New<OrgAddress>();

			DocDocAddress docAdd1 = DocDocAddress.New(add1, Factory);
			DocDocAddress docAdd2 = DocDocAddress.New(add2, Factory);

			DocDocAddressCollection docAddressesOnCartageAdvice = new DocDocAddressCollection(Factory);
			docAddressesOnCartageAdvice.Add(docAdd1);
			docAddressesOnCartageAdvice.Add(docAdd2);

			AssertEquals("None", 0, docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing().Count);

			add1.OA_ForkLift = ZBool.True;
			AssertEquals("Collection should return 1", 1,
				docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing().Count);

			add2.OA_ForkLift = ZBool.True;
			AssertEquals("Collection should return 2", 2,
				docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing().Count);

			System.Collections.Hashtable hashtable = new System.Collections.Hashtable();

			foreach (DocDocAddress dAddress in docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing())
			{
				hashtable.Add(((BusinessObject)dAddress.WrappedObject).PK, (BusinessObject)dAddress.WrappedObject);
			}

			Assert("Add1 is in", hashtable.ContainsKey(((BusinessObject)docAdd1.WrappedObject).PK));
			Assert("Add2 is in", hashtable.ContainsKey(((BusinessObject)docAdd2.WrappedObject).PK));

			docAddressesOnCartageAdvice.Add(docAdd2);
			AssertEquals("still 2", 2, docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing().Count);
		}
	}
}
