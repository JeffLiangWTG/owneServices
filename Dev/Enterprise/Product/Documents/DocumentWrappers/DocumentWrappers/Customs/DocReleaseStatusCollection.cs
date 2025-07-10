namespace Enterprise.DocumentWrappers.Customs
{
	using System.Collections;
	using CargoWise.EntityFramework;
	using Enterprise.DocumentEngineCore.DocWrappers;

	public class DocReleaseStatusCollection : DocumentWrapperCollection<DocReleaseStatus>
	{
		public DocReleaseStatusCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocReleaseStatusCollection(IEnumerable collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}
	}
}
