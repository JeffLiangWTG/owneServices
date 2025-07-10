using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public class ExTariffColumnStyle : ZCodeFindBoxColumnStyle
	{
		public ExTariffColumnStyle(ExTariffColumnStyleInfo columnInfo)
				: this(() => new ExTariffGridFindBox(), columnInfo)
		{ }

		ExTariffColumnStyle(Func<ExTariffGridFindBox> control, ExTariffColumnStyleInfo columnInfo)
			: base(control, columnInfo)
		{
			TariffCodeProperty = columnInfo.TariffCodeProperty;
			TariffTypeProperty = columnInfo.TariffTypeProperty;
		}

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			(control as ExTariffGridFindBox).TariffCodeProperty = TariffCodeProperty;
			(control as ExTariffGridFindBox).TariffTypeProperty = TariffTypeProperty;
		}

		public string TariffCodeProperty { get; set; }
		public string TariffTypeProperty { get; set; }
	}
}
