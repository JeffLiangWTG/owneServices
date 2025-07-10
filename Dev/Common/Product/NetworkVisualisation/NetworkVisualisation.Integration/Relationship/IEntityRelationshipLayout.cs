
namespace CargoWise.NetworkVisualisation.Integration
{
	public interface IEntityRelationshipLayout : IEntityRelationship
	{
		string BackColor { get; }
		bool IsVisible { get; }
		ArrowAppearance Appearance { get; }
	}
}