using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(QuarantineTabPage))]
	sealed class QuarantineTabPageTest : TestCaseWithFactory
	{
		public void TestVisibleAndCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var header = invoice.QuarantineExDocHeader;
			header.QH_ProduceType = string.Empty;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				using (var frm = new ZForm(declaration))
				{
					var bindingSource = ((ICompositeControlBindingSourceProvider)frm).BindingSource as KBindingSource;
					var page = new QuarantineTabPage<ZUserControl>(header, bindingSource, null)
					{ IsVisibleFunc = (c, _) => c, GetCaptionFunc = (c) => c ? "Active" : "Inactive" };
					var tabControl = new ZTabControl();
					tabControl.TabPages.Add(page);
					tabControl.Dock = DockStyle.Fill;
					frm.Controls.Add(tabControl);
					frm.Show();
					Application.DoEvents();
					Assert("Default to true.", page.TabVisible);
					AssertEquals("Default to empty.", string.Empty, page.Text);

					header.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
					Assert("Precondition", !header.IsNEXDOCSActive);
					Assert("Should be changed when the value of QH_ProduceType is changed.", !page.TabVisible);
					AssertEquals("Should be changed when the value of QH_ProduceType is changed.", "Inactive", page.Text);
					header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
					Assert("Precondition", header.IsNEXDOCSActive);
					Assert("Should be changed when the value of QH_ProduceType is changed.", page.TabVisible);
					AssertEquals("Should be changed when the value of QH_ProduceType is changed.", "Active", page.Text);

					header.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
					Assert("Precondition", !header.IsNEXDOCSActive);
					Assert("Should be changed when the value of QH_ProduceType is changed.", !page.TabVisible);
					AssertEquals("Should be changed when the value of QH_ProduceType is changed.", "Inactive", page.Text);
					header.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
					Assert("Precondition", header.IsNEXDOCSActive);
					Assert("Should be changed when the value of QH_ProduceType is changed.", page.TabVisible);
					AssertEquals("Should be changed when the value of QH_ProduceType is changed.", "Active", page.Text);

					header.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
					Assert("Precondition", !header.IsNEXDOCSActive);
					Assert("Should be changed when the value of QH_ProduceType is changed.", !page.TabVisible);
					AssertEquals("Should be changed when the value of QH_ProduceType is changed.", "Inactive", page.Text);
					header.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
					Assert("Precondition", header.IsNEXDOCSActive);
					Assert("Should be changed when the value of QH_ProduceType is changed.", page.TabVisible);
					AssertEquals("Should be changed when the value of QH_ProduceType is changed.", "Active", page.Text);

					header.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
					Assert("Precondition", !header.IsNEXDOCSActive);
					Assert("Should be changed when the value of QH_ProduceType is changed.", !page.TabVisible);
					AssertEquals("Should be changed when the value of QH_ProduceType is changed.", "Inactive", page.Text);
					header.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
					Assert("Precondition", header.IsNEXDOCSActive);
					Assert("Should be changed when the value of QH_ProduceType is changed.", page.TabVisible);
					AssertEquals("Should be changed when the value of QH_ProduceType is changed.", "Active", page.Text);

					header.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
					Assert("Precondition", !header.IsNEXDOCSActive);
					Assert("Should be changed when the value of QH_ProduceType is changed.", !page.TabVisible);
					AssertEquals("Should be changed when the value of QH_ProduceType is changed.", "Inactive", page.Text);
					header.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
					Assert("Precondition", header.IsNEXDOCSActive);
					Assert("Should be changed when the value of QH_ProduceType is changed.", page.TabVisible);
					AssertEquals("Should be changed when the value of QH_ProduceType is changed.", "Active", page.Text);
				}
			}
		}
	}

	[TestedType(typeof(QuarantineTabPageWithSubTabs))]
	sealed class QuarantineTabPageWithSubTabsTest : TestCaseWithFactory
	{
		public void TestTabPages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var header = invoice.QuarantineExDocHeader;
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			using (var form = new ZForm(declaration))
			{
				var bindingSource = ((ICompositeControlBindingSourceProvider)form).BindingSource as KBindingSource;
				var page = new QuarantineTabPageWithSubTabs(header, bindingSource, null)
				{
					TabsPages = (exDocHeader, bindingSource, currencyManager) => new QuarantineTabPage[] {
						new QuarantineTabPage<ZUserControl>(exDocHeader, bindingSource, currencyManager)
						{
							IsVisibleFunc = (isActive, _) => isActive,
							GetCaptionFunc = (isActive) => "Sub Page 1"
						},
						new QuarantineTabPage<ZUserControl>(exDocHeader, bindingSource, currencyManager)
						{
							IsVisibleFunc = (isActive, _) => isActive,
							GetCaptionFunc = (isActive) => "Sub Page 2"
						}
					}
				};

				var tabControl = new ZTabControl();
				tabControl.TabPages.Add(page);
				tabControl.Dock = DockStyle.Fill;
				form.Controls.Add(tabControl);
				form.Show();
				var subTabControl = page.Controls[0];
				AssertEquals(2, subTabControl.Controls.Count);
				AssertEquals("Sub Page 1.Text", "Sub Page 1", subTabControl.Controls[0].Text);
				AssertEquals("Sub Page 2.Text", "Sub Page 2", subTabControl.Controls[1].Text);

				header.QH_ProduceType = string.Empty;
				AssertEquals(0, page.Controls[0].Controls.Count);
			}
		}
	}
}
