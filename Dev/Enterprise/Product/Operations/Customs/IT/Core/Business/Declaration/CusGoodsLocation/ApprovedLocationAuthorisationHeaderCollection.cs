using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class ApprovedLocationAuthorisationHeaderCollection : EU.Business.CusAuthorisationHeaderCollectionFiltered
{
	public ApprovedLocationAuthorisationHeaderCollection(BusinessObjectFactory factory, ZGuid agcOwner) : base(factory, ZString.Empty, agcOwner)
	{
	}

	protected override ZQuery CreateRelationshipFilter()
	{
		var relationShipFilterQuery = base.CreateRelationshipFilter();

		var innerFilterByTypeQuery = new ZQuery(CusPermitHeaderSchema.CPH_Type, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport)
			.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_Type, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport);

		relationShipFilterQuery.AddToFilter(innerFilterByTypeQuery, JoinCondition.And);
		return relationShipFilterQuery;
	}
}
