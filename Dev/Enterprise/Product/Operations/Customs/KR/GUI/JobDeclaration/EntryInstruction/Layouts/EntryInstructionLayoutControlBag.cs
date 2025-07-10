using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class EntryInstructionLayoutControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new EntryInstructionLayoutUserControl();

		[ThreadStatic]
		static EntryInstructionLayoutControlBag instance;

		public static EntryInstructionLayoutControlBag Instance => instance ??= new EntryInstructionLayoutControlBag();

		EntryInstructionLayoutControlBag()
		{
			TotalPackagesCalcEdit = RegisterControl(nameof(EntryInstructionLayoutUserControl.TotalPackagesCalcEdit));
			TotalPackagesUQDropEdit = RegisterControl(nameof(EntryInstructionLayoutUserControl.TotalPackagesUQDropEdit));
			AgreedRateDropEdit = RegisterControl(nameof(EntryInstructionLayoutUserControl.AgreedRateDropEdit));
			UseTypeDropEdit = RegisterControl(nameof(EntryInstructionLayoutUserControl.UseTypeDropEdit));
			UseDateEdit = RegisterControl(nameof(EntryInstructionLayoutUserControl.UseDateEdit));
		}

		public ControlReference TotalPackagesCalcEdit { get; }
		public ControlReference TotalPackagesUQDropEdit { get; }
		public ControlReference AgreedRateDropEdit { get; }
		public ControlReference UseTypeDropEdit { get; }
		public ControlReference UseDateEdit { get; }
	}
}
