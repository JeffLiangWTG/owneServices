using CargoWise.EntityFramework;

namespace Enterprise.Integration;

public static partial class Customs
{
	public static partial class PL
	{
		public interface IMessageLocator
		{
			IEDIMessage FindTransmitMessage(BusinessObjectFactory factory, object dataProvider);
		}
	}
}
