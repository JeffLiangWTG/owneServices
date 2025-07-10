using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIFilteredContactsCollection : FilteredContactsCollection
	{
		public EDIFilteredContactsCollection(OrgContactDependentCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		public EDIFilteredContactsCollection(OrgContactCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		public new EDIOrgContact this[int index] => (EDIOrgContact)base[index];
	}
}
