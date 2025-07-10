using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using static Enterprise.ZArchitecture.Business.CountrySpecificTypeDecider;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestHeaderTypeDecider : TypeDecider, Integration.Customs.ASYCUDA.IAsycudaManifestHeaderTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			var data = GetData(row);
			switch (data.applicationCode)
			{
				case ApplicationCodeTypeList.Codes.Consolidator:
				case ApplicationCodeTypeList.Codes.ShippingLine:
					result = GetGlobalManifestType(factory, data.countryCode, data.manifestType, data.applicationCode);
					break;
				case AsycudaManifestHeader.ApplicationCode_ZAOutturnGateInOut:
					result = ObjectFactory.GetType<Integration.Customs.ZA.IAsycudaManifestHeader>();
					break;
				case ApplicationCodeTypeList.Codes.TRETrade:
					result = ObjectFactory.GetType<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
					break;
				case ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration:
					result = ObjectFactory.GetType<Integration.Customs.ASYCUDA.TWBriefCustomsDeclaration.IAsycudaManifestHeader>();
					break;
				case ApplicationCodeTypeList.Codes.EuH7:
				case ApplicationCodeTypeList.Codes.EuH7V1:
				case ApplicationCodeTypeList.Codes.EuH7V2:
					result = GetEUH7Type(factory, data.countryCode, data.manifestType, data.applicationCode);
					break;
				default:
					result = typeof(ManifestBase.AsycudaManifestHeader);
					break;
			}
			return result;
		}

		public Type GetGlobalManifestType(BusinessObjectFactory factory, ZString countryCode, ZString manifestType, ZString applicationCode)
		{
			Type result = GetTypeFromApplicationBusinessProvider(factory, countryCode, manifestType, applicationCode);
			ZBool countrySpecificTypeExists = false;

			// TODO: JPG - This part should be removed, but leaving it in for now to limit the scope of the current WI.
			if (result == null)
			{
				foreach (var countrySpecificType in countrySpecificTypes)
				{
					if (countrySpecificType.CountryCode == countryCode)
					{
						countrySpecificTypeExists = true;
						result = countrySpecificType.BusinessObjectType;
						if (typeof(IGlobalManifestTypeDecider).IsAssignableFrom(result))
						{
							var typeDecider = (IGlobalManifestTypeDecider)Activator.CreateInstance(result);
							result = typeDecider.GetType(manifestType);
						}

						break;
					}
				}
			}

			if (result == null && !countrySpecificTypeExists)
			{
				foreach (var pair in DataGroupSepcificTypes)
				{
					if (pair.Key.Invoke(countryCode))
					{
						return pair.Value.Invoke();
					}
				}
			}

			return result ?? ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
		}

		Type GetEUH7Type(BusinessObjectFactory factory, ZString countryCode, ZString manifestType, ZString applicationCode)
		{
			return GetTypeFromApplicationBusinessProvider(factory, countryCode, manifestType, applicationCode)
				?? ObjectFactory.GetType<Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader>();
		}

		Type GetTypeFromApplicationBusinessProvider(BusinessObjectFactory factory, ZString countryCode, ZString manifestType, ZString applicationCode)
		{
			Type result = null;
			if (factory != null)
			{
				var provider = ApplicationBusinessProvider.GetApplicationBusinessProvider(factory, (countryCode, manifestType, applicationCode));
				if (provider != null)
				{
					result = provider.AsycudaManifestHeaderType;
				}
			}

			return result;
		}

		(ZString applicationCode, ZString countryCode, ZString manifestType) GetData(DataRow row)
		{
			var applicationCode = ZString.Empty;
			var countryCode = ZString.Empty;
			var manifestType = ZString.Empty;
			if (row != null)
			{
				applicationCode = new ZString(row[AsycudaManifestHeader.Schema.AMA_ApplicationCode]);
				countryCode = new ZString(row[AsycudaManifestHeader.Schema.AMA_RN_NKCountry]);
				manifestType = new ZString(row[AsycudaManifestHeader.Schema.AMA_ManifestType]);
			}
			return (applicationCode, countryCode, manifestType);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaManifestHeader);
		}

		public override Type GetTypeForNew() => GetGlobalManifestType(null, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZString.Empty, ZString.Empty);

		readonly ImmutableArray<CountrySpecificType> countrySpecificTypes = new[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.ASYCUDA.NZManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Uruguay, ObjectFactory.GetType<Integration.Customs.ASYCUDA.UYManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Fiji, ObjectFactory.GetType<Integration.Customs.ASYCUDA.FJManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Mexico, ObjectFactory.GetType<Integration.Customs.ASYCUDA.MXManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Chile, ObjectFactory.GetType<Integration.Customs.ASYCUDA.CLManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.GBGVMS.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.ASYCUDA.BRManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Colombia, ObjectFactory.GetType<Integration.Customs.ASYCUDA.COManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Argentina, ObjectFactory.GetType<Integration.Customs.ASYCUDA.ARManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.ASYCUDA.JPManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.ASYCUDA.TWManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.CookIslands, ObjectFactory.GetType<Integration.Customs.ASYCUDA.CKManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Israel, ObjectFactory.GetType<Integration.Customs.ASYCUDA.ILManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Peru, ObjectFactory.GetType<Integration.Customs.ASYCUDA.PEManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.ASYCUDA.AEManifest.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IAsycudaManifestHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.VietNam, ObjectFactory.GetType<Integration.Customs.ASYCUDA.VNManifest.IAsycudaManifestHeader>)
		}.ToImmutableArray();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(countryCode => ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(countryCode), () => DefaultTypeForEuCountry);
			}
		}

		protected virtual Type DefaultTypeForEuCountry
		{
			get { return ObjectFactory.GetType<Integration.Customs.ASYCUDA.EUManifest.IAsycudaManifestHeader>(); }
		}
	}
}
