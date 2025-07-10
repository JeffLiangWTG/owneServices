using System.Data;

namespace CargoWise.EntityFramework
{
	class ZSqlDataAccessor : ZAccessor
	{
		public ZSqlDataAccessor(DataSet data, ZSqlConnectionInfo connectionInfo)
			: base(data)
		{
			ConnectionInfo = connectionInfo;
		}

		protected override ZLoader Loader
		{
			get { return new ZSqlLoader(Data, ConnectionInfo, SchemaResolver); }
		}

		protected override ZSaver Saver
		{
			get { return new ZSqlSaver(Data, ConnectionInfo, SchemaResolver); }
		}

		protected readonly ZSqlConnectionInfo ConnectionInfo;
	}
}
