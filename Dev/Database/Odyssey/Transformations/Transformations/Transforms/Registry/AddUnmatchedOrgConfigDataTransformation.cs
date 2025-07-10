using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class AddUnmatchedOrgConfigDataTransformation : RegistryDataTransformation
	{
		const string SourceRegistryItemName = "UseUnmatchedOrganisationForMatching";
		const string TargetRegistryItemName = "UnmatchedOrganisationConfiguration";

		public override string UserDescription => $"Copy {SourceRegistryItemName} registry value to {TargetRegistryItemName}";

		protected override void OfflinePostUpgradeTransform()
		{
			var table = GetDataTable(SourceRegistryItemName);
			if (table.Rows.Count == 1)
			{
				var row = table.Rows[0];
				var binaryValue = row[StmDataSchema.Constants.SD_BinaryValue] as byte[];

				if (binaryValue?.Length > 0)
				{
					using var memoryStream = new MemoryStream(binaryValue);
					using var xmlReader = XmlReader.Create(memoryStream);
					var parsedXml = XDocument.Load(xmlReader);
					var useUnmatchedOrg = parsedXml.Root.Elements("IsEnabled").FirstOrDefault()?.Value;
					if (useUnmatchedOrg != null)
					{
						var newXmlValue = $@"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfCodeDescriptionBool xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>ORD</Code><Description>Order (Forwarding)</Description><Bool>{useUnmatchedOrg}</Bool><SystemDefined>False</SystemDefined></CodeDescriptionBool></ArrayOfCodeDescriptionBool>";
						var newXmlBytes = Encoding.Unicode.GetBytes(newXmlValue);
						var sql = @"
IF NOT EXISTS(SELECT NULL FROM dbo.StmData WHERE SD_Name = @targetName)
INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
SELECT
	NEWID(), @targetName, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, @binaryValue, SD_GuidValue, GETUTCDATE(), 'E', GETUTCDATE(), 'E'
FROM
	dbo.StmData AS source
WHERE
	SD_Name = @sourceName
";

						using (var cmd = Db.Connection.Command(sql))
						{
							cmd.AddParameterBasedOnDbColumn("@sourceName", SourceRegistryItemName, StmDataSchema.SD_Name);
							cmd.AddParameterBasedOnDbColumn("@targetName", TargetRegistryItemName, StmDataSchema.SD_Name);
							cmd.AddParameterBasedOnDbColumn("@binaryValue", newXmlBytes, StmDataSchema.SD_BinaryValue);

							cmd.ExecuteNonQuery();
						}
					}
				}
			}
		}
	}
}
