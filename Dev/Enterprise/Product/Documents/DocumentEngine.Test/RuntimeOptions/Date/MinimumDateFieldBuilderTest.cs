using System;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class MinimumDateFieldBuilderTest : MaxMinDateFieldBuilderAbstractTest
	{
		protected override string DefaultXlsFileName => "MinimumDateFields.xls";

		protected override Type ExpectedDateFieldType => typeof(MinimumDateField);

		protected override string XlsFileWithDefault => "MinimumDateFieldsWithDefaults.xls";

		protected override ZDateTime GetValueOfFilter(FilterCollectionBuilder testFilterCollectionBuilder, int index)
		{
			return ((MinimumDateField)(testFilterCollectionBuilder.IFilterCollection[index])).Value;
		}
	}
}
