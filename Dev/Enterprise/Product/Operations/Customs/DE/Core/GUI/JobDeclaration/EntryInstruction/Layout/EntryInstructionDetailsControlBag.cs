using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class EntryInstructionDetailsControlBag : ControlBag
	{
		EntryInstructionDetailsControlBag()
		{
			StyleDropEdit = RegisterControl(nameof(EntryInstructionTopDetailsUserControl.StyleDropEdit));
			SubStyleDropEdit = RegisterControl(nameof(EntryInstructionTopDetailsUserControl.SubStyleDropEdit));
			DescriptionTextBox = RegisterControl(nameof(EntryInstructionTopDetailsUserControl.DescriptionTextBox));
			AdditionalInfoTextBox = RegisterControl(nameof(EntryInstructionTopDetailsUserControl.AdditionalInfoTextBox));
			CPCDropEdit = RegisterControl(nameof(EntryInstructionTopDetailsUserControl.CPCDropEdit));
			DateForDutyDateEdit = RegisterControl(nameof(EntryInstructionTopDetailsUserControl.DateForDutyDateEdit));
			ExitDateDateEdit = RegisterControl(nameof(EntryInstructionTopDetailsUserControl.ExitDateDateEdit));
			AuthorisationNumberDropEdit = RegisterControl(nameof(EntryInstructionTopDetailsUserControl.AuthorisationNumberDropEdit));
			PartyConstellationDropEdit = RegisterControl(nameof(EntryInstructionTopDetailsUserControl.PartyConstellationDropEdit));
		}

		public static EntryInstructionDetailsControlBag Instance => instance ?? (instance = new EntryInstructionDetailsControlBag());

		[ThreadStatic]
		static EntryInstructionDetailsControlBag instance;

		protected override Control CreateTemplate() => new EntryInstructionTopDetailsUserControl();

		public ControlReference StyleDropEdit { get; }

		public ControlReference SubStyleDropEdit { get; }

		public ControlReference DescriptionTextBox { get; }

		public ControlReference AdditionalInfoTextBox { get; }

		public ControlReference CPCDropEdit { get; }

		public ControlReference DateForDutyDateEdit { get; }

		public ControlReference ExitDateDateEdit { get; }

		public ControlReference AuthorisationNumberDropEdit { get; }

		public ControlReference PartyConstellationDropEdit { get; }
	}
}
