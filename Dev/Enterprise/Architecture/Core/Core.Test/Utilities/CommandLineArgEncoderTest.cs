using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class CommandLineArgEncoderTest : TestCase
	{
		public void TestEnquoteArgumentIfNeeded()
		{
			AssertEquals("a", CommandLineArgEncoder.EnquoteArgumentIfNeeded("a"));
			AssertEquals("\"a b\"", CommandLineArgEncoder.EnquoteArgumentIfNeeded("a b"));
			AssertEquals("e:\\path\\", CommandLineArgEncoder.EnquoteArgumentIfNeeded("e:\\path\\")); // Non-real path
			AssertEquals("\\\\\\\"", CommandLineArgEncoder.EnquoteArgumentIfNeeded("\\\""));
			AssertEquals("\"e:\\te mp\\\\\"", CommandLineArgEncoder.EnquoteArgumentIfNeeded("e:\\te mp\\")); // Non-real path
		}
	}
}
