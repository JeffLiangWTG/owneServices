using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.OrderManager;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(UpdateOrderIsReleasedFlagForInProgressOrders))]
	class UpdateOrderIsReleasedFlagForInProgressOrdersTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "JobOrderHeader", "JD_IsReleased", "BIT", "0");

			var helper = new TransformationTestDataCreator();
			var buyerOrganizationPK = helper.CreateOrgHeader("BUYER", "BUYER organization");
			var buyerAddressPK = helper.CreateOrgAddress(buyerOrganizationPK, "Address1", "Code", "Company");

			expectedReleasedOrders = expectedReleasedStatuses
				.Select((status, index) => helper.CreateJobOrderHeader(
					buyerAddressPK,
					status: status,
					orderNumber: $"ORD-Released-{index}"
				))
				.ToList();

			expectedOnHoldOrders = expectedOnHoldStatuses
				.Select((status, index) => helper.CreateJobOrderHeader(
					buyerAddressPK,
					status: status,
					orderNumber: $"ORD-Unreleased-{index}"
				))
				.ToList();
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();

			CombineAssertions("Should set previously in-progress orders to released", () =>
				expectedReleasedOrders.ForEach(orderPK => AssertEquals(expected: true, IsReleased(orderPK))));

			CombineAssertions("Should not set is released for orders that are incomplete or cancelled", () =>
				expectedOnHoldOrders.ForEach(orderPK => AssertEquals(expected: false, IsReleased(orderPK))));
		}

		bool IsReleased(Guid orderPK)
			=> Db.Connection.ExecuteScalar<bool>(
				"SELECT JD_IsReleased FROM dbo.JobOrderHeader WHERE JD_PK = @JD_PK",
				cmd => cmd.AddParameter("@JD_PK", SqlDbType.UniqueIdentifier, orderPK));

		protected override DataTransformation GetNewTestTransformationInstance()
			=> new UpdateOrderIsReleasedFlagForInProgressOrders();

		public override string[] expectedIndex =>
		[
			"NONCLUSTERED INDEX [_WTG__Set JD_IsReleased to true if order is in progress (not incomplete, or cancelled)_1] ON [dbo].[JobOrderHeader] ([JD_OrderStatus]) INCLUDE ([JD_SystemLastEditTimeUtc], [JD_SystemLastEditUser]) WHERE ([JD_OrderStatus]<>'INC' AND [JD_OrderStatus]<>'CAN') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		];

		IEnumerable<Guid> expectedReleasedOrders;
		IEnumerable<Guid> expectedOnHoldOrders;

		static readonly string[] expectedReleasedStatuses = ["ALL", "PLC", "CNF", "SHP", "DLV", "PRT", "BKD", "PRZ"];
		static readonly string[] expectedOnHoldStatuses = ["INC", "CAN"];
	}
}
