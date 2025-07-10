using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class DailyNoticeStatementProcessTaskCollection : ProcessTaskCollection
	{
		public DailyNoticeStatementProcessTaskCollection(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
		}

		public new DailyNoticeStatementProcessTask this[int index] => (DailyNoticeStatementProcessTask)Elements[index];

		public new DailyNoticeStatementProcessTask AddNew()
		{
			return (DailyNoticeStatementProcessTask)base.AddNew();
		}
	}
}
