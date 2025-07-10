using System.Collections.Generic;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
	internal static class TransactionNumberMessages
	{
		internal static string CannotAllocateTransactionNumber() => Res.GetString(
			"076A012C-C818-4A7F-AA81-A541CA8DC9EC",
			"Cannot allocate transaction number."
		);

		internal static string AsecNumberNotSet() => Res.GetString(
			"8DB7E726-5FD3-44BD-AB45-5A76E4BC67EE",
			@"ASEC Number is not set for the current company.
Please set ASEC number in the {0} registry setting.",
			CACustomsDataRegistry.Instance.AccountSecurityNo.Location()
		);

		internal static string TransactionNumberRangeNotConfigured(string securityNo) => Res.GetString(
			"930C8059-2C73-4076-80AE-F5D15C3F9D10",
			@"Transaction number range for ASEC Number {0} is not set.
Please set this range in the Maintain > Customs > Transaction Number form.",
			securityNo
		);

		internal static string DIFNotConfigured(string securityNo) => Res.GetString(
			"4BC74642-02AB-4B3A-A3E4-2E19B7E985B2",
			@"DIF transaction number range for ASEC Number {0} is not set.
Please set this range in the Maintain > Customs > Transaction Number form.",
			securityNo
		);

		internal static string TransactionNumberRangeIsEmpty() => Res.GetString(
			"F0496F97-DE86-465A-97A9-3A7EEEB51B9D",
			@"There are no more Transaction Numbers available.
Please go to Maintain > Customs > Transaction Numbers to allocate more numbers."
		);

		internal static string DIFNumberRangeIsEmpty() => Res.GetString(
			"B9545EFB-5758-4599-BD65-76BBE63E58B8",
			@"There are no more available numbers in the number range for DIF.
You won't be able to add a new DIF record until this range is updated in the Maintain > Customs > Transaction Number form."
		);

		internal static string NumberRangeIsAlmostEmpty(bool includeFixHint)
		{
			string message = Res.GetString(
				"FF53D0FB-6B26-4D90-88A9-43D03A32B600",
				"There are fewer than {0:N0} available numbers left in the number range.",
				TransactionNumber.MinRemainingCapacity
			);
			if (includeFixHint)
			{
				message += "\r\n";
				message += Res.GetString("07ABB083-6E3D-4EEA-A361-12F92C15CCFB", "Please update this range in the Maintain > Customs > Transaction Number form.");
			}
			return message;
		}

		internal static string NextNumberIsCloseToNextDeclarationNumber(long nextNumber, IEnumerable<string> intersectingFountains, bool includeFixHint)
		{
			string message = Res.GetString(
				"87DEC109-B070-41D0-9947-E1C2CA0C3AB8",
				@"The next number in the number range ({0:N0}) is too close to the next number in another number range:
{1}
DIF number range should be at least {2:N0} numbers apart from the previous range and {3:N0} numbers apart from the next range.
",
				nextNumber,
				string.Join("\r\n", intersectingFountains),
				TransactionNumber.MinDistanceFromDIFToPreviousRange, TransactionNumber.MinDistanceFromDIFToNextRange
			);
			if (includeFixHint)
			{
				message += "\r\n";
				message += Res.GetString("07ABB083-6E3D-4EEA-A361-12F92C15CCFB", "Please update this range in the Maintain > Customs > Transaction Number form.");
			}
			return message;
		}

		internal static string NextNumberMaximumExceeded() => Res.GetString(
			"1FA314CF-CC0C-45DC-887B-947D44C2DD19",
			"The next number should be less than {0:N0}.",
			TransactionNumber.MaxMaximumNumber
		);

		internal static string FountainDescription(string fountainName, long nextValue) => Res.GetString(
			"E88E9A6E-65B2-4BB4-AFEE-182BE05B4280",
			"{0} - {1:N0}",
			fountainName,
			nextValue
		);

		internal static string TransactionNumberAlreadyUsed(JobDeclaration relatedDeclaration)
		{
			if (relatedDeclaration != null)
			{
				return Res.GetString("cd47fdd8-0ea6-40ce-81fb-b4c3d0d4f04b", "The Transaction Number is already used in {0}. Please use a different number.", relatedDeclaration.JE_DeclarationReference);
			}
			else
			{
				return Res.GetString("2390f8ae-6755-4dd9-b4d3-53f8cafd35f6", "The Transaction Number is already used. Please use a different number.");
			}
		}
	}
}
