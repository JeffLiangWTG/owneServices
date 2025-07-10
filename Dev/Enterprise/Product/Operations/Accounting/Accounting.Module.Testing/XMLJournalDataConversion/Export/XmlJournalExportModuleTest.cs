using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(XmlJournalExportModule))]
	public class XmlJournalExportModuleTest : ZPopupModuleBasherTest
	{
		protected XmlJournalExportModule Module;

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Module = new XmlJournalExportModule();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OustandingJournalsExport;
		}

		protected override Form GetFormToBashCore(ZPopupModule module)
		{
			throw new ModuleGuiNotSupportedException("Controller uses PromptUserAndExport, so N/A for this test.");
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		#endregion

		#region TestCase

		public void TestLicenseCheckPoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
		}

		#endregion
	}
}
