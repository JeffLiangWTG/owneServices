using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5TransportMeansControlBag : ControlBag
	{
		public Phase5TransportMeansControlBag()
		{
			TransportAtDepartureTypeDropEdit = RegisterControl(nameof(Phase5TransportMeansUserControl.TransportAtDepartureTypeDropEdit));
			TransportAtDepartureIDTextBox = RegisterControl(nameof(Phase5TransportMeansUserControl.TransportAtDepartureIDTextBox));
			TransportAtDepartureNationalityCodeFindBox = RegisterControl(nameof(Phase5TransportMeansUserControl.TransportAtDepartureNationalityCodeFindBox));
		}

		public static Phase5TransportMeansControlBag Instance => instance ?? (instance = new Phase5TransportMeansControlBag());

		[ThreadStatic]
		static Phase5TransportMeansControlBag instance;

		public ControlReference TransportAtDepartureTypeDropEdit { get; }

		public ControlReference TransportAtDepartureIDTextBox { get; }

		public ControlReference TransportAtDepartureNationalityCodeFindBox { get; }

		protected override Control CreateTemplate() => new Phase5TransportMeansUserControl();
	}
}
