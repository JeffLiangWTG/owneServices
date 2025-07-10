using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	public abstract class SystemDefinedOrganisation : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string IsEnabled = "IsEnabled";
			public const string Organisation = "Organisation";
		}

		#endregion

		public SystemDefinedOrganisation()
			: this(null)
		{
		}

		public SystemDefinedOrganisation(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public abstract string Code { get; }

		#region Copy Values

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((SystemDefinedOrganisation)clone).lastSavedOrganisation = LastSavedOrganisation;
		}

		#endregion

		#region Cache

		static Dictionary<string, ZGuid> OrgPKCache
		{
			get
			{
				if (orgPKCache == null)
				{
					orgPKCache = new Dictionary<string, ZGuid>();
				}
				return orgPKCache;
			}
		}

		[ThreadStatic]
		static Dictionary<string, ZGuid> orgPKCache;

#if DEBUG
		public static void ClearOrgPKCacheForTest()
		{
			orgPKCache = null;
		}
#endif

		static Dictionary<string, ZGuid> AddressPKCache
		{
			get
			{
				if (addressPKCache == null)
				{
					addressPKCache = new Dictionary<string, ZGuid>();
				}
				return addressPKCache;
			}
		}

		[ThreadStatic]
		static Dictionary<string, ZGuid> addressPKCache;

		#endregion

		#region Enabled

		public ZBool IsEnabled
		{
			get { return isEnabled; }
			set { SetNonPersistentPropertyValue<ZBool>(IsEnabledInfo, ref isEnabled, value); }
		}

		public ZPropertyInfo IsEnabledInfo
		{
			get { return GetZPropertyInfo(Schema.IsEnabled); }
		}

		ZBool isEnabled;

		#endregion

		#region Organisation

		public ZGuid Organisation
		{
			get
			{
				ZGuid result;
				if (!OrgPKCache.TryGetValue(Code, out result))
				{
					OrgPKCache[Code] = result = GetOrganisationPK();
				}
				return result;
			}
		}

		protected virtual ZGuid GetOrganisationPK()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, Code);

			BusinessObject org = (BusinessObject)CurrentFactory.LoadTop1<IOrgHeader>(filter);

			return (org != null) ? org.PK : ZGuid.Empty;
		}

		public ZGuid LastSavedOrganisation
		{
			get { return lastSavedOrganisation; }
		}

		ZGuid lastSavedOrganisation;

		#endregion

		#region Primary Office Address

		public ZGuid PrimaryOfficeAddress
		{
			get
			{
				ZGuid result;
				if (!AddressPKCache.TryGetValue(Code, out result))
				{
					ZGuid orgPK = Organisation;
					if (orgPK != ZGuid.Empty)
					{
						IOrgAddress address = CurrentFactory.LoadTop1<IOrgAddress>(GetFilterWithCapability(orgPK)) ?? CurrentFactory.LoadTop1<IOrgAddress>(GetFilterWithoutCapability(orgPK));

						if (address != null)
						{
							AddressPKCache[Code] = result = address.PK;
						}
					}
				}
				return result;
			}
		}

		ZQuery GetFilterWithCapability(ZGuid orgPK)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(ObjectFactory.GetType<IOrgAddress>());
			filter.AddToFilter(OrgAddressSchema.OA_OH, orgPK);

			ZDBOnlySubQuery capabilitySubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IOrgAddressCapability>(), OrgAddressCapabilitySchema.PZ_OA);
			capabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, ZBool.True);
			capabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, OrgConstants.AddressType.Office);
			filter.AddSubQuery(capabilitySubQuery, JoinCondition.And);

			return filter;
		}

		ZQuery GetFilterWithoutCapability(ZGuid orgPK)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(IOrgAddress));
			filter.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			return filter;
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
			writer.WriteElementString(Schema.Organisation, Organisation.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IsEnabled = new ZBool(reader.ReadElementString(Schema.IsEnabled));
			lastSavedOrganisation = new ZGuid(reader.ReadElementString(Schema.Organisation));
		}

		#endregion

		#region Test
#if DEBUG

		internal void SetOrganisation(ZGuid orgPK)
		{
			OrgPKCache[Code] = orgPK;
		}

		internal void SetLastSavedOrganisation(ZGuid orgPK)
		{
			lastSavedOrganisation = orgPK;
		}

		internal static void ClearCache()
		{
			orgPKCache = null;
			addressPKCache = null;
		}

#endif
		#endregion
	}
}
