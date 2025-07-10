using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.CommissionManagement.GUI
{
	public class CommissionFinalizerResultCountMessage : ResultCountMessage
	{
		public CommissionFinalizerResultCountMessage(IResultCountHandler resultCountHandler) : base(resultCountHandler, OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids.Value, OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids.Value)
		{
		}

		protected override string GetTooManyResultsErrorMessage(int numberResults)
		{
			IRegistryItemInternals registryItem = OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids;
			return Res.GetString("16F5AC14-2AD2-470C-8911-D8DC8EF94BDE", "This search returns more than the maximum number of records to display.\r\nThe number of search records to display can be defined in the registry up to a maximum value of 40,000 records.\r\n\r\nSee: Registry -> {0}\r\n\r\nThe current value is set to {1:G}.", registryItem.Location, registryItem.Value);
		}
	}
}
