using System;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Default implementation of IControlExtension.
	/// <see>CargoWise.Windows.UI.IControlExtension</see>
	/// </summary>
	public abstract class ControlExtension : IControlExtension
	{
		public IExtendedControl Owner { get; private set; }

		public virtual void Initialize(IExtendedControl owner)
		{
			Argument.NotNull(owner, "owner");
			if (this.Owner != null)
			{
				throw new ArgumentException("The extension has already been initialized");
			}
			this.Owner = owner;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		public virtual void Dispose()
		{
			Owner = null;
		}
	}
}
