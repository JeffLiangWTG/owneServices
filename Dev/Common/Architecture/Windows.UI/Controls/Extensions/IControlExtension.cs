using System;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implement to add a generic capability to controls.
	/// </summary>
	public interface IControlExtension : IDisposable
	{
		IExtendedControl Owner { get; }
		void Initialize(IExtendedControl owner);
	}
}
