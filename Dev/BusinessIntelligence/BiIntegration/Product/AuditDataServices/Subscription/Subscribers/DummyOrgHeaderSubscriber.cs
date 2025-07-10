#if DEBUG

namespace Enterprise.AuditDataServices.Subscription.Subscribers
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using CargoWise.Schema;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.Integration;
	using Enterprise.ZArchitecture.Schema;

	public class DummyOrgHeaderSubscriber : ActualDataChangesAuditSubscriber
	{
		public override string Code
		{
			get { return "~OH"; }
		}

		public override string Description
		{
			get { return "Dummy Org Header Subscriber"; }
		}

		public override bool IsRequired()
		{
			return true;
		}

		public override ITableSchema Table
		{
			get { return OrgHeaderSchema.Instance; }
		}

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get { return null; }
		}

		public override bool NotifyInsert
		{
			get { return true; }
		}

		public override bool NotifyUpdate
		{
			get { return false; }
		}

		public override bool NotifyDelete
		{
			get { return true; }
		}

		public override Action<DataRow> CustomFilter
		{
			get { return null; }
		}

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			foreach (DataRow changeRow in changeTable.Rows)
			{
				var rowVersion = (changeRow.RowState == DataRowState.Deleted) ? DataRowVersion.Original : DataRowVersion.Default;

				string logText = string.Format(CultureInfo.InvariantCulture,
					"{0} - {1} - {2} - {3}",
					changeRow.RowState.ToString(),
					changeRow[OrgHeaderSchema.Constants.PK, rowVersion].ToStringSafe(),
					changeRow[OrgHeaderSchema.Constants.OH_Code, rowVersion].ToStringSafe().Trim(),
					changeRow[OrgHeaderSchema.Constants.OH_FullName, rowVersion].ToStringSafe().Trim()
				);

				logger.Log(LogType.Information, logText);
			}
		}
	}
}

#endif
