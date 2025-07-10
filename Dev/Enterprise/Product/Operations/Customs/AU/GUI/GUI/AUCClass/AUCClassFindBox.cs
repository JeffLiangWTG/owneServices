using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class AUCClassFindBox : ZCodeFindBox
	{
		protected override IFindBoxPopup GetNewPopupForm()
		{
			return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper("I", () => new AUCClassForm());
		}

		protected override void ShowEditForm(ZFilterModule module)
		{
			Globals.Message.ShowInformation("Editing of tariff information is not supported", "Unsupported option");
		}

		protected override void ShowEditOrViewForm()
		{
			Globals.Message.ShowInformation("Editing of tariff information is not supported", "Unsupported option");
		}

		protected IFindBoxListProvider cachedListProvider;
		protected override IFindBoxListProvider ListProvider
		{
			get
			{
				if (cachedListProvider == null)
				{
					ZForm topForm = FindForm() as ZForm;
					BusinessObjectFactory factory;
					if (topForm != null && topForm.BusinessEntity != null)
					{
						factory = topForm.BusinessEntity.Factory;
					}
					else
					{
						factory = new BusinessObjectFactory();
					}
					cachedListProvider = new AUCClassFindBoxListProvider(factory);
				}
				return cachedListProvider;
			}
		}

		protected override bool RequiresList
		{
			get { return false; }
		}
	}
}
