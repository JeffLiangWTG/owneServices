using System;
using System.Data;
using System.Text;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class RenameOptionFromESToEUAccountingIntegrationOptionsRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Rename option 'ESCustomsStatusCodes' to 'EUCustomsStatusCodes' in AccountingIntegrationOptionsRegistry.";

		protected override void OfflinePostUpgradeTransform()
		{
			using (var dataTable = GetDataTable(RegistryItemName))
			{
				foreach (DataRow dataRow in dataTable.Rows)
				{
					var pk = dataRow.Field<Guid>(StmDataSchema.Constants.PK);
					var originBinaryValue = dataRow.Field<byte[]>(StmDataSchema.Constants.SD_BinaryValue);
					if (originBinaryValue == null)
					{
						continue;
					}
					(var renamedOptionBinaryValue, var shouldRename) = RenameOptionRow(originBinaryValue);
					if (shouldRename)
					{
						UpdateDatabaseValue(pk, renamedOptionBinaryValue);
					}
				}
			}
		}

		(byte[], bool) RenameOptionRow(byte[] binaryValue)
		{
			var stringValue = Encoding.Unicode.GetString(binaryValue);
			var shouldRename = stringValue.Contains("ESCustomsStatusCodes");
			if (shouldRename)
			{
				stringValue = stringValue.Replace("ESCustomsStatusCodes", "EUCustomsStatusCodes");
			}

			return (Encoding.Unicode.GetBytes(stringValue), shouldRename);
		}

		public const string RegistryItemName = "EnableAccountingIntegration";
	}
}
