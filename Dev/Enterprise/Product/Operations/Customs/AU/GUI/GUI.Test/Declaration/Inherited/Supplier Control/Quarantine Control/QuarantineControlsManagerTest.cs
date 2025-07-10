using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI.CommercialInvoice;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(QuarantineControlsManager))]
	sealed class QuarantineControlsManagerTest : TestCaseWithFactory
	{
		public void TestInitializeWithNormalInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;

			var invoice = declaration.Invoices.AddNew();

			using (var frm = new ZAUCustomsDeclarationForm(declaration))
			{
				frm.Show();
				Application.DoEvents();

				frm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = frm.CustomsBrokerageUserControl.InvoicesTabPage;

				var supplierHeaderControl = (AUQuarantineSupplierHeaderUserControl)frm.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var tabControl = supplierHeaderControl.InvoiceTabControl;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
				{
					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
					Assert("Precondition. NEXDOC should not be active.", !invoice.IsNEXDOCSActive);
					AssertChildrenPages_DisplayRFP(tabControl);
				}

				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
				Assert("Precondition. NEXDOC should be active.", invoice.IsNEXDOCSActive);
				AssertChildrenPages_DisplayREX(tabControl);

				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
				AssertChildrenPages_DisplayREX(tabControl);

				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
				var inspectionDetailsChildrenPagesSkins = new (string caption, Type controlType)[]
				{
					("Details", typeof(RFPInspectionDetailsUserControl)),
					("Skins && Hides", typeof(REXSkinsAndHidesUserControl))
				};
				AssertChildrenPages_DisplayREX(tabControl, inspectionDetailsChildrenPagesSkins);
			}
		}

		void AssertChildrenPages_DisplayRFP(ZTabControl tabControl)
		{
			var childrenPages = new (string caption, Type controlType)[]
			{
				("RFP Details", typeof(RFPDetailsUserControl)),
				("RFP Indicator Declarations", typeof(RFPIndicatorDeclarationsUserControl)),
				("RFP Inspection Details", typeof(ZTabControl)),
				("RFP Forward/Transfer", typeof(RFPForwardTransferUserControl)),
				("RFP Ships Compartments", typeof(RFPShipsCompartmentsUserControl)),
				("RFP EU Details", typeof(RFPEUTransitUserControl)),
				("RFP Messaging", typeof(RFPMessagingUserControl)),
			};
			AssertTabPages(tabControl, childrenPages);

			var inspectionDetailsChildrenPages = new (string caption, Type controlType)[]
			{
				("Details", typeof(RFPInspectionDetailsUserControl)),
			};
			var inspectionDetailsTabControl = GetSubTabControl(tabControl, "RFP Inspection Details");
			AssertTabPages(inspectionDetailsTabControl, inspectionDetailsChildrenPages, getTabPageWithPrefix: false);

			var tabPage = tabControl.TabPages.Cast<ZTabPage>().First(c => c.Text == "RFP Details");
			tabControl.SelectedTab = tabPage;
		}

		void AssertChildrenPages_DisplayREX(ZTabControl tabControl, (string caption, Type controlType)[] inspectionDetailsChildrenPages = null)
		{
			var childrenPages = new (string caption, Type controlType)[]
			{
				("REX Details", typeof(RFPDetailsUserControl)),
				("REX Declarations", typeof(REXDeclarationsUserControl)),
				("REX Inspection Details", typeof(ZTabControl)),
				("REX Forward/Transfer", typeof(RFPForwardTransferUserControl)),
				("REX Ships Compartments", typeof(RFPShipsCompartmentsUserControl)),
				("REX EU Details", typeof(RFPEUTransitUserControl)),
				("REX Invoice Attachments", typeof(REXInvoiceProductAttachmentsUserControl)),
				("REX Acknowledgement", typeof(REXAcknowledgementUserControl)),
				("REX Messaging", typeof(RFPMessagingUserControl)),
			};
			AssertTabPages(tabControl, childrenPages);

			var inspectionDetailsPages = inspectionDetailsChildrenPages ??
			[
				("Details", typeof(RFPInspectionDetailsUserControl))
			];
			var inspectionDetailsTabControl = GetSubTabControl(tabControl, "REX Inspection Details");
			AssertTabPages(inspectionDetailsTabControl, inspectionDetailsPages, getTabPageWithPrefix: false);

			var tabPage = tabControl.TabPages.Cast<ZTabPage>().First(c => c.Text == "REX Details");
			tabControl.SelectedTab = tabPage;
		}

		public void TestInitializeWithStandaloneInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MakeNonPersistent();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Quarantine;

			using (var frm = new CommercialInvoiceForm(invoice))
			{
				frm.Show();
				Application.DoEvents();

				var tabPage = frm.FindSingle<ZTabPage>("QuarantineTabPage");
				frm.MainTabControl.SelectedTab = tabPage;

				var tabControl = tabPage.FindSingleOrDefault<ZTabControl>();

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
				{
					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
					Assert("Should be always true on a standalone invoice.", invoice.IsNEXDOCSActive);
					AssertChildrenPages_DisplayREX(tabControl);

					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
					Assert("Should be always true on a standalone invoice.", invoice.IsNEXDOCSActive);
					AssertChildrenPages_DisplayREX(tabControl);

					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
					AssertChildrenPages_DisplayREX(tabControl);

					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
					var inspectionDetailsChildrenPages = new (string caption, Type controlType)[]
					{
						("Details", typeof(RFPInspectionDetailsUserControl)),
						("Skins && Hides", typeof(REXSkinsAndHidesUserControl)),
					};
					AssertChildrenPages_DisplayREX(tabControl, inspectionDetailsChildrenPages);
				}
			}
		}

		static ZTabControl GetSubTabControl(ZTabControl tabControl, string caption)
		{
			var tabPage = tabControl.TabPages.Cast<ZTabPage>().First(c => c.Text == caption);
			tabControl.SelectedTab = tabPage;
			return (ZTabControl)tabPage.Controls[0];
		}

		void AssertTabPages(ZTabControl tabControl, (string caption, Type controlType)[] childrenPages, bool getTabPageWithPrefix = true)
		{
			var tabPages = tabControl.TabPages.Cast<ZTabPage>();
			if (getTabPageWithPrefix)
			{
				tabPages = tabPages.Where(c => c.Text.StartsWith("RFP") || c.Text.StartsWith("REX"));
			}

			var expectedNames = childrenPages.Select(c => c.caption).ToArray();
			var actualNames = tabPages.Select(c => c.Text).ToArray();

			AssertArrayEqualsByElements("Should auto update the visible and name of tab control.", expectedNames, actualNames);

			CombineAssertions(() =>
			{
				foreach (var tabPage in tabPages)
				{
					tabControl.SelectedTab = tabPage;

					var expectedType = childrenPages
						.First(c => c.caption == tabPage.Text)
						.controlType;

					var actualType = tabPage.Controls[0].GetType();

					AssertEquals($"The type of {tabPage.Name} should be {expectedType.Name}.", expectedType, actualType);
				}
			});
		}
	}
}
