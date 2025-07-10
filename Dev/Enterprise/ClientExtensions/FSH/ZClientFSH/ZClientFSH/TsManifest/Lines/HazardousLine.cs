
using CargoWise.Types;

namespace Enterprise.Client.FSH.TsManifest.Lines
{
	public class HazardousLine : BaseLine
	{
		public HazardousLine(CargoFieldLine cargoField, FortuneShippingDataRow row) : base(row)
		{
			this.CargoField = cargoField;
		}

		#region Properties

		public ZString IMOClass
		{
			get { return Row[Constants.Hazardous.ImoClass]; }
		}

		public ZString FlashPoint
		{
			get { return Row[Constants.Hazardous.FlashPoint]; }
		}

		public ZString IMOUNNumber
		{
			get { return Row[Constants.Hazardous.ImoUnNo]; }
		}

		#endregion

		public readonly CargoFieldLine CargoField;
	}
}
