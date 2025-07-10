using System.Data.Common;
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.SqlProxy.Interface;

public interface ISqlProxy
{
	ExecuteScalarResult ExecuteScalar(SqlProxyRequest request, CancellationToken cancellationToken);

	DbDataReader ExecuteReader(SqlProxyRequest request, CancellationToken cancellationToken);

	ExecuteNonQueryResult ExecuteNonQuery(SqlProxyRequest request, CancellationToken cancellationToken);

	ExecuteBulkCopyResult BulkCopy(SqlProxyBulkCopyRequest request, CancellationToken cancellationToken);

	BeginTransactionResult BeginTransaction(SqlProxyRequest request, CancellationToken cancellationToken);

	VoidResult RollbackTransaction(Guid transactionId, CancellationToken cancellationToken);

	VoidResult CommitTransaction(Guid transactionId, CancellationToken cancellationToken);

	Task<ExecuteScalarResult> ExecuteScalarAsync(SqlProxyRequest request, CancellationToken cancellationToken);

	Task<DbDataReader> ExecuteReaderAsync(SqlProxyRequest request, CancellationToken cancellationToken);

	Task<ExecuteNonQueryResult> ExecuteNonQueryAsync(SqlProxyRequest request, CancellationToken cancellationToken);

	Task<ExecuteBulkCopyResult> BulkCopyAsync(SqlProxyBulkCopyRequest request, CancellationToken cancellationToken);

	Task<BeginTransactionResult> BeginTransactionAsync(SqlProxyRequest request, CancellationToken cancellationToken);

	Task<VoidResult> RollbackTransactionAsync(Guid transactionId, CancellationToken cancellationToken);

	Task<VoidResult> CommitTransactionAsync(Guid transactionId, CancellationToken cancellationToken);
}
