using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.Business
{
	public static class EnhancedValidationParticipationHelper
	{
		public static IEnumerable<CusEntryHeader> EnhancedValidationParticipation(JobDeclaration declaration, IEnumerable<CusEntryHeader> entries)
		{
			var warningFactor = GetWarningFactor();

			if (!warningFactor.HasValue)
			{
				yield break;
			}

			var factory = declaration.Factory;

			if (!CheckEORITreatment(factory, declaration.GetEori()))
			{
				yield break;
			}

			foreach (var e in entries)
			{
				if (CheckLRNTreatment(factory, e.LRN, warningFactor.Value))
				{
					yield return e;
				}
			}
		}

		public static bool IsSubjectToTreatment(WarningFactor warningFactor, CusEntryHeader entry)
		{
			var declaration = entry.Declaration;
			var factory = declaration.Factory;
			return CheckEORITreatment(factory, declaration.GetEori()) && CheckLRNTreatment(factory, entry.LRN, warningFactor);
		}

		public static WarningFactor? GetWarningFactor()
		{
			if (GBCustomsDataRegistry.Instance.DigitalPromptsTrialParticipateInNudgeTrial.Value
				&& ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today))
			{
				var warningFactorStr = ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.WarningFactor);
				if (decimal.TryParse(warningFactorStr, out var warningFactor))
				{
					return warningFactor switch
					{
						2 => WarningFactor.Two,
						4 => WarningFactor.Four,
						8 => WarningFactor.Eight,
						16 => WarningFactor.Sixteen,
						_ => null,
					};
				}
			}

			return null;
		}

		internal static ZBool CheckEORITreatment(BusinessObjectFactory factory, ZString eori)
			=> factory.GetCachedValue<ZBool>($"EORITreatment|{eori}", () => GetFirstIntFromSHA256(eori) % 2 == 1);

		internal static ZBool CheckLRNTreatment(BusinessObjectFactory factory, ZString lrn, WarningFactor warningFactor)
			=> factory.GetCachedValue<ZBool>($"LRNTreatment|{lrn}|{warningFactor}", () => treatmentThresholdLookup.TryGetValue(warningFactor, out var threshold) && GetFirstIntFromSHA256(lrn) <= threshold);

		static int GetFirstIntFromSHA256(string input)
		{
			using var hash = SHA256.Create();
			var computedHash = hash.ComputeHash(Encoding.UTF8.GetBytes($"{input}peppermint"));
			var firstHex = computedHash[0].ToString("X2")[0].ToString();
			return Convert.ToInt32(firstHex, 16);
		}

		public enum WarningFactor
		{
			Two = 0,
			Four = 1,
			Eight = 2,
			Sixteen = 3
		}

		static readonly ImmutableDictionary<WarningFactor, int> treatmentThresholdLookup = new Dictionary<WarningFactor, int>()
		{
			[WarningFactor.Two] = 7,
			[WarningFactor.Four] = 3,
			[WarningFactor.Eight] = 1,
			[WarningFactor.Sixteen] = 0
		}.ToImmutableDictionary();
	}
}
