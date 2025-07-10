using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrganisationCollection : DocumentWrapperCollection
	{
		public DocOrganisationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocOrganisationCollection(IEnumerable<OrgHeader> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public DocOrganisationCollection(IEnumerable<JobDocAddress> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocOrganisation this[int index]
		{
			get { return (DocOrganisation)base[index]; }
		}
	}
}

