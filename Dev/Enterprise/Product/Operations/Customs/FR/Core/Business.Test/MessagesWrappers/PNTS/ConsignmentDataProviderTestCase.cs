using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public abstract class ConsignmentDataProviderTestCase<T> : DataProviderTestCase<T> where T : class
	{
		protected static TemporaryStoragePack CreatePack(TemporaryStorageBill bill, TemporaryStorageContainer container, int qty, string marksAndNumbers, string packUQ)
		{
			var pack1 = bill.Packs.AddNew();
			pack1.APA_PackQty = qty;
			pack1.APA_MarksAndNumbers = marksAndNumbers;
			pack1.APA_PackUQ = packUQ;
			pack1.ContainerPK = container.PK;
			return pack1;
		}

		protected static TemporaryStorageContainer CreateContainer(TemporaryStorageHeader header, string containerNumber, string emptyFullIndicator, string seal1 = null, string seal2 = null, string seal3 = null)
		{
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = containerNumber;
			container.ACN_EmptyFullIndicator = emptyFullIndicator;
			container.ACN_Seal1 = seal1;
			container.ACN_Seal2 = seal2;
			container.ACN_Seal3 = seal3;
			return container;
		}
	}
}
