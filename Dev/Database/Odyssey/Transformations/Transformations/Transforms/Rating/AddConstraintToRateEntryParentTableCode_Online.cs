using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Rating
{
	public class AddConstraintToRateEntryParentTableCode_Online : DataTransformation
	{
		public override string UserDescription => "Cleanup data for new constraint on column TI_ParentTableCode";

		const int BatchSize = 10000;
		const string LastProcessedDTExtendedPropertyString = "LastProcessedTI_SystemCreateTimeUtc";

		protected override void OnlinePostUpgradeTransform(CancellationToken cancellationToken)
		{
			manager?.ShowInfoMessage("Cleanup data for new constraint on column TI_ParentTableCode");

			var stopwatch = Stopwatch.StartNew();
			var processedChunks = 0;
			var lastProcessedDTString = ExtProperty.Database.Select(Db.Connection, LastProcessedDTExtendedPropertyString);
			var lastProcessedDT = long.TryParse(lastProcessedDTString, out var parsedTicks)
				? new DateTime(parsedTicks)
				: (DateTime?)null;

			foreach (var chunk in DateTimeChunker.GenerateChunks(BatchSize, lastProcessedDT, RateEntrySchema.TI_SystemCreateTimeUtc))
			{
				using (var cmd = Db.Connection.Command(TransactionalSql))
				{
					cmd.AddParameter("@lowerBound", RateEntrySchema.TI_SystemCreateTimeUtc.SqlDbType, chunk.LowerBound);
					cmd.AddParameter("@upperBound", RateEntrySchema.TI_SystemCreateTimeUtc.SqlDbType, chunk.UpperBound);

					try
					{
						cmd.ExecuteNonQuery();
						processedChunks++;
					}
					catch (Exception ex)
					{
						throw new InvalidOperationException(
							$"Error processing chunk {chunk.LowerBound:u} → {chunk.UpperBound:u}: {ex.Message}", ex
						);
					}
				}

				ExtProperty.Database.Update(Db.Connection, LastProcessedDTExtendedPropertyString, chunk.UpperBound.Ticks.ToString());

				if (stopwatch.Elapsed.TotalMinutes >= 1 || cancellationToken.IsCancellationRequested)
				{
					manager?.ShowInfoMessage(
						$"Processed {processedChunks} chunks; last up to {chunk.UpperBound:u}."
					);

					if (cancellationToken.IsCancellationRequested)
					{
						cancellationToken.ThrowIfCancellationRequested();
					}
					stopwatch.Restart();
				}
			}
		}

		const string TransactionalSql = @"
			BEGIN TRY
				BEGIN TRANSACTION;

				-- CleanupConflicts
				DELETE t1
				FROM RateEntry t1
				INNER JOIN RateEntry t2 ON
					t1.TI_SystemCreateTimeUtc BETWEEN @LowerBound AND @UpperBound AND
					t1.TI_ParentTableCode NOT IN ('', 'WW') AND
					t1.TI_PK <> t2.TI_PK AND
					t1.TI_RateCategory = t2.TI_RateCategory AND
					t1.TI_Mode = t2.TI_Mode AND
					t1.TI_OriginLRC = t2.TI_OriginLRC AND
					t1.TI_RateOrigin = t2.TI_RateOrigin AND
					t1.TI_DestinationLRC = t2.TI_DestinationLRC AND
					t1.TI_RateDestination = t2.TI_RateDestination AND
					t1.TI_AircraftType = t2.TI_AircraftType AND
					t1.TI_ViaLRC = t2.TI_ViaLRC AND
					t1.TI_PlannedLoadLRC = t2.TI_PlannedLoadLRC AND
					t1.TI_PlannedDischargeLRC = t2.TI_PlannedDischargeLRC AND
					t1.TI_FirstLoadLRC = t2.TI_FirstLoadLRC AND
					t1.TI_LastDischargeLRC = t2.TI_LastDischargeLRC AND
					t1.TI_FirstRouteSetLoadPortLRC = t2.TI_FirstRouteSetLoadPortLRC AND
					t1.TI_LastRouteSetDischargePortLRC = t2.TI_LastRouteSetDischargePortLRC AND
					t1.TI_RS_NKServiceLevel_NI = t2.TI_RS_NKServiceLevel_NI AND
					t1.TI_PL_NKCarrierServiceLevel = t2.TI_PL_NKCarrierServiceLevel AND
					t1.TI_RS_NKGatewayServiceLevel = t2.TI_RS_NKGatewayServiceLevel AND
					t1.TI_RS_NKShipmentGatewayServiceLevel = t2.TI_RS_NKShipmentGatewayServiceLevel AND
					t1.TI_RH_NKCommodityCode = t2.TI_RH_NKCommodityCode AND
					t1.TI_FMCTariffID = t2.TI_FMCTariffID AND
					t1.TI_CartagePickupAddressPostCode = t2.TI_CartagePickupAddressPostCode AND
					t1.TI_CartageDeliveryAddressPostCode = t2.TI_CartageDeliveryAddressPostCode AND
					t1.TI_TransitTime = t2.TI_TransitTime AND
					t1.TI_PaymentTerm = t2.TI_PaymentTerm AND
					t1.TI_GatewayAgentType = t2.TI_GatewayAgentType AND
					t1.TI_Frequency = t2.TI_Frequency AND
					t1.TI_FrequencyUnit = t2.TI_FrequencyUnit AND
					t1.TI_IsCrossTrade = t2.TI_IsCrossTrade AND
					t1.TI_IsTact = t2.TI_IsTact AND
					t1.TI_MatchContainerRateClass = t2.TI_MatchContainerRateClass AND
					t1.TI_ContractNumber = t2.TI_ContractNumber AND
					t1.TI_ShipmentConsolidationStatus = t2.TI_ShipmentConsolidationStatus AND
					t1.TI_HBLDeliveryMode = t2.TI_HBLDeliveryMode AND
					t1.TI_IsNonOperatedReefer = t2.TI_IsNonOperatedReefer AND
					t1.TI_YardUnitType = t2.TI_YardUnitType AND
					t1.TI_YardUnitLoad = t2.TI_YardUnitLoad AND
					t1.TI_TH = t2.TI_TH AND
				(t1.TI_OH_TransportProvider					=	t2.TI_OH_TransportProvider				OR	t1.TI_OH_TransportProvider is NULL				AND t2.TI_OH_TransportProvider IS NULL)					AND
				(t1.TI_OH_Supplier							=	t2.TI_OH_Supplier						OR	t1.TI_OH_Supplier IS NULL						AND t2.TI_OH_Supplier IS NULL)							AND
				(t1.TI_OH_Consignor							=	t2.TI_OH_Consignor						OR	t1.TI_OH_Consignor IS NULL						AND t2.TI_OH_Consignor IS NULL)							AND
				(t1.TI_OH_Consignee							=	t2.TI_OH_Consignee						OR	t1.TI_OH_Consignee IS NULL						AND t2.TI_OH_Consignee IS NULL)							AND
				(t1.TI_OH_ControllingCustomer				=	t2.TI_OH_ControllingCustomer			OR	t1.TI_OH_ControllingCustomer IS NULL			AND t2.TI_OH_ControllingCustomer IS NULL)				AND
				(t1.TI_OA_CartagePickupAddressOverride		=	t2.TI_OA_CartagePickupAddressOverride	OR	t1.TI_OA_CartagePickupAddressOverride IS NULL	AND t2.TI_OA_CartagePickupAddressOverride IS NULL)		AND
				(t1.TI_OA_CartageDeliveryAddressOverride	=	t2.TI_OA_CartageDeliveryAddressOverride	OR	t1.TI_OA_CartageDeliveryAddressOverride IS NULL	AND t2.TI_OA_CartageDeliveryAddressOverride IS NULL)	AND
				(t1.TI_TZ_OriginZone						=	t2.TI_TZ_OriginZone						OR	t1.TI_TZ_OriginZone IS NULL						AND t2.TI_TZ_OriginZone IS NULL)						AND
				(t1.TI_TZ_DestinationZone					=	t2.TI_TZ_DestinationZone				OR	t1.TI_TZ_DestinationZone IS NULL				AND t2.TI_TZ_DestinationZone IS NULL)					AND
				(t1.TI_R9_FromSuburb						=	t2.TI_R9_FromSuburb						OR	t1.TI_R9_FromSuburb IS NULL						AND t2.TI_R9_FromSuburb IS NULL)						AND
				(t1.TI_R9_ToSuburb							=	t2.TI_R9_ToSuburb						OR	t1.TI_R9_ToSuburb IS NULL						AND t2.TI_R9_ToSuburb IS NULL)							AND
				(t2.TI_RateEndDate IS NULL OR t1.TI_RateStartDate<=t2.TI_RateEndDate) AND
				(t1.TI_RateEndDate IS NULL OR t2.TI_RateStartDate<=t1.TI_RateEndDate)
			OUTER APPLY [dbo].GetRateEntryContainerClass(t1.TI_RC, t1.TI_RateCategory) AS LeftContainerClass
			OUTER APPLY [dbo].GetRateEntryContainerClass(t2.TI_RC, t2.TI_RateCategory) AS RightAddedContainerClass
			WHERE (
				(t1.TI_RC = t2.TI_RC OR (t1.TI_RC IS NULL AND t2.TI_RC IS NULL) OR t1.TI_RC <> t2.TI_RC)
				AND t1.TI_MatchContainerRateClass = 1
				AND LeftContainerClass.ContainerClass = RightAddedContainerClass.ContainerClass
			)

				-- UpdateInvalidParentTableCode
				UPDATE dbo.RateEntry
				SET 
					TI_ParentTableCode = '',
					TI_ParentID = NULL,
					TI_SystemLastEditTimeUtc = GETUTCDATE(),
					TI_SystemLastEditUser = '~BP'
				WHERE 
					TI_SystemCreateTimeUtc BETWEEN @LowerBound AND @UpperBound
					AND TI_ParentTableCode NOT IN ('', 'WW');

				COMMIT TRANSACTION;
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
					ROLLBACK TRANSACTION;
				THROW;
			END CATCH;
			";
	}
}
