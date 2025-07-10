namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkEntityWithChildren : INetworkEntity
	{
		bool CanHaveChildren { get; }

		IObservableReloadableCollection<IProposedNetworkEntity> HiddenEntities { get; }
		IObservableReloadableCollection<IEntityRelationship> HiddenRelationships { get; }
	}
}