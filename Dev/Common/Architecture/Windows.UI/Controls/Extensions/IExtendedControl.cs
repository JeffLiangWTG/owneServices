using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implement on a Control class to enable IControlExtension capabilities.
	/// </summary>
	public interface IExtendedControl
	{
		Control Host { get; }
		IControlExtensionCollection Extensions { get; }
	}
}
