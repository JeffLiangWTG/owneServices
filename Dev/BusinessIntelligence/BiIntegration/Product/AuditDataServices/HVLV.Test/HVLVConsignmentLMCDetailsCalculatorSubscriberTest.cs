using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.AuditDataServices.HVLV.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.HVLV.Test
{
	[TestedType(typeof(HVLVConsignmentLMCDetailsCalculatorSubscriber))]
	class HVLVConsignmentLMCDetailsCalculatorSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public void TestProcessChanges()
		{
			var consignmentsUpdated = new List<Guid>();

			var mockCalculator = new Mock<IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator>();

			mockCalculator.Setup(m => m.UpdateConsignmentsDestinationDetails(It.IsAny<IEnumerable<ZGuid>>()))
				.Callback((IEnumerable<ZGuid> consignmentsToUpdate) => consignmentsUpdated.AddRange(consignmentsToUpdate.Select(pk => pk.ToGuid())));

			var toProcessPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

			var changeTable = GetTestDataTable();

			var consignmentRow1 = CreateRow(changeTable, toProcessPKs[0]);
			consignmentRow1.AcceptChanges();
			consignmentRow1.SetModified();

			var consignmentRow2 = CreateRow(changeTable, toProcessPKs[1]);
			consignmentRow2.AcceptChanges();
			consignmentRow2.SetModified();

			var subscriber = NewDataChangeSubscriber();

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), changeTable);
				AssertContainsExactElementsInAnyOrder(toProcessPKs, consignmentsUpdated);
			}
		}

		public void TestProcessChanges_RemoveDuplicatedConsignmentPKs()
		{
			var consignmentsUpdated = new List<Guid>();

			var mockCalculator = new Mock<IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator>();

			mockCalculator.Setup(m => m.UpdateConsignmentsDestinationDetails(It.IsAny<IEnumerable<ZGuid>>()))
				.Callback((IEnumerable<ZGuid> consignmentsToUpdate) => consignmentsUpdated.AddRange(consignmentsToUpdate.Select(pk => pk.ToGuid())));

			var duplicatedPK = Guid.NewGuid();

			var changeTable = GetTestDataTable();

			var consignmentRow1 = CreateRow(changeTable, duplicatedPK);
			consignmentRow1.AcceptChanges();
			consignmentRow1.SetModified();

			var consignmentRow2 = CreateRow(changeTable, duplicatedPK);
			consignmentRow2.AcceptChanges();
			consignmentRow2.SetModified();

			var subscriber = NewDataChangeSubscriber();

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), changeTable);
				AssertEquals(1, consignmentsUpdated.Count);
				AssertEquals(duplicatedPK, consignmentsUpdated.Single());
			}
		}

		[TestDate(2020, 08, 08, 10, 0, 0)]
		[TestDateIncremental(seconds: 1)]
		public void TestProcessChanges_IgnoresRecordsTooFarInThePast()
		{
			var consignmentsUpdated = new List<Guid>();

			var mockCalculator = new Mock<IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator>();

			mockCalculator.Setup(m => m.UpdateConsignmentsDestinationDetails(It.IsAny<IEnumerable<ZGuid>>()))
				.Callback((IEnumerable<ZGuid> consignmentsToUpdate) => consignmentsUpdated.AddRange(consignmentsToUpdate.Select(pk => pk.ToGuid())));

			var recentPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
			var historicalPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

			var fiveMinutesAgo = ZDateTime.UtcNow.AddMinutes(-5);
			var twentyThreeHoursAgo = ZDateTime.UtcNow.AddHours(-23);
			var twentyFiveHoursAgo = ZDateTime.UtcNow.AddHours(-25);
			var oneWeekAgo = ZDateTime.UtcNow.AddDays(-7);

			var changeTable = GetTestDataTable();

			var recentConsignmentRow1 = CreateRow(changeTable, recentPKs[0], tranEndTimeUtc: fiveMinutesAgo);
			recentConsignmentRow1.AcceptChanges();
			recentConsignmentRow1.SetModified();

			var recentConsignmentRow2 = CreateRow(changeTable, recentPKs[1], tranEndTimeUtc: twentyThreeHoursAgo);
			recentConsignmentRow2.AcceptChanges();
			recentConsignmentRow2.SetModified();

			// Ignore changes if they are more than a day old (For now)
			var historicConsignmentRow1 = CreateRow(changeTable, historicalPKs[0], tranEndTimeUtc: twentyFiveHoursAgo);
			historicConsignmentRow1.AcceptChanges();
			historicConsignmentRow1.SetModified();

			var historicConsignmentRow2 = CreateRow(changeTable, historicalPKs[1], tranEndTimeUtc: oneWeekAgo);
			historicConsignmentRow2.AcceptChanges();
			historicConsignmentRow2.SetModified();

			var subscriber = NewDataChangeSubscriber();

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				var logger = new LoggerForTest();
				subscriber.ProcessChanges(logger, changeTable);
				AssertContainsExactElementsInAnyOrder(recentPKs, consignmentsUpdated);

				var logEntries = logger.LogEntries.ToList();
				AssertEquals(1, logEntries.Count(l => l.Contains("HLC received dataRows: 4 total dataRows")));
				AssertEquals(1, logEntries.Count(l => l.Contains("HLC to process dataRows: 2 dataRows have changed in the past 24 hours")));
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

		public void TestHVLVConsignmentLMCDetailsCalculatorSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();

			CombineAssertions("Subscriber should be configured correctly", () =>
			{
				AssertEquals("Code", "HLC", subscriber.Code);
				AssertEquals("Table Name", HVLVConsignmentSchema.Constants.TableName, subscriber.Table.TableName);
				AssertEquals("Notify Insert", true, subscriber.NotifyInsert);
				AssertEquals("Notify Update", true, subscriber.NotifyUpdate);
				AssertEquals("Notify Delete", false, subscriber.NotifyDelete);
				AssertContainsExactElementsInAnyOrder("Specific Columns",
					new[]
					{
						HVLVConsignmentSchema.Constants.HVC_RN_NKConsigneeCountryCode,
						HVLVConsignmentSchema.Constants.HVC_ConsigneePostcode,
						HVLVConsignmentSchema.Constants.HVC_ConsigneeCity,
						HVLVConsignmentSchema.Constants.HVC_ConsigneeState,
						HVLVConsignmentSchema.Constants.HVC_UndgClass,
					},
					subscriber.SpecificColumns.Select(c => c.Name));
			});
		}

		public void TestProcessChanges_Batched()
		{
			var consignmentsUpdated = new List<Guid>();

			var mockCalculator = new Mock<IHVLVConsignmentLastMileCarrierAndDepotDetailsCalculator>();

			mockCalculator.Setup(m => m.UpdateConsignmentsDestinationDetails(It.IsAny<IEnumerable<ZGuid>>()))
				.Callback((IEnumerable<ZGuid> consignmentsToUpdate) => consignmentsUpdated.AddRange(consignmentsToUpdate.Select(pk => pk.ToGuid())));

			var toProcessPKs = new List<Guid>();

			var changeTable = GetTestDataTable();

			for (int i = 0; i < 10; i++)
			{
				var guid = Guid.NewGuid();
				toProcessPKs.Add(guid);
				var consignmentRow1 = CreateRow(changeTable, guid);
				consignmentRow1.AcceptChanges();
				consignmentRow1.SetModified();
			}

			var subscriber = new HVLVConsignmentLMCDetailsCalculatorSubscriber(3);

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), changeTable);
				AssertContainsExactElementsInAnyOrder(toProcessPKs, consignmentsUpdated);
				AssertEquals(4, mockCalculator.Invocations.Count);
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new HVLVConsignmentLMCDetailsCalculatorSubscriber();

			var table = GetTestDataTable();
			var row1 = GetPopulatedDataRow(table, false);
			var row2 = GetPopulatedDataRow(table, false);
			GetPopulatedDataRow(table, true);
			GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			for (var i = 0; i < 4; i++)
			{
				RunCustomFilter(table.Rows[i], subscriber);
			}
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);
			AssertEquals(row1, table.Rows[0]);
			AssertEquals(row2, table.Rows[1]);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			row[HVLVConsignmentSchema.PK.Name] = Guid.NewGuid();
			if (shouldBeFiltered)
			{
				row[HVLVConsignmentSchema.HVC_IsActive.Name] = 0;
			}
			else
			{
				row[HVLVConsignmentSchema.HVC_IsActive.Name] = 1;
			}
			table.Rows.Add(row);
			return row;
		}

		#region Implementation

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add("TranEndTimeUtc", typeof(DateTime));

			changeTable.Columns.Add(HVLVConsignmentSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(HVLVConsignmentSchema.Constants.HVC_IsActive, typeof(bool));

			return changeTable;
		}

		DataRow CreateRow(DataTable dataTable, Guid pk, ZDateTime? tranEndTimeUtc = null)
		{
			var row = dataTable.NewRow();

			row["TranEndTimeUtc"] = tranEndTimeUtc?.ToDateTime() ?? ZDateTime.UtcNow.ToDateTime();

			row[HVLVConsignmentSchema.Constants.PK] = pk;

			dataTable.Rows.Add(row);
			return row;
		}

		#endregion
	}
}
