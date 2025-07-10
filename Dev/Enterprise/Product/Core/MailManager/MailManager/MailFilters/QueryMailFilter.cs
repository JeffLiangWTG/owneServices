using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MailManager.MailFilters
{
	public class QueryMailFilter : IMailFilter
	{
#if DEBUG
		public static QueryMailFilter AllQueuedItems_ForTesting => new QueryMailFilter("ALL", subject: null);
#endif

		public QueryMailFilter(string code, string[] statuses = null, string subject = null, SQLComparisonOperator subjectComparison = null,
			string from = null, SQLComparisonOperator fromComparison = null, bool isEnabled = true, bool alsoApplyQuery = false)
			: this(code, statuses, subjects: AsArray(subject), subjectComparison: subjectComparison, from: AsArray(from), fromComparison: fromComparison, isEnabled, alsoApplyQuery)
		{
		}

		public QueryMailFilter(string code, string[] statuses = null, string[] subjects = null, SQLComparisonOperator subjectComparison = null,
			string[] from = null, SQLComparisonOperator fromComparison = null, bool isEnabled = true, bool alsoApplyQuery = false)
			: this(code, BuildQuery(statuses, subjects: subjects, subjectOp: subjectComparison, from: from, fromOp: fromComparison), statuses, isEnabled, alsoApplyQuery)
		{
		}

		public QueryMailFilter(string code, ZQuery query, string[] statuses = null, bool isEnabled = true, bool alsoApplyQuery = false)
		{
			Code = code;
			this.query = query;
			if (statuses == null || statuses.Length == 0)
			{
				statuses = new string[] { MailStatus.Queued };
			}
			this.Statuses = statuses;
			this.IsEnabled = isEnabled;
			this.AlsoApplyQuery = alsoApplyQuery;
		}

		readonly ZQuery query;

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] Statuses { get; }
		public string Code { get; }
		public bool IsEnabled { get; }
		public bool AlsoApplyQuery { get; }
		public ZQuery Query => new ZQuery(query) { OrderBy = MailDBItemsSchema.MI_ReceivedDateTime.Name };
		public virtual bool CanProcess(IMailItem item) => ((BusinessObject)item).MatchesFilter(query);

		static string[] AsArray(string item)
			=> !string.IsNullOrEmpty(item) ? new[] { item } : null;

		static ZQuery BuildSubQuery(SchemaColumn column, SQLComparisonOperator op, string[] values)
			=> values.Aggregate(new ZQuery { DefaultJoinCondition = JoinCondition.Or }, (q, s) => q.AddToFilter(column, op, s));

		protected static ZQuery BuildQuery(string[] statuses, string[] from, SQLComparisonOperator fromOp, string[] subjects, SQLComparisonOperator subjectOp)
		{
			var query = new ZQuery(MailDBItemsSchema.MI_Direction, MailDirection.Receive) { OrderBy = MailDBItemsSchema.MI_ReceivedDateTime.Name };
			if (statuses == null || statuses.Length == 0)
			{
				statuses = new string[] { MailStatus.Queued };
			}
			query.AddToFilter(MailDBItemsSchema.MI_Status, statuses);

			if (subjects?.Length > 0)
			{
				query.AddToFilter(BuildSubQuery(MailDBItemsSchema.MI_Subject, subjectOp ?? SQLComparisonOperator.Equal, subjects));
			}

			if (from?.Length > 0)
			{
				query.AddToFilter(BuildSubQuery(MailDBItemsSchema.MI_From, fromOp ?? SQLComparisonOperator.Equal, from));
			}

			return query;
		}

		public ZQuery LoadQuery(int limit = -1)
		{
			var loadQuery = new ZQuery(MailDBItemsSchema.MI_Application, Code) { OrderBy = MailDBItemsSchema.MI_ReceivedDateTime.Name, MaximumRows = limit > 0 ? limit : null };
			loadQuery.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Receive);
			loadQuery.AddToFilter(MailDBItemsSchema.MI_Status, Statuses);
			if (AlsoApplyQuery)
			{
				loadQuery.AddToFilter(query);
			}
			return loadQuery;
		}

		public IMailItem[] Load(BusinessObjectFactory factory, int limit = -1)
		{
			var loadQuery = LoadQuery(limit);
			return factory.Load<MailItem>(loadQuery);
		}
	}
}
