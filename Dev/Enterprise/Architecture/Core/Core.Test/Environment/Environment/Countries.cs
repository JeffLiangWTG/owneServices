using System;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public class Countries : System.Collections.Generic.List<String>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public Countries()
		{
			DbCommand command = Db.Connection.Command("select " + RefCountrySchema.RN_Code.Name + " from " + RefCountrySchema.Constants.SqlSchemaName + "." + RefCountrySchema.Constants.TableName);
			var table = new DataTable();
			command.NewDataAdapter().Fill(table);
			foreach (DataRow row in table.Rows)
			{
				Add((string)row[0]);
			}
		}
	}
}
