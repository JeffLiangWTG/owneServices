
using CargoWise.Types;

namespace Enterprise.Client.FSH.TsManifest.Lines
{
	public class PlaceInfoLine : BaseLine
	{
		public PlaceInfoLine(OceanBillLine oceanBill, FortuneShippingDataRow row) : base(row)
		{
			this.OceanBill = oceanBill;
		}

		#region Properties

		public ZString PortOfDestination
		{
			get { return Row[Constants.PlaceInfoOf1BL.PlaceOfDeliveryCode]; }
		}

		public ZString PortOfDischarge
		{
			get { return Row[Constants.PlaceInfoOf1BL.PortOfDischargeCode]; }
		}

		#endregion

		public readonly OceanBillLine OceanBill;
	}
}
