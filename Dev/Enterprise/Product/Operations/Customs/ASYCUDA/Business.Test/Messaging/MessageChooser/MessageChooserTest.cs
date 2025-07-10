using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(MessageChooser))]
	sealed class MessageChooserTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetSelectItems()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			AssertEquals(2, chooser.ChooserItems.Count);

			var chooserItem = chooser.ChooserItems[0];
			chooserItem.Checked = true;
			chooser.ChooserItems[1].Checked = false;

			AssertContainsExactElementsInAnyOrder(new[] { chooserItem.BizO }, chooser.GetSelectedItems());
		}

		public void TestGetSelectedMessageChooserItems()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			AssertEquals(2, chooser.ChooserItems.Count);

			var chooserItem = chooser.ChooserItems[0];
			chooserItem.Checked = true;
			chooser.ChooserItems[1].Checked = false;

			AssertContainsExactElementsInAnyOrder(new[] { chooserItem }, chooser.GetSelectedMessageChooserItems());
		}

		public void TestAllItemsSelected_IsExport()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGE";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			AssertEquals(2, chooser.ChooserItems.Count);

			foreach (MessageChooserItem chooserItem in chooser.ChooserItems)
			{
				AssertEquals(true, chooserItem.Checked);
			}
		}

		public void TestAllItemsSelected_IsNotSGImport_NotSG()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			AssertEquals(2, chooser.ChooserItems.Count);

			foreach (MessageChooserItem chooserItem in chooser.ChooserItems)
			{
				AssertEquals(true, chooserItem.Checked);
			}
		}

		public void TestSelectAll()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			AssertEquals(2, chooser.ChooserItems.Count);

			chooser.SelectAll();
			foreach (MessageChooserItem chooserItem in chooser.ChooserItems)
			{
				AssertEquals(true, chooserItem.Checked);
			}
		}

		public void TestDeSelectAll()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			AssertEquals(2, chooser.ChooserItems.Count);

			chooser.SelectAll();
			foreach (MessageChooserItem chooserItem in chooser.ChooserItems)
			{
				AssertEquals(true, chooserItem.Checked);
			}

			chooser.DeSelectAll();
			foreach (MessageChooserItem chooserItem in chooser.ChooserItems)
			{
				AssertEquals(false, chooserItem.Checked);
			}
		}

		public void TestChooserItems()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill1 = header.Bills.AddNew();
			var bill1SG = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill1;
			bill1SG.CycleDate = new ZDateTime(2018, 6, 6);
			bill1SG.CycleNumber = "6";
			var bill2 = header.Bills.AddNew();
			var bill2SG = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill2;
			bill2SG.CycleDate = new ZDateTime(2018, 7, 7);
			bill2SG.CycleNumber = "6";

			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			AssertEquals(2, chooser.ChooserItems.Count);
		}

		public void TestSelectedCount()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			bill3.Validation.ValidateAll();

			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2, bill3 }, true);

			Assert(bill3.HasMessageErrors);
			AssertEquals(3, chooser.ChooserItems.Count);
			AssertEquals("bill3 with message errors is not selected by default", 2, chooser.SelectedCount);

			chooser.SelectAll();

			AssertEquals(3, chooser.SelectedCount);
		}

		public void TestSelectedDescription()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			AssertEquals(2, chooser.ChooserItems.Count);

			chooser.SelectAll();

			AssertEquals("2 of 2 bill(s) selected.", chooser.SelectedDescription);
		}

		public void TestLookups()
		{
			var messageChooser = (MessageChooser)GetNewBusinessObject();
			AssertType<MessageChooserLookups>(messageChooser.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			return new MessageChooser(header, new ISelectionItem[] { bill }, true);
		}
	}
}
