using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class RCREntryInstructionControlBag : ControlBag
	{
		public static RCREntryInstructionControlBag Instance => instance ??= new RCREntryInstructionControlBag();

		[ThreadStatic]
		static RCREntryInstructionControlBag instance;

		RCREntryInstructionControlBag()
		{
			RCRActionDropEdit = RegisterControl(nameof(RCRActionDropEdit));
			PreviousBillNumberTextBox = RegisterControl(nameof(PreviousBillNumberTextBox));
			ViaLocationCodeFindBox = RegisterControl(nameof(ViaLocationCodeFindBox));
		}

		protected override Control CreateTemplate() => new RCREntryInstructionLayoutTemplate();

		public ControlReference RCRActionDropEdit { get; }
		public ControlReference PreviousBillNumberTextBox { get; }
		public ControlReference ViaLocationCodeFindBox { get; }
	}
}
