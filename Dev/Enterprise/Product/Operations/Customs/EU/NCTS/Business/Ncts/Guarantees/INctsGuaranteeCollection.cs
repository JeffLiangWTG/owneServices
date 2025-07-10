using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsGuaranteeCollection<out T> : IBusinessObjectCollection<T>
		where T : NctsGuarantee
	{
		new T this[int index] { get; }
		new T AddNew();
	}
}
