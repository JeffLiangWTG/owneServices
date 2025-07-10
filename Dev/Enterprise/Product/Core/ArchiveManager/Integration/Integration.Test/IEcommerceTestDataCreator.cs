using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ArchiveManager.Integration.Test
{
	public interface IEcommerceTestDataCreator
	{
		List<BusinessObject> CreateEcommerceTestData(BusinessObjectFactory factory, bool isActiveProcess = true);
		List<BusinessObject> CreateEcommerceWithCustomsTestData(BusinessObjectFactory factory, bool isActiveProcess = true);
	}
}
