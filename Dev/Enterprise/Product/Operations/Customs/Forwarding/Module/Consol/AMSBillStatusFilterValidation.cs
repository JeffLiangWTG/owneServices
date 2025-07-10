using System.Linq;
using CargoWise.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.Customs.Forwarding.Module
{
	public class AMSBillStatusFilterValidation : ModuleTextFilterValidation
	{
		internal AMSBillStatusFilterValidation(AMSBillStatusFilter parent)
			: base(parent)
		{
		}

		protected new AMSBillStatusFilter Parent => (AMSBillStatusFilter)base.Parent;

		protected override void CheckProperty()
		{
			base.CheckProperty();
			if (!Parent.Property.IsEmpty)
			{
				var createdTimeFilter = Parent.FilterBusinessObject.FilterStrips.OfType<FilterStrip>().Where(x => x.FilterDescription == FilterDescriptions.CreatedTime).Select(x => x.CurrentModuleFilter).OfType<ModuleDateFilter>().FirstOrDefault();
				if (createdTimeFilter == null || !createdTimeFilter.IsPropertySearchValid || createdTimeFilter.Query.ParameterisedText.ParameterisedQueryText.IsNullOrEmpty())
				{
					Parent.PropertyInfo.AddError(ResString.GetMultilingualString("CDE00094-FA7B-4630-927D-2BF1F9D29288", "'AMS Bill Status' filter can be costly to run. To avoid having a query that times out before completion, please add a 'Created Time' filter with a specific range."));
				}
			}
		}
	}
}
