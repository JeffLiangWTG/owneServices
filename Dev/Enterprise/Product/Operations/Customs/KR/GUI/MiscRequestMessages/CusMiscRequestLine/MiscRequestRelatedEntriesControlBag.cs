using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class MiscRequestRelatedEntriesControlBag : ControlBag
	{
		MiscRequestRelatedEntriesControlBag()
		{
			EntryTypeDropEdit = RegisterControl(nameof(MiscRequestRelatedEntriesUserControl.EntryTypeDropEdit));
			EntryNumberTextBox = RegisterControl(nameof(MiscRequestRelatedEntriesUserControl.EntryNumberTextBox));
			EntryDetailsTextBox = RegisterControl(nameof(MiscRequestRelatedEntriesUserControl.EntryDetailsTextBox));
		}

		public static MiscRequestRelatedEntriesControlBag Instance => instance ?? (instance = new MiscRequestRelatedEntriesControlBag());

		[ThreadStatic]
		static MiscRequestRelatedEntriesControlBag instance;

		protected override Control CreateTemplate() => new MiscRequestRelatedEntriesUserControl();

		public ControlReference EntryTypeDropEdit { get; }
		public ControlReference EntryNumberTextBox { get; }
		public ControlReference EntryDetailsTextBox { get; }
	}
}
