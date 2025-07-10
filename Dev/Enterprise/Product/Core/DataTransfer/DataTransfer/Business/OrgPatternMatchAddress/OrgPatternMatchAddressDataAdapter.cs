using System;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class OrgPatternMatchAddressDataAdapter : ValueObjectDataAdapter<OrgPatternMatchAddress, Xsd.Organisation>
	{
		public OrgPatternMatchAddressDataAdapter(ZGuid parentID, Type parentType, string addressType) : base()
		{
			this.ParentID = parentID;
			string parentTableName = BusinessObjectFactory.GetTableNameFromType(parentType);
			ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(parentTableName);
			if (string.IsNullOrEmpty(ParentTableCode))
			{
				Globals.Message.ShowDeveloperErrorAlways("There is no TableNamePrefix for this type", "Incorrect ParentType");
			}
			this.AddressType = addressType;
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		public override XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.SingleOrganisationSchema; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string RootCollectionElementName
		{
			get { return "Organisations"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string RootElementName
		{
			get { return "Organisation"; }
		}

		protected override void ExportToValueObjectCore(OrgPatternMatchAddress bizObj, Xsd.Organisation constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException();
		}

		protected override OrgPatternMatchAddress FindBusinessObject(Xsd.Organisation value, IValueObjectImportContext context)
		{
			ZQuery filter = new ZQuery(OrgPatternMatchAddressSchema.P3_ParentID, ParentID);
			filter.AddToFilter(OrgPatternMatchAddressSchema.P3_ParentTableCode, ParentTableCode);
			filter.AddToFilter(OrgPatternMatchAddressSchema.P3_AddressType, AddressType);
			return (OrgPatternMatchAddress)context.Factory.LoadTop1(typeof(OrgPatternMatchAddress), filter);
		}

		#region ImportFromValueObjectCore

		protected override void ImportFromValueObjectCore(OrgPatternMatchAddress orgPatternMatchAddress, Xsd.Organisation organisation, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(orgPatternMatchAddress.P3_AddressTypeInfo, AddressType);
			context.SetPropertyInfoValueIfValueNotEmpty(orgPatternMatchAddress.P3_ParentTableCodeInfo, ParentTableCode);
			orgPatternMatchAddress.P3_ParentID = ParentID;
			context.SetPropertyInfoValue(orgPatternMatchAddress.P3_CodeInfo, organisation.EDICode, organisation.EDICodeSpecified);
			context.SetPropertyInfoValue(orgPatternMatchAddress.P3_CompanyNameInfo, organisation.OrganisationDetails.Name, organisation.OrganisationDetails.NameSpecified);

			Xsd.OrgAddress address = organisation.OrganisationDetails.Addresses.GetMainAddress();
			if (address != null)
			{
				if (!address.CompanyName.IsEmpty)
				{
					context.SetPropertyInfoValue(orgPatternMatchAddress.P3_CompanyNameInfo, address.CompanyName, address.CompanyNameSpecified);
				}
				context.SetPropertyInfoValue(orgPatternMatchAddress.P3_Address1Info, address.AddressLine1, address.AddressLine1Specified);
				context.SetPropertyInfoValue(orgPatternMatchAddress.P3_Address2Info, address.AddressLine2, address.AddressLine2Specified);
				context.SetPropertyInfoValue(orgPatternMatchAddress.P3_CityInfo, address.CityOrSuburb, address.CityOrSuburbSpecified);
				context.SetPropertyInfoValue(orgPatternMatchAddress.P3_PostCodeInfo, address.PostCode, address.PostCodeSpecified);
				context.SetPropertyInfoValue(orgPatternMatchAddress.P3_StateInfo, address.StateOrProvince, address.StateOrProvinceSpecified);
				context.SetPropertyInfoValueIfValueNotEmpty(orgPatternMatchAddress.P3_PhoneInfo, address.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Business));
				context.SetPropertyInfoValueIfValueNotEmpty(orgPatternMatchAddress.P3_FaxInfo, address.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Fax));
				context.SetPropertyInfoValue(orgPatternMatchAddress.P3_EmailInfo, address.Email, address.EmailSpecified);
			}

			if (organisation.OrganisationDetails.Contacts.IsSpecified && organisation.OrganisationDetails.Contacts.Count > 0)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(orgPatternMatchAddress.P3_ContactNameInfo, organisation.OrganisationDetails.Contacts[0].Name);
			}
		}

		#endregion

		readonly ZGuid ParentID;
		readonly string ParentTableCode;
		readonly string AddressType;
	}
}
