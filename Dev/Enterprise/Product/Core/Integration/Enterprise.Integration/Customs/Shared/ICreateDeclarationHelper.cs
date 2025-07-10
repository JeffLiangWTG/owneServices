namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICreateDeclarationHelper
			{
				void CreateDeclaration(Forwarding.IForwardingShipment shipment);
			}
		}
	}
}
