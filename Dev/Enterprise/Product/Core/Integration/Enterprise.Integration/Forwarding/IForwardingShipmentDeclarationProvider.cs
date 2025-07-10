using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public interface IForwardingShipmentDeclarationProvider
	{
		public BusinessObject GetDeclaration();
	}
}
