using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business.FetchStrategy
{
	public class EDIMessageFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public EDIMessageFetchStrategy(EDIMessage message)
			: base(message)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			if (columns.Any(x => x.ColumnName == EDIMessage.Schema.EM_MessageInterpretation))
			{
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			}
		}

		protected override void FetchForLoadCore()
		{
			Factory.AddFetchHint(EDIInterchangeSchema.Constants.TableName, ((EDIMessage)BusinessObject).EM_EI);
			Factory.AddFetchHint(EDIMessageAttachSchema.EG_EM, BusinessObject.PK);
			base.FetchForLoadCore();
		}
	}
}
