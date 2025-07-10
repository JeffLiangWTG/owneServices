using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class OrgFlatFileConverterForTesting : FlatFileConverter
	{
		public OrgFlatFileConverterForTesting(INotifications notifications, BusinessObjectFactory factory) : base(notifications, factory)
		{
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			Xsd.Organisation organisation = (Xsd.Organisation)valueObject;
			FlatFileDataRowCollection collection = base.MapExport(valueObject);

			FlatFileDataRow row = new FlatFileDataRow(8);
			row[OrgConstantsForTesting.Code] = organisation.EDICode;
			row[OrgConstantsForTesting.Name] = organisation.OrganisationDetails.Name;
			row[OrgConstantsForTesting.AddressLine1] = organisation.OrganisationDetails.Addresses[0].AddressLine1;
			row[OrgConstantsForTesting.AddressLine2] = organisation.OrganisationDetails.Addresses[0].AddressLine2;
			row[OrgConstantsForTesting.City] = organisation.OrganisationDetails.Addresses[0].CityOrSuburb;
			row[OrgConstantsForTesting.Postcode] = organisation.OrganisationDetails.Addresses[0].PostCode;
			row[OrgConstantsForTesting.State] = organisation.OrganisationDetails.Addresses[0].StateOrProvince;
			row[OrgConstantsForTesting.ContactName] = organisation.OrganisationDetails.Contacts[0].Name;
			collection.Add(row);

			return collection;
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			Xsd.Organisations orgs = (Xsd.Organisations)valueObject;
			orgs.Organisation = new Xsd.OrganisationCollection();

			foreach (FlatFileDataRow row in fileLines)
			{
				Xsd.Organisation organisation = orgs.Organisation.AddNew();
				organisation.EDICode = row[OrgConstantsForTesting.Code];
				organisation.OrganisationDetails = new Xsd.OrganisationDetail();
				Xsd.OrganisationDetail details = organisation.OrganisationDetails;

				details.Name = row[OrgConstantsForTesting.Name];
				details.Addresses = new Xsd.OrgAddressCollection();
				details.Addresses.AddNew();
				details.Addresses[0].AddressLine1 = row[OrgConstantsForTesting.AddressLine1];
				details.Addresses[0].AddressLine2 = row[OrgConstantsForTesting.AddressLine2];
				details.Addresses[0].CityOrSuburb = row[OrgConstantsForTesting.City];
				details.Addresses[0].PostCode = row[OrgConstantsForTesting.Postcode];
				details.Addresses[0].StateOrProvince = row[OrgConstantsForTesting.State];

				details.Contacts = new Xsd.OrgContactCollection();
				details.Contacts.AddNew();
				details.Contacts[0].Name = row[OrgConstantsForTesting.ContactName];
			}
		}
	}
}
