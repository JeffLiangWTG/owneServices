using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgPartyScreeningStatus : DocumentWrapper, Integration.DocumentWrappers.IDocOrgPartyScreeningStatus
	{
		DocOrgPartyScreeningStatus(StmEntityScreeningLog stmEntityScreeningLog, BusinessObjectFactory factory)
			: base(stmEntityScreeningLog, factory)
		{
		}

		public static DocOrgPartyScreeningStatus New(StmEntityScreeningLog stmEntityScreeningLog, BusinessObjectFactory factory)
		{
			return (stmEntityScreeningLog != null) ? new DocOrgPartyScreeningStatus(stmEntityScreeningLog, factory) : null;
		}

		StmEntityScreeningLog StmEntityScreeningLog
		{
			get { return (StmEntityScreeningLog)WrappedObject; }
		}

		public ZString StatusDescription
		{
			get { return StmEntityScreeningLog.StatusDescription; }
		}

		public ZGuid PJ_PK
		{
			get { return StmEntityScreeningLog.PK; }
		}

		public ZString PJ_ClearedReason
		{
			get { return StmEntityScreeningLog.PJ_ClearedReason; }
		}

		public ZString PJ_ExcludedLists
		{
			get { return StmEntityScreeningLog.PJ_ExcludedLists; }
		}

		public ZString PJ_SystemCreateUser
		{
			get { return StmEntityScreeningLog.PJ_SystemCreateUser; }
		}

		public ZString PJ_HighConfidenceResults
		{
			get { return StmEntityScreeningLog.PJ_HighConfidenceResults; }
		}

		public ZString PJ_IncludedLists
		{
			get { return StmEntityScreeningLog.PJ_IncludedLists; }
		}

		public ZBool PJ_IsForcedRescreen
		{
			get { return StmEntityScreeningLog.PJ_IsForcedRescreen; }
		}

		public ZString PJ_ListsUsed
		{
			get { return StmEntityScreeningLog.PJ_ListsUsed; }
		}

		public ZInt PJ_LowConfidenceResultsCount
		{
			get { return StmEntityScreeningLog.PJ_LowConfidenceResultsCount; }
		}

		public ZString PJ_MatchingData
		{
			get { return StmEntityScreeningLog.PJ_MatchingData; }
		}

		public ZString PJ_MediumConfidenceResults
		{
			get { return StmEntityScreeningLog.PJ_MediumConfidenceResults; }
		}

		public ZGuid PJ_ParentID
		{
			get { return StmEntityScreeningLog.PJ_ParentID; }
		}

		public ZString PJ_ParentTableCode
		{
			get { return StmEntityScreeningLog.PJ_ParentTableCode; }
		}

		public ZDateTime PJ_SystemCreateTimeUtc
		{
			get { return StmEntityScreeningLog.PJ_SystemCreateTimeUtc; }
		}

		public ZDateTime PJ_ScreenDate
		{
			get { return PJ_SystemCreateTimeUtc.ToLocalBranchTime(); }
		}

		public ZGuid PJ_SourceID
		{
			get { return StmEntityScreeningLog.PJ_SourceID; }
		}

		public ZString PJ_SourceTableCode
		{
			get { return StmEntityScreeningLog.PJ_SourceTableCode; }
		}

		public ZString PJ_Status
		{
			get { return StmEntityScreeningLog.PJ_Status; }
		}

		public ZString ScreenedByFullName
		{
			get { return StmEntityScreeningLog.ScreenedByFullName; }
		}

		public ZString ParentDescription
		{
			get { return StmEntityScreeningLog.ParentDescription; }
		}
		public ZString SourceInformation
		{
			get { return StmEntityScreeningLog.SourceInformation; }
		}
	}
}
