
using CargoWise.Types;

namespace Enterprise.Client.FSH.TsManifest.Lines
{
	public class PartyDetailsLine : BaseLine
	{
		public PartyDetailsLine(OceanBillLine oceanBill, FortuneShippingDataRow row) : base(row)
		{
			this.OceanBill = oceanBill;
		}

		#region Properties

		public ZString AddressCode
		{
			get { return Row[Constants.AddressFields.AddressCode].Replace(" ", ""	).SubstringSafe(0, 6); }
		}

		public ZString Address1
		{
			get { return Row[Constants.AddressFields.Address1]; }
		}

		public ZString Address2
		{
			get { return Row[Constants.AddressFields.Address2]; }
		}

		public ZString Address3
		{
			get { return Row[Constants.AddressFields.Address3]; }
		}

		public ZString Address4
		{
			get { return Row[Constants.AddressFields.Address4]; }
		}

		public ZString Address5
		{
			get { return Row[Constants.AddressFields.Address5]; }
		}

		#endregion

		public readonly OceanBillLine OceanBill;
	}
}
