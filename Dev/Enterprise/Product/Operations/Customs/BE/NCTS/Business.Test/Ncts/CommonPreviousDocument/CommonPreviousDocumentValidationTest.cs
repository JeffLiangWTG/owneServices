using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class CommonPreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_ReferenceNumber()
	{
		previousDocument.CSI_Code = Constants.PreviousDocumentTypes.CargoManifest;

		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumber = "X";
			AssertHasMessageError("1. Message error when first character is not 1,2 or 3", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. First position of the reference number must be 1(=Antwerpen), 2(=Zeebrugge) or 3(=Gent) when type of previous document is N785. Please correct the reference number.");
			AssertHasMessageError("2. Message error when 8th character is not L", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. Position 2 to 7 must contain the stay number and position 8 must be L when type of previous document is N785. Please check stay number and position 8 of the reference number.");
			AssertHasMessageError("3. Message error when 16th character is not *", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. Position 16 must be a * when the type is N785. Please correct the reference number.");
			AssertHasMessageError("4. Message error when the last 4 characters are not numeric", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. Position 17, 18, 19 and 20 must each contain a numeric value.");

			previousDocument.CSI_ReferenceNumber = "1234567890ABCDEFGHIJXX";
			AssertNoMessageError("5. No message error when first character is 1,2 or 3", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. First position of the reference number must be 1(=Antwerpen), 2(=Zeebrugge) or 3(=Gent) when type of previous document is N785. Please correct the reference number.");
			previousDocument.CSI_ReferenceNumber = "2234567890ABCDEFGHIJ";
			AssertNoMessageError("6. No message error when first character is 1,2 or 3", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. First position of the reference number must be 1(=Antwerpen), 2(=Zeebrugge) or 3(=Gent) when type of previous document is N785. Please correct the reference number.");
			previousDocument.CSI_ReferenceNumber = "3234567890ABCDEFGHIJ";
			AssertNoMessageError("7. No message error when first character is 1,2 or 3", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. First position of the reference number must be 1(=Antwerpen), 2(=Zeebrugge) or 3(=Gent) when type of previous document is N785. Please correct the reference number.");
			previousDocument.CSI_ReferenceNumber = "4234567890ABCDEFGHIJ";
			AssertHasMessageError("8. Message error when first character is not 1,2 or 3", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. First position of the reference number must be 1(=Antwerpen), 2(=Zeebrugge) or 3(=Gent) when type of previous document is N785. Please correct the reference number.");

			AssertHasMessageError("9. Message error when 8th character is not L", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. Position 2 to 7 must contain the stay number and position 8 must be L when type of previous document is N785. Please check stay number and position 8 of the reference number.");
			previousDocument.CSI_ReferenceNumber = "2234567L90ABCDEFGHIJ";
			AssertNoMessageError("10. No message error when 8th character is L", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. Position 2 to 7 must contain the stay number and position 8 must be L when type of previous document is N785. Please check stay number and position 8 of the reference number.");

			AssertHasMessageError("11. Message error when 16th character is not *", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. Position 16 must be a * when the type is N785. Please correct the reference number.");
			previousDocument.CSI_ReferenceNumber = "2234567L90ABCDE*GHIJ";
			AssertNoMessageError("12. No message error when 16th character is *", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. Position 16 must be a * when the type is N785. Please correct the reference number.");

			AssertHasMessageError("13. Message error when the last 4 characters are not numeric", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. Position 17, 18, 19 and 20 must each contain a numeric value.");
			previousDocument.CSI_ReferenceNumber = "2234567L90ABCDE*1234";
			AssertNoMessageError("14. No message error when the last 4 characters are numeric", previousDocument.CSI_ReferenceNumberInfo, "The format of ‘Reference number’ must be pssssssLnnnnnnn*aaaa. Position 17, 18, 19 and 20 must each contain a numeric value.");

			previousDocument.CSI_Code = "XXX";
			previousDocument.CSI_ReferenceNumber = "1234567890ABCDEFGHI";
			AssertNoMessageErrors("15. No message error when the previous document is not a cargo manifest", previousDocument.CSI_ReferenceNumberInfo);
		});
	}

	public void TestCheckCSI_ReferenceNumber2()
	{
		previousDocument.CSI_Code = Constants.PreviousDocumentTypes.CargoManifest;

		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumber2 = "*";
			AssertHasMessageError("1. Message error when number of asterixes <> 2", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The separator * can only be used twice/ Please fill in an agent code (maximum 6 positions) followed by  * AND followed by the item number (exactly 4 positions) AND followed by * AND followed by a B/L number (maximum 14 positions)  when type of previous document is N785. Please correct ’Complement of Information’.");
			previousDocument.CSI_ReferenceNumber2 = "***";
			AssertHasMessageError("2. Message error when number of asterixes <> 2", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The separator * can only be used twice/ Please fill in an agent code (maximum 6 positions) followed by  * AND followed by the item number (exactly 4 positions) AND followed by * AND followed by a B/L number (maximum 14 positions)  when type of previous document is N785. Please correct ’Complement of Information’.");
			previousDocument.CSI_ReferenceNumber2 = "**";
			AssertNoMessageError("3. No Message error when number of asterixes = 2", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The separator * can only be used twice/ Please fill in an agent code (maximum 6 positions) followed by  * AND followed by the item number (exactly 4 positions) AND followed by * AND followed by a B/L number (maximum 14 positions)  when type of previous document is N785. Please correct ’Complement of Information’.");

			AssertHasMessageError("4. Message error when agent code is not filled", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The agent code (aaaaaa) is not filled. Please correct ’Complement of Information’.");
			previousDocument.CSI_ReferenceNumber2 = "AGENT**";
			AssertNoMessageError("5. No Message error when agent code is filled", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The agent code (aaaaaa) is not filled. Please correct ’Complement of Information’.");

			AssertNoMessageError("6. No Message error when agent code is 6 characters", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The agent code (aaaaaa) is not filled. Please correct ’Complement of Information’.");
			previousDocument.CSI_ReferenceNumber2 = "ONEVERYLONGAGENT**";
			AssertHasMessageError("7. Message error when agent code is longer than 6 characters", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The agent code (aaaaaa) is longer the 6 positions. Please correct ’Complement of Information’.");

			AssertHasMessageError("8. Message error when item number is different from 4 characters", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The item number (iiii) must contain 4 positions. Please correct the item number.");
			previousDocument.CSI_ReferenceNumber2 = "*1234*";
			AssertNoMessageError("9. No Message error when item number is exactly 4 characters", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The item number (iiii) must contain 4 positions. Please correct the item number.");

			AssertHasMessageError("10. Message error when BL number is not filled", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The B/L number (bbbbbbbbbbbbbb) is not filled. Please correct ’Complement of Information’.");
			previousDocument.CSI_ReferenceNumber2 = "*1234*1234567890ABCDE";
			AssertNoMessageError("11. No Message error when BL number is filled", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The B/L number (bbbbbbbbbbbbbb) is not filled. Please correct ’Complement of Information’.");

			AssertHasMessageError("12. Message error when BL number is more than 14 characters", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The B/L number (bbbbbbbbbbbbbb) can be no longer than 14 positions. Please correct ‘Complement of Information’.");
			previousDocument.CSI_ReferenceNumber2 = "*1234*1234567890ABCD";
			AssertNoMessageError("13. No Message error when BL number is exaclty 14 characters", previousDocument.CSI_ReferenceNumber2Info, "The format of ‘Complement of Information’ must be aaaaaa*iiii*bbbbbbbbbbbbbb. The B/L number (bbbbbbbbbbbbbb) can be no longer than 14 positions. Please correct ‘Complement of Information’.");

			previousDocument.CSI_Code = "XXX";
			previousDocument.CSI_ReferenceNumber2 = "1234567890ABCDEFGHI";
			AssertNoMessageErrors("14. No message error when the previous document is not a cargo manifest", previousDocument.CSI_ReferenceNumber2Info);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		previousDocument = nctsHeader.PreviousDocuments.AddNew();
	}
	CommonPreviousDocument previousDocument;
}
