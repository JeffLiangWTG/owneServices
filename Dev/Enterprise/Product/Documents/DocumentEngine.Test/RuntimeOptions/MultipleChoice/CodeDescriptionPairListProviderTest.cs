using System.Linq;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	public abstract class CodeDescriptionPairListProviderTest : TransactionedTestCase
	{
		public void TestIsCodeDescriptionPairListProvider()
		{
			Assert(CreateCodeDescriptionPairListProvider() is ICodeDescriptionPairListProvider);
		}

		protected void AssertListEqual(ReadOnlyCodeDescriptionPairList actualList, ICodeDescriptionPairList expectedList)
		{
			AssertEquals(expectedList.Count, actualList.Count);
			for (int i = 0; i < expectedList.Count; i++)
			{
				var expectedItem = (ICodeDescription)expectedList[i];
				AssertEquals(expectedItem.Code, actualList[i].Code);
				AssertEquals(expectedItem.Description, actualList[i].Description);
				Assert("All items should be CodeDescriptionPair", actualList[i] is CodeDescriptionPair);
			}
		}

		public void TestAllElementsAreCodeDescriptionPair()
		{
			var list = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().Cast<ICodeDescription>();
			Assert("All items should be CodeDescriptionPair", list.All(item => item is CodeDescriptionPair));
		}

		public abstract void TestIsReturningCorrectCollection();
		protected abstract ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider();
	}
}
