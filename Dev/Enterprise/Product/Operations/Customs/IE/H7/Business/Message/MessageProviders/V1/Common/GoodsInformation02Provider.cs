using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class GoodsInformation02Provider : IGoodsInformation02
	{
		public GoodsInformation02Provider(AsycudaBill bill)
		{
			this.bill = bill;
		}
		readonly AsycudaBill bill;

		public decimal GrossMass => bill.MassInKilos;
	}
}
