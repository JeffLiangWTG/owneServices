using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ProcessHeaderTemplateTextFilter : TemplatedItemTextFilter
	{
		bool FilterApplicableToTemplates { get; }

		public ProcessHeaderTemplateTextFilter(ProcessHeaderFilterBusinessObject filterBusinessObject)
			: base(GetTemplateQuery,
				  filterBusinessObject.GetContexts<BufferManagementBusinessContext>().FirstOrDefault() == BufferManagementBusinessContext.IgnoreDefaultFilters ? FilterVisibility.Visible : FilterVisibility.AlwaysApplied)
		{
			FilterApplicableToTemplates = !(filterBusinessObject is IBMFilterRuleFilterBusinessObject &&
				((IBMFilterRuleFilterBusinessObject)filterBusinessObject).FilterControlIdentifier == "TagRule"); // internal value;
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new TemplateTextFilterValidation(this);
		}

		class TemplateTextFilterValidation : ModuleTextFilterValidation
		{
			internal TemplateTextFilterValidation(ModuleTextBaseFilter parent)
				: base(parent)
			{
			}

			protected override void CheckProperty()
			{
				base.CheckProperty();

				if (!((ProcessHeaderTemplateTextFilter)Parent).FilterApplicableToTemplates)
				{
					Parent.PropertyInfo.AddWarning(TagRuleValidation.FilterTemplateMessage);
				}
			}
		}

		static ZQuery GetTemplateQuery(ZString value)
		{
			var query = new ZQuery();

			if (value == TemplateFilterOptions.Codes.Template)
			{
				query.AddToFilter(ProcessHeaderSchema.FH_P0_Template, SQLComparisonOperator.NotEqual, null);
			}
			else if (value == TemplateFilterOptions.Codes.NonTemplate)
			{
				query.AddToFilter(ProcessHeaderSchema.FH_P0_Template, null);
			}

			return query;
		}
	}
}
