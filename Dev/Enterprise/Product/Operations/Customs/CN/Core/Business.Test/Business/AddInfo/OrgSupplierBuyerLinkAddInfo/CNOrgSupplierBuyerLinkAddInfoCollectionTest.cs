using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNOrgSupplierBuyerLinkAddInfoCollection))]
	class CNOrgSupplierBuyerLinkAddInfoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CNOrgSupplierBuyerLinkAddInfoCollection>
	{
		public override void TestAdd()
		{
			var collection = new CNOrgSupplierBuyerLinkAddInfoCollection(Factory);
			collection.Add(GetNewElementToAddToTheCollection());
			AssertEquals(1, collection.Count);
		}

		public override void TestAddNew()
		{
			var collection = new CNOrgSupplierBuyerLinkAddInfoCollection(Factory);
			collection.Add(GetNewElementToAddToTheCollection());
			AssertEquals(1, collection.Count);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			var collection = new CNOrgSupplierBuyerLinkAddInfoCollection(Factory);
			collection.Add(GetNewElementToAddToTheCollection());
			AssertEquals(1, collection.Count);
		}

		protected override CNOrgSupplierBuyerLinkAddInfoCollection GetCollectionToTest() => new CNOrgSupplierBuyerLinkAddInfoCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.China;
			return new CNOrgSupplierBuyerLinkAddInfo(link.AddInfo as OrgSupplierBuyerLinkAddInfo);
		}
	}
}
