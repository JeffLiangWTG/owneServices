using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public class ADAWMappingUpdateHelper
	{
		public ADAWMappingUpdateHelper()
		{
		}

		protected string ADAWModulePrefix => "GLOWDataImportV2";
		protected readonly string TrueString = "true";
		protected readonly string FalseString = "false";

		public void UpdateADAWTemplateMappingField(IEnumerable<AdAWMappingUpdateInfo> mappingUpdateInfo)
		{
			if (!mappingUpdateInfo.IsNullOrEmpty() && DbObjectCreator.TableExists(Db.Connection, StmModuleFilterSchema.Constants.TableName))
			{
				var moduleIds = string.Join(",", mappingUpdateInfo.Where(x => !x.MappingInterfaceName.IsNullOrEmpty()).Select(x => string.Format(CultureInfo.InvariantCulture, "'{0}_{1}'", ADAWModulePrefix, x.MappingInterfaceName)));
				var templateQuerySQL = string.Format(CultureInfo.InvariantCulture, "SELECT S9_PK, S9_ModuleID, dbo.CLRUncompressAsString(S9_FilterData) as FilterData FROM dbo.StmModuleFilter WHERE S9_ModuleID in ({0})", moduleIds);
				var updateMappings = GetUpdateMappings(templateQuerySQL, mappingUpdateInfo);

				if (updateMappings.Any())
				{
					SaveChanges(updateMappings);
				}
			}
		}

		Dictionary<Guid, XDocument> GetUpdateMappings(string templateQuerySQL, IEnumerable<AdAWMappingUpdateInfo> mappingUpdateInfo)
		{
			var updateMappings = new Dictionary<Guid, XDocument>();

			using (var cmd = Db.Connection.Command(templateQuerySQL))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var template = (string)reader["FilterData"];
					if (!template.IsNullOrEmpty())
					{
						var moduleID = (string)reader["S9_ModuleID"];
						var interfaceName = moduleID.Substring(moduleID.IndexOf('_') + 1);
						var filterDataXml = XDocument.Parse(template);
						var fieldMappings = filterDataXml.Descendants("FieldMapping");
						var needsUpdate = false;

						//delete low-priority mappings to avoid duplicate mapping name
						foreach (var supersededMapping in mappingUpdateInfo.Where(x => !x.SupersededBy.IsNullOrEmpty() && x.MappingInterfaceName == interfaceName))
						{
							if (fieldMappings.Any(x => x.Element("TargetColumnName").Value.Equals(supersededMapping.SupersededBy)))
							{
								fieldMappings.Where(x => x.Element("TargetColumnName").Value.Equals(supersededMapping.OldTargetColumnName)).Remove();
							}
						}

						foreach (var updateInfo in mappingUpdateInfo.Where(x => x.MappingInterfaceName == interfaceName))
						{
							foreach (var fieldMapping in fieldMappings)
							{
								var targetColumn = fieldMapping.Element("TargetColumnName");
								var targetColumnName = targetColumn.Value;
								if (targetColumnName.Equals(updateInfo.OldTargetColumnName))
								{
									targetColumn.Value = targetColumnName.Replace(updateInfo.OldTargetColumnName, updateInfo.NewTargetColumnName);
									var shouldMatchField = fieldMapping.Element("MatchingInfo").Element("ShouldMatch");
									shouldMatchField.Value = updateInfo.MappingInfoShouldMatch ? TrueString : FalseString;
									var shouldCreateField = fieldMapping.Element("MatchingInfo").Element("ShouldCreate");
									shouldCreateField.Value = updateInfo.MappingInfoShouldCreate ? TrueString : FalseString;
									needsUpdate = true;
								}
							}
						}

						if (needsUpdate)
						{
							updateMappings.Add((Guid)reader["S9_PK"], filterDataXml);
						}
					}
				}

				return updateMappings;
			}
		}

		void SaveChanges(Dictionary<Guid, XDocument> itemsToUpdate)
		{
			const string updateSql = @"update dbo.StmModuleFilter set S9_FilterData = dbo.CLRCompressStringAsBytes(@FilterData) where S9_PK = @PK";

			foreach (var pair in itemsToUpdate)
			{
				using (var command = Db.Connection.Command(updateSql))
				{
					command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pair.Key);
					command.AddParameter("@FilterData", SqlDbType.VarChar, pair.Value.ToString());
					command.ExecuteNonQuery();
				}
			}
		}
	}

	public class AdAWMappingUpdateInfo
	{
		public AdAWMappingUpdateInfo(string mappingInterfaceName, string oldTargetColumnName, string newTargetColumnName, bool mappingInfoShouldMatch, bool mappingInfoShouldCreate, string supersededBy)
		{
			MappingInterfaceName = mappingInterfaceName;
			OldTargetColumnName = oldTargetColumnName;
			NewTargetColumnName = newTargetColumnName;
			MappingInfoShouldMatch = mappingInfoShouldMatch;
			MappingInfoShouldCreate = mappingInfoShouldCreate;
			SupersededBy = supersededBy;
		}

		public string MappingInterfaceName { get; set; }
		public string OldTargetColumnName { get; set; }
		public string NewTargetColumnName { get; set; }
		public bool MappingInfoShouldMatch { get; set; }
		public bool MappingInfoShouldCreate { get; set; }
		public string SupersededBy { get; set; }
	}
}
