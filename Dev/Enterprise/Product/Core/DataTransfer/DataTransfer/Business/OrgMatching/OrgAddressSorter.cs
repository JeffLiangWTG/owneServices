using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Business
{
	class OrgAddressSorter : IOrgAddressSorter
	{
		public OrgAddressSorter(DocAddressCollection docAddresses)
		{
			if (docAddresses.IsSpecified)
			{
				foreach (DocAddress docAddress in docAddresses)
				{
					if (docAddress.IsSpecified &&
						docAddress.AddressReferenceSpecified &&
						docAddress.AddressReference.AddressSequenceRef > 0 &&
						docAddress.AddressReference.OrganisationSpecified &&
						docAddress.AddressReference.Organisation.EDICodeSpecified)
					{
						if (!references.ContainsKey(docAddress.AddressReference.Organisation.EDICode))
						{
							references[docAddress.AddressReference.Organisation.EDICode] = new StringCollectionX();
						}
						references[docAddress.AddressReference.Organisation.EDICode].Add(docAddress.AddressReference.AddressSequenceRef.ToString());
					}
				}
			}
		}

		readonly Dictionary<string, StringCollectionX> references = new Dictionary<string, StringCollectionX>();

		public StringCollectionX GetReferences(string ediCode)
		{
			return references.ContainsKey(ediCode) ? references[ediCode] : null;
		}
	}
}
