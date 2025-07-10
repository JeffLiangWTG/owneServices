using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI
{
	public class ReportUrlHandler : UrlHandler, IReportUrlHandler
	{
		protected ReportUrlHandler()
		{
		}

		public static ReportUrlHandler Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new ReportUrlHandler();
				}
				return instance;
			}
		}
		static ReportUrlHandler instance;

		public string Create(ReportCommand reportCommand)
		{
			using (DocumentPack documentPack = new DocumentPack(reportCommand))
			{
				Report report = (Report)documentPack[0];
				return Create(report);
			}
		}

		public string Create(Report report)
		{
			QueryString queryString = new QueryString();
			queryString.Add((NoResString)"Command", ExpectedCommandText);
			queryString.Add("LicenceCode", GetCurrentCompanyLicenceKeyIdentifier(GlbCompany.CurrentCompany));
			queryString.Add("ReportPK", report.Parent.StmMenuCommand.PK.ToString());

			PopulateFilterCriteria(report, queryString);

			if (InstanceDetails.Current != null)
			{
				if(InstanceDetails.Current.Domain != null)
				{
					queryString.Add((NoResString)"Domain", InstanceDetails.Current.Domain);
					queryString.Add((NoResString)"Instance", InstanceDetails.Current.Instance);
				}

				if (InstanceDetails.ShouldAddDatabaseInfoToUrls)
				{
					queryString.Add("ServerName", InstanceDetails.Current.ServerName);
					queryString.Add("DatabaseName", InstanceDetails.Current.DatabaseName);
				}
			}
			queryString.Add((NoResString)"Hash", CreateQueryStringSecurityHash(queryString));

			return GetUrlFromQueryString(queryString);
		}

		#region UrlHandler Overrides

		protected override string ExpectedCommandText
		{
			get { return "RunReport"; }
		}

		protected override bool HandleCore(QueryString queryString)
		{
			var reportCommand = LoadReportCommand(queryString["ReportPK"]);
			var reportSet = NewReportPrintSetToRun(reportCommand);
			var report = GetReportFromReportSet(reportSet) ?? throw new EnterpriseUrlHandlerException("The template of this report has been removed, check it first.");

			if (!HasRightToRunReport(reportCommand))
			{
				return true;
			}

			try
			{
				ProcessReport(report, reportSet, queryString);
			}
			catch (FormatException e)
			{
				throw new EnterpriseUrlHandlerException(e.Message);
			}
			return true;
		}

		protected override string[] GetQueryNamesSecuredBySecurityHash(QueryString queryString)
		{
			return new string[] { "LicenceCode", "ReportPK" };
		}

		protected virtual void ProcessReport(Report report, ReportPrintSet reportSet, QueryString queryString)
		{
			ParseFilterCriteria(report, queryString);

			reportSet.Run(null);
		}

		ReportCommand LoadReportCommand(ZString reportPKAsString)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZGuid reportPK = new ZGuid(reportPKAsString);

			ReportCommand result = factory.Load<ReportCommand>(reportPK) ?? throw new EnterpriseUrlHandlerException("The report this link was created from isn't available for this version of " + Core.Constants.ProductName);

			return result;
		}

		bool HasRightToRunReport(ReportCommand command)
		{
			var reportBusinessContext = command.SU_BusinessContext.Substring(3);
			var moduleInfoId = ModuleIDs.AllIncludingClientModules.FirstOrDefault(m => m.Name == reportBusinessContext) ?? throw new EnterpriseUrlHandlerException(FormattableString.Invariant($"The report {reportBusinessContext} could not be found in client modules"));

			using (var module = ZModuleFactory.Instance.Create(moduleInfoId))
			{
				var securityCheckpoint = (ISecurityCheckpoint)module.SecurityCheckpoint;

				bool result = true;
				if (securityCheckpoint != Env.Security.None)
				{
					SecurityCheckpoint checkpoint = Env.Security.FindOrCreateReportCheckpoint(command.PK.ToGuid(), command.SU_MenuNameMultilingual, module.ID, securityCheckpoint);
					result = checkpoint.IsAllowed;
					if (!result)
					{
						checkpoint.ShowError();
					}
				}
				return result;
			}
		}

		protected virtual ReportPrintSet NewReportPrintSetToRun(ReportCommand reportCommand)
		{
			return new ReportPrintSet(reportCommand);
		}

		protected virtual Report GetReportFromReportSet(ReportPrintSet reportSet)
		{
			return reportSet[0].GetFirstReport();
		}

		#endregion

		#region PopulateFilterCriteria

		void PopulateFilterCriteria(Report report, QueryString queryString)
		{
			report.PrepareForRender();
			PopulateSortOrder(report, queryString);
			PopulateGroupBy(report, queryString);
			PopulateOptionalTemplates(report, queryString);
			PopulateConfiguration(report, queryString);

			PopulateFilterCriteria(report.FilterCollection, queryString, true, false);
			PopulateFilterCriteria(report.FilterCollection, queryString, false, true);
		}

		void PopulateFilterCriteria(CollectionOfIFilter filters, QueryString queryString, bool includePopulatedFilters, bool includeEmptyFilters)
		{
			foreach (IFilter filterObject in filters)
			{
				FilterField filter = filterObject as FilterField;
				if (filter != null)
				{
					if ((includeEmptyFilters && filter.IsEmpty) || (includePopulatedFilters && !filter.IsEmpty))
					{
						PopulateFilterCriteria(filterObject, queryString);
					}
				}
			}
		}

		void PopulateFilterCriteria(IFilter filterObject, QueryString queryString)
		{
			FilterFieldValueSerialisable filter = filterObject as FilterFieldValueSerialisable;
			DateRangeField dateRangeFilter = filterObject as DateRangeField;

			if (dateRangeFilter != null)
			{
				queryString[dateRangeFilter.DisplayName + " From"] = dateRangeFilter.ValueLow.ToISO8601String();
				queryString[dateRangeFilter.DisplayName + " To"] = dateRangeFilter.ValueHigh.ToISO8601String();
			}
			else if (filter != null)
			{
				queryString[filter.DisplayName] = filter.ValueAsStringForSerialisation;
			}
		}

		void PopulateSortOrder(Report report, QueryString queryString)
		{
			foreach (SortOrder sort in report.SortOrderCollection)
			{
				if (sort.Selected)
				{
					queryString["Sort"] = sort.DisplayName;
				}
			}
		}

		void PopulateGroupBy(Report report, QueryString queryString)
		{
			foreach (GroupBy groupBy in report.GroupByCollection)
			{
				if (groupBy.Selected)
				{
					queryString["GroupBy"] = groupBy.DisplayName;
				}
			}
		}

		void PopulateOptionalTemplates(Report report, QueryString queryString)
		{
			int i = 1;
			foreach (OptionalTemplateSheet optionalTemplate in report.OptionalTemplateSheetCollection)
			{
				if (optionalTemplate.Selected)
				{
					queryString["OptionalTemplate" + i] = optionalTemplate.DisplayName;
					i++;
				}
			}
		}

		void PopulateConfiguration(Report report, QueryString queryString)
		{
			if (report.ColumnHeadingManager != null && report.ColumnHeadingManager.CurrentColumnConfigurationManager != null)
			{
				queryString["Configuration"] = report.ColumnHeadingManager.CurrentColumnConfigurationManager.Description.ToString();
			}
		}

		#endregion

		#region ParseFilterCriteria

		void ParseFilterCriteria(Report report, QueryString queryString)
		{
			report.PrepareForRender();

			ParseConfiguration(report, queryString);

			foreach (IFilter filter in report.FilterCollection)
			{
				ParseFilterCriteria(filter, queryString);
			}

			ParseSortOrder(report, queryString);
			ParseGroupBy(report, queryString);
			ParseOptionalTemplates(report, queryString);
		}

		void ParseConfiguration(Report report, QueryString queryString)
		{
			string configurationDescription = queryString["Configuration"];

			if (!string.IsNullOrWhiteSpace(configurationDescription))
			{
				var manager = report.ColumnHeadingManager.ConfigurationManagersForAllSavedConfigurations.FirstOrDefault(x => x.Description == configurationDescription);
				if (manager != null)
				{
					manager.Load(report);
				}
			}
		}

		static void ParseFilterCriteria(IFilter filterObject, QueryString queryString)
		{
			FilterFieldValueSerialisable filter = filterObject as FilterFieldValueSerialisable;
			DateRangeField dateRangeFilter = filterObject as DateRangeField;
			if (dateRangeFilter != null)
			{
				ZDateTime from;
				ZDateTime to;
				if (ZDateTime.TryParseISO8601Date(queryString[dateRangeFilter.DisplayName + " From"], out from))
				{
					dateRangeFilter.ValueLow = from;
				}
				if (ZDateTime.TryParseISO8601Date(queryString[dateRangeFilter.DisplayName + " To"], out to))
				{
					dateRangeFilter.ValueHigh = to;
				}
			}
			else if (filter != null)
			{
				string value = queryString[filter.DisplayName];
				if (!string.IsNullOrEmpty(value))
				{
					filter.ValueAsStringForSerialisation = queryString[filter.DisplayName];
				}
			}
		}

		void ParseSortOrder(Report report, QueryString queryString)
		{
			string selectedSort = queryString["Sort"];
			if (!string.IsNullOrEmpty(selectedSort))
			{
				foreach (SortOrder sort in report.SortOrderCollection)
				{
					sort.Selected = (sort.DisplayName == selectedSort);
				}
			}
		}

		void ParseGroupBy(Report report, QueryString queryString)
		{
			string selectedGroupBy = queryString["GroupBy"];
			if (!string.IsNullOrEmpty(selectedGroupBy) && report.GroupByCollection[selectedGroupBy] != null)
			{
				foreach (GroupBy groupBy in report.GroupByCollection)
				{
					groupBy.Selected = (groupBy.DisplayName == selectedGroupBy);
				}
			}
		}

		void ParseOptionalTemplates(Report report, QueryString queryString)
		{
			foreach (string name in queryString.Keys)
			{
				if (name.StartsWith("OptionalTemplate"))
				{
					OptionalTemplateSheet optionalTemplate = report.OptionalTemplateSheetCollection[queryString[name]];
					if (optionalTemplate != null)
					{
						optionalTemplate.Selected = true;
					}
				}
			}
		}

		#endregion
	}
}
