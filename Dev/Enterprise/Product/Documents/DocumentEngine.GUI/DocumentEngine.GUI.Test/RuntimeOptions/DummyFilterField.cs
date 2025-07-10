using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed partial class FilterFieldNotificationTestCase
	{
		class DummyFilterField : FilterField
		{
			internal DummyFilterField(BusinessObjectFactory factory)
				: base(factory)
			{ }

			public override void FillFilterData(ReportFilterData reportFilterData)
			{
				//Do nothing
			}

			public override void SetFilterValue(ReportFilterData reportFilterData)
			{
				//Do nothing
			}

			public override FilterFieldSuggestedUserControlType SuggestedUserControlType
			{
				get { throw new NotImplementedException(); }
			}

			public override bool IsEmpty
			{
				get { throw new NotImplementedException(); }
			}

			public override void SafeCopyValuesFrom(IFilter source)
			{
				throw new NotImplementedException();
			}

			public override void ClearValues()
			{
				throw new NotImplementedException();
			}

			public override object ValueAsObject
			{
				get { throw new NotImplementedException(); }
			}

			protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
			{
				return true;
			}

			protected override string NonEmptyWhereClause()
			{
				throw new NotImplementedException();
			}
		}
	}
}
