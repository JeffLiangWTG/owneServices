using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class Phase5DeclarationDetailsTabUserControlTest : TestCaseWithFactory
	{
		public void TestGuaranteesGroupBoxVisibility()
		{
			AssertControlVisibilityForTNN<ZGroupBox>("GuaranteesGroupBox");
		}

		public void TestAuthorizationsTabPageVisibility()
		{
			AssertTabVisibilityForTNN("AuthorizationsTabPage");
		}

		public void TestSupplyChainActorTabPageVisibility()
		{
			AssertTabVisibilityForTNN("SupplyChainActorTabPage");
		}

		[RequiresSTA]
		public void TestPreviousDocumentsTabPageVisibility()
		{
			AssertTabVisibilityForTNN("PreviousDocumentsTabPage");
		}

		[RequiresSTA]
		public void TestSupportingDocumentsTabPageVisibility()
		{
			AssertTabVisibilityForProvisionalPeriodTNN("SupportingDocumentsTabPage");
		}

		[RequiresSTA]
		public void TestAdditionalDocumentsTabPageVisibility()
		{
			AssertTabVisibilityForProvisionalPeriodTNN("AdditionalDocumentsTabPage");
		}

		void AssertControlVisibilityForTNN<T>(string controlToBeTested) where T : System.Windows.Forms.Control
		{
			var control = (T)userControl.Controls.Find(controlToBeTested, searchAllChildren: true).FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("When is not TNN is visible", true, control.Visible);

				nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
				AssertEquals("When is TNN is not visible", false, control.Visible);

				nctsHeader.ESNctsHeader.CEN_TNNArrival = false;
				AssertEquals("Visibility changes dynamically when we set not TNN Phase again", true, control.Visible);
			});
		}

		void AssertTabVisibilityForTNN(string tabToBeTested)
		{
			var tabPage = userControl.GetTabPage("DeclarationDetailsTabControl", tabToBeTested);

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
			var tabPage = userControl.GetTabPage("DeclarationDetailsTabControl", tabToBeTested);

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

			form = new ZForm();
			userControl = new Phase5DeclarationDetailsTabUserControl();
			nctsHeader.ESNctsHeader.CEN_TNNArrival = false;

			userControl.SetDataBinding(nctsHeader, "");
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

		ZForm form;
		Phase5DeclarationDetailsTabUserControl userControl;
	}
}
