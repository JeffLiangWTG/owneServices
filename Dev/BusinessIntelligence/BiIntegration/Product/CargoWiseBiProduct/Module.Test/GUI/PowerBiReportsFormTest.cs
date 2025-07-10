using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Module.GUI.Testing
{
	[TestedType(typeof(PowerBiReportsForm))]
	public class PowerBiReportsFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new PowerBiReportsForm();
		}

		#endregion
	}
}
