using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.Data.SqlProxy.Interface;

public static class SqlExceptionBuilder
{
	[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Used in unit tests only.")]
	public static SqlException? CreateSqlException(int errorNumber, string errorMessage = "", string? stackTrace = null, Exception? innerException = null)
	{
		var sqlError =
			CreateSqlError(errorNumber, byte.MaxValue, byte.MinValue, "", errorMessage, "", 0)
			?? throw new InvalidOperationException($"Failed to create instance of {nameof(SqlError)}");

		return CreateSqlException(CreateSqlErrorCollection(sqlError), stackTrace, innerException);
	}

	static SqlException? CreateSqlException(SqlErrorCollection errorCollection, string? stackTrace = null, Exception? innerException = null)
	{
		Argument.NotNull(errorCollection, nameof(errorCollection));

		var methodInfo = GetNonPublicMethodForType(typeof(SqlException), "CreateException", BindingFlags.Static, [typeof(SqlErrorCollection), typeof(string), typeof(Guid), typeof(Exception)]);
		var ex = methodInfo.Invoke(null, [errorCollection, "", Guid.Empty, innerException]);

		if (stackTrace != null)
		{
			var fieldInfo = typeof(SqlException).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
			fieldInfo?.SetValue(ex, stackTrace);
		}

		return ex as SqlException;
	}

	static SqlErrorCollection CreateSqlErrorCollection(params SqlError[] errors)
	{
		var sqlErrorCollectionCtor =
			GetNonPublicParameterlessCtorForType(typeof(SqlErrorCollection), Type.EmptyTypes)?.Invoke(null)
			?? throw new InvalidOperationException($"Failed to create instance of {nameof(SqlErrorCollection)}");

		var result = sqlErrorCollectionCtor as SqlErrorCollection;
		var methodInfo = GetNonPublicMethodForType(typeof(SqlErrorCollection), "Add", BindingFlags.Instance, new[] { typeof(SqlError) });
		foreach (var error in errors)
		{
			methodInfo.Invoke(result, [error]);
		}

		return result!;
	}

	static SqlError? CreateSqlError(int infoNumber, byte errorState, byte errorClass, string server, string errorMessage, string procedure, int lineNumber)
	{
		object[] parameters = [infoNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber];
		var ctorInfo = GetNonPublicParameterlessCtorForType(typeof(SqlError), [typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int)]);

		if (ctorInfo == null)
		{
			Array.Resize(ref parameters, parameters.Length + 1);
			parameters[parameters.Length - 1] = new Exception();
			ctorInfo = GetNonPublicParameterlessCtorForType(typeof(SqlError), [typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(Exception)]);
		}

		if (ctorInfo == null)
		{
			throw new InvalidOperationException($"Failed to create instance of {nameof(SqlError)}");
		}

		return ctorInfo!.Invoke(parameters) as SqlError;
	}

	#region Implementation

	static MethodInfo GetNonPublicMethodForType(Type type, string methodName, BindingFlags extraFlags, Type[] paramTypes)
	{
		return
			type.GetMethod(methodName, BindingFlags.NonPublic | extraFlags, null, paramTypes, null)
			?? throw new InvalidOperationException($"{nameof(GetNonPublicMethodForType)} failed to get method for {methodName} on type: {type.FullName}");
	}

	static ConstructorInfo? GetNonPublicParameterlessCtorForType(Type type, Type[] paramTypes)
	{
		return
			type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null)
			?? throw new InvalidOperationException($"{nameof(GetNonPublicParameterlessCtorForType)} failed to get ctor for on type: {type.FullName}");
	}

	#endregion
}
