using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Module.Testing
{
	[TestedType(typeof(TopLevelCommissionManagementGroupingCollection))]
	public class TopLevelCommissionManagementGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TopLevelCommissionManagementGroupingCollection>
	{
		public void TestGroupings()
		{
			var xxxCompany = Factory.New<GlbCompany>();
			var yyyCompany = Factory.New<GlbCompany>();
			var invoice = Factory.New<ARInvoice>();
			var job = Factory.NewJobForTesting<JobHeader>();
			var adlStaff = Factory.New<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var scwStaff = Factory.New<GlbStaff>();
			scwStaff.GS_Code = "SCW";
			var aaaParty = Factory.New<OrgHeader>();
			var bbbParty = Factory.New<OrgHeader>();

			var commissionLines = new ViewCommissionLineCollection(Factory);
			var invCommissionLineXxxAdl1 = commissionLines.AddNew();
			invCommissionLineXxxAdl1.VCL_GroupingSourceID = invoice.PK;
			invCommissionLineXxxAdl1.VCL_GroupingSourceTableCode = invoice.TablePrefix;
			invCommissionLineXxxAdl1.VCL_GC_Company = xxxCompany.PK;
			invCommissionLineXxxAdl1.VCL_GS_NKStaff = adlStaff.GS_Code;
			invCommissionLineXxxAdl1.VCL_OH_Party = aaaParty.PK;

			var invCommissionLineXxxAdl2 = commissionLines.AddNew();
			invCommissionLineXxxAdl2.VCL_GroupingSourceID = invoice.PK;
			invCommissionLineXxxAdl2.VCL_GroupingSourceTableCode = invoice.TablePrefix;
			invCommissionLineXxxAdl2.VCL_GC_Company = xxxCompany.PK;
			invCommissionLineXxxAdl2.VCL_GS_NKStaff = adlStaff.GS_Code;
			invCommissionLineXxxAdl1.VCL_OH_Party = ZGuid.Empty;

			var invCommissionLineXxxScw = commissionLines.AddNew();
			invCommissionLineXxxScw.VCL_GroupingSourceID = invoice.PK;
			invCommissionLineXxxScw.VCL_GroupingSourceTableCode = invoice.TablePrefix;
			invCommissionLineXxxScw.VCL_GC_Company = xxxCompany.PK;
			invCommissionLineXxxScw.VCL_GS_NKStaff = scwStaff.GS_Code;

			var invCommissionLineXxxAaa1 = commissionLines.AddNew();
			invCommissionLineXxxAaa1.VCL_GroupingSourceID = invoice.PK;
			invCommissionLineXxxAaa1.VCL_GroupingSourceTableCode = invoice.TablePrefix;
			invCommissionLineXxxAaa1.VCL_GC_Company = xxxCompany.PK;
			invCommissionLineXxxAaa1.VCL_OH_Party = aaaParty.PK;

			var invCommissionLineXxxAaa2 = commissionLines.AddNew();
			invCommissionLineXxxAaa2.VCL_GroupingSourceID = invoice.PK;
			invCommissionLineXxxAaa2.VCL_GroupingSourceTableCode = invoice.TablePrefix;
			invCommissionLineXxxAaa2.VCL_GC_Company = xxxCompany.PK;
			invCommissionLineXxxAaa2.VCL_OH_Party = aaaParty.PK;

			var jobCommissionLineYyyAdl1 = commissionLines.AddNew();
			jobCommissionLineYyyAdl1.VCL_GroupingSourceID = job.PK;
			jobCommissionLineYyyAdl1.VCL_GroupingSourceTableCode = job.TablePrefix;
			jobCommissionLineYyyAdl1.VCL_GC_Company = yyyCompany.PK;
			jobCommissionLineYyyAdl1.VCL_GS_NKStaff = adlStaff.GS_Code;

			var jobCommissionLineYyyAdl2 = commissionLines.AddNew();
			jobCommissionLineYyyAdl2.VCL_GroupingSourceID = job.PK;
			jobCommissionLineYyyAdl2.VCL_GroupingSourceTableCode = job.TablePrefix;
			jobCommissionLineYyyAdl2.VCL_GC_Company = yyyCompany.PK;
			jobCommissionLineYyyAdl2.VCL_GS_NKStaff = adlStaff.GS_Code;

			var collection = new TopLevelCommissionManagementGroupingCollection(commissionLines);
			collection.Init();

			var invAdlGrouping = collection.Cast<ViewCommissionLineGrouping>().Single(x => x.SourceId == invoice.PK && x.StaffCode == adlStaff.GS_Code);
			AssertContainsExactElementsInAnyOrder(new[] { invCommissionLineXxxAdl1, invCommissionLineXxxAdl2 }, invAdlGrouping.CommissionLines);

			var invScwGrouping = collection.Cast<ViewCommissionLineGrouping>().Single(x => x.SourceId == invoice.PK && x.StaffCode == scwStaff.GS_Code);
			AssertContainsExactElementsInAnyOrder(new[] { invCommissionLineXxxScw }, invScwGrouping.CommissionLines);

			var invAaaGrouping = collection.Cast<ViewCommissionLineGrouping>().Single(x => x.SourceId == invoice.PK && x.PartyPk == aaaParty.PK);
			AssertContainsExactElementsInAnyOrder(new[] { invCommissionLineXxxAaa1, invCommissionLineXxxAaa2 }, invAaaGrouping.CommissionLines);

			var jobAdlGrouping = collection.Cast<ViewCommissionLineGrouping>().Single(x => x.SourceId == job.PK && x.StaffCode == adlStaff.GS_Code);
			AssertContainsExactElementsInAnyOrder(new[] { jobCommissionLineYyyAdl1, jobCommissionLineYyyAdl2 }, jobAdlGrouping.CommissionLines);

			AssertEquals(4, collection.Count);
		}

		#region Overrides

		TopLevelCommissionManagementGroupingCollection GroupingCollection
		{
			get
			{
				if (groupingCollection == null)
				{
					var viewCommissionLines = new ViewCommissionLineCollection(Factory);
					groupingCollection = new TopLevelCommissionManagementGroupingCollection(viewCommissionLines);
					groupingCollection.Init();
				}

				return groupingCollection;
			}
		}
		TopLevelCommissionManagementGroupingCollection groupingCollection;

		protected override TopLevelCommissionManagementGroupingCollection GetCollectionToTest()
		{
			return GroupingCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ViewCommissionLineGrouping(Factory);
		}

		#endregion
	}
}
