using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class UpdateJobProfitLossReasonUserControlTest : UpdateJobUserControlBaseTest
	{
		protected override ZUserControl GetUpdateJobUserControl()
		{
			var control = new UpdateJobProfitLossReasonUserControl();
			control.SetDataBinding(new UpdateJobProfitLossReasonActionMethodApplicator(Factory), "");

			return control;
		}

		protected override string ChildControlName => "ProfitLossReasonDropEdit";
	}
}
