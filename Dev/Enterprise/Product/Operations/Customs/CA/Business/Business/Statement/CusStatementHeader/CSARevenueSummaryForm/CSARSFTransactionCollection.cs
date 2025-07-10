using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class CSARSFTransactionCollection : NonPersistentBusinessObjectCollection<CSARSFTransaction>
	{
		public CSARSFTransactionCollection(CusStatementHeader cusStatementHeader)
			: base(cusStatementHeader.Factory)
		{
			this.cusStatementHeader = cusStatementHeader;
			Load();
		}

		readonly CusStatementHeader cusStatementHeader;

		public override void Load()
		{
			using (SuspendSettingHasChanges())
			{
				RemoveAll();
				var lines = cusStatementHeader.StatementLines.Cast<CusStatementLine>().Where(x => x.B3_EntryType == JobMessageTypeList.Codes.Import || x.B3_EntryType == JobMessageTypeList.Codes.XTypeEntry);
				foreach (var line in lines)
				{
					Add(new CSARSFTransaction(line));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		protected override bool AllowNewCore => false;
	}
}
