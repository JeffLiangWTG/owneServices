using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocUNLOCO : DocBaseWrapper
	{
		DocUNLOCO(RefUNLOCO refUNLOCO, BusinessObjectFactory factoryForWrapper)
			: base(refUNLOCO, factoryForWrapper)
		{
		}

		public static DocUNLOCO New(RefUNLOCO refUNLOCO, BusinessObjectFactory factoryForWrapper)
		{
			return (refUNLOCO != null) ? new DocUNLOCO(refUNLOCO, factoryForWrapper) : null;
		}

		public static DocUNLOCO New(BusinessObjectFactory factory, ZString nK)
		{
			return New(factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, nK), factory);
		}

		public static DocUNLOCO New(JobDocAddress address, BusinessObjectFactory factoryForWrapper)
		{
			DocUNLOCO result;

			if (address == null)
			{
				result = null;
			}
			else if (address.E2_AddressOverride)
			{
				RefUNLOCO port = RefUNLOCO.GetPortFromNameAndCountryCode(factoryForWrapper, address.E2_City, address.E2_RN_NKCountryCode);
				result = New(port, factoryForWrapper);
			}
			else
			{
				result = New(address.Address, factoryForWrapper);
			}

			return result;
		}

		public static DocUNLOCO New(OrgAddress address, BusinessObjectFactory factoryForWrapper)
		{
			DocUNLOCO result;

			if (address == null)
			{
				result = null;
			}
			else if (!address.OA_RL_NKRelatedPortCode.IsEmpty)
			{
				result = New(factoryForWrapper, address.OA_RL_NKRelatedPortCode);
			}
			else
			{
				result = New(factoryForWrapper, address.Header.OH_RL_NKClosestPort);
			}

			return result;
		}

		public ZString ThreeLetterCode
		{
			get { return Code.IsEmpty ? ZString.Empty : this.Code.Substring(0, 3); }
		}

		public ZString CountryName
		{
			get
			{
				return RefUNLOCO.Country == null ? ZString.Empty : RefUNLOCO.Country.RN_DescMultilingual;
		}
		}

		public ZString CountryCode
		{
			get { return (RefUNLOCO.RL_RN_NKCountryCode.IsValid && RefUNLOCO.Country != null) ? RefUNLOCO.Country.RN_Code : ZString.Empty; }
		}

		public ZString CountryCodeAndName
		{
			get { return (RefUNLOCO.RL_RN_NKCountryCode.IsValid && RefUNLOCO.Country != null) ? RefUNLOCO.Country.RN_Code + " - " + RefUNLOCO.Country.RN_DescMultilingual : ""; }
		}

		public ZString Code
		{
			get { return RefUNLOCO.RL_Code; }
		}

		public ZString CodeAndName
		{
			get { return RefUNLOCO.RL_Code + " - " + RefUNLOCO.RL_PortName; }
		}

		public ZString PortName
		{
			get { return RefUNLOCO.RL_PortName; }
		}

		public ZString PortNameAndCountryName
		{
			get { return PortName != CountryName ? ZString.Format("{0}, {1}", PortName, CountryName) : PortName; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Property is to be used when we need English text")]
		public ZString PortNameAndCountryNameInEnglish
		{
			get
			{
				var countryNameInEnglish = RefUNLOCO.Country == null ? ZString.Empty : RefUNLOCO.Country.RN_Desc;
				return PortName != countryNameInEnglish ? ZString.Format("{0}, {1}", PortName, countryNameInEnglish) : PortName;
			}
		}

		public ZDateTime BeginDaySaving
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (RefUNLOCO.TimeZoneSet != null && RefUNLOCO.TimeZoneSet.DaylightSavingZone != null)
				{
					result = RefUNLOCO.TimeZoneSet.GetDaylightSavingStartOrEndDateInYear(ZDateTime.Now.Year, RefUNLOCO.TimeZoneSet.DaylightSavingZone.StartDateRules);
				}

				return result;
			}
		}

		public ZString CoOrdinates
		{
			get { return RefUNLOCO.RL_GeoLocation.AsText(); }
		}

		public ZDateTime EndDaySaving
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (RefUNLOCO.TimeZoneSet != null && RefUNLOCO.TimeZoneSet.DaylightSavingZone != null)
				{
					result = RefUNLOCO.TimeZoneSet.GetDaylightSavingStartOrEndDateInYear(ZDateTime.Now.Year, RefUNLOCO.TimeZoneSet.DaylightSavingZone.EndDateRules);
				}
				return result;
			}
		}

		public ZShort GMT
		{
			get
			{
				ZShort result = ZShort.Zero;

				if (RefUNLOCO.TimeZoneSet != null && RefUNLOCO.TimeZoneSet.StandardZone != null)
				{
					result = RefUNLOCO.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC;
				}

				return result;
			}
		}

		public ZBool HasAirport
		{
			get { return RefUNLOCO.RL_HasAirport; }
		}

		public ZBool HasBorderCrossing
		{
			get { return RefUNLOCO.RL_HasBorderCrossing; }
		}

		public ZBool HasCustomsLodge
		{
			get { return RefUNLOCO.RL_HasCustomsLodge; }
		}

		public ZBool HasDaylightSaving
		{
			get
			{
				ZBool result = false;
				if (RefUNLOCO.TimeZoneSet != null)
				{
					result = RefUNLOCO.TimeZoneSet.HasDaylightSavings;
				}
				return result;
			}
		}

		public ZBool HasDischarge
		{
			get { return RefUNLOCO.RL_HasDischarge; }
		}

		public ZBool HasOutport
		{
			get { return RefUNLOCO.RL_HasOutport; }
		}

		public ZBool HasPost
		{
			get { return RefUNLOCO.RL_HasPost; }
		}

		public ZBool HasRail
		{
			get { return RefUNLOCO.RL_HasRail; }
		}

		public ZBool HasRoad
		{
			get { return RefUNLOCO.RL_HasRoad; }
		}

		public ZBool HasSeaport
		{
			get { return RefUNLOCO.RL_HasSeaport; }
		}

		public ZBool HasStore
		{
			get { return RefUNLOCO.RL_HasStore; }
		}

		public ZBool HasTerminal
		{
			get { return RefUNLOCO.RL_HasTerminal; }
		}

		public ZBool HasUnload
		{
			get { return RefUNLOCO.RL_HasUnload; }
		}

		public ZString IATA
		{
			get { return RefUNLOCO.RL_IATA; }
		}

		public ZBool IsActive
		{
			get { return RefUNLOCO.RL_IsActive; }
		}

		public ZBool IsSystem
		{
			get { return RefUNLOCO.RL_IsSystem; }
		}

		public ZString NameWithDiacriticals
		{
			get { return RefUNLOCO.RL_NameWithDiacriticals; }
		}

		public DocCountry CountryObj
		{
			get
			{
				if (fCountryObj == null || fCountryObj.Code != RefUNLOCO.RL_RN_NKCountryCode)
				{
					fCountryObj = DocCountry.New(RefUNLOCO.Country, Factory);
				}

				return fCountryObj;
			}
		}

		public ZString CountryStateCode
		{
			get { return RefUNLOCO.CountryStates != null ? RefUNLOCO.CountryStates.RW_Code : ZString.Empty; }
		}

		public ZString CountryStateDesc
		{
			get { return RefUNLOCO.CountryStates != null ? RefUNLOCO.CountryStates.RW_DescriptionMultilingual : ZString.Empty; }
		}

		public ZString State
		{
			get { return CountryStateDesc; }
		}

		public override string ToString()
		{
			return Code;
		}

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}

		#region Implementation

		protected DocCountry fCountryObj;

		internal RefUNLOCO RefUNLOCO
		{
			get { return (RefUNLOCO)WrappedObject; }
		}

		#endregion
	}
}
