using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing
{
	public class CompetitorFilterTransformationHelper
	{
		public Dictionary<Guid, (List<string>, List<int>)> UpdateStmModuleFilterTable(string schemeSql, string newFilterDescription, List<(string Code, string OldFilterDescription)> oldCompetitorCodeDescription, bool shouldReadModuleData = false)
		{
			var updateMappings = new Dictionary<Guid, XDocument>();
			var updateInfo = new Dictionary<Guid, (List<string>, List<int>)>();

			using (var cmd = Db.Connection.Command(schemeSql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var filterTemplate = reader["FilterData"].ToString();
					if (string.IsNullOrEmpty(filterTemplate))
					{
						continue;
					}

					var competitorCodes = new List<string>();
					var indexes = new List<int>();
					var filterDataXml = XDocument.Parse(filterTemplate);
					var fieldMappings = filterDataXml.Descendants("FilterStrip").ToList();

					if (shouldReadModuleData)
					{
						var moduleDataxml = XDocument.Parse((string)reader["ModuleData"]);
						var moduleMappings = moduleDataxml.Descendants("ModuleFilter").ToList();
						PopulateIndexWithModuleData(fieldMappings, moduleMappings, oldCompetitorCodeDescription, newFilterDescription, competitorCodes, indexes);
					}
					else
					{
						PopulateIndexWithoutModuleData(fieldMappings, oldCompetitorCodeDescription, newFilterDescription, competitorCodes, indexes);
					}

					if (indexes.Count > 0)
					{
						var pk = (Guid)reader["S9_PK"];
						updateMappings.Add(pk, filterDataXml);
						updateInfo.Add(pk, (competitorCodes, indexes));
					}
				}
			}

			if (updateMappings.Count > 0)
			{
				SaveChanges(updateMappings, @"update dbo.StmModuleFilter set S9_FilterData = dbo.CLRCompressStringAsBytes(@FilterData) where S9_PK = @PK");
			}

			return updateInfo;
		}

		public void UpdateStmModuleFilterUserDataTable(Dictionary<Guid, (List<string> competitorCodes, List<int> indexes)> updateInfos, string oldElement, string newElement)
		{
			var pkLists = string.Join(",", updateInfos.Keys.ToList().Select(x => "'" + x + "'"));
			var schemeSql = $@"
				SELECT	dbo.CLRUncompressAsString(S0_FilterDataValues) AS FilterDataValues, S0_S9
				FROM	dbo.StmModuleFilterUserData
				WHERE	S0_S9 in ({pkLists})
				";

			var updateMappings = new Dictionary<Guid, XDocument>();
			using (var cmd = Db.Connection.Command(schemeSql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var template = reader["FilterDataValues"].ToString();
					if (string.IsNullOrEmpty(template))
					{
						continue;
					}

					var filterDataXml = XDocument.Parse(template);
					var fieldMappings = filterDataXml.Descendants("ModuleFilter");

					var pk = (Guid)reader["S0_S9"];
					var competitorCodes = updateInfos[pk].competitorCodes;
					var indexes = updateInfos[pk].indexes;

					for (var i = 0; i < indexes.Count; i++)
					{
						var fieldMapping = fieldMappings.ElementAt(indexes[i]);
						var value = fieldMapping.Element(oldElement).Value;
						fieldMapping.Descendants().Remove();
						fieldMapping.Add(new XElement("CompetitorType", competitorCodes[i]));

						if (oldElement.Equals(RelatedPartiesOldXElement) && value.Equals(Guid.Empty.ToString()))
						{
							fieldMapping.Add(new XElement(HasMainCompetitorOnNewXElement, "Y"));
						}
						else
						{
							fieldMapping.Add(new XElement(newElement, value));
						}
					}

					updateMappings.Add(pk, filterDataXml);
				}
			}

			if (updateMappings.Count > 0)
			{
				SaveChanges(updateMappings, @"
UPDATE dbo.StmModuleFilterUserData SET
	S0_FilterDataValues      = dbo.CLRCompressStringAsBytes(@FilterData),
	S0_SystemLastEditTimeUtc = GetUtcDate(),
	S0_SystemLastEditUser    = '~BP'
WHERE
	S0_S9 = @PK");
			}
		}

		public void UpdateStmModuleFilterAndStmModuleFilterUserDataTable(string schemeSql, Dictionary<string, string> filterDescription)
		{
			var updateMappings = new Dictionary<Guid, XDocument>();
			var updateMappingsForUserData = new Dictionary<Guid, XDocument>();

			using (var cmd = Db.Connection.Command(schemeSql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var filterTemplate = reader["FilterData"];
					var isChanged = false;

					if (filterTemplate.ToString().Length == 0)
					{
						continue;
					}

					var filterDataXml = XDocument.Parse((string)filterTemplate);
					var moduleDataxml = XDocument.Parse((string)reader["ModuleData"]);
					var moduleMappings = moduleDataxml.Descendants("ModuleFilter").ToList();
					var fieldMappings = filterDataXml.Descendants("FilterStrip").ToList();

					for (var i = 0; i < fieldMappings.Count; i++)
					{
						var fieldMapping = fieldMappings[i];
						var targetColumn = fieldMapping.Element("FilterDescription");

						if (targetColumn != null)
						{
							if (filterDescription.ContainsKey(targetColumn.Value))
							{
								var moduleMapping = moduleMappings.ElementAt(i);
								var comparer = moduleMapping.Element("Comparer").Value;
								var competitorType = filterDescription[targetColumn.Value];
								if (comparer == "exact")
								{
									targetColumn.Value = "Sales Main Competitor On";
									var value = moduleMapping.Element("Property").Value;
									moduleMapping.Descendants().Remove();
									moduleMapping.Add(new XElement("CompetitorType", competitorType));
									moduleMapping.Add(new XElement("Competitor", value));
									isChanged = true;
								}
								else if (comparer == "is blank")
								{
									targetColumn.Value = "Has Main Competitor On";
									moduleMapping.Descendants().Remove();
									moduleMapping.Add(new XElement("CompetitorType", competitorType));
									moduleMapping.Add(new XElement("HasMainCompetitor", "N"));
									isChanged = true;
								}
								else if (comparer == "is not blank")
								{
									targetColumn.Value = "Has Main Competitor On";
									moduleMapping.Descendants().Remove();
									moduleMapping.Add(new XElement("CompetitorType", competitorType));
									moduleMapping.Add(new XElement("HasMainCompetitor", "Y"));
									isChanged = true;
								}
							}
						}
					}

					if (isChanged)
					{
						var pk = (Guid)reader["S9_PK"];
						updateMappings.Add(pk, filterDataXml);
						updateMappingsForUserData.Add(pk, moduleDataxml);
					}
				}
			}

			if (updateMappings.Count > 0)
			{
				SaveChanges(updateMappings, @"
UPDATE dbo.StmModuleFilter SET
	S9_FilterData = dbo.CLRCompressStringAsBytes(@FilterData),
	S9_SystemLastEditTimeUtc = GetUtcDate(),
	S9_SystemLastEditUser = '~BP'
WHERE
	S9_PK = @PK");
				SaveChanges(updateMappingsForUserData, @"
UPDATE dbo.StmModuleFilterUserData SET
	S0_FilterDataValues = dbo.CLRCompressStringAsBytes(@FilterData),
	S0_SystemLastEditTimeUtc = GetUtcDate(),
	S0_SystemLastEditUser = '~BP'
WHERE
	S0_S9 = @PK");
			}
		}

		void SaveChanges(Dictionary<Guid, XDocument> itemsToUpdate, string query)
		{
			foreach (var pair in itemsToUpdate)
			{
				using (var command = Db.Connection.Command(query))
				{
					command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pair.Key);
					command.AddParameter("@FilterData", SqlDbType.VarChar, pair.Value.ToString());
					command.ExecuteNonQuery();
				}
			}
		}

		void PopulateIndexWithModuleData(List<XElement> fieldMappings, List<XElement> moduleMappings, List<(string Code, string OldFilterDescription)> oldCompetitorCodeDescription, string newFilterDescription, List<string> competitorCodes, List<int> indexes)
		{
			for (var i = 0; i < fieldMappings.Count; i++)
			{
				var targetColumn = fieldMappings[i].Element("FilterDescription");

				if (targetColumn != null && targetColumn.Value == "Related Parties")
				{
					var moduleMapping = moduleMappings[i];
					var partyCode = moduleMapping.Element("PartyType").Value;
					var competitorCode = oldCompetitorCodeDescription.FirstOrDefault(x => x.OldFilterDescription == partyCode).Code;

					if (competitorCode != null)
					{
						var relatedPartyPk = moduleMapping.Element(RelatedPartiesOldXElement).Value;
						targetColumn.Value = relatedPartyPk.Equals(Guid.Empty.ToString()) ? HasMainCompetitorOnFilterDescription : newFilterDescription;
						competitorCodes.Add(competitorCode);
						indexes.Add(i);
					}
				}
			}
		}

		void PopulateIndexWithoutModuleData(List<XElement> fieldMappings, List<(string Code, string OldFilterDescription)> oldCompetitorCodeDescription, string newFilterDescription, List<string> competitorCodes, List<int> indexes)
		{
			for (var i = 0; i < fieldMappings.Count; i++)
			{
				var targetColumn = fieldMappings[i].Element("FilterDescription");

				if (targetColumn != null)
				{
					var competitorCode = oldCompetitorCodeDescription.FirstOrDefault(x => x.OldFilterDescription == targetColumn.Value).Code;

					if (competitorCode != null)
					{
						targetColumn.Value = newFilterDescription;
						competitorCodes.Add(competitorCode);
						indexes.Add(i);
					}
				}
			}
		}

		public const string SalesMainCompetitorOnFilterDescription = "Sales Main Competitor On";
		public const string SalesMainCompetitorOnNewXElement = "Competitor";
		public const string SalesMainCompetitorOnOldXElement = "Property";
		public const string HasMainCompetitorOnFilterDescription = "Has Main Competitor On";
		public const string HasMainCompetitorOnNewXElement = "HasMainCompetitor";
		public const string HasMainCompetitorOnOldXElement = "Property0";
		public const string RelatedPartiesOldXElement = "RelatedParty";
	}
}
