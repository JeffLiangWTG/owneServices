using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPERateDataImporter : RateDataImporter, IObsoleteValidation
	{
		bool hasStartDate;
		ZDateTime startDate;

		#region Constants

		static class Constants
		{
			public const int MinimumRows = 5;
			public const int MinimumColumns = 1;

			public static class ServiceLevels
			{
				public const string EXPRESS = "EXPRESS";
				public const string EXPEDITED = "EXPEDITED";
			}

			public static class ShipmentTypes
			{
				public const string LETTERS = "LETTERS";
				public const string DOCUMENTS = "DOCUMENTS";
				public const string PACKAGES = "PACKAGES";
				public const string SAVER = "SAVER";
			}

			public const int ShipmentTypeRowIndex = 0;
			public const int ShipmentTypeColumnIndex = 0;
			public const int ServiceLevelRowIndex = 1;
			public const int ServiceLevelColumnIndex = 0;
			public const int WeighBreakStartIndex = 4;
			public const int RegionRowIndex = 2;
		}

		#endregion

		protected UPERateDataImporter(RatingHeader companyTariff)
			: base(companyTariff)
		{
			this.companyTariff = companyTariff;
		}

		#region Overrides

		protected override bool IsActiveCore
		{
			get { return companyTariff.IsTariff(); }
		}

		protected override bool EnableIATARateImportCore
		{
			get { return companyTariff.IsTariff(); }
		}

		public override bool IsStandardTACTImport
		{
			get { return false; }
		}

		public override void ImportIATA_TACT(string fileName, Stream stream)
		{
			string[][] data = ImportFile(stream);
			if (IsImportedDataValid(data))
			{
				companyTariff.Factory.Save();

				ZString dateValue = Regex.Match(Path.GetFileName(fileName), @"20\d{2}(\d{4})?").Value;
				if (!dateValue.IsDefault)
				{
					hasStartDate = ZDateTime.TryParseExact(dateValue, out startDate, "yyyyMMdd".Substring(0, dateValue.Length));
				}
				else
				{
					hasStartDate = false;
				}

				if (ProcessData(data))
				{
					UpdateOverlappingsDeleteDuplicates();
					companyTariff.Factory.Save();
					RateEntryCollection.Reload(true, true);
					ImportReport = "Import Completed.";
				}
			}
			else
			{
				ImportReport = "The file you have chosen to import is invalid.";
			}

			if (ImportCompletedHandler != null)
			{
				ImportCompletedHandler();
			}
		}

		#endregion

		#region Import and Validate File

		string[][] ImportFile(Stream stream)
		{
			using (ExcelInterface excelDoc = new ExcelInterface())
			{
				excelDoc.LoadExcelFile(stream);
				stream.Close();
				stream.Dispose();
				excelDoc.ActiveWorksheet = 0;

				var worksheet = excelDoc.WorkSheets[0];

				var rowCount = worksheet.RowCount;
				var columnCount = worksheet.ColumnCount;

				// Ignore trailing empty rows at the bottom
				for (var rowIndex = rowCount - 1; rowIndex > 0; rowIndex--)
				{
					var currentRow = Enumerable.Range(0, columnCount).Select(x => worksheet[rowIndex, x].ToString().Trim());
					if (currentRow.All(string.IsNullOrEmpty))
					{
						continue;
					}

					rowCount = rowIndex + 1;
					break;
				}

				// Ignore trailing empty columns at the very right
				for (var colIndex = columnCount - 1; colIndex > 0; colIndex--)
				{
					var currentColumn = Enumerable.Range(0, rowCount).Select(x => worksheet[x, colIndex].ToString().Trim());
					if (currentColumn.All(string.IsNullOrEmpty))
					{
						continue;
					}

					columnCount = colIndex + 1;
					break;
				}

				var result = new string[rowCount][];

				for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
				{
					var columns = new string[columnCount];

					for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
					{
						columns[columnIndex] = worksheet[rowIndex, columnIndex].ToString().Trim();
					}

					result[rowIndex] = columns;
				}

				return result;
			}
		}

		bool IsImportedDataValid(string[][] data)
		{
			return data.Length >= Constants.MinimumRows && data[0].Length >= Constants.MinimumColumns;
		}

		#endregion

		bool ProcessData(string[][] data)
		{
			bool result = false;

			string shipmentType = data[Constants.ShipmentTypeRowIndex][Constants.ShipmentTypeColumnIndex].ToUpper();
			string serviceLevel = data[Constants.ServiceLevelRowIndex][Constants.ServiceLevelColumnIndex].ToUpper();

			if (serviceLevel == Constants.ServiceLevels.EXPRESS)
			{
				if (shipmentType == Constants.ShipmentTypes.LETTERS)
				{
					ProcessFlatRateImport(data);
					result = true;
				}
				else if (shipmentType == Constants.ShipmentTypes.DOCUMENTS)
				{
					ProcessCombinedImport(data, UPERatingConstants.ServiceLevels.Documents);
					result = true;
				}
				else if (shipmentType == Constants.ShipmentTypes.PACKAGES)
				{
					ProcessCombinedImport(data, UPERatingConstants.ServiceLevels.ExpressPackages);
					result = true;
				}
				else if (shipmentType == Constants.ShipmentTypes.SAVER)
				{
					ProcessCombinedImport(data, UPERatingConstants.ServiceLevels.ExpressSaver);
					result = true;
				}
			}
			else if (serviceLevel == Constants.ServiceLevels.EXPEDITED && shipmentType == Constants.ShipmentTypes.PACKAGES)
			{
				ProcessCombinedImport(data, UPERatingConstants.ServiceLevels.ExpeditedPackages);
				result = true;
			}
			else
			{
				ImportReport = "The file you have chosen to import is invalid.";
			}

			return result;
		}

		void ProcessFlatRateImport(string[][] data)
		{
			string[] regions = data[Constants.RegionRowIndex];
			string[] flatRates = data[Constants.WeighBreakStartIndex];

			for (int regionIndex = 0; regionIndex < regions.Length; regionIndex++)
			{
				RateEntry airEntry = CreateDefaultEntry(UPERatingConstants.ServiceLevels.Envelopes, regions[regionIndex]);
				airEntry.RateLines.RemoveAndDeleteAll();

				RateLine rateLine = airEntry.RateLines.AddNew();
				rateLine.TL_AC = Env.Registry.FreightChargeCode;
				rateLine.TL_RateCalculator = FlatCalculator.Code;

				ZDecimal flatRate = 0m;
				ZDecimal.TryParse(flatRates[regionIndex], out flatRate);
				((FlatCalculator)rateLine.Calculator).BaseRate = flatRate;
			}
		}

		void UpdateOverlappingsDeleteDuplicates()
		{
			foreach (RateEntry rateEntry in RateEntryCollection)
			{
				var query = @"DECLARE @OverlappingTI_PK TABLE (TI_PK uniqueidentifier);

INSERT INTO @OverlappingTI_PK (TI_PK)
SELECT TI_PK FROM dbo.RateEntry
WHERE DATEADD(day, -1, @TI_RateStartDate) >= TI_RateStartDate
AND TI_PK IN (SELECT TI_PK FROM [dbo].[GetOverlappingRateEntries](@TI_PK, @TI_RateCategory, @TI_Mode, @TI_OriginLRC, @TI_DestinationLRC, @TI_ViaLRC, @TI_PlannedLoadLRC, @TI_PlannedDischargeLRC, 
	@TI_FirstLoadLRC, @TI_LastDischargeLRC, @TI_FirstRouteSetLoadPortLRC, @TI_LastRouteSetDischargePortLRC, @TI_RS_NKServiceLevel_NI, @TI_PL_NKCarrierServiceLevel, @TI_RS_NKGatewayServiceLevel, @TI_RS_NKShipmentGatewayServiceLevel, 
	@TI_RH_NKCommodityCode, @TI_FMCTariffID, @TI_CartagePickupAddressPostCode, @TI_CartageDeliveryAddressPostCode, @TI_TransitTime, @TI_Frequency, @TI_FrequencyUnit, @TI_IsCrossTrade, @TI_IsTact, @TI_MatchContainerRateClass, @TI_TH, @TI_OH_TransportProvider, @TI_OH_Supplier, 
	@TI_OH_Consignor, @TI_OH_Consignee, @TI_OH_ControllingCustomer, @TI_OA_CartagePickupAddressOverride, @TI_OA_CartageDeliveryAddressOverride, @TI_RateOrigin, @TI_RateDestination, @TI_TZ_OriginZone, @TI_TZ_DestinationZone, @TI_R9_FromSuburb, @TI_R9_ToSuburb, @TI_RC, @TI_RCC_ComponentCode,
	@TI_ContainerUnitSection, @TI_RRC_RepairCode, @TI_RMC_Material, @TI_EstimateType, @TI_REG_EquipmentGrade, @TI_MNRGroup, @TI_RateStartDate, @TI_RateEndDate, @TI_ParentID, @TI_PaymentTerm, @TI_GatewayAgentType, @TI_ContractNumber, @TI_AircraftType, @TI_ShipmentConsolidationStatus, @TI_HBLDeliveryMode, @TI_IsNonOperatedReefer, @TI_YardUnitType, @TI_YardUnitLoad));

UPDATE dbo.RateEntry
SET TI_RateEndDate = DATEADD(day, -1, @TI_RateStartDate) 
WHERE TI_PK IN (SELECT TI_PK FROM @OverlappingTI_PK);

UPDATE dbo.RateLines
SET TL_RateEndDate = DATEADD(day, -1, @TI_RateStartDate) 
FROM dbo.RateEntry JOIN dbo.RateLines ON TI_PK = TL_TI
WHERE TI_PK IN (SELECT TI_PK FROM @OverlappingTI_PK) 
AND TL_RateEndDate > DATEADD(day, -1, @TI_RateStartDate) 
AND (TL_RateStartDate IS NULL OR TL_RateStartDate <= DATEADD(day, -1, @TI_RateStartDate));

DELETE FROM dbo.RateEntry 
WHERE TI_PK IN (SELECT TI_PK FROM [dbo].[GetOverlappingRateEntries](@TI_PK, @TI_RateCategory, @TI_Mode, @TI_OriginLRC, @TI_DestinationLRC, @TI_ViaLRC, @TI_PlannedLoadLRC, @TI_PlannedDischargeLRC,
	@TI_FirstLoadLRC, @TI_LastDischargeLRC, @TI_FirstRouteSetLoadPortLRC, @TI_LastRouteSetDischargePortLRC, @TI_RS_NKServiceLevel_NI, @TI_PL_NKCarrierServiceLevel, @TI_RS_NKGatewayServiceLevel, @TI_RS_NKShipmentGatewayServiceLevel, 
	@TI_RH_NKCommodityCode, @TI_FMCTariffID, @TI_CartagePickupAddressPostCode, @TI_CartageDeliveryAddressPostCode, @TI_TransitTime, @TI_Frequency, @TI_FrequencyUnit, @TI_IsCrossTrade, @TI_IsTact, @TI_MatchContainerRateClass, @TI_TH, @TI_OH_TransportProvider, @TI_OH_Supplier, 
	@TI_OH_Consignor, @TI_OH_Consignee, @TI_OH_ControllingCustomer, @TI_OA_CartagePickupAddressOverride, @TI_OA_CartageDeliveryAddressOverride, @TI_RateOrigin, @TI_RateDestination, @TI_TZ_OriginZone, @TI_TZ_DestinationZone, @TI_R9_FromSuburb, @TI_R9_ToSuburb, @TI_RC, @TI_RCC_ComponentCode,
	@TI_ContainerUnitSection, @TI_RRC_RepairCode, @TI_RMC_Material, @TI_EstimateType, @TI_REG_EquipmentGrade, @TI_MNRGroup, @TI_RateStartDate, @TI_RateEndDate, @TI_ParentID, @TI_PaymentTerm, @TI_GatewayAgentType, @TI_ContractNumber, @TI_AircraftType, @TI_ShipmentConsolidationStatus, @TI_HBLDeliveryMode, @TI_IsNonOperatedReefer, @TI_YardUnitType, @TI_YardUnitLoad));";

				using (var command = Db.Connection.Command(query, 1200))
				{
					command.AddParameterBasedOnDbColumn("@TI_PK", rateEntry.PK.ToSqlParameter(), RateEntrySchema.PK);
					command.AddParameterBasedOnDbColumn("@TI_RateCategory", rateEntry.TI_RateCategory.ToString(), RateEntrySchema.TI_RateCategory);
					command.AddParameterBasedOnDbColumn("@TI_Mode", rateEntry.TI_Mode.ToString(), RateEntrySchema.TI_Mode);
					command.AddParameterBasedOnDbColumn("@TI_OriginLRC", rateEntry.TI_OriginLRC.ToString(), RateEntrySchema.TI_OriginLRC);
					command.AddParameterBasedOnDbColumn("@TI_DestinationLRC", rateEntry.TI_DestinationLRC.ToString(), RateEntrySchema.TI_DestinationLRC);
					command.AddParameterBasedOnDbColumn("@TI_ViaLRC", rateEntry.TI_ViaLRC.ToString(), RateEntrySchema.TI_ViaLRC);
					command.AddParameterBasedOnDbColumn("@TI_PlannedLoadLRC", rateEntry.TI_PlannedLoadLRC.ToString(), RateEntrySchema.TI_PlannedLoadLRC);
					command.AddParameterBasedOnDbColumn("@TI_PlannedDischargeLRC", rateEntry.TI_PlannedDischargeLRC.ToString(), RateEntrySchema.TI_PlannedDischargeLRC);
					command.AddParameterBasedOnDbColumn("@TI_FirstLoadLRC", rateEntry.TI_FirstLoadLRC.ToString(), RateEntrySchema.TI_FirstLoadLRC);
					command.AddParameterBasedOnDbColumn("@TI_LastDischargeLRC", rateEntry.TI_LastDischargeLRC.ToString(), RateEntrySchema.TI_LastDischargeLRC);
					command.AddParameterBasedOnDbColumn("@TI_FirstRouteSetLoadPortLRC", rateEntry.TI_FirstRouteSetLoadPortLRC.ToString(), RateEntrySchema.TI_FirstRouteSetLoadPortLRC);
					command.AddParameterBasedOnDbColumn("@TI_LastRouteSetDischargePortLRC", rateEntry.TI_LastRouteSetDischargePortLRC.ToString(), RateEntrySchema.TI_LastRouteSetDischargePortLRC);
					command.AddParameterBasedOnDbColumn("@TI_RS_NKServiceLevel_NI", rateEntry.TI_RS_NKServiceLevel_NI.ToString(), RateEntrySchema.TI_RS_NKServiceLevel_NI);
					command.AddParameterBasedOnDbColumn("@TI_PL_NKCarrierServiceLevel", rateEntry.TI_PL_NKCarrierServiceLevel.ToString(), RateEntrySchema.TI_PL_NKCarrierServiceLevel);
					command.AddParameterBasedOnDbColumn("@TI_RS_NKGatewayServiceLevel", rateEntry.TI_RS_NKGatewayServiceLevel.ToString(), RateEntrySchema.TI_RS_NKGatewayServiceLevel);
					command.AddParameterBasedOnDbColumn("@TI_RS_NKShipmentGatewayServiceLevel", rateEntry.TI_RS_NKShipmentGatewayServiceLevel.ToString(), RateEntrySchema.TI_RS_NKShipmentGatewayServiceLevel);
					command.AddParameterBasedOnDbColumn("@TI_RH_NKCommodityCode", rateEntry.TI_RH_NKCommodityCode.ToString(), RateEntrySchema.TI_RH_NKCommodityCode);
					command.AddParameterBasedOnDbColumn("@TI_FMCTariffID", rateEntry.TI_FMCTariffID.ToString(), RateEntrySchema.TI_FMCTariffID);
					command.AddParameterBasedOnDbColumn("@TI_CartagePickupAddressPostCode", rateEntry.TI_CartagePickupAddressPostCode.ToString(), RateEntrySchema.TI_CartagePickupAddressPostCode);
					command.AddParameterBasedOnDbColumn("@TI_CartageDeliveryAddressPostCode", rateEntry.TI_CartageDeliveryAddressPostCode.ToString(), RateEntrySchema.TI_CartageDeliveryAddressPostCode);
					command.AddParameterBasedOnDbColumn("@TI_TransitTime", rateEntry.TI_TransitTime.ToString(), RateEntrySchema.TI_TransitTime);
					command.AddParameterBasedOnDbColumn("@TI_Frequency", (int)rateEntry.TI_Frequency, RateEntrySchema.TI_Frequency);
					command.AddParameterBasedOnDbColumn("@TI_FrequencyUnit", rateEntry.TI_FrequencyUnit.ToString(), RateEntrySchema.TI_FrequencyUnit);
					command.AddParameterBasedOnDbColumn("@TI_IsCrossTrade", (bool)rateEntry.TI_IsCrossTrade, RateEntrySchema.TI_IsCrossTrade);
					command.AddParameterBasedOnDbColumn("@TI_IsTact", (bool)rateEntry.TI_IsTact, RateEntrySchema.TI_IsTact);
					command.AddParameterBasedOnDbColumn("@TI_TH", rateEntry.TI_TH.ToSqlParameter(), RateEntrySchema.TI_TH);
					command.AddParameterBasedOnDbColumn("@TI_OH_TransportProvider", rateEntry.TI_OH_TransportProvider.ToSqlParameter(), RateEntrySchema.TI_OH_TransportProvider);
					command.AddParameterBasedOnDbColumn("@TI_OH_Supplier", rateEntry.TI_OH_Supplier.ToSqlParameter(), RateEntrySchema.TI_OH_Supplier);
					command.AddParameterBasedOnDbColumn("@TI_OH_Consignor", rateEntry.TI_OH_Consignor.ToSqlParameter(), RateEntrySchema.TI_OH_Consignor);
					command.AddParameterBasedOnDbColumn("@TI_OH_Consignee", rateEntry.TI_OH_Consignee.ToSqlParameter(), RateEntrySchema.TI_OH_Consignee);
					command.AddParameterBasedOnDbColumn("@TI_OH_ControllingCustomer", rateEntry.TI_OH_ControllingCustomer.ToSqlParameter(), RateEntrySchema.TI_OH_ControllingCustomer);
					command.AddParameterBasedOnDbColumn("@TI_OA_CartagePickupAddressOverride", rateEntry.TI_OA_CartageDeliveryAddressOverride.ToSqlParameter(), RateEntrySchema.TI_OA_CartageDeliveryAddressOverride);
					command.AddParameterBasedOnDbColumn("@TI_OA_CartageDeliveryAddressOverride", rateEntry.TI_OA_CartageDeliveryAddressOverride.ToSqlParameter(), RateEntrySchema.TI_OA_CartageDeliveryAddressOverride);
					command.AddParameterBasedOnDbColumn("@TI_RateOrigin", rateEntry.TI_RateOrigin.ToString(), RateEntrySchema.TI_RateOrigin);
					command.AddParameterBasedOnDbColumn("@TI_RateDestination", rateEntry.TI_RateDestination.ToString(), RateEntrySchema.TI_RateDestination);
					command.AddParameterBasedOnDbColumn("@TI_TZ_OriginZone", rateEntry.TI_TZ_OriginZone.ToSqlParameter(), RateEntrySchema.TI_TZ_OriginZone);
					command.AddParameterBasedOnDbColumn("@TI_TZ_DestinationZone", rateEntry.TI_TZ_DestinationZone.ToSqlParameter(), RateEntrySchema.TI_TZ_DestinationZone);
					command.AddParameterBasedOnDbColumn("@TI_R9_FromSuburb", rateEntry.TI_R9_FromSuburb.ToSqlParameter(), RateEntrySchema.TI_R9_FromSuburb);
					command.AddParameterBasedOnDbColumn("@TI_R9_ToSuburb", rateEntry.TI_R9_ToSuburb.ToSqlParameter(), RateEntrySchema.TI_R9_ToSuburb);
					command.AddParameterBasedOnDbColumn("@TI_RC", rateEntry.TI_RC.ToSqlParameter(), RateEntrySchema.TI_RC);
					command.AddParameterBasedOnDbColumn("@TI_RCC_ComponentCode", rateEntry.TI_RCC_ComponentCode.ToSqlParameter(), RateEntrySchema.TI_RCC_ComponentCode);
					command.AddParameterBasedOnDbColumn("@TI_ContainerUnitSection", rateEntry.TI_ContainerUnitSection.ToString(), RateEntrySchema.TI_ContainerUnitSection);
					command.AddParameterBasedOnDbColumn("@TI_RRC_RepairCode", rateEntry.TI_RRC_RepairCode.ToSqlParameter(), RateEntrySchema.TI_RRC_RepairCode);
					command.AddParameterBasedOnDbColumn("@TI_RMC_Material", rateEntry.TI_RMC_Material.ToSqlParameter(), RateEntrySchema.TI_RMC_Material);
					command.AddParameterBasedOnDbColumn("@TI_MatchContainerRateClass", (bool)rateEntry.TI_MatchContainerRateClass, RateEntrySchema.TI_MatchContainerRateClass);
					command.AddParameterBasedOnDbColumn("@TI_RateStartDate", rateEntry.TI_RateStartDate.ToDateTime(), RateEntrySchema.TI_RateStartDate);
					command.AddParameterBasedOnDbColumn("@TI_RateEndDate", rateEntry.TI_RateEndDate.ToDateTime(), RateEntrySchema.TI_RateEndDate);
					command.AddParameterBasedOnDbColumn("@TI_ParentID", rateEntry.TI_ParentID.ToSqlParameter(), RateEntrySchema.TI_ParentID);
					command.AddParameterBasedOnDbColumn("@TI_PaymentTerm", rateEntry.TI_PaymentTerm.ToString(), RateEntrySchema.TI_PaymentTerm);
					command.AddParameterBasedOnDbColumn("@TI_GatewayAgentType", rateEntry.TI_PaymentTerm.ToString(), RateEntrySchema.TI_GatewayAgentType);
					command.AddParameterBasedOnDbColumn("@TI_ContractNumber", rateEntry.TI_ContractNumber.ToString(), RateEntrySchema.TI_ContractNumber);
					command.AddParameterBasedOnDbColumn("@TI_AircraftType", rateEntry.TI_AircraftType.ToString(), RateEntrySchema.TI_AircraftType);
					command.AddParameterBasedOnDbColumn("@TI_ShipmentConsolidationStatus", rateEntry.TI_ShipmentConsolidationStatus.ToString(), RateEntrySchema.TI_ShipmentConsolidationStatus);
					command.AddParameterBasedOnDbColumn("@TI_HBLDeliveryMode", rateEntry.TI_HBLDeliveryMode.ToString(), RateEntrySchema.TI_HBLDeliveryMode);
					command.AddParameterBasedOnDbColumn("@TI_IsNonOperatedReefer", rateEntry.TI_IsNonOperatedReefer.ToString(), RateEntrySchema.TI_IsNonOperatedReefer);
					command.AddParameterBasedOnDbColumn("@TI_YardUnitType", rateEntry.TI_YardUnitType.ToString(), RateEntrySchema.TI_YardUnitType);
					command.AddParameterBasedOnDbColumn("@TI_YardUnitLoad", rateEntry.TI_YardUnitLoad.ToString(), RateEntrySchema.TI_YardUnitLoad);
					command.AddParameterBasedOnDbColumn("@TI_EstimateType", rateEntry.TI_EstimateType.ToString(), RateEntrySchema.TI_EstimateType);
					command.AddParameterBasedOnDbColumn("@TI_REG_EquipmentGrade", rateEntry.TI_REG_EquipmentGrade.ToSqlParameter(), RateEntrySchema.TI_REG_EquipmentGrade);
					command.AddParameterBasedOnDbColumn("@TI_MNRGroup", rateEntry.TI_MNRGroup.ToString(), RateEntrySchema.TI_MNRGroup);

					command.ExecuteNonQuery();
				}
			}
		}

		#region Process Combined Import

		void ProcessCombinedImport(string[][] data, string serviceLevel)
		{
			string[] regions = data[Constants.RegionRowIndex];

			for (int regionIndex = 1; regionIndex < regions.Length; regionIndex++)
			{
				RateEntry airEntry = CreateDefaultEntry(serviceLevel, regions[regionIndex]);

				airEntry.RateLines.RemoveAndDeleteAll();
				RateLine rateLine = airEntry.RateLines.AddNew();
				rateLine.TL_AC = Env.Registry.FreightChargeCode;
				rateLine.TL_Rounding = RatingRoundingTypes.UpTo1;
				rateLine.TL_WeightVolume = Core.Constants.Weight.Kilograms;
				rateLine.TL_RateCalculator = CombinedCalculator.Code;

				for (int i = rateLine.RateLineItems.Count - 1; i >= 0; i--)
				{
					RateLineItem rateLineItem = rateLine.RateLineItems[i];
					if (!rateLineItem.RateOperatorIsNonPrintedFlag())
					{
						rateLine.RateLineItems.RemoveAndDelete(rateLineItem);
					}
				}

				rateLine.Calculator.UseInclusiveBreaks = true;
				rateLine.Calculator.IsAccumulated = true;

				for (int weightBreaksIndex = Constants.WeighBreakStartIndex; weightBreaksIndex < data.Length; weightBreaksIndex++)
				{
					AddRateLineItem(rateLine, data, weightBreaksIndex, regionIndex);
				}
			}
		}

		void AddRateLineItem(RateLine rateLine, string[][] data, int weightBreaksIndex, int regionIndex)
		{
			if (data[weightBreaksIndex][0].ToUpper() == "MIN. RATE" || string.IsNullOrEmpty(data[weightBreaksIndex][0]))
			{
			}
			else if (data[weightBreaksIndex][0].ToUpper() == "PRICE PER KG")
			{
				AddRateLineItemForPricePerKG(rateLine, data, weightBreaksIndex, regionIndex);
			}
			else
			{
				AddRateLineItemForWeightBreakFlatRate(rateLine, data, weightBreaksIndex, regionIndex);
			}
		}

		void AddRateLineItemForPricePerKG(RateLine rateLine, string[][] data, int weightBreaksIndex, int regionIndex)
		{
			RateLineItem rateLineItem = rateLine.RateLineItems.AddNew();
			rateLineItem.TM_Type = CombinedCalculator.Items.Operator.Plus;

			ZDecimal weightBreak = 0m;
			ZDecimal.TryParse(data[weightBreaksIndex - 1][0], out weightBreak);
			rateLineItem.TM_Break = weightBreak;

			ZDecimal value = 0m;
			ZDecimal.TryParse(data[weightBreaksIndex + 1][regionIndex], out value);
			rateLineItem.TM_FlatAmount = value;

			ZDecimal.TryParse(data[weightBreaksIndex][regionIndex], out value);
			rateLineItem.TM_Value = value;
		}

		void AddRateLineItemForWeightBreakFlatRate(RateLine rateLine, string[][] data, int weightBreaksIndex, int regionIndex)
		{
			ZDecimal weightBreak = 0m;

			if (ZDecimal.TryParse(data[weightBreaksIndex][0], out weightBreak))
			{
				RateLineItem rateLineItem = rateLine.RateLineItems.AddNew();
				if (weightBreaksIndex == Constants.WeighBreakStartIndex)
				{
					ZDecimal.TryParse(data[weightBreaksIndex][0], out weightBreak);
					rateLineItem.TM_Type = CombinedCalculator.Items.Operator.Minus;
				}
				else
				{
					ZDecimal.TryParse(data[weightBreaksIndex - 1][0], out weightBreak);
					rateLineItem.TM_Type = CombinedCalculator.Items.Operator.Plus;
				}

				rateLineItem.TM_Break = weightBreak;

				ZDecimal value = 0m;
				ZDecimal.TryParse(data[weightBreaksIndex][regionIndex], out value);
				rateLineItem.TM_FlatAmount = value;
			}
		}

		#endregion

		RateEntry CreateDefaultEntry(string serviceLevel, string originLRC)
		{
			RateEntry result = RateEntryCollection.AddNew();
			result.TI_RS_NKServiceLevel_NI = serviceLevel;
			result.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			result.TI_OriginLRC = originLRC.PadLeft(4, '0');
			result.TI_DestinationLRC = Core.Constants.CountryCodes.Australia;
			result.TI_RateStartDate = hasStartDate ? startDate.Date : result.TI_RateStartDate;
			result.TI_RateEndDate = result.TI_RateStartDate.AddYears(2);

			return result;
		}

		readonly RatingHeader companyTariff;

		RateEntryCollection RateEntryCollection => companyTariff.EntryCollections[RatingConstants.RateCategory.AIR]
			.LazyLoadingCollection;
	}
}
