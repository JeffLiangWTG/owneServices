using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class UpdateJobLocalClientUserControlTest : UpdateJobUserControlBaseTest
	{
		protected override ZUserControl GetUpdateJobUserControl()
		{
			var control = new UpdateJobLocalClientUserControl();
			control.SetDataBinding(new UpdateJobLocalClientActionMethodApplicator(Factory), "");

			return control;
		}

		protected override string ChildControlName => "LocalClientControl";
	}
}
