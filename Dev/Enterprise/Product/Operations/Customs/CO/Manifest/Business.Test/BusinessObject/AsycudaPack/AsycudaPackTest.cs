using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.COManifest.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaPack>(bizObj.PK).GetType());
		}

		public void TestBill()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaBill>(pack.Bill);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			return pack;
		}

		public void TestContactName()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Test OrgHeader";
			orgHeader.OH_Code = "Test";

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_ContactName = "Test Contact Name";
			orgContact.OC_Phone = "Test Contact Phone";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			pack.UNDGs.FirstItemForBinding[0].DI_OC_DGContact = orgContact.PK;
			AssertEquals("Contack name", orgContact.PK, pack.ContactPK);
		}

		public void TestGetWarningBeforeBeingDeletedForSentBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Accepted;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB1";

			var pack = bill1.Packs.AddNew();

			AssertEquals("This Bill is already sent to Customs.", pack.GetWarningBeforeBeingDeleted());

			header.AMA_MessageStatus = MessageStatusCodeList.Codes.NotSent;
			AssertNotEquals("This Bill is already sent to Customs.", pack.GetWarningBeforeBeingDeleted());
		}

		public void TestIsHazardous()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var pack = bill1.Packs.AddNew();

			Assert("IsHazardous", !pack.IsHazardous);

			pack.IsHazardous = true;
			Assert("IsHazardous", pack.IsHazardous);
		}
	}
}
