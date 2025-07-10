using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.DataImport.Testing
{
	internal class Level1DataFileImporterTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
		}

		public void TestSplitShipmentsWithWaybillShortNumber()
		{
			// Load and save AU9639TQ.273 on MasterBill 08144444444.
			Level1DataImport importBizo = TestHelper.GetNewLevel1DataImport("QF098", new ZDateTime(2008, 9, 24),
				"INDEL", "AUSYD", "08144444444",
				Path.Combine(UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "AU9639TQ.273.txt")));
			Level1DataFileImporterForAU importer = TestHelper.GetNewLevel1DataImporterForAU(importBizo);
			importer.LoadFile();
			importer.Save();

			UPECusHAWB houseBill;
			Assert("HouseBill 2XY024HWDMR exists", IsHawb("2XY024HWDMR", out houseBill));
			Assert("No split shipments for HouseBill 2XY024HWDMR", !houseBill.IsSplitShipment);

			// Load and save AU9639TR.273 on MasterBill 08144444444.
			importBizo = TestHelper.GetNewLevel1DataImport("QF098", new ZDateTime(2008, 9, 24),
							"INDEL", "AUSYD", "08144444444",
							Path.Combine(UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "AU9639TR.273.txt")));
			importer = TestHelper.GetNewLevel1DataImporterForAU(importBizo);
			importer.LoadFile();  // ensure bizo has new filename within the importer.
			importer.Save();

			Assert("HouseBill 2XY024HWDMR exists", IsHawb("2XY024HWDMR", out houseBill));
			AssertEquals("Duplicate HouseBill 2XY024HWDMR exists", "08144444444", importer.DuplicateShipmentDict.GetValueSafe("2XY024HWDMR").MasterBill);
			Assert("HouseBill 2XY024HWDMR is a parent shipment", IsShipmentParent(houseBill));
			Assert("HouseBill 2XY024HWDMR has a child shipment 1Z2XY0240451743670", IsShipmentChild(houseBill, "1Z2XY0240451743670"));
			Assert("Shipment 1Z2XY0240451743670 is virtual", !IsHawb("1Z2XY0240451743670"));
			Assert("HouseBill 2XY024HWDMR doesn't have a child shipment 1Z2XY0240449615465 yet", !IsShipmentChild(houseBill, "1Z2XY0240449615465"));

			// Load and save AU9639TV.272 on MasterBill 08155555555.
			importBizo = TestHelper.GetNewLevel1DataImport("QF098", new ZDateTime(2008, 9, 24),
							"INDEL", "AUSYD", "08155555555",
							Path.Combine(UPETestHelper.TestResource.ExtractToFile(UPETestHelper.TestResource.DataImport.Path, TempDir.DirectoryName, "AU9639TV.272.txt")));
			importer = TestHelper.GetNewLevel1DataImporterForAU(importBizo);
			importer.LoadFile();
			importer.Save();

			Assert("HouseBill 2XY024HWDMR exists", IsHawb("2XY024HWDMR", out houseBill));
			Assert("HouseBill 2XY024HWDMR is still a parent shipment", IsShipmentParent(houseBill));
			Assert("HouseBill 2XY024HWDMR still has a child shipment 1Z2XY0240451743670", IsShipmentChild(houseBill, "1Z2XY0240451743670"));
			Assert("Shipment 1Z2XY0240451743670 is still virtual", !IsHawb("1Z2XY0240451743670"));
			Assert("2 duplicate house bills", importer.DuplicateShipmentDict.Count == 2);
		}

		bool IsHawb(string houseBillNumber)
		{
			UPECusHAWB houseBill;
			return IsHawb(houseBillNumber, out houseBill);
		}

		bool IsHawb(string houseBillNumber, out UPECusHAWB houseBill)
		{
			ZQuery hawbFilter = new ZQuery(CusHAWBSchema.CS_HAWB, houseBillNumber);
			houseBill = Factory.LoadTop1<UPECusHAWB>(hawbFilter);
			return houseBill != null;
		}

		bool IsShipmentParent(UPECusHAWB houseBill)
		{
			ZQuery jobRelatedFilter = new ZQuery(JobRelatedWayBillSchema.EB_ParentID, houseBill.PK);
			jobRelatedFilter.AddToFilter(JoinCondition.And, JobRelatedWayBillSchema.EB_WaybillNumber, houseBill.CS_HAWB);
			jobRelatedFilter.AddToFilter(JoinCondition.And, JobRelatedWayBillSchema.EB_WaybillType, JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			return Factory.GetDatabaseCount(typeof(JobRelatedWayBill), jobRelatedFilter) == 1;
		}

		bool IsShipmentChild(UPECusHAWB parentHouseBill, string childPackageNumber)
		{
			ZQuery jobRelatedFilter = new ZQuery(JobRelatedWayBillSchema.EB_ParentID, parentHouseBill.PK);
			jobRelatedFilter.AddToFilter(JoinCondition.And, JobRelatedWayBillSchema.EB_WaybillNumber, childPackageNumber);
			jobRelatedFilter.AddToFilter(JoinCondition.And, JobRelatedWayBillSchema.EB_WaybillType, JobRelatedWayBill.Constants.RelatedWayBillType.Child);
			return Factory.GetDatabaseCount(typeof(JobRelatedWayBill), jobRelatedFilter) == 1;
		}

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper(Factory)); }
		}
		UPETestHelper testHelper;

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		protected override void TearDown()
		{
			base.TearDown();
			tempDir?.Dispose();
		}
	}
}
