using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Core.Forms.Testing
{
	[TestedType(typeof(LicenceErrorForm))]
	sealed class LicenceErrorFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new LicenceErrorForm("Some test module", "Some error text");
		}

		#endregion
	}
}
