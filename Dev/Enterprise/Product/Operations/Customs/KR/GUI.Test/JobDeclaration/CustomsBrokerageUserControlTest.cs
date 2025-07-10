using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestSupplierHeaderUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertSupplierHeaderUserControl(declaration, typeof(ImportSupplierHeaderUserControl));

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertSupplierHeaderUserControl(declaration, typeof(ExportSupplierHeaderUserControl));

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertSupplierHeaderUserControl(declaration, typeof(LocalExportSupplierHeaderUserControl));
		}

		void AssertSupplierHeaderUserControl(JobDeclaration declaration, Type expected)
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				using (IDisposable control = brokerageControl.SupplierHeaderUserControlExposed)
				{
					AssertEquals(expected, control.GetType());
				}
			}
		}

		public void TestCustomsPackingUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertCustomsPackingUserControl(declaration, typeof(ImportCustomsPackingUserControl));

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertCustomsPackingUserControl(declaration, typeof(BaseCustomsPackingUserControl));

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertCustomsPackingUserControl(declaration, typeof(BaseCustomsPackingUserControl));
		}

		void AssertCustomsPackingUserControl(JobDeclaration declaration, Type expected)
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.PackingTabPage;
				brokerageControl.LoadPackingTabPage();
				using (IDisposable control = brokerageControl.PackingTabPage.Controls[0])
				{
					AssertEquals(expected, control.GetType());
				}
			}
		}

		public void TestInvoiceLinesUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertInvoiceLinesUserControl(declaration, typeof(ImportInvoiceLineUserControl));

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertInvoiceLinesUserControl(declaration, typeof(ExportInvoiceLineUserControl));

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertInvoiceLinesUserControl(declaration, typeof(LocalExportInvoiceLineUserControl));
		}

		void AssertInvoiceLinesUserControl(JobDeclaration declaration, Type expectedType)
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				using (IDisposable control = brokerageControl.InvoiceLinesUserControlExposed)
				{
					AssertEquals(expectedType, control.GetType());
				}
			}
		}

		public void TestEntriesAndMessagesUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				Assert("The name is misleading. It just has a grid of Entries and Messages per Entry is shown", brokerageControl.MessageUserControl is Customs.GUI.ImportMessageUserControl);

				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				AssertEquals(typeof(ExportMessageUserControl), brokerageControl.MessageUserControl.GetType());

				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				AssertEquals(typeof(LocalExportMessageUserControl), brokerageControl.MessageUserControl.GetType());

				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				AssertEquals(typeof(ImportMessageUserControl), brokerageControl.MessageUserControl.GetType());
			}
		}

		public void TestNotShowContainerTabOnLoadOfLocalExportDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				Assert("Should show container tab for EXP SEA.", brokerageControl.MainTabControl.Contains(brokerageControl.ContainerTabPage));
				Assert("Should show container tab in invoice lines for EXP SEA.", brokerageControl.InvoiceLinesUserControl.LineDetailTabControl.Contains(brokerageControl.InvoiceLinesUserControl.ContainersTabPage));

				declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
				Assert("Should not show container tab for LEX.", !brokerageControl.MainTabControl.Contains(brokerageControl.ContainerTabPage));
				Assert("Should not show container tab in invoice lines for LEX.", !brokerageControl.InvoiceLinesUserControl.LineDetailTabControl.Contains(brokerageControl.InvoiceLinesUserControl.ContainersTabPage));
			}
		}

		public void TestMainTabControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertMainTabControls(declaration);
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertMainTabControls(declaration);
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertMainTabControls(declaration);
		}
		void AssertMainTabControls(JobDeclaration declaration)
		{
			using (var testForm = new BaseJobDeclarationFormTestClass(declaration))
			{
				var baseBrokerageControl = testForm.CustomsBrokerageUserControl;
				AssertNotNull(baseBrokerageControl.MainTabControl.FindSingleOrDefault<BaseDeclarationTabPage>("InvoiceGroupingTabPage"));
			}
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				AssertNull(brokerageControl.MainTabControl.FindSingleOrDefault<BaseDeclarationTabPage>("InvoiceGroupingTabPage"));
			}
		}

		public void TestEntryInstructionDetailsUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				AssertEquals(true, brokerageControl.EntryInstructionsTabVisibleForCountry);

				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				AssertEquals(false, brokerageControl.EntryInstructionsTabVisibleForCountry);

				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				AssertEquals(true, brokerageControl.EntryInstructionsTabVisibleForCountry);
			}
		}
	}

	sealed class JobDeclarationFormForTest : JobDeclarationForm
	{
		public JobDeclarationFormForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl()
		{
			return new CustomsBrokerageUserControlForTest();
		}
	}

	sealed class CustomsBrokerageUserControlForTest : CustomsBrokerageUserControl
	{
		public Customs.GUI.BaseInvoiceLineUserControl InvoiceLinesUserControlExposed => base.GetInvoiceLinesUserControl();
		public BaseCustomsSupplierHeaderUserControl SupplierHeaderUserControlExposed => base.GetSupplierHeaderUserControl();
		public BaseMiscOptionsUserControl MiscUserControlExposed => base.GetMiscOptionsUserControl();
	}
}
