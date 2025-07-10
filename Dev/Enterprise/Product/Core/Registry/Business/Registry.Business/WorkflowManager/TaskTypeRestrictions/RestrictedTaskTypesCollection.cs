using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RestrictedTaskTypesCollection : RegistryBusinessObjectCollection
	{
		public RestrictedTaskTypesCollection()
		{
		}

		public RestrictedTaskTypesCollection(TaskTypeRestrictions parent)
		{
			Parent = parent;
		}

		#region Parent

		TaskTypeRestrictions Parent { get; set; }

		#endregion

		#region RegistryBusinessObjectCollection Overrides

		public new RestrictedTaskTypes this[int i]
		{
			get { return (RestrictedTaskTypes)base[i]; }
		}

		public new RestrictedTaskTypes AddNew()
		{
			var taskType = (RestrictedTaskTypes)base.AddNew();
			SetTaskTypesWorkflowType(taskType);

			return taskType;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var taskType = new RestrictedTaskTypes();
			SetTaskTypesWorkflowType(taskType);

			return taskType;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RestrictedTaskTypesCollection(Parent);
		}

		protected override bool IgnoreCaseInCodes => true;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var taskType = bizOAdded as RestrictedTaskTypes;
			if (taskType != null)
			{
				SetTaskTypesWorkflowType(taskType);
			}
		}

		#endregion

		#region SetTaskTypesWorkflowType

		void SetTaskTypesWorkflowType(RestrictedTaskTypes taskType)
		{
			taskType.WorkflowType = Parent != null ? Parent.WorkflowType : this.Count > 0 ? this[0].WorkflowType : ZString.Empty;
		}

		#endregion
	}
}
