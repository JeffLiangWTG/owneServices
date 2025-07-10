using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(SelectableCommissionLineGroupingForTest))]
	internal class SelectableCommissionLineGroupingTest : NonPersistentBusinessObjectTestCase
	{
		#region Property

		public void TestIsSelected_UpdatesAllLineItems()
		{
			var lineItem1 = new SelectableCommissionLineForTest(Factory.New<ViewCommissionLine>());
			var lineItem2 = new SelectableCommissionLineForTest(Factory.New<ViewCommissionLine>());

			var grouping = GetNewGrouping(Factory, new[] { lineItem1, lineItem2 });

			grouping.IsSelected = true;
			CombineAssertions(() =>
			{
				AssertEquals("lineItem1", true, lineItem1.IsSelected);
				AssertEquals("lineItem2", true, lineItem2.IsSelected);
			});

			grouping.IsSelected = false;
			CombineAssertions(() =>
			{
				AssertEquals("lineItem1", false, lineItem1.IsSelected);
				AssertEquals("lineItem2", false, lineItem2.IsSelected);
			});
		}

		public void TestIsSelected_UpdatesAllSubGroupings()
		{
			var lineItem1 = new SelectableCommissionLineForTest(Factory.New<ViewCommissionLine>());
			var lineItem2 = new SelectableCommissionLineForTest(Factory.New<ViewCommissionLine>());

			var eachLineItemAsAGroupDelegate = new ViewCommissionLineGrouperForTest<SelectableCommissionLineForTest>((lineItems) => lineItems.Select(x => new[] { x }));
			var grouping = GetNewGrouping(Factory, new[] { lineItem1, lineItem2 }, new[] { eachLineItemAsAGroupDelegate });
			AssertEquals("Precondition", 2, grouping.SubGroupingCollection.Count);
			var subGrouping1 = grouping.SubGroupings.First();
			var subGrouping2 = grouping.SubGroupings.Skip(1).First();

			grouping.IsSelected = true;
			CombineAssertions(() =>
			{
				AssertEquals("subGrouping1", true, subGrouping1.IsSelected);
				AssertEquals("subGrouping2", true, subGrouping2.IsSelected);
			});

			grouping.IsSelected = false;
			CombineAssertions(() =>
			{
				AssertEquals("subGrouping1", false, subGrouping1.IsSelected);
				AssertEquals("subGrouping2", false, subGrouping2.IsSelected);
			});
		}

		public void TestIsSelected_UpdatesAllParentGroupings()
		{
			var adlCommissionLine1 = Factory.New<ViewCommissionLine>();
			adlCommissionLine1.VCL_GS_NKStaff = "ADL";
			var adlCommissionLine2 = Factory.New<ViewCommissionLine>();
			adlCommissionLine2.VCL_GS_NKStaff = "ADL";

			var scwCommissionLine1 = Factory.New<ViewCommissionLine>();
			scwCommissionLine1.VCL_GS_NKStaff = "SCW";
			var scwCommissionLine2 = Factory.New<ViewCommissionLine>();
			scwCommissionLine2.VCL_GS_NKStaff = "SCW";

			var adlLineItem1 = new SelectableCommissionLineForTest(adlCommissionLine1);
			var adlLineItem2 = new SelectableCommissionLineForTest(adlCommissionLine2);
			var scwLineItem1 = new SelectableCommissionLineForTest(scwCommissionLine1);
			var scwLineItem2 = new SelectableCommissionLineForTest(scwCommissionLine2);

			var groupByStaffDelegate = new ViewCommissionLineGrouperForTest<SelectableCommissionLineForTest>((lineItems) => lineItems.GroupBy(x => x.ViewCommissionLine.VCL_GS_NKStaff));
			var grouping = GetNewGrouping(Factory,
				new[] { adlLineItem1, adlLineItem2, scwLineItem1, scwLineItem2 },
				new[] { groupByStaffDelegate });

			AssertEquals("Precondition", 2, grouping.SubGroupingCollection.Count);
			var adlSubGrouping = grouping.SubGroupings.Single(x => x.CommissionLines.First().VCL_GS_NKStaff == "ADL");
			var scwSubGrouping = grouping.SubGroupings.Single(x => x.CommissionLines.First().VCL_GS_NKStaff == "SCW");

			adlSubGrouping.IsSelected = false;
			adlLineItem1.IsSelected = true;
			AssertEquals("Should now be true since at least one child item is 'IsSelected'", true, adlSubGrouping.IsSelected);
			adlLineItem2.IsSelected = true;
			AssertEquals("Should still be true since at least one child item is 'IsSelected'", true, adlSubGrouping.IsSelected);

			scwSubGrouping.IsSelected = true;
			scwLineItem1.IsSelected = false;
			AssertEquals("Should not be changed", true, scwSubGrouping.IsSelected);
			scwLineItem2.IsSelected = false;
			AssertEquals("Should now be false since all child items are not 'IsSelected'", false, scwSubGrouping.IsSelected);
		}

		public void TestIsSelected_CallsRefreshBindingForEntitySelectedAmountsInParentGroupings()
		{
			var viewCommissionLine = Factory.New<ViewCommissionLine>();
			var lineItem = new SelectableCommissionLineForTest(viewCommissionLine);
			var allItemsInASingleSubGroupingDelegate = new ViewCommissionLineGrouperForTest<SelectableCommissionLineForTest>((x) => new[] { x });
			var itemGrouping = GetNewGrouping(Factory, new[] { lineItem }, new[] { allItemsInASingleSubGroupingDelegate });
			var itemSubGrouping = itemGrouping.SubGroupings.Single();

			var itemGroupingEntitySelectedAmountValueChangedCalled = false;
			var itemSubGroupingEntitySelectedAmountValueChangedCalled = false;
			itemGrouping.EntitySelectedAmountInfo.ValueChanged += (sender, e) => itemGroupingEntitySelectedAmountValueChangedCalled = true;
			itemSubGrouping.EntitySelectedAmountInfo.ValueChanged += (sender, e) => itemSubGroupingEntitySelectedAmountValueChangedCalled = true;

			lineItem.IsSelected = true;

			CombineAssertions("Should have called EntitySelectedAmount's Refresh Binding for all parent groupings", () =>
			{
				AssertEquals("itemGrouping", true, itemGroupingEntitySelectedAmountValueChangedCalled);
				AssertEquals("itemSubGrouping", true, itemSubGroupingEntitySelectedAmountValueChangedCalled);
			});
		}

		public void TestSelectedEntityAmount()
		{
			var audLine = Factory.New<ViewCommissionLine>();
			audLine.VCL_RX_NKCommissionCurrency = "AUD";
			audLine.VCL_CommissionToLocalExchangeRate = 0.625m;
			audLine.VCL_RX_NKLocalCurrency = "USD";
			audLine.VCL_LocalToPreferredExchangeRate = 1.6m;
			audLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			audLine.VCL_EntityCommissionAmount = 1000;

			var usdLine = Factory.New<ViewCommissionLine>();
			usdLine.VCL_RX_NKCommissionCurrency = "USD";
			usdLine.VCL_CommissionToLocalExchangeRate = 1m;
			usdLine.VCL_RX_NKLocalCurrency = "USD";
			usdLine.VCL_LocalToPreferredExchangeRate = 1.6m;
			usdLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			usdLine.VCL_EntityCommissionAmount = 100;

			var gbpLine = Factory.New<ViewCommissionLine>();
			gbpLine.VCL_RX_NKCommissionCurrency = "GBP";
			gbpLine.VCL_CommissionToLocalExchangeRate = 1.25m;
			gbpLine.VCL_RX_NKLocalCurrency = "USD";
			gbpLine.VCL_LocalToPreferredExchangeRate = 1.6m;
			gbpLine.VCL_RX_NKPreferredPaymentCurrency = "AUD";
			gbpLine.VCL_EntityCommissionAmount = 10;

			var audFinalizerItem = new SelectableCommissionLineForTest(audLine);
			var usdFinalizerItem = new SelectableCommissionLineForTest(usdLine);
			var gbpFinalizerItem = new SelectableCommissionLineForTest(gbpLine);
			audFinalizerItem.IsSelected = false;
			usdFinalizerItem.IsSelected = false;
			gbpFinalizerItem.IsSelected = false;

			var grouping = GetNewGrouping(Factory, new[] { audFinalizerItem, usdFinalizerItem, gbpFinalizerItem });

			AssertEquals((ZDecimal)0m,
				grouping.EntitySelectedAmount);

			audFinalizerItem.IsSelected = true;
			gbpFinalizerItem.IsSelected = true;

			AssertEquals((ZDecimal)1000m +
				20m,
				grouping.EntitySelectedAmount);
		}

		#endregion

		#region Validation

		public void TestValidation_Staff()
		{
			var otherFactory = new BusinessObjectFactory();
			var lineItem1 = new SelectableCommissionLineForTest(Factory.New<ViewCommissionLine>());
			var lineItem2 = new SelectableCommissionLineForTest(Factory.New<ViewCommissionLine>());

			var staff = otherFactory.NewWithValidTestData<GlbStaff>();
			var agreement = otherFactory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.Recipients.AddNew(staff);

			otherFactory.Save();

			var grouping = GetNewGrouping(Factory, new[] { lineItem1, lineItem2 });
			grouping.StaffCode = staff.GS_Code;

			grouping.IsSelected = true;
			Assert(!grouping.IsSelectedInfo.HasWarnings());

			grouping.IsSelected = false;
			grouping.IsSelected = true;
			Assert(!grouping.IsSelectedInfo.HasWarnings());

			var queue = otherFactory.NewWithValidTestData<OrgCommissionCalculationQueue>();
			queue.CAQ_CA0 = agreement.PK;

			otherFactory.Save();

			grouping.IsSelected = false;
			grouping.IsSelected = true;
			Assert(grouping.IsSelectedInfo.HasWarnings());
			Assert(grouping.IsSelectedInfo.HasWarning(string.Format("Some agreements for {0} are still in calculation queue.", staff.GS_FullName)));
		}

		public void TestValidation_Org()
		{
			var otherFactory = new BusinessObjectFactory();
			var lineItem1 = new SelectableCommissionLineForTest(Factory.New<ViewCommissionLine>());
			var lineItem2 = new SelectableCommissionLineForTest(Factory.New<ViewCommissionLine>());

			var org = otherFactory.NewWithValidTestData<OrgHeader>();
			var agreement = otherFactory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.Recipients.AddNew(org);

			otherFactory.Save();

			var grouping = GetNewGrouping(Factory, new[] { lineItem1, lineItem2 });
			grouping.PartyPk = org.PK;

			grouping.IsSelected = true;
			Assert(!grouping.IsSelectedInfo.HasWarnings());

			grouping.IsSelected = false;
			grouping.IsSelected = true;
			Assert(!grouping.IsSelectedInfo.HasWarnings());

			var queue = otherFactory.NewWithValidTestData<OrgCommissionCalculationQueue>();
			queue.CAQ_CA0 = agreement.PK;

			otherFactory.Save();

			grouping.IsSelected = false;
			grouping.IsSelected = true;
			Assert(grouping.IsSelectedInfo.HasWarnings());
			Assert(grouping.IsSelectedInfo.HasWarning(string.Format("Some agreements for {0} are still in calculation queue.", org.OH_FullName)));
		}

		#endregion

		#region Implementation

		static SelectableCommissionLineGroupingForTest GetNewGrouping(BusinessObjectFactory factory, IEnumerable<SelectableCommissionLineForTest> commissionLines, ViewCommissionLineGrouper<SelectableCommissionLineForTest>[] subGroupers = null)
		{
			var grouping = new SelectableCommissionLineGroupingForTest(factory, subGroupers);
			grouping.Init(commissionLines);
			return grouping;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SelectableCommissionLineGroupingForTest(Factory);
		}

		#endregion
	}

	#region Classes

	public class SelectableCommissionLineForTest : NonPersistentBusinessObject, ISelectableViewCommissionLineProvider
	{
		public SelectableCommissionLineForTest(ViewCommissionLine viewCommissionLine)
		{
			this.viewCommissionLine = viewCommissionLine;
		}

		public ZBool IsSelected
		{
			get { return isSelected; }
			set
			{
				SetNonPersistentPropertyValue(IsSelectedInfo, ref isSelected, value);
			}
		}
		ZBool isSelected;

		public ZPropertyInfo IsSelectedInfo
		{
			get { return GetZPropertyInfo(nameof(IsSelected)); }
		}

		public ViewCommissionLine ViewCommissionLine
		{
			get { return viewCommissionLine; }
		}
		readonly ViewCommissionLine viewCommissionLine;
	}

	public class SelectableCommissionLineGroupingForTest : SelectableCommissionLineGrouping<SelectableCommissionLineGroupingForTest, SelectableCommissionLineForTest>
	{
		public SelectableCommissionLineGroupingForTest(BusinessObjectFactory factory, ViewCommissionLineGrouper<SelectableCommissionLineForTest>[] subGroupers = null)
			: base(factory, subGroupers)
		{
		}

		protected override CommissionLineGroupingCollection<SelectableCommissionLineGroupingForTest, SelectableCommissionLineForTest> GetNewSubGroupingCollection(BusinessObjectFactory factory)
		{
			return new SelectableCommissionLineGroupingForTestCollection(factory);
		}
	}

	public class SelectableCommissionLineGroupingForTestCollection : CommissionLineGroupingCollection<SelectableCommissionLineGroupingForTest, SelectableCommissionLineForTest>
	{
		public SelectableCommissionLineGroupingForTestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override SelectableCommissionLineGroupingForTest CreateNew(ViewCommissionLineGrouper<SelectableCommissionLineForTest>[] subGroupers)
		{
			return new SelectableCommissionLineGroupingForTest(Factory, subGroupers);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SelectableCommissionLineGroupingForTest(Factory);
		}
	}

	#endregion
}
