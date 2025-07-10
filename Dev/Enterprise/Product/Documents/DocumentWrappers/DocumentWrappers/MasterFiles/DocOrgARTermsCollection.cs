using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgARTermsCollection : DocumentWrapperCollection
	{
		public DocOrgARTermsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocOrgARTermsCollection(OrgARTermsCollection collectionSource, BusinessObjectFactory factory)
			: base(collectionSource, factory)
		{
		}

		public new DocOrgARTerms this[int index]
		{
			get
			{
				return (DocOrgARTerms)base[index];
			}
		}
	}
}
