using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.Messaging.Module
{
	public class EDIInterchangeTextFilterValidation : ModuleTextFilterValidation
	{
		internal EDIInterchangeTextFilterValidation(EDIInterchangeTextFilter parent, FilterStripBusinessObject filterBusinessObject)
			: base(parent)
		{
			this.filterBusinessObject = filterBusinessObject;
		}

		readonly FilterStripBusinessObject filterBusinessObject;

		protected override void CheckProperty()
		{
			base.CheckProperty();
			bool hasValidDateFilter = false;
			foreach (var filterStrip in filterBusinessObject.FilterStrips)
			{
				var strip = filterStrip as FilterStrip;
				if (strip.IsDateFilter)
				{
					var dateFilter = strip.CurrentModuleFilter as ModuleDateFilter;
					if (dateFilter.GetDaysDifferenceBetweenToAndFromDates <= 7 && dateFilter.GetDaysDifferenceBetweenToAndFromDates >= -7)
					{
						hasValidDateFilter = true;
					}
				}
			}
			if (!hasValidDateFilter)
			{
				Parent.PropertyInfo.AddError(ResString.GetMultilingualString("4E1CC00B-D839-4F6C-A47B-D12099453CBF", "Text based filters are costly to run on SQL Servers. To avoid having a query that times out before completion, please add a Date based filter with a range of 7 days or less. (E.g.: Created Time (UTC))"));
			}
		}
	}
}
