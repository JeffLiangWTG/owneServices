using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class TaskTypeRestrictionsCollection : RegistryBusinessObjectCollection, ICodeDescriptionPairList
	{
		public TaskTypeRestrictionsCollection()
		{
		}

		public TaskTypeRestrictionsCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
		}

		#region IsTaskTypeUsedInTaskAssignmentRestrictions

		public bool IsTaskTypeUsedInTaskAssignmentRestrictions(ZString taskType) =>
				this.Any(r => ((TaskTypeRestrictions)r).TaskType == taskType
				|| ((TaskTypeRestrictions)r).TaskTypesCollection.Any(t => ((RestrictedTaskTypes)t).Code == taskType));

		#endregion

		#region ContainsDifRestrictions

		public bool ContainsActiveDifRestrictions => this.Select(r => (TaskTypeRestrictions)r).Any(r =>
			r.RestrictionType == RestrictionTypeList.Codes.DifferentResource && r.Active);

		#endregion

		#region Find Restrictions By TaskType

		public IEnumerable<TaskTypeRestrictions> FindRestrictionsByTaskType(ZString taskType)
		{
			return this.Cast<TaskTypeRestrictions>().Where(r =>
				r.Active
				&& (r.TaskType == taskType
				|| r.TaskTypesCollection.Any(t => ((RestrictedTaskTypes)t).Code == taskType)));
		}

		#endregion

		#region RegistryBusinessObjectCollection Overrides

		public new TaskTypeRestrictions this[int i]
		{
			get { return (TaskTypeRestrictions)base[i]; }
		}

		public new TaskTypeRestrictions AddNew()
		{
			return (TaskTypeRestrictions)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(Enterprise.ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TaskTypeRestrictionsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TaskTypeRestrictions();
		}

		protected override bool IgnoreCaseInCodes => true;

		#endregion

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			return FindByTaskType(code.ToString()) != null;
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string taskType)
		{
			var element = FindByTaskType(taskType);
			return (element != null) ? element.Description : ZString.Empty;
		}

		public RegistryBusinessObject FindByTaskType(string taskType)
		{
			return this.Cast<TaskTypeRestrictions>()
				.FirstOrDefault(x => string.Compare(x.TaskType, taskType, StringComparison.OrdinalIgnoreCase) == 0);
		}

		#endregion
	}
}
