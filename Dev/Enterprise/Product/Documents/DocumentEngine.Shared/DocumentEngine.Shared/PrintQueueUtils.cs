using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.DocumentEngine.Shared
{
	public static class PrintQueueUtils
	{
		public static IEnumerable<T> GetReferencedRecords<T>(BusinessObjectFactory factory, object value, string tableName, string column) where T : class
		{
			return factory.Load<T>(new ZQuery(ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(column, tableName), value));
		}
	}
}
