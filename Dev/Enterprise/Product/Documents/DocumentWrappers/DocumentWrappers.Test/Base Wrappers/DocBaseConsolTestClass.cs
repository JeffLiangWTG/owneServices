using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocBaseConsolTestClass : DocBaseConsol
	{
		public DocBaseConsolTestClass(CommonConsol consol, BusinessObjectFactory factoryToWrap)
			: base(consol, factoryToWrap)
		{
		}

		public override string ToString()
		{
			return "";
		}

		public void SetFromDocumentCommonConsolTest(DocumentCommonConsol documentCommonConsol)
		{
			SetFromDocumentCommonConsol(documentCommonConsol);
		}
	}
}
