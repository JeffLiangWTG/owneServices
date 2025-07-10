using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	sealed class OutgoingMessageProcessorTestWithExceptionClass : OutgoingMessageProcessorTestClass
	{
		public OutgoingMessageProcessorTestWithExceptionClass(LoggingInformation logger)
			: base(logger)
		{
		}

		internal override BusinessObjectFactory CreateFactory()
		{
			var factory = base.CreateFactory();
			factory.Saving += (s) =>
			{
				var error = SqlExceptionBuilder.CreateSqlError(-2, 2, 3, "server name", "sql timeout exception", "proc", 100);
				var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				throw new ZSaveException(new ZDataException(SqlExceptionBuilder.CreateSqlException(errorCollection), null, null), null);
			};
			return factory;
		}
	}
}
