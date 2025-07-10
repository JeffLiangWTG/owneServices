using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(HTSTariffBulkChangeForm))]
	sealed class TariffBulkChangeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new HTSTariffBulkChangeForm(new CAHTSTariffBulkChange(Factory), false);

		protected override bool AllowSaveOnFormForTestHasChanges => false;
	}
}
