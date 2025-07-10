using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Netting;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocNettingTransactionCollection))]
	sealed class DocNettingCalculationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocNettingTransactionCollection>
	{
		protected override DocNettingTransactionCollection GetCollectionToTest()
		{
			return DocNettingTransactionCollection.New(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocNettingTransaction.New(new NettingTransaction(new ParticipantStatement(Factory, ZGuid.NewZGuid())), Factory);
		}
	}
}
