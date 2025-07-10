using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class CargoMessage : Body
	{
		public CargoMessage(string payloadEdifact)
		{
			payload = payloadEdifact;
		}

		public override ZString PayloadAsString
		{
			get { return payload; }
		}
		readonly string payload;
	}
}
