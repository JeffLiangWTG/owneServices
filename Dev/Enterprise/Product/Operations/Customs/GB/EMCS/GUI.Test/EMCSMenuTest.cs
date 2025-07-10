using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.EMCS.GUI.Testing
{
	sealed class EMCSMenuTest : TestCaseWithFactory
	{
		public void TestSeparatorExistsBeforeCountrySpecificItems()
		{
			using var menu = new EMCSMenu(declaration);
			CombineAssertions(() =>
			{
				AssertEquals("Separator before GB specific", "-", menu.MenuItems[0].Text);
				AssertEquals("Trader Validation next", "Pre-Validate Trader", menu.MenuItems[1].Text);
			});
		}

		public void TestPreValidateTrader_Click()
		{
			using var form = new ZForm(declaration);
			using var menu = new EMCSMenu(declaration);
			CombineAssertions(() =>
			{
				_ = form.Menu.MenuItems.Add(menu);
				var menuItem = menu.MenuItems.FindByText(EMCSPreValidateTraderInfoHelper.Constants.PreValidateTraderInfoCaption);
				AssertNotNull(menuItem);
				AssertType<EMCSMenu>(menu);
				menuItem.PerformClick();
				AssertEquals(EMCSPreValidateTraderInfoHelper.Constants.PreValidateTraderInfoWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				_ = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TEN100", Core.Constants.CountryCodes.UnitedKingdom);
				orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;

				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.ZG_ExciseProductCode = "AAAA";

				var package = declaration.EMCSPackages.AddNew();
				package.B5_UnitCount = 1;
				package.B5_UnitType = "BX";
				Factory.Save();

				menuItem.PerformClick();
				AssertEquals("Pre-Validate Trader has been queued successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EU.EMCS.Business.EMCSJobDeclaration>();
		}

		EU.EMCS.Business.EMCSJobDeclaration declaration;
	}
}
