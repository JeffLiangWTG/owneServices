using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry;

public class UpdateBECustomsRegistryNumberFountainDataTransformation : RegistryDataTransformation
{
	const string RegistryItemName = "BECustomsRegistries";

	public override string UserDescription => "Expand fountain key SN_Name in StmNums with registry StartingDate";

	protected override void OfflinePostUpgradeTransform()
	{
		if (new RegistryTransformationHelper().GetStmDataRowCount(RegistryItemName) == 0)
		{
			return;
		}

		foreach (DataRow row in GetDataTable(RegistryItemName).Rows)
		{
			if (row[StmDataSchema.Constants.SD_BinaryValue] is not byte[] binaryValue || binaryValue.Length == 0)
			{
				continue;
			}

			var owner = ((Guid?)row[StmDataSchema.Constants.SD_Owner]).ToString();

			if (!owner.IsNullOrEmpty())
			{
				using (var ms = new MemoryStream(binaryValue))
				{
					XDocument userData;
					try
					{
						userData = XDocument.Load(ms);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						continue;
					}

					var rootElement = userData.Root;

					if (rootElement?.Name == "ArrayOfCustomsRegistry")
					{
						var updateCustomsRegistryFountainNameSql = "";
						var intParam = 0;
						var parameters = new List<SqlParameter>();
						foreach (var customsRegistry in rootElement.XPathSelectElements("//CustomsRegistry"))
						{
							intParam++;
							var customsRegistryFountainNamePart1 = "BECustomsRegistryNumber" + "-" + owner + "-" + customsRegistry.Element("Organization")?.Value + "-";
							var customsRegistryFountainNamePart2 = customsRegistry.Element("DeclarationType")?.Value;
							var sqlQuery = $"FROM dbo.StmNums WHERE SN_Name = @CustomsRegistryFountainNamePart1{intParam} + @CustomsRegistryFountainNamePart2{intParam}";

							if (Db.Connection.Exists(sqlQuery, cmd =>
							{
								cmd.AddParameter($"@CustomsRegistryFountainNamePart1{intParam}", System.Data.SqlDbType.VarChar, customsRegistryFountainNamePart1);
								cmd.AddParameter($"@CustomsRegistryFountainNamePart2{intParam}", System.Data.SqlDbType.VarChar, customsRegistryFountainNamePart2);
							}))
							{
								var customsRegistryFountainName = customsRegistryFountainNamePart1 + customsRegistryFountainNamePart2;
								var updateCustomsRegistryFountainName = customsRegistryFountainName + "-" + customsRegistry.Element("StartingDate")?.Value;

								parameters.Add(new SqlParameter($"@NewName{intParam}", SqlDbType.VarChar) { Value = updateCustomsRegistryFountainName });
								parameters.Add(new SqlParameter($"@CustomsRegistryFountainName{intParam}", SqlDbType.VarChar) { Value = customsRegistryFountainName });

								updateCustomsRegistryFountainNameSql += $@"UPDATE dbo.StmNums SET SN_Name = @NewName{intParam} WHERE SN_Name = @CustomsRegistryFountainName{intParam};";
							}
						}
						if (!updateCustomsRegistryFountainNameSql.IsNullOrEmpty())
						{
							Db.Connection.ExecuteNonQuery(updateCustomsRegistryFountainNameSql, cmd =>
							{
								foreach (var parameter in parameters)
								{
									cmd.AddParameter(parameter.ParameterName, parameter.SqlDbType, parameter.Value);
								}
							});
						}
					}
				}
			}
		}
	}
}
