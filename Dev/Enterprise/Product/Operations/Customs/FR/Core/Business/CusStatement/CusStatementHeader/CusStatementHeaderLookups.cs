using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class CusStatementHeaderLookups : Customs.Business.CusStatementHeaderLookups
	{
		public CusStatementHeaderLookups(CusStatementHeader parent) : base(parent)
		{
		}

		public new CusStatementHeader Parent => (CusStatementHeader)base.Parent;

		public CodeDescriptionPairList StatusList => Factory.GetCachedValue<StatementStatusList>();

		public CodeDescriptionPairList Profiles => Factory.GetCachedValue("FR.CusStatementHeaderLookups." + Parent.B2_BranchDesignation + "." + Parent.B2_OH_Importer, () =>
		{
			var list = new CodeDescriptionPairList();
			if (Parent.RelatedCusAccount is OrgCusAccount account && !account.CZ_Account.IsEmpty)
			{
				list.AddPair(account.CZ_Account);
			}
			return list;
		});

		public CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue<MethodOfPaymentList>();

		public CodeDescriptionPairList PeriodicityList => Factory.GetCachedValue("FR.CusStatementHeaderLookups." + Parent.B2_StatementType + "." + Parent.B2_StatementTypeInfo.ReadOnly, () =>
		{
			var list = Factory.GetCachedValue<StatementPeriodicityList>();

			if (!Parent.B2_StatementTypeInfo.ReadOnly)
			{
				var outputList = new StatementPeriodicityList();
				outputList.RemoveCode(StatementPeriodicityList.Codes.Unknown);
				list = outputList;
			}
			return list;
		});

		public CodeDescriptionPairList DirectionList => Factory.GetCachedValue<StatementEntryTypeImpExpList>();
	}
}
