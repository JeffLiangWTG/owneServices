using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.ICS.GUI
{
	public sealed class BillControlBag : ControlBag
	{
		public static BillControlBag Instance => instance ?? (instance = new BillControlBag());

		[ThreadStatic]
		static BillControlBag instance;

		BillControlBag()
		{
			SpecialMentionsDropEdit = RegisterControl(nameof(BillUserControl.SpecialMentionsDropEdit));
		}

		protected override Control CreateTemplate() => new BillUserControl();

		public ControlReference SpecialMentionsDropEdit { get; }
	}
}
