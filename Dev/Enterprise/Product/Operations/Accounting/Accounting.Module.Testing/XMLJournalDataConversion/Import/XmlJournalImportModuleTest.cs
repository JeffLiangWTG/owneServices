using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(XmlJournalImportModule))]
	public class XmlJournalImportModuleTest : ZPopupModuleBasherTest
	{
		protected XmlJournalImportModule Module;

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Module = new XmlJournalImportModule();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OustandingJournalsImport;
		}

		protected override Form GetFormToBashCore(ZPopupModule module)
		{
			throw new ModuleGuiNotSupportedException("Controller uses PromptUserAndImport, so N/A for this test.");
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
