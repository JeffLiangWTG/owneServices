using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data.HttpClient;
using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data
{
	/// <summary>
	/// System.Data.Common.DbCommand wrapper.
	/// </summary>
	public partial class DbCommand : IDbCommand
	{
		// Summary:
		//     Contains constants that specify infinite time-out intervals. This class cannot
		//     be inherited.
		public static class Timeout
		{
			// Summary:
			//     A constant used to specify an infinite waiting period.
			public const int Infinite = 0;

			public static int? Default { get { return null; } }

			public static int? Hours(int hours)
			{
				return (hours > 0) ? hours * 60 * 60 : Default;
			}

			public static int? Minutes(int minutes)
			{
				return (minutes > 0) ? minutes * 60 : Default;
			}
		}

		#region Constructors

		const string ExecuteAsReaderFlag = "573EC813-9CB8-4C68-9652-8C03F510993D";
		public const string ExecuteAsReaderFlagMask = "ExecuteAsReaderFlagMask"; // Replace the GUID in ExecuteAsReaderFlagComments
		public const string ExecuteAsReaderFlagComments = "\n /* " + ExecuteAsReaderFlag + " - This is a query Executed as a Reader. Please verify the owner of the query. If the query was created by the customer. Any errors need to be handled but the quality of the statement is out of scope. */ "; // Developer only message

		/// <summary>
		/// Creates a command in an DbConnection
		/// </summary>
		internal DbCommand(string commandText, DbConnection dbConnection, IDbConnection databaseConnection, IDbTransaction transaction, int commandTimeout)
		{
			Argument.NotNull(commandText, nameof(commandText)); // Suggested By ReviewBot 
			Argument.NotNull(dbConnection, nameof(dbConnection));
			Argument.NotNull(databaseConnection, nameof(databaseConnection)); // Suggested By ReviewBot
			if (commandTimeout < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(commandTimeout));
			}
#if DEBUG
			fCreatedWithTransactionOtherThanTransactionedTestCase = dbConnection.IsInTransactionOtherThanTransactionedTestCase;
#endif

			InternalCommand = dbConnection.DataProviderFactory.NewDbCommand(commandText, databaseConnection, transaction);
			InternalCommand.CommandTimeout = commandTimeout;
			fDbConnection = dbConnection;
		}

		#endregion

		#region Sanitize ExecuteAsReaderFlags

		public static string SanitizeExecuteAsReaderFlags(string commandText)
		{
			return commandText?.Replace(ExecuteAsReaderFlag, ExecuteAsReaderFlagMask);
		}

		#endregion

		#region Execute Command Methods

		public object ExecuteScalar()
		{
			object result = null;
			var cache = fDbConnection.Cache;
			if (cache != null)
			{
				if (!cache.GetValue(this, out result))
				{
					result = Execute(new ScalarCommandRunner());
					cache.SetValue(this, result);
				}
			}
			else
			{
				result = Execute(new ScalarCommandRunner());
			}
			return result;
		}

		public int ExecuteNonQuery()
		{
			return ((IDbCommand)this).ExecuteNonQuery();
		}

		public static void DisableAsync()
		{
			asyncDisallowed = true;
		}

		[ThreadStatic]
		static bool asyncDisallowed;
		public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
		{
			if (asyncDisallowed)
			{
				throw new InvalidOperationException("Async operations have been disabled on this thread");
			}

			if (cancellationToken == CancellationToken.None)
			{
				throw new ArgumentException("Cancellation token should not be empty.");
			}

			var cmd = InternalCommand as SqlCommand
				?? throw new InvalidOperationException("Only System.Data.SqlClient.SqlCommand supports async execution.");

			return await cmd.ExecuteNonQueryAsync(cancellationToken);
		}

		int IDbCommand.ExecuteNonQuery()
		{
			var result = Execute(new NonQueryCommandRunner());

			if (result != null)
			{
				return (int)result;
			}

			return -1;
		}

		public IDataReader ExecuteReader()
		{
			return ExecuteReader(CommandBehavior.Default);
		}

		public IDataReader ExecuteReader(CommandBehavior behaviour)
		{
			var result = (IDataReader)Execute(GetReaderCommandRunner(behaviour));

			return result;
		}

		ReaderCommandRunner GetReaderCommandRunner(CommandBehavior behaviour)
		{
#if DEBUG
			if (DbConnection.IsTrackingExecutedCommandQueryPlans)
			{
				return new Testing.ReaderCommandRunnerWithQueryPlanOutput(behaviour, this);
			}
#endif

			return new ReaderCommandRunner(behaviour);
		}

		public int ExecuteProcedureWithReturnValue()
		{
			if (CommandType != CommandType.StoredProcedure)
			{
				CommandType = CommandType.StoredProcedure;
			}

			if (!ContainParameter(ReturnValueParameterName))
			{
				AddReturnValueParameter();
			}

			ExecuteNonQuery();

			return ReturnValue;
		}

		#endregion

		#region Command Parameters

		protected DbParameterCollection InternalParameters
		{
			get
			{
				return InternalCommand.Parameters as DbParameterCollection;
			}
		}

		/// <summary>
		/// Adds a new parameter to the database command.
		/// </summary>
		/// <param name="parameterName"></param>
		/// <param name="parameterValue"></param>
		/// <param name="column"></param>
		public IDbDataParameter AddParameterBasedOnDbColumn(string parameterName, object parameterValue, ISchemaColumn column, bool enableAutoPlumpingForLikeSupport = false)
		{
			Argument.NotNull(column, nameof(column));

			if (GlobalServiceProvider.TryGetInstance(out var serviceProvider))
			{
				var conversion = serviceProvider.GetService<IDbValueConversion>();
				if (conversion != null && conversion.TryUnwrapSimpleValue(parameterValue, out var unwrappedValue))
				{
					parameterValue = unwrappedValue;
				}
			}

			if (column is ISchemaDecimalColumn decimalColumn)
			{
				return AddParameter(parameterName, decimalColumn.SqlDbType, 0, decimalColumn.Precision, decimalColumn.Scale, parameterValue);
			}
			else if (
					column.SqlDbType == SqlDbType.Char
					|| column.SqlDbType == SqlDbType.NChar
					|| column.SqlDbType == SqlDbType.NVarChar
					|| column.SqlDbType == SqlDbType.VarChar)
			{
				if (enableAutoPlumpingForLikeSupport && parameterValue != null)
				{
					return AddParameter(parameterName, column.SqlDbType, Math.Max(parameterValue.ToString().Length, column.MaxLength), parameterValue);
				}
				else
				{
					return AddParameter(parameterName, column.SqlDbType, column.MaxLength, parameterValue);
				}
			}
			else if (column.SqlDbType == SqlDbType.VarBinary)
			{
				return AddParameter(parameterName, column.SqlDbType, column.MaxLength, parameterValue);
			}
			else
			{
				return AddParameter(parameterName, column.SqlDbType, parameterValue);
			}
		}

		/// <summary>
		/// Adds a new parameter to the database command.
		/// </summary>
		/// <param name="name">Parameter Name (includes @ sign)</param>
		/// <param name="sqlDbType">Parameter Data Type (database type)</param>
		/// <param name="paramValue">Parameter Value</param>
		public IDbDataParameter AddParameter(string name, SqlDbType sqlDbType, object paramValue)
		{
			return AddParameter(name, sqlDbType, 0, paramValue);
		}

		/// <summary>
		/// Adds a new parameter to the database command.
		/// </summary>
		/// <param name="name">Parameter Name (includes @ sign)</param>
		/// <param name="sqlDbType">Parameter Data Type (database type)</param>
		/// <param name="size">Parameter Size (needed for character/binary types only)</param>
		/// <param name="paramValue">Parameter Value</param>
		public IDbDataParameter AddParameter(string name, SqlDbType sqlDbType, int size, object paramValue)
		{
			return AddParameter(name, sqlDbType, size, 0, 0, paramValue);
		}

		/// <summary>
		/// Adds a new parameter to the database command.
		/// This is the method that actualy adds the parameter and is called by all other overloads.
		/// </summary>
		/// <param name="name">Parameter Name (includes @ sign)</param>
		/// <param name="sqlDbType">Parameter Data Type (database type)</param>
		/// <param name="size">Parameter Size (if not known, use an overload that omits it)</param>
		/// <param name="precision">Parameter Numeric Precision (numeric/decimal types only)</param>
		/// <param name="scale">Parameter Numeric Scale (numeric/decimal types only)</param>
		/// <param name="paramValue">Parameter Value</param>
		public IDbDataParameter AddParameter(string name, SqlDbType sqlDbType, int size, byte precision, byte scale, object paramValue)
		{
			//if (size == 0 &&
			//    (sqlDbType == SqlDbType.VarChar ||
			//           sqlDbType == SqlDbType.NVarChar ||
			//           sqlDbType == SqlDbType.Char ||
			//           sqlDbType == SqlDbType.NChar))
			//{
			//    size = paramValue == null || paramValue.ToString().Length < 4000 ? 4000 : -1;
			//}

			var parameter = fDbConnection.DataProviderFactory.NewDbParameter(name, sqlDbType, size);
			AddParameter(parameter, precision, scale, paramValue);
			return parameter;
		}

		public void AddParameter(IDbDataParameter parameter, byte precision, byte scale, object paramValue)
		{
			Argument.NotNull(parameter, nameof(parameter)); // Suggested By ReviewBot 

			parameter.Precision = precision;
			parameter.Scale = scale;
			parameter.Value = paramValue;
			//#if DEBUG
			//            if (CommandType != CommandType.StoredProcedure && parameter.Size == 0 && (paramValue is string || paramValue is ZString))
			//			{
			//				ErrorReporter.ReportOnce(parameter.ParameterName, "You must use the version of AddParameter that takes a maximum length for the field being queried");
			//			}
			//#endif
			InternalCommand.Parameters.Add(parameter);
		}

		public void AddUdtParameter(string name, string udtTypeName, object paramValue)
		{
			var parameter = fDbConnection.DataProviderFactory.NewDbParameter(name, SqlDbType.Udt, 0);
			if (parameter is DbParameter sqlParameter)
			{
				new SqlParameterWrapper(sqlParameter).UdtTypeName = udtTypeName;
			}
			else
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Parameter type {parameter.GetType()} does not support UDT parameter types"));
			}
			AddParameter(parameter, 0, 0, paramValue);
		}

		public void AddParameters(IStructuralEquatable[] cmdParamsAsValueTuple)
		{
			if (cmdParamsAsValueTuple != null && cmdParamsAsValueTuple.Any())
			{
				var arrayToUse = cmdParamsAsValueTuple;
				while (arrayToUse.FirstOrDefault() is IEnumerable newItem)
				{
					arrayToUse = newItem.Cast<IStructuralEquatable>().ToArray();
				}

				if (arrayToUse.FirstOrDefault() is object obj && ActionBasedOnArrayType.TryGetValue(obj.GetType(), out Action<IStructuralEquatable[]> action))
				{
					action.Invoke(arrayToUse);
				}
				else if (arrayToUse.Any())
				{
					throw new InvalidOperationException("This method can only be used with Tuple or ValueTuple and only with signatures matching a AddParameter method header specifying a parameter size.");
				}
			}
		}

		Dictionary<Type, Action<IStructuralEquatable[]>> ActionBasedOnArrayType
		{
			get
			{
				if (actionSwitch == null)
				{
					actionSwitch = new Dictionary<Type, Action<IStructuralEquatable[]>>
					{
						//public void AddParameter(string name, SqlDbType sqlDbType, object paramValue)
						// We do *NOT* want to allow parameters without a size as this makes for a redundant parameter.

						//public void AddParameter(string name, SqlDbType sqlDbType, int size, object paramValue)
						{ typeof(ValueTuple<string, SqlDbType, int, object>), array => AddParameterFromTuple(param => AddParameter((string)ReflectItemFromTuple(param, "Item1"), (SqlDbType)ReflectItemFromTuple(param, "Item2"), (int)ReflectItemFromTuple(param, "Item3"), ReflectItemFromTuple(param, "Item4")), array) },
						{ typeof(Tuple<string, SqlDbType, int, object>), array => AddParameterFromTuple(param => AddParameter((string)ReflectItemFromTuple(param, "Item1"), (SqlDbType)ReflectItemFromTuple(param, "Item2"), (int)ReflectItemFromTuple(param, "Item3"), ReflectItemFromTuple(param, "Item4")), array) },

						//public void AddParameter(IDbDataParameter parameter, byte precision, byte scale, object paramValue)
						{ typeof(ValueTuple<IDbDataParameter, byte, byte, object>), array => AddParameterFromTuple(param => AddParameter((IDbDataParameter)ReflectItemFromTuple(param, "Item1"), (byte)ReflectItemFromTuple(param, "Item2"), (byte)ReflectItemFromTuple(param, "Item3"), ReflectItemFromTuple(param, "Item4")), array) },
						{ typeof(Tuple<IDbDataParameter, byte, byte, object>), array => AddParameterFromTuple(param => AddParameter((IDbDataParameter)ReflectItemFromTuple(param, "Item1"), (byte)ReflectItemFromTuple(param, "Item2"), (byte)ReflectItemFromTuple(param, "Item3"), ReflectItemFromTuple(param, "Item4")), array) },

						//public IDbDataParameter AddParameter(string name, SqlDbType sqlDbType, int size, byte precision, byte scale, object paramValue)
						{ typeof(ValueTuple<string, SqlDbType, int, byte, byte, object>), array => AddParameterFromTuple(param => AddParameter((string)ReflectItemFromTuple(param, "Item1"), (SqlDbType)ReflectItemFromTuple(param, "Item2"), (int)ReflectItemFromTuple(param, "Item3"), (byte)ReflectItemFromTuple(param, "Item4"), (byte)ReflectItemFromTuple(param, "Item5"), ReflectItemFromTuple(param, "Item6")), array) },
						{ typeof(Tuple<string, SqlDbType, int, byte, byte, object>), array => AddParameterFromTuple(param => AddParameter((string)ReflectItemFromTuple(param, "Item1"), (SqlDbType)ReflectItemFromTuple(param, "Item2"), (int)ReflectItemFromTuple(param, "Item3"), (byte)ReflectItemFromTuple(param, "Item4"), (byte)ReflectItemFromTuple(param, "Item5"), ReflectItemFromTuple(param, "Item6")), array) }
					};
				}
				return actionSwitch;
			}
		}

		Dictionary<Type, Action<IStructuralEquatable[]>> actionSwitch;

		object ReflectItemFromTuple(ValueType tuple, string itemNumber)
		{
			return tuple?.GetType().GetField(itemNumber)?.GetValue(tuple);
		}

		void AddParameterFromTuple(Action<ValueType> action, IEnumerable array)
		{
			foreach (ValueType param in array)
			{
				action(param);
			}
		}

		#region TVP

		public void AddTableValuedParameter(string parameterName, SchemaColumn column, IEnumerable values)
		{
			Argument.NotNull(values, nameof(values));
			Argument.NotNull(column, nameof(column));

			if (!column.SupportTVP)
			{
				throw new InvalidOperationException("SchemaColumn should support Table Valued Parameter.");
			}

			var table = new DataTable() { Locale = CultureInfo.InvariantCulture };
			var tableColumn = table.Columns.Add("Value", column.DotNetType); // Column name
			tableColumn.AllowDBNull = false;
			table.PrimaryKey = new DataColumn[] { tableColumn };

			var converter = GlobalServiceProvider.TryGetInstance(out var serviceProvider)
				? serviceProvider.GetService<IDbValueConversion>()?.TryGetConverterForValues(values)
				: null;

			foreach (var value in values)
			{
				var row = converter is null ? value : converter.ConvertTo(value, column.DotNetType);

				if (!table.Rows.Contains(row))
				{
					table.Rows.Add(row);
				}
			}

			AddTableValuedParameter(parameterName, column.TVPName, table);
		}

		/// <summary>
		/// Adds a new table valued parameter to the database command.
		/// </summary>
		/// <param name="parameterName">Parameter Name (includes @ sign)</param>
		/// <param name="parameterTypeName">Table-Valued Parameter Type Name (database type)</param>
		/// <param name="parameterValue">DataTable with Parameter Value</param>
		public void AddTableValuedParameter(string parameterName, string parameterTypeName, DataTable parameterValue)
		{
			Argument.NotNull(parameterValue, nameof(parameterValue));
			if (string.IsNullOrWhiteSpace(parameterValue.TableName))
			{
				parameterValue.TableName = string.Format(CultureInfo.InvariantCulture, "Data{0}", Guid.NewGuid()); // data table id
			}
			var param = new SqlParameter(parameterName, SqlDbType.Structured);
			param.TypeName = parameterTypeName;
			param.Value = parameterValue;
			InternalCommand.Parameters.Add(param);
		}

		public void AddTableValuedParameter<T>(string parameterName, string parameterTypeName, IEnumerable<T> values)
		{
			Argument.NotNull(values, nameof(values)); // Suggested By ReviewBot 

			using (var dataTable = new DataTable { Locale = CultureInfo.InvariantCulture })
			{
				dataTable.Columns.Add("Value", typeof(T)); // used only by developer

				foreach (var value in values)
				{
					dataTable.Rows.Add(value);
				}

				AddTableValuedParameter(parameterName, parameterTypeName, dataTable);
			}
		}

		#endregion // TVP

		/// <summary>
		/// Adds a new OUTPUT parameter to the database command.
		/// </summary>
		public IDbDataParameter AddOutputParameter(string name, SqlDbType dbType, int size, byte precision, byte scale, object value)
		{
			var parameter = AddParameter(name, dbType, size, precision, scale, value);
			parameter.Direction = ParameterDirection.Output;
			return parameter;
		}

		/// <summary>
		/// Sets the value of a given parameter.
		/// Note: Lets IndexOutOfRangeException be thrown if the parameter is not in the collection.
		/// </summary>
		public void SetParameterValue(string parameterName, object paramValue)
		{
			var parameter = InternalCommand.Parameters[parameterName];
			var dbDataParameter = parameter as IDbDataParameter;
			if (dbDataParameter != null)
			{
				dbDataParameter.Value = paramValue;
			}
			else
			{
				throw new OdysseyDataException("dbDataParameter is not IDbDataParameter");
			}
		}

		/// <summary>
		/// Gets the value of a given parameter.
		/// Note: Lets IndexOutOfRangeException be thrown if the parameter is not in the collection.
		/// </summary>
		public object GetParameterValue(string parameterName)
		{
			var parameter = InternalParameters[parameterName];
			return parameter != null ? parameter.Value : null;
		}

		/// <summary>
		/// Parameters should only be pulled like this for testing
		/// </summary>
		/// <param name="parameterName"></param>
		/// <returns></returns>
		public DbParameter GetParameter(string parameterName)
		{
			return InternalParameters[parameterName];
		}

		/// <summary>
		/// Removes a given parameter from the parameter collection
		/// </summary>
		public void RemoveParameterIfExists(string parameterName)
		{
			if (ContainParameter(parameterName))
			{
				InternalCommand.Parameters.RemoveAt(parameterName);
			}
		}

		/// <summary>
		/// Checks if the command parameter collection contain a parameter with a given name
		/// </summary>
		public bool ContainParameter(string parameterName)
		{
			return InternalCommand.Parameters.Contains(parameterName);
		}

		public int ParameterCount
		{
			get
			{
				return InternalCommand.Parameters.Count;
			}
		}

		public bool ParameterCollectionEquals(DbCommand comparingCommand)
		{
			Argument.NotNull(comparingCommand, nameof(comparingCommand)); // Suggested By ReviewBot 

			bool result = false;

			var parameters = InternalCommand.Parameters;

			if (parameters.Count == comparingCommand.InternalCommand.Parameters.Count)
			{
				result = true;

				for (int i = 0; i < parameters.Count; i++)
				{
					var parameter = InternalParameters[i];
					if (parameter != null)
					{
						var name = parameter.ParameterName;
						string parameterName = name;
						if (!comparingCommand.ContainParameter(parameterName)
								|| !AreParametersEqual(InternalParameters[i], comparingCommand.InternalParameters[parameterName]))
						{
							result = false;
							break;
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Parameters will be regarded as equal if they have the same:
		/// Name, SqlDbType and Value.
		/// </summary>
		protected bool AreParametersEqual(DbParameter param1, DbParameter param2)
		{
			return
				param1 != null
				&& param2 != null
				&& param1.ParameterName == param2.ParameterName
				&& param1.DbType.Equals(param2.DbType)
				&& param1.Value.Equals(param2.Value);
		}

		#region Stored Procedures Return Value

		protected const string ReturnValueParameterName = "@StoredProcedureReturnValue";

		/// <summary>
		/// Adds a return value parameter for stored procedures.
		/// </summary>
		public void AddReturnValueParameter()
		{
			var parameter = fDbConnection.DataProviderFactory.NewDbParameter(ReturnValueParameterName, SqlDbType.Int, 0);
			parameter.Direction = ParameterDirection.ReturnValue;
			InternalCommand.Parameters.Add(parameter);
		}

		public int ReturnValue
		{
			get
			{
				var parameter = InternalParameters[ReturnValueParameterName];
				if (parameter != null && parameter.Value != null)
				{
					return (int)parameter.Value;
				}

				throw new InvalidOperationException("Value not found");
			}
		}

		#endregion

		#endregion

		#region Properties

		public DbConnection DbConnection
		{
			get { return fDbConnection; }
		}

		readonly DbConnection fDbConnection;

		public IDbConnection Connection
		{
			get
			{
				return InternalCommand.Connection;
			}
			set { throw new ReadOnlyException("DbCommand Connection cannot be set. It's readonly."); }
		}

		public IDbTransaction Transaction
		{
			get
			{
				return InternalCommand.Transaction;
			}
			set { throw new ReadOnlyException("DbCommand Transaction cannot be set. It's readonly."); }
		}

		#region IDbCommand Properties

		public string CommandText
		{
			get
			{
				return InternalCommand.CommandText;
			}
			set
			{
				Argument.NotNull(value, "value"); //can't add a contract since this is an override.
				InternalCommand.CommandText = value;
			}
		}

		public CommandType CommandType
		{
			get
			{
				return InternalCommand.CommandType;
			}
			set
			{
				InternalCommand.CommandType = value;
			}
		}

		public int CommandTimeout
		{
			get
			{
				return InternalCommand.CommandTimeout;
			}
			set
			{
				InternalCommand.CommandTimeout = value;
			}
		}

		public UpdateRowSource UpdatedRowSource
		{
			get
			{
				return InternalCommand.UpdatedRowSource;
			}
			set
			{
				InternalCommand.UpdatedRowSource = value;
			}
		}

		#endregion

		#endregion

		#region DataAdapter

		/// <summary>
		/// Returns a new DataAdapter having this command as its SelectCommand.
		/// In a multi-threaded environment, you must lock the connection until this object is disposed.
		/// </summary>
		public IReadOnlyDataAdapter NewDataAdapter(int timeoutSeconds = 0)
		{
			return new ReadOnlyDataAdapter(InternalCommand, fDbConnection, timeoutSeconds);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			InternalCommand.Dispose();
		}

		#endregion

		#region IDbCommand Members

		public void Cancel()
		{
			InternalCommand.Cancel();
		}

		public void Prepare()
		{
			InternalCommand.Prepare();
		}

		#endregion

		#region Implementation

		internal readonly IDbCommand InternalCommand;

#if DEBUG
		readonly bool fCreatedWithTransactionOtherThanTransactionedTestCase;
#endif

		#region IDbCommand Members - Not Published

		IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
		{
			return ExecuteReader(behavior);
		}

		IDataReader IDbCommand.ExecuteReader()
		{
			return ExecuteReader();
		}

		IDbDataParameter IDbCommand.CreateParameter()
		{
			throw new InvalidOperationException("DbCommand CreateParameter is for internal use only. It should not be called.");
		}

		IDataParameterCollection IDbCommand.Parameters
		{
			get { throw new InvalidOperationException("DbCommand Parameters Collection is for internal use only. It should not be referenced."); }
		}

		#endregion

		#region Execute Command Strategies (CommandRunner classes)

		/// <summary>
		/// Executes a DB command using the given runner.
		/// Each runner represents a different running strategy:
		///   - ExecuteScalar (ScalarCommandRunner) 
		///     Runs a command, returning the 1st field of the 1st row of the ResultSet
		///   - ExecuteNonQuery (NonQueryCommandRunner)
		///     Runs a command, returning only the number of affect rows
		///   - ExecuteReader (ReaderCommandRunner) - 
		///     Runs a command, returning a DataReader
		/// </summary>
		protected object Execute(CommandRunner cmdRunner)
		{
			Argument.NotNull(cmdRunner, nameof(cmdRunner));

			PerformConnectionChecksAndSetDebugCountersBeforeExecute();
			CheckNoReadpastWithoutReadReadcommittedlock();
			CheckSelectStartInSqlText_ForTest();

			var stopwatch = Stopwatch.StartNew();
			try
			{
#if DEBUG
				AddTrackerCommentsToCommandForTest(this);
				DbConnection.InvokeOnBeforeExecute(this);
#endif
				object result = ExecuteCore(cmdRunner);

				// Track executed commands (ExceptionReport use)
				AddUserEventTrackerSqlEvent(InternalCommand, null, stopwatch.Elapsed);
#if DEBUG
				DbConnection.InvokeOnExecute(this);
#endif

				return result;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				AddUserEventTrackerSqlEvent(InternalCommand, ex, stopwatch.Elapsed);
				if (Connection is HttpConnection httpConnection)
				{
					httpConnection.HandleCommandException(ex);
				}
				throw;
			}
		}

		partial void CheckSelectStartInSqlText_ForTest();

		object ExecuteCore(CommandRunner cmdRunner)
		{
			Argument.NotNull(cmdRunner, nameof(cmdRunner)); // Suggested By ReviewBot 

			int retries = 0;
			while (true)
			{
				try
				{
					return ExecuteAsReaderLoginIfNeeded(cmdRunner);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					bool isInTransaction = (fDbConnection.AppTransactionCount > 0);
					bool errorHandled = fDbConnection.ConnectionErrorHandler.HandleDisconnectionAndSecurityErrors(ex);

					if (errorHandled && !isInTransaction && retries < 3)
					{
						++retries;
					}
					else
					{
						if (ex is InvalidOperationException && ex.Source == "System.Data")
						{
							throw new CommunicationException("Unable to execute command on the database", ex);
						}
						throw;
					}
				}
			}
		}

		partial void OverrideCommandRunner_ForTest(ref CommandRunner cmdRunner);

		object ExecuteAsReaderLoginIfNeeded(CommandRunner cmdRunner)
		{
			Argument.NotNull(cmdRunner, nameof(cmdRunner)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(InternalCommand.CommandText, nameof(InternalCommand.CommandText));

			DbConnection.RefreshLastActivity();
			OverrideCommandRunner_ForTest(ref cmdRunner);

			if (InternalCommand.CommandText.Contains(DbCommand.ExecuteAsReaderFlag) && !IsSaveCommand)
			{
				return new ExecuteAsReader().Execute(fDbConnection, () => cmdRunner.Execute(InternalCommand));
			}
			else
			{
				return cmdRunner.Execute(InternalCommand);
			}
		}

		public bool IsSaveCommand { get; set; }

		void PerformConnectionChecksAndSetDebugCountersBeforeExecute()
		{
			Argument.NotNullOrEmpty(InternalCommand.CommandText, nameof(InternalCommand.CommandText));
			CheckAppTransactionRolledBackInDbServer();

#if DEBUG
			CheckCreatedNotInTransactionAndNowIsInTransaction();
			DbConnection.IncrementExecutedCommandCount(CommandText, InternalCommand.Parameters);
#endif
		}

		void CheckAppTransactionRolledBackInDbServer()
		{
			if (Connection is HttpConnection)
			{
				// Do Nothing - this check will be performed in the middler tier
			}
			else
			{
				if (fDbConnection.AppTransactionCount > 0 && !fDbConnection.IsInTransaction)
				{
					throw new TransactionException("Transaction has been rolled back in the server (application transaction count pending reset).", OdysseyDataErrorType.TransactionRolledBack);
				}
			}
		}

		//Checks that query does not contain hint READPAST unless READCOMMITTEDLOCK is also specified
		void CheckNoReadpastWithoutReadReadcommittedlock()
		{
			var sqlText = InternalCommand.CommandText?.ToUpperInvariant();

			if (sqlText.IsNullOrEmpty() || !sqlText.Contains("READPAST"))
			{
				return;
			}

			//the hints are always in brakets without any nested brackets (e.g. WIHT (READPAST, UPDLOCK) ) so get the list of such substrings
			//and check for READPAST without READCOMMITTEDLOCK
			var readpastsWithoutReadcommitedlock = GetStringsInBrackets(sqlText).Any(s => s.Contains("READPAST")
			&& !s.Contains("READCOMMITTEDLOCK") && !s.Contains("UPDLOCK") && !s.Contains("XLOCK") && !s.Contains("REPEATABLEREAD")); //when UPDLOCK is specified the READCOMMITTEDLOCK isolation level hint is ignored; REPEATABLEREAD is higher isolation level

			if (readpastsWithoutReadcommitedlock)
			{
				ErrorReporter.ReportOnce("ReadpastWithoutReadCommittedLock", "There is READPAST without READCOMMITTEDLOCK in sql.\r\nIf this is sql in a comment or non sql field, please ensure \"READPAST\" is not wrapped with parenthesis - ie. change from (* READPAST *) to * READPAST *:\r\n\r\n" + InternalCommand.CommandText);
			}
		}

		// Capture the hint string inside the parentheses of WITH() clauses, and avoid matching WITH clauses inside variable assignments
		// Assume that an "@" followed by a number as a variable name is usually generated by CW1.
		// For example, we don't need to report: "@1 = 'SELECT * FROM sys.tables WITH (READPAST)'"
		IEnumerable<string> GetStringsInBrackets(string text)
		{
			var matches = ReadpastRegex.Matches(text);
			foreach (Match match in matches)
			{
				yield return match.Groups["Hint"].Value;
			}
		}

		// "(?<!...)" is a negative lookbehind, ensuring that the specified pattern does not appear before the current match position.
		//		-	For the regex pattern (?<!abc)def, it matches "def" in "xyzdef" but not in "abcdef".
		// "@\d+\s*=\s*N?" matches "@1 = N'content'".(Assume that an "@" followed by a number as a variable name is usually generated by CW1)
		//		-	"\d" matches any digit character. (1 or mor time).
		//		-	"\s*" matches any whitespace character (0 or more times).
		//		-	"N?" matches the character "N" literally (0 or 1 time).
		// "'(?:[^']|'')*" matches any sequence of characters that are either not single quotes or are two consecutive single quotes, repeated zero or more times.
		//		-	Thery are content of string, such as "''a'' is WITH (READPAST)" in "SET @1 = '''a'' is WITH (READPAST)'".
		// "\bWITH\s*"  matches the word "WITH" as a whole word.
		//		-	"\b" asserts position at a word boundary.(e.g., it matches "WITH" but not "WITHIN").
		// "(?<Hint>...)" is capturing group named "Hint". This group captures everything inside the outer parentheses.
		// "(?: [^()]+ | (?<Depth>\() | (?<-Depth>\) )*" is a non-capturing group with 3 conditions (More information can refer to usage of Balancing Group of Regex):
		//		-	"[^()]+" Matches any character that is not a parenthesis, repeated one or more times.
		//		-	"(?<Depth>\()" Matches an opening parenthesis "(" and increments a named group counter "Depth". This is used to keep track of nested parentheses.
		//		-	"(?<-Depth>\))" Matches a closing parenthesis ")" and decrements the named group counter "Depth". This ensures that nested parentheses are properly balanced.
		// "(?(Depth)(?!))" is a conditional expression that checks if the Depth counter is not zero (meaning there are unclosed parentheses). If Depth is not zero, the negative lookahead.
		static readonly Regex ReadpastRegex = new Regex(@"(?<!@\d+\s*=\s*N?'(?:[^']|'')*)\bWITH\s*\((?<Hint>(?:[^()]+|(?<Depth>\()|(?<-Depth>\)))*(?(Depth)(?!)))\)", RegexOptions.Compiled);

#if DEBUG
		void CheckCreatedNotInTransactionAndNowIsInTransaction()
		{
			if (!fCreatedWithTransactionOtherThanTransactionedTestCase && DbConnection.IsInTransactionOtherThanTransactionedTestCase)
			{
				throw new TransactionException("This command was created without a transaction, but now it is being executed inside a transaction. If this were running outside of a TransactionedTestCase, .NET would throw an exception saying: Execute requires the command to have a transaction object when the connection assigned to the command is in a pending local transaction. The Transaction property of the command has not been initialized.");
			}
		}
#endif

		/// <summary>
		/// Track command executions and failures
		/// </summary>
		protected void AddUserEventTrackerSqlEvent(IDbCommand cmdToTrack, Exception e, TimeSpan elapsedTime)
		{
			Argument.NotNull(cmdToTrack, nameof(cmdToTrack));
			try
			{
				SqlEventTracker.Instance.AddSqlEvent(cmdToTrack, e, elapsedTime);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Diagnostic only. No point throwing exceptions
			}
		}

#if DEBUG
		protected void AddTrackerCommentsToCommandForTest(DbCommand command)
		{
			Argument.NotNull(command, nameof(command));

			try
			{
				DbCommandExecuteTracker.Instance.BeforeExecute_AddExecuteCountForTest(command);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Diagnostic only. No point throwing exceptions
			}
		}
#endif

		#endregion

		#endregion
	}
}

#region Test
#if DEBUG

namespace CargoWise.Data
{
	public delegate void CommandRunnerOverride(ref CommandRunner commandRunner);

	public partial class DbCommand
	{
		static readonly Overridable<CommandRunnerOverride> commandRunnerOverride = new Overridable<CommandRunnerOverride>(null);
		static readonly bool checkSelectStar;

		// Complicated regular expression such as @"\s*SELECT\s*(\w+.*,\s*)*(\*|\w+\.\*)\s*" can't be used here
		// Many unit tests will fail because they run very slow if using complicated regular expresssion
		// Because of this, not all forms of "SELECT *" will be detected here
		static readonly Regex selectStarRegex = new Regex(@"\s*SELECT\s*\*\s*", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static IDisposable SetCommandRunnerOverrideForTest(CommandRunnerOverride commandRunnerOverride)
		{
			DbCommand.commandRunnerOverride.Value = commandRunnerOverride;
			return new DisposableAction(() => { SetCommandRunnerOverrideForTest(null); });
		}

		partial void CheckSelectStartInSqlText_ForTest()
		{
			if (DbCommand.checkSelectStar)
			{
				var sqlText = CommandText;
				if (sqlText.IndexOf("*") > 0 && sqlText.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					if (DbCommand.selectStarRegex.IsMatch(sqlText))
					{
						ErrorReporter.ReportOnce("There is \"SELECT *\" in SQL text: " + sqlText);
					}
				}
			}
		}

		partial void OverrideCommandRunner_ForTest(ref CommandRunner cmdRunner)
		{
			commandRunnerOverride.Value?.Invoke(ref cmdRunner);
		}
	}
}

#endif
#endregion
