using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

sealed class TempStorageRegTransactionNewUserControlTest : TestCaseWithFactory
{
	public void TestPhysicalInOutDateDateEdit()
	{
		var physicalInOutDateDateEdit = control.PhysicalInOutDateDateEdit;
		CombineAssertions(() =>
		{
			AssertType<ZDateEdit>("Type", physicalInOutDateDateEdit);
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegLineTransaction.PhysicalInOutDate), physicalInOutDateDateEdit.GetBindingMember());
			AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Long, physicalInOutDateDateEdit.DateTimeFormat);
		});
	}

	public void TestTransactionDateDateEdit()
	{
		var transactionDateDateEdit = control.TransactionDateDateEdit;
		CombineAssertions(() =>
		{
			AssertType<ZDateEdit>("Type", transactionDateDateEdit);
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegLineTransaction.TransactionDate), transactionDateDateEdit.GetBindingMember());
			AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Long, transactionDateDateEdit.DateTimeFormat);
		});
	}

	public void TestGrossWeightCalcEdit()
	{
		var grossWeightCalcEdit = control.GrossWeightCalcEdit;
		CombineAssertions(() =>
		{
			AssertType<ZCalcEdit>("Type", grossWeightCalcEdit);
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegLineTransaction.SRT_GrossWeight), grossWeightCalcEdit.GetBindingMember());
			AssertEquals("DecimalPlaces", 5, grossWeightCalcEdit.DecimalPlaces);
		});
	}

	public void TestPackageQtyCalcEdit()
	{
		var packageQtyCalcEdit = control.PackageQtyCalcEdit;
		CombineAssertions(() =>
		{
			AssertType<ZCalcEdit>("Type", packageQtyCalcEdit);
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegLineTransaction.SRT_PackageQty), packageQtyCalcEdit.GetBindingMember());
		});
	}

	public void TestInternalReferenceTypeDropEdit()
	{
		var internalReferenceTypeDropEdit = control.InternalReferenceTypeDropEdit;
		CombineAssertions(() =>
		{
			AssertType<ZDropEdit>("Type", internalReferenceTypeDropEdit);
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegLineTransaction.SRT_InternalReferenceType), internalReferenceTypeDropEdit.GetBindingMember());
		});
	}

	public void TestInternalReferenceNumberTextBox()
	{
		var internalReferenceNumberTextBox = control.InternalReferenceNumberTextBox;
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Type", internalReferenceNumberTextBox);
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegLineTransaction.SRT_InternalReferenceNumber), internalReferenceNumberTextBox.GetBindingMember());
		});
	}

	public void TestReferenceTypeDropEdit()
	{
		var referenceTypeDropEdit = control.ReferenceTypeDropEdit;
		CombineAssertions(() =>
		{
			AssertType<ZDropEdit>("Type", referenceTypeDropEdit);
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegLineTransaction.SRT_ReferenceType), referenceTypeDropEdit.GetBindingMember());
		});
	}

	public void TestReferenceTextBox()
	{
		var referenceTextBox = control.ReferenceTextBox;
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Type", referenceTextBox);
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegLineTransaction.SRT_Reference), referenceTextBox.GetBindingMember());
		});
	}

	public void TestCommentsTextBox()
	{
		var commentsTextBox = control.CommentsTextBox;
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Type", commentsTextBox);
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegLineTransaction.SRT_Comments), commentsTextBox.GetBindingMember());
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new TempStorageRegTransactionNewUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	TempStorageRegTransactionNewUserControl control;
}
