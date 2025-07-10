using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(CusSealCollection<CusSeal>))]
	sealed class CusSealCollectionTest : CusSealCollectionAbstractTest<CusSealCollection<CusSeal>, CusSeal>
	{
		protected override int ExpectedMaxRowCount => 4;

		protected override CusSealCollection<CusSeal> GetCollectionToTest()
		{
			var container = Factory.New<BaseCusContainer>();
			return new CusSealCollection<CusSeal>(container, ExpectedMaxRowCount);
		}
	}

	public abstract class CusSealCollectionAbstractTest<T, C> : ActiveBusinessObjectCollectionTestCase<T>
		where T : CusSealCollection<C>
		where C : CusSeal
	{
		public void TestMaxRowCount()
		{
			CombineAssertions(() =>
			{
				for (var i = 0; i < ExpectedMaxRowCount - 1; i++)
				{
					Collection.AddNew();
					Assert($"Currently, collection has {Collection.Count} elements", ((IBindingList)Collection).AllowNew);
				}
				Collection.AddNew();
				Assert($"Currently, collection has {Collection.Count} elements", !((IBindingList)Collection).AllowNew);
			});
		}

		protected abstract int ExpectedMaxRowCount { get; }
	}
}
