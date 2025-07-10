using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Filters
{
	public delegate ZQuery GetOrgWithAddressQueryDelegate(ZGuid orgPK, ZGuid addressPK);

	public class OrgWithAddressFilter : ModuleTextBaseFilter
	{
		public OrgWithAddressFilter(ZString description, GetOrgWithAddressQueryDelegate queryDelegate, bool isDebtor)
			: base(description, queryDelegate)
		{
			this.OrgWithAddressQueryDelegate = queryDelegate;
			this.IsDebtor = isDebtor;
		}

		readonly GetOrgWithAddressQueryDelegate OrgWithAddressQueryDelegate;
		readonly bool IsDebtor;

		protected OrgWithAddressFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection) { }

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new OrgWithAddressFilter(category, parentCollection);
		}

		#region Organization

		[List("Organizations")]
		public ZGuid Organization
		{
			get { return fOrganization; }
			set
			{
				if (SetNonPersistentPropertyValue(OrganizationInfo, ref fOrganization, value))
				{
					fAddresses = null;
					if (!IsValidationSuspended)
					{
						Validation.ValidateOrganization();
					}
					OrganizationInfo.RefreshBinding();
				}
			}
		}
		ZGuid fOrganization;

		public ZPropertyInfo OrganizationInfo
		{
			get { return GetZPropertyInfo(nameof(Organization)); }
		}

		public OrgHeaderCollection Organizations
		{
			get
			{
				if (fOrganizations == null)
				{
					fOrganizations = new OrgHeaderCollection(new BusinessObjectFactory(), GetAH_OHListFilter(IsDebtor));
				}
				return fOrganizations;
			}
		}
		OrgHeaderCollection fOrganizations;

		ZQuery GetAH_OHListFilter(bool isDebtor)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(isDebtor ? OrgCompanyDataSchema.OB_IsDebtor : OrgCompanyDataSchema.OB_IsCreditor, true);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Address

		[List("Addresses")]
		public ZGuid Address
		{
			get { return fAddress; }
			set
			{
				if (SetNonPersistentPropertyValue(AddressInfo, ref fAddress, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateAddress();
					}
					AddressInfo.RefreshBinding();
				}
			}
		}
		ZGuid fAddress;

		public ZPropertyInfo AddressInfo
		{
			get { return GetZPropertyInfo(nameof(Address)); }
		}

		public OrgAddressCollection Addresses
		{
			get
			{
				if (fAddresses == null)
				{
					fAddresses = new OrgAddressCollection(new BusinessObjectFactory(), new ZQuery(OrgAddressSchema.OA_OH, Organization));
					fAddresses.Load();
				}
				return fAddresses;
			}
		}

		OrgAddressCollection fAddresses;

		#endregion

		#region Validation

		public new OrgWithAddressModuleFilterValidation Validation
		{
			get { return (OrgWithAddressModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OrgWithAddressModuleFilterValidation(this);
		}

		#endregion

		#region Implementation

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);
			OrgWithAddressFilter source = ((OrgWithAddressFilter)(filterToCopyFrom));
			source.Organization = Organization;
			source.Address = Address;
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			Organization = ZGuid.Empty;
			Address = ZGuid.Empty;
		}

		protected override FilterCategory DefaultCategory
		{
			get {  return FilterCategories.Organisations; }
		}

		protected override ZQuery GetQuery()
		{
			return IsEmpty ? new ZQuery() : OrgWithAddressQueryDelegate(Organization, Address);
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && Organization.IsEmpty && Address.IsEmpty;

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, Organization, Address, Property }; }
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString("Organization", Organization.ToString());
			writer.WriteElementString("Address", Address.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			if (reader.Name == "Organization")
			{
				Organization = new Guid(reader.ReadElementString("Organization"));
			}
			if (reader.Name == "Address")
			{
				Address = new Guid(reader.ReadElementString("Address"));
			}
		}

		#endregion
	}
}
