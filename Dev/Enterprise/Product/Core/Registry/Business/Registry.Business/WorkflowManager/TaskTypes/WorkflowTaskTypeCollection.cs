using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WorkflowTaskTypeCollection : CodeDescriptionBoolCollection
	{
		public WorkflowTaskTypeCollection()
		{
		}

		public WorkflowTaskTypeCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
		}

		public WorkflowTaskTypeCollection(ReadOnlyCodeDescriptionPairList list, bool defaultBoolForNewChild)
			: base(list, defaultBoolForNewChild)
		{
		}

		public new WorkflowTaskType this[int i]
		{
			get { return (WorkflowTaskType)base[i]; }
		}

		public new WorkflowTaskType AddNew()
		{
			return (WorkflowTaskType)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WorkflowTaskType();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new WorkflowTaskTypeCollection();
		}

		protected override bool IgnoreCaseInCodes => true;

		public WorkflowTaskTypeCollection Clone()
		{
			var clone = new WorkflowTaskTypeCollection();

			foreach (WorkflowTaskType taskType in this)
			{
				var clonedTaskType = new WorkflowTaskType();
				clonedTaskType = clonedTaskType.CopyValuesToClone(taskType);
				clone.Add(clonedTaskType);
			}

			return clone;
		}
	}
}
