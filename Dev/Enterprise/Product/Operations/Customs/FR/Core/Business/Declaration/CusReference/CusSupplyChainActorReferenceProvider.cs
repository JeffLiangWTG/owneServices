using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusSupplyChainActorReferenceProvider : EU.Business.Declaration.CusSupplyChainActorReferenceProvider
	{
		protected CusSupplyChainActorReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
		{
		}

		protected override IReadOnlyList<string> ColumnNamesInSortOrderCore => new string[] { AutoCusReference.Schema.CFR_Code, CommonCusReference.Schema.OwnerOrgPK, AutoCusReference.Schema.CFR_Reference };
	}
}
