using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(IEntryLineValidationDecider))]
	public abstract class EntryLineValidationDeciderTest<T> : TestCaseWithFactory
		where T : class, IEntryLineValidationDecider, new()
	{
	}
}
