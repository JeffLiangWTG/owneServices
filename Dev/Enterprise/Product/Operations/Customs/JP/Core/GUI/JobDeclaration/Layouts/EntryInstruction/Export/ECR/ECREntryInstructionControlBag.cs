using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class ECREntryInstructionControlBag : ControlBag
	{
		public static ECREntryInstructionControlBag Instance => instance ??= new ECREntryInstructionControlBag();

		[ThreadStatic]
		static ECREntryInstructionControlBag instance;

		ECREntryInstructionControlBag()
		{
			CusEntryInstructionNSITextBox = RegisterControl(nameof(CusEntryInstructionNSITextBox));
			ECRNotesTextBox = RegisterControl(nameof(ECRNotesTextBox));
			ECRCargoTypeDropEdit = RegisterControl(nameof(ECRCargoTypeDropEdit));
			VolumeCalcDropEdit = RegisterControl(nameof(VolumeCalcDropEdit));
			CustomsVolumeCalcDropEdit = RegisterControl(nameof(CustomsVolumeCalcDropEdit));
			SpecialCargoCodeFindBox = RegisterControl(nameof(SpecialCargoCodeFindBox));
		}

		protected override Control CreateTemplate() => new ECREntryInstructionLayoutTemplate();

		public ControlReference ECRNotesTextBox { get; }
		public ControlReference ECRCargoTypeDropEdit { get; }
		public ControlReference VolumeCalcDropEdit { get; }
		public ControlReference CustomsVolumeCalcDropEdit { get; }
		public ControlReference SpecialCargoCodeFindBox { get; }
		public ControlReference CusEntryInstructionNSITextBox { get; }
	}
}
