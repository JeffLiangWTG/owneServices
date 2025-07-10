using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(MessageChooserItem))]
	sealed class MessageChooserItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDescription()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bill1";
			var billCountry = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
			billCountry.CycleDate = new ZDateTime(2018, 6, 7);
			billCountry.CycleNumber = "2";
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			var chooserItem = chooser.ChooserItems[0];
			AssertEquals("Bill Number - Bill1 - Cycle: 07-Jun-18/2 - Original", chooserItem.Description);

			chooser = new MessageChooser(header, new ISelectionItem[] { bill }, false);
			chooserItem = chooser.ChooserItems[0];
			AssertEquals("Bill Number - Bill1", chooserItem.Description);
		}

		public void TestBizO()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			var chooserItem = chooser.ChooserItems[0];
			AssertEquals(bill, chooserItem.BizO);
		}

		public void TestBizoHasMessageErrors()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			var chooserItem = chooser.ChooserItems[0];

			Assert(!chooserItem.BizoHasMessageErrors);
			bill.Validation.ValidateAll();
			Assert(chooserItem.BizoHasMessageErrors);
		}

		public void TestChooser()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			var chooserItem = chooser.ChooserItems[0];
			AssertEquals(chooser, chooserItem.Chooser);
		}

		public void TestBill()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			var chooserItem = chooser.ChooserItems[0];
			AssertEquals(bill, chooserItem.Bill);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			return chooser.ChooserItems[0];
		}
	}
}
