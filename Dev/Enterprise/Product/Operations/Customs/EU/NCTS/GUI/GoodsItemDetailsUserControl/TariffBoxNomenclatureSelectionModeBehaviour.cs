using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class TariffBoxNomenclatureSelectionModeBehaviour : ControlBehaviour<TariffFindBox, NctsDepartureCargoDesc>
	{
		protected override void UpdateBehaviourCore(TariffFindBox control, NctsDepartureCargoDesc dataItem)
		{
			if (control == null || dataItem == null)
			{
				return;
			}

			control.GetSelectNomenclatureModes = () => dataItem.GetTariffNomenclatureSelectionModes().ToList();
		}
	}
}
