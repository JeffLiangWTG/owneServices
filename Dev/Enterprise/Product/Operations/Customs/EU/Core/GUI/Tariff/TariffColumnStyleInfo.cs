using System;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.EU.GUI
{
	public class TariffColumnStyleInfo : ZBaseFindBoxColumnStyleInfo
	{
		public string ParameterForediTariff
		{
			get;
			set;
		}

		public override Type ColumnStyleType
		{
			get { return typeof(TariffColumnStyle); }
		}
	}
}
