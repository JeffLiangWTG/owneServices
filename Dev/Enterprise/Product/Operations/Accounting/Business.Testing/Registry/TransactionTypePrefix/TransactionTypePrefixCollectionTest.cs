using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(TransactionTypePrefixCollection))]
	public class TransactionTypePrefixCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TransactionTypePrefixCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override TransactionTypePrefixCollection GetCollectionToTest()
		{
			return new TransactionTypePrefixCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TransactionTypePrefix();
		}
	}
}
