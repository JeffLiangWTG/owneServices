using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class NctsPackageControlBag : ControlBag
	{
		NctsPackageControlBag()
		{
			SequenceNumberCalcEdit = RegisterControl(nameof(NctsPackageUserControl.SequenceNumberCalcEdit));
			DeclaredValueLabel = RegisterControl(nameof(NctsPackageUserControl.DeclaredValueLabel));
			UnitCountCalcEdit = RegisterControl(nameof(NctsPackageUserControl.UnitCountCalcEdit));
			UnitTypeDropEdit = RegisterControl(nameof(NctsPackageUserControl.UnitTypeDropEdit));
			MarksAndNumbersTextBox = RegisterControl(nameof(NctsPackageUserControl.MarksAndNumbersTextBox));
			PackageIDTextBox = RegisterControl(nameof(NctsPackageUserControl.PackageIDTextBox));
			BrandTextBox = RegisterControl(nameof(NctsPackageUserControl.BrandTextBox));
			ModelTextBox = RegisterControl(nameof(NctsPackageUserControl.ModelTextBox));
			UnloadedValueLabel = RegisterControl(nameof(NctsPackageUserControl.UnloadedValueLabel));
			DifUnitCountCalcEdit = RegisterControl(nameof(NctsPackageUserControl.DifUnitCountCalcEdit));
			DifUnitTypeDropEdit = RegisterControl(nameof(NctsPackageUserControl.DifUnitTypeDropEdit));
			DifMarksAndNumbersTextBox = RegisterControl(nameof(NctsPackageUserControl.DifMarksAndNumbersTextBox));
			DifPackageIDTextBox = RegisterControl(nameof(NctsPackageUserControl.DifPackageIDTextBox));
			DifBrandTextBox = RegisterControl(nameof(NctsPackageUserControl.DifBrandTextBox));
			DifModelTextBox = RegisterControl(nameof(NctsPackageUserControl.DifModelTextBox));
			PlaceHolder1Label = RegisterControl(nameof(NctsPackageUserControl.PlaceHolder1Label));
		}

		public static NctsPackageControlBag Instance => instance ?? (instance = new NctsPackageControlBag());

		[ThreadStatic]
		static NctsPackageControlBag instance;

		public ControlReference SequenceNumberCalcEdit { get; }

		public ControlReference DeclaredValueLabel { get; }

		public ControlReference UnitCountCalcEdit { get; }

		public ControlReference UnitTypeDropEdit { get; }

		public ControlReference MarksAndNumbersTextBox { get; }

		public ControlReference PackageIDTextBox { get; }

		public ControlReference BrandTextBox { get; }

		public ControlReference ModelTextBox { get; }

		public ControlReference DifSequenceNumberCalcEdit { get; }

		public ControlReference UnloadedValueLabel { get; }

		public ControlReference DifUnitCountCalcEdit { get; }

		public ControlReference DifUnitTypeDropEdit { get; }

		public ControlReference DifMarksAndNumbersTextBox { get; }

		public ControlReference DifPackageIDTextBox { get; }

		public ControlReference DifBrandTextBox { get; }

		public ControlReference DifModelTextBox { get; }

		public ControlReference PlaceHolder1Label { get; }

		protected override Control CreateTemplate() => new NctsPackageUserControl();
	}
}
