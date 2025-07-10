using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class DocDefermentAccountCollection : DocumentWrapperCollection<DocDefermentAccount>
	{
		public DocDefermentAccountCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocDefermentAccountCollection(IEnumerable<IDefermentAccount> defermentAccounts, BusinessObjectFactory factoryToWrap)
			: base(defermentAccounts, factoryToWrap)
		{
		}
	}
}

