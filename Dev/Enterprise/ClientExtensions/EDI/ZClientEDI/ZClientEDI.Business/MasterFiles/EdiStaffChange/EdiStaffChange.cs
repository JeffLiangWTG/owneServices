using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.HR
{
	public class EdiStaffChange : AutoEdiStaffChange
	{
		public EdiStaffChange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static void ClearStaffChangesTable()
		{
			using (var cmd = Db.Connection.Command("truncate table " + Schema.TableName))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}

