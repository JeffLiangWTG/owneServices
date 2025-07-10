using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class AdditionalInfoControlBag : ControlBag
	{
		AdditionalInfoControlBag()
		{
			ContentInfoTypeSeparatorUserControl = RegisterControl(nameof(AdditionalInfoUserControl.ContentInfoTypeSeparatorUserControl));
			ContentInfoTypeGrid = RegisterControl(nameof(AdditionalInfoUserControl.ContentInfoTypeGrid));
			AdditionalTariffsSeparatorUserControl = RegisterControl(nameof(AdditionalInfoUserControl.AdditionalTariffsSeparatorUserControl));
			AdditionalTariffsGrid = RegisterControl(nameof(AdditionalInfoUserControl.AdditionalTariffsGrid));
		}

		public static AdditionalInfoControlBag Instance => instance ?? (instance = new AdditionalInfoControlBag());

		[ThreadStatic]
		static AdditionalInfoControlBag instance;

		protected override Control CreateTemplate() => new AdditionalInfoUserControl();

		public ControlReference ContentInfoTypeGrid { get; }

		public ControlReference AdditionalTariffsGrid { get; }

		public ControlReference AdditionalTariffsSeparatorUserControl { get; }

		public ControlReference ContentInfoTypeSeparatorUserControl { get; }
	}
}
