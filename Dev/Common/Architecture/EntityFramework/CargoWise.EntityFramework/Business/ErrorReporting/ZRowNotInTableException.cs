using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class ZRowNotInTableException : ZDataException
	{
		#region Constructor

		public ZRowNotInTableException(RowNotInTableException rowException, DataRow row, DbConnection connection)
			: base(rowException, GetRowNotInTableFriendlyMessage(), GetRowNotInTableDebugMessage(row), row, connection)
		{
		}

#if NETFRAMEWORK
		public ZRowNotInTableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#endregion

		#region Properties

		public override bool ShouldBeReportedToEDI
		{
			get { return true; }
		}

		#endregion

		#region Methods

		static string GetRowNotInTableFriendlyMessage()
		{
			return Res.GetString("b0a65637-3d68-416c-bb43-a23ebabfd825", "A local data row is in an unexpected state and cannot be saved. Please close the current form and retry the operation. If the problem continues, please contact your system administrator");
		}

		#region SuppressResourceStringsCheckRegion

		static string GetRowNotInTableDebugMessage(DataRow row)
		{
			ZStringBuilder errorBuilder = new ZStringBuilder();

			errorBuilder.Append(string.Format("DataRow RowError: {0}", row.RowError));

			if (row.Table == null)
			{
				errorBuilder.Append("The rows table is null");
			}
			else if (row.HasVersion(DataRowVersion.Original))
			{
				errorBuilder.Append("Original Row Data:");
				foreach (var column in row.Table.Columns.Cast<DataColumn>())
				{
					errorBuilder.Append(string.Format("{0} = {1}", column.ColumnName, row[column, DataRowVersion.Original].ToString()));
				}
			}

			return errorBuilder.ToStringWithNewLineBetweenAppends();
		}

		#endregion
		#endregion
	}
}
