using System;
using Enterprise.Customs.Universal.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class NctsTariffColumnStyleInfo : TariffColumnStyleInfo
{
	public override Type ColumnStyleType => typeof(NctsTariffColumnStyle);
}
