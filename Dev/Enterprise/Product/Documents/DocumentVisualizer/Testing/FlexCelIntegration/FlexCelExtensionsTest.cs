using System.Drawing;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration.Testing
{
	sealed class FlexCelExtensionsTest : TestCase
	{
		#region TestRound

		public void TestRound()
		{
			AssertEquals("Round", 2.1d, 2.12345d.Round());
			AssertEquals("Round", 9d, 9d.Round());
			AssertEquals("Round", 7.6d, 7.5541312.Round());
		}

		#endregion

		#region TestFontStyle

		public void TestFontStyle_ToStyle()
		{
			AssertEquals("Bold", FontStyle.Bold, TFlxFontStyles.Bold.ToFontStyle());
			AssertEquals("Italic", FontStyle.Italic, TFlxFontStyles.Italic.ToFontStyle());
			AssertEquals("StrikeOut", FontStyle.Strikeout, TFlxFontStyles.StrikeOut.ToFontStyle());
			AssertEquals("Subscript", FontStyle.Regular, TFlxFontStyles.Subscript.ToFontStyle());
			AssertEquals("Shadow", FontStyle.Regular, TFlxFontStyles.Shadow.ToFontStyle());
			AssertEquals("Superscript", FontStyle.Regular, TFlxFontStyles.Superscript.ToFontStyle());
		}

		public void TestFontStyle_ToFlxStyle()
		{
			AssertEquals("Bold", TFlxFontStyles.Bold, FontStyle.Bold.ToFlxFontStyle());
			AssertEquals("Italic", TFlxFontStyles.Italic, FontStyle.Italic.ToFlxFontStyle());
			AssertEquals("Strikeout", TFlxFontStyles.StrikeOut, FontStyle.Strikeout.ToFlxFontStyle());
			AssertEquals("Regular", TFlxFontStyles.None, FontStyle.Regular.ToFlxFontStyle());
		}

		#endregion

		#region TestAlignment

		public void TestAlignment_ToAlignment()
		{
			AssertEquals("Center", Alignment.Center, THFlxAlignment.center.ToAlignment());
			AssertEquals("RightColumn", Alignment.Right, THFlxAlignment.right.ToAlignment());
			AssertEquals("LeftColumn", Alignment.Left, THFlxAlignment.left.ToAlignment());
			AssertEquals("Justify", Alignment.Justify, THFlxAlignment.justify.ToAlignment());

			AssertEquals("Center", Alignment.Center, TVFlxAlignment.center.ToAlignment());
			AssertEquals("TopRow", Alignment.Top, TVFlxAlignment.top.ToAlignment());
			AssertEquals("BottomRow", Alignment.Bottom, TVFlxAlignment.bottom.ToAlignment());
		}

		public void TestAlignment_ToFlxAlignment()
		{
			AssertEquals("Center", THFlxAlignment.center, Alignment.Center.ToHFlxAlignment());
			AssertEquals("RightColumn", THFlxAlignment.right, Alignment.Right.ToHFlxAlignment());
			AssertEquals("LeftColumn", THFlxAlignment.left, Alignment.Left.ToHFlxAlignment());
			AssertEquals("Justify", THFlxAlignment.justify, Alignment.Justify.ToHFlxAlignment());

			AssertEquals("Center", TVFlxAlignment.center, Alignment.Center.ToVFlxAlignment());
			AssertEquals("TopRow", TVFlxAlignment.top, Alignment.Top.ToVFlxAlignment());
			AssertEquals("BottomRow", TVFlxAlignment.bottom, Alignment.Bottom.ToVFlxAlignment());
		}

		#endregion

		#region TestPattern

		public void TestPattern_ToPattern()
		{
			AssertEquals("Solid", PatternStyle.Solid, TFlxPatternStyle.Solid.ToPattern());
			AssertEquals("None", PatternStyle.None, TFlxPatternStyle.None.ToPattern());
			AssertEquals("Unsupported (should default to None)", PatternStyle.None, TFlxPatternStyle.Automatic.ToPattern());
		}

		public void TestPattern_ToTFlxPattern()
		{
			AssertEquals("Solid", TFlxPatternStyle.Solid, PatternStyle.Solid.ToTFlxPattern());
			AssertEquals("None", TFlxPatternStyle.None, PatternStyle.None.ToTFlxPattern());
		}

		#endregion
	}
}