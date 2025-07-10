
namespace CargoWise.NetworkVisualisation.Integration
{
	public interface IEntityRelationship
	{
		IProposedNetworkEntity From { get; set; }
		IProposedNetworkEntity To { get; set; }

		string DisplayText { get; }
	}
}