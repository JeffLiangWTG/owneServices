using System.Windows.Forms;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(SendNewSystemShutdownDateForm))]
	internal sealed class SendNewSystemShutdownDateFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var systemShutdownDate = new SystemShutdownDate(Factory.New<LicenceDatabase>());
			return new SendNewSystemShutdownDateForm(systemShutdownDate);
		}

		#endregion
	}
}
