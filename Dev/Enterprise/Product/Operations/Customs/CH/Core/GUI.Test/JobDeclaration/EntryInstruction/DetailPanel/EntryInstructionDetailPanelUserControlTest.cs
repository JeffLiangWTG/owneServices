using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

class EntryInstructionDetailPanelUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new EntryInstructionDetailPanelUserControl())
		{
			AssertEquals(typeof(CusEntryInstruction), control.BindingSource.DataSourceType);
		}
	}

	public void TestControls()
	{
		using (var control = new EntryInstructionDetailPanelUserControl())
		{
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("ReasonDropEdit", () => control.FindSingle<ZDropEdit>("DeclarationReasonDropEdit"));
				AssertNoExceptionThrown("PartialDeliveryCheckBox", () => control.FindSingle<ZCheckBox>("PartialDeliveryCheckBox"));
				AssertNoExceptionThrown("TransportChargesMethodOfPaymentDropEdit", () => control.FindSingle<ZDropEdit>("TransportChargesMethodOfPaymentDropEdit"));
				AssertNoExceptionThrown("ProcedureCodeDropEdit", () => control.FindSingle<ZDropEdit>("ProcedureCodeDropEdit"));
				AssertNoExceptionThrown("NextProcedureDropEdit", () => control.FindSingle<ZDropEdit>("NextProcedureDropEdit"));
			});
		}
	}
}
