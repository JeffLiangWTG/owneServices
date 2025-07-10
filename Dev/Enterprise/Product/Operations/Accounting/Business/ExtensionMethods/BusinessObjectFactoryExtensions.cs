using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public static class BusinessObjectFactoryExtensions
	{
		public static T LoadScalarValue<T>(this BusinessObjectFactory factory, string sqlText, ZSqlParameterCollection parameters) where T : IZType
			=> factory.LoadScalarValue<T>(sqlText, parameters.ToArray());

		public static T LoadScalarValue<T>(this BusinessObjectFactory factory, string sqlText, params ZSqlParameter[] parameters) where T : IZType
		{
			var result = default(T);

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sqlText, parameters);

			if (collection.Any())
			{
				if (collection.First().PropertyNames.Length > 1 || collection.Count > 1)
				{
					throw new InvalidOperationException(FormattableString.Invariant($"Incompatible result table returned by the query. So a scalar value cannot be returned.\r\nTable details:\r\nColumns: {string.Join(", ", collection.First().PropertyNames)}.\r\nNumber of rows: {collection.Count}."));
				}
				else
				{
					var obj = collection.First()[collection.First().PropertyNames[0]];
					if (obj != null)
					{
						result = (T)obj;
					}
				}
			}

			return result;
		}
	}
}
