using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class CategorisedWorkflowTaskTypes : RegistryBusinessObject, ICategorisedRegistryBusinessObjectCollection
	{
		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((CategorisedWorkflowTaskTypes)clone).SetTaskTypes(TaskTypes.Clone());
		}

		public void SetTaskTypes(WorkflowTaskTypeCollection taskTypes)
		{
			UnRegisterEditableChildObject(TaskTypes);
			this.taskTypes = taskTypes;
			RegisterEditableChildObject(TaskTypes);
		}

		[ChildEditable]
		public WorkflowTaskTypeCollection TaskTypes
		{
			get
			{
				if (taskTypes == null)
				{
					taskTypes = new WorkflowTaskTypeCollection();
					RegisterEditableChildObject(taskTypes);
				}
				return taskTypes;
			}
		}

		WorkflowTaskTypeCollection taskTypes;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CategorisedWorkflowTaskTypes();
		}

		protected override int MaxDescriptionLength => 256;

		#region Xml Serialization

		protected override void ReadMoreElements(System.Xml.XmlReader reader)
		{
			base.ReadMoreElements(reader);
			SetTaskTypes((WorkflowTaskTypeCollection)CollectionSerialiser.Deserialize(reader));
		}

		protected override void WriteMoreElements(System.Xml.XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			CollectionSerialiser.Serialize(writer, TaskTypes);
		}

		ZXmlSerializer CollectionSerialiser
		{
			get { return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(WorkflowTaskTypeCollection))); }
		}

		ZXmlSerializer collectionSerialiser;

		#endregion

		#region ICollectionOfCodeDescriptionBoolCollections

		RegistryBusinessObjectCollection ICategorisedRegistryBusinessObjectCollection.InnerCollection => TaskTypes;

		#endregion

	}
}
