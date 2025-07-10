using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class LineKeyForTest : NonPersistentBusinessObject, ILineKey
	{
		public ZString ShipmentRef { get; set; }

		public ZString FlightNo { get; set; }

		public ZDateTime ImportDate { get; set; }

		public ZString ConsigneePostCode { get; set; }
	}
}
