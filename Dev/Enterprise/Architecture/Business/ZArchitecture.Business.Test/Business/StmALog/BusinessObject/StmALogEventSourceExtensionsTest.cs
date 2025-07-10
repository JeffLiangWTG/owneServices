using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmALogEventSourceExtensionsTest : TestCase
	{
		public void TestGenerateEventReference()
		{
			var guid = new ZGuid();
			var reference1 = StmALogEventSourceExtensions.GenerateEventReference("test1", guid);
			AssertEquals(string.Concat("test1", "|", guid), reference1);
			var reference2 = StmALogEventSourceExtensions.GenerateEventReference("test2", guid, "subject Line");
			AssertEquals(string.Concat("test2|subject Line", "|", guid), reference2);
		}
	}
}
