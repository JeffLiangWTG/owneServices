using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.Business
{
	public sealed class LRNGenerator
	{
		public static class Constants
		{
			public static class TemporaryStorage
			{
				public const string LrnPrefix = "TS";
				public const string FountainPrefix = "IETemporaryStorageLocalReferenceNumber";
			}

			public static class AES
			{
				/// <summary>
				/// WI00649539 : Share number fountain for AES / AIS / NCTS. 'IEAESLocalReferenceNumber' is used because the key name is not important and AES was first
				/// </summary>
				public const string FountainPrefix = "IEAESLocalReferenceNumber";
			}
		}

		public LRNGenerator(BusinessObjectFactory businessObjectFactory, GlbBranch branch) : this(
			businessObjectFactory,
			branch,
			GlbCompany.CurrentCompany.LicenceEnterpriseCode,
			Constants.AES.FountainPrefix
		)
		{ }

		public LRNGenerator(BusinessObjectFactory businessObjectFactory, GlbBranch branch, string lrnPrefix, string fountainPrefix)
		{
			Factory = businessObjectFactory;
			Branch = branch;
			LrnPrefix = lrnPrefix;
			FountainPrefix = fountainPrefix;
		}
		BusinessObjectFactory Factory { get; }
		GlbBranch Branch { get; }
		string LrnPrefix { get; }
		string FountainPrefix { get; }

		public string GetLRNAndSetIfNeeded(ZPropertyInfoString info, Func<bool> isLastEDIMessageReceivedASyntaxError)
		{
			var lrn = info.Value;
			var isOldFormat = lrn.Length == 17;
			if (lrn.IsEmpty || !new Regex(isOldFormat ? OldLrnPattern : NewLrnPattern).IsMatch(lrn))
			{
				info.Value = GetNewLRN();
			}
			else if (!info.HasChanges && !isLastEDIMessageReceivedASyntaxError())
			{
				info.Value = isOldFormat ? GetIncrementedLRNVersion(lrn, OldLRNFullLength, OldLRNMainSequenceLength, OldLRNVersionNumberLength) : GetIncrementedLRNVersion(lrn, LRNFullLength, LRNMainSequenceLength, LRNVersionNumberLength);
			}

			return info.Value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Regex pattern")]
		const string OldLrnPattern = "^[A-Za-z0-9\\s]{3}\\d{10}V\\d{3}"; //TODO: Remove old pattern and check after 01-MAY-24

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Regex pattern")]
		const string NewLrnPattern = "^[A-Za-z0-9\\s]{9}\\d{10}V\\d{2}";

		string GetNewLRN()
		{
			return GetFormattedLRNSequence(GetNewLRNSequence()) + GetFormattedLRNVersion(1, 2);
		}

		string GetIncrementedLRNVersion(ZString lrn, int fullLength, int mainSequenceLength, int versionNumberLength)
		{
			if (lrn.IsEmpty || lrn.Length != fullLength)
			{
				throw new InvalidOperationException("lrn is empty or has an unexpected length");
			}

			var mainSequence = lrn.Left(mainSequenceLength);
			var versionNumber = lrn.Substring(mainSequenceLength + 1, versionNumberLength);
			return mainSequence + GetFormattedLRNVersion(int.Parse(versionNumber) + 1, versionNumberLength);
		}

		const int OldLRNMainSequenceLength = 13;
		const int OldLRNVersionNumberLength = 3;
		const int OldLRNFullLength = 17;
		const int LRNMainSequenceLength = 19;
		const int LRNVersionNumberLength = 2;
		const int LRNFullLength = 22;

		long GetNewLRNSequence()
		{
			return GetNumberFountain().GetNext(Factory);
		}

		INumberFountainProxy GetNumberFountain() => Env.NumberFountains.GetIENumberFountain(GetFountainKey());

		ZString GetFormattedLRNSequence(long sequence)
		{
			var lrnPrefix = GetLrnPrefix();
			var sequenceLength = LRNFullLength - lrnPrefix.Length - 2 - 3;
			return FormattableString.Invariant($"{lrnPrefix}{ZDateTime.UtcToday:yy}{sequence.ToString().PadLeft(sequenceLength, '0')}");
		}

		ZString GetLrnPrefix()
		{
			return $"{LrnPrefix}{GlbCompany.CurrentCompany.LicenceServerID}{Branch.GB_Code}";
		}

		ZString GetFormattedLRNVersion(int version, int length) => FormattableString.Invariant($"V{version.ToString().PadLeft(length, '0')}");

		ZString GetFountainKey() => FormattableString.Invariant($"{FountainPrefix}_{Branch.GB_Code}_{ZDateTime.UtcToday.Year}");
	}
}
