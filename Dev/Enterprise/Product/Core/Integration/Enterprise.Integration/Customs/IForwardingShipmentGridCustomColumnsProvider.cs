using Enterprise.Integration.ZArchitecture;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IForwardingShipmentGridCustomColumnsProvider
		{
			void AddColumns(IGridControl zGrid);
		}
	}
}
