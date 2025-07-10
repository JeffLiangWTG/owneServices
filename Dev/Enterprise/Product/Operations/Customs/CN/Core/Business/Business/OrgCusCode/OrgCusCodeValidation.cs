using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs.CN;
using CheckRegNoFormatMatch = System.Func<CargoWise.Types.ZString, CargoWise.Types.ZString>;

namespace Enterprise.Customs.CN.Business
{
	public class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, IOrgCusCodeValidation
	{
		public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
		{
		}

		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();
			var targetInfo = Parent.OK_CustomsRegNoInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			if (!targetInfo.HasErrors() && Parent.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.China && OK_CustomsRegNoPatternsErrors.ContainsKey(Parent.OK_CodeType))
			{
				if (CheckValidFormat(targetInfo))
				{
					CheckValidGBR();
					CheckValidUSC();
				}
			}
		}

		static ZString RegexMatch(ZString regNo, ZString regex, ZString formatError)
		{
			var isMatch = Regex.IsMatch(regNo, regex);
			return isMatch ? ZString.Empty : formatError;
		}

		static readonly ImmutableDictionary<ZString, CheckRegNoFormatMatch> OK_CustomsRegNoPatternsErrors = ImmutableDictionary.CreateRange(new Dictionary<ZString, CheckRegNoFormatMatch>
		{
			{
				OrgCusCode.CodeTypes.CustomsClientCode, regNo => RegexMatch(regNo, "^[0-9]{4}[0-9W][0-9ABC][A-Z0-9-[IO]]{4}$", CNCustomsClientCodeFormatError)
			},
			{
				OrgCusCode.CodeTypes.GovBusinessCode, regNo => RegexMatch(regNo, "^[0-9]{15}$", CNGovBusinessCodeFormatError)
			},
			{
				OrgCusCode.ChinaCodeTypes.USC, CheckUSC
			},
			{
				OrgCusCode.CodeTypes.DepotControlledPremisesID, regNo => RegexMatch(regNo, (NoResString)@"^\d{4}$", CNDepotControlledPremisesIDFormatError)
			},
			{
				OrgCusCode.ChinaCodeTypes.CIQ, regNo => RegexMatch(regNo, @"^[A-Z0-9]{10}$", CNCIQFormatError)
			}
		});

		static ZString CheckUSC(ZString regNo)
		{
			if (!Regex.IsMatch(regNo, "^[0-9A-Z-[IOZSV]]{18}$"))
			{
				return CNUSCFormatError_18CharactersWithOnlyDigitsAndUppercaseCharactersExcludingIOZSV;
			}

			var first = regNo[0].ToString();
			if (first != "1" && first != "5" && first != "9" && first != "Y")
			{
				return CNUSCFormatError_FirstCharacterShouldBe159YOnly;
			}

			var second = regNo[1].ToString();

			if ((first == "1" || first == "5") && second != "1" && second != "2" && second != "3" && second != "9")
			{
				return CNUSCFormatError_SecondCharacterShouldBe1239OnlyWhenFirstCharacterIs1Or5;
			}

			if (first == "9" && second != "1" && second != "2" && second != "3")
			{
				return CNUSCFormatError_SecondCharacterShouldBe123OnlyWhenFirstCharacterIs9;
			}

			if (first == "Y" && second != "1")
			{
				return CNUSCFormatError_SecondCharacterShouldBe1OnlyWhenFirstCharacterIsY;
			}

			var org = regNo.Substring(2, 6);
			if (!org.IsNumbersOnlyOrEmpty)
			{
				return CNUSCFormatError_3rdTo8thCharactersShouldBeDigitsOnly;
			}

			return ZString.Empty;
		}

		bool CheckValidFormat(ZPropertyInfo targetInfo)
		{
			var errorMessage = CheckCustomsRegNoFormat(Parent.OK_CodeType, Parent.OK_CustomsRegNo);
			var checkResult = errorMessage.IsEmpty;
			if (!checkResult)
			{
				targetInfo.AddMessageError(errorMessage);
			}

			return checkResult;
		}

		public static ZString CheckCustomsRegNoFormat(ZString regNoType, ZString regNo)
		{
			ZString result = ZString.Empty;
			if (OK_CustomsRegNoPatternsErrors.TryGetValue(regNoType, out var formatMatch))
			{
				result = formatMatch.Invoke(regNo);
			}

			return result;
		}

		#region Check GBR

		ZString ValidMod11_10CheckDigit(ZString code)
		{
			var p = 10;
			for (var i = 0; i < code.Length - 1; i++)
			{
				var a = int.Parse(code.Substring(i, 1), CultureInfo.CurrentCulture);
				p = ((p % 11) + a) % 10;
				p = (p == 0) ? 20 : p * 2;
			}

			int checkDigit = (11 - (p % 11)) % 10;
			return checkDigit.ToString(CultureInfo.CurrentCulture);
		}

		void CheckValidGBR()
		{
			if (Parent.OK_CodeType == OrgCusCode.CodeTypes.GovBusinessCode)
			{
				var code = Parent.OK_CustomsRegNo;
				var checkDigit = ValidMod11_10CheckDigit(code);
				if (code.Right(1) != checkDigit)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(IncorrectChecksum(checkDigit));
				}
			}
		}

		#endregion

		#region Check USC

		static readonly ImmutableDictionary<string, int> USCMapCharInt = ImmutableDictionary.CreateRange(new Dictionary<string, int>
		{
			{ "A", 10 }, { "B", 11 }, { "C", 12 }, { "D", 13 }, { "E", 14 }, { "F", 15 }, { "G", 16 },
			{ "H", 17 }, { "J", 18 }, { "K", 19 }, { "L", 20 }, { "M", 21 }, { "N", 22 }, { "P", 23 },
			{ "Q", 24 }, { "R", 25 }, { "T", 26 }, { "U", 27 }, { "W", 28 }, { "X", 29 }, { "Y", 30 }
		});

		static readonly ImmutableDictionary<int, string> USCMapIntChar = ImmutableDictionary.CreateRange(USCMapCharInt.ToDictionary(x => x.Value, x => x.Key));

		static readonly ImmutableArray<int> USCPositionWeight = ImmutableArray.Create(1, 3, 9, 27, 19, 26, 16, 17, 20, 29, 25, 13, 8, 24, 10, 30, 28);

		internal static ZString ValidChecksum(ZString code)
		{
			var result = ZString.Empty;
			int sum = 0;
			for (var i = 0; i < (code.Length - 1) && i < USCPositionWeight.Length; i++)
			{
				var currentChar = code.Substring(i, 1);
				var currentValue = 0;
				if (USCMapCharInt.ContainsKey(currentChar))
				{
					currentValue = USCMapCharInt[currentChar];
				}
				else if (!int.TryParse(currentChar, NumberStyles.None, CultureInfo.CurrentCulture, out currentValue))
				{
					currentValue = 0;
				}

				sum += currentValue * USCPositionWeight[i];
			}
			var checkNumber = (31 - (sum % 31)) % 31;
			if (checkNumber >= 0 && checkNumber <= 9)
			{
				result = checkNumber.ToString(CultureInfo.CurrentCulture);
			}
			else if (USCMapIntChar.ContainsKey(checkNumber))
			{
				result = USCMapIntChar[checkNumber];
			}
			return result;
		}

		void CheckValidUSC()
		{
			if (Parent.OK_CodeType == OrgCusCode.ChinaCodeTypes.USC)
			{
				var code = Parent.OK_CustomsRegNo;
				var checksum = ValidChecksum(code);
				if (code.Right(1) != checksum)
				{
					Parent.OK_CustomsRegNoInfo.AddMessageError(IncorrectChecksum(checksum));
				}
			}
		}

		#endregion

		#region Messages

		public static string IncorrectChecksum(ZString lastCharacter)
		{
			return Res.GetString("d191b1c3-db7d-495d-aa67-023b458f8719", @"Last character should be '{0}'", lastCharacter);
		}

		public static string CNCustomsClientCodeFormatError => Res.GetString("a70bdbe6-d383-4c9d-89a1-d7992b3cb4be", @"Customs Client Code should consist of 10 alphanumeric characters except 'I' and 'O', the first four should be digits, the fifth should be a digit or 'W', the sixth should be a digit, 'A', 'B' or 'C'.");

		public static string CNGovBusinessCodeFormatError => Res.GetString("886f7e51-2f2d-4811-bd00-7b212ba3e48e", @"Government Business Number should have 15 digits");

		public static string CNUSCFormatError_18CharactersWithOnlyDigitsAndUppercaseCharactersExcludingIOZSV => Res.GetString("3CBD1AB2-0C5D-4EC6-95D4-A0ABB453996C", @"Unified Social Credit Identifier should have 18 characters with only digits and upper case characters (excluding I/O/Z/S/V).");

		public static string CNUSCFormatError_FirstCharacterShouldBe159YOnly => Res.GetString("5C8144C9-5FEB-4427-A773-D89F9CEFFD17", @"First character of Unified Social Credit Identifier should be '1', '5', '9' or 'Y' only.");

		public static string CNUSCFormatError_SecondCharacterShouldBe1239OnlyWhenFirstCharacterIs1Or5 => Res.GetString("B6303519-F965-4589-B676-9FB6C5C1E3AC", @"Second character of Unified Social Credit Identifier should be '1', '2', '3' or '9' only when first character is '1' or '5'.");

		public static string CNUSCFormatError_SecondCharacterShouldBe123OnlyWhenFirstCharacterIs9 => Res.GetString("7CDF6330-6093-4792-962B-FB2408EA8F67", @"Second character of Unified Social Credit Identifier should be '1', '2' or '3' only when first character is '9'.");

		public static string CNUSCFormatError_SecondCharacterShouldBe1OnlyWhenFirstCharacterIsY => Res.GetString("2B2BB208-166D-4314-B6FB-0860029F845E", @"Second character of Unified Social Credit Identifier should be '1' only when first character is 'Y'.");

		public static string CNUSCFormatError_3rdTo8thCharactersShouldBeDigitsOnly => Res.GetString("8917C2B4-C16A-4911-AE8D-DE1908E32886", @"The 3rd-8th character of Unified Social Credit Identifier should be digits only.");

		public static string CNDepotControlledPremisesIDFormatError => Res.GetString("FD994F5C-DD08-4A0D-B238-C2B2D7F9A392", "Depot Controlled Premises ID should be 4-digits only.");

		public static string CNCIQFormatError => Res.GetString("1732A988-BB05-4178-A0AC-5E04C3E16466", @"China Import-Export Inspection and Quarantine Code should have 10 alphanumeric characters.");

		#endregion
	}
}
