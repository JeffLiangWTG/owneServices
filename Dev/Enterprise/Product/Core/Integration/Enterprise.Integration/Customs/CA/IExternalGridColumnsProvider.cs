using Enterprise.Integration.ZArchitecture;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface IExternalGridColumnsProvider
			{
				void AddColumns(IGridControl zGrid);
			}

			public interface IShipmentGridColumnsProvider : IExternalGridColumnsProvider
			{
			}
		}
	}
}
