using Schema = Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.IE.GUI
{
	public class TSDAdditionalInfosUserControlWithGrid : EU.TemporaryStorage.GUI.UCC6TemporaryStorageAdditionalInfosUserControlWithGrid, Integration.Customs.IE.IUCC6TemporaryStorageAdditionalInfosUserControlWithGrid
	{
		protected override void AdditionalInfosGridColumnsVisible()
		{
			AdditionalInfosGrid.SetAvailability(false, [Schema.CSI_ReferenceNumber2, Schema.CSI_RX_NKCurrency, Schema.CSI_Value]);
		}
	}
}
