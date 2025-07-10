using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.AU.Declaration.GUI;

[SuppressCheckControlModuleId]
[FormBasherTestPopupExclude]
public class AUCClassGridFindBox : ZGridFindBox
{
	public AUCClassGridFindBox()
	{ }

	protected override IFindBoxPopup GetNewPopupForm()
	{
		return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper("I", () => new AUCClassForm());
	}

	protected override IFindBoxListProvider ListProvider
	{
		get
		{
			var bizo = (FindForm() as ZForm)?.BusinessEntity;
			var factory = bizo?.Factory ?? new BusinessObjectFactory();
			return new AUCClassFindBoxListProvider(factory);
		}
	}
}
