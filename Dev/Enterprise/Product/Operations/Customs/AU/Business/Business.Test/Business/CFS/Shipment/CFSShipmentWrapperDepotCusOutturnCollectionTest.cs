using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CFSShipmentWrapperDepotCusOutturnCollection))]
	public class CFSShipmentWrapperDepotCusOutturnCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CFSShipmentWrapperDepotCusOutturnCollection(Wrapper);
		}

		CFSShipmentWrapper Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					wrapper = CFSShipmentWrapper.Load(Factory.New<CFSShipment>());
				}
				return wrapper;
			}
		}
		CFSShipmentWrapper wrapper;
	}
}
