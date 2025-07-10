
using CargoWise.Types;

namespace Enterprise.Client.FSH.TsManifest.Lines
{
	public class HouseBillLine : BaseLine
	{
		public HouseBillLine(OceanBillLine oceanBill, FortuneShippingDataRow row) : base(row)
		{
			this.OceanBill = oceanBill;
		}

		#region Properties

		public ZString HouseBillNumber
		{
			get { return Row[Constants.HouseColoBillInfo.HouseBillNo]; }
		}

		public ZString ColoadMasterNumber
		{
			get { return Row[Constants.HouseColoBillInfo.ColoBillNo]; }
		}

		public ZInt NumberOfPackages
		{
			get { return ZInt.ParseSafe(Row[Constants.HouseColoBillInfo.NoOfPackages], 0); }
		}

		#endregion

		public readonly OceanBillLine OceanBill;
	}
}
