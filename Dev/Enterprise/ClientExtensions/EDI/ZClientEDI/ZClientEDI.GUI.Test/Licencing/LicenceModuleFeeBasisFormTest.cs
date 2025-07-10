using System.Windows.Forms;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(LicenceModuleFeeBasisForm))]
	internal sealed class LicenceModuleFeeBasisFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new LicenceModuleFeeBasisForm(new LicenceModuleFeeBasis());
		}

		#endregion
	}
}
