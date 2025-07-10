using System.Linq;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using OrganisationTypes = Enterprise.MasterFiles.Integration.OrganisationTypes;

namespace Enterprise.DataTransfer.Business
{
	class OrganisationValueObjectDataAdapterForOrgMatching : OrganisationValueObjectDataAdapter
	{
		public OrganisationValueObjectDataAdapterForOrgMatching(OrganisationTypes orgTypes)
			: this(orgTypes, null)
		{
		}

		public OrganisationValueObjectDataAdapterForOrgMatching(OrganisationTypes orgTypes, IOrgAddressSorter sorter)
			: base(orgTypes)
		{
			this.sorter = sorter;
		}

		readonly IOrgAddressSorter sorter;

		protected override void ImportAddresses(Xml.XsdVersion1.OrgAddressCollection address, OrgHeader organisation, IValueObjectImportContext context, ZString errorContext)
		{
			ImportAddresses(address, organisation, context, errorContext, sorter);
		}

		public static void ImportAddresses(Xml.XsdVersion1.OrgAddressCollection address, IOrgHeaderForMatching organisation, IValueObjectImportContext context, ZString errorContext, IOrgAddressSorter sorter)
		{
			var addressHelper = new AddressValueObjectHelper(errorContext);
			var docAddr = new Xml.XsdVersion1.OrgAddressCollection();

			System.Collections.IList references = sorter != null ? sorter.GetReferences(organisation.OH_Code) : null;
			if (references != null && references.Count > 0)
			{
				foreach (Xml.XsdVersion1.OrgAddress addr in address)
				{
					var capMain = addr.AddressCapabilities.IsSpecified &&
						addr.AddressCapabilities.Cast<AddressCapability>().Any(cap => cap.AddressTypeSpecified && cap.AddressType == AddressCapabilityAddressType.MAIN);

					if (capMain || references.Contains(addr.Sequence.ToString()))
					{
						docAddr.Add(addr);
					}
				}
			}
			else
			{
				docAddr = address;
			}
			addressHelper.ImportFromValueObjectCollection(docAddr, organisation, context);
		}
	}
}
