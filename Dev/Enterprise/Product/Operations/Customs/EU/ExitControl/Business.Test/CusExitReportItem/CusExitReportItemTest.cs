using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitReportItem))]
	sealed class CusExitReportItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertType<CusExitReportItemValidation>(reportItem.Validation);
		}

		public void TestLookups()
		{
			AssertType<CusExitReportItemLookups>(reportItem.Lookups);
		}

		public void TestReport()
		{
			AssertType<CusExitReport>(reportItem.Report);
		}

		public void TestConsignmentItem()
		{
			AssertType<CusExitConsignmentItem>(reportItem.ConsignmentItem);
		}

		public void TestPackage()
		{
			(var reportItem, _, _) = GetNewBusinessObject(Factory);
			var package = Factory.New<CusExitConsignmentPackage>();
			reportItem.ERI_CXP_Package = package.PK;
			AssertEquals(package.PK, reportItem.Package.PK);
			AssertType<CusExitConsignmentPackage>(reportItem.Package);
		}

		public void TestAdditionalInfos()
		{
			(var item, _, _) = GetNewBusinessObject(Factory);
			var additionalInfo1 = item.AdditionalInfos.AddNew();
			var additionalInfo2 = item.AdditionalInfos.AddNew();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			item = newFactory.Load<CusExitReportItem>(item.PK);
			var additionalInfos = item.AdditionalInfos;
			AssertType<AdditionalInfoCollection<AdditionalInfo>>(additionalInfos);
			AssertEquals("additionalInfos.Count", 2, additionalInfos.Count);
			var additionalInfo1InDiffFactory = additionalInfos[0];
			var additionalInfo2InDiffFactory = additionalInfos[1];
			if (additionalInfo2InDiffFactory.PK == additionalInfo1.PK)
			{
				additionalInfo1InDiffFactory = additionalInfos[1];
				additionalInfo2InDiffFactory = additionalInfos[0];
			}
			AssertEquals(additionalInfo1.PK, additionalInfo1InDiffFactory.PK);
			AssertEquals(additionalInfo2.PK, additionalInfo2InDiffFactory.PK);
			item.Delete();
			AssertEquals("additionalInfo1InDiffFactory.IsDeleted", true, additionalInfo1InDiffFactory.IsDeleted);
			AssertEquals("additionalInfo2InDiffFactory.IsDeleted", true, additionalInfo2InDiffFactory.IsDeleted);
		}

		public void TestCusAuthorizationUsages() => CombineAssertions(() =>
		{
			(var item, _, _) = GetNewBusinessObject(Factory);
			var initialUsages = item.CusAuthorizationUsages;
			AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitReportItem>>("Type of CusAuthorizationUsageCollection.", initialUsages);

			AddNewAuthorization("123", "321");
			AddNewAuthorization("456", "654");
			Factory.Save();
			AssertEquals("Save authorization usages.", 2, initialUsages.Count);

			var newFactory = new BusinessObjectFactory();
			var loadedUsages = newFactory.Load<CusExitReportItem>(item.PK).CusAuthorizationUsages;
			AssertEquals("Number of loaded authorization usages equals to saved.", 2, loadedUsages.Count);
			Assert("Loaded items are equal to saved.", initialUsages.Any(x => loadedUsages.Any(y => x.PK == y.PK && x.AGC_Code == y.AGC_Code && x.AGC_Number == y.AGC_Number)));

			loadedUsages.Delete();
			Assert("All authorization usages are marked as deleted.", loadedUsages.All(x => x.IsDeleted));

			void AddNewAuthorization(string code, string number)
			{
				var authorizationUsage = initialUsages.AddNew();
				authorizationUsage.AGC_Code = code;
				authorizationUsage.AGC_Number = number;
			}
		});

		public void TestERI_GrossMass_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(reportItem.ERI_GrossMassInfo);
			AssertEquals("Gross Mass KG", resourceStringData.Caption);
		}

		public void TestERI_NetMass_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(reportItem.ERI_NetMassInfo);
			AssertEquals("Net Mass KG", resourceStringData.Caption);
		}

		public void TestPackageType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Package", ZString.Empty, reportItem.PackageType);

				var package = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
				package.Package.CXP_PackageType = "XX";
				reportItem.ERI_CXP_Package = package.Package.PK;
				AssertEquals("Has Package", "XX", reportItem.PackageType);
			});
		}

		public void TestPackageType_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitReportItem), nameof(reportItem.PackageType));
			AssertEquals("Pack Type", resourceStringData.Caption);
		}

		public void TestPackageSequenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Seq. No.", ZShort.Zero, reportItem.SeqNo);

				var package = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
				package.Package.CXP_Sequence = 5;
				reportItem.ERI_CXP_Package = package.Package.PK;
				AssertEquals("Has SeqNo", (short)5, reportItem.SeqNo);
			});
		}

		public void TestPackageSequenceNumber_ResourceStringData()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitReportItem), nameof(reportItem.SeqNo));
			AssertEquals("Sequence Number", resourceStringData.Caption);
			AssertEquals("Seq. No.", resourceStringData.ShortCaption);
			AssertEquals("Seq. Number", resourceStringData.MediumCaption);
			AssertEquals("Package Sequence Number", resourceStringData.FullDescription);
		}

		public void TestPackageMarksAndNumbers()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Package", ZString.Empty, reportItem.PackageMarksAndNumbers);

				var package = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
				package.Package.CXP_MarksAndNumbers = "XX";
				reportItem.ERI_CXP_Package = package.Package.PK;
				AssertEquals("Has Package", "XX", reportItem.PackageMarksAndNumbers);
			});
		}

		public void TestPackageMarksAndNumbers_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitReportItem), nameof(reportItem.PackageMarksAndNumbers));
			AssertEquals("Marks & Numbers", resourceStringData.Caption);
		}

		public void TestPackageContainerOrEquipment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Package Container Or Equipment", ZString.Empty, reportItem.PackageContainerOrEquipment);

				var package = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
				var container = Factory.New<CusExitContainer>();
				package.CNP_CXN_Container = container.PK;
				container.CXN_ContainerNumber = "XYZ123";
				reportItem.ERI_CXP_Package = package.Package.PK;
				AssertEquals("Has Package Container Or Equipment", "XYZ123", reportItem.PackageContainerOrEquipment);
			});
		}

		public void TestPackageContainerOrEquipment_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitReportItem), nameof(reportItem.PackageContainerOrEquipment));
			AssertEquals("Container/Equipment", resourceStringData.Caption);
		}

		public void TestConsignmentItemLineNumber_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitReportItem), nameof(reportItem.ConsignmentItemLineNumber));
			AssertEquals("Item No.", resourceStringData.Caption);
		}

		public void TestConsignmentItemUniqueConsignmentReference_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitReportItem), nameof(reportItem.ConsignmentItemUniqueConsignmentReference));
			AssertEquals("Reference Number UCR", resourceStringData.Caption);
		}

		public void TestTypeDecider()
		{
			AssertType<CusExitReportItemTypeDecider>(CusExitReportItem.TypeDecider);
		}

		public void TestOnSaving()
		{
			var packagePivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			reportItem.ERI_CXP_Package = packagePivot.CNP_CXP_Package;

			var packagePivot2 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var reportItem2 = report.CusExitReportItems.AddNew();
			reportItem2.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			reportItem2.ERI_CXP_Package = packagePivot2.CNP_CXP_Package;

			var consignmentItem2 = reportItem.ConsignmentItem.Consignment.CusExitConsignmentItems.AddNew();
			var reportItem3 = report.CusExitReportItems.AddNew();
			reportItem3.ERI_CCI_ConsignmentItem = consignmentItem2.PK;

			var reportItemForBinding = report.CusExitReportItemsForBinding.Single(x => x.ERI_CCI_ConsignmentItem == consignmentItem.PK);
			var reportItemNotForBinding = report.CusExitReportItems.Single(x => x.ERI_CCI_ConsignmentItem == consignmentItem.PK && x.PK != reportItemForBinding.PK);

			CombineAssertions(() =>
			{
				reportItemForBinding.ERI_GrossMass = 1.1M;
				reportItemForBinding.ERI_NetMass = 2.2M;
				reportItemForBinding.OnSaving();
				AssertEquals("Same ConsignmentItem, ERI_GrossMass", 1.1M, reportItemNotForBinding.ERI_GrossMass);
				AssertEquals("Same ConsignmentItem, ERI_NetMass", 2.2M, reportItemNotForBinding.ERI_NetMass);
				AssertEquals("Different ConsignmentItem, ERI_GrossMass", ZDecimal.Zero, reportItem3.ERI_GrossMass);
				AssertEquals("Different ConsignmentItem, ERI_NetMass", ZDecimal.Zero, reportItem3.ERI_NetMass);

				reportItemForBinding.ERI_GrossMass = 3.3M;
				reportItemNotForBinding.OnSaving();
				AssertEquals("Will not update when reportItemNotForBinding(Package) is saving", 3.3M, reportItemForBinding.ERI_GrossMass);
			});
		}

		public void TestIsUCC6() => CombineAssertions(() =>
		{
			var objectForTest = GetNewBusinessObject() as CusExitReportItem;
			AssertEquals("Not UCC6", objectForTest.IsUCC6, false);

			AssertEquals("No Parent = not UCC6", Factory.New<CusExitReportItem>().IsUCC6, false);

			var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitReports.AddNew().CusExitReportItems.AddNew();
			AssertEquals("UCC6 taken from parent", ucc6ObjectForTest.IsUCC6, true);
		});

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(Factory).Item;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => reportItem;

		protected override void SetUp()
		{
			base.SetUp();
			(reportItem, report, consignmentItem) = GetNewBusinessObject(Factory);
		}

		CusExitReport report;
		CusExitReportItem reportItem;
		CusExitConsignmentItem consignmentItem;

		public static (CusExitReportItem Item, CusExitReport Report, CusExitConsignmentItem ExitConsignmentItem) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<CusExitHeader>();
			header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
			var consignment = header.CusExitConsignments.AddNew();
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_DateTime = ZDateTimeOffset.Now;
			report.CER_OfficeOfExit = "DE001";
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			return (reportItem, report, consignmentItem);
		}
	}
}
