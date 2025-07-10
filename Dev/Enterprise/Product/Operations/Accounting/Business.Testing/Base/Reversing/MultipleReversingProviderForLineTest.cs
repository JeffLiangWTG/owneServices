using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.WIPAccrual;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(MultipleReversingProviderForLine))]
	public class MultipleReversingProviderForLineTest : MultipleReversingProviderBaseTest
	{
		protected override BusinessObject GetCurrentBusinessObjectForIEnumeratorTesting() => TestObjectForLine.Current;

		protected override BusinessObject[] GetBusinessObjectsForIEnumeratorTesting()
		{
			return new BusinessObject[] { Factory.New<WIP>(), Factory.New<Accrual>() };
		}

		MultipleReversingProviderForLine TestObjectForLine => (MultipleReversingProviderForLine)TestObject;

		protected override MultipleReversingProviderBase GetNewTestObject()
		{
			return new MultipleReversingProviderForLine();
		}
	}
}
