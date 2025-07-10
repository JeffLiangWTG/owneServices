using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using DeclarationValueChangedAnnouncer = Enterprise.Customs.CA.Business.DeclarationValueChangedAnnouncer;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestChangeControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Invoice Group Tab Page Visible", false, brokerageControl.InvoiceGroupingTabPage.TabVisible);
				AssertEquals("Packing Tab Page Visible", false, brokerageControl.PackingTabPage.TabVisible);
				AssertEquals("Messages Tab Page Visible", true, brokerageControl.MessagesTabPage.TabVisible);
				AssertEquals("Invoices Tab Page Visible", true, brokerageControl.InvoicesTabPage.TabVisible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Invoice Group Tab Page Visible", true, brokerageControl.InvoiceGroupingTabPage.TabVisible);
				AssertEquals("Packing Tab Page Visible", true, brokerageControl.PackingTabPage.TabVisible);
				AssertEquals("Messages Tab Page Visible", true, brokerageControl.MessagesTabPage.TabVisible);
				AssertEquals("Invoices Tab Page Visible", true, brokerageControl.InvoicesTabPage.TabVisible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
				AssertEquals("Invoice Group Tab Page Visible", true, brokerageControl.InvoiceGroupingTabPage.TabVisible);
				AssertEquals("Packing Tab Page Visible", true, brokerageControl.PackingTabPage.TabVisible);
				AssertEquals("Messages Tab Page Visible", false, brokerageControl.MessagesTabPage.TabVisible);
				AssertEquals("Invoices Tab Page Visible", true, brokerageControl.InvoicesTabPage.TabVisible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				AssertEquals("Invoice Group Tab Page Visible", false, brokerageControl.InvoiceGroupingTabPage.TabVisible);
				AssertEquals("Packing Tab Page Visible", true, brokerageControl.PackingTabPage.TabVisible);
				AssertEquals("Messages Tab Page Visible", true, brokerageControl.MessagesTabPage.TabVisible);
				AssertEquals("Invoices Tab Page Visible", false, brokerageControl.InvoicesTabPage.TabVisible);
			}
		}

		public void TestSupplierHeaderUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertSupplierHeaderUserControl(declaration, typeof(CAImportSupplierHeaderUserControl));

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertSupplierHeaderUserControl(declaration, typeof(CAExportSupplierHeaderUserControl));
		}

		public void TestInvoiceLinesUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertInvoiceLinesUserControl(declaration, typeof(CAImportInvoiceLineUserControl));

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertInvoiceLinesUserControl(declaration, typeof(CAExportInvoiceLineUserControl));
		}

		public void TestMiscOptionsUserControl()
		{
			using (var form = new JobDeclarationForm(Factory.New<JobDeclaration>()))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
				AssertEquals(typeof(CAMiscOptionsUserControl), brokerageControl.MiscOptions.GetType());
			}
		}

		public void TestMessageUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				AssertEquals("MessageUserControl", typeof(ExportMessagesUserControl), brokerageControl.MessageUserControl.GetType());
			}
		}

		public void TestK84UserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.K84TabPage;
				AssertEquals("K84UserControl", typeof(K84UserControl), brokerageControl.K84UserControl.GetType());
			}
		}

		public void TestCargoControlNumbersControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.K84TabPage;
				Assert(string.Format("Cargo Control Numbers tab does not visible"), brokerageControl.K84UserControl.CCNTabPage.TabVisible);
			}
		}

		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
			}

			var propertyInfoStorageField = declaration.Factory.GetType().GetField("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
			var propertyInfoStorage = propertyInfoStorageField.GetValue(declaration.Factory);

			var valueChangedDictionaryProperty = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var valueChangedDictionary = valueChangedDictionaryProperty.GetValue(propertyInfoStorage);

			var dictionaryField = valueChangedDictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var dictionary = (Dictionary<string, Dictionary<BusinessObject, EventHandler>>)dictionaryField.GetValue(valueChangedDictionary);

			foreach (var dictionaryOfSubcriber in dictionary)
			{
				foreach (var subcriber in dictionaryOfSubcriber.Value)
				{
					var subcriberTarget = subcriber.Value.Target;
					if (subcriberTarget is DeclarationValueChangedAnnouncer)
					{
						var onValueChangedField = subcriberTarget.GetType().BaseType.GetField("OnValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
						var onValueChanged = onValueChangedField.GetValue(subcriberTarget);
						var onValueChangedTarget = ((EventHandler)onValueChanged).Target;
						Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is CustomsBrokerageUserControl));
					}
					else
					{
						Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is CustomsBrokerageUserControl));
					}
				}
			}
		}

		void AssertSupplierHeaderUserControl(JobDeclaration declaration, Type expected)
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(expected, brokerageControl.SupplierHeaderUserControl.GetType());
			}
		}

		void AssertInvoiceLinesUserControl(JobDeclaration declaration, Type expectedType)
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(expectedType, brokerageControl.InvoiceLinesUserControl.GetType());
			}
		}
	}
}
