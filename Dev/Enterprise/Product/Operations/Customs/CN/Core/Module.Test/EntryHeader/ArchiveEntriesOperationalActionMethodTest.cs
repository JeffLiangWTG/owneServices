using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(ArchiveEntriesOperationalActionMethod))]
	class ArchiveEntriesOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<ArchiveEntriesOperationalActionMethod>
	{
		protected override ArchiveEntriesOperationalActionMethod NewMethod() => new ArchiveEntriesOperationalActionMethod();
	}
}
