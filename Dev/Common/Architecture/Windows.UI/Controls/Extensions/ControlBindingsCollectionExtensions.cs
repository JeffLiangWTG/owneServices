using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	public static class ControlBindingsCollectionExtensions
	{
		/// <summary>
		/// Remove a Binding object from the collection, if it exists in the collection.
		/// </summary>
		public static void RemoveBinding(this ControlBindingsCollection dataBindings, string propertyName)
		{
			Binding binding = dataBindings[propertyName];
			if (binding != null)
			{
				dataBindings.Remove(binding);
			}
		}
	}
}
