using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(IEntryHeaderValidationDecider))]
	public abstract class EntryHeaderValidationDeciderTest<T> : TestCaseWithFactory
		where T : class, IEntryHeaderValidationDecider, new()
	{
	}
}
