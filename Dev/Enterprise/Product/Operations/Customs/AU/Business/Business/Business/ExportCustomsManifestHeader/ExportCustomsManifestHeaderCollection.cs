using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportCustomsManifestHeaderCollection : BusinessObjectCollection<ExportCustomsManifestHeader>
	{
		public ExportCustomsManifestHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(ExportCustomsManifestHeader);
		}
	}
}
