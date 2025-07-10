using System;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	public class SqlExceptionReformatterTest : TestCase
	{
		public void TestSqlErrorReformattedWithCorrectValues()
		{
			var exceptionCtors = typeof(SqlException).GetTypeInfo().DeclaredConstructors;
			var errorCollectionCtors = typeof(SqlErrorCollection).GetTypeInfo().DeclaredConstructors;
			var errorCtors = typeof(SqlError).GetTypeInfo().DeclaredConstructors;
			var assembly = Assembly.Load("CargoWise.EntityFramework");
			var formatter = new SqlExceptionReformatter();
			var add = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic);

			var error = (SqlError)errorCtors.First(c => c.GetParameters().Length == 8).Invoke(new object[] { 999, (byte)0, (byte)0, "Server", "{1f1f8280-72a2-45da-be79-53dbeb788dc0,False,10000} ~NotConcurrencyError~", "Procedure", 0, null });
			var error2 = (SqlError)errorCtors.First(c => c.GetParameters().Length == 8).Invoke(new object[] { 999, (byte)0, (byte)0, "Server", "{1f1f8280-72a2-45da-be79-53dbeb788dc0,True,50000} ~ConcurrencyError~", "Procedure", 0, null });
			var errorCollection = (SqlErrorCollection)errorCollectionCtors.First().Invoke(Array.Empty<Type>());
			add.Invoke(errorCollection, new object[] { error });
			add.Invoke(errorCollection, new object[] { error2 });
			var exception = (SqlException)exceptionCtors.First().Invoke(new object[] { "message", errorCollection, null, Guid.NewGuid() });
			var details = formatter.Reformat(exception);

			AssertEquals("1f1f8280-72a2-45da-be79-53dbeb788dc0", details.PrimaryKey.ToString());
			AssertEquals(true, details.IsConcurrencyError);
			AssertEquals("~NotConcurrencyError~", exception.Message);
			AssertEquals(10000, error.Number);
			AssertEquals("~NotConcurrencyError~", error.Message);
			AssertEquals(999, error2.Number);
			AssertEquals("~ConcurrencyError~", error2.Message);
		}

		public void TestSqlErrorReformattedWithDuplicatedIndex()
		{
			var exceptionCtors = typeof(SqlException).GetTypeInfo().DeclaredConstructors;
			var errorCollectionCtors = typeof(SqlErrorCollection).GetTypeInfo().DeclaredConstructors;
			var errorCtors = typeof(SqlError).GetTypeInfo().DeclaredConstructors;
			var assembly = Assembly.Load("CargoWise.EntityFramework");
			var formatter = new SqlExceptionReformatter();
			var add = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic);

			var error = (SqlError)errorCtors.First(c => c.GetParameters().Length == 8).Invoke(new object[] { 2601, (byte)0, (byte)0, "Server", "Cannot insert duplicate key row in object 'table' with unique index 'index'. The duplicate key value is (testValue, 12).", "Procedure", 0, null });
			var errorCollection = (SqlErrorCollection)errorCollectionCtors.First().Invoke(Array.Empty<Type>());
			add.Invoke(errorCollection, new object[] { error });
			var exception = (SqlException)exceptionCtors.First().Invoke(new object[] { "Cannot insert duplicate key row in object 'table' with unique index 'index'. The duplicate key value is (testValue, 12).", errorCollection, null, Guid.NewGuid() });
			var details = formatter.Reformat(exception);

			AssertEquals(Guid.Empty, details.PrimaryKey);
			AssertEquals("testValue, 12", details.DuplicatedValue);
		}

		public void TestSqlErrorReformattedWith_TriggerLikelyConcurrencyError()
		{
			var exceptionCtors = typeof(SqlException).GetTypeInfo().DeclaredConstructors;
			var errorCollectionCtors = typeof(SqlErrorCollection).GetTypeInfo().DeclaredConstructors;
			var errorCtors = typeof(SqlError).GetTypeInfo().DeclaredConstructors;
			var assembly = Assembly.Load("CargoWise.EntityFramework");
			var formatter = new SqlExceptionReformatter();
			var add = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic);

			var error = (SqlError)errorCtors.First(c => c.GetParameters().Length == 8).Invoke(new object[] { 999, (byte)0, (byte)0, "Server", $"{{1f1f8280-72a2-45da-be79-53dbeb788dc0,false,50000}} {ExceptionExtensions.TriggerPrefix.TriggerLikelyConcurrencyError}: Over-pick attempt.", "Procedure", 0, null });
			var errorCollection = (SqlErrorCollection)errorCollectionCtors.First().Invoke(Array.Empty<Type>());
			add.Invoke(errorCollection, new object[] { error });
			var exception = (SqlException)exceptionCtors.First().Invoke(new object[] { "message", errorCollection, null, Guid.NewGuid() });
			var details = formatter.Reformat(exception);

			CombineAssertions(() =>
			{
				AssertEquals("1f1f8280-72a2-45da-be79-53dbeb788dc0", details.PrimaryKey.ToString());
				AssertEquals(nameof(details.IsConcurrencyError), true, details.IsConcurrencyError);
				AssertEquals(nameof(details.IsConcurrencyTriggerError), true, details.IsConcurrencyTriggerError);
				AssertEquals("Over-pick attempt.", exception.Message);
				AssertEquals(50000, error.Number);
			});
		}
	}
}
