using System.ComponentModel;

namespace CargoWise.EntityFramework
{
	public delegate void ValidationRequestedEventHandler(object sender, ValidationRequestedEventArgs e);

	public class ValidationRequestedEventArgs : CancelEventArgs
	{
		public ValidationRequestedEventArgs(ZPropertyInfo property)
		{
			this.Property = property;
		}

		public ZPropertyInfo Property { get; private set; }
	}
}
