using System;
using System.Windows.Forms;
using Enterprise.Client.Rohlig.Bellin;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.Rohlig.Module.Testing
{
	public class BellinModuleTest : TestCase
	{
		public void TestMenuItem()
		{
			bool result = false;
			using (BellinModule module = new BellinModule())
			{
				foreach (MenuItem menuItem in module.FormActionMenu.FindByText("&Actions").MenuItems)
				{
					foreach (MenuItem item in menuItem.MenuItems)
					{
						if (item.Text.EndsWith(Constants.BellinCaption.Replace("Export", "")))
						{
							result = true;
							break;
						}
					}
				}
			}

			Assert("Bellin Menu Item was not Added to the APTransactionModule Menu", result);
		}

		public void TestFormType()
		{
			using (BellinModuleTestClass module = new BellinModuleTestClass())
			{
				module.ExportBellinTransactions_Click(this, EventArgs.Empty);
				AssertEquals("Form should be a BellinExportForm", typeof(BellinExportForm), module.ExpectedFormType);
			}
		}

		class BellinModuleTestClass : BellinModule
		{
			public Type ExpectedFormType;
			protected override void ShowDialog(ZForm form)
			{
				ExpectedFormType = form.GetType();
			}
		}
	}
}
