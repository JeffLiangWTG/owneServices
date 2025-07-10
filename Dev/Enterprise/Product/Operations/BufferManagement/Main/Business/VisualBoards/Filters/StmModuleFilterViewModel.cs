using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	public class StmModuleFilterViewModel : NonPersistentBusinessObject<StmModuleFilterViewModelValidation>
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public StmModuleFilterViewModel(StmModuleFilter workflowFilter, StmModuleFilter taskFilter, string name)
			: base(workflowFilter.Factory)
		{
			WorkflowFilter = workflowFilter;
			TaskFilter = taskFilter;
			RegisterEditableChildObjects();

			WorkflowFilterPk = workflowFilter.PK;
			TaskFilterPk = taskFilter.PK;
			Name = name;
		}

		void RegisterEditableChildObjects()
		{
			RegisterEditableChildObject(WorkflowFilter);
			RegisterEditableChildObject(TaskFilter);
		}

		public StmModuleFilter WorkflowFilter { get; }
		public ZGuid WorkflowFilterPk { get; }
		public ZPropertyInfo WorkflowFilterPkInfo => GetZPropertyInfo(nameof(WorkflowFilterPk));

		public StmModuleFilter TaskFilter { get; }
		public ZGuid TaskFilterPk { get; }
		public ZPropertyInfo TaskFilterPkInfo => GetZPropertyInfo(nameof(TaskFilterPk));

		public string Name { get; }

		public override StmModuleFilterViewModelValidation GetNewValidation()
		{
			return new StmModuleFilterViewModelValidation(this);
		}
	}
}
