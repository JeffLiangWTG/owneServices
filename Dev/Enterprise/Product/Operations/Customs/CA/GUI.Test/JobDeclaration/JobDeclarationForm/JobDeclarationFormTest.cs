using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.GUI.Testing
{
	abstract class JobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		public void TestHasDeniedPartyActionMenu()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.B2Adjustments;
			using (var form = new JobDeclarationForm(declaration))
			{
				AssertHasDeniedPartyActionMenu(form, false);
			}

			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.ImportCopyforB2;
			using (var form = new JobDeclarationForm(declaration))
			{
				AssertHasDeniedPartyActionMenu(form, false);
			}

			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.XTypeEntry;
			using (var form = new JobDeclarationForm(declaration))
			{
				AssertHasDeniedPartyActionMenu(form, false);
			}

			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				AssertHasDeniedPartyActionMenu(form, true);
			}
		}

		public void TestCustomsBrokerageUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.B2Adjustments;
			AssertCustomsBrokerageUserControlType(typeof(B2AdjustmentsCustomsBrokerageUserControl), declaration);

			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.XTypeEntry;
			AssertCustomsBrokerageUserControlType(typeof(B2AdjustmentsCustomsBrokerageUserControl), declaration);

			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.LVSForConsolidation;
			AssertCustomsBrokerageUserControlType(typeof(LVXCustomsBrokerageUserControl), declaration);

			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.LowValueShipments;
			AssertCustomsBrokerageUserControlType(typeof(LowValueShipmentsUserControl), declaration);

			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.ImportCopyforB2;
			AssertCustomsBrokerageUserControlType(typeof(IM2CustomsBrokerageUserControl), declaration);

			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Import;
			AssertCustomsBrokerageUserControlType(typeof(CustomsBrokerageUserControl), declaration);
		}

		public void TestUniversalCopyVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.B2Adjustments;
			AssertUniversalCopyVisible(false, declaration);
			AssertCommercialInvoiceVisible(true, false, declaration);

			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.ImportCopyforB2;
			AssertUniversalCopyVisible(false, declaration);
			AssertCommercialInvoiceVisible(true, false, declaration);

			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.XTypeEntry;
			AssertUniversalCopyVisible(false, declaration);
			AssertCommercialInvoiceVisible(true, false, declaration);

			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Import;
			AssertUniversalCopyVisible(true, declaration);
			AssertCommercialInvoiceVisible(true, true, declaration);
		}

		public void TestPlugins()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Business.JobMessageTypeList.Codes.ImportCopyforB2;
			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}

			dec.JE_MessageType = Business.JobMessageTypeList.Codes.XTypeEntry;
			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public void TestUseImporterSecurityNumberPopup()
		{
			using (CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "98765"))
			using (CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABCDEFGH"))
			using (CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var importer = Factory.New<OrgHeader>();
				var addInfo = OrgImpAddInfo.Get(importer);
				addInfo.ZO_AccountSecurityNumber = "12345";
				addInfo.ZO_AccountSecirityPassword = "12345678";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Import;
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					declaration.JE_OH_Importer = importer.PK;
					AssertEquals("Do you want to use the Importer's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(declaration.CA_UseImporterAccountSecurityNumber);
					AssertEquals("12345", declaration.TransactionNumber.AccountSecurityCode);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					Application.DoEvents();
					AssertEquals("Do you want to use the Importer's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(!declaration.CA_UseImporterAccountSecurityNumber);
					AssertEquals("98765", declaration.TransactionNumber.AccountSecurityCode);
				}
			}
		}

		public override void TestMinimumSizeNotTooBig()
		{
			var declaration = Factory.New<JobDeclaration>();
			var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1680);
			var minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(962);
			using (var form = new JobDeclarationForm(declaration))
			{
				Assert("CA Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
				Assert("CA Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
			}
		}

		protected override BaseJobComInvoiceLine CreateInvoiceLineForPerformanceTest(BaseJobComInvoiceHeader invoiceHeader, int index, int invoiceIndex)
		{
			var invoiceLine = (JobComInvoiceLine)base.CreateInvoiceLineForPerformanceTest(invoiceHeader, index, invoiceIndex);

			if (invoiceLine.Declaration.IsImport)
			{
				// Enabling all PGA for this line.
				foreach (PGARequirement requirement in invoiceLine.PGARequirements)
				{
					requirement.Indicator = YesNoList.Codes.Yes;
				}

				invoiceLine.HCPGAHeader.Components.AddNew().CA_Name = "TEST";
				invoiceLine.HCPGAHeader.LPCOViews.AddNew().CLP_RN_NKOriginCountryCode = "AU";
				invoiceLine.PHACPGAHeader.LPCOViews.AddNew().CLP_RN_NKOriginCountryCode = "AU";
				invoiceLine.DFOPGAHeader.LPCOViews.AddNew().CLP_RN_NKOriginCountryCode = "AU";
				invoiceLine.GACPGAHeader.LPCOViews.AddNew().CLP_RN_NKOriginCountryCode = "AU";
			}

			return invoiceLine;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}

		void AssertCustomsBrokerageUserControlType(Type controlType, JobDeclaration declaration)
		{
			using (var form = new JobDeclarationForm(declaration) { ControllerID = ControllerIDs.Customs.JobDeclaration })
			{
				var userControl = form.CustomsBrokerageUserControl;
				AssertEquals(controlType, userControl.GetType());
			}
		}

		void AssertUniversalCopyVisible(bool visible, JobDeclaration declaration)
		{
			using (var form = new JobDeclarationFormForTest(declaration) { ControllerID = ControllerIDs.Customs.JobDeclaration })
			{
				form.Show();
				Application.DoEvents();

				var copyMenu = form.ActionsMenuItem.MenuItems.FindByName("UniversalCopy");
				AssertEquals(visible, copyMenu.Visible);
			}
		}

		void AssertCommercialInvoiceVisible(bool isCreated, bool visible, JobDeclaration declaration)
		{
			using (var form = new JobDeclarationFormForTest(declaration) { ControllerID = ControllerIDs.Customs.JobDeclaration })
			{
				form.Show();
				Application.DoEvents();
				var copyMenu = form.TopLevelMenuItems.FirstOrDefault(x => x.Text == ("Commercial &Invoices"));
				if (isCreated)
				{
					AssertEquals(visible, copyMenu.Visible);
				}
				else
				{
					AssertNull(copyMenu);
				}
			}
		}

		void AssertHasDeniedPartyActionMenu(BaseJobDeclarationForm form, bool visibility)
		{
			var actionsMenu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
			var containsDeniedParty = false;
			foreach (MenuItem item in actionsMenu.MenuItems)
			{
				if (item.Text.Contains("Screen"))
				{
					containsDeniedParty = true;
					break;
				}
			}
			AssertEquals("Actions menu contains 'Denied Party' menu item", visibility, containsDeniedParty);
		}

		internal sealed class JobDeclarationFormForTest : JobDeclarationForm
		{
			public JobDeclarationFormForTest(JobDeclaration declaration)
				: base(declaration)
			{
			}

			internal new MenuItem ActionsMenuItem => base.ActionsMenuItem;

			internal IEnumerable<MenuItem> TopLevelMenuItems =>
				TopLevelMenu.MenuItems.Cast<MenuItem>();

			internal IEnumerable<PreSaveDialogStrategy> GetPreSaveDialogStrategiesForTesting() => GetPreSaveDialogStrategies();
		}
	}
}
