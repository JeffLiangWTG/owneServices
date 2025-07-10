using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public interface IRelatedCancellableDataSupporter
	{
		void SetIsCancelled(IBusiness parent, bool value);
		string CanCancel(IBusiness parent);
	}
}
