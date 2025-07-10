using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen
{
	[TableName(OrgHeaderSchema.Constants.TableName)]
	class MatchingOrganisation : Wrapper, IMatchingOrganisation
	{
		internal override void Init(DataRow row, WrapperFactory factory)
		{
			base.Init(row, factory);

			originalCollections = new PatternMatchingOriginalCollections()
			{
				CustomsCodes = new List<IMatchingCusCode>(CustomsCodes),
				Addresses = new List<IMatchingAddress>(Addresses),
				OrganisationNamesFromBrands = new List<OrganisationName>(OrganisationNamesFromBrands),
				OrganisationNamesExceptBrands = new List<OrganisationName>(OrganisationNamesExceptBrands),
			};
		}

		PatternMatchingOriginalCollections originalCollections;

		#region Child Lists

		List<IMatchingBrandOrRelatedName> brands;
		public List<IMatchingBrandOrRelatedName> Brands
		{
			get
			{
				return brands ?? (brands = Factory
					.Load<MatchingBrandOrRelatedName>(new ZQuery(OrgBrandOrRelatedNameSchema.P1_OH, PK))
					.Cast<IMatchingBrandOrRelatedName>()
					.ToList());
			}
		}

		List<IMatchingAddress> addresses;
		public IEnumerable<IMatchingAddress> Addresses
		{
			get
			{
				return addresses ?? (addresses = Factory
					.Load<MatchingAddress>(new ZQuery(OrgAddressSchema.OA_OH, PK))
					.Cast<IMatchingAddress>()
					.ToList());
			}
		}

		List<IMatchingCusCode> customsCodes;
		public IEnumerable<IMatchingCusCode> CustomsCodes
		{
			get
			{
				return customsCodes ?? (customsCodes = Factory
					.Load<MatchingCusCode>(new ZQuery(OrgCusCodeSchema.OK_OH, PK))
					.Cast<IMatchingCusCode>()
					.ToList());
			}
		}

		#region IEnumerable<OrganisationName> OrganisationNamesExceptBrands

		List<OrganisationName> organisationNamesExceptBrands;
		public IEnumerable<OrganisationName> OrganisationNamesExceptBrands
		{
			get { return organisationNamesExceptBrands ?? (organisationNamesExceptBrands = GetOrganisationNamesExceptBrands()); }
		}

		List<OrganisationName> GetOrganisationNamesExceptBrands()
		{
			var result = new List<OrganisationName>();
			result.Add(new OrganisationName(OH_FullName, OH_Language));

			foreach (var address in Addresses)
			{
				if (!address.OA_CompanyNameOverride.IsEmpty && address.OA_IsActive)
				{
					result.Add(new OrganisationName(address.OA_CompanyNameOverride, address.OA_Language) { OrgAddressPK = address.PK });
				}
			}

			return result;
		}

		#endregion

		#region IEnumerable<OrganisationName> OrganisationNamesFromBrands

		List<OrganisationName> organisationNamesFromBrands;
		public IEnumerable<OrganisationName> OrganisationNamesFromBrands
		{
			get { return organisationNamesFromBrands ?? (organisationNamesFromBrands = GetOrganisationNamesFromBrands()); }
		}

		List<OrganisationName> GetOrganisationNamesFromBrands()
		{
			var result = new List<OrganisationName>();
			foreach (var brandName in Brands)
			{
				result.Add(new OrganisationName(brandName.P1_RelatedName, OH_Language));
			}

			return result;
		}

		#endregion

		public PatternMatchingOriginalCollections OriginalCollections
		{
			get { return originalCollections; }
		}

		#endregion

		#region Main Address

		public IDisposable CacheMainAddress()
		{
			return null;
		}

		public MultilingualString CountryName
		{
			get { return (NoResString)Factory.GetPortInfo(OH_RL_NKClosestPort).CountryName; }
		}

		public ZString PortName
		{
			get { return Factory.GetPortInfo(OH_RL_NKClosestPort).PortName; }
		}

		#endregion

		#region Data Row Properties

		public ZString OH_Code
		{
			get { return GetValue(OrgHeaderSchema.OH_Code); }
			set { SetValue(OrgHeaderSchema.OH_Code, value); }
		}

		public ZString OH_FullName
		{
			get { return GetValue(OrgHeaderSchema.OH_FullName); }
		}

		public ZBool OH_IsActive
		{
			get { return GetValue(OrgHeaderSchema.OH_IsActive); }
		}

		public ZString OH_Language
		{
			get { return GetValue(OrgHeaderSchema.OH_Language); }
		}

		public ZString OH_RL_NKClosestPort
		{
			get { return GetValue(OrgHeaderSchema.OH_RL_NKClosestPort); }
		}

		public ZBool OH_RL_NKClosestPortInfoHasChanges
		{
			get { return OH_RL_NKClosestPort != GetOriginalValue(OrgHeaderSchema.OH_RL_NKClosestPort); }
		}

		protected override SchemaPKColumn PKSchemaColumn
		{
			get { return OrgHeaderSchema.PK; }
		}

		#endregion

		#region Requires Regen Properties

		public ZBool PatternMatchRequiresFullRegen
		{
			get { return patternMatchRequiresFullRegen; }
			set { patternMatchRequiresFullRegen = value; }
		}
		ZBool patternMatchRequiresFullRegen = true;

		public ZBool PatternMatchRequiresRegen
		{
			get { return patternMatchRequiresRegen; }
			set { patternMatchRequiresRegen = value; }
		}
		ZBool patternMatchRequiresRegen = true;

		#endregion
	}
}
