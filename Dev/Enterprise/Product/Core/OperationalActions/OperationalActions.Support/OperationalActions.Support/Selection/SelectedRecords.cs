using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Support
{
	public sealed class SelectedRecords : ISelectedRecords
	{
		public SelectedRecords()
		{
			PrimaryKeys = System.Array.Empty<ZGuid>();
		}

		public SelectedRecords(params ZGuid[] primaryKeys)
		{
			Argument.NotNull(primaryKeys, "primaryKeys");
			PrimaryKeys = primaryKeys;
		}

		public ZGuid[] PrimaryKeys { get; set; }
		public bool AutoSelectedAllKeys { get; set; }
	}
}
