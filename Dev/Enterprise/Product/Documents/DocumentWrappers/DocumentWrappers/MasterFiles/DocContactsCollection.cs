using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocContactsCollection : DocumentWrapperCollection
	{
		public DocContactsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocContactsCollection(MasterFiles.Business.OrgContactDependentCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocContacts this[int index]
		{
			get { return (DocContacts)base[index]; }
		}
	}
}

