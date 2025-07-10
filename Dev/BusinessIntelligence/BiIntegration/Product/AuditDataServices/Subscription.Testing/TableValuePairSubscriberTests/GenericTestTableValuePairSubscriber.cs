namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System;
	using System.Collections.Generic;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.Integration;

	public class GenericTestTableValuePairSubscriber : TableValuePairSubscriber
	{
		public GenericTestTableValuePairSubscriber(string code, string description,
			Exception exceptionToThrowDuringProcessChanges = null
			)
		{
			this.code = code;
			this.description = description;

			this.ExceptionToThrowDuringProcessChanges = exceptionToThrowDuringProcessChanges;
		}

		public override string Code
		{
			get { return code; }
		}
		readonly string code;

		public override string Description
		{
			get { return description; }
		}
		readonly string description;

		public override bool IsRequired()
		{
			return true;
		}

		public override void ProcessChanges(ILogger logger, Dictionary<IChangedTableSchema, List<string>> changedTableColumnValues)
		{
			if (ExceptionToThrowDuringProcessChanges != null)
			{
				throw ExceptionToThrowDuringProcessChanges;
			}
			foreach (var kvp in changedTableColumnValues)
			{
				var changedTable = kvp.Key;
				var values = kvp.Value;
				values.Sort();
				var changedValues = string.Join(", ", values);
				var message = $"[{changedTable.SchemaName}].[{changedTable.TableName}] - ({changedValues})";
				logger.Log(LogType.Information, message);
			}
		}

		public Exception ExceptionToThrowDuringProcessChanges { get; private set; }
	}
}
