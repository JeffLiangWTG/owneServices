using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class InventoryHeldCodeDataFile : EmbeddedDataFile
	{
		public InventoryHeldCodeDataFile() : base(DataFileRelativePath, InventoryHeldCodeDataFileTables)
		{
		}

		internal InventoryHeldCodeDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, InventoryHeldCodeDataFileTables)
		{
		}

		protected override string SelectQuery
		{
			get
			{
				return $@"
            SELECT * 
            FROM dbo.{WhsInventoryHeldCodeSchema.Constants.TableName} 
            WHERE {WhsInventoryHeldCodeSchema.Constants.WHC_IsSystem} = 1 
            ORDER BY {WhsInventoryHeldCodeSchema.Constants.PK}";
			}
		}

		#region Implementation

		const string DataFileRelativePath = @"Public\WhsInventoryHeldCode\WhsInventoryHeldCode.xml";
		public override string ResourceRelativeName => "WhsInventoryHeldCode.WhsInventoryHeldCode.xml";

		protected static readonly string[] InventoryHeldCodeDataFileTables = new string[] { WhsInventoryHeldCodeSchema.Constants.TableName };

		#endregion
	}
}
