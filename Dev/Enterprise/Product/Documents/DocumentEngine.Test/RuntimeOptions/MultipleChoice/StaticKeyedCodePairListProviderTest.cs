using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	public abstract class StaticKeyedCodePairListProviderTest<K, L> : TransactionedTestCase where L : StaticKeyedCodePairListProvider<K>
	{
		public void TestListIsCached()
		{
			ReadOnlyCodeDescriptionPairList listForKey1 = ListProvider.GetCodeDescriptionPairList(FirstKey);
			int commandCount = CargoWise.Data.Db.Connection.ExecutedCommandCount;
			listForKey1 = ListProvider.GetCodeDescriptionPairList(FirstKey);
			AssertEquals("There should be no db hit!!", commandCount, CargoWise.Data.Db.Connection.ExecutedCommandCount);
		}

		public void TestListReturnsExpectedValues()
		{
			AssertCodesAndDescriptionsForExpectedValues(ExpectedValuesForFirstKey, FirstKey);
			AssertCodesAndDescriptionsForExpectedValues(ExpectedValuesForSecondKey, SecondKey);
		}

		void AssertCodesAndDescriptionsForExpectedValues(ReadOnlyCodeDescriptionPairList expectedValues, K key)
		{
			foreach (CodeDescriptionPair pair in expectedValues)
			{
				AssertEquals("Actual list should contain " + pair.Code, true, ListProvider.GetCodeDescriptionPairList(key).ContainsCode(pair.Code));
				AssertEquals("Acutal list description for code should match expected", pair.Description, ListProvider.GetCodeDescriptionPairList(key).GetDescriptionFromCode(pair.Code));
			}
		}

		L ListProvider
		{
			get
			{
				return listProvider ?? (listProvider = GetListProvider());
			}
		}
		L listProvider;

		protected abstract K FirstKey { get; }
		protected abstract K SecondKey { get; }

		protected abstract L GetListProvider();

		protected abstract ReadOnlyCodeDescriptionPairList ExpectedValuesForFirstKey { get; }
		protected abstract ReadOnlyCodeDescriptionPairList ExpectedValuesForSecondKey { get; }
	}
}
