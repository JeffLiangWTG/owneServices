using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class CommonPackedItemDetailsUserControl : ZUserControl
	{
		public CommonPackedItemDetailsUserControl()
		{
			InitializeComponent();
			TariffFindBox.GetDataGrouping = GetDataGrouping;
			TariffFindBox.GetTariffType = GetTariffType;
			TariffFindBox.GetEffectiveDate = GetEffectiveDate;
			TariffFindBox.GetSelectNomenclatureModes = GetSelectNomenclatureModes;
		}

		ZString GetDataGrouping()
		{
			return ApplicationBusinessProvider?.PackedItemTariffDataGrouping ?? ZString.Empty;
		}

		ZString GetTariffType()
		{
			return ApplicationBusinessProvider?.PackedItemTariffType ?? ZString.Empty;
		}

		ZDateTime GetEffectiveDate()
		{
			return ApplicationBusinessProvider?.GetEffectiveDateForDutyRate(headerCachedValue.Value) ?? ZDateTime.Today;
		}

		List<SelectionStyle> GetSelectNomenclatureModes()
		{
			return ApplicationBusinessProvider?.SelectNomenclatureModes;
		}

		ApplicationBusinessProvider ApplicationBusinessProvider
		{
			get
			{
				var parentUserControl = TariffFindBox.Parent as DynamicLayoutPanel;
				var currentDataItem = parentUserControl?.CurrentDataItem as AsycudaPack;
				if (currentDataItem != null)
				{
					var header = CachedValueHelper.GetValue(ref headerCachedValue, () => currentDataItem.Bill?.Header);

					if (header != null)
					{
						return CachedValueHelper.GetValue(ref providerCachedValue, () => header.ApplicationBusinessProvider);
					}
				}

				return null;
			}
		}
		CachedValue<ApplicationBusinessProvider> providerCachedValue;
		CachedValue<AsycudaManifestHeader> headerCachedValue;
	}
}
