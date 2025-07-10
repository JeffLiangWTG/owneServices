using System;
using System.Drawing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class OFontTest : TestCase
	{
		public void TestGetFont()
		{
			AssertNotNull("Font was null", OFont.GetFont());
		}

		public void TestGetFontBold()
		{
			AssertFontEquals("BoldFont", OFont.GetFontBold(), "Tahoma", 8, FontStyle.Bold);
		}

		public void TestGetDefaultFontSize()
		{
			AssertEquals("RichTextBox font should be size 10", 10, OFont.GetDefaultFontSize(OFontTypes.RichText));
			AssertEquals("Normal font should be size 8", 8, OFont.GetDefaultFontSize(OFontTypes.Normal));
			AssertEquals("Header font should be size 12", 12, OFont.GetDefaultFontSize(OFontTypes.Header));
			AssertEquals("SubText font should be size 6", 6, OFont.GetDefaultFontSize(OFontTypes.SubText));
			AssertEquals("Larger font should be size 10", 10, OFont.GetDefaultFontSize(OFontTypes.Larger));
			AssertEquals("Medium font should be size 8", 8, OFont.GetDefaultFontSize(OFontTypes.Medium));
			AssertEquals("Largest font should be size 12", 12, OFont.GetDefaultFontSize(OFontTypes.Largest));
			AssertEquals("Small font should be size 6", 6, OFont.GetDefaultFontSize(OFontTypes.Small));
		}
		public void TestGetRichTextBoxFont()
		{
			AssertEquals("RichTextBox font should be GenericSansSerif", FontFamily.GenericSansSerif.Name, OFont.GetRichTextBoxFont().FontFamily.Name);
			AssertEquals("RichTextBox font should be size 10", 10, (int)OFont.GetRichTextBoxFont().Size);
		}

		public void TestFontCreation()
		{
			AssertCreatesFont(OFontTypes.Normal, "Tahoma", 8, FontStyle.Regular);
			AssertCreatesFont(OFontTypes.Bolded, "Tahoma", 8, FontStyle.Bold);
			AssertCreatesFont(OFontTypes.Header, "Tahoma", 12, FontStyle.Bold);
			AssertCreatesFont(OFontTypes.SubText, "Tahoma", 6, FontStyle.Bold);
			AssertCreatesFont(OFontTypes.RichText, FontFamily.GenericSansSerif.Name, 10, FontStyle.Regular);
		}

		public void TestFontCreationForWeb()
		{
			try
			{
				OFont.ResetFonts();
				AssertCreatesFont(OFontTypes.Normal, "Tahoma", 8, FontStyle.Regular);

				using (Globals.SetIsWebForTest(true))
				{
					OFont.BeforeNewFontCreationForTesting = () => { throw new ArgumentException(); };
					AssertCreatesFont(OFontTypes.Bolded, "Tahoma", 8, FontStyle.Regular);
				}
			}
			finally
			{
				OFont.BeforeNewFontCreationForTesting = null;
				OFont.ResetFonts();
			}
		}

		public void TestFindFontType()
		{
			foreach (var font in new[] { OFontTypes.Normal, OFontTypes.Bolded, OFontTypes.Header, OFontTypes.SubText, OFontTypes.RichText })
			{
				AssertEquals(font.ToString(), font, OFont.FindFontType(OFont.GetFont(font)));
			}
		}

		public void TestFindFontType_Unknown()
		{
			using (var font = new Font(OFont.GetFont().Name, 7f, FontStyle.Bold))
			{
				AssertEquals("Since it's size cannot be perfectly matched we don't want to return anything but Unknown", OFontTypes.Unknown, OFont.FindFontType(font));
			}
		}

		public void TestFindFontType_BySize()
		{
			CombineAssertions("Matching found font type by size", () =>
			{
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 5f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 5.2f);
				AssertMatchFontTypeAndSize(OFontTypes.Small, 5.5f);
				AssertMatchFontTypeAndSize(OFontTypes.Small, 6f);
				AssertMatchFontTypeAndSize(OFontTypes.Small, 6.2f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 6.5f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 6.8f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 7f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 7.2f);
				AssertMatchFontTypeAndSize(OFontTypes.Normal, 7.5f);
				AssertMatchFontTypeAndSize(OFontTypes.Normal, 7.8f);
				AssertMatchFontTypeAndSize(OFontTypes.Normal, 8f);
				AssertMatchFontTypeAndSize(OFontTypes.Normal, 8.2f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 8.5f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 9f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 9.2f);
				AssertMatchFontTypeAndSize(OFontTypes.Larger, 9.5f);
				AssertMatchFontTypeAndSize(OFontTypes.Larger, 9.8f);
				AssertMatchFontTypeAndSize(OFontTypes.Larger, 10f);
				AssertMatchFontTypeAndSize(OFontTypes.Larger, 10.2f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 10.5f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 10.8f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 11f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 11.2f);
				AssertMatchFontTypeAndSize(OFontTypes.Largest, 11.5f);
				AssertMatchFontTypeAndSize(OFontTypes.Largest, 11.8f);
				AssertMatchFontTypeAndSize(OFontTypes.Largest, 12f);
				AssertMatchFontTypeAndSize(OFontTypes.Largest, 12.2f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 12.5f);
				AssertMatchFontTypeAndSize(OFontTypes.Unknown, 13f);
			});
		}

		void AssertMatchFontTypeAndSize(OFontTypes oFontType, float size)
		{
			Font font = new Font(FontFamily.GenericSerif, size);
			AssertEquals($"Size {size} matched to OFont Type {oFontType.ToString()}", oFontType, OFont.FindFontType(font));
		}

		void AssertCreatesFont(OFontTypes type, string name, int size, FontStyle style) =>
			AssertFontEquals(type.ToString(), OFont.GetFont(type), name, size, style);

		void AssertFontEquals(string description, Font font, string name, int size, FontStyle style)
		{
			CombineAssertions("For " + description, () =>
			{
				AssertEquals("Name", name, font.Name);
				AssertEquals("Size", size, font.Size, 0.5);
				AssertEquals("Syle", style, font.Style);
			});
		}

		public void TestResetFont()
		{
			Font font1 = OFont.GetFont();
			OFont.ResetFonts();
			Font font2 = OFont.GetFont();
			AssertTwoFonts(font1, font2, false);

			font1 = OFont.GetFontBold();
			OFont.ResetFonts();
			font2 = OFont.GetFontBold();
			AssertTwoFonts(font1, font2, false);

			font1 = OFont.GetRichTextBoxFont();
			OFont.ResetFonts();
			font2 = OFont.GetRichTextBoxFont();
			AssertTwoFonts(font1, font2, false);
		}

		void AssertTwoFonts(Font font1, Font font2, bool areSame)
		{
			AssertNotNull(font1);
			AssertNotNull(font2);
			AssertEquals(areSame, object.ReferenceEquals(font1, font2));
		}
	}
}
