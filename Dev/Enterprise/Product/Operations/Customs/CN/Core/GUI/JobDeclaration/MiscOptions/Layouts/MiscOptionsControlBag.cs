using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public sealed class MiscOptionsControlBag : ControlBag
	{
		public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

		[ThreadStatic]
		static MiscOptionsControlBag instance;

		protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();

		MiscOptionsControlBag()
		{
			MoreMergeOptionsSeparatorUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.MoreMergeOptionsSeparatorUserControl));
			MergeOptionsGrid = RegisterControl(nameof(MiscOptionsLayoutUserControl.MergeOptionsGrid));
		}

		public ControlReference MoreMergeOptionsSeparatorUserControl { get; }
		public ControlReference MergeOptionsGrid { get; }
	}
}
