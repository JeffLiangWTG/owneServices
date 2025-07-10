using System;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business
{
	public static class BarcodeFormatHelper
	{
		const short TwoDigitYearDateLength = 6;
		const short FourDigitYearDateLength = 8;

		public static bool IsDateFormat(ZString format)
		{
			return
				format.EqualsIgnoringCase(GS1DataFormatTypes.Codes.YYMMDD) ||
				format.EqualsIgnoringCase(OtherDataFormatTypes.Codes.DDMMYY) ||
				format.EqualsIgnoringCase(OtherDataFormatTypes.Codes.DDMMYYYY) ||
				format.EqualsIgnoringCase(OtherDataFormatTypes.Codes.MMDDYY) ||
				format.EqualsIgnoringCase(OtherDataFormatTypes.Codes.MMDDYYYY);
		}

		public static ZShort LengthOfDateFormat(ZString format)
		{
			switch (format)
			{
				case GS1DataFormatTypes.Codes.YYMMDD:
				case OtherDataFormatTypes.Codes.DDMMYY:
				case OtherDataFormatTypes.Codes.MMDDYY:
					return TwoDigitYearDateLength;

				case OtherDataFormatTypes.Codes.DDMMYYYY:
				case OtherDataFormatTypes.Codes.MMDDYYYY:
					return FourDigitYearDateLength;

				default:
					return 0;
			}
		}

		public static ReadOnlyCodeDescriptionPairList GetFormatTypes(bool isGS1)
		{
			ReadOnlyCodeDescriptionPairList result;

			if (isGS1)
			{
				// GS1 barcodes only format dates as YYMMDD
				result = new GS1DataFormatTypes();
			}
			else
			{
				var formatTypes = new CodeDescriptionPairList();
				formatTypes.AddRange(new GS1DataFormatTypes());

				// remove GS1 Date format so that we can add at the end later so dates are in right order.
				formatTypes.RemoveCode(GS1DataFormatTypes.Codes.YYMMDD);
				formatTypes.AddRange(new OtherDataFormatTypes());
				formatTypes.AddPair(GS1DataFormatTypes.Codes.YYMMDD, GS1DataFormatTypes.Descriptions.YYMMDD);

				result = formatTypes;
			}

			return result;
		}

		public static string GetLengthTypeBasedOnMinAndMaxLength(ZShort minLength, ZShort maxLength) => minLength == maxLength
			? maxLength == 0
				? LengthTypes.Codes.Any
				: LengthTypes.Codes.Fixed
			: LengthTypes.Codes.Range;

		public static FormatType ParseFormat(string format) => Enum.TryParse(format, out FormatType result) ? result : FormatType.Undefined;
	}
}

