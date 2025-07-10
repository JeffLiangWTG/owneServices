using System;

namespace CargoWise.Windows.UI
{
	interface IKControl
	{
		/// <summary>
		/// Occurs when a handle is created for the control and after CreatHandle method finishes.
		/// </summary>
		event EventHandler HandleFullyCreated;
	}
}
