namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using CargoWise.Data;
	using CargoWise.Schema;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.Integration;

	public class GenericExceptionThrowingSubscriber : IActualDataChangesAuditSubscriber
	{
		public GenericExceptionThrowingSubscriber(Exception exceptionToThrow)
		{
			this.ExceptionToThrow = exceptionToThrow;
		}
		public Exception ExceptionToThrow { get; }

		public IAuditSubscriberWrapper GetWrapper(DbConnection auditConnection, ILogger logger)
		{
			throw ExceptionToThrow;
		}

		#region Not Implemented Fields
		public ITableSchema Table => throw new NotImplementedException();

		public IEnumerable<SchemaColumn> SpecificColumns => throw new NotImplementedException();

		public Action<DataRow> CustomFilter => throw new NotImplementedException();

		public bool NotifyInsert => throw new NotImplementedException();

		public bool NotifyUpdate => throw new NotImplementedException();

		public bool NotifyDelete => throw new NotImplementedException();

		public string Code => "~XX";

		public string Description => throw new NotImplementedException();

		public bool IsRequired()
		{
			throw new NotImplementedException();
		}

		public void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
