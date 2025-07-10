using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Module.BillingPrices
{
	[TestedType(typeof(BillingPricesModule))]
	public class BillingPricesModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.BillingPrices;
		}

		public void TestBulkCopyMenu()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDNYC";
			org.CreateAndLoadLicenceForOrg();
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RN_NKCountry = "US";
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			priceHeader.L6_LicenceEdition = "EXP";
			var priceItem = priceHeader.Items.AddNew();
			priceItem.L7_Code = "ISF";
			priceItem.L7_Description = "ediImporterSecurityFiling";
			priceItem.L7_ParentCategory = "ODM";
			priceItem.L7_ParentCode = "COR";
			priceItem.L7_FeeType = "MFE";
			priceItem.L7_Price = 2m;
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var module = new BillingPricesModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.SetFormsModalTo(form);
				Application.DoEvents();
				var bullCopy = module.ActionsMenuItem.MenuItems.FindByText("Bulk Copy");
				AssertNotNull(bullCopy);
				IFilterModuleInternalsForTesting moduleInternals = module;
				moduleInternals.PerformSearch();
				AssertEquals("Precondition", 1, module.GridCollection.Count);
				module.DisplayGrid.Select(0);
				bullCopy.PerformClick();
				using (var lastShownForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType(typeof(PriceItemBulkCopyForm), lastShownForm);
					AssertNotNull(lastShownForm);
				}
			}
		}
	}
}
