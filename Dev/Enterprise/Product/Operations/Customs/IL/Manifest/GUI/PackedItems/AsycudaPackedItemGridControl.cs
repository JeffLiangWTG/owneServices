using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class AsycudaPackedItemGridControl : ZUserControl
	{
		public AsycudaPackedItemGridControl()
		{
			InitializeComponent();

			new UNDGDataItemFormManager(PackedItemGrid, "", UNDGDataItemFormManagerConfig.IMOHideProperties()).Initialize();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			asycudaManifestHeader = dataSource as AsycudaManifestHeader;

			base.SetDataBinding(dataSource, dataMember);

			SetTariffControlProperties();
		}

		AsycudaManifestHeader asycudaManifestHeader;

		void SetTariffControlProperties()
		{
			var tariffColumnStyle = PackedItemGrid.ColumnStyles.ToArray().FirstOrDefault(x => x is Universal.GUI.TariffColumnStyleInfo) as Universal.GUI.TariffColumnStyleInfo;
			if (tariffColumnStyle != null)
			{
				tariffColumnStyle.GetDataGrouping = GetDataGroupingForUniversalTariff;
				tariffColumnStyle.GetTariffType = GetTariffTypeForUniversalTariff;
				tariffColumnStyle.GetEffectiveDate = GetEffectiveDateForUniversalTariff;
			}
		}

		ZString GetTariffTypeForUniversalTariff()
		{
			return Universal.Constants.TariffTypes.Import;
		}

		ZString GetDataGroupingForUniversalTariff()
		{
			return asycudaManifestHeader.DataGrouping;
		}

		ZDateTime GetEffectiveDateForUniversalTariff()
		{
			return ZDateTime.Today;
		}
	}
}
