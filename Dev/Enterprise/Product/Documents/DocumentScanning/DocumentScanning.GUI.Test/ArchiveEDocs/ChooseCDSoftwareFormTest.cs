using System.IO;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	[TestedType(typeof(ChooseCDSoftwareForm))]
	sealed class ChooseCDSoftwareFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			DocumentFactory masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			ArchiveEDocsManager archiver = new ArchiveEDocsManager(masterFactory);
			return new ChooseCDSoftwareForm(new ChooseCDSoftwareManager(masterFactory, archiver));
		}

		[RequiresSTA]
		public void TestNullManagerCausedByDoubleClick()
		{
			ChooseCDSoftwareManager manager = new ChooseCDSoftwareManager(new DocumentFactoryProvider().GetFactory(Factory), new ArchiveEDocsManager(new DocumentFactoryProvider().GetFactory(Factory)));
			manager.SelectedSoftware.IsTesting = true;
			using (ChooseCDSoftwareForm form = new ChooseCDSoftwareForm(manager))
			{
				AssertNotNull(manager);
				form.OKButton_Click(null, null);
				AssertNoExceptionThrown(() => form.OKButton_Click(null, null));
				Directory.Delete(Path.Combine(Temp.TempPath, "CDArchive"), true);
			}
		}
	}
}
