using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	partial class TempStorageRegisterHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestDDTNumberUserControl()
		{
			var ddtNumberUserControl = control.DDTNumberUserControl;
			CombineAssertions(() =>
			{
				AssertType<DDTNumberUserControl>("Type", ddtNumberUserControl);
				AssertEquals("GetBindingMember", ".", ddtNumberUserControl.GetBindingMember());
			});
		}

		public void TestArrivalDateEdit()
		{
			var arrivalDateEdit = control.ArrivalDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Type", arrivalDateEdit);
				AssertEquals("GetBindingMember", nameof(CusTempStorageRegHeader.SRH_ArrivalDate), arrivalDateEdit.GetBindingMember());
			});
		}

		public void TestPresentationDateEdit()
		{
			var presentationDateEdit = control.PresentationDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Type", presentationDateEdit);
				AssertEquals("GetBindingMember", nameof(CusTempStorageRegHeader.SRH_PresentationDate), presentationDateEdit.GetBindingMember());
			});
		}

		public void TestPreviousReferenceTypeDropEdit()
		{
			var previousReferenceTypeDropEdit = control.PreviousReferenceTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", previousReferenceTypeDropEdit);
				AssertEquals("GetBindingMember", nameof(CusTempStorageRegHeader.SRH_PreviousReferenceType), previousReferenceTypeDropEdit.GetBindingMember());
			});
		}

		public void TestPreviousReferenceNumberTextBox()
		{
			var previousReferenceNumberTextBox = control.PreviousReferenceNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", previousReferenceNumberTextBox);
				AssertEquals("GetBindingMember", nameof(CusTempStorageRegHeader.SRH_PreviousReference), previousReferenceNumberTextBox.GetBindingMember());
			});
		}

		public void TestStatusDropEdit()
		{
			var statusDropEdit = control.StatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", statusDropEdit);
				AssertEquals("GetBindingMember", nameof(CusTempStorageRegHeader.SRH_Status), statusDropEdit.GetBindingMember());
			});
		}

		public void TestInternalReferenceTextBox()
		{
			var internalReferenceTextBox = control.InternalReferenceTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", internalReferenceTextBox);
				AssertEquals("GetBindingMember", nameof(CusTempStorageRegHeader.SRH_InternalReference), internalReferenceTextBox.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TempStorageRegisterHeaderUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TempStorageRegisterHeaderUserControl control;
	}
}
