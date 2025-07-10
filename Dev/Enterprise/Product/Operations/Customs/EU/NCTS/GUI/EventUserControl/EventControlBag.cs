using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class EventControlBag : ControlBag
	{
		EventControlBag()
		{
			Phase5EventTabUserControl = RegisterControl(nameof(EventUserControl.Phase5EventTabUserControl));
		}

		public static EventControlBag Instance => instance ?? (instance = new EventControlBag());

		[ThreadStatic]
		static EventControlBag instance;

		public ControlReference Phase5EventTabUserControl { get; }

		protected override Control CreateTemplate() => new EventUserControl();
	}
}
