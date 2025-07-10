using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignmentPackage))]
	class CusExitConsignmentPackageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCusExitReportItems()
		{
			(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
			AssertType<ExitControlBase.Business.CusExitReportItemCollection<CusExitReportItem>>(consignmentPackage.CusExitReportItems);
		}

		public void TestHeader()
		{
			(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
			AssertType<CusExitHeader>(consignmentPackage.Header);
		}

		public void TestCXP_Sequence_Caption()
		{
			(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentPackage.CXP_SequenceInfo);
			AssertEquals("Seq No.", captionResourceString.Caption);
		}

		public void TestCXP_Sequence_ReadOnly()
		{
			(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
			AssertEquals(true, consignmentPackage.CXP_SequenceInfo.ReadOnly);
		}

		public void TestCXP_Quantity_Caption()
		{
			(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentPackage.CXP_QuantityInfo);
			AssertEquals("Pack Qty", captionResourceString.Caption);
		}

		public void TestCXP_PackageType_Caption()
		{
			(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentPackage.CXP_PackageTypeInfo);
			AssertEquals("Pack Type", captionResourceString.Caption);
		}

		public void TestCXP_MarksAndNumbers_Caption()
		{
			(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentPackage.CXP_MarksAndNumbersInfo);
			AssertEquals("Marks & Numbers", captionResourceString.Caption);
		}

		public void TestTypeDecider()
		{
			AssertType<CusExitConsignmentPackageTypeDecider>(CusExitConsignmentPackage.TypeDecider);
		}

		public void TestCusExitConsignmentPivot()
		{
			(var consignmentPackage, var pivot1, _, _, _) = GetNewBusinessObject(Factory);
			AssertSame("Should return the corrent CusExitConsignmentPivot as ConsignmentPivot.", pivot1, consignmentPackage.ConsignmentPivot);
		}

		public void TestCXP_Calc_ReportQuantity()
		{
			(var consignmentPackage, _, _, var consignment, var header) = GetNewBusinessObject(Factory);
			consignmentPackage.CXP_Quantity = 100;
			AssertEquals("Get the value from CXP_Quantity", 100, consignmentPackage.CXP_Calc_ReportQuantity);

			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem = consignmentPackage.CusExitReportItems.AddNew();
			reportItem.ERI_CER_Report = report.PK;
			reportItem.ERI_Quantity = 200;
			using (consignmentPackage.SetupReportItemData(report.PK))
			{
				AssertEquals("Get the value from the first ReportItem.ERI_Quantity", 200, consignmentPackage.CXP_Calc_ReportQuantity);
			}
			AssertEquals("Get the value from CXP_Quantity when not report item data", 100, consignmentPackage.CXP_Calc_ReportQuantity);
		}

		public void TestCXP_Calc_ReportQuantity_Caption()
		{
			(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentPackage.CXP_Calc_ReportQuantityInfo);
			AssertEquals("Pack Qty", captionResourceString.Caption);
		}

		public void TestCXP_Calc_ShouldReportItem()
		{
			(var consignmentPackage, _, _, var consignment, var header) = GetNewBusinessObject(Factory);
			AssertEquals(false, consignmentPackage.CXP_Calc_ShouldReportItem);

			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem = consignmentPackage.CusExitReportItems.AddNew();
			reportItem.ERI_CER_Report = report.PK;
			using (consignmentPackage.SetupReportItemData(report.PK))
			{
				AssertEquals(true, consignmentPackage.CXP_Calc_ShouldReportItem);
			}
		}

		public void TestCXP_Calc_ShouldReportItemCaption()
		{
			(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(consignmentPackage.CXP_Calc_ShouldReportItemInfo);
			AssertEquals("Select", captionResourceString.Caption);
		}

		[TestDate(2022, 12, 16)]
		[TestDateIncremental(milliseconds: 10)]
		public void TestFirstReportItem()
		{
			(var consignmentPackage, _, _, var consignment, var header) = GetNewBusinessObject(Factory);

			var report = header.CusExitReports.AddNew();
			report.CER_OfficeOfExit = "DE001";
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem1 = consignmentPackage.CusExitReportItems.AddNew();
			reportItem1.ERI_CER_Report = report.PK;
			Factory.Save();

			var reportItem2 = consignmentPackage.CusExitReportItems.AddNew();
			reportItem2.ERI_CER_Report = report.PK;
			Factory.Save();

			using (consignmentPackage.SetupReportItemData(report.PK))
			{
				AssertEquals("FirstReportItem should be reportItem1", reportItem1, consignmentPackage.FirstReportItem);

				reportItem1.Delete();
				AssertEquals("FirstReportItem should be reportItem2", reportItem2, consignmentPackage.FirstReportItem);
			}
		}

		public void TestDelete()
		{
			(var consignmentPackage, _, _, var consignment, var header) = GetNewBusinessObject(Factory);
			var report = header.CusExitReports.AddNew();
			report.CER_OfficeOfExit = "OFE";
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem1 = consignmentPackage.CusExitReportItems.AddNew();
			reportItem1.ERI_CER_Report = report.PK;
			Factory.Save();

			consignmentPackage.Delete();
			AssertEquals("Should delete CusExitReportItems when deleting CusExitConsignmentPackage.", true, reportItem1.IsDeleted);
		}

		public void TestGetMatchingExitReportItems()
		{
			(var consignmentPackage, _, _, var consignment, var header) = GetNewBusinessObject(Factory);

			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem1 = consignmentPackage.CusExitReportItems.AddNew();
			reportItem1.ERI_CER_Report = report.PK;
			var reportItem2 = consignmentPackage.CusExitReportItems.AddNew();
			reportItem2.ERI_CER_Report = report.PK;
			using (consignmentPackage.SetupReportItemData(report.PK))
			{
				AssertContainsExactElementsInAnyOrder(new[] { reportItem1.PK, reportItem2.PK }, consignmentPackage.GetMatchingExitReportItems().Select(x => x.PK));
			}
		}

		public void TestSetupReportItemData()
		{
			(var consignmentPackage, _, _, var consignment, var header) = GetNewBusinessObject(Factory);
			consignmentPackage.CXP_Calc_ReportQuantity = 50;
			consignmentPackage.CXP_Calc_ShouldReportItem = false;
			consignmentPackage.CXP_Calc_ReportQuantityInfo.ValueChanged += (s, e) =>
			{
				AssertEquals("CXP_Calc_ReportQuantity old value", 50, ((ValueChangedEventArgs)e).OldValue);
			};

			consignmentPackage.CXP_Calc_ShouldReportItemInfo.ValueChanged += (s, e) =>
			{
				AssertEquals("CXP_Calc_ShouldReportItem old value", false, ((ValueChangedEventArgs)e).OldValue);
			};

			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem = consignmentPackage.CusExitReportItems.AddNew();
			reportItem.ERI_CER_Report = report.PK;
			reportItem.ERI_Quantity = 23;
			using (consignmentPackage.SetupReportItemData(report.PK))
			{
				CombineAssertions(() =>
				{
					AssertEquals(true, consignmentPackage.CXP_Calc_ShouldReportItem);
					AssertEquals(23, consignmentPackage.CXP_Calc_ReportQuantity);
				});
			}
		}

		public void TestIsBulkPackageType()
		{
			(var consignmentPackage, _, _, var consignment, var header) = GetNewBusinessObject(Factory);
			Factory.SetupBulkCusCode();
			consignmentPackage.CXP_PackageType = "VG";
			AssertEquals("Bulk Code", true, consignmentPackage.IsBulk);
			consignmentPackage.CXP_PackageType = "NE";
			AssertEquals("BreakBulk Code", false, consignmentPackage.IsBulk);
		}

		public void TestIsBreakBulkPackageType()
		{
			(var consignmentPackage, _, _, var consignment, var header) = GetNewBusinessObject(Factory);
			Factory.SetupBreakBulkCusCode();
			consignmentPackage.CXP_PackageType = "VG";
			AssertEquals("Bulk Code", false, consignmentPackage.IsBreakBulk);
			consignmentPackage.CXP_PackageType = "NE";
			AssertEquals("BreakBulk Code", true, consignmentPackage.IsBreakBulk);
		}

		public void TestCXP_MarksAndNumbersStatus_ResourceStringData()
		{
			(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
			var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(consignmentPackage.CXP_MarksAndNumbersStatusInfo, null);
			AssertNotNull(resourceStringData);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Status", resourceStringData.Caption);
				AssertEquals("Short Caption", "Status", resourceStringData.ShortCaption);
				AssertEquals("Medium Caption", "Status", resourceStringData.MediumCaption);
				AssertEquals("Full Description", "Consignment Package Status", resourceStringData.FullDescription);
			});
		}

		public void TestIsUCC6() => CombineAssertions(() =>
		{
			var objectForTest = GetNewBusinessObject() as CusExitConsignmentPackage;
			AssertEquals("Not UCC6", objectForTest.IsUCC6, false);

			AssertEquals("No Parent = not UCC6", Factory.New<CusExitConsignmentPackage>().IsUCC6, false);

			var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitConsignmentPackages.AddNew();
			AssertEquals("UCC6 taken from parent", ucc6ObjectForTest.IsUCC6, true);
		});

		public void TestValidation() => CombineAssertions(() =>
		{
			var objectForTest = GetNewBusinessObject() as CusExitConsignmentPackage;
			AssertType<CusExitConsignmentPackageValidation>("not ucc6", objectForTest.Validation);

			var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitConsignmentPackages.AddNew();
			AssertType<CusExitConsignmentPackageValidation>("ucc6 - Validation Decider should be different, not validation class", ucc6ObjectForTest.Validation);
		});

		public void TestValidationDecider() => CombineAssertions(() =>
		{
			var item = GetNewBusinessObject() as CusExitConsignmentPackage;
			AssertNull("not ucc6", item.ValidationDecider);

			item = Factory.GetUcc6ExitHeader().CusExitConsignmentPackages.AddNew();
			AssertType<CusExitConsignmentPackageUcc6ValidationDecider>("ucc6", item.ValidationDecider);
		});

		public void TestLookups() => CombineAssertions(() =>
		{
			var objectForTest = GetNewBusinessObject() as CusExitConsignmentPackage;
			AssertType<CusExitConsignmentPackageLookups>("not ucc6", objectForTest.Lookups);

			var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitConsignmentPackages.AddNew();
			AssertType<CusExitConsignmentPackageUcc6Lookups>("not ucc6", ucc6ObjectForTest.Lookups);
		});

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).package;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		public static (CusExitConsignmentPackage package, CusExitConsignmentPivot pivot, CusExitConsignmentItem consignmentItem, CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			(var pivot, var consignmentItem, var consignment, var header) = CusExitConsignmentPivotTest.GetNewBusinessObject(factory);
			return (pivot.Package, pivot, consignmentItem, consignment, header);
		}
	}
}
