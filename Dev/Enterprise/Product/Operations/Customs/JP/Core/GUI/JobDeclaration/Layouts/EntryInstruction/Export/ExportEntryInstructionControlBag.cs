using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class ExportEntryInstructionControlBag : ControlBag
	{
		public static ExportEntryInstructionControlBag Instance => instance ??= new ExportEntryInstructionControlBag();

		[ThreadStatic]
		static ExportEntryInstructionControlBag instance;

		ExportEntryInstructionControlBag()
		{
			AwbOrBillNumberTextBox = RegisterControl(nameof(AwbOrBillNumberTextBox));
			ExportControlNumberTextBox = RegisterControl(nameof(ExportControlNumberTextBox));
			GoodsDescriptionTextBox = RegisterControl(nameof(GoodsDescriptionTextBox));
			PreInspectedCargoDropEdit = RegisterControl(nameof(PreInspectedCargoDropEdit));
			LoadingConfirmationIsRequiredCheckBox = RegisterControl(nameof(LoadingConfirmationIsRequiredCheckBox));
			VanningLocationsGroupBox = RegisterControl(nameof(VanningLocationsGroupBox));
			DeclarationCargoTypeDropEdit = RegisterControl(nameof(DeclarationCargoTypeDropEdit));
		}

		protected override Control CreateTemplate() => new ExportEntryInstructionLayoutTemplate();

		public ControlReference AwbOrBillNumberTextBox { get; }
		public ControlReference ExportControlNumberTextBox { get; }
		public ControlReference GoodsDescriptionTextBox { get; }
		public ControlReference PreInspectedCargoDropEdit { get; }
		public ControlReference LoadingConfirmationIsRequiredCheckBox { get; }
		public ControlReference VanningLocationsGroupBox { get; }
		public ControlReference DeclarationCargoTypeDropEdit { get; }
	}
}
