using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public sealed class ManifestHeadersWrapper : NonPersistentBusinessObject<ManifestHeadersWrapperValidation>
	{
		public ManifestHeadersWrapper(ForwardingConsol consol)
			: base(consol.Factory)
		{
			Consol = Argument.NotNull(consol, nameof(consol));
		}

		public ForwardingConsol Consol { get; }

		#region Properties

		[List(nameof(CountryCodes))]
		[MaxLength(AsycudaManifestHeader.Schema.AMA_RN_NKCountryMaxLength)]
		public ZString WR_CountryCode
		{
			get => countryCode;
			set
			{
				using (SuspendSettingHasChanges())
				{
					var oldValue = WR_CountryCode;
					SetNonPersistentPropertyValue(WR_CountryCodeInfo, ref countryCode, value);
					if (oldValue != WR_CountryCode)
					{
						var manifestTypeCodes = ManifestTypes.GetAllCodes();
						WR_ManifestType = manifestTypeCodes.Length == 1 ? manifestTypeCodes[0] : string.Empty;

						if (!IsValidationSuspended)
						{
							Validation.ValidateWR_CountryCode();
						}
					}
				}
			}
		}
		ZString countryCode;

		public ZPropertyInfo WR_CountryCodeInfo => GetZPropertyInfo(nameof(WR_CountryCode));

		[List(nameof(ManifestTypes))]
		[MaxLength(AsycudaManifestHeader.Schema.AMA_ManifestTypeMaxLength)]
		public ZString WR_ManifestType
		{
			get => manifestType;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(WR_ManifestTypeInfo, ref manifestType, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateWR_ManifestType();
					}
				}
			}
		}
		ZString manifestType;

		public ZPropertyInfo WR_ManifestTypeInfo => GetZPropertyInfo(nameof(WR_ManifestType));

		[List(nameof(KeywordCombinations))]
		public ZString WR_KeywordCombination
		{
			get => keywordCombination;
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(WR_KeywordCombinationInfo, ref keywordCombination, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateWR_KeywordCombination();
					}
				}
			}
		}
		ZString keywordCombination;

		public ZPropertyInfo WR_KeywordCombinationInfo => GetZPropertyInfo(nameof(WR_KeywordCombination));

		public AsycudaManifestHeader ManifestToDelete
		{
			get
			{
				AsycudaManifestHeader manifest = null;
				var (countrycode, manifesttype) = GetKeywordsToDeleteManifest();
				if (!countrycode.IsEmpty)
				{
					manifest = Headers.Cast<AsycudaManifestHeader>().FirstOrDefault(x => x.AMA_RN_NKCountry == countrycode && x.AMA_ManifestType == manifesttype);
				}

				return manifest;
			}
		}

		(ZString CountryCode, ZString ManifestType) GetKeywordsToDeleteManifest()
		{
			var manifestToDeleteStr = WR_KeywordCombination;
			if (!manifestToDeleteStr.IsEmpty)
			{
				if (manifestToDeleteStr.Contains('|'))
				{
					var keywords = manifestToDeleteStr.Split(new[] { '|' }, 2);
					var countrycode = keywords[0];
					if (!countrycode.IsEmpty)
					{
						return (countrycode, keywords[1]);
					}
				}
				else
				{
					return (manifestToDeleteStr, ZString.Empty);
				}
			}

			return (ZString.Empty, ZString.Empty);
		}

		public void DeleteManifest(AsycudaManifestHeader manifest)
		{
			Headers.RemoveAndDelete(manifest);
		}

		#endregion

		public IDictionary<ZString, ISet<ZString>> GetConsolCountriesAndTransportModesThatMightNeedManifest() => Factory.GetValue(ref consolCountriesAndTransporModesThatMightNeedManifestCached, () =>
		{
			var dictionary = new Dictionary<ZString, ISet<ZString>>();
			AddCountryAndTransportIfNeeded(dictionary, Consol.JK_TransportMode, Consol.JK_RL_NKLoadPort, Consol.JK_RL_NKDischargePort);
			Consol.Transports.Cast<Transport>().ForEach(x => AddCountryAndTransportIfNeeded(dictionary, x.JW_TransportMode, x.JW_RL_NKLoadPort, x.JW_RL_NKDiscPort));
			return dictionary;
		});
		CachedProperty<IDictionary<ZString, ISet<ZString>>> consolCountriesAndTransporModesThatMightNeedManifestCached;

		void AddCountryAndTransportIfNeeded(Dictionary<ZString, ISet<ZString>> dictionary, ZString transportMode, ZString loadCountry, ZString dischargeCountry)
		{
			if (!transportMode.IsEmpty)
			{
				UpdateTransportMode(loadCountry.Left(2));
				UpdateTransportMode(dischargeCountry.Left(2));
			}

			void UpdateTransportMode(ZString countryToUpdate)
			{
				if (!countryToUpdate.IsEmpty
					&& countryToUpdate != Core.Constants.CountryCodes.Singapore
					&& AsycudaManifestHeaderLookups.GetAcceptableTransportModesFromApplicationBusinessProvider(Factory, countryToUpdate, ApplicationCodeTypeList.Codes.Consolidator, Consol.JobDirection).Contains(transportMode))
				{
					var transportModes = dictionary.GetOrAdd(countryToUpdate, () => new HashSet<ZString>());
					transportModes.Add(transportMode);
				}
			}
		}

		public bool IsManifestConsolDecouplingEnabled => AsycudaManifestHeader.IsManifestConsolDecouplingEnabled;

		public ZString CorrectCountryCodeIfNeeded(ZString countryCode)
		{
			var result = countryCode;

			if (!CountryCodes.ContainsCode(result))
			{
				var providers = ZZDatabaseValidationHelper.GetNVCApplicationBusinessProviders(Factory);

				foreach (var provider in providers)
				{
					if (provider.CountryCodes.Contains(countryCode))
					{
						var countryCodeFromProvider = provider.GetManifestDescriptions(Factory, new[] { result }, (type) => true)
							.FirstOrDefault(c => !c.CountryCode.IsEmpty)
							.CountryCode;

						if (!countryCodeFromProvider.IsEmpty)
						{
							result = countryCodeFromProvider;
							break;
						}
					}
				}
			}

			return result;
		}

		#region Lookups

		public CodeDescriptionPairList CountryCodes
		{
			get
			{
				return Factory.GetCachedValue($"ManifestHeadersWrapper+CountryCodes+{Consol.JobDirection}+{Consol.TransportMode}", () =>
				{
					var result = new CodeDescriptionPairList();

					result.AddPairsEvenIfThisWillCauseDuplicateEntries(ZZDatabaseValidationHelper.GetNVCApplicationBusinessProviders(Factory)
						.SelectMany(provider => provider.GetManifestDescriptions(Factory, provider.ApplicableCountryCodes(Consol.JobDirection, Consol.TransportMode, ApplicationCodeTypeList.Codes.Consolidator), type => true))
						.Select(x => new CodeDescriptionPair(x.CountryCode.ToString(), x.Description.ToString())).OrderBy(c => c.Description));
					result.RemoveCode(Core.Constants.CountryCodes.Singapore);
					return SortByDescriptionUnifyIdenticalCountryCodesUnderOneDescription(result);
				});
			}
		}

		public CodeDescriptionPairList ManifestTypes => GetManifestTypes(WR_CountryCode);

		public CodeDescriptionPairList GetManifestTypes(string countryCode)
		{
			return Factory.GetCachedValue(string.Join("_", "ManifestHeadersWrapper+ManifestTypes", countryCode, Consol.JobDirection, Consol.TransportMode), () =>
			{
				var result = new CodeDescriptionPairList();

				result.AddPairsIfNotExist(ZZDatabaseValidationHelper.GetNVCApplicationBusinessProviders(Factory)
					.Where(provider => provider.ApplicableCountryCodes(Consol.JobDirection, Consol.TransportMode, ApplicationCodeTypeList.Codes.Consolidator).Contains((ZString)countryCode))
					.SelectMany(provider => provider.GetApplicableManifestTypes(Consol.JobDirection, Consol.TransportMode))
					.Select(manifestType => new CodeDescriptionPair(manifestType.Code,
						manifestType.Description)));

				return result;
			});
		}

		public CodeDescriptionPairList KeywordCombinations
		{
			get
			{
				var result = new CodeDescriptionPairList();

				Headers.Cast<AsycudaManifestHeader>().Select(x => new
				{
					CountryCode = x.AMA_RN_NKCountry,
					CountryName = x.Country?.Description ?? ZString.Empty,
					ManifestType = x.AMA_ManifestType,
					ManifestTypeDescription = x.AMA_ManifestTypeDescription
				})
					.Where(x => !x.CountryCode.IsEmpty)
					.OrderBy(x => x.CountryCode)
					.ForEach(
						x =>
						{
							if (x.ManifestType.IsEmpty)
							{
								result.AddPairIfNotExist(x.CountryCode, x.CountryName);
							}
							else
							{
								result.AddPairIfNotExist(Invariant($"{x.CountryCode}|{x.ManifestType}"), Invariant($"{x.CountryName}|{x.ManifestTypeDescription}"));
							}
						});

				result.Sort();

				return result;
			}
		}

		public CodeDescriptionPairList SortByDescriptionUnifyIdenticalCountryCodesUnderOneDescription(CodeDescriptionPairList list)
		{
			var result = new CodeDescriptionPairList();
			var duplicateCodes = list.GetAllCodesZString().GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key);
			if (duplicateCodes.Any())
			{
				foreach (var duplicateCode in duplicateCodes)
				{
					var duplicatePairs = list.Cast<ICodeDescription>().Where(x => x.Code == duplicateCode);
					var combinedCode = duplicatePairs.First().Code;
					var refCountry = MasterFiles.Business.RefCountry.LoadFromCountryCode(Factory, (ZString)combinedCode);
					var listWithoutDuplicate = list.Cast<ICodeDescription>().Where(c => c.Code != duplicateCode);
					result.AddPairsIfNotExist(listWithoutDuplicate);
					result.AddPairIfNotExist(combinedCode, refCountry.Description);
				}
			}
			else
			{
				result.AddPairsIfNotExist(list.Cast<ICodeDescription>());
			}
			result.SortByDescription();
			return result;
		}

		#endregion

		#region Header

		public AsycudaManifestHeaderCollection Headers
		{
			get
			{
				if (headers == null)
				{
					headers = new AsycudaManifestHeaderCollection(Consol);
					headers.Load();
					Consol.RegisterEditableChildObject(headers);
				}

				return headers;
			}
		}
		AsycudaManifestHeaderCollection headers;

		public void SynchroniseHeaders()
		{
			foreach (AsycudaManifestHeader header in Headers)
			{
				header.SynchroniseWithSourceIfNeeded();
			}
		}

		public void UnloadHeaders()
		{
			if (headers != null)
			{
				if (Consol != null && !Consol.IsDeleted)
				{
					Consol.UnRegisterEditableChildObject(headers);
				}

				foreach (AsycudaManifestHeader header in headers)
				{
					if (header != null && !header.IsDeleted)
					{
						header.RemoveSynchroniser();
					}
				}

				headers = null;
			}
		}

		public AsycudaManifestHeader CreateCountry(ZString countryCode, ZString manifestType)
		{
			var header = Headers.GetHeader(countryCode, manifestType);
			if (header == null && CountryCodes.ContainsCode(countryCode) && GetManifestTypes(countryCode).ContainsCode(manifestType))
			{
				var typeDecider = new AsycudaManifestHeaderTypeDecider();
				header = (AsycudaManifestHeader)Factory.New(typeDecider.GetGlobalManifestType(Factory, countryCode, manifestType, ApplicationCodeTypeList.Codes.Consolidator));
				using (header.GetValidationSuspender())
				using (header.GetCheckBusinessObjectTypeSuspender())
				{
					if (!countryCode.IsEmpty &&
					countryCode != Core.Constants.CountryCodes.EuropeanUnion &&
					header.AMA_RN_NKCountry != countryCode)
					{
						header.AMA_RN_NKCountry = countryCode;
					}
					header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
					header.AMA_ManifestType = manifestType;
				}
				Headers.Add(header);
				header.SynchroniseWithSourceIfNeeded();
				header.HasChanges = true;
			}

			return header;
		}

		#endregion

		#region Validation

		public override ManifestHeadersWrapperValidation GetNewValidation()
		{
			return new ManifestHeadersWrapperValidation(this);
		}

		#endregion
	}
}
