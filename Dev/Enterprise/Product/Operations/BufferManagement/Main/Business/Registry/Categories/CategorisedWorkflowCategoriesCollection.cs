using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	[XmlSerializerAssembly("Enterprise.BufferManagement.Business.XmlSerializers")]
	public class CategorisedWorkflowCategoriesCollection : CategorisedWorkflowRelatedItemsCollectionWithInnerCollection<CategorisedWorkflowCategories, WorkflowCategoryCollection, WorkflowCategory>
	{
		public CategorisedWorkflowCategoriesCollection()
		{
		}

		public CategorisedWorkflowCategoriesCollection(bool initialiseWithWorkflowDescriptorList)
			: base(initialiseWithWorkflowDescriptorList)
		{
		}

		public static CategorisedWorkflowCategoriesCollection GetDefault() => new CategorisedWorkflowCategoriesCollection(initialiseWithWorkflowDescriptorList: true);

		protected override void AddDefaultElementCore(RegistryBusinessObject addedElement)
		{
			addedElement.Code = "UDF";
			addedElement.Description = ResString.GetMultilingualString("00f5dd70-fa15-4e31-9d91-2745159c5796",
				"Undefined - You can modify this in the System Registry, under Workflow Manager/Buffer Management/Workflow Categories");
		}

		public WorkflowCategoryCollection GetCategoriesFromWorkflowCode(string code) => GetOrCreateInnerCollectionFromWorkflowCode(code) ?? new WorkflowCategoryCollection();

		public WorkflowCategory GetCategory(string workflowType, string category) => GetElement(workflowType, category);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new CategorisedWorkflowCategoriesCollection();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CategorisedWorkflowCategories();
	}
}
