using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.NCTS.Business;

public static class PreviousDocumentValidationHelper
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const locals")]
	public static void CheckCSI_ReferenceNumberN785(ZString code, ZString referenceNumber, ZPropertyInfo referenceNumberInfo)
	{
		const string format = "pssssssLnnnnnnn*aaaa";
		const string place1 = "Antwerpen";
		const string place2 = "Zeebrugge";
		const string place3 = "Gent";

		if (code == Constants.PreviousDocumentTypes.CargoManifest)
		{
			var position1 = referenceNumber.SubstringSafe(0, 1);
			var position8 = referenceNumber.SubstringSafe(7, 1);
			var position16 = referenceNumber.SubstringSafe(15, 1);
			var range17To20 = referenceNumber.SubstringSafe(16, 4);

			if (position1 != "1" && position1 != "2" && position1 != "3")
			{
				referenceNumberInfo.AddMessageError(Res.GetString("AD12268C-A6FE-4907-BCEB-96AC5E62B9D1", "The format of ‘Reference number’ must be {0}. First position of the reference number must be 1(={1}), 2(={2}) or 3(={3}) when type of previous document is N785. Please correct the reference number.", format, place1, place2, place3));
			}

			if (position8 != "L")
			{
				referenceNumberInfo.AddMessageError(Res.GetString("6E425161-87F2-42C8-8F13-4CF815D6DABC", "The format of ‘Reference number’ must be {0}. Position 2 to 7 must contain the stay number and position 8 must be L when type of previous document is N785. Please check stay number and position 8 of the reference number.", format));
			}

			if (position16 != "*")
			{
				referenceNumberInfo.AddMessageError(Res.GetString("1668202A-4C32-4536-9724-AF3144A0BA19", "The format of ‘Reference number’ must be {0}. Position 16 must be a * when the type is N785. Please correct the reference number.", format));
			}

			if (!range17To20.IsNumbersOnlyOrEmpty || range17To20.IsEmpty)
			{
				referenceNumberInfo.AddMessageError(Res.GetString("1311522D-A0A3-4A2A-94C1-1F523EC04E05", "The format of ‘Reference number’ must be {0}. Position 17, 18, 19 and 20 must each contain a numeric value.", format));
			}
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const locals")]
	public static void CheckCSI_ReferenceNumber2N785(ZString code, ZString referenceNumber2, ZPropertyInfo referenceNumber2Info)
	{
		const string format = "aaaaaa*iiii*bbbbbbbbbbbbbb";
		const string agentCode = "aaaaaa";
		const string itemNumber = "iiii";
		const string blNumber = "bbbbbbbbbbbbbb";

		if (code == Constants.PreviousDocumentTypes.CargoManifest)
		{
			var numberOfAsterixes = referenceNumber2.Occurrences("*");
			var firstAsterixPosition = referenceNumber2.IndexOf("*");
			var lastAsterixPosition = referenceNumber2.LastIndexOf("*");
			var totalLength = referenceNumber2.TrimEnd().Length;

			if (numberOfAsterixes != 2)
			{
				referenceNumber2Info.AddMessageError(Res.GetString("72681D3A-A56E-4E0B-BF8B-9A07E1B57FFA", "The format of ‘Complement of Information’ must be {0}. The separator * can only be used twice/ Please fill in an agent code (maximum 6 positions) followed by  * AND followed by the item number (exactly 4 positions) AND followed by * AND followed by a B/L number (maximum 14 positions)  when type of previous document is N785. Please correct ’Complement of Information’.", format));
			}

			if (firstAsterixPosition == 0)
			{
				referenceNumber2Info.AddMessageError(Res.GetString("1B4507F6-B5CA-4B2F-BD6F-EA8F039D8670", "The format of ‘Complement of Information’ must be {0}. The agent code ({1}) is not filled. Please correct ’Complement of Information’.", format, agentCode));
			}

			if (firstAsterixPosition > 6)
			{
				referenceNumber2Info.AddMessageError(Res.GetString("B97F9EBF-DC50-42CC-9428-B1ED9D481939", "The format of ‘Complement of Information’ must be {0}. The agent code ({1}) is longer the 6 positions. Please correct ’Complement of Information’.", format, agentCode));
			}

			if (lastAsterixPosition - firstAsterixPosition != 5)
			{
				referenceNumber2Info.AddMessageError(Res.GetString("0581D69C-447E-4C21-B4BD-65833DCDA5D0", "The format of ‘Complement of Information’ must be {0}. The item number ({1}) must contain 4 positions. Please correct the item number.", format, itemNumber));
			}

			if (lastAsterixPosition == totalLength - 1)
			{
				referenceNumber2Info.AddMessageError(Res.GetString("E3B788F0-BFA5-464B-AEEB-9BAE8F9E866F", "The format of ‘Complement of Information’ must be {0}. The B/L number ({1}) is not filled. Please correct ’Complement of Information’.", format, blNumber));
			}

			if (totalLength - 1 - lastAsterixPosition > 14)
			{
				referenceNumber2Info.AddMessageError(Res.GetString("3088E9B5-FF41-40E8-AB95-48F57425E92A", "The format of ‘Complement of Information’ must be {0}. The B/L number ({1}) can be no longer than 14 positions. Please correct ‘Complement of Information’.", format, blNumber));
			}
		}
	}
}
