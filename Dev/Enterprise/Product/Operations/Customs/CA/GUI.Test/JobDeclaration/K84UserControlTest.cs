using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class K84UserControlTest : TestCaseWithFactory
	{
		public void TestB3ScheduleContolVisibility()
		{
			CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions("Test display without schedule", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var brokerageUserControl = (LowValueShipmentsUserControl)testForm.CustomsBrokerageUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
					var scheduledB3Box = brokerageUserControl.K84TabPage.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					Assert(!scheduledB3Box.Visible);
				}
			});

			CombineAssertions("Test display with schedule B3 Message Time", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var testheader = testDeclaration.ActiveEntryHeaders.AddNew();
				testheader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testMessage = testheader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage.EM_HeldUntilDate = ZDateTime.UtcNow;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var brokerageUserControl = (LowValueShipmentsUserControl)testForm.CustomsBrokerageUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
					var scheduledB3Box = brokerageUserControl.K84TabPage.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					Assert(scheduledB3Box.Visible);
					AssertEquals("BackColorCheck", Color.Yellow, scheduledB3Box.DateTextBox.BackColor);
				}
			});

			CombineAssertions("Test display with Auto-Sending schedule", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var strategy = new DummyJobDeclarationB3SendingStrategy(testDeclaration);
				strategy.ShouldAutoSendB3MessageForTesting = true;
				strategy.ScheduledB3AutoSendingDateForTesting = new ZDateTime(2015, 5, 20);
				JobDeclarationB3SendingStrategy.StrategiesForTesting = new List<JobDeclarationB3SendingStrategy>();
				JobDeclarationB3SendingStrategy.StrategiesForTesting.Add(strategy);
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var brokerageUserControl = (LowValueShipmentsUserControl)testForm.CustomsBrokerageUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
					var scheduledB3Box = brokerageUserControl.K84TabPage.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					Assert(scheduledB3Box.Visible);
					AssertEquals("BackColorCheck", SystemColors.Control, scheduledB3Box.DateTextBox.BackColor);
					AssertEquals(new ZDateTime(2015, 5, 20).ToLongTimeString().ToUpper(), scheduledB3Box.Text);
				}
				JobDeclarationB3SendingStrategy.StrategiesForTesting = null;
			});

			CombineAssertions("Test display with schedule B3 Message Time", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var testheader = testDeclaration.ActiveEntryHeaders.AddNew();
				testheader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testMessage = testheader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage.EM_HeldUntilDate = new ZDateTime(2015, 5, 19);
				var strategy = new DummyJobDeclarationB3SendingStrategy(testDeclaration);
				strategy.ShouldAutoSendB3MessageForTesting = true;
				strategy.ScheduledB3AutoSendingDateForTesting = new ZDateTime(2015, 5, 20);
				JobDeclarationB3SendingStrategy.StrategiesForTesting = new List<JobDeclarationB3SendingStrategy>();
				JobDeclarationB3SendingStrategy.StrategiesForTesting.Add(strategy);
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var brokerageUserControl = (LowValueShipmentsUserControl)testForm.CustomsBrokerageUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
					var scheduledB3Box = brokerageUserControl.K84TabPage.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					Assert(scheduledB3Box.Visible);
					AssertEquals("BackColorCheck", Color.Yellow, scheduledB3Box.DateTextBox.BackColor);
					AssertEquals(testDeclaration.ScheduledB3MessageTime.ToLongTimeString().ToUpper(), scheduledB3Box.Text);
				}
			});

			CombineAssertions("Test display with Auto-Sending schedule", () =>
			{
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var testheader = testDeclaration.ActiveEntryHeaders.AddNew();
				testheader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var testMessage = testheader.Messages.AddNew();
				testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				testMessage.EM_HeldUntilDate = new ZDateTime(2015, 5, 21);
				var strategy = new DummyJobDeclarationB3SendingStrategy(testDeclaration);
				strategy.ShouldAutoSendB3MessageForTesting = true;
				strategy.ScheduledB3AutoSendingDateForTesting = new ZDateTime(2015, 5, 20);
				JobDeclarationB3SendingStrategy.StrategiesForTesting = new List<JobDeclarationB3SendingStrategy>();
				JobDeclarationB3SendingStrategy.StrategiesForTesting.Add(strategy);
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var brokerageUserControl = (LowValueShipmentsUserControl)testForm.CustomsBrokerageUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
					var scheduledB3Box = brokerageUserControl.K84TabPage.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
					Assert(scheduledB3Box.Visible);
					AssertEquals("BackColorCheck", SystemColors.Control, scheduledB3Box.DateTextBox.BackColor);
					AssertEquals(testDeclaration.ScheduledB3AutoSendingDate.ToLongTimeString().ToUpper(), scheduledB3Box.Text);
				}
				JobDeclarationB3SendingStrategy.StrategiesForTesting = null;
			});
		}

		public void TestLowValueShipmentLabel()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateTaxOrFee(Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 2500, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 2000m;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2000m;
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2015, 12, 25);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var line = entryHeader.MergedLines.AddNew();
			Factory.Save();

			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageUserControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
				var lowValueShipmentLabel = brokerageUserControl.K84TabPage.Controls.Find("LowValueShipmentLabel", true)[0];
				Assert(lowValueShipmentLabel.Visible);

				line.CL_CustomsValue = 3000m;
				entryHeader.ResetTotalsAndCachedValues();
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
				Assert(!lowValueShipmentLabel.Visible);
			}
		}

		public void TestCA_OGDStatusDescriptionTextBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageUserControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
				var statusDescriptionTextBox = brokerageUserControl.K84TabPage.Controls.Find("CA_OGDStatusDescriptionTextBox", true)[0] as ZArchitecture.ZTextBox;
				Assert(statusDescriptionTextBox.Visible);

				declaration.CA_ServiceOption = ServiceOptions.Codes.IID;
				Assert(!statusDescriptionTextBox.Visible);
				AssertEquals("Caption", "OGD Status", statusDescriptionTextBox.CaptionResourceString.Caption);
				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				AssertEquals("Caption", "PGA Status", statusDescriptionTextBox.CaptionResourceString.Caption);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert(!statusDescriptionTextBox.Visible);
			}
		}

		public void TestB3AcceptedDateEditVisibility()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var testForm = new JobDeclarationForm(testDeclaration))
			{
				testForm.Show();
				var brokerageUserControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
				var b3AcceptedDateEdit = brokerageUserControl.K84TabPage.Controls.Find("B3AcceptedDateEdit", true)[0] as ZDateEdit;
				Assert(!b3AcceptedDateEdit.Visible);
			}
			var entryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			using (var testForm = new JobDeclarationForm(testDeclaration))
			{
				testForm.Show();
				var brokerageUserControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
				var b3AcceptedDateEdit = brokerageUserControl.K84TabPage.Controls.Find("B3AcceptedDateEdit", true)[0] as ZDateEdit;
				Assert(b3AcceptedDateEdit.Visible);
			}

			var testMessage = testDeclaration.B3EntryHeader.Messages.AddNew();
			testMessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testMessage.EM_Status = EDIMessage.Status.Queued;
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage.EM_HeldUntilDate = ZDateTime.UtcNow;
			using (var testForm = new JobDeclarationForm(testDeclaration))
			{
				testForm.Show();
				var brokerageUserControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
				var scheduledB3Box = brokerageUserControl.K84TabPage.Controls.Find("ScheduledB3DateEdit", true)[0] as ZDateEdit;
				Assert(scheduledB3Box.Visible);
				var b3AcceptedDateEdit = brokerageUserControl.K84TabPage.Controls.Find("B3AcceptedDateEdit", true)[0] as ZDateEdit;
				Assert(!b3AcceptedDateEdit.Visible);
			}
		}

		public void TestWarehouseTransactionStatusDescriptionTextBoxVisibility()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
				var warehouseTransactionStatusDescriptionTextBox = brokerageUserControl.K84TabPage.Controls.Find("WarehouseTransactionStatusDescriptionTextBox", true)[0] as ZArchitecture.ZTextBox;
				Assert("WarehouseTransactionStatusDescriptionTextBox.Visible", !warehouseTransactionStatusDescriptionTextBox.Visible);

				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
				declaration.JE_OH_Importer = helper.Importer.PK;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
				Assert("WarehouseTransactionStatusDescriptionTextBox.Visible", warehouseTransactionStatusDescriptionTextBox.Visible);
			}
		}
	}
}
