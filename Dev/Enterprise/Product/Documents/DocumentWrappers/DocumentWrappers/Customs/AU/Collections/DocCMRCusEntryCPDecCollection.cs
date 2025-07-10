using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCMRCusEntryCPDecCollection : DocumentWrapperCollection
	{
		public DocCMRCusEntryCPDecCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCMRCusEntryCPDecCollection(CMRCusEntryCPDecCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCMRCusEntryCPDec this[int index]
		{
			get
			{
				return (DocCMRCusEntryCPDec)Elements[index];
			}
		}
	}
}
