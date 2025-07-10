using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class Relationship : IEntityRelationship, IEntityRelationshipLayout
	{
		public IProposedNetworkEntity From { get; set; }
		public IProposedNetworkEntity To { get; set; }

		public string BackColor { get; set; }
		public bool IsVisible { get; set; }
		public ArrowAppearance Appearance { get; set; }

		public string DisplayText
		{
			get { return this.GetDisplayName(); }
		}
	}
}
