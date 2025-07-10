using System.Windows.Forms;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI.Testing
{
	[TestedType(typeof(ReopenPeriodKeyGeneratorForm))]
	internal sealed class ReopenPeriodKeyGeneratorFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			EDIOrgHeader header = Factory.NewWithValidTestData<EDIOrgHeader>();
			header.OH_Code = "SOUTHPARK";
			header.CreateAndLoadLicenceForOrg();
			header.Factory.Save();

			ReopenPeriodKeyBusinessObject reopenPeriodKeyBusinessObject = new ReopenPeriodKeyBusinessObject(header);
			return new ReopenPeriodKeyGeneratorForm(reopenPeriodKeyBusinessObject);
		}

		#endregion
	}
}
