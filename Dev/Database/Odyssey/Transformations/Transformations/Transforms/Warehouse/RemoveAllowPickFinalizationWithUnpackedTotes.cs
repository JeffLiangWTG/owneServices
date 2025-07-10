using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.ProductWarehouse
{
	public class RemoveAllowPickFinalizationWithUnpackedTotes : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => new[] { RegistryItemName };

		protected override void OfflinePostUpgradeTransform()
		{
			var table = GetDataTable(RegistryItemName);

			if (table.Rows.Count == 1)
			{
				var binaryValue = table.Rows[0].Field<byte[]>(StmDataSchema.Constants.SD_BinaryValue);
				if ((binaryValue is not null) && bool.Parse(Encoding.Unicode.GetString(binaryValue)))
				{
					var sqlText = @"
					UPDATE
						dbo.WhsClientPickPackParamsByWhs
					SET
						WPP_AllowPickFinalizationWithUnpackedTotes = 1,
						WPP_SystemLastEditUser = '~BP',
						WPP_SystemLastEditTimeUtc = GETUTCDATE();";

					Db.Connection.ExecuteNonQuery(sqlText);
				}
			}

			base.OfflinePostUpgradeTransform();
		}

		protected string RegistryItemName => "AllowPickFinalizationWithUnpackedTotes";
	}
}
