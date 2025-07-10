using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusStatementLineCollection : DependentBusinessObjectCollection<CusStatementLine, CusStatementHeader>
	{
		public CusStatementLineCollection(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
		}

		public new CusStatementHeader Master => base.Master;

		public CusStatementLine GetStatementLineFor(ZString entryNum, ZString entryType, ZString entryStatus)
		{
			var query = new ZQuery();
			query.AddToFilter(CusStatementLineSchema.B3_EntryNum, entryNum);
			query.AddToFilter(CusStatementLineSchema.B3_EntryType, entryType);
			query.AddToFilter(CusStatementLineSchema.B3_EntryStatus, entryStatus);

			return Find(query).FirstOrDefault() as CusStatementLine;
		}

		protected override bool AllowNewCore => false;
	}
}
