using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CusGuaranteeLineTransaction))]
	public class CusGuaranteeLineTransactionTest : SharedCusPermitLineTransactionTest<CusGuaranteeLineTransaction>
	{
		protected override string ShortName => "Guarantee Transaction";

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override CusGuaranteeLineTransaction GetNewLineTransaction(BusinessObjectFactory factory)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var lineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			lineTransaction.FillWithValidTestData();
			return lineTransaction;
		}
	}
}
