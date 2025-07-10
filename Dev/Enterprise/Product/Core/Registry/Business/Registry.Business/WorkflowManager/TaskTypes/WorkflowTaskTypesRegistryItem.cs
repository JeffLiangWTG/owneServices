using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class WorkflowTaskTypesRegistryItem : TranslatableRegistryItem<CategorisedWorkflowTaskTypesCollection, CategorisedWorkflowTaskTypesCollection>
	{
		public WorkflowTaskTypesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new CategorisedWorkflowTaskTypesCollection())
		{
		}

		public WorkflowTaskTypesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, new CategorisedWorkflowTaskTypesCollection())
		{
		}

		public WorkflowTaskTypesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CategorisedWorkflowTaskTypesCollection defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public WorkflowTaskTypesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CategorisedWorkflowTaskTypesCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WorkflowTaskTypesRegistryDataType(), storage, options, defaultValue))
		{
			this.defaultValue = defaultValue;
		}
		readonly CategorisedWorkflowTaskTypesCollection defaultValue;

		public override bool IsTranslatable
		{
			get { return true; }
		}

		public override IEnumerable<string> GetCaptions(CategorisedWorkflowTaskTypesCollection value)
		{
			return value.Cast<CategorisedWorkflowTaskTypes>()
					.SelectMany(item => item.TaskTypes)
					.Cast<WorkflowTaskType>()
					.Select(c => c.EnglishDescription.ToString())
					.Where(s => !string.IsNullOrWhiteSpace(s))
					.Distinct();
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				return defaultValue.Cast<CategorisedWorkflowTaskTypes>()
					.SelectMany(item => item.TaskTypes)
					.Cast<WorkflowTaskType>()
					.Select(c => (ResourceString)c.Description);
			}
		}

		protected override CategorisedWorkflowTaskTypesCollection Convert(CategorisedWorkflowTaskTypesCollection value)
		{
			foreach (CategorisedWorkflowTaskTypes item in value.ToArray())
			{
				foreach (WorkflowTaskType taskType in item.TaskTypes.ToArray())
				{
					taskType.Description = GetMultilingualString(taskType.EnglishDescription);
				}
			}
			return value;
		}

		public override int MaxLength
		{
			get { return 256; }
		}

		public string GetCompletionStatementTaskType(string workflowCode)
		{
			return completionStatementTaskTypes.GetOrAdd(workflowCode, code => Value.GetCompletionStatementTaskType(code));
		}

		public CodeDescriptionPairList GetTaskTypesCodePairListFromWorkflowCode(string workflowTypeCode)
		{
			return taskTypesByWorkflowTypeCache.GetOrAdd(workflowTypeCode, code =>
			{
				var workflowTaskTypeCollection = Value.GetTaskTypesFromWorkflowCode(code);
				var result = new CodeDescriptionPairList();
				foreach (WorkflowTaskType collection in workflowTaskTypeCollection)
				{
					result.AddPair(collection.Code, collection.Description);
				}
				return result;
			});
		}

		readonly ConcurrentDictionary<string, string> completionStatementTaskTypes = new ConcurrentDictionary<string, string>();
		readonly ConcurrentDictionary<string, CodeDescriptionPairList> taskTypesByWorkflowTypeCache = new ConcurrentDictionary<string, CodeDescriptionPairList>();

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			completionStatementTaskTypes.Clear();
			taskTypesByWorkflowTypeCache.Clear();
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.WorkflowManagerTaskTypesRegistryItemEditor, Enterprise.Registry.GUI")]
	public class WorkflowTaskTypesRegistryDataType : NonPersistentBusinessObjectCollectionRegistryDataType<CategorisedWorkflowTaskTypesCollection>
	{
	}
}
