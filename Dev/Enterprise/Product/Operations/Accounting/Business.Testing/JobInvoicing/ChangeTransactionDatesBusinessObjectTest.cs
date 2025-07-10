using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ChangeTransactionDatesBusinessObject))]
	public class ChangeTransactionDatesBusinessObjectTest : ChangeTransactionDatesBusinessObjectBaseTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OperationsJobConfigurationCodes codes = new OperationsJobConfigurationCodes(null);
			return new ChangeTransactionDatesBusinessObject(JobInvoicingSecurity, Factory, codes);
		}

		protected SecurityCheckpoint JobInvoicingSecurity
		{
			get
			{
				if (JobInvoicingSecurity_innerValue == null)
				{
					ForwardingShipment shipment = TestObjectCreator.CreateShipment("S0001");
					JobInvoicingSecurity_innerValue = ((IJobInvoicingPlugIn)shipment).InvoicingSupporter.JobInvoicingSecurity;
				}
				return JobInvoicingSecurity_innerValue;
			}
		}
		SecurityCheckpoint JobInvoicingSecurity_innerValue;

		ChangeTransactionDatesBusinessObject TestBizo
		{
			get { return TestBizo_innerValue ?? (TestBizo_innerValue = (ChangeTransactionDatesBusinessObject)GetNewBusinessObject()); }
		}
		ChangeTransactionDatesBusinessObject TestBizo_innerValue;

		protected virtual SecurityCheckpoint ModifyTransactionDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.ModifyTransactionDate); }
		}

		protected virtual SecurityCheckpoint ModifyPostDateSecurity
		{
			get { return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.ModifyPostDate); }
		}

		#endregion

		protected BackDateInvoicesConfiguration BackDateInvoicesConfiguration_OverrideTransactionDateFalse
		{
			get
			{
				return new BackDateInvoicesConfiguration();
			}
		}

		protected BackDateInvoicesConfiguration BackDateInvoicesConfiguration_OverrideTransactionDateTrue
		{
			get
			{
				BackDateInvoicesConfiguration config = new BackDateInvoicesConfiguration();
				config.InvoiceDateConfigurationCollection[0].Override = true;
				config.InvoiceDateConfigurationCollection[0].Today = true;
				return config;
			}
		}

		public void TestTransactionDateReadOnly()
		{
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_OverrideTransactionDateFalse);
			ModifyTransactionDateSecurity.IsAllowed = false;
			Assert(TestBizo.InvoiceDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_OverrideTransactionDateFalse);
			ModifyTransactionDateSecurity.IsAllowed = true;
			Assert(TestBizo.InvoiceDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_OverrideTransactionDateTrue);
			ModifyTransactionDateSecurity.IsAllowed = false;
			Assert(TestBizo.InvoiceDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_OverrideTransactionDateTrue);
			ModifyTransactionDateSecurity.IsAllowed = true;
			Assert(!TestBizo.InvoiceDateInfo.ReadOnly);
		}

		public void TestPostDateReadOnly()
		{
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { OverridePostDate = false });
			ModifyPostDateSecurity.IsAllowed = false;
			Assert(TestBizo.PostDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { OverridePostDate = false });
			ModifyPostDateSecurity.IsAllowed = true;
			Assert(TestBizo.PostDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { OverridePostDate = true });
			ModifyPostDateSecurity.IsAllowed = false;
			Assert(TestBizo.PostDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { OverridePostDate = true });
			ModifyPostDateSecurity.IsAllowed = true;
			Assert(!TestBizo.PostDateInfo.ReadOnly);
		}

		public void TestPopulateValuesFrom()
		{
			ModifyPostDateSecurity.IsAllowed = true;

			TestBizo.InvoiceDate = ZDateTime.BrettsBirthday;
			TestBizo.PostDate = ZDateTime.BrettsBirthday;

			ChangeTransactionDatesBusinessObject otherBizO = (ChangeTransactionDatesBusinessObject)GetNewBusinessObject();

			otherBizO.InvoiceDate = ZDateTime.BrettsBirthday.AddDays(-5);
			otherBizO.PostDate = ZDateTime.BrettsBirthday.AddDays(-10);
			SetInvoiceDateReadOnly(true);
			SetPostDateReadOnly(true);

			TestBizo.PopulateValuesFrom(otherBizO);
			AssertEquals("Should be no changes to InvoiceDate", ZDateTime.BrettsBirthday, TestBizo.InvoiceDate);
			AssertEquals("Should be no changes to PostDate", ZDateTime.BrettsBirthday, TestBizo.PostDate);

			SetPostDateReadOnly(true);
			SetInvoiceDateReadOnly(false);

			TestBizo.PopulateValuesFrom(otherBizO);
			AssertEquals("New value should be set to InvoiceDate", ZDateTime.BrettsBirthday.AddDays(-5), TestBizo.InvoiceDate);
			AssertEquals("Should be no changes to PostDate", ZDateTime.BrettsBirthday, TestBizo.PostDate);

			TestBizo.InvoiceDate = ZDateTime.BrettsBirthday;
			SetInvoiceDateReadOnly(true);
			SetPostDateReadOnly(false);
			// Cannot make both properties editable using registry settings as below
			// ChangeTransactionDatesNotificationSubscriberGuiHelperTest.TestYesToAllPopulate... methods provide additional unit test coverage for this method 
		}

		void SetInvoiceDateReadOnly(bool readOnly)
		{
			if (readOnly)
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_OverrideTransactionDateFalse);
				Assert(TestBizo.InvoiceDateInfo.ReadOnly);
			}
			else
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BackDateInvoicesConfiguration_OverrideTransactionDateTrue);
				Assert(!TestBizo.InvoiceDateInfo.ReadOnly);
			}
		}

		void SetPostDateReadOnly(bool readOnly)
		{
			if (readOnly)
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { OverridePostDate = false });
				Assert(TestBizo.PostDateInfo.ReadOnly);
			}
			else
			{
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration() { OverridePostDate = true });
				Assert(!TestBizo.PostDateInfo.ReadOnly);
			}
		}

		public void TestRevenueRecognitionDatesReadOnly()
		{
			Assert("RevenueRecognitionDates should always be readonly", TestBizo.RevenueRecognitionDatesInfo.ReadOnly);
		}
	}
}
