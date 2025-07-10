using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage;

public class TemporaryStorageHeaderWrapper : DocBaseWrapper
{
	public static TemporaryStorageHeaderWrapper New(TemporaryStorageHeader header, BusinessObjectFactory factoryToWrap) => new TemporaryStorageHeaderWrapper(header, factoryToWrap);

	protected TemporaryStorageHeaderWrapper(TemporaryStorageHeader header, BusinessObjectFactory factory) : base(header, factory)
	{
	}

	new TemporaryStorageHeader ParentBusinessObject => (TemporaryStorageHeader)base.ParentBusinessObject;

	public ZString JobReferenceNumber => ParentBusinessObject.AMA_JobReference;

	public ZString AuthorisationNumber => ParentBusinessObject.GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty;

	public ZString LocalClient
	{
		get
		{
			var localClient = ParentBusinessObject.InvoicingSupporter.Job?.LocalChargesAddr;
			return localClient is null
				? ZString.Empty
				: new TemporaryStorageAddressFormatter(localClient).AsString();
		}
	}

	public ZString Declarant => new TemporaryStorageAddressFormatter(ParentBusinessObject.Declarant).AsString();

	public DocBaseWrapperCollection<TemporaryStorageBillWrapper> Bills => bills ??= GetBillCollection();
	DocBaseWrapperCollection<TemporaryStorageBillWrapper> bills;

	protected virtual DocBaseWrapperCollection<TemporaryStorageBillWrapper> GetBillCollection() => new TemporaryStorageBillWrapperCollection(ParentBusinessObject.Bills.Cast<TemporaryStorageBill>(), Factory);
}
