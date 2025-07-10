namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IMenuItemCollection
	{
		void Add(IMenuItemDescriptor menuItemDescriptor);

		void Refresh();

		void Clear();
	}
}