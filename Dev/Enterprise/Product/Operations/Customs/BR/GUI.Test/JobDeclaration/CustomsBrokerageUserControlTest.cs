using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestBaseCustomsDeclarationUserControl()
		{
			using (var testForm = new JobDeclarationForm(Factory.New<JobDeclaration>()))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				using (IDisposable control = brokerageControl.DeclarationUserControl)
				{
					AssertType<CustomsDeclarationUserControl>(control);
				}
			}
		}

		public void TestSupplierHeaderUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertSupplierHeaderUserControl(declaration, typeof(ImportSupplierHeaderUserControl));
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertSupplierHeaderUserControl(declaration, typeof(ExportSupplierHeaderUserControl));
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertSupplierHeaderUserControl(declaration, typeof(ImportLicenseSupplierHeaderUserControl));
		}

		protected void AssertSupplierHeaderUserControl(JobDeclaration declaration, Type expected)
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				using (IDisposable control = brokerageControl.SupplierHeaderUserControl)
				{
					AssertEquals(expected, control.GetType());
				}
			}
		}

		public void TestInvoiceLinesUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertInvoiceLinesUserControl(declaration, typeof(ImportInvoiceLineUserControl));
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertInvoiceLinesUserControl(declaration, typeof(ExportInvoiceLineUserControl));
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertInvoiceLinesUserControl(declaration, typeof(ImportSiscomexInvoiceLineUserControl));
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertInvoiceLinesUserControl(declaration, typeof(ImportLicenseInvoiceLineUserControl));
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			AssertInvoiceLinesUserControl(declaration, typeof(ExportInvoiceLineUserControl));
		}

		void AssertInvoiceLinesUserControl(JobDeclaration declaration, Type expectedType)
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (IDisposable control = brokerageControl.InvoiceLinesUserControl)
				{
					AssertEquals(expectedType, control.GetType());
				}
			}
		}

		public void TestImportLicenseEntryInstructionUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
				using (IDisposable control = brokerageControl.CustomsEntryInstructionUserControl)
				{
					AssertType<ImportLicenseEntryInstructionDetailsUserControl>(control);
				}
			}
		}

		public void TestExportEntryInstructionUserControl()
		{
			using (var testForm = new JobDeclarationForm(Factory.New<JobDeclaration>()))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
				using (IDisposable control = brokerageControl.CustomsEntryInstructionUserControl)
				{
					AssertType<ExportEntryInstructionDetailsUserControl>(control);
				}
			}
		}

		public void TestImportEntryInstructionUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
				using (IDisposable control = brokerageControl.CustomsEntryInstructionUserControl)
				{
					AssertType<ImportEntryInstructionDetailsUserControl>(control);
				}
			}
		}

		public void TestGetMessageUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				using (IDisposable control = brokerageControl.MessageUserControl)
				{
					AssertType<MessageUserControlSubmitionTypeBLT>(control);
				}

				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				using (IDisposable control = brokerageControl.MessageUserControl)
				{
					AssertType<EntriesWithMessagesOnDeclarationUserControl>(control);
				}
			}
		}

		public void TestGetMessageUserControlForImportLicense()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				using (IDisposable control = brokerageControl.MessageUserControl)
				{
					AssertType<ImportLicenseMessageUserControl>(control);
				}

				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				using (IDisposable control = brokerageControl.MessageUserControl)
				{
					AssertType<EntriesWithMessagesOnDeclarationUserControl>(control);
				}
			}
		}

		public void TestCustomsOfficesTabControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl;
				var customsOfficesTabControl = brokerageControl.Controls.Find("CustomsOfficesTabControl", true)[0];
				Assert("CustomsOfficesTabControl should be visible for Export", customsOfficesTabControl.Visible);
			}
		}

		public void TestCustomsOfficesGroupBoxHeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl;
				var customsOfficesGroupbox = brokerageControl.Controls.Find("CustomsOfficesGroupBox", true)[0];
				AssertEquals("Customs offices GroupBox height for Export", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(201), customsOfficesGroupbox.Height);
				var importGroupCustomsOffice = customsOfficesGroupbox.Controls.Find("ImportOfficesUserControl", true)[0];
				AssertEquals("ImportOfficesUserControl visible", false, importGroupCustomsOffice.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertEquals("Customs offices GroupBox height for Import", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(160), customsOfficesGroupbox.Height);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("Customs offices GroupBox height for Import License", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(160), customsOfficesGroupbox.Height);
			}
		}

		public void TestMiscOptionsUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
				AssertNull(brokerageControl.MiscOptions);
			}
		}

		public void TestImportSiscomexEntryInstructionUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.EntryInstructionDetailsTabPage;
				using (IDisposable control = brokerageControl.CustomsEntryInstructionUserControl)
				{
					AssertType<ImportSiscomexEntryInstructionDetailsUserControl>(control);
				}
			}
		}

		public void TestMessageTabPageCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				var messageTabPage = brokerageControl.MainTabControl.SelectedTab;
				AssertEquals("label of MessagesTabPage", "Entries", messageTabPage.CaptionResourceString.Caption);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("label of MessagesTabPage", "Licenses", messageTabPage.CaptionResourceString.Caption);
			}
		}

		public void TestBrokerageAvailableTabsForImportLicense()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;

				CombineAssertions(() =>
				{
					AssertEquals("The available tabs count should be 14 ", 14, brokerageControl.MainTabControl.TabCount);
					int tabIndex = 0;
					AssertEquals("This tab should be DeclarationTabPage", brokerageControl.DeclarationTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be EntryInstructionDetailsTabPage", brokerageControl.EntryInstructionDetailsTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be InvoiceGroupingTabPage", brokerageControl.InvoiceGroupingTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be InvoicesTabPage", brokerageControl.InvoicesTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be InvoiceLinesTabPage", brokerageControl.InvoiceLinesTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be MessagesTabPage", brokerageControl.MessagesTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be MiscOptionsTabPage", brokerageControl.MiscOptionsTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be WorkflowTabPage", brokerageControl.WorkflowTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be BillingTabPage", "BillingTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
					AssertEquals("This tab should be AddressesTabPage", "AddressesTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
					AssertEquals("This tab should be DocDataTabPage", "DocDataTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
					AssertEquals("This tab should be eDocsTabPage", "eDocsTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
					AssertEquals("This tab should be BrokerageStmNoteTabPage", "BrokerageStmNoteTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
					AssertEquals("This tab should be EventTabPage", "EventTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
				});
			}
		}

		public void TestBrokerageAvailableTabsForLPCO()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;

				CombineAssertions(() =>
				{
					AssertEquals("The available tabs count should be 14 ", 14, brokerageControl.MainTabControl.TabCount);
					int tabIndex = 0;
					AssertEquals("This tab should be DeclarationTabPage", brokerageControl.DeclarationTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be EntryInstructionDetailsTabPage", brokerageControl.EntryInstructionDetailsTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be InvoiceGroupingTabPage", brokerageControl.InvoiceGroupingTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be InvoicesTabPage", brokerageControl.InvoicesTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be InvoiceLinesTabPage", brokerageControl.InvoiceLinesTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be MessagesTabPage", brokerageControl.MessagesTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be MiscOptionsTabPage", brokerageControl.MiscOptionsTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be WorkflowTabPage", brokerageControl.WorkflowTabPage, brokerageControl.MainTabControl.Controls[tabIndex++]);
					AssertEquals("This tab should be BillingTabPage", "BillingTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
					AssertEquals("This tab should be AddressesTabPage", "AddressesTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
					AssertEquals("This tab should be DocDataTabPage", "DocDataTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
					AssertEquals("This tab should be eDocsTabPage", "eDocsTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
					AssertEquals("This tab should be BrokerageStmNoteTabPage", "BrokerageStmNoteTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
					AssertEquals("This tab should be EventTabPage", "EventTabPage", brokerageControl.MainTabControl.Controls[tabIndex++].Name);
				});
			}
		}

		public void TestDefaultTabForRightTabControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				var customsDeclarationUserControl = brokerageControl.DeclarationUserControl as CustomsDeclarationUserControl;
				var rightTabControl = customsDeclarationUserControl.RightTabControl;

				AssertEquals("RightTabControl default tab should be 'OrdersTabPage'.", "OrdersTabPage", rightTabControl.SelectedTab.Name);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

				AssertEquals("RightTabControl default tab should be 'DocsTabPage'.", "DocsTabPage", rightTabControl.SelectedTab.Name);
			}
		}
	}
}
