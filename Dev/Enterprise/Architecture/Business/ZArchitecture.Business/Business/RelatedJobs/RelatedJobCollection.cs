using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class RelatedJobCollection : BusinessObjectCollection<BusinessObject>
	{
		public RelatedJobCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new IRelatedJob this[int index]
		{
			get { return (IRelatedJob)Elements[index]; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			if (!(bizOAdded is IRelatedJob))
			{
				throw new ArgumentException(
					"The BusinessObject '" + bizOAdded.GetType().Name + "' added to RelatedJobCollection must implement IRelatedJob.");
			}
			base.OnAdded(bizOAdded);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
