using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	[TestedType(typeof(EdiDisableStaffCommissionAgreementsAction))]
	public class EdiDisableStaffCommissionAgreementsActionTest : DisableStaffCommissionAgreementsActionTest
	{
		#region Execute
		public override void TestExecuteAndApprove_CalcQueue_BackDateNotSet()
		{
			OrganisationRegistry.Instance.DefaultSpecifiedBackdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);

			var action = EdiDisableStaffCommissionAgreementsAction.New(AdlStaff);
			action.Date = new ZDateTime(2002, 2, 2);
			action.ShouldUpdateEarlierEndDates = false;

			action.ExecuteAndApprove(null);

			var queueItem = Factory.LoadTop1<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, Agreement1.PK));

			AssertNotNull("Queue Item for Agreement 1 exists", queueItem);

			AssertEquals("Always override", true, queueItem.CAQ_OverwriteExistingCommissions);
			AssertEquals("No Date Set", ZDateTime.MinSmallDateTimeValue, queueItem.CAQ_MinimumInvoicePostedDate);
		}

		public override void TestExecuteAndApprove_CalcQueue_BackDateSet()
		{
			OrganisationRegistry.Instance.DefaultSpecifiedBackdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2002, 1, 1));

			var action = EdiDisableStaffCommissionAgreementsAction.New(AdlStaff);
			action.Date = new ZDateTime(2002, 2, 2);
			action.ShouldUpdateEarlierEndDates = false;

			action.ExecuteAndApprove(null);

			var queueItem = Factory.LoadTop1<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, Agreement1.PK));

			AssertNotNull("Queue Item for Agreement 1 exists", queueItem);

			AssertEquals("Always override", true, queueItem.CAQ_OverwriteExistingCommissions);
			AssertEquals("Date Set", new ZDateTime(2002, 1, 1), queueItem.CAQ_MinimumInvoicePostedDate);
		}

		#endregion
	}
}
