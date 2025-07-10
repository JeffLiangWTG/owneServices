using System;
using System.Data;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Business.Actions
{
	public class HVLVArchiveAction : IArchivePreparationAction
	{
		readonly IArchiveLogger logger;
		readonly IArchiveSet archiveSet;
		readonly IArchiveConfiguration config;

		DocumentFactory Factory { get; }

		public HVLVArchiveAction(IArchiveLogger logger, IArchiveSet archiveSet, IArchiveConfiguration config)
		{
			_ = Argument.NotNull(logger, nameof(logger));
			_ = Argument.NotNull(archiveSet, nameof(archiveSet));
			_ = Argument.NotNull(config, nameof(config));

			this.logger = logger;
			this.archiveSet = archiveSet;
			this.config = config;

			var businessObjectFactory = new BusinessObjectFactory();
			businessObjectFactory.NameForDebugging = "HAR Local BusinessObjectFactory";

			Factory = new DocumentFactoryProvider().GetFactory(businessObjectFactory);
			Factory.NameForDebugging = "HAR Main DocumentFactory";
		}

		public void Execute()
		{
			var archivePeriodInMonths = SystemDataRegistry.Instance.HVLVArchiveSystemOnOrBeforeMinimum.Value;
			logger.LogInfo(archiveSet.SystemDescriptor.Code, $"Archiving the consignments, consignment header, and items associated with JobShipment '{archiveSet.MainArchiveItemNK}'");

			Db.Connection.RunInTransaction(() =>
			{
				PopulateHVLVShipmentHistory();
				PopulateHVLVUsageHistory();
				SetHVLVDataToArchived();
				PopulateHVLVDeliveryByArea();
				MoveArchivedHVLVDataIntoEDocs();

				Factory.Save();
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void PopulateHVLVShipmentHistory()
		{
			using var command = Db.Connection.Command(PopulateHSHSQL);
			_ = command.AddParameter("@DateTimeNow", SqlDbType.DateTime, config.StartRunTime);
			_ = command.AddParameter("@ShipmentPK", SqlDbType.UniqueIdentifier, archiveSet.MainArchiveItem.PK);

			_ = command.ExecuteNonQuery();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void PopulateHVLVUsageHistory()
		{
			using var command = Db.Connection.Command(PopulateHUSSQL);
			_ = command.AddParameter("@ShipmentPK", SqlDbType.UniqueIdentifier, archiveSet.MainArchiveItem.PK);

			_ = command.ExecuteNonQuery();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void SetHVLVDataToArchived()
		{
			using var command = Db.Connection.Command(ArchiveHVLVDataSQL);
			_ = command.AddParameter("@ShipmentPK", SqlDbType.UniqueIdentifier, archiveSet.MainArchiveItem.PK);
			_ = command.ExecuteNonQuery();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void PopulateHVLVDeliveryByArea()
		{
			using var command = Db.Connection.Command(PopulateHDBSQL);
			_ = command.AddParameter("@DateTimeNow", SqlDbType.DateTime, config.StartRunTime);
			_ = command.AddParameter("@ShipmentPK", SqlDbType.UniqueIdentifier, archiveSet.MainArchiveItem.PK);

			_ = command.ExecuteNonQuery();
		}

		public void MoveArchivedHVLVDataIntoEDocs()
		{
			var archiveFileType = SystemDataRegistry.Instance.ArchivingFileFormat.Value;
			if (archiveFileType == Core.Constants.FileFormats.CSV)
			{
				ExportCSVFile();
			}
			else if (archiveFileType == Core.Constants.FileFormats.XML)
			{
				ExportXMLFile();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void ExportCSVFile()
		{
			using var command = Db.Connection.Command(ExportCSVSQL);
			_ = command.AddParameter("@HCH_PK", SqlDbType.UniqueIdentifier, new Guid());

			var forwardingShipment = Factory.Load<ForwardingShipment>(archiveSet.MainArchiveItem.PK);

			if (forwardingShipment.HVLVConsignmentHeader != null)
			{
				var csvString = new StringBuilder();
				var zipFileName = forwardingShipment.JS_UniqueConsignRef + (NoResString)" - HVLV Archived Data.csv";

				command.SetParameterValue("@HCH_PK", forwardingShipment.HVLVConsignmentHeader.PK.ToGuid());
				var sourceDataTable = command.ExecuteDataSet()[0];

				foreach (DataColumn column in sourceDataTable.Columns)
				{
					_ = csvString.Append(column.ColumnName + ',');
				}

				_ = csvString.Append("\r\n");

				try
				{
					foreach (DataRow row in sourceDataTable.Rows)
					{
						foreach (DataColumn column in sourceDataTable.Columns)
						{
							if (row[column.ColumnName] != null)
							{
								var value = row[column.ColumnName].ToString();
								if (value.Contains(","))
								{
									_ = csvString.Append(string.Format("\"{0}\"", value) + ',');
								}
								else
								{
									_ = csvString.Append(value + ',');
								}
							}
						}

						_ = csvString.Append("\r\n");
					}
				}
				catch (Exception e)
				{
					ErrorReporter.ReportOnce("HVLVArchiveAction.ExportCSVFileError", e.Message, e);
				}

				using var sourceStream = new MemoryStream();
				using var outputStream = new MemoryStream();
				var btArray = Encoding.ASCII.GetBytes(csvString.ToString());

				var creator = new ZipCreator();
				sourceStream.Write(btArray, 0, btArray.Length);
				creator.ZipStream(new ZipStream[] { new(zipFileName, sourceStream) }, outputStream);

				forwardingShipment.DocManagerInfo.MasterFactory = Factory;
				_ = forwardingShipment.DocManagerInfo.AddFileOrDocument(
					outputStream.ToArray(),
					zipFileName + ".gz",
					Core.Constants.RefDocTypes.HVLVArchivedData,
					visibleCompanyPK: Env.CurrentCompanyPK,
					visibleBranchPK: Env.CurrentBranchPK,
					visibleDepartmentPK: Env.CurrentDepartmentPK);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void ExportXMLFile()
		{
			using var command = Db.Connection.Command(ExportXMLSQL);
			_ = command.AddParameter("@HCH_PK", SqlDbType.UniqueIdentifier, new Guid());

			var forwardingShipment = Factory.Load<ForwardingShipment>(archiveSet.MainArchiveItem.PK);

			if (forwardingShipment.HVLVConsignmentHeader != null)
			{
				var xmlBytes = Array.Empty<byte>();
				var zipFileName = forwardingShipment.JS_UniqueConsignRef + (NoResString)" - HVLV Archived Data.xml.gz";

				command.SetParameterValue("@HCH_PK", forwardingShipment.HVLVConsignmentHeader.PK.ToGuid());

				using (var reader = command.ExecuteReader())
				using (var stream = new MemoryStream())
				{
					if (reader.Read())
					{
						var buffer = new byte[8040];
						long bytesRead;
						long offset = 0;

						while ((bytesRead = reader.GetBytes(0, offset, buffer, 0, buffer.Length)) > 0)
						{
							offset += bytesRead;
							stream.Write(buffer, 0, (int)bytesRead);
						}

						xmlBytes = stream.ToArray();
					}
				}

				if (xmlBytes.Length > 0)
				{
					forwardingShipment.DocManagerInfo.MasterFactory = Factory;
					_ = forwardingShipment.DocManagerInfo.AddFileOrDocument(
						xmlBytes,
						zipFileName,
						Core.Constants.RefDocTypes.HVLVArchivedData,
						visibleCompanyPK: Env.CurrentCompanyPK,
						visibleBranchPK: Env.CurrentBranchPK,
						visibleDepartmentPK: Env.CurrentDepartmentPK);
				}
			}
		}

		const string PopulateHSHSQL = @"
DECLARE @ItemCountsOnShipments TABLE
	(
		ShipmentRef VARCHAR(20),
		ItemCount INT,
		ClearedItemCount INT,
		HeldItemCount INT,
		NoneReportedItemCount INT,
		ScannedItemCount INT,
		ScannedClearedItemCount INT,
		ScannedHeldItemCount INT,
		ScannedNoneReportedItemCount INT,
		SurplusItemCount INT,
		InterceptedItemCount INT,
		SeizedItemCount INT,
		DiscardedItemCount INT,
		NonDeliveredItemCount INT,
		TransportBookingBookedItemCount INT,
		DamagedItemCount INT,
		PillagedItemCount INT,
		UllagedItemCount INT,
		ActiveItemCount INT,
		ItemHasContainerNumberCount INT,
		ItemHasOuterPackageCount INT,
		ItemHasSecurityFiledCount INT
	)

INSERT INTO @ItemCountsOnShipments
SELECT
	JS_UniqueConsignRef,
	COUNT(i.HVI_PK),
	COUNT(CASE WHEN i.HVI_ReleaseStatus = 'C' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_ReleaseStatus = 'H' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_ReleaseStatus = 'N' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_IsScannedAtDestination = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_IsScannedAtDestination = 1 AND i.HVI_ReleaseStatus = 'C' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_IsScannedAtDestination = 1 AND i.HVI_ReleaseStatus = 'H' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_IsScannedAtDestination = 1 AND i.HVI_ReleaseStatus = 'N' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_Status = 'SUD' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_Status = 'RDX' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_Status = 'SZD' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_Status = 'DDD' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_Status != 'DLV' AND i.HVI_Status != 'POD' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_KM_LastMileTransportBooking IS NOT NULL THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_IsDamaged = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_IsPillaged = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_IsUllaged = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_IsActive = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_ContainerNumber != '' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_HVO_OuterPackage IS NOT NULL THEN 1 ELSE NULL END),
	COUNT(CASE WHEN i.HVI_SecurityFilingFirstUsageTimeUtc IS NOT NULL THEN 1 ELSE NULL END)
FROM dbo.JobShipment
JOIN dbo.HVLVConsignmentHeader ON dbo.JobShipment.JS_PK = dbo.HVLVConsignmentHeader.HCH_JS_Shipment
JOIN dbo.HVLVConsignment c ON c.HVC_HCH_Header = dbo.HVLVConsignmentHeader.HCH_PK
LEFT JOIN dbo.HVLVItem i on i.HVI_HVC_Consignment = HVC_PK
WHERE HCH_IsArchived = 0 AND JS_PK = @ShipmentPK
GROUP BY JS_UniqueConsignRef

DECLARE @ItemLineCountsOnShipments TABLE
(
	ShipmentRef VARCHAR(20),
	DetailsProvidedCount INT,
	ClassificationLookupUsedCount INT,
	ProductCodeUsedCount INT,
	OriginTariffProvidedCount INT,
	DestinationTariffProvidedCount INT
)

INSERT INTO @ItemLineCountsOnShipments
SELECT
	JS_UniqueConsignRef,
	COUNT(s.HVS_PK),
	COUNT(CASE WHEN s.HVS_CC_Lookup IS NOT NULL THEN 1 END),
	COUNT(CASE WHEN s.HVS_ProductCode != '' THEN 1 END),
	COUNT(CASE WHEN s.HVS_OriginTariff != '' THEN 1 END),
	COUNT(CASE WHEN s.HVS_DestinationTariff != '' THEN 1 END)
FROM dbo.JobShipment
JOIN dbo.HVLVConsignmentHeader ON dbo.JobShipment.JS_PK = dbo.HVLVConsignmentHeader.HCH_JS_Shipment
JOIN dbo.HVLVConsignment c ON c.HVC_HCH_Header = dbo.HVLVConsignmentHeader.HCH_PK
JOIN dbo.HVLVItem i on i.HVI_HVC_Consignment = HVC_PK
LEFT JOIN dbo.HVLVItemLine s ON s.HVS_HVI_HVLVItem = i.HVI_PK
WHERE HCH_IsArchived = 0 AND JS_PK = @ShipmentPK
GROUP BY JS_UniqueConsignRef

DECLARE @ConsignmentCountsOnShipments TABLE
	(
		ShipmentRef VARCHAR(20),
		ConsignmentIsDangerousGoodsCount INT,
		ConsignmentIsTaxPrePaidCount INT,
		ConsignmentIsPreScreenedCount INT,
		ConsignmentIsHazardousCount INT,
		ConsignmentRequiresFumigationCount INT,
		ConsignmentIsPersonalEffectsCount INT,
		ConsignmentIsTimberCount INT,
		ConsignmentIsPerishableCount INT,
		StandAloneDeclarationCount INT,
		ConsignmentCount INT,
		ActiveConsignmentCount INT,
		MultiItemConsignmentCount INT,
		MaximumWeight DECIMAL(9,3),
		AverageWeight DECIMAL(9,3),
		MaximumVolume DECIMAL(9,3),
		AverageVolume DECIMAL(9,3),
		ConsignmentSignatureRequiredCount INT,
		ConsignmentAuthorityToLeaveCount INT,
		ConsignmentHasConsigneeInstructionCount INT,
		ConsignmentSelfBookedCount INT,
		ConsignmentHasDestinationDepotCount INT,
		ConsignmentHasVendorIDCount INT,
		ConsignmentINCOIsFOBCount INT,
		ConsignmentINCOIsCIFCount INT,
		ConsignmentLMCAgentUsedCount INT,
		ConsignmentCarrierAccountNumberProvidedCount INT,
		ConsignmentAddressValidatedCount INT,
		ConsignmentConsigneeIsOrgCount INT,
		ConsignmentNonCustomsClearedCount INT,
		ConsignmentACASMessageUsedCount INT
	)

INSERT INTO @ConsignmentCountsOnShipments
SELECT
	JS_UniqueConsignRef,
	COUNT(CASE WHEN c.HVC_UndgClass != '' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_IsTaxPrePaid = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_PreScreeningStatus != 'UNK' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_IsHazardous = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_RequiresFumigation = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_IsPersonalEffects = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_IsTimber = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_IsPerishable = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_JE_ImportDeclaration IS NOT NULL THEN 1 ELSE NULL END) + COUNT(CASE WHEN c.HVC_JE_ExportDeclaration IS NOT NULL THEN 1 ELSE NULL END),
	COUNT(c.HVC_PK),
	COUNT(CASE WHEN c.HVC_IsActive = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_ItemCount > 1 THEN 1 ELSE NULL END),
	MAX(ConvertedWeight.Value),
	AVG(ConvertedWeight.Value),
	MAX(ConvertedVolume.Value),
	AVG(ConvertedVolume.Value),
	COUNT(CASE WHEN c.HVC_IsSignatureRequired = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_AuthorityToLeave = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_ConsigneeInstructions != '' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_IsSelfBooked = 1 THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_OA_DestinationDepot IS NOT NULL THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_VendorIdentifier != '' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_INCO = 'FOB' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_INCO = 'CIF' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_OH_LastMileCarrierBookingAgent IS NOT NULL THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_CarrierAccountNumber != '' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_ConsigneeAddressValidationStatus != 'NYV' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_OA_ConsigneeAddress IS NOT NULL THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_ImportReleaseStatus != 'CLR' AND c.HVC_ExportReleaseStatus != 'CLR' THEN 1 ELSE NULL END),
	COUNT(CASE WHEN c.HVC_ACASStatus != '' THEN 1 ELSE NULL END)
FROM dbo.JobShipment
JOIN dbo.HVLVConsignmentHeader ON dbo.JobShipment.JS_PK = dbo.HVLVConsignmentHeader.HCH_JS_Shipment
JOIN dbo.HVLVConsignment c ON c.HVC_HCH_Header = dbo.HVLVConsignmentHeader.HCH_PK
CROSS APPLY dbo.ConvertVolume((CASE WHEN HVC_ActualVolume > 0 THEN HVC_ActualVolume ELSE HVC_ManifestedVolume END), HVC_VolumeUQ, 'M3') AS ConvertedVolume
CROSS APPLY dbo.ConvertWeight((CASE WHEN HVC_ActualWeight > 0 THEN HVC_ActualWeight ELSE HVC_ManifestedWeight END), HVC_WeightUQ, 'KG') AS ConvertedWeight
WHERE HCH_IsArchived = 0 AND JS_PK = @ShipmentPK
GROUP BY JS_UniqueConsignRef

INSERT INTO dbo.HVLVShipmentHistory
	(
		HSH_PK,
		HSH_ShipmentId,
		HSH_ConsignmentIsDangerousGoodsCount,
		HSH_ConsignmentIsTaxPrePaidCount,
		HSH_ConsignmentIsPreScreenedCount,
		HSH_ConsignmentIsHazardousCount,
		HSH_ConsignmentRequiresFumigationCount,
		HSH_ConsignmentIsPersonalEffectsCount,
		HSH_ConsignmentIsTimberCount,
		HSH_ConsignmentIsPerishableCount,
		HSH_StandAloneDeclarationCount,
		HSH_ConsignmentCount,
		HSH_ActiveConsignmentCount,
		HSH_MultiItemConsignmentCount,
		HSH_MaximumWeight,
		HSH_AverageWeight,
		HSH_MaximumVolume,
		HSH_AverageVolume,
		HSH_ConsignmentSignatureRequiredCount,
		HSH_ConsignmentAuthorityToLeaveCount,
		HSH_ConsignmentHasConsigneeInstructionCount,
		HSH_ConsignmentSelfBookedCount,
		HSH_ConsignmentHasDestinationDepotCount,
		HSH_ConsignmentHasVendorIDCount,
		HSH_ConsignmentINCOIsFOBCount,
		HSH_ConsignmentINCOIsCIFCount,
		HSH_ConsignmentLMCAgentUsedCount,
		HSH_ConsignmentCarrierAccountNumberProvidedCount,
		HSH_ConsignmentAddressValidatedCount,
		HSH_ConsignmentConsigneeIsOrgCount,
		HSH_ConsignmentNonCustomsClearedCount,
		HSH_ConsignmentACASMessageUsedCount,
		HSH_ItemCount,
		HSH_ClearedItemCount,
		HSH_HeldItemCount,
		HSH_NoneReportedItemCount,
		HSH_ScannedItemCount,
		HSH_ScannedClearedItemCount,
		HSH_ScannedHeldItemCount,
		HSH_ScannedNoneReportedItemCount,
		HSH_SurplusItemCount,
		HSH_InterceptedItemCount,
		HSH_SeizedItemCount,
		HSH_DiscardedItemCount,
		HSH_NonDeliveredItemCount,
		HSH_TransportBookingBookedItemCount,
		HSH_DamagedItemCount,
		HSH_PillagedItemCount,
		HSH_UllagedItemCount,
		HSH_ActiveItemCount,
		HSH_ItemHasContainerNumberCount,
		HSH_ItemHasOuterPackageCount,
		HSH_ItemHasSecurityFiledCount,
		HSH_SystemLastEditTimeUtc,
		HSH_SystemCreateTimeUtc,
		HSH_SystemLastEditUser,
		HSH_SystemCreateUser,
		HSH_ItemLineDetailsProvidedCount,
		HSH_ItemLineClassificationLookupUsedCount,
		HSH_ItemLineProductCodeUsedCount,
		HSH_ItemLineOrigINTariffProvidedCount,
		HSH_ItemLineDestinationTariffProvidedCount
	)
SELECT
	NEWID(),
	c.ShipmentRef,
	c.ConsignmentIsDangerousGoodsCount,
	c.ConsignmentIsTaxPrePaidCount,
	c.ConsignmentIsPreScreenedCount,
	c.ConsignmentIsHazardousCount,
	c.ConsignmentRequiresFumigationCount,
	c.ConsignmentIsPersonalEffectsCount,
	c.ConsignmentIsTimberCount,
	c.ConsignmentIsPerishableCount,
	c.StandAloneDeclarationCount,
	c.ConsignmentCount,
	c.ActiveConsignmentCount,
	c.MultiItemConsignmentCount,
	c.MaximumWeight,
	c.AverageWeight,
	c.MaximumVolume,
	c.AverageVolume,
	c.ConsignmentSignatureRequiredCount,
	c.ConsignmentAuthorityToLeaveCount,
	c.ConsignmentHasConsigneeInstructionCount,
	c.ConsignmentSelfBookedCount,
	c.ConsignmentHasDestinationDepotCount,
	c.ConsignmentHasVendorIDCount,
	c.ConsignmentINCOIsFOBCount,
	c.ConsignmentINCOIsCIFCount,
	c.ConsignmentLMCAgentUsedCount,
	c.ConsignmentCarrierAccountNumberProvidedCount,
	c.ConsignmentAddressValidatedCount,
	c.ConsignmentConsigneeIsOrgCount,
	c.ConsignmentNonCustomsClearedCount,
	c.ConsignmentACASMessageUsedCount,
	i.ItemCount,
	i.ClearedItemCount,
	i.HeldItemCount,
	i.NoneReportedItemCount,
	i.ScannedItemCount,
	i.ScannedClearedItemCount,
	i.ScannedHeldItemCount,
	i.ScannedNoneReportedItemCount,
	i.SurplusItemCount,
	i.InterceptedItemCount,
	i.SeizedItemCount,
	i.DiscardedItemCount,
	i.NonDeliveredItemCount,
	i.TransportBookingBookedItemCount,
	i.DamagedItemCount,
	i.PillagedItemCount,
	i.UllagedItemCount,
	i.ActiveItemCount,
	i.ItemHasContainerNumberCount,
	i.ItemHasOuterPackageCount,
	i.ItemHasSecurityFiledCount,
	@DateTimeNow,
	@DateTimeNow,
	'~BP',
	'~BP',
	COALESCE(l.DetailsProvidedCount, 0),
	COALESCE(l.ClassificationLookupUsedCount, 0),
	COALESCE(l.ProductCodeUsedCount, 0),
	COALESCE(l.OriginTariffProvidedCount, 0),
	COALESCE(l.DestinationTariffProvidedCount, 0)
FROM @ConsignmentCountsOnShipments c
JOIN @ItemCountsOnShipments i
ON c.ShipmentRef = i.ShipmentRef
LEFT JOIN @ItemLineCountsOnShipments l
on c.ShipmentRef = l.ShipmentRef;
";

		const string PopulateHUSSQL = @"
INSERT INTO dbo.HVLVUsageHistory
(
HUS_PK,
HUS_HSH_ShipmentHistory,
HUS_Category,
HUS_Count
)
SELECT
NEWID(),
HVLVShipmentHistory.HSH_PK,
HVLVUsage.HXU_Category, 
COUNT(*) as CategoryCount
FROM dbo.HVLVUsage
JOIN dbo.HVLVItem ON HVLVUsage.HXU_HVI_ParentItem = HVLVItem.HVI_PK
JOIN dbo.JobShipment ON HVLVItem.HVI_JS_LoadedOnShipment = JobShipment.JS_PK
JOIN dbo.HVLVConsignmentHeader ON JobShipment.JS_PK = HVLVConsignmentHeader.HCH_JS_Shipment
JOIN dbo.HVLVShipmentHistory ON JobShipment.JS_UniqueConsignRef = HVLVShipmentHistory.HSH_ShipmentId
WHERE JobShipment.JS_PK = @ShipmentPK AND HVLVConsignmentHeader.HCH_IsArchived = 0
GROUP BY HVLVShipmentHistory.HSH_PK, HVLVUsage.HXU_Category
";

		const string ArchiveHVLVDataSQL = @"
UPDATE dbo.HVLVConsignmentHeader
SET HCH_IsArchived = 1
WHERE HCH_JS_Shipment = @ShipmentPK AND HCH_IsArchived = 0
";

		const string PopulateHDBSQL = @"
DECLARE @VolumeWeightCap DECIMAL(9,3) = 999999.999;
DECLARE @ConsignmentCalculatedProperties TABLE
	(
		ConsignmentPK UNIQUEIDENTIFIER,
		CreatedMonth TINYINT,
		CreatedYear SMALLINT,
		ItemCount INT
	)

INSERT INTO @ConsignmentCalculatedProperties
	(
		ConsignmentPK,
		CreatedMonth,
		CreatedYear,
		ItemCount
	)
SELECT
	HVC_PK,
	CONVERT(TINYINT, MONTH(HVC_SystemCreateTimeUtc)),
	CONVERT(SMALLINT, YEAR(HVC_SystemCreateTimeUtc)),
	(
		SELECT COUNT(*)
		FROM dbo.HVLVItem
		WHERE HVC_PK = HVI_HVC_Consignment
	)
FROM dbo.HVLVConsignment
JOIN dbo.HVLVConsignmentHeader ON HCH_PK = HVC_HCH_Header
WHERE HCH_JS_Shipment = @ShipmentPK AND HCH_IsArchived = 1

INSERT INTO dbo.HVLVDeliveryByArea
	(
		HDB_PK,
		HDB_OH_LastMileCarrier,
		HDB_RN_NKConsigneeCountry,
		HDB_ConsigneePostcode,
		HDB_CreatedMonth,
		HDB_CreatedYear,
		HDB_ConsignmentCount,
		HDB_MultiItemCount,
		HDB_ConsignmentVolumeSumInM3,
		HDB_ConsignmentWeightSumInKG,
		HDB_IsDangerousCount,
		HDB_AuthorityToLeaveCount,
		HDB_SignatureRequiredCount,
		HDB_SelfBookedCount,
		HDB_HasConsigneeInstructionCount,
		HDB_ConsigneeAddressValidatedCount,
		HDB_SystemCreateTimeUtc,
		HDB_SystemCreateUser,
		HDB_SystemLastEditTimeUtc,
		HDB_SystemLastEditUser
	)
SELECT
	NEWID(),
	HVC_OH_LastMileCarrier,
	HVC_RN_NKConsigneeCountryCode,
	HVC_ConsigneePostcode,
	CreatedMonth,
	CreatedYear,
	COUNT(1),
	SUM(ItemCount),
	CASE WHEN SUM(ConvertedVolume.Value) > @VolumeWeightCap THEN @VolumeWeightCap ELSE SUM(ConvertedVolume.Value) END AS TotalVolume,
	CASE WHEN SUM(ConvertedWeight.Value) > @VolumeWeightCap THEN @VolumeWeightCap ELSE SUM(ConvertedWeight.Value) END AS TotalWeight,
	SUM(CASE HVC_UndgClass WHEN '' THEN 0 ELSE 1 END),
	SUM(CONVERT(INT, HVC_AuthorityToLeave)),
	SUM(CONVERT(INT, HVC_IsSignatureRequired)),
	SUM(CONVERT(INT, HVC_IsSelfBooked)),
	SUM(CASE HVC_ConsigneeInstructions WHEN '' THEN 0 ELSE 1 END),
	SUM(CASE HVC_ConsigneeAddressValidationStatus WHEN 'NYV' THEN 0 ELSE 1 END),
	@DateTimeNow,
	'~BP',
	@DateTimeNow,
	'~BP'
FROM dbo.HVLVConsignment
JOIN dbo.HVLVConsignmentHeader ON HCH_PK = HVC_HCH_Header
JOIN @ConsignmentCalculatedProperties ON ConsignmentPK = HVC_PK
CROSS APPLY dbo.ConvertVolume((CASE WHEN HVC_ActualVolume > 0 THEN HVC_ActualVolume ELSE HVC_ManifestedVolume END), HVC_VolumeUQ, 'M3') AS ConvertedVolume
CROSS APPLY dbo.ConvertWeight((CASE WHEN HVC_ActualWeight > 0 THEN HVC_ActualWeight ELSE HVC_ManifestedWeight END), HVC_WeightUQ, 'KG') AS ConvertedWeight
WHERE HCH_JS_Shipment = @ShipmentPK AND HCH_IsArchived = 1
GROUP BY HVC_OH_LastMileCarrier, HVC_RN_NKConsigneeCountryCode, HVC_ConsigneePostcode, CreatedMonth, CreatedYear
";

		const string ExportXMLSQL = @"
DECLARE @Xml VARCHAR(MAX) =
(
	SELECT *
	FROM dbo.HVLVConsignmentHeader
		LEFT JOIN dbo.HVLVConsignment ON HVC_HCH_Header = HCH_PK
		LEFT JOIN dbo.HVLVItem ON HVI_HVC_Consignment = HVC_PK
		LEFT JOIN dbo.HVLVItemLine ON HVS_HVI_HVLVItem = HVI_PK
	WHERE HCH_PK = @HCH_PK
	FOR XML AUTO
)

SELECT COMPRESS(@Xml)
";

		const string ExportCSVSQL = @"
SELECT *
FROM dbo.HVLVConsignmentHeader
	LEFT JOIN dbo.HVLVConsignment ON HVC_HCH_Header = HCH_PK
	LEFT JOIN dbo.HVLVItem ON HVI_HVC_Consignment = HVC_PK
	LEFT JOIN dbo.HVLVItemLine ON HVS_HVI_HVLVItem = HVI_PK
WHERE HCH_PK = @HCH_PK
";
	}
}
