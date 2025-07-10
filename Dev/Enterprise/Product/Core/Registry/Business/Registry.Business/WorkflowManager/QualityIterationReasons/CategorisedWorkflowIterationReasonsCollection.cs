using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CategorisedWorkflowIterationReasonsCollection : CategorisedWorkflowRelatedItemsCollectionWithInnerCollection<CategorisedWorkflowIterationReasons, WorkflowIterationReasonCollection, WorkflowIterationReason>
	{
		public CategorisedWorkflowIterationReasonsCollection()
		{
		}

		public CategorisedWorkflowIterationReasonsCollection(bool initialiseWithWorkflowDescriptorList)
			: base(initialiseWithWorkflowDescriptorList)
		{
		}

		public static CategorisedWorkflowIterationReasonsCollection GetDefault() => new CategorisedWorkflowIterationReasonsCollection(initialiseWithWorkflowDescriptorList: true);

		protected override void AddDefaultElementCore(RegistryBusinessObject addedElement)
		{
			addedElement.Code = "UDF";
			addedElement.Description = ResString.GetMultilingualString("a93d1d85-b0f1-463f-ba0a-1d5416ea6cf1", "Undefined - You can modify this in the System Registry, under Workflow Manager/Quality Iteration Reasons");
		}

		public WorkflowIterationReasonCollection GetIterationReasonsFromWorkflowCode(string code) => GetOrCreateInnerCollectionFromWorkflowCode(code) ?? new WorkflowIterationReasonCollection();

		public ZString GetIterationReasonValidationFromWorkflowCode(string code)
		{
			var parent = (CategorisedWorkflowIterationReasons)FindByCode(code);
			if (parent != null && !parent.IterationReasonValidation.IsEmpty)
			{
				return parent.IterationReasonValidation;
			}

			return IterationReasonValidationList.Codes.Error;
		}

		public WorkflowIterationReason GetIterationReason(string workflowType, string iterationReason) => GetElement(workflowType, iterationReason);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new CategorisedWorkflowIterationReasonsCollection();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CategorisedWorkflowIterationReasons();
	}
}
