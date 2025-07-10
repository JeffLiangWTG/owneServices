using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using MenuNames = Enterprise.Core.Constants.MenuNameConstants;

namespace Enterprise.Accounting.Integration.Testing
{
	public abstract class BulkPostingModuleTest : TestCaseWithFactory
	{
		public void TestPostMenuItemForModule()
		{
			using (IBulkPostingModuleInternalsForTesting module = GetNewModuleForTest())
			{
				IMenuItem postMenuItem = module.PostMenuItem;
				AssertNotNull("Post Menu Item should not be null", postMenuItem);
				AssertEquals("Post Menu Item Text", "&Post", postMenuItem.GetType().GetProperty("Text").GetValue(postMenuItem, null));

				var allowedOptions = GetAllowedPostingOptions();
				AssertNotNull("GetAllowedPostingOptions shouldn't return null", allowedOptions);
				if (!IsPostingConsolBulk)
				{
					CheckMenuItem(module, allowedOptions, MenuNames.PostAllChargesAndCosts, JobInvoicingPostingOption.All);
					CheckMenuItem(module, allowedOptions, MenuNames.PostLocalClientCharges, JobInvoicingPostingOption.LocalClient);
					CheckMenuItem(module, allowedOptions, MenuNames.PostOverseasAgentCharges, JobInvoicingPostingOption.Agent);
					CheckMenuItem(module, allowedOptions, MenuNames.PostAllRevenueCharges, JobInvoicingPostingOption.Revenue);
					CheckMenuItem(module, allowedOptions, MenuNames.PostDisbursementChargesonly, JobInvoicingPostingOption.Disbursement);
					CheckMenuItem(module, allowedOptions, MenuNames.PostCosts, JobInvoicingPostingOption.Costs);
					CheckMenuItem(module, allowedOptions, MenuNames.PostAllSisterCompanyCharges, JobInvoicingPostingOption.AllSisterCompanyCharges);
					CheckMenuItem(module, allowedOptions, MenuNames.PostLocalSisterCompanyChargesOnly, JobInvoicingPostingOption.LocalSisterCompanyChargesOnly);
				}
				else
				{
					CheckMenuItem(module, allowedOptions, MenuNames.PostOverseasAgentCharges, JobInvoicingPostingOption.Agent);
					CheckMenuItem(module, allowedOptions, MenuNames.PostAllCosts, JobInvoicingPostingOption.Costs);
					CheckMenuItem(module, allowedOptions, MenuNames.PostWholeConsol, JobInvoicingPostingOption.All);
					CheckMenuItem(module, allowedOptions, MenuNames.PostConsolCostsOnly, JobInvoicingPostingOption.ConsolCosts);
				}
			}
		}

		protected abstract IBulkPostingModuleInternalsForTesting GetNewModuleForTest();
		protected virtual ZBool IsPostingConsolBulk
		{
			get { return ZBool.False; }
		}

		protected virtual JobInvoicingPostingOption[] GetAllowedPostingOptions()
		{
			return new[]
				{
					JobInvoicingPostingOption.All,
					JobInvoicingPostingOption.LocalClient,
					JobInvoicingPostingOption.Agent,
					JobInvoicingPostingOption.Revenue,
					JobInvoicingPostingOption.Disbursement,
					JobInvoicingPostingOption.Costs,
					JobInvoicingPostingOption.ConsolCosts,
					JobInvoicingPostingOption.AllSisterCompanyCharges,
					JobInvoicingPostingOption.LocalSisterCompanyChargesOnly,
				};
		}

		void CheckMenuItem(IBulkPostingModuleInternalsForTesting module, JobInvoicingPostingOption[] allowedOptions, string childMenuItemText, JobInvoicingPostingOption expectedPostingOption)
		{
			IMenuItem foundMenuItem = null;
			foreach (IMenuItem menuItem in module.PostMenuItem.MenuItems)
			{
				var menuName = (string)menuItem.GetType().GetProperty("Text").GetValue(menuItem, null);
				if (menuName == childMenuItemText)
				{
					foundMenuItem = menuItem;
					break;
				}
			}

			var errorMessagePrefix = string.Format("Menu item '{0}' ", childMenuItemText);

			if (allowedOptions.Contains(expectedPostingOption))
			{
				AssertNotNull(errorMessagePrefix + "should be added", foundMenuItem);

				var originalDontDoActualPostingValue = module.DontDoActualPosting;
				try
				{
					module.DontDoActualPosting = true;
					foundMenuItem.GetType().InvokeMember("PerformClick",
						BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
						null, foundMenuItem, null);
					AssertEquals("Posting Option for [" + childMenuItemText + "]", expectedPostingOption, module.LastUsedPostingOptionForTest);
				}
				finally
				{
					module.DontDoActualPosting = originalDontDoActualPostingValue;
				}
			}
			else
			{
				AssertNull(errorMessagePrefix + "should not be added", foundMenuItem);
			}
		}
	}
}
