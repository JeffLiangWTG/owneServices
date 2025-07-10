using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLConsolidations.Testing
{
	[TestedType(typeof(GLEliminationJournalCreationForm))]
	public class GLEliminationJournalCreationFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			var adapter = new ConsolidationBatchExportAdapter(Factory, ZGuid.Empty, false);
			return new GLEliminationJournalCreationForm(adapter);
		}
	}
}
