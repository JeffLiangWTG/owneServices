using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageBillWrapper : DocumentWrappers.Customs.EU.TemporaryStorage.TemporaryStorageBillWrapper
{
	public static TemporaryStorageBillWrapper New(TemporaryStorageBill bill, BusinessObjectFactory factory) => new TemporaryStorageBillWrapper(bill, factory);

	TemporaryStorageBillWrapper(TemporaryStorageBill bill, BusinessObjectFactory factory) : base(bill, factory)
	{
	}

	new TemporaryStorageBill ParentBusinessObject => (TemporaryStorageBill)base.ParentBusinessObject;

	protected override ZString MrnCore => ParentBusinessObject.Mrn;
}
