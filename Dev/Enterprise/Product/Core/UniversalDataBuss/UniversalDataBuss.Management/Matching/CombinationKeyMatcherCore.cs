using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Management
{
	public abstract class CombinationKeyMatcherCore<T>
		where T : BusinessObject
	{
		public CombinationKeyMatcherCore(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		protected readonly BusinessObjectFactory factory;

		protected List<ZGuid> GetParentPKs(ZDBOnlyQuery parentQuery)
		{
			var matchingParents = factory.Load<T>(parentQuery);

			var parentPKs = new List<ZGuid>();
			foreach (var matchingParent in matchingParents)
			{
				parentPKs.Add(matchingParent.PK);
			}

			return parentPKs.Count == 0 ? null : parentPKs;
		}
	}
}
