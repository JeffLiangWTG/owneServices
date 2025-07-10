using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CustomsRegNo_USC_CalculateCheckSum()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CheckSum For 91110000600140372J123123123123123123", "J", OrgCusCodeValidation.ValidChecksum("91110000600140372J123123123123123123"));
				AssertEquals("CheckSum For VVVVVVVV", "0", OrgCusCodeValidation.ValidChecksum("VVVVVVVV"));
				AssertEquals("CheckSum For VVVVVVVVVVVVVVVVVVVVVVVVVVVV", "0", OrgCusCodeValidation.ValidChecksum("VVVVVVVVVVVVVVVVVVVVVVVVVVVV"));
				AssertEquals("CheckSum For 9137020076672319X", "0", OrgCusCodeValidation.ValidChecksum("9137020076672319X0"));
				AssertEquals("CheckSum For 91110000600040372", "0", OrgCusCodeValidation.ValidChecksum("911100006000403720"));
				AssertEquals("CheckSum For 91110000600140372", "J", OrgCusCodeValidation.ValidChecksum("911100006001403720"));
				AssertEquals("CheckSum For 00000000000000000", "0", OrgCusCodeValidation.ValidChecksum("000000000000000000"));
				AssertEquals("CheckSum For 00000000000000000", "0", OrgCusCodeValidation.ValidChecksum("000000000000000000"));
				AssertEquals("CheckSum For 10000000000000000", "Y", OrgCusCodeValidation.ValidChecksum("100000000000000000"));
				AssertEquals("CheckSum For 01000000000000000", "W", OrgCusCodeValidation.ValidChecksum("010000000000000000"));
				AssertEquals("CheckSum For 00100000000000000", "N", OrgCusCodeValidation.ValidChecksum("001000000000000000"));
				AssertEquals("CheckSum For 00010000000000000", "4", OrgCusCodeValidation.ValidChecksum("000100000000000000"));
				AssertEquals("CheckSum For 00001000000000000", "C", OrgCusCodeValidation.ValidChecksum("000010000000000000"));
				AssertEquals("CheckSum For 00000100000000000", "5", OrgCusCodeValidation.ValidChecksum("000001000000000000"));
				AssertEquals("CheckSum For 00000010000000000", "F", OrgCusCodeValidation.ValidChecksum("000000100000000000"));
				AssertEquals("CheckSum For 00000001000000000", "E", OrgCusCodeValidation.ValidChecksum("000000010000000000"));
				AssertEquals("CheckSum For 00000000100000000", "B", OrgCusCodeValidation.ValidChecksum("000000001000000000"));
				AssertEquals("CheckSum For 00000000010000000", "2", OrgCusCodeValidation.ValidChecksum("000000000100000000"));
				AssertEquals("CheckSum For 00000000001000000", "6", OrgCusCodeValidation.ValidChecksum("000000000010000000"));
				AssertEquals("CheckSum For 00000000000100000", "J", OrgCusCodeValidation.ValidChecksum("000000000001000000"));
				AssertEquals("CheckSum For 00000000000010000", "P", OrgCusCodeValidation.ValidChecksum("000000000000100000"));
				AssertEquals("CheckSum For 00000000000001000", "7", OrgCusCodeValidation.ValidChecksum("000000000000010000"));
				AssertEquals("CheckSum For 00000000000000100", "M", OrgCusCodeValidation.ValidChecksum("000000000000001000"));
				AssertEquals("CheckSum For 00000000000000010", "1", OrgCusCodeValidation.ValidChecksum("000000000000000100"));
				AssertEquals("CheckSum For 00000000000000001", "3", OrgCusCodeValidation.ValidChecksum("000000000000000010"));
				AssertEquals("CheckSum For 20000000000000000", "X", OrgCusCodeValidation.ValidChecksum("200000000000000000"));
				AssertEquals("CheckSum For 02000000000000000", "R", OrgCusCodeValidation.ValidChecksum("020000000000000000"));
				AssertEquals("CheckSum For 00200000000000000", "D", OrgCusCodeValidation.ValidChecksum("002000000000000000"));
				AssertEquals("CheckSum For 00020000000000000", "8", OrgCusCodeValidation.ValidChecksum("000200000000000000"));
				AssertEquals("CheckSum For 00002000000000000", "Q", OrgCusCodeValidation.ValidChecksum("000020000000000000"));
				AssertEquals("CheckSum For 00000200000000000", "A", OrgCusCodeValidation.ValidChecksum("000002000000000000"));
				AssertEquals("CheckSum For 00000020000000000", "Y", OrgCusCodeValidation.ValidChecksum("000000200000000000"));
				AssertEquals("CheckSum For 00000002000000000", "W", OrgCusCodeValidation.ValidChecksum("000000020000000000"));
				AssertEquals("CheckSum For 00000000200000000", "N", OrgCusCodeValidation.ValidChecksum("000000002000000000"));
				AssertEquals("CheckSum For 00000000020000000", "4", OrgCusCodeValidation.ValidChecksum("000000000200000000"));
				AssertEquals("CheckSum For 00000000002000000", "C", OrgCusCodeValidation.ValidChecksum("000000000020000000"));
				AssertEquals("CheckSum For 00000000000200000", "5", OrgCusCodeValidation.ValidChecksum("000000000002000000"));
				AssertEquals("CheckSum For 00000000000020000", "F", OrgCusCodeValidation.ValidChecksum("000000000000200000"));
				AssertEquals("CheckSum For 00000000000002000", "E", OrgCusCodeValidation.ValidChecksum("000000000000020000"));
				AssertEquals("CheckSum For 00000000000000200", "B", OrgCusCodeValidation.ValidChecksum("000000000000002000"));
				AssertEquals("CheckSum For 00000000000000020", "2", OrgCusCodeValidation.ValidChecksum("000000000000000200"));
				AssertEquals("CheckSum For 00000000000000002", "6", OrgCusCodeValidation.ValidChecksum("000000000000000020"));
				AssertEquals("CheckSum For 30000000000000000", "W", OrgCusCodeValidation.ValidChecksum("300000000000000000"));
				AssertEquals("CheckSum For 03000000000000000", "N", OrgCusCodeValidation.ValidChecksum("030000000000000000"));
				AssertEquals("CheckSum For 00300000000000000", "4", OrgCusCodeValidation.ValidChecksum("003000000000000000"));
				AssertEquals("CheckSum For 00030000000000000", "C", OrgCusCodeValidation.ValidChecksum("000300000000000000"));
				AssertEquals("CheckSum For 00003000000000000", "5", OrgCusCodeValidation.ValidChecksum("000030000000000000"));
				AssertEquals("CheckSum For 00000300000000000", "F", OrgCusCodeValidation.ValidChecksum("000003000000000000"));
				AssertEquals("CheckSum For 00000030000000000", "E", OrgCusCodeValidation.ValidChecksum("000000300000000000"));
				AssertEquals("CheckSum For 00000003000000000", "B", OrgCusCodeValidation.ValidChecksum("000000030000000000"));
				AssertEquals("CheckSum For 00000000300000000", "2", OrgCusCodeValidation.ValidChecksum("000000003000000000"));
				AssertEquals("CheckSum For 00000000030000000", "6", OrgCusCodeValidation.ValidChecksum("000000000300000000"));
				AssertEquals("CheckSum For 00000000003000000", "J", OrgCusCodeValidation.ValidChecksum("000000000030000000"));
				AssertEquals("CheckSum For 00000000000300000", "P", OrgCusCodeValidation.ValidChecksum("000000000003000000"));
				AssertEquals("CheckSum For 00000000000030000", "7", OrgCusCodeValidation.ValidChecksum("000000000000300000"));
				AssertEquals("CheckSum For 00000000000003000", "M", OrgCusCodeValidation.ValidChecksum("000000000000030000"));
				AssertEquals("CheckSum For 00000000000000300", "1", OrgCusCodeValidation.ValidChecksum("000000000000003000"));
				AssertEquals("CheckSum For 00000000000000030", "3", OrgCusCodeValidation.ValidChecksum("000000000000000300"));
				AssertEquals("CheckSum For 00000000000000003", "9", OrgCusCodeValidation.ValidChecksum("000000000000000030"));
				AssertEquals("CheckSum For 40000000000000000", "U", OrgCusCodeValidation.ValidChecksum("400000000000000000"));
				AssertEquals("CheckSum For 04000000000000000", "K", OrgCusCodeValidation.ValidChecksum("040000000000000000"));
				AssertEquals("CheckSum For 00400000000000000", "T", OrgCusCodeValidation.ValidChecksum("004000000000000000"));
				AssertEquals("CheckSum For 00040000000000000", "G", OrgCusCodeValidation.ValidChecksum("000400000000000000"));
				AssertEquals("CheckSum For 00004000000000000", "H", OrgCusCodeValidation.ValidChecksum("000040000000000000"));
				AssertEquals("CheckSum For 00000400000000000", "L", OrgCusCodeValidation.ValidChecksum("000004000000000000"));
				AssertEquals("CheckSum For 00000040000000000", "X", OrgCusCodeValidation.ValidChecksum("000000400000000000"));
				AssertEquals("CheckSum For 00000004000000000", "R", OrgCusCodeValidation.ValidChecksum("000000040000000000"));
				AssertEquals("CheckSum For 00000000400000000", "D", OrgCusCodeValidation.ValidChecksum("000000004000000000"));
				AssertEquals("CheckSum For 00000000040000000", "8", OrgCusCodeValidation.ValidChecksum("000000000400000000"));
				AssertEquals("CheckSum For 00000000004000000", "Q", OrgCusCodeValidation.ValidChecksum("000000000040000000"));
				AssertEquals("CheckSum For 00000000000400000", "A", OrgCusCodeValidation.ValidChecksum("000000000004000000"));
				AssertEquals("CheckSum For 00000000000040000", "Y", OrgCusCodeValidation.ValidChecksum("000000000000400000"));
				AssertEquals("CheckSum For 00000000000004000", "W", OrgCusCodeValidation.ValidChecksum("000000000000040000"));
				AssertEquals("CheckSum For 00000000000000400", "N", OrgCusCodeValidation.ValidChecksum("000000000000004000"));
				AssertEquals("CheckSum For 00000000000000040", "4", OrgCusCodeValidation.ValidChecksum("000000000000000400"));
				AssertEquals("CheckSum For 00000000000000004", "C", OrgCusCodeValidation.ValidChecksum("000000000000000040"));
				AssertEquals("CheckSum For 40000000000000000", "U", OrgCusCodeValidation.ValidChecksum("400000000000000000"));
				AssertEquals("CheckSum For 04000000000000000", "K", OrgCusCodeValidation.ValidChecksum("040000000000000000"));
				AssertEquals("CheckSum For 00400000000000000", "T", OrgCusCodeValidation.ValidChecksum("004000000000000000"));
				AssertEquals("CheckSum For 00040000000000000", "G", OrgCusCodeValidation.ValidChecksum("000400000000000000"));
				AssertEquals("CheckSum For 00004000000000000", "H", OrgCusCodeValidation.ValidChecksum("000040000000000000"));
				AssertEquals("CheckSum For 00000400000000000", "L", OrgCusCodeValidation.ValidChecksum("000004000000000000"));
				AssertEquals("CheckSum For 00000040000000000", "X", OrgCusCodeValidation.ValidChecksum("000000400000000000"));
				AssertEquals("CheckSum For 00000004000000000", "R", OrgCusCodeValidation.ValidChecksum("000000040000000000"));
				AssertEquals("CheckSum For 00000000400000000", "D", OrgCusCodeValidation.ValidChecksum("000000004000000000"));
				AssertEquals("CheckSum For 00000000040000000", "8", OrgCusCodeValidation.ValidChecksum("000000000400000000"));
				AssertEquals("CheckSum For 00000000004000000", "Q", OrgCusCodeValidation.ValidChecksum("000000000040000000"));
				AssertEquals("CheckSum For 00000000000400000", "A", OrgCusCodeValidation.ValidChecksum("000000000004000000"));
				AssertEquals("CheckSum For 00000000000040000", "Y", OrgCusCodeValidation.ValidChecksum("000000000000400000"));
				AssertEquals("CheckSum For 00000000000004000", "W", OrgCusCodeValidation.ValidChecksum("000000000000040000"));
				AssertEquals("CheckSum For 00000000000000400", "N", OrgCusCodeValidation.ValidChecksum("000000000000004000"));
				AssertEquals("CheckSum For 00000000000000040", "4", OrgCusCodeValidation.ValidChecksum("000000000000000400"));
				AssertEquals("CheckSum For 00000000000000004", "C", OrgCusCodeValidation.ValidChecksum("000000000000000040"));
				AssertEquals("CheckSum For 91310115MA1K43074", "B", OrgCusCodeValidation.ValidChecksum("91310115MA1K43074B"));
				AssertEquals("CheckSum For 91310115MA1K43074", "B", OrgCusCodeValidation.ValidChecksum("91310115MA1K430740"));
				AssertEquals("CheckSum For 91320191MA1WFRUX4", "W", OrgCusCodeValidation.ValidChecksum("91320191MA1WFRUX4W"));
				AssertEquals("CheckSum For 91320191MA1WFRUX4", "W", OrgCusCodeValidation.ValidChecksum("91320191MA1WFRUX40"));
				AssertEquals("CheckSum For 91310230MA1K0T3EX", "4", OrgCusCodeValidation.ValidChecksum("91310230MA1K0T3EX4"));
				AssertEquals("CheckSum For 91310230MA1K0T3EX", "4", OrgCusCodeValidation.ValidChecksum("91310230MA1K0T3EX0"));
				AssertEquals("CheckSum For 91310230MA1K0RQN4", "Y", OrgCusCodeValidation.ValidChecksum("91310230MA1K0RQN4Y"));
				AssertEquals("CheckSum For 91310230MA1K0RQN4", "Y", OrgCusCodeValidation.ValidChecksum("91310230MA1K0RQN40"));
				AssertEquals("CheckSum For 91310120MA1HP0DQ6", "C", OrgCusCodeValidation.ValidChecksum("91310120MA1HP0DQ6C"));
				AssertEquals("CheckSum For 91310120MA1HP0DQ6", "C", OrgCusCodeValidation.ValidChecksum("91310120MA1HP0DQ60"));
				AssertEquals("CheckSum For 71211011755771963", "C", OrgCusCodeValidation.ValidChecksum("71211011755771963C"));
				AssertEquals("CheckSum For 71211011755771963", "C", OrgCusCodeValidation.ValidChecksum("712110117557719630"));
				AssertEquals("CheckSum For 92371702MA3HUHEA9", "2", OrgCusCodeValidation.ValidChecksum("92371702MA3HUHEA92"));
				AssertEquals("CheckSum For 92371702MA3HUHEA9", "2", OrgCusCodeValidation.ValidChecksum("92371702MA3HUHEA90"));
				AssertEquals("CheckSum For 92370321MA3N3X587", "0", OrgCusCodeValidation.ValidChecksum("92370321MA3N3X5870"));
				AssertEquals("CheckSum For 92370321MA3N3X587", "0", OrgCusCodeValidation.ValidChecksum("92370321MA3N3X5870"));
			}

			);
		}

		public void TestCheckOK_CustomsRegNo_USC_SpecialValue_NoException()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.USC;
			CombineAssertions(() =>
			{
				cusCode.OK_CustomsRegNo = "000000000000000000";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_FirstCharacterShouldBe159YOnly);
				cusCode.OK_CustomsRegNo = "911100006001403720";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("J"));
				cusCode.OK_CustomsRegNo = "911100006000403721";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("0"));
				cusCode.OK_CustomsRegNo = "911100006000403720";
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				cusCode.OK_CustomsRegNo = "9137020076672319X0";
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				cusCode.OK_CustomsRegNo = "91310115MA1K43074B";
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				cusCode.OK_CustomsRegNo = "91310115MA1K430740";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("B"));
				cusCode.OK_CustomsRegNo = "91320191MA1WFRUX4W";
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				cusCode.OK_CustomsRegNo = "91320191MA1WFRUX40";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("W"));
				cusCode.OK_CustomsRegNo = "91310230MA1K0T3EX4";
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				cusCode.OK_CustomsRegNo = "91310230MA1K0T3EX0";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("4"));
				cusCode.OK_CustomsRegNo = "91310230MA1K0RQN4Y";
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				cusCode.OK_CustomsRegNo = "91310230MA1K0RQN40";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("Y"));
				cusCode.OK_CustomsRegNo = "91310120MA1HP0DQ6C";
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				cusCode.OK_CustomsRegNo = "91310120MA1HP0DQ60";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("C"));
				cusCode.OK_CustomsRegNo = "71211011755771963C";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_FirstCharacterShouldBe159YOnly);
				cusCode.OK_CustomsRegNo = "712110117557719630";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_FirstCharacterShouldBe159YOnly);
				cusCode.OK_CustomsRegNo = "92371702MA3HUHEA92";
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				cusCode.OK_CustomsRegNo = "92371702MA3HUHEA90";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("2"));
				cusCode.OK_CustomsRegNo = "92370321MA3N3X5870";
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
			}

			);
		}

		public void TestCheckOK_CustomsRegNo_USC_FirstCharacterIn159Y()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.USC;
			foreach (var first in new[] { "0", "2", "3", "4", "6", "7", "8", "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "T", "U", "W", "X" })
			{
				cusCode.OK_CustomsRegNo = $"{first}2320412MA1Q46MK4N";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_FirstCharacterShouldBe159YOnly);
			}

			foreach (var first in new[] { "1", "5", "9", "Y" })
			{
				cusCode.OK_CustomsRegNo = $"{first}2320412MA1Q46MK4N";
				AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_FirstCharacterShouldBe159YOnly);
			}
		}

		public void TestCheckOK_CustomsRegNo_USC_SecondCharacterIn1239_FirstCharacterIs1()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.USC;
			foreach (var second in new[] { "0", "4", "5", "6", "7", "8", "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "T", "U", "W", "X", "Y" })
			{
				cusCode.OK_CustomsRegNo = $"1{second}320412MA1Q46MK4N";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_SecondCharacterShouldBe1239OnlyWhenFirstCharacterIs1Or5);
			}

			foreach (var second in new[] { "1", "2", "3", "9" })
			{
				cusCode.OK_CustomsRegNo = $"1{second}320412MA1Q46MK4N";
				AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_SecondCharacterShouldBe1239OnlyWhenFirstCharacterIs1Or5);
			}
		}

		public void TestCheckOK_CustomsRegNo_USC_SecondCharacterIn1239_FirstCharacterIs5()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.USC;
			foreach (var second in new[] { "0", "4", "5", "6", "7", "8", "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "T", "U", "W", "X", "Y" })
			{
				cusCode.OK_CustomsRegNo = $"5{second}320412MA1Q46MK4N";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_SecondCharacterShouldBe1239OnlyWhenFirstCharacterIs1Or5);
			}

			foreach (var second in new[] { "1", "2", "3", "9" })
			{
				cusCode.OK_CustomsRegNo = $"5{second}320412MA1Q46MK4N";
				AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_SecondCharacterShouldBe1239OnlyWhenFirstCharacterIs1Or5);
			}
		}

		public void TestCheckOK_CustomsRegNo_USC_SecondCharacterIn123_FirstCharacterIs9()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.USC;
			foreach (var second in new[] { "0", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "T", "U", "W", "X", "Y" })
			{
				cusCode.OK_CustomsRegNo = $"9{second}320412MA1Q46MK4N";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_SecondCharacterShouldBe123OnlyWhenFirstCharacterIs9);
			}

			foreach (var second in new[] { "1", "2", "3" })
			{
				cusCode.OK_CustomsRegNo = $"9{second}320412MA1Q46MK4N";
				AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_SecondCharacterShouldBe123OnlyWhenFirstCharacterIs9);
			}
		}

		public void TestCheckOK_CustomsRegNo_USC_SecondCharacterIs1_FirstCharacterIsY()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.USC;
			foreach (var second in new[] { "0", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "T", "U", "W", "X", "Y" })
			{
				cusCode.OK_CustomsRegNo = $"Y{second}320412MA1Q46MK4N";
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_SecondCharacterShouldBe1OnlyWhenFirstCharacterIsY);
			}

			cusCode.OK_CustomsRegNo = "Y1320412MA1Q46MK4N";
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_SecondCharacterShouldBe1OnlyWhenFirstCharacterIsY);
		}

		public void TestCheckOK_CustomsRegNo_CCD()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CustomsRegNo = "123456789"; // 9 chars
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "123456789A"; // not all digits
			AssertNoMessageErrorContaining("The last four characters must be uppercase letters or digits", cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "12'45~789`"; // invalid chars
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "1234567890"; // good
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "1234A67890"; // invalid char at the fifth position
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "1234W67890"; // good
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "123456abcd";
			AssertHasMessageErrorContaining("The last four characters must be uppercase letters or digits", cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "123456ABCD";
			AssertNoMessageErrorContaining("The last four characters must be uppercase letters or digits", cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "1234W6a1b2";
			AssertHasMessageErrorContaining("The last four characters must be uppercase letters or digits", cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "1234W6A1B2";
			AssertNoMessageErrorContaining("The last four characters must be uppercase letters or digits", cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "ABCD123456";
			AssertHasMessageErrorContaining("The first four characters should be digits", cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "12345C7890";
			AssertNoMessageErrorContaining("The sixth can be a digit, 'A', 'B' or 'C'", cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
			cusCode.OK_CustomsRegNo = "12345678IO";
			AssertHasMessageErrorContaining("Should not contain 'I' or 'O'", cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCustomsClientCodeFormatError);
		}

		public void TestCheckOK_CustomsRegNo_GBR()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GovBusinessCode;
			cusCode.OK_CustomsRegNo = "12345678901234"; // 14 chars
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNGovBusinessCodeFormatError);
			cusCode.OK_CustomsRegNo = "1234567890123456"; // 16 chars 
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNGovBusinessCodeFormatError);
			cusCode.OK_CustomsRegNo = "1234~678!0123@5"; // invalid chars
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNGovBusinessCodeFormatError);
			cusCode.OK_CustomsRegNo = "12345678901234A"; // not all digits
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNGovBusinessCodeFormatError);
			cusCode.OK_CustomsRegNo = "123456789012348"; // invalid checksum
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("5"));
			cusCode.OK_CustomsRegNo = "123456789012345"; // good
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("5"));
		}

		public void TestCheckOK_CustomsRegNo_USC()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.USC;
			cusCode.OK_CustomsRegNo = "1A123456ABC456DEF01"; // 19 chars
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_18CharactersWithOnlyDigitsAndUppercaseCharactersExcludingIOZSV);
			cusCode.OK_CustomsRegNo = "1A123456ABC456DEF"; // 17 chars
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_18CharactersWithOnlyDigitsAndUppercaseCharactersExcludingIOZSV);
			cusCode.OK_CustomsRegNo = "22320412MA1Q46MK4N"; // invalid char 1
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_FirstCharacterShouldBe159YOnly);
			cusCode.OK_CustomsRegNo = "1Y320412MA1Q46MK4N"; // invalid char 2
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_SecondCharacterShouldBe1239OnlyWhenFirstCharacterIs1Or5);
			cusCode.OK_CustomsRegNo = "19123A56ABC456DEF0"; // invalid char 3-8
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_3rdTo8thCharactersShouldBeDigitsOnly);
			cusCode.OK_CustomsRegNo = "1A123456ABC456DSF0"; // invalid char 9 - 18
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_18CharactersWithOnlyDigitsAndUppercaseCharactersExcludingIOZSV);
			cusCode.OK_CustomsRegNo = "1A123456ABc456dek0"; // lower case char
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_18CharactersWithOnlyDigitsAndUppercaseCharactersExcludingIOZSV);
			cusCode.OK_CustomsRegNo = "$A123456A#D456DE*0"; // invalid chars
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_18CharactersWithOnlyDigitsAndUppercaseCharactersExcludingIOZSV);
			cusCode.OK_CustomsRegNo = "11123456ABC456DEF0"; // invalid checksum
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_18CharactersWithOnlyDigitsAndUppercaseCharactersExcludingIOZSV);
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_FirstCharacterShouldBe159YOnly);
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_SecondCharacterShouldBe1239OnlyWhenFirstCharacterIs1Or5);
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNUSCFormatError_3rdTo8thCharactersShouldBeDigitsOnly);
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("E"));
			cusCode.OK_CustomsRegNo = "92320412MA1Q46MK4J"; // invalid checksum
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("N"));
			cusCode.OK_CustomsRegNo = "92320412MA1Q46MK4N";
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.IncorrectChecksum("N"));
		}

		public void TestCheckOK_CustomsRegNo_CPD()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.DepotControlledPremisesID;
			cusCode.OK_CustomsRegNo = "123A";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNDepotControlledPremisesIDFormatError);
			cusCode.OK_CustomsRegNo = "12345";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNDepotControlledPremisesIDFormatError);
			cusCode.OK_CustomsRegNo = "1234";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNDepotControlledPremisesIDFormatError);
		}

		public void TestCheckOK_CustomsRegNo_CIQ()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.CIQ;
			cusCode.OK_CustomsRegNo = "CIQ0000001";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
			cusCode.OK_CustomsRegNo = "CIQ000000";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCIQFormatError);
			cusCode.OK_CustomsRegNo = "CIQ00000011";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCIQFormatError);
			cusCode.OK_CustomsRegNo = "CIQ000000@";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.CNCIQFormatError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
		}
		OrgCusCode cusCode;
		OrgHeader organisation;
	}
}
