using System.Collections.Generic;
using System.Linq;
using Enterprise.BarcodeParsingEngine;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeFormatHelperTest : BarcodeParsingTestCase
	{
		public void TestDateFormat()
		{
			var formatTypes = BarcodeFormatHelper.GetFormatTypes(false);
			var dateFormats = new List<string>
			{
				GS1DataFormatTypes.Codes.YYMMDD,
				OtherDataFormatTypes.Codes.DDMMYY,
				OtherDataFormatTypes.Codes.DDMMYYYY,
				OtherDataFormatTypes.Codes.MMDDYY,
				OtherDataFormatTypes.Codes.MMDDYYYY
			};

			foreach (var type in formatTypes)
			{
				var code = type.ToString();
				AssertEquals(dateFormats.Contains(code), BarcodeFormatHelper.IsDateFormat(code));
			}

			AssertEquals(false, BarcodeFormatHelper.IsDateFormat("XXX"));
		}

		public void TestLengthOfDateFormat()
		{
			AssertEquals(6, (int)BarcodeFormatHelper.LengthOfDateFormat(GS1DataFormatTypes.Codes.YYMMDD));
			AssertEquals(6, (int)BarcodeFormatHelper.LengthOfDateFormat(OtherDataFormatTypes.Codes.DDMMYY));
			AssertEquals(6, (int)BarcodeFormatHelper.LengthOfDateFormat(OtherDataFormatTypes.Codes.MMDDYY));
			AssertEquals(8, (int)BarcodeFormatHelper.LengthOfDateFormat(OtherDataFormatTypes.Codes.DDMMYYYY));
			AssertEquals(8, (int)BarcodeFormatHelper.LengthOfDateFormat(OtherDataFormatTypes.Codes.MMDDYYYY));
			AssertEquals(0, (int)BarcodeFormatHelper.LengthOfDateFormat("XXX"));
		}

		public void TestGetFormatTypes()
		{
			var gs1Codes = new GS1DataFormatTypes().GetAllCodes();
			var types1 = BarcodeFormatHelper.GetFormatTypes(true);
			AssertEquals(true, types1.GetAllCodes().Intersect(gs1Codes).Count() == gs1Codes.Length);
			AssertEquals(false, types1.ContainsCode(OtherDataFormatTypes.Codes.DDMMYY));
			AssertEquals(false, types1.ContainsCode(OtherDataFormatTypes.Codes.DDMMYYYY));
			AssertEquals(false, types1.ContainsCode(OtherDataFormatTypes.Codes.MMDDYY));
			AssertEquals(false, types1.ContainsCode(OtherDataFormatTypes.Codes.MMDDYYYY));
			AssertEquals(false, types1.ContainsCode("XXX"));

			var types2 = BarcodeFormatHelper.GetFormatTypes(false);
			AssertEquals(true, types2.GetAllCodes().Intersect(gs1Codes).Count() == gs1Codes.Length);
			AssertEquals(true, types2.ContainsCode(GS1DataFormatTypes.Codes.YYMMDD));
			AssertEquals(true, types2.ContainsCode(OtherDataFormatTypes.Codes.DDMMYY));
			AssertEquals(true, types2.ContainsCode(OtherDataFormatTypes.Codes.DDMMYYYY));
			AssertEquals(true, types2.ContainsCode(OtherDataFormatTypes.Codes.MMDDYY));
			AssertEquals(true, types2.ContainsCode(OtherDataFormatTypes.Codes.MMDDYYYY));
			AssertEquals(false, types2.ContainsCode("XXX"));
		}

		public void TestGetLengthTypeBasedOnMinAndMaxLength()
		{
			AssertEquals(LengthTypes.Codes.Any, BarcodeFormatHelper.GetLengthTypeBasedOnMinAndMaxLength(0, 0));
			AssertEquals(LengthTypes.Codes.Range, BarcodeFormatHelper.GetLengthTypeBasedOnMinAndMaxLength(0, 1));
			AssertEquals(LengthTypes.Codes.Range, BarcodeFormatHelper.GetLengthTypeBasedOnMinAndMaxLength(1, 0));
			AssertEquals(LengthTypes.Codes.Fixed, BarcodeFormatHelper.GetLengthTypeBasedOnMinAndMaxLength(1, 1));
		}

		public void TestFormat()
		{
			AssertEquals(FormatType.Undefined, BarcodeFormatHelper.ParseFormat("Undefined"));
			AssertEquals(FormatType.A, BarcodeFormatHelper.ParseFormat("A"));
			AssertEquals(FormatType.AS, BarcodeFormatHelper.ParseFormat("AS"));
			AssertEquals(FormatType.AN, BarcodeFormatHelper.ParseFormat("AN"));
			AssertEquals(FormatType.ANS, BarcodeFormatHelper.ParseFormat("ANS"));
			AssertEquals(FormatType.ANY, BarcodeFormatHelper.ParseFormat("ANY"));
			AssertEquals(FormatType.D, BarcodeFormatHelper.ParseFormat("D"));
			AssertEquals(FormatType.N, BarcodeFormatHelper.ParseFormat("N"));
			AssertEquals(FormatType.D1, BarcodeFormatHelper.ParseFormat("D1"));
			AssertEquals(FormatType.D2, BarcodeFormatHelper.ParseFormat("D2"));
			AssertEquals(FormatType.D3, BarcodeFormatHelper.ParseFormat("D3"));
			AssertEquals(FormatType.D4, BarcodeFormatHelper.ParseFormat("D4"));
			AssertEquals(FormatType.D5, BarcodeFormatHelper.ParseFormat("D5"));
			AssertEquals(FormatType.Undefined, BarcodeFormatHelper.ParseFormat("XXX"));
		}
	}
}
