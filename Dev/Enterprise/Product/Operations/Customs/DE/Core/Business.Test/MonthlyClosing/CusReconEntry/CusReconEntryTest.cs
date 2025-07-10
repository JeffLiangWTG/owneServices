using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusReconEntry))]
	sealed class CusReconEntryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCRE_EntryDate_Caption()
		{
			AssertEquals("Local Clearance Date", DataBoundResourceStrings.GetDataForProperty(entry.CRE_EntryDateInfo).Caption);
		}

		public void TestCRE_EntryDate_ReadOnly()
		{
			AssertEquals(true, entry.CRE_EntryDateInfo.ReadOnly);
		}

		public void TestCRE_OriginalEntryNumber_Caption()
		{
			AssertEquals("Registration Number", DataBoundResourceStrings.GetDataForProperty(entry.CRE_OriginalEntryNumberInfo).Caption);
		}

		public void TestCRE_OriginalEntryNumber_ReadOnly()
		{
			AssertEquals(true, entry.CRE_OriginalEntryNumberInfo.ReadOnly);
		}

		public void TestCRE_EntryType_ReadOnly()
		{
			AssertEquals(true, entry.CRE_EntryTypeInfo.ReadOnly);
		}

		public void TestGetNewLookups()
		{
			AssertType<CusReconEntryLookups>(entry.Lookups);
		}

		public void TestCurrentSnapshot()
		{
			CombineAssertions(() =>
			{
				var snapshot = entry.CusReconSnapshots.AddNew();
				snapshot.CRS_Type = CusReconConstants.Lodged;
				AssertNull("No CUR snapshots", entry.CurrentSnapshot);
				snapshot.CRS_Type = CusReconConstants.Current;
				AssertSame("Has CUR snapshot", snapshot, entry.CurrentSnapshot);
			});
		}

		public void TestLodgedSnapshot()
		{
			CombineAssertions(() =>
			{
				var snapshot = entry.CusReconSnapshots.AddNew();
				snapshot.CRS_Type = CusReconConstants.Current;
				AssertNull("No LDG snapshots", entry.LodgedSnapshot);
				snapshot.CRS_Type = CusReconConstants.Lodged;
				AssertSame("Has LDG snapshot", snapshot, entry.LodgedSnapshot);
			});
		}

		public void TestJobNumber()
		{
			entry.EntryHeader.Declaration.JE_DeclarationReference = "B0001000";
			AssertEquals(entry.JobNumber, "B0001000");
		}

		public void TestJobNumber_Caption()
		{
			AssertEquals("Job Number", DataBoundResourceStrings.GetDataForProperty(typeof(CusReconEntry), nameof(CusReconEntry.JobNumber)).Caption);
		}

		public void TestOwnerRef()
		{
			entry.EntryHeader.Declaration.JE_OwnerRef = "OREF1";
			AssertEquals(entry.OwnerRef, "OREF1");
		}

		public void TestOwnerRef_Caption()
		{
			AssertEquals("Owner Ref", DataBoundResourceStrings.GetDataForProperty(typeof(CusReconEntry), nameof(CusReconEntry.OwnerRef)).Caption);
		}

		public void TestEntryHasChanges()
		{
			var snapshot = entry.CusReconSnapshots.AddNew();
			snapshot.CRS_Type = CusReconConstants.Lodged;

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, entry.EntryHasChanges);
				snapshot.CRS_Type = CusReconConstants.Current;
				AssertEquals("Has CUR snapshot", "Yes", entry.EntryHasChanges);
			});
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var reconEntry = factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = factory.NewWithValidTestData<OrgAddress>().PK;
			return reconEntry;
		}

		protected override void SetUp()
		{
			base.SetUp();

			entry = (CusReconEntry)GetNewBusinessObjectForDeleteTest(Factory);
		}
		CusReconEntry entry;
	}
}
