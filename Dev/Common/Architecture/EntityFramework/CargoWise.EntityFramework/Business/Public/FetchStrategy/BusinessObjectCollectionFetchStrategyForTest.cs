using System.Collections.Generic;

namespace CargoWise.EntityFramework.Testing
{
	public class BusinessObjectCollectionFetchStrategyForTest : BusinessObjectCollectionFetchStrategy
	{
		public BusinessObjectCollectionFetchStrategyForTest(BusinessObjectCollection collection) : base(collection)
		{
		}

		public new IBusinessObjectCollection Collection
		{
			get { return base.Collection; }
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			FetchForValidateCoreCount++;
		}

		public int FetchForValidateCoreCount
		{
			get
			{
				int result;
				return FetchDetailForTestDictionary.TryGetValue(Collection, out result) ? result : 0;
			}
			set
			{
				if (FetchDetailForTestDictionary.ContainsKey(Collection))
				{
					FetchDetailForTestDictionary[Collection] = value;
				}
				else
				{
					FetchDetailForTestDictionary.Add(Collection, value);
				}
			}
		}

		Dictionary<IBusinessObjectCollection, int> FetchDetailForTestDictionary
		{
			get
			{
				return Collection.Factory.GetCachedValue("BusinessObjectCollectionFetchStrategyForTest_FetchDetailForTestDictionary", delegate
				{
					return new Dictionary<IBusinessObjectCollection, int>();
				});
			}
		}
	}
}
