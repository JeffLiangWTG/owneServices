using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public abstract class Body
	{
		public abstract ZString PayloadAsString { get; }
	}
}
