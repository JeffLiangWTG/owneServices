using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.AuditDataServices.Accounting.Test
{
	public abstract class AccountingSubscriberBaseTest : ActualDataChangesAuditSubscriberTest
	{
		protected abstract string ExpectedCode { get; }

		protected abstract string ExpectedDescription { get; }

		protected abstract string ExpectedTableName { get; }

		protected virtual bool ExpectedNotifyInsert => true;

		protected virtual bool ExpectedNotifyUpdate => false;

		protected virtual bool ExpectedNotifyDelete => false;

		public abstract void TestProcessChanges();

		public abstract void TestProcessChanges_BeforeGenerateJournalEntriesCDCStartDate();

		public abstract void TestProcessChanges_HasException();

		public void TestCode() => AssertEquals(ExpectedCode, NewDataChangeSubscriber().Code);

		public void TestDescription() => AssertEquals(ExpectedDescription, NewDataChangeSubscriber().Description);

		public void TestTable() => AssertEquals(ExpectedTableName, NewDataChangeSubscriber().Table.TableName);

		public virtual void TestSpecificColumns() => AssertNull(NewDataChangeSubscriber().SpecificColumns);

		public void TestNotifyInsert() => AssertEquals(ExpectedNotifyInsert, NewDataChangeSubscriber().NotifyInsert);

		public void TestNotifyUpdate() => AssertEquals(ExpectedNotifyUpdate, NewDataChangeSubscriber().NotifyUpdate);

		public void TestNotifyDelete() => AssertEquals(ExpectedNotifyDelete, NewDataChangeSubscriber().NotifyDelete);

		public void TestIsRequired()
		{
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, NewDataChangeSubscriber().IsRequired());

			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, NewDataChangeSubscriber().IsRequired());
		}
		protected bool AssertChangeRow(DataRow[] changeRows, string pkName, int count)
		{
			AssertEquals(count, changeRows.Length);
			foreach (DataRow changeRow in changeRows)
			{
				var result = generalLedgerDataRows.TryGetValue((Guid)changeRow[pkName], out var exceptedRow);
				AssertEquals(true, result);
				AssertNotNull(exceptedRow);
				AssertEquals("TableName", exceptedRow.Table.TableName, changeRow.Table.TableName);
				AssertEquals("TableName", ExpectedTableName, changeRow.Table.TableName);
			}
			return true;
		}

		protected Dictionary<ZGuid, DataRow> generalLedgerDataRows = new Dictionary<ZGuid, DataRow>();
		protected readonly BusinessObjectFactory factory = new BusinessObjectFactory();
		protected readonly Mock<ILogger> loggerMock = new Mock<ILogger>();
		protected readonly Mock<IGeneralLedgerDataProcessor> generalLedgerDataProcessorMock = new Mock<IGeneralLedgerDataProcessor>();

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Substitute(generalLedgerDataProcessorMock.Object);
		}

		#endregion
	}
}
