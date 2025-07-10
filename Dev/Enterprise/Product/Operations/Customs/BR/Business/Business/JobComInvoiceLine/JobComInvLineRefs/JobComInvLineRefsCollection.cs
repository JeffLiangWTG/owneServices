using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class JobComInvLineRefsCollection<T> : DependentBusinessObjectCollection<T, BusinessObject>, IEnumerable<BusinessObject>, IBusinessObjectCollection, IList, ICollection, IEnumerable where T : JobComInvLineRefs
	{
		public JobComInvLineRefsCollection(BusinessObject parent, ZString referenceType)
			: base(parent)
		{
			JG_ReferenceType = referenceType;
		}

		public readonly ZString JG_ReferenceType;

		public IEnumerator<BusinessObject> GetEnumerator()
		{
			return Elements.Cast<BusinessObject>().GetEnumerator();
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JobComInvLineRefsSchema.JG_ReferenceType, JG_ReferenceType);
			return result;
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			JobComInvLineRefs dependent = (JobComInvLineRefs)child;
			dependent.JG_ReferenceType = JG_ReferenceType;
		}
	}
}
