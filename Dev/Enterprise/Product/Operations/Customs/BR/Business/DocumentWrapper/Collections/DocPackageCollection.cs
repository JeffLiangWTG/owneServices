using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.BR.Business
{
	public class DocPackageCollection : DocumentWrapperCollection
	{
		public DocPackageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocPackageCollection(IEnumerable<BasePackage> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocPackage this[int index]
		{
			get { return (DocPackage)base[index]; }
		}
	}
}
