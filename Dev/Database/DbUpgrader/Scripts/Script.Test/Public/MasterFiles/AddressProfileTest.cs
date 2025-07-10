using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(AddressProfile))]
	class AddressProfileTest : DbCreateScriptTest
	{
		public void TestLatitudeLongitude()
		{
			var insertSql = @"
			DECLARE @Lat FLOAT = -27.44;
			DECLARE @Long FLOAT = 153.04374;

			INSERT [dbo].[OrgHeader] ([OH_PK], [OH_Code], [OH_RL_NKClosestPort], [OH_Language], [OH_SystemLastEditTimeUtc], [OH_SystemLastEditUser], [OH_SystemCreateTimeUtc], [OH_SystemCreateUser], [OH_IsActive], [OH_IsValid], [OH_FullName], [OH_IsForwarder], [OH_IsShippingProvider], [OH_IsAirWholesaler], [OH_IsSeaWholesaler], [OH_IsRailProvider], [OH_IsLineHaulProvider], [OH_IsMiscFreightServices], [OH_IsAirCTO], [OH_IsAirLine], [OH_IsBroker], [OH_IsContainerYard], [OH_IsLocalTransport], [OH_IsPackDepot], [OH_IsSeaCTO], [OH_IsShippingLine], [OH_IsUnpackDepot], [OH_IsRailHead], [OH_IsRoadFreightDepot], [OH_IsShippingConsortium], [OH_IsFumigationContractor], [OH_IsGlobalAccount], [OH_IsNationalAccount], [OH_IsSalesLead], [OH_IsCompetitor], [OH_IsTempAccount], [OH_IsPersonalEffectsAccount], [OH_IsUserFlag1], [OH_IsUserFlag2], [OH_IsUserFlag3], [OH_IsUserFlag4], [OH_IsUserFlag5], [OH_IsUserFlag6], [OH_IsUserFlag7], [OH_IsUserFlag8], [OH_IsUserFlag9], [OH_IsUserFlag10], [OH_IsUserFlag11], [OH_IsUserFlag12], [OH_IsUserFlag13], [OH_IsUserFlag14], [OH_IsConsignee], [OH_IsConsignor], [OH_IsTransportClient], [OH_IsWarehouseClient], [OH_IsDistributionCentre], [OH_IsUserFlag15], [OH_IsUserFlag16], [OH_IsUserFlag17], [OH_IsUserFlag18], [OH_IsUserFlag19], [OH_IsUserFlag20], [OH_IsUserFlag21], [OH_IsUserFlag22], [OH_IsUserFlag23], [OH_IsUserFlag24], [OH_IsControllingCustomer], [OH_IsControllingAgent], [OH_Category], [OH_ScreeningStatus], [OH_RSL_ShippingLine], [OH_IsUserFlag25], [OH_IsUserFlag26], [OH_IsUserFlag27], [OH_IsUserFlag28], [OH_IsUserFlag29], [OH_IsUserFlag30], [OH_IsUserFlag31], [OH_IsUserFlag32], [OH_AutoVersion], [OH_IsFerryWaterTerminal], [OH_IsContainerLeasingCompany], [OH_OverrideAdditionalAddressInformation], [OH_IsInlandWaterwayProvider], [OH_IsVGMContractor], [OH_SystemCreateBranch], [OH_SystemCreateDepartment])
			VALUES
				('408a0b69-ecc4-4643-b2ec-24b233c5d1a1', 'WTGAUBNE1', 'AUBNE', 'EN', GetUtcDate(), 'STE', GetUtcDate(), 'E', 1, 1, 'WTG (AU) CORPORATION', 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 'BUS', 'MAT', NULL, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, '', '');
			INSERT [dbo].[OrgAddress] ([OA_PK], [OA_Code], [OA_Address1], [OA_Address2], [OA_State], [OA_PostCode], [OA_Phone], [OA_Fax], [OA_Mobile], [OA_ContainerHandling], [OA_AccessPoint], [OA_LabourRequired], [OA_CommunicationRequired], [OA_Dock_Height], [OA_FCLEquipmentNeeded], [OA_LCLEquipmentNeeded], [OA_AIREquipmentNeeded], [OA_RL_NKRelatedPortCode], [OA_OH], [OA_IsActive], [OA_IsValid], [OA_CompanyNameOverride], [OA_DockLeveler], [OA_ForkLift], [OA_PalletJack], [OA_Email], [OA_DeliveryRoute], [OA_DeliveryRouteSequence], [OA_UseCumulativeFreeWaitingTime], [OA_RN_NKCountryCode], [OA_ValidationStatus], [OA_AddressMap], [OA_AuthorityToLeave], [OA_OtherWarehouseFacilities], [OA_LoadingUnloadingConstraints], [OA_City], [OA_GroupNumber], [OA_AdditionalAddressInformation], [OA_GeofencePolygon], [OA_SuppressAddressValidationError], [OA_SystemCreateTimeUtc], [OA_SystemCreateUser], [OA_SystemLastEditTimeUtc], [OA_SystemLastEditUser], [OA_Language], [OA_VerifiesContainerGrossWeight], [OA_GeoLocation], [OA_JobLoadingDuration], [OA_AutoVersion])
			VALUES
				('a67d6697-7184-4c36-9823-b3507de71a65', 'Sample', 'Sample address', '', 'QLD', '4029', '', '', '', '', '', '', '', '', 'WUP', 'PSL', 'PSL', 'AUBNE', '408a0b69-ecc4-4643-b2ec-24b233c5d1a1', 1, 1, '', 0, 0, 0, 'example.email@wtg.zone', '', 0, 0, 'AU', 'VAD', '', 'DEF', '', '', 'Herston', 0, '', 0xE61000000104000000000000000001000000FFFFFFFFFFFFFFFF03, 0, GetUtcDate(), 'E  ', GetUtcDate(), '~BP', 'EN', 0, geography::Point(@Lat, @Long, 4326), 0, 0);
			INSERT [dbo].[OrgAddressCapability] ([PZ_PK], [PZ_IsValid], [PZ_AddressType], [PZ_IsMainAddress], [PZ_OA], [PZ_AutoVersion], [PZ_SystemCreateTimeUtc], [PZ_SystemCreateUser], [PZ_SystemLastEditTimeUtc], [PZ_SystemLastEditUser])
			VALUES
				(newid(), 1, 'OFC', 1, 'a67d6697-7184-4c36-9823-b3507de71a65', 0, NULL, '', NULL, '');";

			using (var command = TestConnection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}

			var selectSql = @"SELECT Latitude, Longitude FROM dbo.AddressProfile(NULL, 'N', '', 'Forwarder') WHERE OrgPk = '408a0b69-ecc4-4643-b2ec-24b233c5d1a1'";
			using (var command = TestConnection.Command(selectSql))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var latitude = reader[0];
						var longitude = reader[1];
						CombineAssertions(() =>
						{
							AssertEquals("Latitude", -27.44, latitude);
							AssertEquals("Longitude", 153.04374, longitude);
						});
					}
				}
			}
		}
	}
}

