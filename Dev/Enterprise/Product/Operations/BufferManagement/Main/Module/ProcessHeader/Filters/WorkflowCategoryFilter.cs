using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	delegate ZQuery WorkflowCategoryQuery(ZString workflowType, ZString workflowCategory);

	public class WorkflowCategoryFilter : ModuleTextBaseFilter
	{
		public WorkflowCategoryFilter(GetList workflowTypeListDelegate)
			: base(ProcessHeader.ModuleFilterConstants.WorkflowCategory, (WorkflowCategoryQuery)GetWorkflowCategoryQuery, workflowTypeListDelegate)
		{
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|WorkflowCategory", "Workflow Category");
		}

		public static class Schema
		{
			public const string WorkflowType = "WorkflowType";
			public const string WorkflowCategory = "WorkflowCategory";
			public const string WorkflowCategoryList = "WorkflowCategoryList";
		}

		#region Properties

		public ZString WorkflowType
		{
			get => workflowType;
			set => SetNonPersistentPropertyValue(WorkflowTypeInfo, ref workflowType, value);
		}
		ZString workflowType;
		public ZPropertyInfo WorkflowTypeInfo { get => GetZPropertyInfo(Schema.WorkflowType); }

		public ZString WorkflowCategory
		{
			get => workflowCategory;
			set => SetNonPersistentPropertyValue(WorkflowCategoryInfo, ref workflowCategory, value);
		}
		ZString workflowCategory;
		public ZPropertyInfo WorkflowCategoryInfo { get => GetZPropertyInfo(Schema.WorkflowCategory); }

		public CodeDescriptionPairList WorkflowCategoryList
		{
			get
			{
				if (categoryWorkflowType != WorkflowType || workflowCategoryList == null)
				{
					categoryWorkflowType = WorkflowType;
					workflowCategoryList = CreateWorkflowCategoryList();
				}
				return workflowCategoryList;
			}
		}
		CodeDescriptionPairList workflowCategoryList;
		string categoryWorkflowType;

		CodeDescriptionPairList CreateWorkflowCategoryList()
		{
			var result = new CodeDescriptionPairList();
			var categoriesCollection = BMSRegistry.Instance.WorkflowCategories.Value;

			var categories = categoriesCollection.GetCategoriesFromWorkflowCode(WorkflowType);
			if (categories != null)
			{
				foreach (WorkflowCategory category in categories)
				{
					result.AddPair(category.Code, category.Description);
				}
			}

			return result;
		}

		static ZQuery GetWorkflowCategoryQuery(ZString workflowType, ZString workflowCategory)
		{
			var query = new ZQuery(ProcessHeaderSchema.FH_WorkflowType, workflowType);
			query.AddToFilter(ProcessHeaderSchema.FH_Category, workflowCategory);
			return query;
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(Schema.WorkflowType, WorkflowType);
			writer.WriteElementString(Schema.WorkflowCategory, WorkflowCategory);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			WorkflowType = reader.ReadElementString(Schema.WorkflowType);
			WorkflowCategory = reader.ReadElementString(Schema.WorkflowCategory);
		}

		#endregion

		#region Implementation

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return null;
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { WorkflowType, WorkflowCategory }; }
		}

		protected override bool IsEmptyCore => WorkflowType.IsEmpty || WorkflowCategory.IsEmpty;

		#endregion

		#region Validation

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new WorkflowCategoryFilterValidation(this);
		}

		class WorkflowCategoryFilterValidation : ModuleTextFilterValidation
		{
			public WorkflowCategoryFilterValidation(WorkflowCategoryFilter parent)
				: base(parent)
			{
				this.Parent = parent;
			}

			protected readonly new WorkflowCategoryFilter Parent;

			public void ValidateWorkflowType()
			{
				ValidateCalculatedProperty(Parent.WorkflowTypeInfo);
			}

			protected virtual void CheckWorkflowType()
			{
				MandatoryValidation.CheckEntered(Parent.WorkflowTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.WorkflowTypeInfo, (ICodeDescriptionPairList)Parent.List);
			}

			public void ValidateWorkflowCategory()
			{
				ValidateCalculatedProperty(Parent.WorkflowCategoryInfo);
			}

			protected virtual void CheckWorkflowCategory()
			{
				MandatoryValidation.CheckEntered(Parent.WorkflowCategoryInfo);
				ListValidation.ErrorIfInvalidCode(Parent.WorkflowCategoryInfo, Parent.WorkflowCategoryList);
			}

			public override void ValidateAll()
			{
				ValidateWorkflowType();
				ValidateWorkflowCategory();
				base.ValidateAll();
			}
		}

		#endregion
	}
}
