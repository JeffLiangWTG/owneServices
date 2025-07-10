using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.GLAccountFormat.Testing
{
	public class GLFormatMaskTest : TestCase
	{
		class TestingGLFormatMask : GLFormatMask
		{
			public TestingGLFormatMask(string mask, bool isTarget) : base(mask, isTarget)
			{
			}

			public List<GLNumberBox> BoxesWrapper
			{
				get { return Boxes; }
			}
		}

		public void TestNumberCount()
		{
			TestMask = GetGLFormatMask("XX.XX.X", true);
			AssertEquals(5, TestMask.NumberCount);

			TestMask = GetGLFormatMask("XXXXX", true);
			AssertEquals(5, TestMask.NumberCount);

			TestMask = GetGLFormatMask("XX", true);
			AssertEquals(2, TestMask.NumberCount);

			TestMask = GetGLFormatMask("X.X.X.X.X", true);
			AssertEquals(5, TestMask.NumberCount);
		}

		public void TestIsEmpty()
		{
			TestMask = GetGLFormatMask("XX.XX.X", true);
			Assert(TestMask.IsEmpty);

			TestMask = GetGLFormatMask("XX.XX.X", false);
			Assert(!TestMask.IsEmpty);

			TestMask = GetGLFormatMask("XX.XX.X", true);
			TestMask.BoxesWrapper[0].Caption = "X";
			Assert(!TestMask.IsEmpty);
		}

		public void TestIsFullyEntered()
		{
			TestMask = GetGLFormatMask("XX.XX.X", true);
			Assert(!TestMask.IsFullyEntered);

			TestMask = GetGLFormatMask("XX.XX.X", false);
			Assert(TestMask.IsFullyEntered);

			TestMask = GetGLFormatMask("XX.XX.X", false);
			TestMask.BoxesWrapper[0].Caption = "";
			Assert(!TestMask.IsFullyEntered);
		}

		public void TestFormatMask()
		{
			TestMask = GetGLFormatMask("XX.XX.X", true);
			AssertEquals("00.00.0", TestMask.FormatMask);

			TestMask = GetGLFormatMask("XX.XX.X", false);
			AssertEquals("XX.XX.X", TestMask.FormatMask);
		}

		public void TestRequestRemainingNumbers()
		{
			TestMask = GetGLFormatMask("XX.XX.X", false);
			var aBox = TestMask.BoxesWrapper[0];
			AssertEquals("has just label inside", 1, aBox.Controls.Count);
			aBox.Caption = "";

			TestMask.RequestRemainingNumbers();
			AssertEquals("has label + CalcEdit inside", 2, aBox.Controls.Count);
		}

		public void TestDeselectEverything()
		{
			TestMask = GetGLFormatMask("X.X", false);
			var box1 = TestMask.BoxesWrapper[0];
			var box2 = TestMask.BoxesWrapper[1];

			Assert(!box1.IsHighlighted);
			Assert(!box2.IsHighlighted);

			box1.Highlight();
			box2.Highlight();

			Assert(box1.IsHighlighted);
			Assert(box2.IsHighlighted);

			TestMask.DeselectEverything();
			Assert(!box1.IsHighlighted);
			Assert(!box2.IsHighlighted);
		}

		#region Implementation

		protected override void TearDown()
		{
			DisposeTestMask();
			base.TearDown();
		}

		TestingGLFormatMask GetGLFormatMask(string mask, bool isTarget)
		{
			DisposeTestMask();
			return new TestingGLFormatMask(mask, isTarget);
		}

		void DisposeTestMask()
		{
			if (TestMask != null)
			{
				TestMask.Dispose();
			}
		}

		TestingGLFormatMask TestMask;

		#endregion
	}
}
