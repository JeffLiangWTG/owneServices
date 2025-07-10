using System;
using CargoWise.Data;

namespace Enterprise.DataTransfer.Native.DB
{
	public static class DataSetContext
	{
		public static DbConnection Connection
		{
			get { return connection ?? (connection = Db.Connection); }
			set { connection = value; }
		}

		[ThreadStatic]
		static DbConnection connection;

		public static void RestoreDefaultConnection()
		{
			Connection = Db.Connection;
		}
	}
}
