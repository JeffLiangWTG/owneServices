using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	[Serializable]
	sealed class WorkflowModifiedFieldChangeTriggerProcessorForTest : WorkflowModifiedFieldChangeTriggerProcessor
	{
		public int[] LogIndexWithNonCriticalExceptionToThrow { get; set; }

		public int[] LogIndexWithCriticalExceptionToThrow { get; set; }

		public int[] LogIndexWithSaveExceptionToThrow { get; set; }

		protected override void ProcessLog(INotifications notifications, TriggeringBusinessObjectFactoryProvider factoryProvider, StmChangeLog changeLog)
		{
			changeLogIndex++;

			if (LogIndexWithNonCriticalExceptionToThrow != null && LogIndexWithNonCriticalExceptionToThrow.Any(x => x == changeLogIndex))
			{
				throw new Exception("Unhandled exception");
			}

			if (LogIndexWithSaveExceptionToThrow != null && LogIndexWithSaveExceptionToThrow.Any(x => x == changeLogIndex))
			{
				var sqlError = SqlExceptionBuilder.CreateSqlError(2601, byte.MaxValue, byte.MinValue, Core.Constants.ProductName, "Data failed to save because unique index conflict NR_UC__E2_ParentID_E2_AddressType_E2_AddressSequence.", "", 1);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(sqlError);
				var exception = SqlExceptionBuilder.CreateSqlException(errors);

				var factory = new BusinessObjectFactory();
				var parentForTest = (BusinessObject)factory.New<Forwarding.IForwardingShipment>();
				var jobDocAddress = JobDocAddress.New(parentForTest);
				factory.Save();
				throw new ZSaveException(new ZDataException(exception, ((IBusinessObjectInternals)jobDocAddress).Row, null), factoryProvider.Current);
			}

			base.ProcessLog(notifications, factoryProvider, changeLog);
		}

		int changeLogIndex;

		public Dictionary<string, string> PropertyDependenciesExposed => TriggerFieldDependencies;
	}
}
