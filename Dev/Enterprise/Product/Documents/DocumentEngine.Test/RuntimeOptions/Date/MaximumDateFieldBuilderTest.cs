using System;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class MaximumDateFieldBuilderTest : MaxMinDateFieldBuilderAbstractTest
	{
		protected override string DefaultXlsFileName => "MaximumDateFields.xls";

		protected override Type ExpectedDateFieldType => typeof(MaximumDateField);

		protected override string XlsFileWithDefault => "MaximumDateFieldsWithDefault.xls";

		protected override ZDateTime GetValueOfFilter(FilterCollectionBuilder testFilterCollectionBuilder, int index)
		{
			return ((MaximumDateField)testFilterCollectionBuilder.IFilterCollection[index]).Value;
		}
	}
}
