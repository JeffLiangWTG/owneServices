using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class UpdateJobOverseasAgentUserControlTest : UpdateJobUserControlBaseTest
	{
		protected override ZUserControl GetUpdateJobUserControl()
		{
			var control = new UpdateJobOverseasAgentUserControl();
			control.SetDataBinding(new UpdateJobOverseasAgentActionMethodApplicator(Factory), "");

			return control;
		}

		protected override string ChildControlName => "OverseasAgentControl";
	}
}
