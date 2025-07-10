using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class EntryInstructionBasicDetailsControlTest : TestCase
	{
		public void TestToWarehouseAddressControl()
		{
			AssertType<ZAddressControl>(control.ToWarehouseAddressControl);
		}

		public void TestFromWarehouseAddressControl()
		{
			AssertType<ZAddressControl>(control.FromWarehouseAddressControl);
		}

		public void TestToWarehouseControl()
		{
			AssertType<ToWarehouseUserControl>(control.ToWarehouseUserControl);
		}

		public void TestFromWarehouseControl()
		{
			AssertType<FromWarehouseUserControl>(control.FromWarehouseUserControl);
		}

		public void TestToWarehouseLabel()
		{
			AssertType<ZLabel>(control.ToWarehouseLabel);
		}

		public void TestFromWarehouseLabel()
		{
			AssertType<ZLabel>(control.FromWarehouseLabel);
		}

		public void TestLocationOfGoodsUserControl()
		{
			AssertType<LocationOfGoodsUserControl>(control.LocationOfGoodsUserControl);
		}

		public void TestToWarehouseTypeTextBox()
		{
			AssertType<ZTextBox>(control.ToWarehouseTypeTextBox);
		}

		public void TestToWarehouseCodeTextBox()
		{
			AssertType<ZTextBox>(control.ToWarehouseCodeTextBox);
		}

		public void TestFromWarehouseTypeTextBox()
		{
			AssertType<ZTextBox>(control.FromWarehouseTypeTextBox);
		}

		public void TestFromWarehouseCodeTextBox()
		{
			AssertType<ZTextBox>(control.FromWarehouseCodeTextBox);
		}

		public void TestNewOwnerOrganisationControl()
		{
			AssertType<MasterFiles.GUI.ZOrganisationControl>(control.NewOwnerOrganisationControl);
		}

		public void TestAcceptanceDateEdit()
		{
			AssertType<ZDateEdit>(control.AcceptanceDateEdit);
		}

		public void TestRequestedDocumentsGroupBox()
		{
			AssertType<ZGroupBox>(control.RequestedDocumentsGroupBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new EntryInstructionBasicDetailsControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		EntryInstructionBasicDetailsControl control;
	}
}
