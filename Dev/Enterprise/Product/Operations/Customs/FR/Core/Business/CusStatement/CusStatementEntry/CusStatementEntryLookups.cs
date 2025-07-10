using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class CusStatementEntryLookups : Customs.Business.CusStatementLineLookups
	{
		public CusStatementEntryLookups(CusStatementEntry parent) : base(parent)
		{
		}

		public new CusStatementEntry Parent => (CusStatementEntry)base.Parent;

		public CodeDescriptionPairList EntryTypeList => Factory.GetCachedValue<StatementEntryTypeList>();
	}
}
