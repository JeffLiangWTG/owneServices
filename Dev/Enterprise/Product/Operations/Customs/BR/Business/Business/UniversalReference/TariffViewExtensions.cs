using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business
{
	public static class TariffViewExtensions
	{
		public static TariffProfileQuestion[] GetTariffNCMCharacteristics(this TariffView tariff, ZDateTime effectiveDate, bool isExport, bool isImport)
		{
			return tariff != null && (isExport || isImport) ? GetTariffNCMCharacteristics() : Array.Empty<TariffProfileQuestion>();

			TariffProfileQuestion[] GetTariffNCMCharacteristics()
			{
				var direction = isExport ? (isImport ? CharacteristicDirection.Both : CharacteristicDirection.Export) : CharacteristicDirection.Import;
				var characteristicType = BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.Value ? Constants.Profile.Types.NCMTE : Constants.Profile.Types.NCM;

				return tariff.Factory.GetCachedValue($"LoadCharacteristics{characteristicType}_{tariff.PK}_{effectiveDate}_{direction}", () =>
				{
					var characteristics = new RefCusTariffBRCharacteristic.Loader(tariff.Factory).LoadCharacteristics(tariff, characteristicType, direction, effectiveDate);
					characteristics.ForEach(x => x.FetchForLoadChildEditableObjectsIfNeeded());
					return characteristics.Distinct().Select(TariffProfileQuestion.New).ToArray();
				});
			}
		}

		public static RefCusTariffBRCharacteristic[] GetTariffNVECharacteristics(this TariffView tariff, ZDateTime effectiveDate)
		{
			return tariff != null ? GetTariffNVECharacteristics() : Array.Empty<RefCusTariffBRCharacteristic>();

			RefCusTariffBRCharacteristic[] GetTariffNVECharacteristics() => tariff.Factory.GetCachedValue($"LoadCharacteristicsNVE_{tariff.PK}_{effectiveDate}", () =>
			{
				var characteristics = new RefCusTariffBRCharacteristic.Loader(tariff.Factory).LoadCharacteristics(tariff, Constants.Profile.Types.NVE, CharacteristicDirection.Import, effectiveDate, true);
				characteristics.ForEach(x => x.FetchForLoadChildEditableObjectsIfNeeded());
				return characteristics;
			});
		}

		public static TariffProfileQuestion[] GetTariffLPCCharacteristics(this TariffView tariff, ZDateTime effectiveDate)
		{
			return tariff != null ? GetTariffLPCCharacteristics() : Array.Empty<TariffProfileQuestion>();

			TariffProfileQuestion[] GetTariffLPCCharacteristics()
			{
				var characteristicType = BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.Value ? Constants.Profile.Types.LPCT : Constants.Profile.Types.LPC;

				return tariff.Factory.GetCachedValue($"LoadCharacteristics{characteristicType}_{tariff.PK}_{effectiveDate}", () =>
				{
					var characteristics = new RefCusTariffBRCharacteristic.Loader(tariff.Factory).LoadCharacteristics(tariff, characteristicType, CharacteristicDirection.Export, effectiveDate);
					characteristics.ForEach(x => x.FetchForLoadChildEditableObjectsIfNeeded());
					return characteristics.Select(TariffProfileQuestion.New).ToArray();
				});
			}
		}

		public static TariffProfileQuestion[] GetTariffNCMQuestions(this TariffView tariff, ZString modality, ZDateTime effectiveDate, string targetValue)
		{
			return tariff != null && !modality.IsEmpty ? GetTariffNCMQuestions() : Array.Empty<TariffProfileQuestion>();

			TariffProfileQuestion[] GetTariffNCMQuestions()
			{
				var profileType = BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.Value ? Constants.Profile.Types.NCMTE : Constants.Profile.Types.NCM;

				return tariff.Factory.GetCachedValue($"GetTariffNCMQuestions_{tariff.ZZ1_TariffCode}_{tariff.ZZ1_ZZI_TariffType}_{profileType}_{modality}_{effectiveDate}_{targetValue}", () =>
				{
					var questions = new RefCusProfileQuestion.Loader(tariff.Factory).Load(profileType, tariff.ZZ1_ZZI_TariffType, tariff.ZZ1_TariffCode, Core.Constants.CountryCodes.Brazil, effectiveDate, true,
										profileAttributes: new[] { (Constants.Profile.AttributeNames.Modality, new[] { modality.ToString() }) },
										questionAttributes: new[] { (Constants.ProfileQuestion.AttributeNames.Target, targetValue) });
					questions.ForEach(x => x.FetchForLoadChildEditableObjectsIfNeeded());
					return questions.Select(TariffProfileQuestion.New).ToArray();
				});
			}
		}

		static ZString[] TreatmentTributaryProfileTypes => new ZString[]
		{
			Constants.Profile.Types.TTDTY,
			Constants.Profile.Types.TTPIS,
			Constants.Profile.Types.TTCOF,
			Constants.Profile.Types.TTADD,
			Constants.Profile.Types.TTIPI
		};

		static ZString[] TreatmentTributaryProfileTypesForTest => new ZString[]
		{
			Constants.Profile.Types.TTDTYTE,
			Constants.Profile.Types.TTPISTE,
			Constants.Profile.Types.TTCOFTE,
			Constants.Profile.Types.TTADDTE,
			Constants.Profile.Types.TTIPITE
		};

		public static TariffProfile[] GetTariffTTProfiles(this TariffView tariff, ZString countryOfOrigin, ZDateTime effectiveDate)
		{
			return tariff != null && !countryOfOrigin.IsEmpty ? GetTariffTTProfiles() : [];

			TariffProfile[] GetTariffTTProfiles()
			{
				var isProfileTypeForTest = BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.Value;
				return tariff.Factory.GetCachedValue($"GetTariffTTProfiles_{tariff.ZZ1_TariffCode}_{tariff.ZZ1_ZZI_TariffType}_IsForTest={isProfileTypeForTest}_{countryOfOrigin}_{effectiveDate}", () =>
				{
					var profileTypes = isProfileTypeForTest ? TreatmentTributaryProfileTypesForTest : TreatmentTributaryProfileTypes;
					var profiles = new RefCusProfile.Loader(tariff.Factory).Load(profileTypes, tariff.ZZ1_ZZI_TariffType, tariff.ZZ1_TariffCode, Core.Constants.CountryCodes.Brazil, effectiveDate,
							attributes: new[] { (Constants.Profile.AttributeNames.Origin, new[] { countryOfOrigin.ToString(), Core.Constants.Groups.ALL }) });
						profiles.ForEach(x => x.FetchForLoadChildEditableObjectsIfNeeded());
						return profiles.Select(TariffProfile.New).ToArray();
				});
			}
		}
	}
}
