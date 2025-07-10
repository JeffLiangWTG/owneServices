using System.Drawing;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class FontUtilsTest : TestCase
	{
		[RequiresSoftware(RequiredSoftware.OfficeFonts)]
		public void TestIsStyleAvailable()
		{
			var courrier = new FontFamily("Courier New");
			Assert(FontUtils.IsStyleAvailable(courrier, FontStyle.Regular));
			Assert(FontUtils.IsStyleAvailable(courrier, FontStyle.Bold));
			Assert(FontUtils.IsStyleAvailable(courrier, FontStyle.Italic));
			Assert(FontUtils.IsStyleAvailable(courrier, FontStyle.Bold | FontStyle.Italic));
			Assert(!FontUtils.IsStyleAvailable(courrier, FontStyle.Underline));
			Assert(!FontUtils.IsStyleAvailable(courrier, FontStyle.Strikeout));
			Assert(!FontUtils.IsStyleAvailable(courrier, FontStyle.Bold | FontStyle.Underline));
			Assert(!FontUtils.IsStyleAvailable(courrier, FontStyle.Strikeout | FontStyle.Italic));
			var brushScriptNT = new FontFamily("Brush Script MT");
			Assert(FontUtils.IsStyleAvailable(brushScriptNT, FontStyle.Italic));
			Assert(!FontUtils.IsStyleAvailable(brushScriptNT, FontStyle.Regular));
			Assert(!FontUtils.IsStyleAvailable(brushScriptNT, FontStyle.Bold));
			Assert(!FontUtils.IsStyleAvailable(brushScriptNT, FontStyle.Bold | FontStyle.Italic));
			Assert(!FontUtils.IsStyleAvailable(brushScriptNT, FontStyle.Underline));
			Assert(!FontUtils.IsStyleAvailable(brushScriptNT, FontStyle.Strikeout));
			Assert(!FontUtils.IsStyleAvailable(brushScriptNT, FontStyle.Bold | FontStyle.Underline));
			Assert(!FontUtils.IsStyleAvailable(brushScriptNT, FontStyle.Strikeout | FontStyle.Italic));
		}
	}
}
