using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class MiscellaneousOptionsControlBag : ControlBag
	{
		public MiscellaneousOptionsControlBag()
		{
			BranchCodeFindBox = RegisterControl(nameof(MiscellaneousOptionsUserControl.BranchCodeFindBox));
		}

		public static MiscellaneousOptionsControlBag Instance => instance ?? (instance = new MiscellaneousOptionsControlBag());

		[ThreadStatic]
		static MiscellaneousOptionsControlBag instance;

		public ControlReference BranchCodeFindBox { get; }

		protected override Control CreateTemplate() => new MiscellaneousOptionsUserControl();
	}
}
