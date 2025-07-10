using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration.Management
{
	public interface IUniversalMilestoneWriter
	{
		void PopulateMilestones(BusinessObject source, IDataObject destination, bool includeInternal);
	}
}
