using System;
using CargoWise.EntityFramework;

namespace Enterprise.DataTransfer.Native.Adapter.Utils
{
	public class RowID
	{
		public RowID(Guid pk, string tableName)
		{
			this.PK = pk;
			this.TableName = tableName;
		}

		public readonly Guid PK;
		public readonly string TableName;

		public static implicit operator RowID(BusinessObject bizObj)
		{
			return new RowID(bizObj.PK.ToGuid(), bizObj.TableName);
		}
	}
}
