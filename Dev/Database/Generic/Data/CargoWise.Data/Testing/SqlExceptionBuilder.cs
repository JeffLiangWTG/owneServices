#if DEBUG
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.Data.Testing
{
	public static class SqlExceptionBuilder
	{
		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Full name needed")]
		public static TSqlException CreateSqlException<TSqlException>(int infoNumber, byte errorState, byte errorClass, string server, string errorMessage, string procedure, int lineNumber, Exception innerException = null)
			where TSqlException : System.Data.Common.DbException
		{
			Type sqlErrorType = null;
			Type sqlErrorCollectionType = null;

			if (typeof(TSqlException) == typeof(System.Data.SqlClient.SqlException))
			{
				sqlErrorCollectionType = typeof(System.Data.SqlClient.SqlErrorCollection);
				sqlErrorType = typeof(System.Data.SqlClient.SqlError);
			}
#if NET
			else if (typeof(TSqlException) == typeof(Microsoft.Data.SqlClient.SqlException))
			{
				sqlErrorCollectionType = typeof(Microsoft.Data.SqlClient.SqlErrorCollection);
				sqlErrorType = typeof(Microsoft.Data.SqlClient.SqlError);
			}
#endif
			else
			{
				throw new ArgumentException($"Unsupported SqlException type: {typeof(TSqlException).FullName}");
			}

			var sqlError = CreateSqlError(sqlErrorType, infoNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber);
			var sqlErrorCollection = CreateSqlErrorCollection(sqlErrorCollectionType, sqlErrorType, sqlError);
			return CreateSqlException(typeof(TSqlException), sqlErrorCollectionType, sqlErrorCollection, innerException) as TSqlException;
		}

		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Full name needed")]
		public static TSqlException CreateSqlException<TSqlException>(int errorNumber, string errorMessage)
			where TSqlException : System.Data.Common.DbException
			=> CreateSqlException<TSqlException>(errorNumber, byte.MaxValue, byte.MinValue, "CargoWise One", errorMessage, "", 0);

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Used in unit tests only.")]
		public static System.Data.Common.DbException CreateSqlException(int errorNumber, string errorMessage)
		{
			var sqlError = CreateSqlError(errorNumber, byte.MaxValue, byte.MinValue, "CargoWise One", errorMessage, "", 0);
			return CreateSqlException(CreateSqlErrorCollection(sqlError));
		}

		public static SqlException CreateSqlException(params SqlError[] errors)
		{
			Argument.NotNull(errors, nameof(errors)); // Suggested By ReviewBot 

			return CreateSqlException(CreateSqlErrorCollection(errors));
		}

		public static System.Data.Common.DbException CreateSqlException(int infoNumber, byte errorState, byte errorClass, string server, string errorMessage, string procedure, int lineNumber, Exception innerException = null)
		{
			return CreateSqlException(CreateSqlErrorCollection(CreateSqlError(infoNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber)), innerException);
		}

		public static SqlException CreateSqlException(SqlErrorCollection errorCollection, Exception innerException = null)
		{
			return CreateSqlException<SqlException, SqlErrorCollection>(errorCollection, innerException);
		}

		public static TSqlException CreateSqlException<TSqlException, TSqlErrorCollection>(TSqlErrorCollection errorCollection, Exception innerException = null)
			where TSqlException : System.Data.Common.DbException
			where TSqlErrorCollection : class, ICollection, IEnumerable
		{
			return CreateSqlException(typeof(TSqlException), typeof(TSqlErrorCollection), errorCollection, innerException) as TSqlException;
		}

		static object CreateSqlException(Type sqlExceptionType, Type sqlErrorCollectionType, object errorCollection, Exception innerException = null)
		{
			Argument.NotNull(errorCollection, nameof(errorCollection));

			MethodInfo method = GetNonPublicMethodForType(sqlExceptionType, "CreateException", BindingFlags.Static, new Type[] { sqlErrorCollectionType, typeof(string), typeof(Guid), typeof(Exception) });
			var ex = method.Invoke(null, new object[] { errorCollection, "", Guid.Empty, innerException });

			var fieldInfo = typeof(Exception).GetField("_stackTraceString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			fieldInfo.SetValue(ex, Environment.StackTrace);

			return ex;
		}

		#region CreateSqlErrorCollection
		public static SqlErrorCollection CreateSqlErrorCollection(params SqlError[] errors)
			=> CreateSqlErrorCollection<SqlErrorCollection, SqlError>(errors);

		public static TSqlErrorCollection CreateSqlErrorCollection<TSqlErrorCollection, TSqlError>(params TSqlError[] errors)
			where TSqlErrorCollection : class, ICollection, IEnumerable
			where TSqlError : class
			=> CreateSqlErrorCollection(typeof(TSqlErrorCollection), typeof(TSqlError), errors) as TSqlErrorCollection;

		static object CreateSqlErrorCollection(Type tSqlErrorCollectionType, Type sqlErrorType, params object[] errors)
		{
			Argument.NotNull(errors, nameof(errors)); // Suggested By ReviewBot 

			var resultObj = GetNonPublicParameterlessCtorForType(tSqlErrorCollectionType, Type.EmptyTypes).Invoke(null);

			MethodInfo addMethod = GetNonPublicMethodForType(tSqlErrorCollectionType, "Add", BindingFlags.Instance, new Type[] { sqlErrorType });
			foreach (SqlError error in errors)
			{
				addMethod.Invoke(resultObj, new object[] { error });
			}
			return resultObj;
		}
		#endregion

		#region CreateSqlError
		public static SqlError CreateSqlError(int infoNumber, byte errorState, byte errorClass, string server, string errorMessage, string procedure, int lineNumber)
			=> CreateSqlError<SqlError>(infoNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber);

		public static TSqlError CreateSqlError<TSqlError>(int infoNumber, byte errorState, byte errorClass, string server, string errorMessage, string procedure, int lineNumber) where TSqlError : class
			=> CreateSqlError(typeof(TSqlError), infoNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber) as TSqlError;

		static object CreateSqlError(Type tSqlErrorType, int infoNumber, byte errorState, byte errorClass, string server, string errorMessage, string procedure, int lineNumber)
		{
			object[] parameters = { infoNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber };
			var info = GetNonPublicParameterlessCtorForType(tSqlErrorType, new Type[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int) });

			if (info == null)
			{
				Array.Resize(ref parameters, parameters.Length + 1);
				parameters[parameters.Length - 1] = new Exception();
				info = GetNonPublicParameterlessCtorForType(tSqlErrorType, new Type[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(Exception) });
			}

			return info.Invoke(parameters);
		}
		#endregion

		#region Implementation

		static MethodInfo GetNonPublicMethodForType(Type type, string methodName, BindingFlags extraFlags, Type[] paramTypes)
		{
			Argument.NotNull(type, nameof(type)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(methodName, nameof(methodName));

			var result = type.GetMethod(methodName, BindingFlags.NonPublic | extraFlags, null, paramTypes, null);
			return result;
		}

		static ConstructorInfo GetNonPublicParameterlessCtorForType(Type type, Type[] paramTypes)
		{
			Argument.NotNull(type, nameof(type)); // Suggested By ReviewBot 

			var result = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null);
			return result;
		}

		#endregion
	}
}
#endif
