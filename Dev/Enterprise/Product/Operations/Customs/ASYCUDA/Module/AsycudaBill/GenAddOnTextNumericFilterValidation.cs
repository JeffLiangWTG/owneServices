using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class GenAddOnTextNumericFilterValidation : ModuleTextFilterValidation
	{
		readonly int noOfDecimalPlaces;
		readonly SQLComparisonOperator comparisonOperat;
		internal GenAddOnTextNumericFilterValidation(GenAddOnTextNumericFilter parent, FilterStripBusinessObject filterBusinessObject, int noOfDecimals, SQLComparisonOperator comparisonOperator)
			: base(parent)
		{
			this.filterBusinessObject = filterBusinessObject;
			noOfDecimalPlaces = noOfDecimals;
			comparisonOperat = comparisonOperator;
		}

		readonly FilterStripBusinessObject filterBusinessObject;

		protected override void CheckProperty()
		{
			if ((comparisonOperat != SpecialComparisonOperator.IsBlank && comparisonOperat != SpecialComparisonOperator.IsNotBlank))
			{
				base.CheckProperty();
				bool isValidNumber = false;
				ZString error = ResString.GetMultilingualString("51EF7DBA-249F-4C28-BE70-B8AD3390E29E", "This field requires a numerical value to be entered.");
				foreach (var filterStrip in filterBusinessObject.FilterStrips)
				{
					var strip = filterStrip as FilterStrip;
					if (strip.IsTextFilter)
					{
						var textFilter = strip.CurrentModuleFilter as ModuleTextFilter;
						if (ZDecimal.TryParse(textFilter.Property, out ZDecimal result))
						{
							if (result.DecimalPlaces <= noOfDecimalPlaces)
							{
								isValidNumber = true;
							}
							else
							{
								error = ResString.GetMultilingualString("2C630076-0441-4D12-B0EA-AE5C634BE6EE", "This field can only contain up to 2 decimal places.");
							}
						}
					}
				}
				if (!isValidNumber)
				{
					Parent.PropertyInfo.AddError(error);
				}
			}
		}
	}
}
