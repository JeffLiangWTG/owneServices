using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Environment
{
	public class BusinessObjectProxy<T> where T : BusinessObject
	{
		public BusinessObjectProxy(Guid primaryKey)
		{
			this.primaryKey = primaryKey;
		}

		readonly Guid primaryKey;

		public Guid PrimaryKey(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");
			var bizO = BusinessObject(factory);
			return bizO != null ? bizO.PK.ToGuid() : Guid.Empty;
		}

		public T BusinessObject(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");
			return factory.Load<T>(primaryKey);
		}
	}
}
