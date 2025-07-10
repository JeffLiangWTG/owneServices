using System;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class OrgAddressesExporter
	{
		public OrgAddressesExporter(OrgAddress userSelectedAddress)
		{
			if (userSelectedAddress == null)
			{
				throw new ArgumentNullException(nameof(userSelectedAddress));
			}

			this.userSelectedAddress = userSelectedAddress;
			this.Organisation = userSelectedAddress.Header;
		}

		public OrgAddressesExporter(OrgHeader organisation, IDocAddresses docAddressesParent)
		{
			if (organisation == null)
			{
				throw new ArgumentNullException(nameof(organisation));
			}

			this.Organisation = organisation;
			this.docAddressesParent = docAddressesParent;
		}

		readonly OrgAddress userSelectedAddress;
		readonly IDocAddresses docAddressesParent;
		public OrgHeader Organisation { get; private set; }

		/// <summary>
		/// Export Value in OrgHeader(Business Object) to Organization(Value Object) for Addresses
		/// When specify as Light Weight XML(SimplifiedXML == true),
		///		It should only export the address selected by user, 
		///		If the address does not specify, it should only show main address
		/// When specify as Verbose XML(SimplifiedXML != true)
		///		It should show all address that belongs to the OrgHeader
		///		Main address should be the first address
		/// </summary>
		/// <param name="organisation"></param>
		/// <param name="result"></param>
		/// <param name="errorContext"></param>
		/// <param name="context"></param>
		public void Export(Organisation result, string errorContext, IValueObjectExportContext context)
		{
			var addressHelper = new AddressValueObjectHelper(errorContext);
			var addresses = Organisation.Addresses;
			var addressResults = result.OrganisationDetails.Addresses;

			var selectedAddress = userSelectedAddress ?? Organisation.MainAddress;

			if (context == null || context.SimplifiedXML)
			{
				addressHelper.ExportToValueObjectCollection(selectedAddress, addressResults, context);
			}
			else
			{
				addressHelper.ExportToValueObjectCollection(selectedAddress, addresses, docAddressesParent, addressResults, context);
			}
		}
	}
}
