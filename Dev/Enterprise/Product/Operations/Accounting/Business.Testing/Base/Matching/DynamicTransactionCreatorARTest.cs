using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(DynamicTransactionCreatorAR))]
	public class DynamicTransactionCreatorARTest : DynamicTransactionCreatorSingleLedgerTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DynamicTransactionCreatorAR(new OrganizationSubBalanceCollection(Factory),
				ZGuid.Empty, Factory);
		}
	}
}
