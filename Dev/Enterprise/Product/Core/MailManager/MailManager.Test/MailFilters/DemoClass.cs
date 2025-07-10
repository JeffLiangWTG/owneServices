using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MailManager.MailFilters;

namespace Enterprise.MailManager.Testing
{
	sealed class DemoClass
	{
		public static readonly Overridable<int> TS1Count = new Overridable<int>(0);
		public static readonly Overridable<int> TS2Count = new Overridable<int>(0);

		[MailFilter("TS1")]
		public static IMailFilter MakeTs1Filter()
		{
			TS1Count.Value++;
			return new QueryMailFilter("TS1", subject: "I am ts1");
		}

		[MailFilter("TS2")]
		public static IMailFilter MakeTs2Filter()
		{
			TS2Count.Value++;
			return new QueryMailFilter("TS2", subject: "I am ts2", subjectComparison: SQLComparisonOperator.EndsWith);
		}
	}
}
