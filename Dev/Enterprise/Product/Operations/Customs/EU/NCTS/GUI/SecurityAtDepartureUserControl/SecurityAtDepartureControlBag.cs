using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class SecurityAtDepartureControlBag : ControlBag
	{
		public SecurityAtDepartureControlBag()
		{
			PlaceOfLoadingUserControl = RegisterControl(nameof(SecurityAtDepartureUserControl.PlaceOfLoadingUserControl));
			PlaceOfUnloadingUserControl = RegisterControl(nameof(SecurityAtDepartureUserControl.PlaceOfUnloadingUserControl));
			PlaceOfUnloadingCodeFindBox = RegisterControl(nameof(SecurityAtDepartureUserControl.PlaceOfUnloadingCodeFindBox));
			SpecificCircumstanceIndicatorDropEdit = RegisterControl(nameof(SecurityAtDepartureUserControl.SpecificCircumstanceIndicatorDropEdit));
		}

		public static SecurityAtDepartureControlBag Instance => instance ?? (instance = new SecurityAtDepartureControlBag());

		[ThreadStatic]
		static SecurityAtDepartureControlBag instance;

		public ControlReference PlaceOfLoadingUserControl { get; }

		public ControlReference PlaceOfUnloadingUserControl { get; }

		public ControlReference PlaceOfUnloadingCodeFindBox { get; }

		public ControlReference SpecificCircumstanceIndicatorDropEdit { get; }

		protected override Control CreateTemplate() => new SecurityAtDepartureUserControl();
	}
}
