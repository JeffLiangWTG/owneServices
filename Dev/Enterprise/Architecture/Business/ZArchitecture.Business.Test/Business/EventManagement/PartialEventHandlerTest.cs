using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestsSubclassesOf(typeof(PartialEventHandler))]
	public abstract class PartialEventHandlerTest : TestCaseWithFactory
	{
		public abstract void TestCancelDuplicate();

		public abstract void TestCompletion();
	}
}
