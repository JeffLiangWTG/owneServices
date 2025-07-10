using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.TransitWarehouse
{
	public interface ITransitWarehouseParent : IBusiness
	{
		ZGuid PK { get; }
		string JobNumber { get; }
		ZString ConsignorCompanyName { get; }
		ZString ConsigneeCompanyName { get; }

		void AttachPackages(ITransitPackage[] packagesToAttach);
		void RemovePackages(ITransitPackage[] packagesToRemove);
		void SetTransportCompany(ZGuid tranpsportCompanyAddressPK);
		ZGuid GetPickupCFSOrgAddressPK();
		SortedList<int, ZGuid> GetOrderedCFSOrgAddressPKs();
		ZString GetPreAttachingValidationMessage(ITransitPackage[] packagesToAttach);
		ZBool CanAttachPackages(out ZString errorMessage);
		ZBool CanDetachPackages(out ZString errorMessage);
	}
}
