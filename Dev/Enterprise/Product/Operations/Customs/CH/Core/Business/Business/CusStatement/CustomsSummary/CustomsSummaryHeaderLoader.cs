using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public sealed class CustomsSummaryHeaderLoader : BusinessObject.Loader
{
	public CustomsSummaryHeaderLoader(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override Type GetTypeOfBusinessObjectToLoad() => typeof(CustomsSummaryHeader);

	public CustomsSummaryHeader LoadBorderau(ZString bordereauNumber, ZDate bordereauDate)
	{
		var query = new ZQuery();
		query.AddToFilter(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
		query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, bordereauNumber);
		query.AddToFilter(CusStatementHeaderSchema.B2_ProcessDate, bordereauDate);
		return Factory.LoadTop1<CustomsSummaryHeader>(query);
	}
}
