using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNOrgSupplierBuyerLinkAddInfo))]
	class CNOrgSupplierBuyerLinkAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			return new CNOrgSupplierBuyerLinkAddInfo(link.GetAddInfo());
		}

		public void TestIsDeleted()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			var addInfo = link.GetAddInfo();
			var testItem = new CNOrgSupplierBuyerLinkAddInfo(addInfo);
			AssertEquals("New created CNOrgSupplierBuyerLinkAddInfo, IsDeleted shows false.", false, testItem.IsDeleted);

			link.Delete();
			AssertEquals("CNOrgSupplierBuyerLinkAddInfo should show IsDeleted when parent OrgSupplierBuyerLink is deleted.", true, testItem.IsDeleted);
		}
	}
}
