using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignmentItem))]
	sealed class CusExitConsignmentItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<CusExitConsignmentItemTypeDecider>(CusExitConsignmentItem.TypeDecider);
		}

		public void TestHumanReadableName()
		{
			consignmentItem.CCI_LineNumber = 15;
			AssertEquals("HumanReadableName", "Consignment Item (15)", consignmentItem.HumanReadableName);
		}

		public void TestConsignment()
		{
			AssertEquals(consignment.PK, consignmentItem.Consignment.PK);
		}

		public void TestPackagesSequenceDictionary()
		{
			consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			consignmentItem.CusExitConsignmentPackagePivots.AddNew();

			var dictionary = new Dictionary<ZShort, ZShort>
			{
				{ 1, 1 },
				{ 2, 1 },
				{ 3, 1 }
			};
			AssertEquals("Dictionary", true, consignmentItem.PackagesSequenceDictionary.ContainsSameElementsInAnyOrder(dictionary));
		}

		public void TestCusExitReportItems()
		{
			var reportItem1 = Factory.New<CusExitReportItem>();
			reportItem1.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			var reportItem2 = Factory.New<CusExitReportItem>();
			reportItem2.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			var reportItem3 = Factory.New<CusExitReportItem>();
			reportItem3.ERI_CCI_ConsignmentItem = Factory.New<CusExitConsignmentItem>().PK;
			AssertContainsExactElementsInAnyOrder(new[] { reportItem1.PK, reportItem2.PK }, consignmentItem.CusExitReportItems.Select(x => x.PK));
		}

		public void TestCusExitConsignmentPivots()
		{
			var consignmentPivot1 = Factory.New<CusExitConsignmentPivot>();
			consignmentPivot1.CNP_CCI_ConsignmentItem = consignmentItem.PK;
			var consignmentPivot2 = Factory.New<CusExitConsignmentPivot>();
			consignmentPivot2.CNP_CCI_ConsignmentItem = consignmentItem.PK;
			var consignmentPivot3 = Factory.New<CusExitConsignmentPivot>();
			consignmentPivot3.CNP_CCI_ConsignmentItem = Factory.New<CusExitConsignmentItem>().PK;
			var exitConsignmentPackagePivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var exitConsignmentContainerPivot = consignmentItem.CusExitConsignmentContainerPivots.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { consignmentPivot1.PK, consignmentPivot2.PK, exitConsignmentPackagePivot.PK, exitConsignmentContainerPivot.PK }, consignmentItem.CusExitConsignmentPivots.Select(x => x.PK));
		}

		public void TestCusExitConsignmentPackagePivots()
		{
			consignmentItem.CusExitConsignmentPivots.AddNew();
			consignmentItem.CusExitConsignmentContainerPivots.AddNew();
			var exitConsignmentPackagePivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var exitConsignmentPackagePivot2 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var packagePivots = consignmentItem.CusExitConsignmentPackagePivots;

			CombineAssertions(() =>
			{
				AssertType<ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>("Type", packagePivots);
				consignmentItem.IsRegisteredEditableChildObject(packagePivots);
				AssertContainsExactElementsInAnyOrder("Elements", new[] { exitConsignmentPackagePivot.PK, exitConsignmentPackagePivot2.PK }, packagePivots.Select(x => x.PK));
			});
		}

		public void TestCusExitConsignmentContainerPivots()
		{
			consignmentItem.CusExitConsignmentPivots.AddNew();
			consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var exitConsignmentContainerPivot = consignmentItem.CusExitConsignmentContainerPivots.AddNew();
			var exitConsignmentContainerPivot2 = consignmentItem.CusExitConsignmentContainerPivots.AddNew();
			var containerPivots = consignmentItem.CusExitConsignmentContainerPivots;

			CombineAssertions(() =>
			{
				AssertType<ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>("Type", containerPivots);
				consignmentItem.IsRegisteredEditableChildObject(containerPivots);
				AssertContainsExactElementsInAnyOrder("Elements", new[] { exitConsignmentContainerPivot.PK, exitConsignmentContainerPivot2.PK }, containerPivots.Select(x => x.PK));
			});
		}

		public void TestCCI_LineNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CCI_LineNumber of consignmentItem is 1", (ZShort)1, consignmentItem.CCI_LineNumber);

				var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
				consignmentItem2.CCI_LineNumber = 1;
				AssertEquals("CCI_LineNumber of consignmentItem2 is 2", (ZShort)1, consignmentItem2.CCI_LineNumber);

				consignmentItem.Delete();
				AssertEquals("consignmentItem is deleted, CusExitConsignmentItems count is 1", 1, consignment.CusExitConsignmentItems.Count);
			});
		}

		public void TestCCI_LineNumber_ReadOnly()
		{
			AssertEquals(false, consignmentItem.CCI_LineNumberInfo.ReadOnly);
		}

		public void TestCCI_LineNumber_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentItem.CCI_LineNumberInfo);
			AssertEquals("Item Number", captionResourceString.Caption);
		}

		public void TestCCI_GrossMass_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentItem.CCI_GrossMassInfo);
			AssertEquals("Gross Mass KG", captionResourceString.Caption);
		}

		public void TestCCI_NetMass_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentItem.CCI_NetMassInfo);
			AssertEquals("Net Mass KG", captionResourceString.Caption);
		}

		public void TestCCI_UniqueConsignmentReference_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentItem.CCI_UniqueConsignmentReferenceInfo);
			AssertEquals("Reference Number UCR", captionResourceString.Caption);
		}

		public void TestCCI_Calc_ReportGrossMass()
		{
			consignmentItem.CCI_GrossMass = 100m;
			AssertEquals("Get the value from CCI_GrossMass", 100m, consignmentItem.CCI_Calc_ReportGrossMass);

			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			reportItem.ERI_GrossMass = 200m;
			using (consignmentItem.SetupReportItemData(report))
			{
				AssertEquals("Get the value from the first ReportItem.ERI_GrossMass", 200m, consignmentItem.CCI_Calc_ReportGrossMass);
			}
			AssertEquals("Get the value from CCI_GrossMass when not report item data", 100m, consignmentItem.CCI_Calc_ReportGrossMass);
		}

		public void TestCCI_Calc_ReportGrossMass_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentItem.CCI_Calc_ReportGrossMassInfo);
			AssertEquals("Gross Mass KG", captionResourceString.Caption);
		}

		public void TestCCI_Calc_ReportNetMass()
		{
			consignmentItem.CCI_NetMass = 100m;
			AssertEquals("Get the value from CCI_NetMass", 100m, consignmentItem.CCI_Calc_ReportNetMass);

			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			reportItem.ERI_NetMass = 200m;
			using (consignmentItem.SetupReportItemData(report))
			{
				AssertEquals("Get the value from the first ReportItem.ERI_NetMass", 200m, consignmentItem.CCI_Calc_ReportNetMass);
			}
			AssertEquals("Get the value from CCI_NetMass when not report item data", 100m, consignmentItem.CCI_Calc_ReportNetMass);
		}

		public void TestCCI_Calc_ReportNetMass_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentItem.CCI_Calc_ReportNetMassInfo);
			AssertEquals("Net Mass KG", captionResourceString.Caption);
		}

		public void TestResetCCI_Calc_ReportNetMassWhenCCI_Calc_ReportGrossMassChanged()
		{
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			reportItem.ERI_GrossMass = 100m;
			reportItem.ERI_NetMass = 50m;
			using (consignmentItem.SetupReportItemData(report))
			{
				AssertEquals(100m, consignmentItem.CCI_Calc_ReportGrossMass);
				AssertEquals(50m, consignmentItem.CCI_Calc_ReportNetMass);

				consignmentItem.CCI_Calc_ReportGrossMass = 50m;
				AssertEquals(25m, consignmentItem.CCI_Calc_ReportNetMass);
			}
		}

		public void TestCCI_Calc_ShouldReportItem()
		{
			AssertEquals(false, consignmentItem.CCI_Calc_ShouldReportItem);

			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			using (consignmentItem.SetupReportItemData(report))
			{
				AssertEquals(true, consignmentItem.CCI_Calc_ShouldReportItem);
			}
		}

		public void TestCCI_Calc_ShouldReportItem_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentItem.CCI_Calc_ShouldReportItemInfo);
			AssertEquals("Select", captionResourceString.Caption);
		}

		public void TestGetMatchingExitReportItems()
		{
			AssertEquals(Enumerable.Empty<CusExitReportItem>(), consignmentItem.GetMatchingExitReportItems());

			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem1 = report.CusExitReportItems.AddNew();
			reportItem1.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			var reportItem2 = report.CusExitReportItems.AddNew();
			var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
			reportItem2.ERI_CCI_ConsignmentItem = consignmentItem2.PK;
			var reportItem3 = report.CusExitReportItems.AddNew();
			reportItem3.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			using (consignmentItem.SetupReportItemData(report))
			{
				AssertContainsExactElementsInAnyOrder(new[] { reportItem1.PK, reportItem3.PK }, consignmentItem.GetMatchingExitReportItems().Select(x => x.PK));
			}
		}

		public void TestFirstMatchingReportItem()
		{
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_OfficeOfExit = "DE001";
			var reportItem1 = report.CusExitReportItems.AddNew();
			reportItem1.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			Factory.Save();
			var reportItem2 = report.CusExitReportItems.AddNew();
			reportItem2.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			Factory.Save();

			using (consignmentItem.SetupReportItemData(report))
			{
				CombineAssertions(() =>
				{
					AssertEquals("FirstReportItem should be reportItem1", reportItem1, consignmentItem.FirstMatchingReportItem);

					reportItem1.Delete();
					AssertEquals("FirstReportItem should be reportItem2", reportItem2, consignmentItem.FirstMatchingReportItem);
				});
			}
		}

		public void TestRefreshReportItemData()
		{
			consignmentItem.CCI_Calc_ReportGrossMass = 100m;
			consignmentItem.CCI_Calc_ReportNetMass = 50m;
			consignmentItem.CCI_Calc_ShouldReportItem = false;
			consignmentItem.CCI_Calc_ReportGrossMassInfo.ValueChanged += (s, e) =>
			{
				AssertEquals("CCI_Calc_ReportGrossMass old value", 100m, ((ValueChangedEventArgs)e).OldValue);
			};

			consignmentItem.CCI_Calc_ReportNetMassInfo.ValueChanged += (s, e) =>
			{
				AssertEquals("CCI_Calc_ReportNetMass old value", 50m, ((ValueChangedEventArgs)e).OldValue);
			};

			consignmentItem.CCI_Calc_ShouldReportItemInfo.ValueChanged += (s, e) =>
			{
				AssertEquals("CCI_Calc_ShouldReportItem old value", false, ((ValueChangedEventArgs)e).OldValue);
			};

			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			reportItem.ERI_GrossMass = 270m;
			reportItem.ERI_NetMass = 210m;
			using (consignmentItem.SetupReportItemData(report))
			{
				CombineAssertions(() =>
				{
					AssertEquals(true, consignmentItem.CCI_Calc_ShouldReportItem);
					AssertEquals(270m, consignmentItem.CCI_Calc_ReportGrossMass);
					AssertEquals(210m, consignmentItem.CCI_Calc_ReportNetMass);
				});
			}
		}

		public void TestDelete()
		{
			var exitConsignmentPivot = consignmentItem.CusExitConsignmentPivots.AddNew();
			var exitConsignmentPackagePivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var exitConsignmentContainerPivot = consignmentItem.CusExitConsignmentContainerPivots.AddNew();

			consignmentItem.Delete();
			CombineAssertions(() =>
			{
				AssertEquals("exitConsignmentPivot", true, exitConsignmentPivot.IsDeleted);
				AssertEquals("exitConsignmentPackagePivot", true, exitConsignmentPackagePivot.IsDeleted);
				AssertEquals("exitConsignmentContainerPivot", true, exitConsignmentContainerPivot.IsDeleted);
			});
		}

		public void TestCCI_UniqueConsignmentReferenceStatus_ResourceStringData()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(consignmentItem.CCI_UniqueConsignmentReferenceStatusInfo, null);
			AssertNotNull(captionResourceString);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "UCR Status", captionResourceString.Caption);
				AssertEquals("Short Caption", "UCR Status", captionResourceString.ShortCaption);
			});
		}

		public void TestCCI_DiscrepancyStatus_ResourceStringData()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(consignmentItem.CCI_DiscrepancyStatusInfo, null);
			AssertNotNull(captionResourceString);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Status", captionResourceString.Caption);
				AssertEquals("Short Caption", "Status", captionResourceString.ShortCaption);
				AssertEquals("Medium Caption", "Status", captionResourceString.MediumCaption);
				AssertEquals("Full Description", "Consignment Item Status", captionResourceString.FullDescription);
			});
		}

		public void TestIsUCC6() => CombineAssertions(() =>
		{
			var objectForTest = GetNewBusinessObject() as CusExitConsignmentItem;
			AssertEquals("Not UCC6", objectForTest.IsUCC6, false);

			AssertEquals("No Parent = not UCC6", Factory.New<CusExitConsignmentItem>().IsUCC6, false);

			var ucc6ObjectForTest = Factory.GetUcc6ExitConsignment().CusExitConsignmentItems.AddNew();
			AssertEquals("UCC6 taken from parent", ucc6ObjectForTest.IsUCC6, true);
		});

		public void TestStatusIsMissing() => CombineAssertions(() =>
		{
			var consignment = GetNewBusinessObject() as CusExitConsignmentItem;
			AssertEquals("empty", consignment.StatusIsMissing, false);

			consignment.CCI_DiscrepancyStatus = DiscrepanciesStatusCodeList.Codes.Missing;
			AssertEquals("Missing", consignment.StatusIsMissing, true);

			consignment.CCI_DiscrepancyStatus = DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			AssertEquals("DifferencesToDeclared", consignment.StatusIsMissing, false);
		});

		public void TestStatusIsDifferencesToDeclared() => CombineAssertions(() =>
		{
			var consignment = GetNewBusinessObject() as CusExitConsignmentItem;
			AssertEquals("empty", consignment.StatusIsDifferencesToDeclared, false);

			consignment.CCI_DiscrepancyStatus = DiscrepanciesStatusCodeList.Codes.Missing;
			AssertEquals("Missing", consignment.StatusIsDifferencesToDeclared, false);

			consignment.CCI_DiscrepancyStatus = DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			AssertEquals("DifferencesToDeclared", consignment.StatusIsDifferencesToDeclared, true);
		});

		public void UCRStatusIsDifferencesToDeclared() => CombineAssertions(() =>
		{
			var consignment = GetNewBusinessObject() as CusExitConsignmentItem;
			AssertEquals("empty", consignment.UCRStatusIsDifferencesToDeclared, false);

			consignment.CCI_UniqueConsignmentReferenceStatus = DiscrepanciesStatusCodeList.Codes.Missing;
			AssertEquals("Missing", consignment.UCRStatusIsDifferencesToDeclared, false);

			consignment.CCI_UniqueConsignmentReferenceStatus = DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			AssertEquals("DifferencesToDeclared", consignment.UCRStatusIsDifferencesToDeclared, true);
		});

		public void TestAdditionalInfos()
		{
			var item = GetNewBusinessObject() as CusExitConsignmentItem;
			AssertType<AdditionalInfoCollection<AdditionalInfo>>(item.AdditionalInfos);
		}

		public void TestGetCusSupportingInfoTypes_AdditionalInfo()
		{
			var item = GetNewBusinessObject() as CusExitConsignmentItem;
			var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)item).GetCusSupportingInfoTypes();
			AssertEquals(typeof(AdditionalInfo), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		}

		public void TestValidation() => CombineAssertions(() =>
		{
			var item = GetNewBusinessObject() as CusExitConsignmentItem;
			AssertType<CusExitConsignmentItemValidation>(item.Validation);
		});

		public void TestLookups() => CombineAssertions(() =>
		{
			var item = GetNewBusinessObject() as CusExitConsignmentItem;
			AssertType<CusExitConsignmentItemLookups>("not ucc6", item.Lookups);

			item = Factory.GetUcc6ExitConsignment().CusExitConsignmentItems.AddNew();
			AssertType<CusExitConsignmentItemUcc6Lookups>("ucc6", item.Lookups);
		});

		public void TestClearStatusAndSetSealsReadOnlyIfStatusIsMissing() => CombineAssertions(() =>
		{
			var header = Factory.GetUcc6ExitHeader();
			var item = header.CusExitConsignments.AddNew().CusExitConsignmentItems.AddNew();
			item.CCI_LineNumber = 1;
			var addDoc = item.AdditionalInfos.AddNew();
			addDoc.CSI_Status = "MIS";
			var package = item.CusExitConsignmentPackagePivots.AddNew().Package;
			package.CXP_MarksAndNumbers = "DIF";

			AssertEquals("addDoc is not ReadOnly", false, addDoc.ReadOnly);
			AssertEquals("package is not ReadOnly", false, package.ReadOnly);

			item.CCI_DiscrepancyStatus = "MIS";
			AssertEquals("addDoc is ReadOnly", true, addDoc.ReadOnly);
			AssertEquals("package is ReadOnly", true, package.ReadOnly);
			AssertEquals("addDoc Status is empty", ZString.Empty, addDoc.CSI_Status);
			AssertEquals("package Status is empty", ZString.Empty, package.CXP_MarksAndNumbersStatus);

			item.CCI_DiscrepancyStatus = "DIF";
			AssertEquals("addDoc is not ReadOnly", false, addDoc.ReadOnly);
			AssertEquals("package is not ReadOnly", false, package.ReadOnly);

			item.CCI_DiscrepancyStatus = "MIS";
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var headerReloaded = newFactory.LoadUcc6ExitHeader(header);
			var itemReloaded = headerReloaded.CusExitConsignments[0].CusExitConsignmentItems[0];
			var addDocReloaded = itemReloaded.AdditionalInfos[0];
			var packageReloaded = itemReloaded.CusExitConsignmentPackagePivots[0].Package;
			AssertEquals("addDocReloaded is ReadOnly", true, addDocReloaded.ReadOnly);
			AssertEquals("packageReloaded is ReadOnly", true, packageReloaded.ReadOnly);
		});

		public void TestCusExitConsignmentPackagePivotsUcc6() => CombineAssertions(() =>
		{
			var item = Factory.GetUcc6ExitConsignment().CusExitConsignmentItems.AddNew();
			var cusExitConsignmentPackagePivots = item.CusExitConsignmentPackagePivots;
			AssertType<ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(cusExitConsignmentPackagePivots);
			AssertEquals("SupportsSorting", true, cusExitConsignmentPackagePivots.SupportsSorting);
			AssertEquals("SortDirection", System.ComponentModel.ListSortDirection.Ascending, cusExitConsignmentPackagePivots.SortDirection);
			AssertEquals("SortInformation.PropertyName", nameof(CusExitConsignmentPivot.SequenceNumber), cusExitConsignmentPackagePivots.SortInformation.PropertyName);
		});

		public void TestMaxPivotItemCountCore()
		{
			AssertEquals("Max Pivot Item Count", 99, consignmentItem.MaxPivotItemCount);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(Factory).consignmentItem;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => consignmentItem;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).consignmentItem;

		protected override void SetUp()
		{
			base.SetUp();
			(consignmentItem, consignment, header) = GetNewBusinessObject(Factory);
		}
		CusExitConsignment consignment;
		CusExitConsignmentItem consignmentItem;
		CusExitHeader header;

		public static (CusExitConsignmentItem consignmentItem, CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			(var consignment, var header) = CusExitConsignmentTest.GetNewBusinessObject(factory);
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;
			return (consignmentItem, consignment, header);
		}
	}
}
