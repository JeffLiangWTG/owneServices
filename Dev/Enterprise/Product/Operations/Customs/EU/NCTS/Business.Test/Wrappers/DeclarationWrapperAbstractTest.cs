using System;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(DeclarationWrapper))]
	public abstract class DeclarationWrapperAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : DeclarationWrapper
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()), new object[] { null });
		}
	}
}
