using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public class TempStorageRegTransactionNewControlBag : ControlBag
{
	public static TempStorageRegTransactionNewControlBag Instance => tempStorageRegTransactionNewControlBag.Value;

	public TempStorageRegTransactionNewControlBag()
	{
		PhysicalInOutDateDateEdit = RegisterControl(nameof(TempStorageRegTransactionNewUserControl.PhysicalInOutDateDateEdit));
		TransactionDateDateEdit = RegisterControl(nameof(TempStorageRegTransactionNewUserControl.TransactionDateDateEdit));
		GrossWeightCalcEdit = RegisterControl(nameof(TempStorageRegTransactionNewUserControl.GrossWeightCalcEdit));
		PackageQtyCalcEdit = RegisterControl(nameof(TempStorageRegTransactionNewUserControl.PackageQtyCalcEdit));
		InternalReferenceTypeDropEdit = RegisterControl(nameof(TempStorageRegTransactionNewUserControl.InternalReferenceTypeDropEdit));
		InternalReferenceNumberTextBox = RegisterControl(nameof(TempStorageRegTransactionNewUserControl.InternalReferenceNumberTextBox));
		ReferenceTypeDropEdit = RegisterControl(nameof(TempStorageRegTransactionNewUserControl.ReferenceTypeDropEdit));
		ReferenceTextBox = RegisterControl(nameof(TempStorageRegTransactionNewUserControl.ReferenceTextBox));
		CommentsTextBox = RegisterControl(nameof(TempStorageRegTransactionNewUserControl.CommentsTextBox));
	}

	public ControlReference PhysicalInOutDateDateEdit { get; }

	public ControlReference TransactionDateDateEdit { get; }

	public ControlReference GrossWeightCalcEdit { get; }

	public ControlReference PackageQtyCalcEdit { get; }

	public ControlReference InternalReferenceTypeDropEdit { get; }

	public ControlReference InternalReferenceNumberTextBox { get; }

	public ControlReference ReferenceTypeDropEdit { get; }

	public ControlReference ReferenceTextBox { get; }

	public ControlReference CommentsTextBox { get; }

	protected override Control CreateTemplate() => new TempStorageRegTransactionNewUserControl();

	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	static readonly Lazy<TempStorageRegTransactionNewControlBag> tempStorageRegTransactionNewControlBag = new (() => new TempStorageRegTransactionNewControlBag());
}
