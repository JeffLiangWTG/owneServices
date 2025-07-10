using System.Windows.Forms;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Client.TEL.Modules.TELJobConsolModule;
namespace Enterprise.Client.TEL.Modules.Testing
{
	[TestedType(typeof(TELTempImportForm))]
	class TELTempImportFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new TELTempImportForm(new DataImporterBusinessObject(Factory), null);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "ProgressTextBox";
		}
	}
}

