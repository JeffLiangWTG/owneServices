using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestExcludeBusinessObjectsAllHaveTestCases, Enterprise.Core.NonSerializedClass]
	class MockSchedulableFilterField : FilterField, ISchedulableFilterField
	{
		public MockSchedulableFilterField()
			: this(null)
		{
		}

		public MockSchedulableFilterField(BusinessObjectFactory factory)
			: base(factory)
		{
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

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.None; }
		}

		public override bool IsEmpty
		{
			get { throw new NotImplementedException(); }
		}

		public override object ValueAsObject
		{
			get { throw new NotImplementedException(); }
		}

		protected override string NonEmptyWhereClause()
		{
			throw new NotImplementedException();
		}

		#region ISchedulableFilterField Members

		public ReportScheduleTask LastScheduleTask
		{
			get { return lastScheduleTask; }
		}

		public bool Scheduled
		{
			get { throw new NotImplementedException(); }
		}

		public void SetScheduleTask(ReportScheduleTask value)
		{
			lastScheduleTask = value;
		}

		ReportScheduleTask lastScheduleTask;

		#endregion

		public override void SafeCopyValuesFrom(IFilter source)
		{
			throw new NotImplementedException();
		}

		public override void ClearValues()
		{
			throw new NotImplementedException();
		}
	}
}
