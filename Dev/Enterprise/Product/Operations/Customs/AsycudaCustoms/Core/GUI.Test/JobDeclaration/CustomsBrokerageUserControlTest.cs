using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestEntryInstructionsTabPage()
		{
			var declaration = Factory.New<Business.JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			using (var userControl = new CustomsBrokerageUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				using (var entryInstructionDetailsTabPage = userControl.EntryInstructionDetailsTabPage)
				{
					AssertEquals("EntryInstructionsTabPage is visible.", true, entryInstructionDetailsTabPage.TabRelevant);
				}
			}
		}

		public void TestSupplierHeaderUserControl()
		{
			var declaration = Factory.New<Business.JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			AssertSupplierHeaderUserControl(declaration, typeof(ImportSupplierHeaderUserControl));

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			AssertSupplierHeaderUserControl(declaration, typeof(ExportSupplierHeaderUserControl));
		}

		protected void AssertSupplierHeaderUserControl(Business.JobDeclaration declaration, Type expected)
		{
			using (var form = new ZForm(declaration))
			using (var brokerageControl = new CustomsBrokerageUserControlForTest())
			{
				form.Controls.Add(brokerageControl);
				form.Show();

				using (IDisposable control = brokerageControl.GetSupplierHeaderUserControl_Exposed())
				{
					AssertEquals(expected, control.GetType());
				}
			}
		}

		public void TestInvoiceLinesUserControl()
		{
			var declaration = Factory.New<Business.JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			AssertInvoiceLinesUserControl(declaration, typeof(BaseInvoiceLineUserControl));

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			AssertInvoiceLinesUserControl(declaration, typeof(ExportInvoiceLineUserControl));
		}

		protected void AssertInvoiceLinesUserControl(Business.JobDeclaration declaration, Type expectedType)
		{
			using (var form = new ZForm(declaration))
			using (var brokerageControl = new CustomsBrokerageUserControlForTest())
			{
				form.Controls.Add(brokerageControl);
				form.Show();

				using (var control = brokerageControl.GetInvoiceLinesUserControl_Exposed())
				{
					AssertEquals(expectedType, control.GetType());
				}
			}
		}

		public void TestLoadDynamicMiscOptionsPage()
		{
			var declaration = Factory.New<Business.JobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var brokerageControl = new CustomsBrokerageUserControlForTest())
			{
				form.Controls.Add(brokerageControl);
				form.Show();

				brokerageControl.JobDeclaration = declaration;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
				AssertNull("Base Misc Options User Control has Loaded", brokerageControl.MiscOptions);
				AssertNotNull("Dynamic Misc Options User Control not Loaded", brokerageControl.DynamicMiscOptions);
			}
		}

		public void TestMessageUserControl()
		{
			var declaration = Factory.New<Business.JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var brokerageControl = new CustomsBrokerageUserControlForTest())
			{
				form.Controls.Add(brokerageControl);
				form.Show();

				using (var control = brokerageControl.GetMessageUserControl_Exposed())
				{
					AssertEquals(typeof(CustomsEntryAndDiscardedMessagesUserControl), control.GetType());
				}
			}
		}

		public void TestComponentsVisibility()
		{
			using (var mainControl = new CustomsBrokerageUserControl())
			using (var invoiceGroupingTabPage = mainControl.InvoiceGroupingTabPage)
			{
				Assert("InvoiceGroupingTabPage is NOT visible.", !invoiceGroupingTabPage.TabRelevant);
			}
		}

		class CustomsBrokerageUserControlForTest : CustomsBrokerageUserControl
		{
			public Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl_Exposed() => GetSupplierHeaderUserControl();

			public Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl_Exposed() => GetInvoiceLinesUserControl();

			public BaseCustomsEntryUserControl GetMessageUserControl_Exposed() => GetMessageUserControl();
		}
	}
}
