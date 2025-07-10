using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public static class ReportManager
	{
		public static IDisposable SetupReportData(CusExitConsignment consignment, CusExitReport report)
		{
			IDisposable result = null;
			if (consignment != null)
			{
				var disposableActions = new List<IDisposable>();
				var reportPK = report?.PK ?? ZGuid.Empty;
				foreach (var item in consignment.CusExitConsignmentItems)
				{
					disposableActions.Add(item.SetupReportItemData(report));
					item.CusExitConsignmentPackagePivots.Cast<CusExitConsignmentPivot>().Select(x => x.Package).WhereNotNull().ForEach(package =>
					{
						disposableActions.Add(package.SetupReportItemData(reportPK));
					});
				}
				result = new DisposableAction(() => disposableActions.ForEach(x => x.Dispose()));
			}
			return result;
		}

		public static void CreateOrUpdateReport(CusExitConsignment consignment, CusExitReport report)
		{
			if (consignment != null && consignment.Header is CusExitHeader header)
			{
				if (report == null)
				{
					report = header.CusExitReports.AddNew();
				}
				report.CER_CXC_Consignment = consignment.PK;

				var consignmentItems = consignment.CusExitConsignmentItems;
				report.SetReportBehaviorFromConsignmentItems();
				report.DefaultDataFromParent();

				var reportPK = report.PK;
				foreach (var consignmentItem in consignmentItems)
				{
					consignmentItem.SetMatchingExitReport(report);
					if (consignmentItem.CCI_Calc_ShouldReportItem)
					{
						var consignmentItemPK = consignmentItem.PK;
						var grossMass = consignmentItem.CCI_Calc_ReportGrossMass;
						var netMass = consignmentItem.CCI_Calc_ReportNetMass;
						foreach (var package in consignmentItem.CusExitConsignmentPackagePivots.Select(x => x.Package).WhereNotNull())
						{
							package.SetMatchingExitReportPK(reportPK);
							if (package.CXP_Calc_ShouldReportItem)
							{
								var reportItem = package.FirstReportItem;
								if (reportItem == null)
								{
									reportItem = report.CusExitReportItems.AddNew();
									reportItem.ERI_CCI_ConsignmentItem = consignmentItemPK;
									reportItem.ERI_CXP_Package = package.PK;
									reportItem.ERI_Quantity = package.CXP_Quantity;
								}
								else
								{
									reportItem.ERI_Quantity = package.CXP_Calc_ReportQuantity;
								}
								reportItem.ERI_GrossMass = grossMass;
								reportItem.ERI_NetMass = netMass;
							}
							else
							{
								package.GetMatchingExitReportItems().DeleteAll();
							}
						}
					}
					else
					{
						consignmentItem.GetMatchingExitReportItems().DeleteAll();
						foreach (var package in consignmentItem.CusExitConsignmentPackagePivots.Select(x => x.Package).WhereNotNull())
						{
							package.SetMatchingExitReportPK(reportPK);
							package.GetMatchingExitReportItems().DeleteAll();
						}
					}
				}

				report.ResetCusExitReportItemsForBindingAdditionalFilter();
			}
		}
	}
}
