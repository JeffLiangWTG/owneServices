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
	public class AHECCFindBox : ZCodeFindBox
	{
		protected override IFindBoxPopup GetNewPopupForm()
		{
			return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper("E", () => new AHECCForm());
		}

		protected override IFindBoxListProvider ListProvider
		{
			get { return new AHECCFindBoxListProvider(); }
		}

		protected override void ShowEditForm(ZFilterModule module)
		{
			Globals.Message.ShowInformation("Editing of AHECC information is not supported", "Unsupported option");
		}

		protected override void ShowEditOrViewForm()
		{
			Globals.Message.ShowInformation("Editing of AHECC information is not supported", "Unsupported option");
		}

		protected override bool RequiresList
		{
			get { return false; }
		}
	}
}
