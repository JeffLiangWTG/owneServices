using System.Collections.Generic;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectFetchStrategyForTest : BusinessObjectFetchStrategy
	{
		public BusinessObjectFetchStrategyForTest(BusinessObject bizO)
			: base(bizO)
		{
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			FetchForLoadChildEditableObjectsCoreCount++;
		}

		public int FetchForLoadChildEditableObjectsCoreCount
		{
			get { return FetchDetail.LoadChildEditableObjects; }
			set { FetchDetail.LoadChildEditableObjects = value; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			FetchForLoadCoreCount++;
		}

		public int FetchForLoadCoreCount
		{
			get { return FetchDetail.LoadCount; }
			set { FetchDetail.LoadCount = value; }
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			FetchForDeleteCoreCount++;
		}

		public int FetchForDeleteCoreCount
		{
			get { return FetchDetail.DeleteCount; }
			set { FetchDetail.DeleteCount = value; }
		}

		protected override void FetchForFactorySaveCore()
		{
			base.FetchForFactorySaveCore();
			FetchForFactorySaveCoreCount++;
		}

		public int FetchForFactorySaveCoreCount
		{
			get { return FetchDetail.FactorySaveCount; }
			set { FetchDetail.FactorySaveCount = value; }
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			FetchForValidateCoreCount++;
		}

		public int FetchForValidateCoreCount
		{
			get { return FetchDetail.ValidateCount; }
			set { FetchDetail.ValidateCount = value; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			FetchForViewCoreCount++;
		}

		public int FetchForViewCoreCount
		{
			get { return FetchDetail.ViewCount; }
			set { FetchDetail.ViewCount = value; }
		}

		FetchDetailForTest FetchDetail
		{
			get
			{
				FetchDetailForTest details;
				if (!FetchDetailForTestDictionary.TryGetValue(BusinessObject, out details))
				{
					details = new FetchDetailForTest();
					FetchDetailForTestDictionary.Add(BusinessObject, details);
				}
				return details;
			}
		}

		Dictionary<BusinessObject, FetchDetailForTest> FetchDetailForTestDictionary
		{
			get
			{
				return Factory.GetCachedValue("BusinessObjectFetchStrategyForTest_FetchDetailForTestDictionary", delegate
				{
					return new Dictionary<BusinessObject, FetchDetailForTest>();
				});
			}
		}

		class FetchDetailForTest
		{
			public int LoadCount;
			public int LoadChildEditableObjects;
			public int DeleteCount;
			public int FactorySaveCount;
			public int ValidateCount;
			public int ViewCount;
		}
	}
}
