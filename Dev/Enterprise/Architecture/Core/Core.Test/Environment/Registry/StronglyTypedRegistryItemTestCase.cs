using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public abstract class StronglyTypedRegistryItemTestCase<T> : StronglyTypedRegistryItemTestCase<T, T>
	{
	}

	[TestsSubclassesOf(typeof(StronglyTypedRegistryItem<>))]
	public abstract class StronglyTypedRegistryItemTestCase<TGet, TSet> : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public virtual void TestCasting()
		{
			FallbackLevel fallback = Fallback;

			Guid companyPK = fallback.CompanyPK(true);
			Guid branchPK = fallback.BranchPK;
			Guid departmentPK = fallback.DepartmentPK;

			TSet validValue = ValidValue;
			AssertNotNull("Please override ValidValue and return a non-null value for testing.", validValue);
			SetGetValue(companyPK, branchPK, departmentPK, validValue);
		}

		protected virtual FallbackLevel Fallback
		{
			get { return new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		protected virtual TSet ValidValue
		{
			get { return (TSet)(object)Item.DefaultValue; }
		}

		void SetGetValue(Guid companyPK, Guid branchPK, Guid departmentPK, TSet value)
		{
			TGet dummyValue;
			Item.SetValue(companyPK, branchPK, departmentPK, value);
			dummyValue = Item.DefaultValue;
			dummyValue = Item.GetValueWithoutFallback(companyPK, branchPK, departmentPK);
			dummyValue = Item.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}

		protected StronglyTypedRegistryItem<TGet, TSet> Item
		{
			get
			{
				if (item == null)
				{
					item = GetNewRegistryItem();
				}
				return item;
			}
		}

		StronglyTypedRegistryItem<TGet, TSet> item;
		protected abstract StronglyTypedRegistryItem<TGet, TSet> GetNewRegistryItem();
	}
}
