using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI.Testing
{
	public class CashAdvanceRequestHelperTest : TestCaseWithFactory
	{
		public void TestMarkAsCancel()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var cashAdvanceRequestCollection = new List<AccCashAdvanceRequestHeader>();
				var securityCheckPoint = Env.Security.ARCashAdvanceRequestCancel;

				securityCheckPoint.IsAllowed = false;
				CashAdvanceRequestHelper.MarkAsCancelRequest(securityCheckPoint, cashAdvanceRequestCollection.ToArray());
				AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);

				securityCheckPoint.IsAllowed = true;
				CashAdvanceRequestHelper.MarkAsCancelRequest(securityCheckPoint, cashAdvanceRequestCollection.ToArray());
				AssertEquals("Please select an Advance Payment Request to perform this action.", UnitTestUserNotification.Instance.LastMessage.Text);

				CashAdvanceRequestHeader1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
				Factory.Save();
				cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader1);
				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, CashAdvanceRequestHeader1.CAH_Status);
				CashAdvanceRequestHelper.MarkAsCancelRequest(securityCheckPoint, cashAdvanceRequestCollection.ToArray());
				AssertStartsWith("expect warning message", "Following Advance Payment requests could not be marked as Cancelled", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, CashAdvanceRequestHeader1.CAH_Status);

				CashAdvanceRequestHeader1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
				Factory.Save();
				cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader1);
				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceRequestHeader1.CAH_Status);
				CashAdvanceRequestHelper.MarkAsCancelRequest(securityCheckPoint, cashAdvanceRequestCollection.ToArray());
				AssertStartsWith("expect success message", "Following Advance Payment requests are marked as Cancelled", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Cancelled, CashAdvanceRequestHeader1.CAH_Status);

				CashAdvanceRequestHeader1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
				CashAdvanceRequestLine1.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
				Factory.Save();
				cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader1);
				cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader2);
				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceRequestHeader1.CAH_Status);
				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceRequestHeader2.CAH_Status);
				CashAdvanceRequestHelper.MarkAsCancelRequest(securityCheckPoint, cashAdvanceRequestCollection.ToArray());
				AssertStartsWith("expect success message", "Following Advance Payment requests are marked as Cancelled", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Cancelled, CashAdvanceRequestHeader1.CAH_Status);
				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Cancelled, CashAdvanceRequestHeader2.CAH_Status);
			}
		}

		public void TestMarkAsPaid()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var cashAdvanceRequestCollection = new List<AccCashAdvanceRequestHeader>();
					var securityCheckPoint = Env.Security.ARCashAdvanceRequestMarkAsPaid;

					securityCheckPoint.IsAllowed = false;
					CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(securityCheckPoint, cashAdvanceRequestCollection.ToArray(), (ca) => ca.MarkAsPaid(), "Paid");
					AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);

					securityCheckPoint.IsAllowed = true;
					CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(securityCheckPoint, cashAdvanceRequestCollection.ToArray(), (ca) => ca.MarkAsPaid(), "Paid");
					AssertEquals("Please select at least one Advance Payment request to mark as Paid.", UnitTestUserNotification.Instance.LastMessage.Text);

					CashAdvanceRequestHeader1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Invoiced;
					Factory.Save();
					cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader1);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, CashAdvanceRequestHeader1.CAH_Status);
					CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(securityCheckPoint, cashAdvanceRequestCollection.ToArray(), (ca) => ca.MarkAsPaid(), "Paid");
					AssertStartsWith("expect warning message", "Following Advance Payment requests could not be marked as Paid", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, CashAdvanceRequestHeader1.CAH_Status);

					CashAdvanceRequestHeader1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
					Factory.Save();
					cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader1);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceRequestHeader1.CAH_Status);
					CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(securityCheckPoint, cashAdvanceRequestCollection.ToArray(), (ca) => ca.MarkAsPaid(), "Paid");
					AssertStartsWith("expect success message", "Following Advance Payment requests are marked as Paid", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, CashAdvanceRequestHeader1.CAH_Status);

					CashAdvanceRequestHeader1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
					CashAdvanceRequestLine1.CAL_LocalPaidAmount = 0M;
					CashAdvanceRequestLine1.CAL_OSAmount = 1000M;
					CashAdvanceRequestLine1.CAL_OSPaidAmount = 0M;
					CashAdvanceRequestLine1.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
					Factory.Save();
					cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader1);
					cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader2);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceRequestHeader1.CAH_Status);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceRequestHeader2.CAH_Status);
					CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(securityCheckPoint, cashAdvanceRequestCollection.ToArray(), (ca) => ca.MarkAsPaid(), "Paid");
					AssertStartsWith("expect success message", "Following Advance Payment requests are marked as Paid", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, CashAdvanceRequestHeader1.CAH_Status);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, CashAdvanceRequestHeader2.CAH_Status);
				}
			}
		}

		public void TestMarkAsUnPaid()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var cashAdvanceRequestCollection = new List<AccCashAdvanceRequestHeader>();
					var securityCheckPoint = Env.Security.ARCashAdvanceRequestMarkAsUnPaid;

					securityCheckPoint.IsAllowed = false;
					CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(securityCheckPoint, cashAdvanceRequestCollection.ToArray(), (ca) => ca.UndoPaidStatus(), "UnPaid");
					AssertStartsWith("expect no security rights message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);

					securityCheckPoint.IsAllowed = true;
					CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(securityCheckPoint, cashAdvanceRequestCollection.ToArray(), (ca) => ca.UndoPaidStatus(), "UnPaid");
					AssertEquals("Please select at least one Advance Payment request to mark as UnPaid.", UnitTestUserNotification.Instance.LastMessage.Text);

					CashAdvanceRequestHeader1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Invoiced;
					Factory.Save();
					cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader1);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, CashAdvanceRequestHeader1.CAH_Status);
					CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(securityCheckPoint, cashAdvanceRequestCollection.ToArray(), (ca) => ca.UndoPaidStatus(), "UnPaid");
					AssertStartsWith("expect warning message", "Following Advance Payment requests could not be marked as UnPaid", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, CashAdvanceRequestHeader1.CAH_Status);

					CashAdvanceRequestHeader1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
					Factory.Save();
					cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader1);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, CashAdvanceRequestHeader1.CAH_Status);
					CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(securityCheckPoint, cashAdvanceRequestCollection.ToArray(), (ca) => ca.UndoPaidStatus(), "UnPaid");
					AssertStartsWith("expect success message", "Following Advance Payment requests are marked as UnPaid", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceRequestHeader1.CAH_Status);

					CashAdvanceRequestHeader1.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
					CashAdvanceRequestHeader2.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
					CashAdvanceRequestLine1.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
					CashAdvanceRequestLine2.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
					CashAdvanceRequestLine1.CAL_LocalPaidAmount = 10;
					CashAdvanceRequestLine2.CAL_LocalPaidAmount = 10;
					CashAdvanceRequestLine1.CAL_OSPaidAmount = 10;
					CashAdvanceRequestLine2.CAL_OSPaidAmount = 10;
					Factory.Save();
					cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader1);
					cashAdvanceRequestCollection.Add(CashAdvanceRequestHeader2);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, CashAdvanceRequestHeader1.CAH_Status);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, CashAdvanceRequestHeader2.CAH_Status);
					CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(securityCheckPoint, cashAdvanceRequestCollection.ToArray(), (ca) => ca.UndoPaidStatus(), "UnPaid");
					AssertStartsWith("expect success message", "Following Advance Payment requests are marked as UnPaid", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceRequestHeader1.CAH_Status);
					AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceRequestHeader2.CAH_Status);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var testObjectCreator = new TestObjectCreator(Factory);

			var job1 = testObjectCreator.CreateJob("S00001000", null, 0, null, 0);
			var job2 = testObjectCreator.CreateJob("S00001111", null, 0, null, 0);

			CashAdvanceRequestHeader1 = testObjectCreator.CreateCashAdvanceRequestHeader(job1.PK, testObjectCreator.LocalClient.PK, LedgerTypes.AccountsReceivable, 10m, 1000m, testObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			CashAdvanceRequestHeader2 = testObjectCreator.CreateCashAdvanceRequestHeader(job2.PK, testObjectCreator.LocalClient.PK, LedgerTypes.AccountsReceivable, 100m, 100m, testObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);

			CashAdvanceRequestLine1 = testObjectCreator.CreateCashAdvanceRequestLine(CashAdvanceRequestHeader1.PK, CashAdvanceRequestHeader1.CAH_LocalAmount, CashAdvanceRequestHeader1.CAH_OSAmount, CashAdvanceRequestHeader1.CAH_Status);
			CashAdvanceRequestLine2 = testObjectCreator.CreateCashAdvanceRequestLine(CashAdvanceRequestHeader2.PK, CashAdvanceRequestHeader2.CAH_LocalAmount, CashAdvanceRequestHeader2.CAH_OSAmount, CashAdvanceRequestHeader2.CAH_Status);

			CashAdvanceRequestHeader1.Lines.Add(CashAdvanceRequestLine1);
			CashAdvanceRequestHeader2.Lines.Add(CashAdvanceRequestLine2);

			Factory.Save();
		}
		AccCashAdvanceRequestHeader CashAdvanceRequestHeader1;
		AccCashAdvanceRequestHeader CashAdvanceRequestHeader2;
		AccCashAdvanceRequestLine CashAdvanceRequestLine1;
		AccCashAdvanceRequestLine CashAdvanceRequestLine2;
	}
}
