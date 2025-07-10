using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(AUImportTCOBulkChangeForm))]
	sealed class AUImportTCOBulkChangeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new AUImportTCOBulkChangeForm(new AUImportTariffBulkChange(Factory));

		protected override bool AllowSaveOnFormForTestHasChanges => false;
	}
}
