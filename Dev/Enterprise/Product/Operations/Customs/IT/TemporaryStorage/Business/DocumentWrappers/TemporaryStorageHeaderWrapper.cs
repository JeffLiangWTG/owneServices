using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
public class TemporaryStorageHeaderWrapper : DocumentWrappers.Customs.EU.TemporaryStorage.TemporaryStorageHeaderWrapper
{
	public static TemporaryStorageHeaderWrapper New(TemporaryStorageHeader header, BusinessObjectFactory factoryToWrap) => new TemporaryStorageHeaderWrapper(header, factoryToWrap);

	TemporaryStorageHeaderWrapper(TemporaryStorageHeader header, BusinessObjectFactory factory) : base(header, factory)
	{
	}

	new TemporaryStorageHeader ParentBusinessObject => (TemporaryStorageHeader)base.ParentBusinessObject;

	protected override DocBaseWrapperCollection<DocumentWrappers.Customs.EU.TemporaryStorage.TemporaryStorageBillWrapper> GetBillCollection() => new TemporaryStorageBillWrapperCollection(ParentBusinessObject.Bills.Cast<TemporaryStorageBill>(), Factory);
}
