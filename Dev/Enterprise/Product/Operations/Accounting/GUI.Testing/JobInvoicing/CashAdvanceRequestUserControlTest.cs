using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing
{
	internal class CashAdvanceRequestUserControlTest : TestCaseWithFactory
	{
		public void TestCashAdvanceHeaderGridColumns()
		{
			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);
				Assert(headerGrid.ReadOnly);

				var visibleColumnNameList = new List<string>(new string[] { "CAH_Ledger", "OrganizationCode", "CAH_RX_NKTransactionCurrency", "CAH_OSAmount", "CAH_OSPaidAmount", "CAH_OSOutstandingAmount", "StatusDescription", "CAH_RequestReferenceNumber" });
				var optionalColumnNameList = new List<string>(new string[] { "OrganizationName" });
				var columnStyles = headerGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals($"column count should be {visibleColumnNameList.Count + optionalColumnNameList.Count}", visibleColumnNameList.Count + optionalColumnNameList.Count, columnStyles.Count());

				foreach (var columnName in visibleColumnNameList)
				{
					var columnInfo = columnStyles.FirstOrDefault(s => s.ColumnName == columnName);
					AssertNotNull($"{columnName}", columnInfo);
					AssertEquals($"The column '{columnInfo.ColumnName}' should be visible", true, columnInfo.IsVisible);
				}
				foreach (var columnName in optionalColumnNameList)
				{
					var columnInfo = columnStyles.FirstOrDefault(s => s.ColumnName == columnName);
					AssertNotNull($"{columnName}", columnInfo);
					AssertEquals($"The column '{columnInfo.ColumnName}' should not be visible", false, columnInfo.IsVisible);
				}
			}
		}

		public void TestCashAdvanceLineGridColumns()
		{
			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				var lineGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("lineGrid");
				AssertNotNull("lineGrid", lineGrid);
				Assert(lineGrid.ReadOnly);

				var visibleColumnNameList = new List<string>(new string[] { "RelatedChargeCode", "OrganizationCode", "Currency", "CAL_OSAmount", "CAL_OSPaidAmount", "CAL_OSOutstandingAmount", "StatusDescription" });
				var optionalColumnNameList = new List<string>(new string[] { "OrganizationName", "CAL_LocalAmount", "CAL_LocalPaidAmount" });
				var columnStyles = lineGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals("column count should be 10", 10, columnStyles.Count());

				foreach (var columnName in visibleColumnNameList)
				{
					var columnInfo = columnStyles.FirstOrDefault(s => s.ColumnName == columnName);
					AssertNotNull($"{columnName}", columnInfo);
					AssertEquals($"The column '{columnInfo.ColumnName}' should be visible", true, columnInfo.IsVisible);
				}
				foreach (var columnName in optionalColumnNameList)
				{
					var columnInfo = columnStyles.FirstOrDefault(s => s.ColumnName == columnName);
					AssertNotNull($"{columnName}", columnInfo);
					AssertEquals($"The column '{columnInfo.ColumnName}' should not be visible", false, columnInfo.IsVisible);
				}
			}
		}

		public void TestCashAdvanceHeaderAndLineGrids()
		{
			var shipment = ObjectCreator.CreateShipment("S00001002");
			var job = ObjectCreator.CreateJob(shipment);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, sellCurrency: ObjectCreator.AUD, osSellAmt: 100m, debtor: ObjectCreator.AALSHI);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, sellCurrency: ObjectCreator.AUD, osSellAmt: 200m, debtor: ObjectCreator.ABIGAS);
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, sellCurrency: ObjectCreator.AUD, osSellAmt: 300m, debtor: ObjectCreator.ABIGAS);
			Factory.Save();

			var cashAdvanceHeader1 = ObjectCreator.CreateCashAdvanceRequestHeader(job.PK, ObjectCreator.AALSHI.PK, LedgerTypes.AccountsReceivable, 100m, 100m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			var cashAdvanceLine1ForHeader1 = ObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader1.PK, 100m, 100m, CashAdvanceStatusCodes.RequestLine.Requested);
			charge1.JR_CAL_ARLine = cashAdvanceLine1ForHeader1.PK;
			cashAdvanceHeader1.Lines.Add(cashAdvanceLine1ForHeader1);

			var cashAdvanceHeader2 = ObjectCreator.CreateCashAdvanceRequestHeader(job.PK, ObjectCreator.ABIGAS.PK, LedgerTypes.AccountsReceivable, 500m, 500m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			var cashAdvanceLine1ForHeader2 = ObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader2.PK, 200m, 200m, CashAdvanceStatusCodes.RequestLine.Requested);
			charge2.JR_CAL_ARLine = cashAdvanceLine1ForHeader2.PK;
			cashAdvanceHeader2.Lines.Add(cashAdvanceLine1ForHeader2);

			var cashAdvanceLine2ForHeader2 = ObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader2.PK, 300m, 300m, CashAdvanceStatusCodes.RequestLine.Requested);
			charge3.JR_CAL_ARLine = cashAdvanceLine2ForHeader2.PK;
			cashAdvanceHeader2.Lines.Add(cashAdvanceLine2ForHeader2);
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);
				var lineGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("lineGrid");
				AssertNotNull("lineGrid", lineGrid);

				headerGrid.Select(0);
				AssertEquals(cashAdvanceHeader1.PK, headerGrid.SelectedElements[0].PK);

				lineGrid.SelectAllElements();
				AssertEquals("There should be one row in line grid.", 1, lineGrid.SelectedElements.Length);
				AssertEquals(cashAdvanceLine1ForHeader1.PK, lineGrid.SelectedElements[0].PK);

				headerGrid.UnSelectAll();
				headerGrid.Select(1);
				headerGrid.CurrentRowIndex = 1;
				AssertEquals(cashAdvanceHeader2.PK, headerGrid.SelectedElements[0].PK);

				lineGrid.SelectAllElements();
				AssertEquals("There should be two rows in line grid.", 2, lineGrid.SelectedElements.Length);
				AssertContainsExactElementsInAnyOrder(new[] { cashAdvanceLine1ForHeader2.PK, cashAdvanceLine2ForHeader2.PK }, lineGrid.SelectedElements.Select(x => x.PK));
			}
		}

		public void TestVisibilityOfMarkAsPaidUnpaidButtons()
		{
			foreach (var regValuePair in new (bool enableARCashAdvanceFunctionality, bool allowManualSettingOfARCashAdvanceRequestStatusToPaid, bool enableAPCashAdvanceFunctionality, bool allowManualSettingOfAPCashAdvanceRequestStatusToPaid)[]
			{
				(false, false, false, false), (false, true, false, false), (true, false, false, false), (true, true, false, false),
				(false, false, true, false), (false, true, true, false), (true, false, true, false), (true, true, true, false),
				(false, false, true, true), (false, true, true, true), (true, false, true, true), (true, true, true, true),
				(false, false, false, true), (false, true, false, true), (true, false, false, true), (true, true, false, true),
			})
			{
				using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValuePair.enableARCashAdvanceFunctionality))
				using (AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValuePair.enableAPCashAdvanceFunctionality))
				using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValuePair.allowManualSettingOfARCashAdvanceRequestStatusToPaid))
				using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValuePair.allowManualSettingOfAPCashAdvanceRequestStatusToPaid))
				using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
				using (var form = new ZForm())
				{
					cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
					var expectedValue = (regValuePair.enableARCashAdvanceFunctionality && regValuePair.allowManualSettingOfARCashAdvanceRequestStatusToPaid) || (regValuePair.enableAPCashAdvanceFunctionality && regValuePair.allowManualSettingOfAPCashAdvanceRequestStatusToPaid);

					var markAsPaidButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("MarkAsPaidButton");
					AssertNotNull("MarkAsPaidButton", markAsPaidButton);
					AssertEquals("Mark as Paid", markAsPaidButton.CaptionResourceString.Caption);
					AssertEquals(expectedValue, markAsPaidButton.Visible);

					var markAsUnpaidButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("MarkAsUnpaidButton");
					AssertNotNull("MarkAsUnpaidButton", markAsUnpaidButton);
					AssertEquals("Mark as Unpaid", markAsUnpaidButton.CaptionResourceString.Caption);
					AssertEquals(expectedValue, markAsUnpaidButton.Visible);
				}
			}
		}

		public void TestVisibilityOfPrintButton()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipmentJob = ObjectCreator.CreateJob("S00001000", null, 0, null, 0);
			shipmentJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipmentJob.JH_ParentID = shipment.PK;
			var charge1 = ObjectCreator.CreateCharge(shipmentJob, ObjectCreator.CC1, 200M, 200M);
			CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			Factory.Save();
			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(shipmentJob.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				var printButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("printButton");
				AssertNotNull(printButton);
				Assert(printButton.Visible);
			}

			var declaration = ObjectCreator.CreateDeclaration("B001");
			var declarationJob = ObjectCreator.CreateJob("B001", null, 0, null, 0);
			declarationJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			declarationJob.JH_ParentID = declaration.PK;
			ObjectCreator.CreateCashAdvanceRequestHeader(declarationJob.PK, ObjectCreator.AALSHI.PK, LedgerTypes.AccountsReceivable, 200m, 200m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			Factory.Save();
			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(declarationJob.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				var printButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("printButton");
				AssertNotNull(printButton);
				Assert(printButton.Visible);
			}

			var dummyjob = SetupCashAdvanceObjects();
			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(dummyjob.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				var printButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("printButton");
				AssertNotNull(printButton);
				AssertEquals(DummyBizoSchema.Constants.Prefix, dummyjob.JH_ParentTableCode);
				Assert("expect print button always visible", printButton.Visible);
			}
		}

		public void TestPrintCashAdvanceRequests()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = ObjectCreator.CreateJob("S00001000", null, 0, null, 0);
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			Factory.Save();

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				var printButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("printButton");
				printButton.PerformClick();
				AssertEquals("Please select a request or requests before printing.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				headerGrid.SelectAllElements();
				printButton.PerformClick();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintCashAdvanceRequestsForInactiveOrg()
		{
			var shipment = ObjectCreator.CreateShipment("S00001002");
			var job = ObjectCreator.CreateJob(shipment);
			ObjectCreator.CreateCharge(job, ObjectCreator.CC1, sellCurrency: ObjectCreator.AUD, osSellAmt: 100m, debtor: ObjectCreator.AALSHI);
			ObjectCreator.AALSHI.OH_IsActive = false;
			Factory.Save();

			var header = ObjectCreator.CreateCashAdvanceRequestHeader(job.PK, ObjectCreator.AALSHI.PK, LedgerTypes.AccountsReceivable, 100m, 100m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			header.CAH_RequestReferenceNumber = "00001001";
			Factory.Save();

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				var printButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("printButton");
				printButton.PerformClick();
				AssertEquals("Please select a request or requests before printing.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				headerGrid.SelectAllElements();
				printButton.PerformClick();
				AssertEquals(@"The following Advance Payment request(s) cannot be printed
Advance Payment request 00001001 cannot be printed because it is for an inactive organization
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintCancelledCashAdvanceRequests()
		{
			var shipment = ObjectCreator.CreateShipment("S00001002");
			var job = ObjectCreator.CreateJob(shipment);
			ObjectCreator.CreateCharge(job, ObjectCreator.CC1, sellCurrency: ObjectCreator.AUD, osSellAmt: 100m, debtor: ObjectCreator.AALSHI);
			Factory.Save();

			var header = ObjectCreator.CreateCashAdvanceRequestHeader(job.PK, ObjectCreator.AALSHI.PK, LedgerTypes.AccountsReceivable, 100m, 100m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Cancelled);
			header.CAH_RequestReferenceNumber = "00001001";
			Factory.Save();

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				var printButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("printButton");
				printButton.PerformClick();
				AssertEquals("Please select a request or requests before printing.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				headerGrid.SelectAllElements();
				printButton.PerformClick();
				AssertEquals(@"The following Advance Payment request(s) cannot be printed
Advance Payment request 00001001 cannot be printed because it is canceled
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMarkingCashAdvanceHeadersAsPaid_NoSecurityRight()
		{
			ObjectCreator.ResetSecurityCore();
			var job = SetupCashAdvanceObjects();

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				Env.Security.GetInvoicingSecurityCheckPoint(cashAdvancecontrol.PluginSecurity, SecurityCore.MarkARCashAdvancePaid).IsAllowed = false;

				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				headerGrid.Select(0);
				AssertEquals(job.CashAdvanceRequests[0].PK, headerGrid.SelectedElements[0].PK);

				var markAsPaidButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("MarkAsPaidButton");
				AssertNotNull("MarkAsPaidButton", markAsPaidButton);

				markAsPaidButton.PerformClick();

				AssertMultilineASCIIEquals("LastMessage", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Advance Payments -> Allow Mark AR Advance Payment Request as Paid", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestMarkingCashAdvanceHeadersAsPaid_HasSecurityRight()
		{
			var job = SetupCashAdvanceObjects();
			job.CashAdvanceRequests.Sort(nameof(AccCashAdvanceRequestHeader.CAH_RequestReferenceNumber));
			foreach (AccCashAdvanceRequestLine line in job.CashAdvanceRequests[0].Lines)
			{
				line.Cancel();
			}
			Factory.Save();

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				Env.Security.GetInvoicingSecurityCheckPoint(cashAdvancecontrol.PluginSecurity, SecurityCore.MarkARCashAdvancePaid).IsAllowed = true;
				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				headerGrid.SelectAllElements();
				var markAsPaidButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("MarkAsPaidButton");
				AssertNotNull("MarkAsPaidButton", markAsPaidButton);

				markAsPaidButton.PerformClick();

				AssertMultilineASCIIEquals("LastMessage", @"Following Advance Payment requests are marked as Paid-
000002

Following Advance Payment requests could not be marked as Paid.
000001-Advance Payment is not in requested status.

", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals("First CAH", "CAN", job.CashAdvanceRequests[0].CAH_Status);
				AssertEquals("Second CAH", "PAI", job.CashAdvanceRequests[1].CAH_Status);
			}
		}

		public void TestMarkingCashAdvanceHeadersAsPaid_NothingSelected()
		{
			var job = SetupCashAdvanceObjects();
			job.CashAdvanceRequests.Sort(nameof(AccCashAdvanceRequestHeader.CAH_RequestReferenceNumber));
			foreach (AccCashAdvanceRequestLine line in job.CashAdvanceRequests[0].Lines)
			{
				line.Cancel();
			}
			job.CashAdvanceRequests[1].MarkAsPaid();
			Factory.Save();

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				Env.Security.GetInvoicingSecurityCheckPoint(cashAdvancecontrol.PluginSecurity, SecurityCore.MarkARCashAdvancePaid).IsAllowed = true;
				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				var markAsPaidButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("MarkAsPaidButton");
				AssertNotNull("MarkAsPaidButton", markAsPaidButton);
				markAsPaidButton.PerformClick();

				AssertMultilineASCIIEquals("LastMessage", "Please select at least one Advance Payment request to mark as Paid.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestMarkingCashAdvanceHeadersAsUnpaid_NoSecurityRight()
		{
			ObjectCreator.ResetSecurityCore();
			var job = SetupCashAdvanceObjects();

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				Env.Security.GetInvoicingSecurityCheckPoint(cashAdvancecontrol.PluginSecurity, SecurityCore.MarkARCashAdvanceUnPaid).IsAllowed = false;

				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				headerGrid.Select(0);
				AssertEquals(job.CashAdvanceRequests[0].PK, headerGrid.SelectedElements[0].PK);

				var markAsUnpaidButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("MarkAsUnpaidButton");
				AssertNotNull("MarkAsUnpaidButton", markAsUnpaidButton);

				markAsUnpaidButton.PerformClick();

				AssertMultilineASCIIEquals("LastMessage", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Advance Payments -> Allow Mark AR Advance Payment Request Not Paid", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestMarkingCashAdvanceHeadersAsUnpaid_HasSecurityRight()
		{
			var job = SetupCashAdvanceObjects();
			job.CashAdvanceRequests.Sort(nameof(AccCashAdvanceRequestHeader.CAH_RequestReferenceNumber));
			foreach (AccCashAdvanceRequestLine line in job.CashAdvanceRequests[0].Lines)
			{
				line.Cancel();
			}
			job.CashAdvanceRequests[1].MarkAsPaid();
			Factory.Save();

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				Env.Security.GetInvoicingSecurityCheckPoint(cashAdvancecontrol.PluginSecurity, SecurityCore.MarkARCashAdvancePaid).IsAllowed = true;
				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				headerGrid.SelectAllElements();
				var markAsUnpaidButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("MarkAsUnpaidButton");
				AssertNotNull("MarkAsUnpaidButton", markAsUnpaidButton);

				markAsUnpaidButton.PerformClick();

				AssertMultilineASCIIEquals("LastMessage", @"Following Advance Payment requests are marked as Unpaid-
000002
Following Advance Payment requests could not be marked as Unpaid.
000001-Advance Payment is not in paid status.

", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals("First CAH", "CAN", job.CashAdvanceRequests[0].CAH_Status);
				AssertEquals("Second CAH", "REQ", job.CashAdvanceRequests[1].CAH_Status);
			}
		}

		public void TestMarkingCashAdvanceHeadersAsPaid_Concurrency()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_ARLine = cal3.PK;
			cah1.CAH_RequestReferenceNumber = "000001";
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var cahInFactory1 = factory1.Load<AccCashAdvanceRequestHeader>(new ZQuery(AccCashAdvanceRequestHeaderSchema.PK, job.CashAdvanceRequests.Select(c => c.PK).ToArray()));
			var cahInFactory2 = factory2.Load<AccCashAdvanceRequestHeader>(new ZQuery(AccCashAdvanceRequestHeaderSchema.PK, job.CashAdvanceRequests.Select(c => c.PK).ToArray()));
			var cahCollection = new AccCashAdvanceRequestHeaderCollection(factory2);
			cahCollection.AddRange(cahInFactory2);

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(cahCollection);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				Env.Security.GetInvoicingSecurityCheckPoint(cashAdvancecontrol.PluginSecurity, SecurityCore.MarkARCashAdvancePaid).IsAllowed = true;
				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				headerGrid.SelectAllElements();
				var markAsPaidButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("MarkAsPaidButton");
				AssertNotNull("MarkAsPaidButton", markAsPaidButton);

				foreach (AccCashAdvanceRequestHeader header in cahInFactory1)
				{
					header.MarkAsPaid();
				}
				factory1.Save();
				markAsPaidButton.PerformClick();

				AssertStartsWith("expect warning message", "Following Advance Payment requests could not be marked as Paid", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestCancelCashAdvanceHeaders_Concurrency()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_ARLine = cal3.PK;
			cah1.CAH_RequestReferenceNumber = "000001";
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var cahInFactory1 = factory1.Load<AccCashAdvanceRequestHeader>(new ZQuery(AccCashAdvanceRequestHeaderSchema.PK, job.CashAdvanceRequests.Select(c => c.PK).ToArray()));
			var cahInFactory2 = factory2.Load<AccCashAdvanceRequestHeader>(new ZQuery(AccCashAdvanceRequestHeaderSchema.PK, job.CashAdvanceRequests.Select(c => c.PK).ToArray()));
			var cahCollection = new AccCashAdvanceRequestHeaderCollection(factory2);
			cahCollection.AddRange(cahInFactory2);

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(cahCollection);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				Env.Security.GetInvoicingSecurityCheckPoint(cashAdvancecontrol.PluginSecurity, SecurityCore.MarkARCashAdvancePaid).IsAllowed = true;
				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				headerGrid.SelectAllElements();
				var cancelButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("CancelButton");
				AssertNotNull("CancelButton", cancelButton);

				foreach (AccCashAdvanceRequestHeader header in cahInFactory1)
				{
					header.CancelRequest();
				}
				factory1.Save();
				cancelButton.PerformClick();

				AssertStartsWith("expect warning message", "Following Advance Payment requests could not be marked as Cancelled", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestMarkingCashAdvanceHeadersAsUnpaid_NothingSelected()
		{
			var job = SetupCashAdvanceObjects();
			job.CashAdvanceRequests.Sort(nameof(AccCashAdvanceRequestHeader.CAH_RequestReferenceNumber));
			foreach (AccCashAdvanceRequestLine line in job.CashAdvanceRequests[0].Lines)
			{
				line.Cancel();
			}
			job.CashAdvanceRequests[1].MarkAsPaid();
			Factory.Save();

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				Env.Security.GetInvoicingSecurityCheckPoint(cashAdvancecontrol.PluginSecurity, SecurityCore.MarkARCashAdvancePaid).IsAllowed = true;
				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				var markAsUnpaidButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("MarkAsUnpaidButton");
				AssertNotNull("MarkAsUnpaidButton", markAsUnpaidButton);
				markAsUnpaidButton.PerformClick();

				AssertMultilineASCIIEquals("LastMessage", "Please select at least one Advance Payment request to mark as Unpaid.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		Job SetupCashAdvanceObjects()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_ARLine = cal3.PK;

			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");

			cah1.CAH_RequestReferenceNumber = "000001";
			cah4.CAH_RequestReferenceNumber = "000002";

			Factory.Save();
			return job;
		}

		(AccCashAdvanceRequestHeader cah, AccCashAdvanceRequestLine cal) CreateCashAdvanceRequest(Charge charge, ZDecimal localAmount, ZDecimal osAmount, ZString status, ZString currency)
		{
			var cah = ObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, ObjectCreator.Debtor, LedgerTypes.AccountsReceivable, localAmount, osAmount, currency);
			var cal = ObjectCreator.CreateCashAdvanceRequestLine(cah, localAmount, osAmount);
			charge.JR_CAL_ARLine = cal.PK;
			cal.CAL_Status = status;
			return (cah, cal);
		}

		public void TestCancelButton()
		{
			var shipment = ObjectCreator.CreateShipment("S00001002");
			var job = ObjectCreator.CreateJob(shipment);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, sellCurrency: ObjectCreator.AUD, osSellAmt: 100m, debtor: ObjectCreator.AALSHI);
			Factory.Save();

			var cashAdvanceHeader1 = ObjectCreator.CreateCashAdvanceRequestHeader(job.PK, ObjectCreator.AALSHI.PK, LedgerTypes.AccountsReceivable, 100m, 100m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			var cashAdvanceLine1ForHeader1 = ObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader1.PK, 100m, 100m, CashAdvanceStatusCodes.RequestLine.Requested);
			charge1.JR_CAL_ARLine = cashAdvanceLine1ForHeader1.PK;
			cashAdvanceHeader1.Lines.Add(cashAdvanceLine1ForHeader1);
			Factory.Save();
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cashAdvanceHeader1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, cashAdvanceLine1ForHeader1.CAL_Status);

			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				var cancelButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("CancelButton");
				AssertNotNull("cancelButton", cancelButton);
				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				headerGrid.UnSelectAll();
				AssertEquals(0, headerGrid.SelectedElements.Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				cancelButton.PerformClick();
				AssertEquals("Please select an Advance Payment Request to perform this action.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cashAdvanceHeader1.CAH_Status);
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, cashAdvanceLine1ForHeader1.CAL_Status);

				headerGrid.Select(0);
				AssertEquals(cashAdvanceHeader1.PK, headerGrid.SelectedElements[0].PK);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				cancelButton.PerformClick();
				AssertStartsWith("expect success message", "Following Advance Payment requests are marked as Cancelled", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Cancelled, cashAdvanceHeader1.CAH_Status);
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Cancelled, cashAdvanceLine1ForHeader1.CAL_Status);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				cancelButton.PerformClick();
				AssertStartsWith("expect warning message", "Following Advance Payment requests could not be marked as Cancelled", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Cancelled, cashAdvanceHeader1.CAH_Status);
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Cancelled, cashAdvanceLine1ForHeader1.CAL_Status);
			}
		}

		public void TestCancelRequestWhenUserDoesNotHaveSecurityRight()
		{
			ObjectCreator.ResetSecurityCore();
			var job = SetupCashAdvanceObjects();

			using (var cashAdvancecontrol = new CashAdvanceRequestUserControl())
			using (var form = new ZForm())
			{
				cashAdvancecontrol.PluginSecurity = Env.Security.MaintainShipmentJobInvoicing;
				cashAdvancecontrol.Bind(job.CashAdvanceRequests);
				form.Controls.Add(cashAdvancecontrol);
				form.Show();

				Env.Security.GetInvoicingSecurityCheckPoint(cashAdvancecontrol.PluginSecurity, SecurityCore.CancelARCashAdvanceRequest).IsAllowed = false;

				var headerGrid = cashAdvancecontrol.FindSingleOrDefault<ZGrid>("headerGrid");
				AssertNotNull("headerGrid", headerGrid);

				headerGrid.Select(0);
				AssertEquals(job.CashAdvanceRequests[0].PK, headerGrid.SelectedElements[0].PK);

				var markAsCancelButton = cashAdvancecontrol.FindSingleOrDefault<ZButton>("CancelButton");
				AssertNotNull("CancelButton", markAsCancelButton);

				markAsCancelButton.PerformClick();

				AssertMultilineASCIIEquals("LastMessage", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Advance Payments -> Cancel AR Advance Payment Request", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
