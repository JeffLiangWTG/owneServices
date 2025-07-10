using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class EntryManualReleaseHandler : NonPersistentBusinessObject<EntryManualReleaseHandlerValidation>
{
	public static class Schema
	{
		public const string ReleaseCode = "ReleaseCode";
		public const int ReleaseCodeMaxLength = 6;
		public const string ReleaseDate = "ReleaseDate";
	}

	public EntryManualReleaseHandler(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}
	readonly CusEntryHeader entryHeader;

	[MaxLength(Schema.ReleaseCodeMaxLength)]
	public ZString ReleaseCode
	{
		get { return releaseCode; }
		set
		{
			var oldValue = ReleaseCode;
			SetNonPersistentPropertyValue(ReleaseCodeInfo, ref releaseCode, value);

			if (oldValue != value && !IsValidationSuspended)
			{
				Validation.ValidateReleaseCode();
			}
		}
	}
	ZString releaseCode;

	public ZPropertyInfo ReleaseCodeInfo => GetZPropertyInfo(Schema.ReleaseCode);

	public ZDate ReleaseDate
	{
		get { return releaseDate; }
		set
		{
			var oldValue = releaseDate;
			SetNonPersistentPropertyValue(ReleaseDateInfo, ref releaseDate, value);

			if (oldValue != value && !IsValidationSuspended)
			{
				Validation.ValidateReleaseDate();
			}
		}
	}
	ZDate releaseDate;

	public ZPropertyInfo ReleaseDateInfo => GetZPropertyInfo(Schema.ReleaseDate);

	public EntryManualReleaseResult CanDoManualRelease()
	{
		var hasMovementReferenceNumber = !entryHeader.MovementReferenceNumber.IsEmpty;
		var hasRegistrationNumber = !entryHeader.RegistrationNumber.IsEmpty;
		var hasReferenceOrRegistrationNumber = hasMovementReferenceNumber || hasRegistrationNumber;

		var releaseInfo = entryHeader.EntryNumbersProvider.ReleaseInfo;
		var hasSystemGeneratedReleaseCode = releaseInfo != null && releaseInfo.CE_EntryIsSystemGenerated == true;

		if (hasReferenceOrRegistrationNumber && !hasSystemGeneratedReleaseCode)
		{
			return new EntryManualReleaseResult(canDoManualRelease: true, errorMessage: ZString.Empty);
		}

		var errorMessage = hasSystemGeneratedReleaseCode ? ManualReleaseCaptionsProvider.EntryHasSystemGeneratedReleaseCode : ManualReleaseCaptionsProvider.EntryHasNoRegistrationNumberNorMRN;
		return new EntryManualReleaseResult(canDoManualRelease: false, errorMessage: errorMessage);
	}

	#region Validation

	public override EntryManualReleaseHandlerValidation GetNewValidation()
	{
		return new EntryManualReleaseHandlerValidation(this);
	}

	#endregion

	static class ManualReleaseCaptionsProvider
	{
		public static string EntryHasNoRegistrationNumberNorMRN => Res.GetString("27418431-9DD4-42A8-A877-AC2E05E43CE2", "This Entry has no Registration number nor MRN, it is not possible to insert the Release code.");
		public static string EntryHasSystemGeneratedReleaseCode => Res.GetString("86255F1A-DE4B-40A9-8460-6586D4D32C8D", "This Entry has a System generated Release Code, it is not possible to change it.");
	}
}
