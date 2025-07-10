using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGroupRuleCategory : AutoProcessTemplateReleaseGroupRuleCategory, IProcessTemplateReleaseGroupRuleCategory
	{
		public ProcessTemplateReleaseGroupRuleCategory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject(nameof(Rule))]
		public override ZGuid PTC_PTR_Rule
		{
			get => base.PTC_PTR_Rule;
			set => base.PTC_PTR_Rule = value;
		}

		[List("Lookups.WorkflowCategories")]
		[ResourceStringData("ProcessTemplateReleaseGroupRuleCategory.PTC_Category", Caption = "Category")]
		public override ZString PTC_Category
		{
			get => base.PTC_Category;
			set => base.PTC_Category = value;
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Related Business Objects

		public ProcessTemplateReleaseGroupRule Rule => Factory.Load<ProcessTemplateReleaseGroupRule>(PTC_PTR_Rule);

		[ResourceStringData("ProcessTemplateReleaseGroupRuleCategory.CategoryDescription", Caption = "Description", FullDescription = "Category Description")]
		public ZString CategoryDescription => Lookups.WorkflowCategories.GetDescriptionFromCode(PTC_Category);

		#endregion
	}
}
