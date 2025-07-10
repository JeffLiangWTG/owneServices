using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public abstract class CusTempStorageJobHeaderCollection : ActiveBusinessObjectCollection<CusTempStorageJobHeader>
{
	protected CusTempStorageJobHeaderCollection(BusinessObjectFactory factory, GlbBranch branch)
		: base(factory)
	{
		this.branch = Argument.NotNull(branch, nameof(branch));
	}
	protected readonly GlbBranch branch;

	protected override object[] GetCollectionState() => new object[] { branch };
	protected override ZQuery CreateRelationshipFilter()
	{
		var result = base.CreateRelationshipFilter();
		var cusTempStorageJobHeaderQuery = new ZDBOnlyQuery(typeof(CusTempStorageJobHeader));

		var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), CusTempStorageJobHeaderSchema.SJH_GB);
		branchQuery.AddToFilter(GlbBranchSchema.GB_GC, branch.Company.PK);
		cusTempStorageJobHeaderQuery.AddSubQuery(branchQuery, JoinCondition.And);

		result.AddToFilter(cusTempStorageJobHeaderQuery);
		return result;
	}

	protected override void SetRelationshipDefaultsForElementCore(CusTempStorageJobHeader jobHeader, bool throwIfRelationshipNotSupported)
	{
		jobHeader.SJH_GB = branch.PK;
		base.SetRelationshipDefaultsForElementCore(jobHeader, throwIfRelationshipNotSupported);
	}

	protected override bool AllowNew => false;
}
