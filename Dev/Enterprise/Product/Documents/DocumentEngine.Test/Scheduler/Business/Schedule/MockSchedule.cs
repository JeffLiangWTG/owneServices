using System;
using System.Text;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	public class MockSchedule : Schedule
	{
		public MockSchedule()
		{
		}

		public string DescriptionStart
		{
			get { return descriptionStart; }
			set { descriptionStart = value; }
		}

		internal override string PeriodDescription
		{
			get { return "xxx"; }
		}

		protected override void AppendDescriptionStart(StringBuilder builder)
		{
			base.AppendDescriptionStart(builder);
			builder.Append(DescriptionStart);
		}

		protected override DateTime ToStorageValueCore(DateTime periodAdjustedValue)
		{
			return periodAdjustedValue.AddSeconds(30);
		}

		string descriptionStart;
	}
}
