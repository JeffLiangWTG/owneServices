using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class HouseConsignmentsTabUserControlTest : TestCaseWithFactory
	{
		public void TestHouseConsignmentPreviousDocumentsTabPageVisibility()
		{
			AssertTabVisibilityForTNN("HouseConsignmentPreviousDocumentsTabPage");
		}

		[RequiresSTA]
		public void TestHouseConsignmentSupportingDocumentsTabPageVisibility()
		{
			AssertTabVisibilityForProvisionalPeriodTNN("HouseConsignmentSupportingDocumentsTabPage");
		}

		[RequiresSTA]
		public void TestHouseConsignmentAdditionalDocumentsTabPageVisibility()
		{
			AssertTabVisibilityForProvisionalPeriodTNN("HouseConsignmentAdditionalDocumentsTabPage");
		}

		void AssertTabVisibilityForTNN(string tabToBeTested)
		{
			var tabPage = userControl.GetTabPage("HouseConsignmentTabControl", tabToBeTested);

			CombineAssertions(() =>
			{
				AssertEquals("When is not TNN is visible", true, tabPage.TabVisible);

				nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
				AssertEquals("When is TNN is not visible", false, tabPage.TabVisible);

				nctsHeader.ESNctsHeader.CEN_TNNArrival = false;
				AssertEquals("Visibility changes dynamically when we set not TNN Phase again", true, tabPage.TabVisible);
			});
		}

		void AssertTabVisibilityForProvisionalPeriodTNN(string tabToBeTested)
		{
			var tabPage = userControl.GetTabPage("HouseConsignmentTabControl", tabToBeTested);

			CombineAssertions(() =>
			{
				using (ESTestHelper.TemporarilySetTransitionPeriod(true))
				{
					AssertEquals("TransitionPeriod + When is not TNN is visible", true, tabPage.TabVisible);

					nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
					AssertEquals("TransitionPeriod + When is TNN is not visible", false, tabPage.TabVisible);

					nctsHeader.ESNctsHeader.CEN_TNNArrival = false;
					AssertEquals("TransitionPeriod + Visibility changes dynamically when we set not TNN Phase again", true, tabPage.TabVisible);
				}

				using (ESTestHelper.TemporarilySetTransitionPeriod(false))
				{
					nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
					AssertEquals("Not transition and is TNN: is visible", true, tabPage.TabVisible);

					nctsHeader.ESNctsHeader.CEN_TNNArrival = false;
					AssertEquals("Not transition and is not TNN", true, tabPage.TabVisible);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			bill = nctsHeader.Bills.AddNew();
			_ = bill.GoodsItems.AddNew();

			form = new ZForm(nctsHeader);
			userControl = new HouseConsignmentsTabUserControl();
			nctsHeader.ESNctsHeader.CEN_TNNArrival = false;

			userControl.SetDataBinding(bill, "");
			form.Controls.Add(userControl);
			form.Show();
		}

		protected override void TearDown()
		{
			base.TearDown();
			form.Dispose();
			userControl.Dispose();
		}

		NctsHeader nctsHeader;
		NctsBill bill;

		ZForm form;
		HouseConsignmentsTabUserControl userControl;
	}
}
