using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	sealed class UpdateGlowAuditIndexFields : DataTransformation
	{
		public override string UserDescription => "Update audit fields' names to standard names";

		protected override void OfflinePostUpgradeTransform()
		{
			var dataOfUpdateStmModuleFilterForCW1 =
				new List<(IList<string> moduleIDs, string oldName, string newName)> {
					(
						[
							"CarrierShipmentHeader",
							"DtbConsignment",
							"HVLVConsignment",
							"WhsVASOrder",
							"DtbConsignmentRunSheet",
						],
						"CREATINGUSER",
						"CREATEUSER"
					),
					(
						[
							"JobConsol",
							"ShipmentReceival",
							"JobShipment",
						],
						"SYSTEMCREATETIMEUTC",
						"CREATETIME"
					),
				};

			var dataOfUpdateStmModuleFilterForGlow =
				new List<(IList<string> moduleIDs, string oldName, string newName)> {
					(
						[
							"IEntityInfo_ICarrierShipment",
							"SDT_ICarrierShipment_547b1faa060c41aebe6036480709dabb_51159077-d462-49dc-aa0c-d47d205b2557",
							"IEntityInfo_IDtbConsignment_5d7bb187f1124668a0615f856789a49e",
							"SDT_IDtbConsignment_c8d1e0e7eb8b495eb5edc3f7f0cc4205_e7166e6c-743e-4783-99b4-ba3f2ed2c343",
							"SEP_NeoDashboard_34bac4a067db46d8a2dd07aca6ffccde_b8b4be9f-d607-4606-8fa7-ad18ca67c7f7",
							"SEP_NeoDashboard_88498f6e7d394d2ea3a01d1f35248609_0c192a43-d287-4c30-b024-dcf0edd08ae4",
							"IEntityInfo_IDtbConsignment",
							"IEntityInfo_IHVLVConsignment_8a85c607db774a4da7ce0622d9c51236",
							"IEntityInfo_IHVLVConsignment_3c95f54513ea4829aebfe9ffb9b4c647",
							"IEntityInfo_IHVLVConsignment_94d76ffabddb4f35938a7dfb833a35a6",
							"SEP_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f",
							"IEntityInfo_IHVLVConsignment",
							"IEntityInfo_NeoDashboard",
							"SDT_WTJ_3b2982abef5141ba94b8093d1ff752ae_766ae519-8900-4dd3-8b49-06d65eba921f",
							"IEntityInfo_WTJ",
							"SDT_WTT_3b2982abef5141ba94b8093d1ff752ae_597caefa-d7f7-4fa6-be4c-caaee47cccbb",
							"SDT_WTT_f94a65e8e6694b44b58869bd9992a1d7_89ffa89c-fccc-4c80-bbcb-2ff872f115e7",
							"IEntityInfo_WTT",
						],
						"CREATINGUSER",
						"CREATEUSER"
					),
					(
						[
							"IEntityInfo_IJobConsol_8c7c3344d28b4def9060b51894d52560",
							"IEntityInfo_IJobConsol_af0c1402f3924606bf567dfea264632e",
							"IEntityInfo_IJobConsol",
							"SDT_IJobShipment_3cfd7b375ace42ddb34220951a47a161_67d54bfc-4eb7-4b7b-9ba4-edbd29c38cbe",
							"SDT_IJobShipment_fad31d07b17b44958d9f68f90cef1101_47687897-4072-40d4-999e-008845b7beda",
							"SDT_IJobShipment_bb06ff77067a442ca63d5e025dbedc48_e03271ec-9051-4093-a444-0dcc9af589af",
							"SDT_IJobShipment_0f6326e99f3b44bca9c12ccd34493e8d_080d4b37-413b-477f-9feb-edb1560ac2c9",
							"SDT_IJobShipment_0e460a51cc2b42228f97d8fb6b5b2bee_94d7cd33-a5b2-49d8-aa59-7e1cb1a342af",
							"IEntityInfo_IJobShipment_8c36363ac8a541969bf0b0f8ed7bfd61",
							"IEntityInfo_IJobShipment_8e1d92d31df945e7ac840a4a2efab8bb",
							"IEntityInfo_IJobShipment_79cf3e9f175a4a67af77918cc5984c3d",
							"SEP_IJobShipment_189a43c94e784fa28743ce5e22a8951e_490465d1-5497-4aa9-bba8-fbac663311f4",
							"IEntityInfo_IJobShipment_6304763e5b644c069fb98fe353716da5",
							"IEntityInfo_IJobShipment_d4f1974737b64b2c9ccfc3fe6214292e",
							"SEP_NeoDashboard_34bac4a067db46d8a2dd07aca6ffccde_b8b4be9f-d607-4606-8fa7-ad18ca67c7f7",
							"SEP_NeoDashboard_88498f6e7d394d2ea3a01d1f35248609_0c192a43-d287-4c30-b024-dcf0edd08ae4",
							"IEntityInfo_IJobShipment",
							"IEntityInfo_NeoDashboard",
						],
						"SYSTEMCREATETIMEUTC",
						"CREATETIME"
					),
					(
						[
							"IEntityInfo_IWhsItemCycleCountLocation_aedc60ad8fb3489c852f2e27527f0047",
							"IEntityInfo_IWhsItemCycleCountLocation",
						],
						"CREATEDDATE",
						"CREATETIME"
					)
				};

			var dataOfUpdateStmDataForGlow =
				new List<(IList<string> sdNames, string oldName, string newName)>
				{
					(
						[
							"GridSettings_SDT_ICarrierShipment_547b1faa060c41aebe6036480709dabb_51159077-d462-49dc-aa0c-d47d205b2557",
							"GridSettings_Search_ICarrierShipment_%",
							"GridSettings_Search_IDtbConsignment",
							"GridSettings_SDT_IDtbConsignment_c8d1e0e7eb8b495eb5edc3f7f0cc4205_e7166e6c-743e-4783-99b4-ba3f2ed2c343",
							"GridSettings_SEP_NeoDashboard_34bac4a067db46d8a2dd07aca6ffccde_b8b4be9f-d607-4606-8fa7-ad18ca67c7f7",
							"GridSettings_SEP_NeoDashboard_88498f6e7d394d2ea3a01d1f35248609_0c192a43-d287-4c30-b024-dcf0edd08ae4",
							"GridSettings_Search_IDtbConsignment_%",
							"GridSettings_Search_NeoDashboard_%",
							"GridSettings_Trackable_3c95f54513ea4829aebfe9ffb9b4c647_Search_IHVLVConsignment",
							"GridSettings_Trackable_8a85c607db774a4da7ce0622d9c51236_Search_IHVLVConsignment",
							"GridSettings_Trackable_94d76ffabddb4f35938a7dfb833a35a6_Search_IHVLVConsignment",
							"GridSettings_SEP_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f",
							"GridSettings_Trackable_3c95f54513ea4829aebfe9ffb9b4c647_Search_IHVLVConsignment_%",
							"GridSettings_Trackable_8a85c607db774a4da7ce0622d9c51236_Search_IHVLVConsignment_%",
							"GridSettings_Trackable_94d76ffabddb4f35938a7dfb833a35a6_Search_IHVLVConsignment_%",
							"GridSettings_Search_IHVLVConsignment_%",
							"GridSettings_SDT_WTJ_3b2982abef5141ba94b8093d1ff752ae_766ae519-8900-4dd3-8b49-06d65eba921f",
							"GridSettings_Search_WTJ_%",
							"GridSettings_SDT_WTT_3b2982abef5141ba94b8093d1ff752ae_597caefa-d7f7-4fa6-be4c-caaee47cccbb",
							"GridSettings_SDT_WTT_f94a65e8e6694b44b58869bd9992a1d7_89ffa89c-fccc-4c80-bbcb-2ff872f115e7",
							"GridSettings_Search_WTT_%",
						],
						"CREATINGUSER",
						"CREATEUSER"
					),
					(
						[
							"GridSettings_Search_IJobConsol",
							"GridSettings_Search_IJobConsol_%",
							"GridSettings_SDT_IJobShipment_3cfd7b375ace42ddb34220951a47a161_67d54bfc-4eb7-4b7b-9ba4-edbd29c38cbe",
							"GridSettings_SDT_IJobShipment_fad31d07b17b44958d9f68f90cef1101_47687897-4072-40d4-999e-008845b7beda",
							"GridSettings_SDT_IJobShipment_bb06ff77067a442ca63d5e025dbedc48_e03271ec-9051-4093-a444-0dcc9af589af",
							"GridSettings_SDT_IJobShipment_0f6326e99f3b44bca9c12ccd34493e8d_080d4b37-413b-477f-9feb-edb1560ac2c9",
							"GridSettings_SDT_IJobShipment_0e460a51cc2b42228f97d8fb6b5b2bee_94d7cd33-a5b2-49d8-aa59-7e1cb1a342af",
							"GridSettings_Search_IJobShipment",
							"GridSettings_Trackable_79cf3e9f175a4a67af77918cc5984c3d_Search_IJobShipment",
							"GridSettings_SEP_IJobShipment_189a43c94e784fa28743ce5e22a8951e_490465d1-5497-4aa9-bba8-fbac663311f4",
							"GridSettings_Trackable_6304763e5b644c069fb98fe353716da5_Search_IJobShipment",
							"GridSettings_Trackable_d4f1974737b64b2c9ccfc3fe6214292e_Search_IJobShipment",
							"GridSettings_Trackable_d4f1974737b64b2c9ccfc3fe6214292e_Search_IJobShipment_%",
							"GridSettings_SEP_NeoDashboard_34bac4a067db46d8a2dd07aca6ffccde_b8b4be9f-d607-4606-8fa7-ad18ca67c7f7",
							"GridSettings_SEP_NeoDashboard_88498f6e7d394d2ea3a01d1f35248609_0c192a43-d287-4c30-b024-dcf0edd08ae4",
							"GridSettings_Search_IJobShipment_%",
							"GridSettings_Trackable_79cf3e9f175a4a67af77918cc5984c3d_Search_IJobShipment_%",
							"GridSettings_Trackable_6304763e5b644c069fb98fe353716da5_Search_IJobShipment_%",
							"GridSettings_Search_NeoDashboard_%",
						],
						"SYSTEMCREATETIMEUTC",
						"CREATETIME"
					),
					(
						[
							"GridSettings_Search_IWhsItemCycleCountLocation",
							"GridSettings_Search_IWhsItemCycleCountLocation_%",
						],
						"CREATEDDATE",
						"CREATETIME"
					)
				};

			dataOfUpdateStmModuleFilterForCW1.ForEach(UpdateStmModuleFilterForCW1);

			dataOfUpdateStmModuleFilterForGlow.ForEach(UpdateStmModuleFilterForGlow);

			dataOfUpdateStmDataForGlow.ForEach(UpdateStmDataForGlow);
		}

		void UpdateStmModuleFilterForCW1((IList<string> moduleIDs, string oldName, string newName) tuple)
		{
			var sql = @$"
SELECT
	S9_PK, dbo.CLRUncompressAsString(S9_FilterData) AS FilterData
FROM
	dbo.StmModuleFilter
WHERE 1 = 1
	AND S9_IsIndexSearch = 1
	AND S9_ModuleID IN ({GetSqlStringForList(tuple.moduleIDs)})
	AND TRY_CAST(dbo.CLRUncompressAsString(S9_FilterData) as XML).exist('//*[local-name()=""FilterStrip""][*[local-name() = ""FilterDescription"" and lower-case(text()[1]) = ""{tuple.oldName.ToLowerInvariant()}""]]') = 1";
			var pkFilterDataMap = new Dictionary<Guid, string>();
			using (var command = Db.Connection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					if (reader["FilterData"] is string filterDataString)
					{
						pkFilterDataMap.Add((Guid)reader["S9_PK"], filterDataString);
					}
				}
			}

			foreach (var pkFilterDataMapItem in pkFilterDataMap)
			{
				var filterDataXml = XDocument.Parse(pkFilterDataMapItem.Value);
				var filterStrips = filterDataXml.Descendants().Where(x => x.Name.LocalName.Equals("FilterStrip"));
				var filterStripIndex = 0;
				var needsUpdateIndexesInStmModuleFilterUserData = new List<int>();
				foreach (var filterStrip in filterStrips)
				{
					var filterDescription = filterStrip.Elements().FirstOrDefault(x => x.Name.LocalName.Equals("FilterDescription"));
					if (filterDescription != null)
					{
						if (filterDescription.Value.Equals(tuple.oldName, StringComparison.OrdinalIgnoreCase))
						{
							filterDescription.Value = tuple.newName;
							needsUpdateIndexesInStmModuleFilterUserData.Add(filterStripIndex);
						}
						else if (filterDescription.Value.Equals(tuple.newName, StringComparison.OrdinalIgnoreCase))
						{
							needsUpdateIndexesInStmModuleFilterUserData.Add(filterStripIndex);
						}
					}
					filterStripIndex++;
				}

				if (needsUpdateIndexesInStmModuleFilterUserData.Count > 0)
				{
					sql = @"
UPDATE
	dbo.StmModuleFilter
SET
	S9_FilterData = @FilterData,
	S9_SystemLastEditTimeUtc = GETUTCDATE(),
	S9_SystemLastEditUser = '~BP'
WHERE
	S9_PK = @S9PK";

					using (var command = Db.Connection.Command(sql))
					{
						command.AddParameter("@S9PK", SqlDbType.UniqueIdentifier, pkFilterDataMapItem.Key);
						command.AddParameter("@FilterData", SqlDbType.VarBinary, Encoding.UTF8.GetBytes(filterDataXml.ToString(SaveOptions.DisableFormatting)));
						command.ExecuteNonQuery();
					}

					if (tuple.newName.Equals("CREATEUSER", StringComparison.OrdinalIgnoreCase))
					{
						sql = @$"
SELECT
	S0_PK, dbo.CLRUncompressAsString(S0_FilterDataValues) AS FilterDataValues
FROM dbo.StmModuleFilterUserData
WHERE
	S0_S9 = @S9PK";
						var pkFilterDataValuesMap = new Dictionary<Guid, string>();
						using (var command = Db.Connection.Command(sql))
						{
							command.AddParameter("S9PK", SqlDbType.UniqueIdentifier, pkFilterDataMapItem.Key);
							using (var reader = command.ExecuteReader())
							{
								while (reader.Read())
								{
									if (reader["FilterDataValues"] is string filterDataValuesString)
									{
										pkFilterDataValuesMap.Add((Guid)reader["S0_PK"], filterDataValuesString);
									}
								}
							}
						}

						foreach (var pkFilterDataValuesMapItem in pkFilterDataValuesMap)
						{
							var dataValuesXML = XDocument.Parse(pkFilterDataValuesMapItem.Value);
							var moduleFilters = dataValuesXML.Descendants().Where(x => x.Name.LocalName.Equals("ModuleFilter"));
							var moduleFilterIndex = 0;
							foreach (var moduleFilter in moduleFilters)
							{
								if (needsUpdateIndexesInStmModuleFilterUserData.Contains(moduleFilterIndex))
								{
									var comparer = moduleFilter.Elements().FirstOrDefault(x => x.Name.LocalName.Equals("Comparer"));
									if (comparer != null)
									{
										if (comparer.Value.Equals("any starts with", StringComparison.OrdinalIgnoreCase)
											|| comparer.Value.Equals("any exact", StringComparison.OrdinalIgnoreCase)
											|| comparer.Value.Equals("all exact", StringComparison.OrdinalIgnoreCase))
										{
											comparer.Value = "exact";
										}
										else if (comparer.Value.Equals("none exact", StringComparison.OrdinalIgnoreCase)
											|| comparer.Value.Equals("no starts with", StringComparison.OrdinalIgnoreCase))
										{
											comparer.Value = "not equal";
										}
									}
								}
								moduleFilterIndex++;
							}

							sql = @"
UPDATE
	dbo.StmModuleFilterUserData
SET
	S0_FilterDataValues = @FilterDataValues,
	S0_SystemLastEditTimeUtc = GETUTCDATE(),
	S0_SystemLastEditUser = '~BP'
WHERE
	S0_PK = @S0PK";

							using (var command = Db.Connection.Command(sql))
							{
								command.AddParameter("@S0PK", SqlDbType.UniqueIdentifier, pkFilterDataValuesMapItem.Key);
								command.AddParameter("@FilterDataValues", SqlDbType.VarBinary, Encoding.UTF8.GetBytes(dataValuesXML.ToString(SaveOptions.DisableFormatting)));
								command.ExecuteNonQuery();
							}
						}
					}
				}
			}
		}

		void UpdateStmModuleFilterForGlow((IList<string> moduleIDs, string oldName, string newName) tuple)
		{
			var sql = @$"
SELECT
	S9_PK, dbo.CLRUncompressAsString(S9_FilterData) AS FilterData
FROM dbo.StmModuleFilter
WHERE 1 = 1
	AND S9_ModuleID IN ({GetSqlStringForList(tuple.moduleIDs)})
	AND TRY_CAST(dbo.CLRUncompressAsString(S9_FilterData) as XML).exist('//*[local-name()=""Filter""][*[local-name() = ""PropertyPath"" and lower-case(text()[1]) = ""{tuple.oldName.ToLowerInvariant()}""]]') = 1";
			var pkFilterDataMap = new Dictionary<Guid, string>();
			using (var command = Db.Connection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					if (reader["FilterData"] is string filterDataString)
					{
						pkFilterDataMap.Add((Guid)reader["S9_PK"], filterDataString);
					}
				}
			}

			foreach (var pkFilterDataMapItem in pkFilterDataMap)
			{
				var filterDataXml = XDocument.Parse(pkFilterDataMapItem.Value);
				var filters = filterDataXml.Descendants().Where(x => x.Name.LocalName.Equals("Filter"));
				foreach (var filter in filters)
				{
					var propertyPath = filter.Elements().FirstOrDefault(x => x.Name.LocalName.Equals("PropertyPath"));
					if (propertyPath != null)
					{
						if (propertyPath.Value.Equals(tuple.oldName, StringComparison.OrdinalIgnoreCase))
						{
							propertyPath.Value = tuple.newName;
						}

						if (propertyPath.Value.Equals("CREATEUSER", StringComparison.OrdinalIgnoreCase))
						{
							var filterType = filter.Elements().FirstOrDefault(x => x.Name.LocalName.Equals("FilterType"));
							if (filterType != null && filterType.Value.Equals("StringFilter", StringComparison.OrdinalIgnoreCase))
							{
								filterType.Value = "SimpleLookupFilter";
							}

							var operation = filter.Elements().FirstOrDefault(x => x.Name.LocalName.Equals("Operation"));
							if (operation != null)
							{
								if (operation.Value.Equals("StartsWith", StringComparison.OrdinalIgnoreCase)
									|| operation.Value.Equals("Contains", StringComparison.OrdinalIgnoreCase))
								{
									operation.Value = "Is";
								}
								else if (operation.Value.Equals("NotStarting", StringComparison.OrdinalIgnoreCase))
								{
									operation.Value = "IsNot";
								}
							}
						}
					}
				}

				sql = @"
UPDATE
	dbo.StmModuleFilter
SET
	S9_FilterData = @FilterData,
	S9_SystemLastEditTimeUtc = GETUTCDATE(),
	S9_SystemLastEditUser = '~BP'
WHERE
	S9_PK = @S9PK";

				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameter("@S9PK", SqlDbType.UniqueIdentifier, pkFilterDataMapItem.Key);
					command.AddParameter("@FilterData", SqlDbType.VarBinary, Encoding.UTF8.GetBytes(filterDataXml.ToString(SaveOptions.DisableFormatting)));
					command.ExecuteNonQuery();
				}
			}
		}

		void UpdateStmDataForGlow((IList<string> sdNames, string oldName, string newName) tuple)
		{
			var sql = @$"
SELECT
	SD_PK, dbo.CLRUncompressAsString(SD_BinaryValue) AS BinaryValue
FROM
	dbo.StmData
WHERE 1 = 1
	AND ({GetSqlStringForLikeList(tuple.sdNames, "SD_NAME")})
	AND
	(
		TRY_CAST(dbo.CLRUncompressAsString(SD_BinaryValue) as XML).exist('//*[local-name()=""GridSortDefinition""][*[local-name() = ""FieldName"" and lower-case(text()[1]) = ""{tuple.oldName.ToLowerInvariant()}""]]') = 1
		OR
		TRY_CAST(dbo.CLRUncompressAsString(SD_BinaryValue) as XML).exist('//*[local-name()=""GridColumnDefinition""][*[local-name() = ""FieldName"" and lower-case(text()[1]) = ""{tuple.oldName.ToLowerInvariant()}""]]') = 1
	)";
			var pkBinaryValueMap = new Dictionary<Guid, string>();
			using (var command = Db.Connection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (reader["BinaryValue"] is string binaryValueString)
						{
							pkBinaryValueMap.Add((Guid)reader["SD_PK"], binaryValueString);
						}
					}
				}
			}

			foreach (var pkBinaryValueMapItem in pkBinaryValueMap)
			{
				var binaryValueXml = XDocument.Parse(pkBinaryValueMapItem.Value);
				var oldNameNodes = binaryValueXml.Descendants().Where(x
					=> x.Name.LocalName.Equals("FieldName")
					&& x.Value.Equals(tuple.oldName, StringComparison.OrdinalIgnoreCase)
					&& (x.Parent.Name.LocalName.Equals("GridSortDefinition") || x.Parent.Name.LocalName.Equals("GridColumnDefinition")));

				if (oldNameNodes.Any())
				{
					oldNameNodes.ForEach(x => x.Value = tuple.newName);

					sql = @"
UPDATE
dbo.StmData
SET
SD_BinaryValue = @BinaryValue,
SD_SystemLastEditTimeUtc = GETUTCDATE(),
SD_SystemLastEditUser = '~BP'
WHERE
SD_PK = @SDPK";

					using (var command = Db.Connection.Command(sql))
					{
						command.AddParameter("@SDPK", SqlDbType.UniqueIdentifier, pkBinaryValueMapItem.Key);
						command.AddParameter("@BinaryValue", SqlDbType.VarBinary, Encoding.UTF8.GetBytes(binaryValueXml.ToString(SaveOptions.DisableFormatting)));
						command.ExecuteNonQuery();
					}
				}
			}
		}

		string GetSqlStringForList(IList<string> list) => string.Join(", ", list.Select(x => $"'{x}'"));

		string GetSqlStringForLikeList(IList<string> list, string propertyName) => string.Join(" OR ", list.Select(x => $"{propertyName} LIKE '{x}'"));
	}
}
