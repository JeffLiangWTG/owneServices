using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	class TariffBoxNomenclatureSelectionModeBehaviour : ControlBehaviour<TariffFindBox, JobComInvoiceLine>
	{
		protected override void UpdateBehaviourCore(TariffFindBox control, JobComInvoiceLine dataItem)
		{
			if (control == null || dataItem == null)
			{
				return;
			}

			control.GetSelectNomenclatureModes = () => dataItem.GetTariffNomenclatureSelectionModes().ToList();
		}
	}
}
