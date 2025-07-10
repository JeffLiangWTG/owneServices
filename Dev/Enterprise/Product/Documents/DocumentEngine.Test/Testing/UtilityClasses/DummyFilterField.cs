using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestExcludeBusinessObjectsAllHaveTestCases()]
	[Enterprise.Core.NonSerializedClass]
	internal class DummyFilterField : FilterField
	{
		public DummyFilterField(bool actEmpty, string displayName, BusinessObjectFactory factory)
			: base(factory)
		{
			fActEmpty = actEmpty;
			ParameterList.Add(new SqlParameter("Some Param", ""));
			this.DisplayName = displayName;
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			//Do nothing
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			//Do nothing
		}

		public override object ValueAsObject
		{
			get { return null; }
		}

		public override bool IsEmpty
		{
			get { return fActEmpty; }
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.None; }
		}

		readonly bool fActEmpty;

		protected override string NonEmptyWhereClause()
		{
			return "non empty";
		}

		public override void SafeCopyValuesFrom(IFilter source)
		{
		}

		public override void ClearValues()
		{
		}
	}
}
