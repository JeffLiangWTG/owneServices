using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class UpdateJobOperatorUserControlTest : UpdateJobUserControlBaseTest
	{
		protected override ZUserControl GetUpdateJobUserControl()
		{
			var control = new UpdateJobOperatorUserControl();
			control.SetDataBinding(new UpdateJobOperatorActionMethodApplicator(Factory), "");

			return control;
		}

		protected override string ChildControlName => "OperatorFindBox";
	}
}
