#if DEBUG

namespace Enterprise.AuditDataServices.Subscription.Subscribers
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Text;
	using CargoWise.Schema;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.Integration;
	using Enterprise.ZArchitecture.Schema;

	public class DummyRefCountrySubscriber : ActualDataChangesAuditSubscriber
	{
		public override string Code
		{
			get { return "~RN"; }
		}

		public override string Description
		{
			get { return "Dummy Ref Country Subscriber"; }
		}

		public override bool IsRequired()
		{
			return true;
		}

		public override ITableSchema Table
		{
			get { return RefCountrySchema.Instance; }
		}

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get
			{
				yield return RefCountrySchema.RN_Desc;
				yield return RefCountrySchema.RN_IsActive;
			}
		}

		public override bool NotifyInsert
		{
			get { return true; }
		}

		public override bool NotifyUpdate
		{
			get { return true; }
		}

		public override bool NotifyDelete
		{
			get { return true; }
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[RefCountrySchema.Constants.RN_IsActive] == DBNull.Value || Convert.ToInt32(row[RefCountrySchema.Constants.RN_IsActive]) != 1)
			{
				row.Delete();
			}
		};

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			foreach (DataRow changeRow in changeTable.Rows)
			{
				var logText = new StringBuilder();

				logText.AppendFormat(CultureInfo.InvariantCulture,
					"{0} - PK {1} = ",
					changeRow.RowState.ToString(),
					changeRow[RefCountrySchema.Constants.PK, (changeRow.RowState == DataRowState.Deleted) ? DataRowVersion.Original : DataRowVersion.Default].ToString()
				);

				if (changeRow.RowState == DataRowState.Modified || changeRow.RowState == DataRowState.Deleted)
				{
					logText.AppendFormat(CultureInfo.InvariantCulture,
						"Code(before) {0} - Desc(before) {1} => ",
						changeRow[RefCountrySchema.Constants.RN_Code, DataRowVersion.Original].ToString().Trim(),
						changeRow[RefCountrySchema.Constants.RN_Desc, DataRowVersion.Original].ToString().Trim()
					);
				}

				if (changeRow.RowState == DataRowState.Modified || changeRow.RowState == DataRowState.Added)
				{
					logText.AppendFormat(CultureInfo.InvariantCulture,
						"Code(after) {0} - Desc(after) {1}",
						changeRow[RefCountrySchema.Constants.RN_Code].ToString().Trim(),
						changeRow[RefCountrySchema.Constants.RN_Desc].ToString().Trim()
					);
				}

				logger.Log(LogType.Information, logText.ToString());
			}
		}
	}
}

#endif
