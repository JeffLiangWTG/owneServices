using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ComplianceRisk.Integration.ComplianceEventList;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList.Codes;
using static Enterprise.Registry.Business.DPSFreightMovementRestrictionsOptions.Codes;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskStatusSupporter : IComplianceRiskStatusSupporter
	{
		public ComplianceRiskStatusObject GetStatus(IComplianceItemRiskStatusProvider provider)
		{
			var complianceRiskStatus = GetComplianceRiskStatus(provider);
			if (complianceRiskStatus == null)
			{
				return new ComplianceRiskStatusObject(Blocked, HighRisk, Blocked, Incomplete);
			}

			return new ComplianceRiskStatusObject(complianceRiskStatus.COR_OverallRisk, complianceRiskStatus.COR_PartyRisk,
				complianceRiskStatus.COR_LocationRisk, complianceRiskStatus.COR_CommodityRisk);
		}

		public bool IsDPSFreightMovementRestricted(ZString screeningStatus, IComplianceJobDirectionProvider jobDirection, BusinessObject parent)
		{
			if (parent is IComplianceItemRiskStatusProvider complianceRiskProvider
				&& complianceRiskProvider.IsEnabledComplianceWise)
			{
				var complianceRiskStatus = parent.Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, complianceRiskProvider.ParentID));
				if (complianceRiskStatus == null)
				{
					return IsJobDirectionRestricted(jobDirection);
				}

				return complianceRiskStatus.COR_OverallRisk != OverrideClear
					&& complianceRiskStatus.COR_OverallRisk != Clear
					&& IsJobDirectionRestricted(jobDirection);
			}
			else
			{
				var isJobWarehouse = parent.TableName == WhsDocketSchema.Constants.TableName;
				var isImport = (isJobWarehouse || jobDirection == null) ? (ZBool)false : jobDirection.IsImport();
				var isExport = (isJobWarehouse || jobDirection == null) ? (ZBool)false : jobDirection.IsExport();

				return OrganisationsDataRegistry.Instance.IsDPSFreightMovementRestricted(screeningStatus, isExport, isImport);
			}
		}

		static bool IsJobDirectionRestricted(IComplianceJobDirectionProvider jobDirection)
		{
			var isRestricted = false;
			var restrictionType = OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.Value;

			if (restrictionType == All ||
				(restrictionType == Exp && jobDirection.IsExport()) ||
				(restrictionType == Int && (jobDirection.IsExport() || jobDirection.IsImport())))
			{
				isRestricted = true;
			}

			return isRestricted;
		}

		public IEnumerable<ComplianceCommodity> FetchCommodityDetailsInDB(IComplianceItemRiskStatusProvider provider)
		{
			var complianceCommodity = Enumerable.Empty<ComplianceCommodity>();
			var complianceRiskQuery = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, provider.ParentID);
			var complianceRiskStatus = provider.Factory.LoadTop1<ComplianceRiskStatus>(complianceRiskQuery);
			if (complianceRiskStatus != null)
			{
				var commodityDetails = provider.Factory.Load<ComplianceCommodityDetail>(new ZQuery(ComplianceCommodityDetailSchema.CCD_COR_ComplianceRisk, complianceRiskStatus.PK));
				if (commodityDetails != null && commodityDetails.Any())
				{
					var jobParent = CodePropertyAttribute.CodeFromBusinessObject(provider.Factory.Load(provider.ParentTableCode, provider.ParentID));
					complianceCommodity = commodityDetails.Select(o => new ComplianceCommodity(o.CCD_HarmonizedCode, o.CCD_CountryOrGrouping, jobParent, provider.ParentID, o.CCD_RN_NKOrigin, string.Empty, o.CCD_Description, o.CCD_RiskStatus, o.CCD_AssessmentNotes, o.CCD_SystemCreateTimeUtc));
				}
			}

			return complianceCommodity;
		}

		public void PreFetchHintsCommodityDetails(IComplianceItemRiskStatusProvider[] providers, BusinessObjectFactory factory)
		{
			var complianceRiskQueries = new List<ZQuery>();

			foreach (var provider in providers)
			{
				var complianceRiskQuery = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, provider.ParentID);
				factory.AddFetchHint(typeof(ComplianceRiskStatus), complianceRiskQuery);

				complianceRiskQueries.Add(complianceRiskQuery);
			}

			foreach (var complianceRiskQuery in complianceRiskQueries)
			{
				var complianceRiskStatus = factory.LoadTop1<ComplianceRiskStatus>(complianceRiskQuery);
				if (complianceRiskStatus != null)
				{
					factory.AddFetchHint(typeof(ComplianceCommodityDetail), new ZQuery(ComplianceCommodityDetailSchema.CCD_COR_ComplianceRisk, complianceRiskStatus.PK));
				}
			}
		}

		public void CopyAssessmentDetailsFromBookingToShipment(ZGuid bookingPK, ZGuid shipmentPK, BusinessObjectFactory factory)
		{
			var bookingComplianceRiskStatus = factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, bookingPK));
			var existingShipmentComplianceRiskStatus = factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipmentPK));

			if (bookingComplianceRiskStatus != null && existingShipmentComplianceRiskStatus == null)
			{
				var shipmentComplianceRiskStatus = factory.New<ComplianceRiskStatus>();
				shipmentComplianceRiskStatus.COR_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				shipmentComplianceRiskStatus.COR_ParentID = shipmentPK;
				shipmentComplianceRiskStatus.CopyFromBooking = true;
				shipmentComplianceRiskStatus.CopyFromBookingFirstLoaded = true;
				shipmentComplianceRiskStatus.COR_OverallRisk = bookingComplianceRiskStatus.COR_OverallRisk;
				shipmentComplianceRiskStatus.COR_PartyRisk = bookingComplianceRiskStatus.COR_PartyRisk;
				shipmentComplianceRiskStatus.COR_LocationRisk = bookingComplianceRiskStatus.COR_LocationRisk;

				var bookingCommodityDetails = factory.Load<ComplianceCommodityDetail>(new ZQuery(ComplianceCommodityDetailSchema.CCD_COR_ComplianceRisk, bookingComplianceRiskStatus.PK));
				foreach (var bookingCommodityDetail in bookingCommodityDetails)
				{
					var shipmentCommodityDetail = factory.New<ComplianceCommodityDetail>();
					shipmentCommodityDetail.CCD_COR_ComplianceRisk = shipmentComplianceRiskStatus.PK;
					shipmentCommodityDetail.CCD_HarmonizedCode = bookingCommodityDetail.CCD_HarmonizedCode;
					shipmentCommodityDetail.CCD_CountryOrGrouping = bookingCommodityDetail.CCD_CountryOrGrouping;
					shipmentCommodityDetail.CCD_RN_NKOrigin = bookingCommodityDetail.CCD_RN_NKOrigin;
					shipmentCommodityDetail.CCD_Description = bookingCommodityDetail.CCD_Description;
				}
			}
			else if (existingShipmentComplianceRiskStatus != null)
			{
				existingShipmentComplianceRiskStatus.CopyFromBooking = true;
				existingShipmentComplianceRiskStatus.CopyFromBookingFirstLoaded = true;
			}
		}

		public void DeleteNotSavedAssessmentDetailsCopiedFromBooking(ZGuid shipmentPK, BusinessObjectFactory factory)
		{
			var query = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipmentPK);
			query.FetchOnlyFromLocalCache = true;
			var complianceRiskStatus = factory.LoadTop1<ComplianceRiskStatus>(query);
			if (complianceRiskStatus != null && !complianceRiskStatus.IsInDatabase && complianceRiskStatus.CopyFromBooking)
			{
				var subQuery = new ZQuery(ComplianceCommodityDetailSchema.CCD_COR_ComplianceRisk, complianceRiskStatus.PK);
				subQuery.FetchOnlyFromLocalCache = true;
				var commodityDetails = factory.Load<ComplianceCommodityDetail>(subQuery);
				foreach (var commodityDetail in commodityDetails)
				{
					commodityDetail.Delete();
				}

				complianceRiskStatus.Delete();
			}
		}

		public void AddComplianceDocumentHoldStatusEventLog(IComplianceItemRiskStatusProvider provider, ZString eventUser, ZString documentName)
		{
			var eventLog = provider.Factory.New<StmComplianceEvent>();
			eventLog.SCE_ParentID = provider.ParentID;
			eventLog.SCE_ParentTableCode = provider.ParentTableCode;
			eventLog.SCE_EventType = AutoEvents.HoldStatusOverrideCode;
			eventLog.SCE_EventSubType = Codes.Override;
			eventLog.SCE_EventReference = ZString.Format(Descriptions.DocumentHoldStatusOverriden + "|{0}", documentName);
			eventLog.SCE_EventTimeOffset = ZDateTimeOffset.Now;
			eventLog.SCE_SystemCreateUser = eventUser;
		}

		public void AddBorderWiseIntegrationLegalBooksViewedEventLog(ComplianceCommodityDetail commodityDetail)
		{
			if (commodityDetail.ComplianceRiskStatus != null && commodityDetail.IsInDatabase)
			{
				var newFactory = new BusinessObjectFactory();
				var eventLog = newFactory.New<StmComplianceEvent>();
				eventLog.SCE_ParentID = commodityDetail.ComplianceRiskStatus.COR_ParentID;
				eventLog.SCE_ParentTableCode = commodityDetail.ComplianceRiskStatus.COR_ParentTableCode;
				eventLog.SCE_EventType = EventType.BorderWiseIntegration;
				eventLog.SCE_EventSubType = Codes.LegalBooksViewed;
				eventLog.SCE_EventReference = commodityDetail.CCD_RN_NKOrigin.IsEmpty ? commodityDetail.CCD_HarmonizedCode : string.Format("{0}|ORG={1}", commodityDetail.CCD_HarmonizedCode, commodityDetail.CCD_RN_NKOrigin);
				eventLog.SCE_EventTimeOffset = ZDateTimeOffset.Now;

				ZExceptionReporting.ProcessWithSaveExceptionHandling(newFactory.Save, null);
			}
		}

		ComplianceRiskStatus GetComplianceRiskStatus(IComplianceItemRiskStatusProvider provider)
		{
			var parentID = provider.ParentID;
			if (provider is IComplianceWiseShipment shipment
				&& shipment.IsBooking
				&& !shipment.IsForwardRegistered
				&& !shipment.OneTimeQuotePK.IsEmpty)
			{
				parentID = shipment.OneTimeQuotePK;
			}
			return provider.Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, parentID));
		}
	}
}
