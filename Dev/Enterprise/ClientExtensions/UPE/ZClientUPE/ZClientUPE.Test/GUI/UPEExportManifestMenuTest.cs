using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.DataImport
{
	public class UPEExportManifestMenuTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
		}

		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			new UPEExportManifestMenu();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		[ExpectNoExceptions()]
		public void TestInitialise()
		{
			using (UPEExportManifestMenu menu = new UPEExportManifestMenu())
			{
			}
		}
	}
}
