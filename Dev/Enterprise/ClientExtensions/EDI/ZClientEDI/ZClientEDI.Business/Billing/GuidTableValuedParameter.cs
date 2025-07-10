using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Database.Shared;

namespace Enterprise.Client.EDI.Billing.Business
{
	public sealed class GuidTableValuedParameter : IDisposable
	{
		public GuidTableValuedParameter(IEnumerable<Guid> pks)
		{
			dataTable = new DataTable();
			dataTable.Locale = CultureInfo.InvariantCulture;
			dataTable.Columns.Add("Value", typeof(Guid));

			foreach (var pk in pks)
			{
				dataTable.Rows.Add(new object[] { pk });
			}
		}

		public void AddTo(DbCommand cmd, string name)
		{
			cmd.AddTableValuedParameter(name, TVPHelper.TVP_uniqueidentifier, dataTable);
		}

		readonly DataTable dataTable;

		public void Dispose()
		{
			((IDisposable)dataTable).Dispose();
		}
	}
}
