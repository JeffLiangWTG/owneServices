using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.STI.Navision
{
	public class OrgBatchListener : NavisionBatchListener
	{
		public OrgBatchListener(ZDateTime dateTimeExportStarted)
			: base(dateTimeExportStarted)
		{
		}

		public override string BusinessObjectTableName
		{
			get { return OrgHeaderSchema.Constants.TableName; }
		}

		public override Type BusinessObjectType
		{
			get { return typeof(OrgHeader); }
		}

		protected override Type BusinessObjectCollectionType
		{
			get { return typeof(OrgHeaderCollection); }
		}

		protected override NavisionFlatFileExporter Exporter
		{
			get
			{
				if (fExporter == null)
				{
					fExporter = new OrgFlatFileExporter(Factory, Instructions, Instructions.SpecifiedFilename);
				}
				return fExporter;
			}
		}
		protected NavisionFlatFileExporter fExporter;

		protected override ZString ExportDirectory
		{
			get { return STIDataRegistry.Instance.OrganisationExportDirectory; }
		}

		protected override ZString FileNamePreFix
		{
			get { return Constants.FileNamePrefixes.OrganisationFiles; }
		}

		protected override void Process(BusinessObject matchingBusinessObject, StmALog log, INotifications notifications)
		{
			base.Process(matchingBusinessObject, null, notifications);
			matchingBusinessObject.GetLogs().AddNew(Events.DataExport);
			matchingBusinessObject.Factory.Save();
		}

		protected override bool AdditionalMatching(StmALog log)
		{
			bool result = false;
			if (!log.IsDeleted && (log.SL_SE_NKEvent == Events.AddedARecordToTheSystem.Code || log.SL_SE_NKEvent == Events.EditedARecord.Code))
			{
				OrganisationFound = GetOrgHeader(log);
				if (OrganisationFound != null)
				{
					StmALog mostRecentDataExportEvent = OrganisationFound.Logs.MostRecentLogByEventTime(Events.DataExport);
					result = OrganisationFound.OH_IsDebtor && (mostRecentDataExportEvent == null || log.SL_EventTime > mostRecentDataExportEvent.SL_EventTime);
				}
			}

			return result;
		}
		OrgHeader OrganisationFound;

		protected override BusinessObject LoadBusinessObject(StmALog log)
		{
			return OrganisationFound;
		}

		protected override bool MatchTableName(StmALog log)
		{
			return log.SL_Table == BusinessObjectTableName || log.SL_Table == OrgCompanyDataSchema.Constants.TableName || log.SL_Table == OrgAddressSchema.Constants.TableName ||
					 log.SL_Table == OrgStaffAssignmentsSchema.Constants.TableName || log.SL_Table == OrgMiscServSchema.Constants.TableName;
		}

		#region Implementation

		OrgHeader GetOrgHeader(StmALog log)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(BusinessObjectType);
			ZDBOnlySubQuery subQuery = null;
			switch (log.SL_Table)
			{
				case OrgCompanyDataSchema.Constants.TableName:
					subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), null);
					subQuery.AddToFilter(OrgCompanyDataSchema.PK, log.SL_Parent);
					query.AddSubQuery(OrgHeaderSchema.PK, OrgCompanyDataSchema.OB_OH, subQuery, JoinCondition.And);
					break;

				case OrgStaffAssignmentsSchema.Constants.TableName:
					subQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), null);
					subQuery.AddToFilter(OrgStaffAssignmentsSchema.PK, log.SL_Parent);
					query.AddSubQuery(OrgHeaderSchema.PK, OrgStaffAssignmentsSchema.O8_OH, subQuery, JoinCondition.And);
					break;

				case OrgMiscServSchema.Constants.TableName:
					subQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), null);
					subQuery.AddToFilter(OrgMiscServSchema.PK, log.SL_Parent);
					query.AddSubQuery(OrgHeaderSchema.PK, OrgMiscServSchema.OM_OH, subQuery, JoinCondition.And);
					break;

				case OrgAddressSchema.Constants.TableName:
					subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), null);
					subQuery.AddToFilter(OrgAddressSchema.PK, log.SL_Parent);
					query.AddSubQuery(OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, subQuery, JoinCondition.And);
					break;

				case OrgHeaderSchema.Constants.TableName:
					query.AddToFilter(OrgHeaderSchema.PK, log.SL_Parent);
					break;

				default:
					query.AddToFilter(OrgHeaderSchema.PK, ZGuid.Empty);
					break;
			}
			return (OrgHeader)Factory.LoadTop1(BusinessObjectType, query);
		}

		#endregion
	}
}
