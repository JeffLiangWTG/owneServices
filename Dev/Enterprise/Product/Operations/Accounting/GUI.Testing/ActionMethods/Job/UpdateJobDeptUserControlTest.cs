using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class UpdateJobDeptUserControlTest : UpdateJobUserControlBaseTest
	{
		protected override ZUserControl GetUpdateJobUserControl()
		{
			var control = new UpdateJobDeptUserControl();
			control.SetDataBinding(new UpdateJobDeptActionMethodApplicator(Factory), "");

			return control;
		}

		protected override string ChildControlName => "DepartmentFindBox";
	}
}
