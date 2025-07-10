#if DEBUG

namespace Enterprise.DbUpgrader.Shared
{
	using System;
	using CargoWise.Common;
	using CargoWise.Data;
	using CargoWise.Database.Abstractions;
	using CargoWise.Schema;
	using Enterprise.ZArchitecture.Schema;
	using Microsoft.Extensions.DependencyInjection;

	public static class RowWrapperRepositoryExtensions
	{
		#region Core

		public static Entity CreateGlobalCompany(this RowWrapperRepository repository, string code = "ROG", string name = "ROGA & KOPYTA", string currency = "USD")
		{
			var row = repository.New(GlbCompanySchema.Instance);
			row[GlbCompanySchema.GC_Code] = code;
			row[GlbCompanySchema.GC_Name] = name;
			row[GlbCompanySchema.GC_RX_NKLocalCurrency] = currency;

			return new Entity { Repository = repository, Row = row, Schema = GlbCompanySchema.Instance };
		}

		public static Entity CreateStmData(this Entity owner, string sD_Name, string sD_Type = "STR", byte[] sD_BinaryData = null)
		{
			var row = owner.Repository.New(StmDataSchema.Instance);
			row[StmDataSchema.SD_Owner] = owner.Row.PK;
			row[StmDataSchema.SD_Name] = sD_Name;
			row[StmDataSchema.SD_Type] = sD_Type;
			row[StmDataSchema.SD_BinaryValue] = sD_BinaryData;

			return new Entity { Repository = owner.Repository, Row = row, Schema = StmDataSchema.Instance };
		}

		#endregion

		#region Master Files

		public static RowWrapper CreateProcessTask(this RowWrapperRepository repository, string parentTableCode, string type, string eventCode, bool respondToCascadedEvents = false, string status = null)
		{
			var processTask = repository.New(ProcessTasksSchema.Instance);
			processTask[ProcessTasksSchema.P9_ParentTableCode] = parentTableCode;
			processTask[ProcessTasksSchema.P9_Type] = type;
			processTask[ProcessTasksSchema.P9_SE_NKMilestoneEvent] = eventCode;
			processTask[ProcessTasksSchema.P9_RespondToCascadedEvents] = respondToCascadedEvents;
			processTask[ProcessTasksSchema.P9_SystemCreateTimeUtc] = DateTime.UtcNow;
			processTask[ProcessTasksSchema.P9_SystemLastEditTimeUtc] = DateTime.UtcNow;

			if (status != null)
			{
				processTask[ProcessTasksSchema.P9_Status] = status;
			}

			return processTask;
		}

		public static RowWrapper CreateProcessTask(this Entity parent, string type, string eventCode, string triggerCondition = "", Guid? referenceId = null, string status = null, string conditionValue = null, string cascadedEventsContext = null)
		{
			var processTask = parent.Repository.New(ProcessTasksSchema.Instance);
			processTask[ProcessTasksSchema.P9_ParentTableCode] = GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>().GetColumnNamePrefix(parent.Schema.TableName);
			processTask[ProcessTasksSchema.P9_ParentID] = parent.Row.PK;
			processTask[ProcessTasksSchema.P9_Type] = type;
			processTask[ProcessTasksSchema.P9_SE_NKMilestoneEvent] = eventCode;
			processTask[ProcessTasksSchema.P9_TriggerCondition] = triggerCondition;
			processTask[ProcessTasksSchema.P9_Notes] = conditionValue != null ? System.Text.Encoding.ASCII.GetBytes(conditionValue) : null;
			processTask[ProcessTasksSchema.P9_SystemCreateTimeUtc] = DateTime.UtcNow;
			processTask[ProcessTasksSchema.P9_SystemLastEditTimeUtc] = DateTime.UtcNow;

			if (referenceId.HasValue)
			{
				processTask[ProcessTasksSchema.P9_ReferencedID] = referenceId.Value;
			}

			if (status != null)
			{
				processTask[ProcessTasksSchema.P9_Status] = status;
			}

			if (cascadedEventsContext != null)
			{
				processTask[ProcessTasksSchema.P9_CascadedEventsContext] = cascadedEventsContext;
			}

			return processTask;
		}

		public static Entity CreateProcessTaskTemplate(this RowWrapperRepository repository, string type, bool isSystem = false, string name = null)
		{
			var row = repository.New(ProcessTaskTemplateSchema.Instance);
			row[ProcessTaskTemplateSchema.P0_ProcessType] = type;
			row[ProcessTaskTemplateSchema.P0_Name] = name ?? Guid.NewGuid().ToString();
			row[ProcessTaskTemplateSchema.P0_IsSystem] = isSystem;

			return new Entity { Repository = repository, Row = row, Schema = ProcessTaskTemplateSchema.Instance };
		}

		public static Entity CreateRefContainer(this RowWrapperRepository repository)
		{
			var row = repository.New(RefContainerSchema.Instance);

			return new Entity { Repository = repository, Row = row, Schema = RefContainerSchema.Instance };
		}

		public static Entity CreateOrgHeader(this RowWrapperRepository repository, string oH_Code)
		{
			var orgHeader = repository.New(OrgHeaderSchema.Instance);
			orgHeader[OrgHeaderSchema.OH_Code] = oH_Code;

			return new Entity { Repository = repository, Row = orgHeader, Schema = OrgHeaderSchema.Instance };
		}

		public static Entity CreateOrgAddress(this Entity orgHeader, string oA_Address1 = "Kyiv")
		{
			var orgAddress = orgHeader.Repository.New(OrgAddressSchema.Instance);
			orgAddress[OrgAddressSchema.OA_OH] = orgHeader.Row.PK;
			orgAddress[OrgAddressSchema.OA_Address1] = oA_Address1;

			return new Entity { Repository = orgHeader.Repository, Row = orgAddress, Schema = OrgAddressSchema.Instance };
		}

		public static Entity CreateGlbPerson(this RowWrapperRepository repository, string fullName)
		{
			var glbPerson = repository.New(GlbPersonSchema.Instance);
			glbPerson[GlbPersonSchema.PER_FullName] = fullName;

			return new Entity { Repository = repository, Row = glbPerson, Schema = GlbPersonSchema.Instance };
		}

		public static Entity CreatePatternMatchingResult(this RowWrapperRepository repository, Guid masterPK, string masterTableCode, string status, string excludedBy, byte scorePercent)
		{
			var patternMatchingResult = repository.New(PatternMatchingResultSchema.Instance);
			patternMatchingResult[PatternMatchingResultSchema.PMT_MasterPK] = masterPK;
			patternMatchingResult[PatternMatchingResultSchema.PMT_MasterTableCode] = masterTableCode;
			patternMatchingResult[PatternMatchingResultSchema.PMT_FoundTimeUtc] = DateTime.UtcNow;
			patternMatchingResult[PatternMatchingResultSchema.PMT_Status] = status;
			patternMatchingResult[PatternMatchingResultSchema.PMT_GS_NKExcludeBy] = excludedBy;
			patternMatchingResult[PatternMatchingResultSchema.PMT_ScorePercent] = scorePercent;

			return new Entity { Repository = repository, Row = patternMatchingResult, Schema = PatternMatchingResultSchema.Instance };
		}

		public static Entity CreatePatternMatchingResult(this RowWrapperRepository repository, Guid masterPK, string masterTableCode, Guid targetPK, string targetTableCode, string status, string excludedBy, byte scorePercent)
		{
			var patternMatchingResult = repository.CreatePatternMatchingResult(masterPK, masterTableCode, status, excludedBy, scorePercent).Row;
			patternMatchingResult[PatternMatchingResultSchema.PMT_TargetPK] = targetPK;
			patternMatchingResult[PatternMatchingResultSchema.PMT_TargetTableCode] = targetTableCode;

			return new Entity { Repository = repository, Row = patternMatchingResult, Schema = PatternMatchingResultSchema.Instance };
		}

		public static Entity CreateDocAddress(this Entity parentJob, string parentTableCode)
		{
			var docAddress = parentJob.Repository.New(JobDocAddressSchema.Instance);
			docAddress[JobDocAddressSchema.E2_ParentID] = parentJob.Row.PK;
			docAddress[JobDocAddressSchema.E2_ParentTableCode] = parentTableCode;

			return new Entity { Repository = parentJob.Repository, Row = docAddress, Schema = JobDocAddressSchema.Instance };
		}

		public static Entity CreateRefVessel(this RowWrapperRepository repository, string code)
		{
			var refVessel = repository.New(RefVesselSchema.Instance);
			refVessel[RefVesselSchema.RV_Code] = code;

			return new Entity { Repository = repository, Row = refVessel, Schema = RefVesselSchema.Instance };
		}

		#endregion

		#region Forwarding

		public static Entity CreateConsol(this RowWrapperRepository repository, string uniqueConsignRef = null, string originPort = "UAIEV", string destinationPort = "SGSIN", string masterBillNum = null)
		{
			var row = repository.New(JobConsolSchema.Instance);
			row[JobConsolSchema.JK_RL_NKLoadPort] = originPort;
			row[JobConsolSchema.JK_RL_NKDischargePort] = destinationPort;

			if (!uniqueConsignRef.IsNullOrEmpty())
			{
				row[JobConsolSchema.JK_UniqueConsignRef] = uniqueConsignRef;
			}

			if (masterBillNum != null)
			{
				row[JobConsolSchema.JK_MasterBillNum] = masterBillNum;
			}

			return new Entity { Repository = repository, Row = row, Schema = JobConsolSchema.Instance };
		}

		public static Entity CreateShipment(this RowWrapperRepository repository, string uniqueConsignRef = "", string originPort = "", string destinationPort = "")
		{
			var row = repository.New(JobShipmentSchema.Instance);
			row[JobShipmentSchema.JS_RL_NKOrigin] = originPort;
			row[JobShipmentSchema.JS_RL_NKDestination] = destinationPort;

			if (!uniqueConsignRef.IsNullOrEmpty())
			{
				row[JobShipmentSchema.JS_UniqueConsignRef] = uniqueConsignRef;
			}

			return new Entity { Repository = repository, Row = row, Schema = JobShipmentSchema.Instance };
		}

		public static Entity CreateShipment(this Entity consol, string uniqueConsignRef = "McLaren", string jS_ShipmentType = null, string jS_HouseBill = null)
		{
			var shipment = consol.Repository.New(JobShipmentSchema.Instance);
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = uniqueConsignRef;

			if (jS_ShipmentType != null)
			{
				shipment[JobShipmentSchema.JS_ShipmentType] = jS_ShipmentType;
			}

			if (jS_HouseBill != null)
			{
				shipment[JobShipmentSchema.JS_HouseBill] = jS_HouseBill;
			}

			var link = consol.Repository.New(JobConShipLinkSchema.Instance);
			link[JobConShipLinkSchema.JN_JK] = consol.Row.PK;
			link[JobConShipLinkSchema.JN_JS] = shipment.PK;

			return new Entity { Repository = consol.Repository, Row = shipment, Schema = JobShipmentSchema.Instance };
		}

		public static Entity CreateShipmentPreplanning(this RowWrapperRepository repository, string buyerCode = "somebuyer", string buyerAddressLine = "Address 1", string originPort = null, string destinationPort = null)
		{
			var buyer = repository.New(OrgHeaderSchema.Instance);
			buyer[OrgHeaderSchema.OH_Code] = buyerCode;

			var buyerAddress = repository.New(OrgAddressSchema.Instance);
			buyerAddress[OrgAddressSchema.OA_OH] = buyer.PK;
			buyerAddress[OrgAddressSchema.OA_Address1] = buyerAddressLine;

			var row = repository.New(JobShipmentPreplanningSchema.Instance);
			row[JobShipmentPreplanningSchema.EF_RL_NKPortLoad] = originPort;
			row[JobShipmentPreplanningSchema.EF_RL_NKPortDisch] = destinationPort;
			row[JobShipmentPreplanningSchema.EF_OA_BuyerAddress] = buyerAddress.PK;

			return new Entity { Repository = repository, Row = row, Schema = JobShipmentPreplanningSchema.Instance };
		}

		public static Entity CreateTransport(this Entity parent, string from = null, string to = null, Entity linkedToSailling = null)
		{
			var row = parent.Repository.New(JobConsolTransportSchema.Instance);
			row[JobConsolTransportSchema.JW_ParentType] = GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>().GetColumnNamePrefix(parent.Schema.TableName);
			row[JobConsolTransportSchema.JW_ParentGUID] = parent.Row.PK;

			if (from != null)
			{
				row[JobConsolTransportSchema.JW_RL_NKLoadPort] = from;
			}

			if (to != null)
			{
				row[JobConsolTransportSchema.JW_RL_NKDiscPort] = to;
			}

			if (linkedToSailling != null)
			{
				row[JobConsolTransportSchema.JW_JX] = linkedToSailling.Row.PK;
			}

			return new Entity { Repository = parent.Repository, Row = row, Schema = JobConsolTransportSchema.Instance };
		}

		public static Entity CreateVoyage(this RowWrapperRepository repository, string vessel, string voyage)
		{
			var row = repository.New(JobVoyageSchema.Instance);
			row[JobVoyageSchema.JV_RV_NKVessel] = vessel;
			row[JobVoyageSchema.JV_VoyageFlight] = voyage;

			return new Entity { Repository = repository, Row = row, Schema = JobVoyageSchema.Instance };
		}

		public static Entity CreateOrigin(this Entity voyage, string port)
		{
			var row = voyage.Repository.New(JobVoyOriginSchema.Instance);
			row[JobVoyOriginSchema.JA_RL_NKPortOfLoading] = port;
			row[JobVoyOriginSchema.JA_JV] = voyage.Row.PK;

			return new Entity { Repository = voyage.Repository, Row = row, Schema = JobVoyOriginSchema.Instance };
		}

		public static Entity CreateDestination(this Entity voyage, string port)
		{
			var row = voyage.Repository.New(JobVoyDestinationSchema.Instance);
			row[JobVoyDestinationSchema.JB_RL_NKPortOfDischarge] = port;
			row[JobVoyDestinationSchema.JB_JV] = voyage.Row.PK;

			return new Entity { Repository = voyage.Repository, Row = row, Schema = JobVoyOriginSchema.Instance };
		}

		public static Entity CreateSailling(this RowWrapperRepository repository, Entity origin, Entity destination)
		{
			var row = repository.New(JobSailingSchema.Instance);
			row[JobSailingSchema.JX_JA] = origin.Row.PK;
			row[JobSailingSchema.JX_JB] = destination.Row.PK;

			return new Entity { Repository = repository, Row = row, Schema = JobSailingSchema.Instance };
		}

		public static Entity CreateContainer(this RowWrapperRepository repository, Entity booking = null)
		{
			var row = repository.New(JobContainerSchema.Instance);

			if (booking != null)
			{
				row[JobContainerSchema.JC_JS_FCLBookingOnlyLink] = booking.Row.PK;
			}

			return new Entity { Repository = repository, Row = row, Schema = JobContainerSchema.Instance };
		}

		#endregion

		#region Agency

		public static Entity CreateBooking(this RowWrapperRepository repository, string uniqueConsignRef = "mclaren", string originPort = null, string destinationPort = null)
		{
			var shipment = repository.CreateShipment(uniqueConsignRef, originPort, destinationPort);
			shipment.Row[JobShipmentSchema.JS_IsBooking] = true;

			return shipment;
		}

		public static Entity CreateRefContainerStock(this RowWrapperRepository repository, Entity refContainer)
		{
			var row = repository.New(RefContainerStockSchema.Instance);
			row[RefContainerStockSchema.R6_RC] = refContainer.Row.PK;

			return new Entity { Repository = repository, Row = row, Schema = RefContainerStockSchema.Instance };
		}

		#endregion

		#region eManifest

		public static Entity CreateSupplierBookingLine(this Entity shipment, Guid dL_DH_BookingHeader)
		{
			var row = shipment.Repository.New(SupplierBookingLineSchema.Instance);
			row[SupplierBookingLineSchema.DL_JS_ApprovedShipment] = shipment.Row.PK;
			row[SupplierBookingLineSchema.DL_DH_BookingHeader] = dL_DH_BookingHeader;
			row[SupplierBookingLineSchema.DL_SystemCreateTimeUtc] = DateTime.UtcNow;
			row[SupplierBookingLineSchema.DL_SystemLastEditTimeUtc] = DateTime.UtcNow;

			return new Entity { Repository = shipment.Repository, Row = row, Schema = RefContainerStockSchema.Instance };
		}

		public static Entity CreateSupplierBookingHeader(this RowWrapperRepository repository, Guid dH_OA_Consignor)
		{
			var row = repository.New(SupplierBookingHeaderSchema.Instance);
			row[SupplierBookingHeaderSchema.DH_OA_Consignor] = dH_OA_Consignor;
			row[SupplierBookingHeaderSchema.DH_SystemCreateTimeUtc] = DateTime.UtcNow;
			row[SupplierBookingHeaderSchema.DH_SystemLastEditTimeUtc] = DateTime.UtcNow;

			return new Entity { Repository = repository, Row = row, Schema = RefContainerSchema.Instance };
		}

		#endregion

		#region Customs

		public static Entity CreateCusMAWB(this Entity consol, string cM_MAWB = null, string cM_MasterHouseBill = null)
		{
			var row = consol.Repository.New(CusMAWBSchema.Instance);
			row[CusMAWBSchema.CM_JK] = consol.Row.PK;

			if (cM_MasterHouseBill != null)
			{
				row[CusMAWBSchema.CM_MasterHouseBill] = cM_MasterHouseBill;
			}

			if (cM_MAWB != null)
			{
				row[CusMAWBSchema.CM_MAWB] = cM_MAWB;
			}

			return new Entity { Repository = consol.Repository, Row = row, Schema = RefContainerStockSchema.Instance };
		}

		public static Entity CreateDeclaration(this RowWrapperRepository repository)
		{
			var row = repository.New(JobDeclarationSchema.Instance);
			var companyPK = Guid.Empty;
			var branchPK = Guid.Empty;
			Db.Connection.ExecuteReader("SELECT TOP 1 GB_PK, GB_GC FROM dbo.GlbBranch",
				reader =>
				{
					companyPK = Guid.Parse(reader["GB_GC"].ToString());
					branchPK = Guid.Parse(reader["GB_PK"].ToString());
				});
			row[JobDeclarationSchema.JE_GB] = branchPK;
			row[JobDeclarationSchema.JE_GC] = companyPK;
			return new Entity { Repository = repository, Row = row, Schema = JobDeclarationSchema.Instance };
		}

		#endregion

		#region Rating

		public static Entity CreateRatingHeader(this RowWrapperRepository repository, Guid? tH_OH = null, string rateType = "COS", Guid? companyPK = null)
		{
			var row = repository.New(RatingHeaderSchema.Instance);
			row[RatingHeaderSchema.TH_RateType] = rateType;
			if (tH_OH.HasValue)
			{
				row[RatingHeaderSchema.TH_OH] = tH_OH.Value;
			}
			if (companyPK.HasValue)
			{
				row[RatingHeaderSchema.TH_GC] = companyPK;
			}

			return new Entity { Repository = repository, Row = row, Schema = RatingHeaderSchema.Instance };
		}

		public static Entity CreateRateEntry(this Entity header, string origin = "UA", string destination = "AU", string tI_RateCategory = "AIR", string tI_Mode = "LSE", string tI_PaymentTerm = "", string tI_RX_NKCurrency = "", Guid? publisherPK = null)
		{
			var row = header.Repository.New(RateEntrySchema.Instance);
			row[RateEntrySchema.TI_RateStartDate] = DateTime.Now;
			row[RateEntrySchema.TI_TH] = header.Row.PK;
			row[RateEntrySchema.TI_OriginLRC] = origin;
			row[RateEntrySchema.TI_DestinationLRC] = destination;
			row[RateEntrySchema.TI_RateCategory] = tI_RateCategory;
			row[RateEntrySchema.TI_Mode] = tI_Mode;
			row[RateEntrySchema.TI_GC_Publisher] = publisherPK != null
				? publisherPK
				: header.Row[RatingHeaderSchema.TH_GC];
			row[RateEntrySchema.TI_PaymentTerm] = tI_PaymentTerm;
			row[RateEntrySchema.TI_RX_NKCurrency] = tI_RX_NKCurrency;

			return new Entity { Repository = header.Repository, Row = row, Schema = RateEntrySchema.Instance };
		}

		public static Entity CreateRateLine(this Entity rateEntry, Guid tL_AC, decimal? tL_ConversionFactor = null, string tL_WeightVolume = "KG", byte tL_ActualPercentage = 100, string tL_FactorNumerator = "", string tL_FactorDenominator = "", string tL_RX_NKCurrency = "")
		{
			return rateEntry.Repository.CreateRateLine(rateEntry.Row.PK, tL_AC, tL_ConversionFactor, tL_WeightVolume, tL_ActualPercentage, tL_FactorNumerator: tL_FactorNumerator, tL_FactorDenominator: tL_FactorDenominator, tL_RX_NKCurrency: tL_RX_NKCurrency);
		}

		public static Entity CreateRateLine(this RowWrapperRepository repository, Guid tL_TI, Guid tL_AC, decimal? tL_ConversionFactor = null, string tL_WeightVolume = "KG", byte tL_ActualPercentage = 100, string tL_RateCalculator = "FLT", string tL_FactorNumerator = "", string tL_FactorDenominator = "", string tL_RX_NKCurrency = "")
		{
			var row = repository.New(RateLinesSchema.Instance);
			row[RateLinesSchema.TL_TI] = tL_TI;
			row[RateLinesSchema.TL_AC] = tL_AC;
			row[RateLinesSchema.TL_WeightVolume] = tL_WeightVolume;
			row[RateLinesSchema.TL_ActualPercentage] = tL_ActualPercentage;
			row[RateLinesSchema.TL_RateCalculator] = tL_RateCalculator;
			row[RateLinesSchema.TL_FactorNumerator] = tL_FactorNumerator;
			row[RateLinesSchema.TL_FactorDenominator] = tL_FactorDenominator;
			row[RateLinesSchema.TL_RX_NKCurrency] = tL_RX_NKCurrency;

			if (tL_ConversionFactor.HasValue)
			{
				row[RateLinesSchema.TL_ConversionFactor] = tL_ConversionFactor.Value;
			}

			return new Entity { Repository = repository, Row = row, Schema = RateLinesSchema.Instance };
		}

		public static Entity CreateRateLine(this Entity rateEntry, Guid tL_AC, string tL_Condition, string tL_ConditionalExpression, string tL_RX_NKCurrency = "")
		{
			var row = rateEntry.Repository.New(RateLinesSchema.Instance);
			row[RateLinesSchema.TL_TI] = rateEntry.Row.PK;
			row[RateLinesSchema.TL_AC] = tL_AC;
			row[RateLinesSchema.TL_Condition] = tL_Condition;
			row[RateLinesSchema.TL_ConditionalExpression] = tL_ConditionalExpression;
			row[RateLinesSchema.TL_RX_NKCurrency] = tL_RX_NKCurrency;

			return new Entity { Repository = rateEntry.Repository, Row = row, Schema = RateLinesSchema.Instance };
		}

		public static Entity CreateRateLineItem(this Entity rateLine, string tM_Type = "", string tM_Text = "")
		{
			var row = rateLine.Repository.New(RateLineItemsSchema.Instance);
			row[RateLineItemsSchema.TM_TL] = rateLine.Row.PK;
			row[RateLineItemsSchema.TM_Type] = tM_Type;
			row[RateLineItemsSchema.TM_Text] = tM_Text;

			return new Entity { Repository = rateLine.Repository, Row = row, Schema = RateLineItemsSchema.Instance };
		}

		#endregion

		#region Transport Zone Sets

		public static Entity CreateTransportZoneSet(this RowWrapperRepository repository, string countryCode = "AU", string type = "ALL")
		{
			var row = repository.New(RateTransportProviderSchema.Instance);
			row[RateTransportProviderSchema.TP_RN_NKCountry] = countryCode;
			row[RateTransportProviderSchema.TP_ZoneType] = type;
			row[RateTransportProviderSchema.TP_IsActive] = true;

			return new Entity { Repository = repository, Row = row, Schema = RateTransportProviderSchema.Instance };
		}

		public static Entity CreateTransportZone(this Entity zoneSet, string zoneName)
		{
			var row = zoneSet.Repository.New(RateTransportZonesSchema.Instance);
			row[RateTransportZonesSchema.TZ_TP] = zoneSet.Row.PK;
			row[RateTransportZonesSchema.TZ_ZoneName] = zoneName;
			row[RateTransportZonesSchema.TZ_IsActive] = true;

			return new Entity { Repository = zoneSet.Repository, Row = row, Schema = RateTransportZonesSchema.Instance };
		}

		#endregion

		public static Entity Setup(this Entity entity, Action<Entity> initalizer)
		{
			initalizer(entity);
			return entity;
		}

		public class Entity
		{
			public RowWrapperRepository Repository;
			public RowWrapper Row;
			public ITableSchema Schema;
		}
	}
}

#endif
