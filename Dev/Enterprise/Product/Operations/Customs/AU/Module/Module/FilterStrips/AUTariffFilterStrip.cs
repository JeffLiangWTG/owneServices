using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public class AUTariffFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is AUTariffModuleFilter)
			{
				result = GetFilterControls(((AUTariffModuleFilter)currentModuleFilter).FilterType);
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}

		Control[] GetFilterControls(TariffModuleFilterType filterType)
		{
			var cC_TariffNumFindBox = CreateTariffFindBox(filterType);

			cC_TariffNumFindBox.AllowDrop = true;
			cC_TariffNumFindBox.BindTo = "CC_TariffNum";
			cC_TariffNumFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 1);
			cC_TariffNumFindBox.Name = "CC_TariffNumFindBox";
			cC_TariffNumFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(FilterControlBoxWidth, FilterControlBoxHeight);
			FilterControlBindingSource.SetBindingMember(cC_TariffNumFindBox, cC_TariffNumFindBox.BindTo);

			return new Control[] { cC_TariffNumFindBox };
		}

		protected internal ZCodeFindBox CreateTariffFindBox(TariffModuleFilterType filterType)
		{
			if (filterType == TariffModuleFilterType.Export)
			{
				if (AUCAHECCWrapper.EnableCWRefForAHECC)
				{
					return new UniversalTariffExportFindBox();
				}
				else
				{
					return new AHECCFindBox();
				}
			}
			else
			{
				if (AUCClassWrapper.UseCustomsReferenceData)
				{
					return new UniversalTariffImportFindBox();
				}
				else
				{
					return new AUCClassFindBox();
				}
			}
		}
	}
}
