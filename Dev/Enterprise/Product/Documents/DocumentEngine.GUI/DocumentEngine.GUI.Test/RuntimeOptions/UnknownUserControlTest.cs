using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(UnknownUserControl))]
	sealed class UnknownUserControlTest : RuntimeOptionUserControlBaseTest<UnknownUserControl>
	{
		public override void TestChangeLabelSizeForAlignment()
		{
			//Unnecessary since methods are empty
			Assert(true);
		}

		public override void TestDesiredCaptionWidth()
		{
			//Unnecessary since methods are empty
			Assert(true);
		}
	}
}
