using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class UpdateJobStatusUserControlTest : UpdateJobUserControlBaseTest
	{
		protected override ZUserControl GetUpdateJobUserControl()
		{
			var control = new UpdateJobStatusUserControl();
			control.SetDataBinding(new UpdateJobStatusActionMethodApplicator(Factory), "");

			return control;
		}

		protected override string ChildControlName => "StatusCodeDropEdit";
	}
}
