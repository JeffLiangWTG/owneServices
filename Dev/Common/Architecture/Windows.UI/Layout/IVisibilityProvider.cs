namespace CargoWise.Windows.UI.Layout
{
	using System;
	using System.Windows.Forms;

	public interface IVisibilityProvider
	{
		bool Visible { get; }
		event EventHandler VisibleChanged;
	}

	public interface IControlVisibilityProvider : IVisibilityProvider
	{
		Control SourceControl { get; }
	}
}
