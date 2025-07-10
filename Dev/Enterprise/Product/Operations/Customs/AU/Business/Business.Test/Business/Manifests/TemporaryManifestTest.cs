using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(TemporaryManifest))]
	internal sealed class TemporaryManifestTest : NonPersistentBusinessObjectTestCase
	{
		public void TestKeep()
		{
			temporaryManifest.ShouldSave = true;
			AssertEquals(true, temporaryManifest.ShouldSave);

			temporaryManifest.ShouldSave = false;
			AssertEquals(false, temporaryManifest.ShouldSave);

			AssertNotNull(temporaryManifest.ShouldSaveInfo);
			AssertEquals(true, temporaryManifest.ShouldSaveInfo.GetType().IsSubclassOf(typeof(ZPropertyInfo)));
			AssertEquals(false, temporaryManifest.ShouldSaveInfo.ReadOnly);

			temporaryManifest.IsDuplicated = true;
			AssertEquals(true, temporaryManifest.IsDuplicated);

			temporaryManifest.IsDuplicated = false;
			AssertEquals(false, temporaryManifest.IsDuplicated);
		}

		public void TestTemporaryManifestForExport()
		{
			AssertEquals(calcManifest, temporaryManifest.CalcExportManifest);
			AssertEquals(true, temporaryManifest.CalcExportManifest.ReadOnly);
		}

		public void TestTemporaryManifestForImport()
		{
			CusSeaManTranHead importManifest = Factory.New<CusSeaManTranHead>();
			TemporaryManifest temporaryImportManifest = new TemporaryManifest(Factory, importManifest);

			AssertEquals(typeof(CusSeaManTranHead), temporaryImportManifest.ImportManifest.GetType());
			AssertEquals(importManifest, temporaryImportManifest.ImportManifest);
			AssertEquals(true, temporaryImportManifest.ImportManifest.ReadOnly);
		}

		public void TestKeepTrueByDefault()
		{
			AssertEquals(true, temporaryManifest.ShouldSave);
		}

		public void TestShouldSave()
		{
			temporaryManifest.IsDuplicated = false;
			temporaryManifest.ShouldSave = false;
			AssertNoNotifications(temporaryManifest.ShouldSaveInfo);

			temporaryManifest.ShouldSave = true;
			AssertNoNotifications(temporaryManifest.ShouldSaveInfo);

			temporaryManifest.IsDuplicated = true;
			temporaryManifest.ShouldSave = false;
			AssertNoNotifications(temporaryManifest.ShouldSaveInfo);

			temporaryManifest.ShouldSave = true;
			AssertHasError(temporaryManifest.ShouldSaveInfo, "This manifest is duplicated and can not be selected for update.");
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return temporaryManifest;
		}

		protected override void SetUp()
		{
			base.SetUp();

			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "TestVessel";
			calcManifest = new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest);
			calcManifest.VesselName = "TestVessel";
			temporaryManifest = new TemporaryManifest(Factory, calcManifest);
		}

		CalcExportManifestHeader calcManifest;
		TemporaryManifest temporaryManifest;

		#endregion
	}
}
