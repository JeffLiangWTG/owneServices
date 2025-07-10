using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ArchiveManager.Integration.Test
{
	public interface IForwardingTestDataCreator
	{
		void CreateConsolData(int noOfShipmentsToCreate, int inputIndex, out ZGuid consolPK, bool isCancelled, out ZGuid[] shipmentPKs, out int outPutindex, bool isCFSLoadList = false);

		void CreateCartageData(ZGuid pk, out ZGuid cartagePK, bool isCancelled = false);

		void CreateCartageDataSafeDuplicate(ZGuid pk, out ZGuid cartagePK, bool isCancelled = false);

		void CreateShipmentData(out ZGuid shipmentPK);

		void CreateShipmentData(out ZGuid shipmentPK, bool isCancelled = false);

		void CreateConsolDataWithShipment(out ZGuid consolPK, out ZGuid shipmentPK, bool isCancelled = false);

		List<BusinessObject> CreateArchiveableForwardingData(BusinessObjectFactory factory, bool isActiveProcess = true);

		ZGuid CreateShipmentWithMessagesData();
	}
}
