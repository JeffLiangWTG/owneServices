using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.AFR.Business
{
	public static class ValidationUtils
	{
		public static ZDateTime GetCurrentJPDate
		{
			get { return ZDateTime.UtcNow.AddHours(9).Date; }
		}

		internal static bool IsValidBillPrefix(this ZString originalValue)
		{
			var trimmedValue = originalValue.TrimEnd('-');
			return trimmedValue.Length <= 4
				&& trimmedValue.Length >= 3
				&& trimmedValue.KeepAlphanumericCharacters() == trimmedValue;
		}

		internal static bool ContainsInvalidNACCSCharacters(this ZString originalValue)
		{
			return originalValue.ExcludeChars(ValidationConstants.Constants.ValidNACCSCharacters).Length != 0;
		}

		internal static void AddErrorIfContainsInvalidNACCSCharacter(this ZPropertyInfo targetInfo, ZPropertyInfo sourceInfo, ZString sourceValue)
		{
			if (sourceValue.ContainsInvalidNACCSCharacters())
			{
				targetInfo.AddError(ValidationConstants.Shared.InvalidNACCSCharMessageForSending(sourceInfo.HumanReadableName));
			}
		}

		internal static void AddMessageErrorIfContainsInvalidNACCSCharacter(this ZPropertyInfo targetInfo, ZString sourceValue)
		{
			targetInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(targetInfo, sourceValue);
		}

		internal static void AddMessageErrorIfContainsInvalidNACCSCharacter(this ZPropertyInfo targetInfo, ZPropertyInfo sourceInfo, ZString sourceValue)
		{
			if (sourceValue.ContainsInvalidNACCSCharacters())
			{
				targetInfo.AddMessageError(ValidationConstants.Shared.InvalidNACCSChar(sourceInfo.HumanReadableName));
			}
		}

		internal static bool IsInappropriateGoodsDescription(string goodsDescription)
		{
			var sourceValue = goodsDescription.Trim().ToUpper();
			return ListOfVagueDescriptionOfGoods.Contains(sourceValue) ||
				ListOfAbbreviationsForUnknown.Contains(sourceValue) ||
				ListOfAsPerAttached.Contains(sourceValue) ||
				SingleRepeatingCharacter.IsMatch(sourceValue);
		}

		internal static bool IsInappropriateVesselCode(string vesselCode)
		{
			var sourceValue = vesselCode.Trim().ToUpper();
			return ListOfAbbreviationsForUnknown.Contains(sourceValue) ||
				SingleRepeatingCharacter.IsMatch(sourceValue);
		}

		internal static bool IsNonUniversalHSCode(string hsCode)
		{
			var sourceValue = hsCode.Trim();
			return sourceValue.StartsWith(ValidationConstants.Constants.NonUniversalHSCode98) ||
				sourceValue.StartsWith(ValidationConstants.Constants.NonUniversalHSCode99);
		}

		#region Inappropriate Samples

		static string[] ListOfVagueDescriptionOfGoods
		{
			get
			{
				return listOfVagueDescriptionOfGoods ?? (listOfVagueDescriptionOfGoods = new string[]
						{
							"APPAREL",
							"APPLIANCES",
							"AUTO PARTS",
							"CAPS",
							"CHEMICALS HAZARDOUS",
							"CHEMICALS NON-HAZARDOUS",
							"ELECTRONIC GOODS",
							"ELECTRONICS",
							"EQUIPMENT",
							"FAK",
							"FAK(FREIGHT OF ALL KINDS)",
							"FLOORLING",
							"FOODSTUFFS",
							"FREIGHT OF ALL KINDS",
							"GENERAL CARGO",
							"IRON",
							"LADIES APPAREL",
							"LEATHER ARTICLES",
							"MACHINERY",
							"MACHINES",
							"MENS APPAREL",
							"NO DESCRIPTION",
							"PARTS",
							"PIPES",
							"PLASTIC GOODS",
							"POLYURETHANE",
							"RODS",
							"RUBBER ARTICLES",
							"SAID TO CONTAIN",
							"STC",
							"STC(SAID TO CONTAIN)",
							"SCRAP",
							"STEEL",
							"TILES",
							"TOOLS",
							"WEARING APPAREL",
							"WIRES",
							"HOUSE HOLD GOODS"
						});
			}
		}
		[ThreadStatic]
		static string[] listOfVagueDescriptionOfGoods;

		static string[] ListOfAbbreviationsForUnknown
		{
			get
			{
				return listOfAbbreviationsForUnknown ?? (listOfAbbreviationsForUnknown = new string[] {
					"NA",
					"N/A",
					"NM",
					"N/M",
					"UNKNOWN"
				});
			}
		}
		[ThreadStatic]
		static string[] listOfAbbreviationsForUnknown;

		static string[] ListOfAsPerAttached
		{
			get
			{
				return listOfAsPerAttached ?? (listOfAsPerAttached = new string[] {
					"AS PER ATTACHED",
					"AS PER ATTACHED SHEET",
					"AS PER ATTACHED DOCUMENT",
					"AS PER ATTACHMENT",
					"AS ATTACHED"
				});
			}
		}
		[ThreadStatic]
		static string[] listOfAsPerAttached;

		static Regex SingleRepeatingCharacter
		{
			get { return singleRepeatingCharacter ?? (singleRepeatingCharacter = new Regex(@"^(.)\1+$", RegexOptions.Compiled)); }
		}
		[ThreadStatic]
		static Regex singleRepeatingCharacter;

		#endregion
	}
}
