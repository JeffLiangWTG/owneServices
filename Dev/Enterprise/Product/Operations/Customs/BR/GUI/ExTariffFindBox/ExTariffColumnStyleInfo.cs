using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public class ExTariffColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(ExTariffColumnStyle); }
		}

		[Browsable(true), DefaultValue("")]
		public string TariffCodeProperty { get; set; }

		[Browsable(true), DefaultValue("")]
		public string TariffTypeProperty { get; set; }
	}
}
