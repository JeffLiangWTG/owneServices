namespace Enterprise.DocumentEngine.Visualisation
{
	using System.Collections.Generic;

	public interface IVisualiserDrawer
	{
		void Draw(IEnumerable<VisualiserComponent> components);
	}
}