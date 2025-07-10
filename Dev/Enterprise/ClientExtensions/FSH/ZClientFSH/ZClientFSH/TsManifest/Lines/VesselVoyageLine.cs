using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Client.FSH.TsManifest.Lines
{
	public class VesselVoyageLine : BaseLine
	{
		public VesselVoyageLine(FortuneShippingDataRow row) : base(row)
		{
		}

		#region Properties

		public ZString Vessel
		{
			get { return Row[Constants.VslAndVoyFields.Vessel]; }
		}

		public ZString VesselCode
		{
			get { return Row[Constants.VslAndVoyFields.VesselCode]; }
		}

		public ZString Voyage
		{
			get { return Row[Constants.VslAndVoyFields.Voyage]; }
		}

		#endregion

		#region Related Lines

		public OceanBillLine AddNewOceanBill(FortuneShippingDataRow row)
		{
			OceanBillLine result = new OceanBillLine(this, row);
			fOceanBillLines.Add(result);
			return result;
		}

		public IReadOnlyList<OceanBillLine> OceanBills => fOceanBillLines;

		protected List<OceanBillLine> fOceanBillLines = new List<OceanBillLine>();

		#endregion
	}
}
