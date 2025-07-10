using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.Customs.IT.Business;

public sealed class LocalReferenceNumberGenerator : ILocalReferenceNumberGenerator
{
	public LocalReferenceNumberGenerator(IDbConnected dbConnected)
	{
		this.dbConnected = Argument.NotNull(dbConnected, nameof(dbConnected));
	}

	string ILocalReferenceNumberGenerator.Generate()
	{
		using var transactionManager = dbConnected.Connection.BeginTransactionWithManager();
		var lrnStrategy = new LocalReferenceNumberStrategy(dbConnected);
		var referenceNumber = lrnStrategy.GetMessageReferenceNumber();
		transactionManager.CommitTransaction();
		return referenceNumber;
	}

	readonly IDbConnected dbConnected;
}
