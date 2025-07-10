using System.Windows.Forms;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI
{
	[TestedType(typeof(ImportDirectoryForm))]
	public class ImportDirectoryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ImportDirectoryForm(new FileImporter(new DocumentFactoryProvider().GetFactory(Factory), true));
		}
	}
}
