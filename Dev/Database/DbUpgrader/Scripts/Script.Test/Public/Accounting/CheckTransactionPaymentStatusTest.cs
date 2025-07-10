using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(CheckTransactionPaymentStatus))]
	class CheckTransactionPaymentStatusTest : DbCreateScriptTest
	{
		// UTs are in Accounting project test class TransactionPaymentDataAccessTest.
	}
}
