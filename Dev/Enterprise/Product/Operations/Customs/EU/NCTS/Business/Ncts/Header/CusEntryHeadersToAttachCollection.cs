using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusEntryHeadersToAttachCollection : BusinessObjectCollection<CusEntryHeader>
	{
		public CusEntryHeadersToAttachCollection(NctsHeader nctsHeader)
			: base(nctsHeader.Factory)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(NctsHeader));
		}

		readonly NctsHeader nctsHeader;

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var subQuery1 = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			var subQuery2 = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_GC, JobDeclarationSchema.JE_GC);
			subQuery2.AddToFilter(GlbBranchSchema.PK, nctsHeader.BH_GB);
			subQuery1.AddSubQuery(subQuery2, JoinCondition.And);
			query.AddSubQuery(subQuery1, JoinCondition.And);

			return query;
		}
	}
}
