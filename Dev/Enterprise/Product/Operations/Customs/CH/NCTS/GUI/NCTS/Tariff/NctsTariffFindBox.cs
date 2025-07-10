using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.CH.NCTS.GUI;

[SuppressCheckControlModuleId]
[FormBasherTestPopupExclude]
public class NctsTariffFindBox : TariffFindBox, INctsTariffFindBox
{
	public NctsTariffFindBox()
	{
		this.InitializeFindBox();
	}

	ITariffFormatter INctsTariffFindBox.GetTariffFormatter() => GetTariffFormatter();

	public Func<bool> GetShouldShowExactDescription { get; set; }

	protected override IFindBoxListProvider ListProvider => this.GetFindBoxListProvider();
}
