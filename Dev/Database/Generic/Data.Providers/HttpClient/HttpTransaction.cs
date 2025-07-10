using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.HttpClient
{
	public class HttpTransaction : DbTransaction
	{
		internal HttpTransaction(HttpConnection connection, IsolationLevel isolationLevel)
		{
			Argument.NotNull(connection, nameof(connection));
			Connection = connection;
			IsolationLevel = isolationLevel;

			var request = new SqlProxyRequest(Connection) { IsolationLevel = isolationLevel };
			var result = HttpLoaderFactory.GetClient().BeginTransaction(request, CancellationToken.None);
			HttpTransactionId = result.TransactionId;
		}

		public override void Commit()
		{
			if (HttpTransactionId == Guid.Empty)
			{
				throw new InvalidOperationException("Commit has no corresponding begin transaction");
			}

			HttpLoaderFactory.GetClient().CommitTransaction(HttpTransactionId);
			HttpTransactionId = Guid.Empty;
		}

		public new HttpConnection Connection { get; }

		protected override DbConnection DbConnection => Connection;

		public override IsolationLevel IsolationLevel { get; }

		public override void Rollback()
		{
			if (HttpTransactionId != Guid.Empty)
			{
				HttpLoaderFactory.GetClient().RollbackTransaction(HttpTransactionId);
				HttpTransactionId = Guid.Empty;
			}
		}

		public bool IsPending => HttpTransactionId != Guid.Empty;

		public Guid HttpTransactionId { get; private set; }
	}
}
