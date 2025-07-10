using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.Wow.Testing
{
	class WoolworthsMockJobDocsAndCartageParent : MockJobDocsAndCartageParent, IShipmentWithDocsAndCartage
	{
		public WoolworthsMockJobDocsAndCartageParent(BusinessObjectFactory factory) : base(factory)
		{
		}

		Type IDocsAndCartageParent.DocsAndCartageType
		{
			get
			{
				return typeof(WoolworthsJobDocsAndCartage);
			}
		}
	}
}
