using IntegrationIGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Customs.Forwarding.GUI
{
	public interface IGridCustomColumnsProvider
	{
		void AddColumns(IntegrationIGridControl control);
	}
}
