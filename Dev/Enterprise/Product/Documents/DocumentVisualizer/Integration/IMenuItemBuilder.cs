namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IMenuItemBuilder
	{
		IMenuItemDescriptor Build(string commandID);
		IMenuItemDescriptor Build(string caption, object image, IMenuItemDescriptor[] subMenuItems);
	}
}
