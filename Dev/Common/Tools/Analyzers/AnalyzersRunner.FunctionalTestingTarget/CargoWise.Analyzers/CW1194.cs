using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1194
	{
		OrgAddressCollection collection;
		public void BadCode(BusinessObjectFactory factory, ZQuery filter)
		{
			collection = new OrgAddressCollection(factory, filter);
			// CW1194 - UnsafeBusinessObjectCollectionCreationAnalyzer
			collection.Load();
		}
	}
}
