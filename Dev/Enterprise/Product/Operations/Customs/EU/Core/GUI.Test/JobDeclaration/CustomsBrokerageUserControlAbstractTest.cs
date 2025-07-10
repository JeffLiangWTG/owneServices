using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestsSubclassesOf(typeof(CustomsBrokerageUserControl))]
	public abstract class CustomsBrokerageUserControlAbstractTest<T> : TestCaseWithFactory
		where T : CustomsBrokerageUserControl
	{
		public void TestJobDeclarationUserControl()
		{
			AssertEquals(JobDeclarationUserControlType, control.DeclarationUserControlForTesting.GetType());
		}

		public void TestSupplierHeaderUserControl_Import()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertSupplierHeaderUserControl(ImportSupplierHeaderUserControlType);
		}

		public void TestSupplierHeaderUserControl_Export()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertSupplierHeaderUserControl(ExportSupplierHeaderUserControlType);
		}

		public void TestInvoiceLinesUserControl_Import()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertInvoiceLinesUserControl(ImportInvoiceLineUserControlType);
		}

		public void TestInvoiceLinesUserControl_Export()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertInvoiceLinesUserControl(ExportInvoiceLineUserControlType);
		}

		public void TestMessageUserControl()
		{
			control.MainTabControl.SelectedTab = control.MessagesTabPage;
			AssertEquals(MessageUserControlType, control.MessageUserControl.GetType());
		}

		public void TestEntryInstructionDetailsUserControl()
		{
			if (control.EntryInstructionsTabVisibleForCountry)
			{
				control.MainTabControl.SelectedTab = control.EntryInstructionDetailsTabPage;
				AssertEquals(EntryInstructionDetailsUserControlType, control.CustomsEntryInstructionUserControl.GetType());
			}
			else
			{
				Assert("Entry Instruction Not visible", true);
			}
		}

		public void TestContainersUserControlType()
		{
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			control.MainTabControl.SelectedTab = control.ContainerTabPage;
			AssertEquals(ContainersUserControlType, control.ContainerUserControl.GetType());
		}

		public void TestDV1UserControl_Import()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.ZG_IsHighValueOvrd = true;
				control.MainTabControl.SelectedTab = control.DV1DetailsTabPage;
				if (!control.DV1DetailsTabPage.TabVisible)
				{
					Assert("DV1 is not used/supported in this country", true);
				}
				else
				{
					AssertEquals(DV1UserControlType, control.DV1DetailsTabPage.Controls.Find("DV1UserControl", false).FirstOrDefault().GetType());
				}
			}
		}

		protected abstract Type ImportSupplierHeaderUserControlType { get; }

		protected abstract Type ExportSupplierHeaderUserControlType { get; }

		protected abstract Type ImportInvoiceLineUserControlType { get; }

		protected abstract Type ExportInvoiceLineUserControlType { get; }

		protected virtual Type MiscOptionsUserControlType => typeof(MiscOptionsUserControl);

		protected abstract Type MessageUserControlType { get; }

		protected abstract Type JobDeclarationUserControlType { get; }

		protected abstract Type EntryInstructionDetailsUserControlType { get; }

		protected virtual Type DV1UserControlType => typeof(DV1UserControl);

		protected virtual Type ContainersUserControlType => typeof(CustomsCusContainersWithTrackingAndAdditionalSealUserControl);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			form = new ZForm(declaration);
			control = Activator.CreateInstance<T>();
			form.Controls.Add(control);
			form.Show();
			control.JobDeclaration = declaration;
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
			form.Dispose();
		}
		protected JobDeclaration declaration;
		protected CustomsBrokerageUserControl control;
		ZForm form;

		void AssertSupplierHeaderUserControl(Type expected)
		{
			control.MainTabControl.SelectedTab = control.InvoicesTabPage;
			AssertEquals(expected, control.SupplierHeaderUserControl.GetType());
		}

		protected void AssertInvoiceLinesUserControl(Type expected)
		{
			control.MainTabControl.SelectedTab = control.InvoiceLinesTabPage;
			AssertEquals(expected, control.InvoiceLinesUserControl.GetType());
		}
	}
}
