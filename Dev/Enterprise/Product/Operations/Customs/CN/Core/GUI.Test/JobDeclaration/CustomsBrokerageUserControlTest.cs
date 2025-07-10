using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestSupplierHeaderUserControl()
		{
			var declaration = Factory.New<Business.JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			AssertSupplierHeaderUserControl(declaration, typeof(CustomsSupplierHeaderUserControl));
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			AssertSupplierHeaderUserControl(declaration, typeof(CustomsSupplierHeaderUserControl));
		}

		protected void AssertSupplierHeaderUserControl(Business.JobDeclaration declaration, Type expected)
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				using (var control = GetProtectedBrokerageControl(brokerageControl, "GetSupplierHeaderUserControl"))
				{
					AssertEquals(expected, control.GetType());
				}
			}
		}

		public void TestInvoiceLinesUserControl()
		{
			var declaration = Factory.New<Business.JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			AssertInvoiceLinesUserControl(declaration, typeof(InvoiceLineUserControl));
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			AssertInvoiceLinesUserControl(declaration, typeof(InvoiceLineUserControl));
		}

		protected void AssertInvoiceLinesUserControl(Business.JobDeclaration declaration, Type expectedType)
		{
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				using (var control = GetProtectedBrokerageControl(brokerageControl, "GetInvoiceLinesUserControl"))
				{
					AssertEquals(expectedType, control.GetType());
				}
			}
		}

		public void TestMessageUserControl()
		{
			using (var testForm = new JobDeclarationForm(Factory.New<Business.JobDeclaration>()))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				using (IDisposable control = GetProtectedBrokerageControl(brokerageControl, "GetMessageUserControl"))
				{
					AssertEquals(typeof(CustomsEntriesWithMessagesUserControl), control.GetType());
				}
			}
		}

		public void TestCustomsCusContainersUserControl()
		{
			using (var testForm = new JobDeclarationForm(Factory.New<Business.JobDeclaration>()))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				using (IDisposable control = GetProtectedBrokerageControl(brokerageControl, "GetContainerUserControl"))
				{
					AssertEquals(typeof(CustomsCusContainersUserControl), control.GetType());
				}
			}
		}

		public void TestContainerTabPageVisible()
		{
			var declaration = Factory.New<Business.JobDeclaration>();
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				Assert(brokerageControl.ContainerTabPage.TabRelevant);
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Rail;
				Assert(!brokerageControl.ContainerTabPage.TabRelevant);
				declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
				Assert(brokerageControl.ContainerTabPage.TabRelevant);
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Road;
				Assert(!brokerageControl.ContainerTabPage.TabRelevant);
				declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
				Assert(brokerageControl.ContainerTabPage.TabRelevant);
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
				Assert(brokerageControl.ContainerTabPage.TabRelevant);
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.FixedTransportInstallations;
				Assert(!brokerageControl.ContainerTabPage.TabRelevant);
			}
		}

		public void TestEntryInstructionUserControl()
		{
			using (var testForm = new JobDeclarationForm(Factory.New<Business.JobDeclaration>()))
			{
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				using (IDisposable control = GetProtectedBrokerageControl(brokerageControl, "GetEntryInstructionUserControl"))
				{
					AssertEquals(typeof(EntryInstructionUserControl), control.GetType());
				}
			}
		}

		static Control GetProtectedBrokerageControl(CustomsBrokerageUserControl brokerageControl, string methodName)
		{
			return (Control)typeof(CustomsBrokerageUserControl).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(brokerageControl, Array.Empty<object>());
		}
	}
}
