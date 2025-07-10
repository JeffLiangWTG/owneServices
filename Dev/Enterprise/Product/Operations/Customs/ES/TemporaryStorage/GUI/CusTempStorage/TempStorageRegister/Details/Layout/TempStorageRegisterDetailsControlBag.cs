using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public sealed class TempStorageRegisterDetailsControlBag : ControlBag
	{
		public static TempStorageRegisterDetailsControlBag Instance => tempStorageRegisterDetailsControlBag.Value;

		public TempStorageRegisterDetailsControlBag()
		{
			GrossWeightUQTextBox = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.GrossWeightUQTextBox));
			GoodsOwnerIdentifierTextBox = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.GoodsOwnerIdentifierTextBox));
			PackageMarksTextBox = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.PackageMarksTextBox));
			UnionStatusDropEdit = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.UnionStatusDropEdit));
			GrossWeightRemainingCalcEdit = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.GrossWeightRemainingCalcEdit));
			PackagesRemainingCalcEdit = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.PackagesRemainingCalcEdit));
			LineNumberCalcEdit = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.LineNumberCalcEdit));
			PackageTypeDropEdit = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.PackageTypeDropEdit));
			LimitDateEdit = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.LimitDateEdit));
			GoodsDescriptionTextBox = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.GoodsDescriptionTextBox));
			LocationOfGoodsTextBox = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.LocationOfGoodsTextBox));
			OwnerReferenceNumberTextBox = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.OwnerReferenceNumberTextBox));
			BondAmountRemainingCalculatedCalcEdit = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.BondAmountRemainingCalculatedCalcEdit));
			CustomsStatusDropEdit = RegisterControl(nameof(TempStorageRegisterDetailsUserControl.CustomsStatusDropEdit));
		}

		public ControlReference GrossWeightUQTextBox { get; }

		public ControlReference GoodsOwnerIdentifierTextBox { get; }

		public ControlReference PackageMarksTextBox { get; }

		public ControlReference UnionStatusDropEdit { get; }

		public ControlReference GrossWeightRemainingCalcEdit { get; }

		public ControlReference PackagesRemainingCalcEdit { get; }

		public ControlReference LineNumberCalcEdit { get; }

		public ControlReference PackageTypeDropEdit { get; }

		public ControlReference LimitDateEdit { get; }

		public ControlReference GoodsDescriptionTextBox { get; }

		public ControlReference LocationOfGoodsTextBox { get; }

		public ControlReference OwnerReferenceNumberTextBox { get; }

		public ControlReference BondAmountRemainingCalculatedCalcEdit { get; }

		public ControlReference CustomsStatusDropEdit { get; }

		protected override Control CreateTemplate() => new TempStorageRegisterDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TempStorageRegisterDetailsControlBag> tempStorageRegisterDetailsControlBag = new (() => new TempStorageRegisterDetailsControlBag());
	}
}
