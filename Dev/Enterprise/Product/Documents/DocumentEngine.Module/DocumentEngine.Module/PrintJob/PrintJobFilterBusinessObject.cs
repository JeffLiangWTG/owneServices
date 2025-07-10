using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Module
{
	public class PrintJobFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddStatusFilters(filters);
			AddDateFilters(filters);
			AddRelatedItemFilters(filters);
			AddJobTypeFilters(filters);
			AddSignByFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Subject", StmPrintJobSchema.SP_EmailSubjectLine).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintJobFilter|Subject", "Subject");
			filters.AddTextFilter("Name", StmPrintJobSchema.SP_DocumentName).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintJobFilter|Name", "Name");
		}

		#endregion

		#region Status

		void AddJobTypeFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter filter = filters.AddTextFilter("Job Type",
				delegate(ZString value)
				{
					if (value.EqualsIgnoringCase(nameof(PrintJobType.ALL)))
					{
						return new ZQuery();
					}
					return new ZQuery(StmPrintJobSchema.SP_JobType, value);
				}, JobTypeList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.DefaultProperty = nameof(PrintJobType.ALL);
			filter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintJobFilter|JobType", "Job Type");
		}

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter statusFilter = filters.AddTextFilter("Status", GetStatusFilter, StatusList);
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.DefaultProperty = "PRG";
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintJobFilter|Status", "Status");
		}

		ZQuery GetStatusFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value == "PRG")
			{
				query.AddToFilter(StmPrintJobSchema.SP_RetryAttempts, SQLComparisonOperator.Equal, 0);
				query.AddToFilter(JoinCondition.And, StmPrintJobSchema.SP_Status, SQLComparisonOperator.NotEqual, nameof(PrintJobStatus.FAL));
			}
			else if (value == "FLD")
			{
				query.AddToFilter(StmPrintJobSchema.SP_RetryAttempts, SQLComparisonOperator.GreaterThan, 0);
				query.AddToFilter(JoinCondition.Or, StmPrintJobSchema.SP_Status, SQLComparisonOperator.Equal, nameof(PrintJobStatus.FAL));
			}

			return query;
		}

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Run (Local)", StmPrintJobSchema.SP_RunDateTime, true).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintJobFilter|RunLocal", "Run (Local)");
			filters.AddDateFilter("Run (UTC)", StmPrintJobSchema.SP_RunDateTime).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintJobFilter|Run", "Run (UTC)");
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			filters.AddNkFilter("Printed By", StmPrintJobSchema.SP_GS_NKJobSubmittedBy, ModuleIDs.GlbStaff, StaffList).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintJobFilter|PrintedBy", "Printed By");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description string")]
		protected const string SignByFilterDescription = "Sign By";

		#endregion

		#region Sign By

		void AddSignByFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter filter = filters.AddTextFilter(SignByFilterDescription,
				delegate(ZString value)
				{
					return new ZQuery(StmPrintJobSchema.SP_SignBy, value);
				}, DocumentsSignBy.GetSignByList());
			filter.Category = FilterCategories.StatusAndFlags;
			filter.DefaultProperty = DocumentsSignBy.NON;
			filter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintJobFilter|SignBy", "Sign By");
		}

		#endregion

		#endregion

		#region Lookups

		#region Staff List

		public GlbStaffCollection StaffList
		{
			get
			{
				if (fStaffList == null)
				{
					fStaffList = new GlbStaffCollection(Factory);
				}

				return fStaffList;
			}
		}

		GlbStaffCollection fStaffList;

		#endregion

		#region Status List

		public CodeDescriptionPairList StatusList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair("PRG", Res.GetString("DocumentEngine|PrintJobFilter|Status|PRG", "In Progress"));
				list.AddPair("FLD", Res.GetString("DocumentEngine|PrintJobFilter|Status|FLD", "Failed"));

				return list;
			}
		}

		#endregion

		#region Job Type List

		public CodeDescriptionPairList JobTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
				foreach (string name in Enum.GetNames(typeof(PrintJobType)))
				{
					result.AddPair(name, PrintTypeName.Name(name));
				}
				return result;
			}
		}

		#endregion

		#endregion
	}
}
