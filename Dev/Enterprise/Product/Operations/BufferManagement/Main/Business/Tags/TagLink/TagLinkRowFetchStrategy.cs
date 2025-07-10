using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class TagLinkRowFetchStrategy : IRowFetchStrategy
	{
		void IRowFetchStrategy.FetchForLoad(BusinessObjectFactory factory, DataRow[] rows)
		{
			foreach (var row in rows)
			{
				var magnitudePK = new ZGuid(row[TagLinkSchema.TGL_TGM_Magnitude.Name]);

				if (magnitudePK.IsValid)
				{
					factory.AddFetchHint(TagMagnitudeSchema.PK, magnitudePK);
				}
			}
		}
	}
}
