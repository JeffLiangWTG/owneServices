using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CategorisedWorkflowTaskTypesCollection : CategorisedWorkflowRelatedItemsCollectionWithInnerCollection<CategorisedWorkflowTaskTypes, WorkflowTaskTypeCollection, WorkflowTaskType>
	{
		public CategorisedWorkflowTaskTypesCollection()
		{
		}

		public CategorisedWorkflowTaskTypesCollection(bool initialiseWithWorkflowDescriptorList)
			: base(initialiseWithWorkflowDescriptorList)
		{
			if (initialiseWithWorkflowDescriptorList)
			{
				AddSHODefaultTasks();
			}
		}

		public static CategorisedWorkflowTaskTypesCollection GetDefault() => new CategorisedWorkflowTaskTypesCollection(initialiseWithWorkflowDescriptorList: true);

		public WorkflowTaskTypeCollection GetTaskTypesFromWorkflowCode(string code) => GetOrCreateInnerCollectionFromWorkflowCode(code) ?? new WorkflowTaskTypeCollection();

		public WorkflowTaskType GetTaskType(string workflowType, string taskType) => GetElement(workflowType, taskType);

		protected override void AddDefaultElementCore(RegistryBusinessObject addedElement)
		{
			addedElement.Code = "UDF";
			addedElement.Description = ResString.GetMultilingualString("99ac496b-9836-4170-86f7-74581752919d", "Undefined - You can modify this in the System Registry, under Workflow Manager/Task Types");
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new CategorisedWorkflowTaskTypesCollection();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CategorisedWorkflowTaskTypes();

		void AddSHODefaultTasks()
		{
			const string ApprovalCode = "APP";
			var collection = GetOrCreateInnerCollectionFromWorkflowCode("SHO");
			var appType = collection.AddNew();
			appType.Code = ApprovalCode;
			appType.Description = ResString.GetMultilingualString("0985C8F6-43D2-41F6-B0BB-E640BE087AAD", "Approve");
			appType.IsApprovalTask = true;
			appType.IsActive = true;
		}

		internal string GetCompletionStatementTaskType(string code)
		{
			// Note: This function can be accessed by multiple threads, so no mutations should occur.
			if (FindByCode(code) is CategorisedWorkflowTaskTypes types)
			{
				foreach (WorkflowTaskType type in types.TaskTypes)
				{
					if (type.IsCompletionStatementTaskType)
					{
						return type.Code;
					}
				}
			}
			return string.Empty;
		}
	}
}
