using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public sealed class ArrivalNotificationDetailsControlBag : ControlBag
	{
		public ArrivalNotificationDetailsControlBag()
		{
			ExpectedNextCustomsProcedureDropEdit = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.ExpectedNextCustomsProcedureDropEdit));
		}

		public static ArrivalNotificationDetailsControlBag Instance => instance ?? (instance = new ArrivalNotificationDetailsControlBag());

		[ThreadStatic]
		static ArrivalNotificationDetailsControlBag instance;

		public ControlReference ExpectedNextCustomsProcedureDropEdit { get; }

		protected override Control CreateTemplate() => new ArrivalNotificationDetailsUserControl();
	}
}
