using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business;

public interface ICusExitReportCollection<out T> : IActiveBusinessObjectCollection<T>
	where T : CusExitReport
{
	new T this[int index] { get; }
	void MarkAsNeedingValidation();
}

public class CusExitReportCollection<T> : ActiveBusinessObjectCollection<T>, ICusExitReportCollection<T>
	where T : CusExitReport
{
	public CusExitReportCollection(BusinessObjectFactory factory, ZQuery filter)
		: base(factory, filter)
	{
	}

	public CusExitReportCollection(CusExitHeader master)
		: base(master.Factory, master, new ZQuery(), CusExitReportSchema.CER_CXH_Header)
	{
	}
}
