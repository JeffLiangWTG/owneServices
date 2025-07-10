
using CargoWise.Types;

namespace Enterprise.Client.FSH.TsManifest.Lines
{
	public class CargoDescriptionLine : BaseLine
	{
		public CargoDescriptionLine(CargoFieldLine cargoField, FortuneShippingDataRow row) : base(row)
		{
			this.CargoField = cargoField;
		}

		#region Properties

		public ZString Description
		{
			get { return Row[Constants.CargoDescription.Description].Replace("^", System.Environment.NewLine); }
		}

		#endregion

		public readonly CargoFieldLine CargoField;
	}
}
