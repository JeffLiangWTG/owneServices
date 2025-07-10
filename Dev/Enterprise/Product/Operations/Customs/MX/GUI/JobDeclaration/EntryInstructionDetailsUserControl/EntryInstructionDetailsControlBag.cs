using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public sealed class EntryInstructionDetailsControlBag : ControlBag
	{
		public EntryInstructionDetailsControlBag()
		{
			UCRNumberTextBox = RegisterControl(nameof(EntryInstructionDetailsPanelUserControl.UCRNumberTextBox));
		}

		public static EntryInstructionDetailsControlBag Instance => instance ?? (instance = new EntryInstructionDetailsControlBag());

		[ThreadStatic]
		static EntryInstructionDetailsControlBag instance;

		protected override Control CreateTemplate() => new EntryInstructionDetailsPanelUserControl();

		public ControlReference UCRNumberTextBox { get; }
	}
}
