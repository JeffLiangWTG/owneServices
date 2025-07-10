using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Netting;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocNettingCurrencyCollection))]
	sealed class DocNettingCurrencyCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocNettingCurrencyCollection>
	{
		protected override DocNettingCurrencyCollection GetCollectionToTest()
		{
			return DocNettingCurrencyCollection.New(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocNettingCurrency.New(new NettingCurrency(Factory), Factory);
		}
	}
}
