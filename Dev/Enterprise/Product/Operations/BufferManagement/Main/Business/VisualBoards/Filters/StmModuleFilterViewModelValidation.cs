using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class StmModuleFilterViewModelValidation : ZValidation
	{
		public StmModuleFilterViewModelValidation(StmModuleFilterViewModel parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly StmModuleFilterViewModel parent;

		public void ValidateWorkflowFilterPk()
		{
			ValidateCalculatedProperty(parent.WorkflowFilterPkInfo);
		}

		protected void CheckWorkflowFilterPk()
		{
			FilterValidator.ValidateFilter(parent.WorkflowFilter);
		}

		public void ValidateTaskFilterPk()
		{
			ValidateCalculatedProperty(parent.TaskFilterPkInfo);
		}

		protected void CheckTaskFilterPk()
		{
			FilterValidator.ValidateFilter(parent.TaskFilter);
		}

		public override Type AutoValidationType => typeof(StmModuleFilterViewModelValidation);

		public override void ValidateAll()
		{
			ValidateWorkflowFilterPk();
			ValidateTaskFilterPk();
		}
	}
}
