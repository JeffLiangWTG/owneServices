using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public class ConsignmentItemControlBag : ControlBag
	{
		public ConsignmentItemControlBag()
		{
			MainUserControl = RegisterControl(nameof(GUI.ConsignmentItemUserControlDetails.MainUserControl));
		}

		public static ConsignmentItemControlBag Instance => instance ?? (instance = new ConsignmentItemControlBag());

		[ThreadStatic]
		static ConsignmentItemControlBag instance;

		public ControlReference MainUserControl { get; }

		protected override Control CreateTemplate() => new ConsignmentItemUserControlDetails();
	}
}
