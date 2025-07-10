using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class AdornerEventArgs : RoutedEventArgs
	{
		readonly FrameworkElement adorner;

		public AdornerEventArgs(RoutedEvent routedEvent, object source, FrameworkElement adorner)
			: base(routedEvent, source)
		{
			this.adorner = adorner;
		}

		public FrameworkElement Adorner
		{
			get
			{
				return adorner;
			}
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
	public delegate void AdornerEventHandler(object sender, AdornerEventArgs e);
}
