using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondUnionCollectionForSeaCargo : CusUnderbondUnionCollection
	{
		public CusUnderbondUnionCollectionForSeaCargo(IAUCusUnderbondUnionCollectionParent parent)
			: base(parent)
		{
		}

		public override void Load()
		{
			var possibleParents = GetListOfAllPossibleProviders().WhereNotNull().ToList();
			if (possibleParents.Any())
			{
				possibleParents.Select(x => x.Identifier).Batch(5_000).ForEach(group =>
				{
					var underbonds = Factory.Load<CusUnderbond>(new ZQuery(CusUnderbondSchema.C4_ParentID, group));
					foreach (var underbond in underbonds)
					{
						base.Add(underbond);
					}
				});
			}
		}
	}
}
