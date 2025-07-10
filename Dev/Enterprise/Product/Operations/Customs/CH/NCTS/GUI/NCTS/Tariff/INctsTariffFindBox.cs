using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public interface INctsTariffFindBox : ITariffFindBox
{
	public Func<ZString> GetTariffType { get; set; }

	public Func<ZString> GetDataGrouping { get; set; }

	public Func<bool> GetShouldShowExactDescription { get; }

	ITariffFormatter GetTariffFormatter();
}
