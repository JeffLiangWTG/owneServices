using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine
{
	public static class WebReportHelper
	{
		public static WebReportCommandCollection GetAvailableWebReports(BusinessObjectFactory factory, List<ZString> businessContexts, bool isLoggedIn, Func<WebSecurityRight, bool> isRightGranted)
		{
			var filter = new ZQuery(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.WebReports);
			filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.StartsWith, Core.Constants.BusinessContextPrefixes.Reports);
			if (businessContexts?.Count > 0)
			{
				filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.Contains, businessContexts);
			}
			var allowedReports = GetAllowedReportPKs(factory, isLoggedIn, isRightGranted);
			if (allowedReports.Length > 0)
			{
				filter.AddToFilter(StmMenuItemSchema.PK, allowedReports);
			}
			else
			{
				filter.IsNoResultQuery = true;
			}

			var webReports = new WebReportCommandCollection(factory, filter);
			webReports.Load();
			return webReports;
		}

		static ZGuid[] GetAllowedReportPKs(BusinessObjectFactory factory, bool isLoggedIn, Func<WebSecurityRight, bool> isRightGranted)
		{
			var result = new List<ZGuid>();
			if (isLoggedIn)
			{
				var reports = new StmMenuItemCollection(factory);
				var reportQuery = new ZQuery(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.WebReports);
				reportQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.StartsWith, Core.Constants.BusinessContextPrefixes.Reports);
				reportQuery.AddToFilter(StmMenuItemSchema.SU_IsVisibleOnWeb, ZBool.True);
				reports.Load(reportQuery);

				foreach (StmMenuItem report in reports)
				{
					var right = ReportsWebSecurityRights.GetSecurityRightForReport(report);

					if (right != null && isRightGranted(right))
					{
						result.Add(report.PK);
					}
				}
			}

			return result.ToArray();
		}
	}
}
