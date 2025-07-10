using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class SecurityAtDepartureControlBag : ControlBag
	{
		public SecurityAtDepartureControlBag()
		{
			PlaceOfLoadingCodeFindBox = RegisterControl(nameof(SecurityAtDepartureUserControl.PlaceOfLoadingCodeFindBox));
			PlaceOfUnloadingCodeFindBox = RegisterControl(nameof(SecurityAtDepartureUserControl.PlaceOfUnloadingCodeFindBox));
		}

		public static SecurityAtDepartureControlBag Instance => instance ?? (instance = new SecurityAtDepartureControlBag());

		[ThreadStatic]
		static SecurityAtDepartureControlBag instance;

		public ControlReference PlaceOfLoadingCodeFindBox { get; }
		public ControlReference PlaceOfUnloadingCodeFindBox { get; }

		protected override Control CreateTemplate() => new SecurityAtDepartureUserControl();
	}
}
