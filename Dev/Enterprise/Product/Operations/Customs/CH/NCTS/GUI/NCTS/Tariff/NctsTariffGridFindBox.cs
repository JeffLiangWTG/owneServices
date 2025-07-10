using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class NctsTariffGridFindBox : TariffGridFindBox, INctsTariffFindBox
{
	public NctsTariffGridFindBox()
	{
		this.InitializeFindBox();
	}

	ITariffFormatter INctsTariffFindBox.GetTariffFormatter() => GetTariffFormatter();

	Func<bool> INctsTariffFindBox.GetShouldShowExactDescription => null;

	protected override IFindBoxListProvider ListProvider => this.GetFindBoxListProvider();
}
