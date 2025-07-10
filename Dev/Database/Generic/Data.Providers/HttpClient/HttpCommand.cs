using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;

namespace CargoWise.Data.HttpClient
{
	public class HttpCommand : DbCommand
	{
		internal HttpCommand(HttpConnection httpConnection)
		{
			HttpConnection = httpConnection;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Disposable factory method pattern.")]
		internal static HttpCommand NewFromTemplate(HttpConnection httpConnection, IDbCommand templateCommand)
		{
			Argument.NotNull(httpConnection, nameof(httpConnection));
			Argument.NotNull(templateCommand, nameof(templateCommand));
			Argument.NotNull(templateCommand.Parameters, nameof(templateCommand.Parameters));

			HttpCommand newHttpCommand = null;
			try
			{
				newHttpCommand = new HttpCommand(httpConnection)
				{
					CommandText = templateCommand.CommandText,
					CommandTimeout = templateCommand.CommandTimeout,
					CommandType = templateCommand.CommandType
				};

				foreach (var templateParam in templateCommand.Parameters)
				{
					newHttpCommand.Parameters.Add(templateParam);
				}
			}
			catch
			{
				try
				{
					newHttpCommand?.Dispose();
				}
				catch
				{
					// ignore all exceptions from disposing newHttpCommand
				}
				throw;
			}

			return newHttpCommand;
		}

		readonly HttpCommandRunner commandRunner = new HttpCommandRunner();

		public override void Cancel()
		{
			throw new NotSupportedException($"{nameof(Cancel)} operation is not supported on {nameof(HttpCommand)}");
		}

		public override string CommandText { get; set; }
		public override int CommandTimeout { get; set; }
		public override CommandType CommandType { get; set; } = CommandType.Text;

		internal HttpConnection HttpConnection { get; }

		protected override DbParameter CreateDbParameter()
		{
			return new HttpDbParameter();
		}

		protected override DbParameterCollection DbParameterCollection
		{
			get
			{
				return parameters ??= new HttpParameterCollection();
			}
		}

		HttpParameterCollection parameters;

		public override int ExecuteNonQuery()
		{
			return (int)commandRunner.ExecuteNonQuery(this);
		}

		public override object ExecuteScalar()
		{
			return commandRunner.ExecuteScalar(this);
		}

		public Guid HttpTransactionId
		{
			get { return HttpConnection.HttpTransaction?.HttpTransactionId ?? Guid.Empty; }
		}

		protected override DbConnection DbConnection
		{
			get { return HttpConnection; }
			set { throw new ReadOnlyException("DbCommand Connection cannot be set. It's readonly."); }
		}

		protected override DbTransaction DbTransaction
		{
			get { return HttpConnection.HttpTransaction; }
			set
			{
				throw new NotSupportedException($"{nameof(DbTransaction)} cannot be set on {nameof(HttpCommand)}");
			}
		}
		public override bool DesignTimeVisible { get; set; }
		public override UpdateRowSource UpdatedRowSource { get; set; }

		#region CodeContracts

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			return commandRunner.ExecuteReader(this, behavior);
		}

		public override void Prepare()
		{
			throw new NotSupportedException($"{nameof(Prepare)} operation is not supported on {nameof(HttpCommand)}");
		}

		#endregion
	}
}
