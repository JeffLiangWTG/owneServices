using System;
using System.Data.SqlClient;
using System.Reflection;

namespace CargoWise.eHub.Gateway.Tests
{
	public static class SqlExceptionMock
	{
		public static SqlException CreateSqlException(string errorMessage, int errorNumber)
		{
			SqlErrorCollection collection = Construct<SqlErrorCollection>();
			SqlError error = Construct<SqlError>(errorNumber, (byte)2, (byte)3, "server name", errorMessage, "proc", 100, (uint)1);

			typeof(SqlErrorCollection)
				.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(collection, new object[] { error });

			var e = typeof(SqlException)
				.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.ExplicitThis, new[] { typeof(SqlErrorCollection), typeof(string) }, new ParameterModifier[] { })
				.Invoke(null, new object[] { collection, "11.0.0" }) as SqlException;

			return e;
		}

		static T Construct<T>(params object[] p)
		{
			var t = new Type[p.Length];
			for (var i = 0; i < p.Length; i++)
			{
				t[i] = p[i].GetType();
			}
			var constructorInfo = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, t, null);
			if (constructorInfo == null)
			{
				throw new InvalidOperationException(string.Format("Cannot find a matching private or static constructor for type {0} with the constructor parameters ({1})", typeof(T).Name, string.Join<Type>(", ", t)));
			}
			return (T)constructorInfo.Invoke(p);
		}
	}
}
