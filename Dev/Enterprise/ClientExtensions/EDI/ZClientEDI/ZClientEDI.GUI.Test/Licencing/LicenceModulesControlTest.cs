using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI.Testing
{
	public class LicenceModulesControlTest : TestCaseWithFactory
	{
		public void TestUpdateControlsShouldNotAccessDeletedObjects()
		{
			using (var licModulesControl = new LicenceModulesControl())
			{
				var licHeader = Factory.NewWithValidTestData<LicenceHeader>();

				licHeader.Delete();
				licModulesControl.SetDataBinding(licHeader, string.Empty);

				AssertEquals("Should not try to access properties of LicenceHeader", 0, ErrorReporter.TotalErrorCount);
			}
		}
	}
}