using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsGuaranteeUnderOrgHeaderCollection : CusBondDetailCollection
	{
		public NctsGuaranteeUnderOrgHeaderCollection(OrgHeader organisation)
			: base(organisation)
		{
		}

		public new NctsGuarantee this[int index]
		{
			get { return (NctsGuarantee)base[index]; }
		}

		public new NctsGuarantee AddNew()
		{
			return (NctsGuarantee)base.AddNew();
		}

		protected override BusinessObject AddNewCore(System.Type bizOType)
		{
			return base.AddNewCore(typeof(NctsGuarantee));
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, SQLComparisonOperator.Equal, ApplicationCodeList.Codes.EuNcts);
			return query;
		}
	}
}
