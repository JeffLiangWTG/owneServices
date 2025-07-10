using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitConsignment : EU.ExitControl.Business.CusExitConsignment
		, Integration.Customs.ESExitControl.ICusExitConsignment
	{
		public CusExitConsignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new ExitControlBase.Business.ICusExitConsignmentItemCollection<CusExitConsignmentItem> CusExitConsignmentItems => (ExitControlBase.Business.ICusExitConsignmentItemCollection<CusExitConsignmentItem>)base.CusExitConsignmentItems;

		protected override ExitControlBase.Business.ICusExitConsignmentItemCollection<ExitControlBase.Business.CusExitConsignmentItem> CreateNewCusExitConsignmentItemCollection() => new ExitControlBase.Business.CusExitConsignmentItemCollection<CusExitConsignmentItem>(this);

		public override void OnSaving()
		{
			base.OnSaving();

			if (!Header.CusExitReports.Any(r => r.CER_CXC_Consignment == PK))
			{
				var newReport = Header.CusExitReports.AddNew();
				newReport.CER_Type = ExitReportTypeList.Codes.Presentation;
				newReport.CER_CXC_Consignment = PK;
			}
		}

		public void CreateOrUpdateReport()
		{
			var report = (CusExitReport)Header.CusExitReports.FirstOrDefault(x => x.CER_CXC_Consignment == PK);
			if (report == null)
			{
				report = (CusExitReport)Header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = PK;
			}

			var consignmentItemsWithDIF = GetConsignmentItemsWithDIF();
			foreach (var consignmentItem in consignmentItemsWithDIF)
			{
				var consignmentItemPK = consignmentItem.PK;

				var reportItemForConsignment = report.CusExitReportItems.FirstOrDefault(x => x.ERI_CCI_ConsignmentItem == consignmentItem.PK);
				var grossMass = reportItemForConsignment?.ERI_GrossMass ?? consignmentItem.CCI_Calc_ReportGrossMass;
				var netMass = reportItemForConsignment?.ERI_NetMass ?? consignmentItem.CCI_Calc_ReportNetMass;

				var packagePivotsWithDIF = consignmentItem.CusExitConsignmentPackagePivots.Where(p => p.Package != null && ((CusExitConsignmentPackage)p.Package).StatusIsDifferencesToDeclared).Select(p => p.Package);

				if (!packagePivotsWithDIF.IsNullOrEmpty())
				{
					foreach (CusExitConsignmentPackage package in packagePivotsWithDIF)
					{
						var reportItemForPackage = report.CusExitReportItems.FirstOrDefault(x => x.ERI_CXP_Package == package.PK);
						CreateReportItem(report, reportItemForPackage, consignmentItemPK, grossMass, netMass, package);
					}
				}
				else
				{
					CreateReportItem(report, reportItemForConsignment, consignmentItemPK, grossMass, netMass, null);
				}

				var additionalInfosWithDIF = consignmentItem.AdditionalInfos.Where(a => a.StatusIsDifferencesToDeclared);

				if (!additionalInfosWithDIF.IsNullOrEmpty())
				{
					var reportItemsForConsignment = report.CusExitReportItems.Where(x => x.ERI_CCI_ConsignmentItem == consignmentItem.PK);
					var reportItemForAddInfos = reportItemsForConsignment.Count() > 1
																				? reportItemsForConsignment.FirstOrDefault(x => !x.AdditionalInfos.IsNullOrEmpty()) ?? reportItemsForConsignment.First()
																				: reportItemsForConsignment.FirstOrDefault();
					foreach (var addInfo in additionalInfosWithDIF)
					{
						if (!reportItemForAddInfos.AdditionalInfos.Any(x => x.CSI_ItemNumber == addInfo.CSI_ItemNumber))
						{
							var reportItemAddInfo = addInfo.Clone();
							reportItemForAddInfos.AdditionalInfos.Add(reportItemAddInfo);
						}
					}
				}
			}

			RemoveNotDIFAdditionalInfos(report);
			RemoveItemsWithNotDIFPackagesAndNoAdditionalInfos(report);

			foreach (var reportItem in GetReportItemsWithConsignmentItemNotDIF(report))
			{
				report.CusExitReportItems.Delete(reportItem);
			}

			report.ResetCusExitReportItemsForBindingAdditionalFilter();
		}

		IEnumerable<CusExitConsignmentItem> GetConsignmentItemsWithDIF()
		{
			return CusExitConsignmentItems.Where(x => x.StatusIsDifferencesToDeclared
														|| (!x.StatusIsMissing
																&& (x.CusExitConsignmentPackagePivots.Any(p => p.Package != null && ((CusExitConsignmentPackage)p.Package).StatusIsDifferencesToDeclared)
																	|| x.AdditionalInfos.Cast<AdditionalInfo>().Any(a => a.StatusIsDifferencesToDeclared))));
		}

		IEnumerable<AdditionalInfo> GetAdditionalInfosNotDIF(CusExitReport report, CusExitConsignmentItem consignmentItem)
		{
			var addInfosNotDIF = consignmentItem.AdditionalInfos.Where(a => !a.StatusIsDifferencesToDeclared).Select(a => a.CSI_ItemNumber);
			return report.CusExitReportItems.Where(x => x.ERI_CCI_ConsignmentItem == consignmentItem.PK).SelectMany(x => x.AdditionalInfos).Cast<AdditionalInfo>().Where(a => addInfosNotDIF.Contains(a.CSI_ItemNumber));
		}

		IEnumerable<CusExitReportItem> GetReportItemsWithConsignmentItemPackagesNotDIF(CusExitReport report)
		{
			var packagesNotDIF = CusExitConsignmentItems.Where(x => !x.StatusIsMissing)
														.SelectMany(x => x.CusExitConsignmentPackagePivots)
																		.Where(p => p.Package != null && !((CusExitConsignmentPackage)p.Package).StatusIsDifferencesToDeclared)
																		.Select(p => p.Package.PK);
			return report.CusExitReportItems.Where(i => packagesNotDIF.Contains(i.ERI_CXP_Package));
		}

		IEnumerable<CusExitReportItem> GetReportItemsWithConsignmentItemNotDIF(CusExitReport report)
		{
			var consignmentItemsNotDIF = CusExitConsignmentItems.Where(x => x.StatusIsMissing
																				|| (x.CCI_DiscrepancyStatus.IsEmpty
																					&& x.CusExitConsignmentPackagePivots.IsNullOrEmpty()
																					&& x.AdditionalInfos.Cast<AdditionalInfo>().IsNullOrEmpty()))
																.Select(x => x.PK);
			return report.CusExitReportItems.Where(i => consignmentItemsNotDIF.Contains(i.ERI_CCI_ConsignmentItem));
		}

		void RemoveNotDIFAdditionalInfos(CusExitReport report)
		{
			foreach (var consignmentItem in CusExitConsignmentItems.Where(x => !x.StatusIsMissing))
			{
				var additionalInfosNotDIF = GetAdditionalInfosNotDIF(report, consignmentItem).ToArray();
				foreach (var addInfoToRemove in additionalInfosNotDIF)
				{
					var reportItemsWithAddInfos = report.CusExitReportItems.Where(x => x.ERI_CCI_ConsignmentItem == consignmentItem.PK && !x.AdditionalInfos.IsNullOrEmpty());
					foreach (var reportItem in reportItemsWithAddInfos)
					{
						if (reportItem.AdditionalInfos.Contains(addInfoToRemove))
						{
							reportItem.AdditionalInfos.Delete(addInfoToRemove);
						}
					}
				}
			}
		}

		void RemoveItemsWithNotDIFPackagesAndNoAdditionalInfos(CusExitReport report)
		{
			foreach (var reportItem in GetReportItemsWithConsignmentItemPackagesNotDIF(report))
			{
				reportItem.ERI_CXP_Package = ZGuid.Empty;
				reportItem.ERI_Quantity = ZInt.Zero;
			}

			var reportItemsForConsignmentItem = report.CusExitReportItems.GroupBy(x => x.ERI_CCI_ConsignmentItem);
			foreach (var group in reportItemsForConsignmentItem)
			{
				var reportItemsList = group.ToList();
				var hasReportItemWithPackage = reportItemsList.Any(x => !x.ERI_CXP_Package.IsEmpty);
				var reportItemWithAddInfos = reportItemsList.FirstOrDefault(x => !x.AdditionalInfos.IsNullOrEmpty());

				if (!hasReportItemWithPackage && reportItemsList.Count > 1)
				{
					var reportItemToKeep = reportItemWithAddInfos ?? reportItemsList.FirstOrDefault();
					reportItemsList.Remove(reportItemToKeep);
					reportItemsList.ForEach(x => report.CusExitReportItems.Delete(x));
				}
				else if (hasReportItemWithPackage)
				{
					if (reportItemWithAddInfos != null)
					{
						reportItemsList.Remove(reportItemWithAddInfos);
					}
					reportItemsList.Where(x => x.ERI_CXP_Package.IsEmpty).ForEach(x => report.CusExitReportItems.Delete(x));
				}
			}
		}

		void CreateReportItem(CusExitReport report, CusExitReportItem reportItem, ZGuid consignmentItemPK, ZDecimal grossMass, ZDecimal netMass, CusExitConsignmentPackage package)
		{
			if (reportItem == null)
			{
				reportItem = report.CusExitReportItems.AddNew();
				reportItem.ERI_CCI_ConsignmentItem = consignmentItemPK;
				reportItem.ERI_GrossMass = grossMass;
				reportItem.ERI_NetMass = netMass;

				if (package != null)
				{
					reportItem.ERI_Quantity = package.CXP_Calc_ReportQuantity;
					reportItem.ERI_CXP_Package = package.PK;
				}
			}
		}

		protected override bool IsUCC6Core => Header?.IsUCC6 ?? true;
	}
}
