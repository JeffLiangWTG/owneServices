using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.HVLV.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.HVLV.Test
{
	[TestedType(typeof(HVLVBookingHeaderLMCDetailsCalculatorSubscriber))]
	class HVLVBookingHeaderLMCDetailsCalculatorSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public void TestProcessChanges_ProcessConsignmentsOfBookingHeaderByClusterKey()
		{
			var factory = new BusinessObjectFactory();

			var bookingHeader1 = factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader3 = factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader3.HVH_IsProcessedAtOriginDepot = true;

			factory.Save();

			var updatedbookingHeaderClusterKeys = new List<int>();

			var mockCalculator = new Mock<IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator>();

			mockCalculator.Setup(m => m.UpdateConsignmentsDestinationDetailsByClusterKeys(It.IsAny<IEnumerable<int>>()))
				.Callback((IEnumerable<int> updateBookingHeaderClusterKeys) => updatedbookingHeaderClusterKeys.AddRange(updateBookingHeaderClusterKeys));

			var changeTable = GetTestDataTable();

			var bookingHeaderRow1 = CreateRow(changeTable, bookingHeader1);
			bookingHeaderRow1.AcceptChanges();
			bookingHeaderRow1.SetModified();

			var bookingHeaderRow2 = CreateRow(changeTable, bookingHeader2);
			bookingHeaderRow2.AcceptChanges();
			bookingHeaderRow2.SetModified();

			var bookingHeaderRow3 = CreateRow(changeTable, bookingHeader3);
			bookingHeaderRow3.AcceptChanges();
			bookingHeaderRow3.SetModified();

			var subscriber = NewDataChangeSubscriber();
			var toProcessClusterKeys = new int[] { bookingHeader1.HVH_ClusterKey, bookingHeader2.HVH_ClusterKey };

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), changeTable);
				AssertContainsExactElementsInAnyOrder(toProcessClusterKeys, updatedbookingHeaderClusterKeys);
			}
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 1)]
		public void TestProcessChanges_IgnoresRecordsTooFarInThePast()
		{
			var factory = new BusinessObjectFactory();

			var recentBookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			var historicalBookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();

			factory.Save();

			var updatedbookingHeaderClusterKeys = new List<int>();

			var mockCalculator = new Mock<IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator>();

			mockCalculator.Setup(m => m.UpdateConsignmentsDestinationDetailsByClusterKeys(It.IsAny<IEnumerable<int>>()))
				.Callback((IEnumerable<int> updateBookingHeaderClusterKeys) => updatedbookingHeaderClusterKeys.AddRange(updateBookingHeaderClusterKeys));

			var fiveMinutesAgo = ZDateTime.UtcNow.AddMinutes(-5);
			var twentyFiveHoursAgo = ZDateTime.UtcNow.AddHours(-25);

			var changeTable = GetTestDataTable();

			var recentBookingHeaderRow = CreateRow(changeTable, recentBookingHeader, tranEndTimeUtc: fiveMinutesAgo);
			recentBookingHeaderRow.AcceptChanges();
			recentBookingHeaderRow.SetModified();

			var historicalBookingHeaderRow = CreateRow(changeTable, historicalBookingHeader, tranEndTimeUtc: twentyFiveHoursAgo);
			historicalBookingHeaderRow.AcceptChanges();
			historicalBookingHeaderRow.SetModified();

			var subscriber = NewDataChangeSubscriber();
			var toProcessClusterKeys = new int[] { recentBookingHeader.HVH_ClusterKey };

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				var logger = new LoggerForTest();
				subscriber.ProcessChanges(logger, changeTable);
				AssertContainsExactElementsInAnyOrder(toProcessClusterKeys, updatedbookingHeaderClusterKeys);

				var logEntries = logger.LogEntries.ToList();
				AssertEquals(1, logEntries.Count(l => l.Contains("HLB received dataRows: 2 total dataRows")));
				AssertEquals(1, logEntries.Count(l => l.Contains("HLB to process dataRows: 1 dataRows have changed in the past 24 hours")));
			}
		}

		public void TestIsRequired_MatchesRegistryValues()
		{
			var subscriber = NewDataChangeSubscriber();

			using (HVLVDataRegistry.Instance.HVLVAutomaticallyCalculateLMCDepotDetails.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("IsRequired when registry disabled:", false, subscriber.IsRequired());
			}

			using (HVLVDataRegistry.Instance.HVLVAutomaticallyCalculateLMCDepotDetails.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("IsRequired when registry enabled:", true, subscriber.IsRequired());
			}
		}

		public void TestHVLVBookingHeaderLMCDetailsCalculatorSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();

			CombineAssertions("Subscriber should be configured correctly", () =>
			{
				AssertEquals("Code", "HLB", subscriber.Code);
				AssertEquals("Table Name", HVLVBookingHeaderSchema.Constants.TableName, subscriber.Table.TableName);
				AssertEquals("Notify Insert", false, subscriber.NotifyInsert);
				AssertEquals("Notify Update", true, subscriber.NotifyUpdate);
				AssertEquals("Notify Delete", false, subscriber.NotifyDelete);
				AssertContainsExactElementsInAnyOrder("Specific Columns",
					new[]
					{
						HVLVBookingHeaderSchema.Constants.HVH_OA_OriginDepot,
						HVLVBookingHeaderSchema.Constants.HVH_RS_NKBookingServiceLevel,
					},
					subscriber.SpecificColumns.Select(c => c.Name));
			});
		}

		public override void TestCustomFilter()
		{
			var subscriber = new HVLVBookingHeaderLMCDetailsCalculatorSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		#region Implementation

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add("TranEndTimeUtc", typeof(DateTime));

			changeTable.Columns.Add(HVLVBookingHeaderSchema.Constants.PK, typeof(ZGuid));
			changeTable.Columns.Add(HVLVBookingHeaderSchema.Constants.HVH_IsProcessedAtOriginDepot, typeof(ZBool));

			return changeTable;
		}

		DataRow CreateRow(DataTable dataTable, HVLVBookingHeader bookingHeader, ZDateTime? tranEndTimeUtc = null)
		{
			var row = dataTable.NewRow();

			row[HVLVBookingHeaderSchema.Constants.PK] = bookingHeader.PK;
			row["TranEndTimeUtc"] = tranEndTimeUtc?.ToDateTime() ?? ZDateTime.UtcNow.ToDateTime();
			row[HVLVBookingHeaderSchema.Constants.HVH_IsProcessedAtOriginDepot] = bookingHeader.HVH_IsProcessedAtOriginDepot;

			dataTable.Rows.Add(row);
			return row;
		}

		#endregion
	}
}
