using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business;

public interface ICusExitConsignmentCollection<out T> : IActiveBusinessObjectCollection<T>
	where T : CusExitConsignment
{
	new T this[int index] { get; }
	void MarkAsNeedingValidation();
	event CollectionCountChangedEventHandler CollectionCountChange;
}

public class CusExitConsignmentCollection<T> : ActiveBusinessObjectCollection<T>, ICusExitConsignmentCollection<T>
	where T : CusExitConsignment
{
	public CusExitConsignmentCollection(CusExitHeader master)
		: base(master.Factory, master, new ZQuery(), CusExitConsignmentSchema.CXC_CXH_Header)
	{
	}
}
