using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(CommissionFinalizer))]
	public class CommissionFinalizerTest : NonPersistentBusinessObjectTestCase
	{
		#region Find

		public void TestFind()
		{
			var commissionLine1 = Factory.NewWithValidTestData<AccCommissionLine>();
			var commissionLine2 = Factory.NewWithValidTestData<AccCommissionLine>();
			var paidCommissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			paidCommissionLine.CL0_PaidDateTimeUtc = new ZDateTime(2002, 2, 2);
			var cancelledCommissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			cancelledCommissionLine.CL0_CancelledDateTimeUtc = new ZDateTime(2002, 2, 2);

			Factory.Save();

			var finalizer = new CommissionFinalizer();
			finalizer.Find(new CommissionFinalizerFilterBusinessObject());

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionLine>.PKOnlyComparer,
				new[]
				{
					commissionLine1,
					commissionLine2
				},
				finalizer.CommissionFinalizerLineItems.Select(x => x.ViewCommissionLine.AccCommissionLine));
		}

		public void TestFind_ReloadFromDatabase()
		{
			var commissionLine1 = Factory.NewWithValidTestData<AccCommissionLine>();
			var commissionLine2 = Factory.NewWithValidTestData<AccCommissionLine>();
			Factory.Save();

			var finalizer = new CommissionFinalizer();
			finalizer.Find(new CommissionFinalizerFilterBusinessObject());

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionLine>.PKOnlyComparer,
				new[]
				{
					commissionLine1,
					commissionLine2
				},
				finalizer.CommissionFinalizerLineItems.Select(x => x.ViewCommissionLine.AccCommissionLine));

			var anotherFactory = new BusinessObjectFactory();
			using (GetFactoryIsolater(anotherFactory))
			{
				var commissionLine1InAnotherFactory = anotherFactory.Load<AccCommissionLine>(commissionLine1.PK);
				commissionLine1InAnotherFactory.CL0_PaidDateTimeUtc = new ZDateTime(2002, 2, 2);
				anotherFactory.Save();
			}

			finalizer.Find(new CommissionFinalizerFilterBusinessObject());
			AssertContainsExactElementsInAnyOrder("Should no longer return commissionLine1 since it has been paid out",
				BusinessObjectEqualityComparer<AccCommissionLine>.PKOnlyComparer,
				new[]
				{
					commissionLine2
				},
				finalizer.CommissionFinalizerLineItems.Select(x => x.ViewCommissionLine.AccCommissionLine));
		}

		public void TestFind_MaximumRows()
		{
			OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader.CH0_GC = company.PK;
			commissionHeader.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeader.CH0_GroupingSourceID = ZGuid.NewZGuid();

			for (var i = 0; i < 7; i++)
			{
				commissionHeader.Lines.AddNew();
			}

			Factory.Save();

			using (Environment.Env.Instance.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var finalizer = new CommissionFinalizer();
				finalizer.Find(new CommissionFinalizerFilterBusinessObject());

				AssertEquals("Maximum Rows Limit enforced", 6, finalizer.CommissionFinalizerLineItems.Count());
			}
		}

		#endregion

		#region Approval Request

		public void TestGetNewApprovalRequest()
		{
			var commissionLine1 = Factory.NewWithValidTestData<AccCommissionLine>();
			var commissionLine2 = Factory.NewWithValidTestData<AccCommissionLine>();
			var commissionLine3 = Factory.NewWithValidTestData<AccCommissionLine>();
			var commissionLine4 = Factory.NewWithValidTestData<AccCommissionLine>();
			Factory.Save();

			var finalizer = new CommissionFinalizer();
			finalizer.ViewCommissionLineCollection.AdditionalFilter = new ZQuery();
			AssertEquals("Precondition", 4, finalizer.ViewCommissionLineCollection.Count);
			AssertEquals("Precondition", 4, finalizer.CommissionFinalizerLineItemCollection.Count);

			finalizer.CommissionFinalizerLineItems.Single(x => x.ViewCommissionLine.PK == commissionLine1.PK).IsSelected = true;
			finalizer.CommissionFinalizerLineItems.Single(x => x.ViewCommissionLine.PK == commissionLine2.PK).IsSelected = true;
			finalizer.CommissionFinalizerLineItems.Single(x => x.ViewCommissionLine.PK == commissionLine3.PK).IsSelected = false;
			finalizer.CommissionFinalizerLineItems.Single(x => x.ViewCommissionLine.PK == commissionLine4.PK).IsSelected = false;

			var approvalRequest = finalizer.GetNewApprovalRequest(Factory);

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionLine>.PKOnlyComparer,
				new[]
				{
					commissionLine1,
					commissionLine2,
				},
				approvalRequest.Items.Select(x => x.CommissionLine));

			AssertEquals(true, approvalRequest.Items.All(x => x.CRI_IsSelected));
		}

		#endregion
	}
}
