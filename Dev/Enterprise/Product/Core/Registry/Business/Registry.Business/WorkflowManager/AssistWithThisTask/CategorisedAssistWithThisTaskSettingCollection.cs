using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CategorisedAssistWithThisTaskSettingCollection : CategorisedWorkflowRelatedItemsCollection<CategorisedAssistWithThisTaskSetting>
	{
		public CategorisedAssistWithThisTaskSettingCollection()
		{
		}

		public CategorisedAssistWithThisTaskSettingCollection(bool initialiseWithWorkflowDescriptorList)
			: base(initialiseWithWorkflowDescriptorList)
		{
		}

		public static CategorisedAssistWithThisTaskSettingCollection GetDefault() => new CategorisedAssistWithThisTaskSettingCollection(initialiseWithWorkflowDescriptorList: true);

		public AssistWithThisTaskSettingForOneWorkflowType GetTaskDetailsForWorkflowType(string workflowType)
		{
			var parent = (CategorisedAssistWithThisTaskSetting)FindByCode(workflowType);

			return parent?.Setting;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new CategorisedAssistWithThisTaskSettingCollection();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CategorisedAssistWithThisTaskSetting();
	}
}
