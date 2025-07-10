using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class PortLoader
	{
		#region Constructors

		public PortLoader()
		{
		}

		#endregion

		#region Methods

#if DEBUG
		public static ZString ClosestPortCodeForTesting
		{
			get
			{
				return closestPortCodeForTesting;
			}
			set
			{
				closestPortCodeForTesting = value;
				if (!value.IsEmpty)
				{
					ReturnEmptyClosestPortCodeForTesting = false;
				}
			}
		}

		[ThreadStatic]
		static ZString closestPortCodeForTesting;

		public static bool ReturnEmptyClosestPortCodeForTesting
		{
			get
			{
				return returnEmptyClosestPortCodeForTesting;
			}
			set
			{
				returnEmptyClosestPortCodeForTesting = value;
			}
		}

		[ThreadStatic]
		static bool returnEmptyClosestPortCodeForTesting;
#endif

		public ZString GetClosestPortCode(JobDocAddress docAddress)
		{
			return GetClosestPortCode(docAddress, false, true);
		}

		public ZString GetClosestPortCode(JobDocAddress docAddress, bool searchByPostCode, bool returnZZZAsEmpty)
		{
#if DEBUG
			if (PortLoader.ReturnEmptyClosestPortCodeForTesting)
			{
				return ZString.Empty;
			}
			if (!PortLoader.ClosestPortCodeForTesting.IsEmpty)
			{
				return PortLoader.ClosestPortCodeForTesting;
			}
#endif
			ZString result = ZString.Empty;
			if (docAddress != null)
			{
				if (docAddress.E2_AddressOverride || searchByPostCode)
				{
					result = GetClosestPortCode(docAddress.E2_Postcode, docAddress.E2_City, docAddress.E2_State, docAddress.E2_RN_NKCountryCode, ZString.Empty, docAddress.Factory, returnZZZAsEmpty);
				}
				else
				{
					if (docAddress.Address != null)
					{
						result = docAddress.Address.OA_RL_NKRelatedPortCode;
						if (result.IsEmpty && docAddress.Organisation != null)
						{
							result = docAddress.Organisation.OH_RL_NKClosestPort;
						}
					}
				}
			}
			return result;
		}

		public ZString GetClosestPortCode(ZString postcode, ZString city, ZString state, ZString countryCode, ZString countryName, BusinessObjectFactory factory)
		{
			return GetClosestPortCode(postcode, city, state, countryCode, countryName, factory, true);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1307")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public ZString GetClosestPortCode(ZString postcode, ZString city, ZString state, ZString countryCode, ZString countryName, BusinessObjectFactory factory, ZBool returnZZZAsEmpty)
		{
#if DEBUG
			if (PortLoader.ReturnEmptyClosestPortCodeForTesting)
			{
				return ZString.Empty;
			}
			if (!PortLoader.ClosestPortCodeForTesting.IsEmpty)
			{
				return PortLoader.ClosestPortCodeForTesting;
			}
#endif
			ZString result = ZString.Empty;

			if (factory == null)
			{
				factory = new BusinessObjectFactory();
			}

			if (!postcode.IsEmpty)
			{
				ZDBOnlyQuery findByPostalCodeQuery = new ZDBOnlyQuery(typeof(RefDomesticCartageZone));
				findByPostalCodeQuery.AddToFilter(RefDomesticCartageZoneSchema.F1_CityTownPostCode, SQLComparisonOperator.Equal, postcode);
				findByPostalCodeQuery.OrderBy = string.Format("PATINDEX('[A-Z]', {0}) DESC, {1} ASC,  {2}", RefDomesticCartageZoneSchema.F1_Zone.Name, RefDomesticCartageZoneSchema.F1_Zone.Name, RefDomesticCartageZoneSchema.F1_Distance.Name);

				if (countryCode.IsEmpty && !countryName.IsEmpty)
				{
					RefCountry country = RefCountry.LoadFromCountryName(factory, countryName);
					if (country != null)
					{
						countryCode = country.RN_Code;
					}
				}

				if (!countryCode.IsEmpty && !countryCode.StartsWith("ZZ", StringComparison.Ordinal))
				{
					findByPostalCodeQuery.AddToFilter(RefDomesticCartageZoneSchema.F1_RL_NKLoco, SQLComparisonOperator.StartsWith, countryCode);
				}

				RefDomesticCartageZone cartageZone = (factory).LoadTop1<RefDomesticCartageZone>(findByPostalCodeQuery);
				result = cartageZone != null ? cartageZone.F1_RL_NKLoco : ZString.Empty;
			}
			if (result.IsEmpty)
			{
				result = new PortCodeLoader().GenerateRequiredPortCode(factory, !countryCode.IsEmpty ? countryCode : countryName, city, state);
				if (result.EndsWith("ZZZ") && returnZZZAsEmpty)
				{
					result = ZString.Empty;
				}
#if DEBUG
				PortCodeLoaderCalledForPortCode = true;
#endif
			}

			return result;
		}

#if DEBUG
		public bool PortCodeLoaderCalledForPortCode;
#endif

		#endregion
	}
}
