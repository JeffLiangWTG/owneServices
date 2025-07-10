using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public static class ValidationHelper
	{
		public static void CheckMaxLength(ZPropertyInfo propertyInfo, int maxLength)
		{
			var value = (ZString)propertyInfo.Value;
			if (value.Length > maxLength)
			{
				var messageError = Res.GetString("c14463ff-9369-41d1-97e2-72c47e92b494", "Length of {0} must not exceed {1} characters.", propertyInfo.HumanReadableName, maxLength);
				propertyInfo.AddMessageError(messageError);
			}
		}

		public static void CheckValidPhoneNumber(string phoneNumber, ZPropertyInfo zPropertyInfo, string orgName)
		{
			if (!phoneNumber.IsNullOrEmpty() && !Regex.IsMatch(phoneNumber, @"^\+(?:[0-9] ?){6,14}[0-9]$"))
			{
				zPropertyInfo.AddMessageError(ResString.GetMultilingualString("D1E2937D-74E4-44CE-BEDF-CB66D05AC584", "Phone Number of {0} is in an invalid format. The phone number is required to start with + and have 6 to 14 characters (digits and spaces) E.g. +14155552671, +1 415 555 2671, +44 7911 123456, +44 7911 123 456, +33 6 12 34 56 78, +33123456789", orgName));
			}
		}

		public static void CheckTransportDocumentType(AsycudaBill asycudaBill)
		{
			var manifestHeader = asycudaBill.Header;
			var specificCircumstanceIndicator = manifestHeader.SpecificCircumstanceIndicator.ToString();

			if (manifestHeader.IsSea
				&& specificCircumstanceIndicator.In(EUICS2SpecificCircumstanceList.Codes.F14, EUICS2SpecificCircumstanceList.Codes.F15, EUICS2SpecificCircumstanceList.Codes.F16, EUICS2SpecificCircumstanceList.Codes.F17))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(asycudaBill.TransportDocumentTypeInfo);
			}
		}

		public static void AddShipperRequiredMessageErrorIfEmpty(ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				var messageError = Res.GetString("B8A56F06-035A-4965-924D-29F30EFEE6CF", "A Shipper is required.");
				propertyInfo.AddMessageError(messageError);
			}
		}

		public static string EmptyPhoneNumberAndEmailErrorMessage => Res.GetString("5ebe0083-5c8b-4f44-9d27-e5f4b525b5e1", "The Organization must have either a Phone or Email recorded against it.");

		internal static ImmutableHashSet<IZType> SpecificCircumstanceListF22F26F50 { get; } = new HashSet<IZType>
		{
			(ZString)EUICS2SpecificCircumstanceList.Codes.F22,
			(ZString)EUICS2SpecificCircumstanceList.Codes.F26,
			(ZString)EUICS2SpecificCircumstanceList.Codes.F50,
		}.ToImmutableHashSet();

		internal static ImmutableHashSet<IZType> SpecificCircumstanceListRequiringTwoItineraryRecords { get; } = new HashSet<IZType>
		{
			(ZString)EUICS2SpecificCircumstanceList.Codes.F14,
			(ZString)EUICS2SpecificCircumstanceList.Codes.F15,
			(ZString)EUICS2SpecificCircumstanceList.Codes.F22,
			(ZString)EUICS2SpecificCircumstanceList.Codes.F26,
			(ZString)EUICS2SpecificCircumstanceList.Codes.F40,
			(ZString)EUICS2SpecificCircumstanceList.Codes.F50,
		}.ToImmutableHashSet();
	}
}
