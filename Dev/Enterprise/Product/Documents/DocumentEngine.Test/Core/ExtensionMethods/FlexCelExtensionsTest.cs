using System.Drawing;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	sealed class FlexCelExtensionsTest : TestCase
	{
		public void TestToFontStyle()
		{
			CombineAssertions(delegate()
			{
				AssertEquals("TFlxFontStyles.None.ToFontStyle()", FontStyle.Regular, TFlxFontStyles.None.ToFontStyle());
				AssertEquals("TFlxFontStyles.Bold.ToFontStyle()", FontStyle.Bold, TFlxFontStyles.Bold.ToFontStyle());
				AssertEquals("TFlxFontStyles.Italic.ToFontStyle()", FontStyle.Italic, TFlxFontStyles.Italic.ToFontStyle());
				AssertEquals("(TFlxFontStyles.Bold | TFlxFontStyles.Italic).ToFontStyle()", FontStyle.Bold | FontStyle.Italic, (TFlxFontStyles.Bold | TFlxFontStyles.Italic).ToFontStyle());
			});
		}
	}
}
