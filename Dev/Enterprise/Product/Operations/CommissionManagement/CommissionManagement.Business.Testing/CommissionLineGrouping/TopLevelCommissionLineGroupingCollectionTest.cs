using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(TopLevelCommissionLineGroupingCollectionForTest))]
	internal class TopLevelCommissionLineGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TopLevelCommissionLineGroupingCollectionForTest>
	{
		#region Refresh

		public void TestRefresh()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			var party = Factory.NewWithValidTestData<OrgHeader>();
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var rate = Factory.NewWithValidTestData<OrgCommissionAgreementRecipientRate>();
			var commissionableChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true));

			var jobCommissionHeader1 = Factory.NewWithValidTestData<AccCommissionHeader>();
			jobCommissionHeader1.CH0_GroupingSourceTableCode = job.TablePrefix;
			jobCommissionHeader1.CH0_GroupingSourceID = job.PK;
			jobCommissionHeader1.CH0_JobNumber = job.JH_JobNum;
			var jobStaffLine1 = jobCommissionHeader1.Lines.AddNew();
			jobStaffLine1.CL0_GS_NKStaff = "ADL";
			jobStaffLine1.CL0_CAT = rate.PK;
			var jobPartyLine1 = jobCommissionHeader1.LineGroups.AddNew(commissionableChargeCode).Lines.AddNew();
			jobPartyLine1.CL0_OH_Party = party.PK;
			jobPartyLine1.CL0_CAT = rate.PK;

			var jobCommissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			jobCommissionHeader2.CH0_GC = jobCommissionHeader1.CH0_GC;
			jobCommissionHeader2.CH0_GroupingSourceTableCode = job.TablePrefix;
			jobCommissionHeader2.CH0_GroupingSourceID = job.PK;
			jobCommissionHeader2.CH0_JobNumber = job.JH_JobNum;
			var jobStaffLine2 = jobCommissionHeader2.Lines.AddNew();
			jobStaffLine2.CL0_GS_NKStaff = "ADL";
			jobStaffLine2.CL0_CAT = rate.PK;
			var jobPartyLine2 = jobCommissionHeader2.LineGroups.AddNew(commissionableChargeCode).Lines.AddNew();
			jobPartyLine2.CL0_OH_Party = party.PK;
			jobPartyLine2.CL0_CAT = rate.PK;

			var invoiceCommissionHeader1 = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceCommissionHeader1.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			invoiceCommissionHeader1.CH0_GroupingSourceID = invoice.PK;
			var invoiceStaffLine1 = invoiceCommissionHeader1.Lines.AddNew();
			invoiceStaffLine1.CL0_GS_NKStaff = "ADL";
			invoiceStaffLine1.CL0_CAT = rate.PK;
			var invoicePartyLine1 = invoiceCommissionHeader1.LineGroups.AddNew(commissionableChargeCode).Lines.AddNew();
			invoicePartyLine1.CL0_OH_Party = party.PK;
			invoicePartyLine1.CL0_CAT = rate.PK;

			var invoiceCommissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceCommissionHeader2.CH0_GC = invoiceCommissionHeader1.CH0_GC;
			invoiceCommissionHeader2.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			invoiceCommissionHeader2.CH0_GroupingSourceID = invoice.PK;
			var invoiceStaffLine2 = invoiceCommissionHeader2.Lines.AddNew();
			invoiceStaffLine2.CL0_GS_NKStaff = "ADL";
			invoiceStaffLine2.CL0_CAT = rate.PK;
			var invoicePartyLine2 = invoiceCommissionHeader2.LineGroups.AddNew(commissionableChargeCode).Lines.AddNew();
			invoicePartyLine2.CL0_OH_Party = party.PK;
			invoicePartyLine2.CL0_CAT = rate.PK;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var lineCollection = new ViewCommissionLineCollection(anotherFactory);
			var firstLevelGrouping = new ViewCommissionLineRecipientAndLocalCompanyAndSourceGrouper<ViewCommissionLine>();
			var secondLevelGrouping = new ViewCommissionLineAllCurrenciesGrouper<ViewCommissionLine>();
			var groupingCollection = new TopLevelCommissionLineGroupingCollectionForTest(lineCollection, new ViewCommissionLineGrouper<ViewCommissionLine>[] { firstLevelGrouping, secondLevelGrouping });
			groupingCollection.Init();

			AssertMaxTableHits(0, AccCommissionHeaderSchema.Constants.TableName, anotherFactory);

			var jobStaffGrouping = groupingCollection.Cast<CommissionLineGroupingForTest>().Single(x => x.SourceId == job.PK && x.StaffCode == "ADL");
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionLine>.PKOnlyComparer,
				new[]
				{
					jobStaffLine1,
					jobStaffLine2
				},
				jobStaffGrouping.CommissionLines.Select(x => x.AccCommissionLine));

			var jobPartyGrouping = groupingCollection.Cast<CommissionLineGroupingForTest>().Single(x => x.SourceId == job.PK && x.PartyPk == party.PK);
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionLine>.PKOnlyComparer,
				new[]
				{
					jobPartyLine1,
					jobPartyLine2
				},
				jobPartyGrouping.CommissionLines.Select(x => x.AccCommissionLine));

			var invoiceStaffGrouping = groupingCollection.Cast<CommissionLineGroupingForTest>().Single(x => x.SourceId == invoice.PK && x.StaffCode == "ADL");
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionLine>.PKOnlyComparer,
				new[]
				{
					invoiceStaffLine1,
					invoiceStaffLine2
				},
				invoiceStaffGrouping.CommissionLines.Select(x => x.AccCommissionLine));

			var invoicePartyGrouping = groupingCollection.Cast<CommissionLineGroupingForTest>().Single(x => x.SourceId == invoice.PK && x.PartyPk == party.PK);
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionLine>.PKOnlyComparer,
				new[]
				{
					invoicePartyLine1,
					invoicePartyLine2
				},
				invoicePartyGrouping.CommissionLines.Select(x => x.AccCommissionLine));

			AssertEquals(4, groupingCollection.Count);
		}

		#endregion

		#region Implementation

		protected override TopLevelCommissionLineGroupingCollectionForTest GetCollectionToTest()
		{
			var commissionLineCollection = new ViewCommissionLineCollection(Factory);

			var collection = new TopLevelCommissionLineGroupingCollectionForTest(commissionLineCollection, null);
			collection.Init();
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var grouping = new CommissionLineGroupingForTest(Factory);
			grouping.Init(new[] { Factory.New<ViewCommissionLine>() });
			return grouping;
		}

		#endregion
	}

	class TopLevelCommissionLineGroupingCollectionForTest : TopLevelCommissionLineGroupingCollection<CommissionLineGroupingForTest, ViewCommissionLine>
	{
		public TopLevelCommissionLineGroupingCollectionForTest(IBusinessObjectCollection innerCollection, ViewCommissionLineGrouper<ViewCommissionLine>[] subGroupers)
			: base(innerCollection, subGroupers)
		{
		}

		protected override CommissionLineGroupingForTest CreateNew(ViewCommissionLineGrouper<ViewCommissionLine>[] subGroupers)
		{
			return new CommissionLineGroupingForTest(Factory, subGroupers);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommissionLineGroupingForTest(Factory);
		}
	}
}
