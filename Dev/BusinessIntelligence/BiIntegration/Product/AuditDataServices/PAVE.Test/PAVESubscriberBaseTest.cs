using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service.Client;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.AuditDataServices.PAVE.Test
{
	abstract class PAVESubscriberBaseTest : ActualDataChangesAuditSubscriberTest
	{
		abstract protected string ExpectedCode { get; }
		abstract protected ITableSchema ExpectedTable { get; }
		abstract protected IEnumerable<SchemaColumn> ExpectedSpecificColumns { get; }

		public void TestCode() => AssertEquals(ExpectedCode, NewDataChangeSubscriber().Code);

		public void TestTable() => AssertEquals(ExpectedTable.TableName, NewDataChangeSubscriber().Table.TableName);

		public void TestSpecificColumns()
		{
			if (ExpectedSpecificColumns == null)
			{
				AssertNull(NewDataChangeSubscriber().SpecificColumns);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(ExpectedSpecificColumns, NewDataChangeSubscriber().SpecificColumns);
			}
		}

		public void TestIsRequired() => AssertEquals(true, NewDataChangeSubscriber().IsRequired());

		public abstract void TestIsEnabled();

		protected void ProcessChanges(DataTable changes) => NewDataChangeSubscriber().ProcessChanges(Logger, changes);

		protected virtual ILogger Logger => loggerMock.Object;

		#region SetUp

		readonly protected IBMSRegistry BMSRegistry = ObjectFactory.Get<IBMSRegistry>();
		readonly protected Mock<ILogger> loggerMock = new Mock<ILogger>();
		readonly protected Mock<IPAVEServiceClientFactory> serviceClientFactoryMock = new Mock<IPAVEServiceClientFactory>();

		IDisposable tableCachingDisabled;

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Substitute(serviceClientFactoryMock.Object);

			tableCachingDisabled = ObjectFactory.Get<IBMTestHelper>().TemporarilyDisableTableCachingInUberFactory([BMComponentSchema.Constants.TableName, BMSystemSchema.Constants.TableName]); // preparation for removing these tables from UberFactory
		}

		protected override void TearDown()
		{
			tableCachingDisabled.Dispose();
			base.TearDown();
		}

		#endregion
	}
}
