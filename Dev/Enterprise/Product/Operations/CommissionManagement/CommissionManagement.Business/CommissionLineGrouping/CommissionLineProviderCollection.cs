using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionLineProviderCollection<T> : BusinessObjectCollection<T>
		where T : BusinessObject, IViewCommissionLineProvider
	{
		#region Constructors

		public CommissionLineProviderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CommissionLineProviderCollection(BusinessObjectFactory factory, IEnumerable<T> lines)
			: base(factory)
		{
			AddRange(lines);
		}

		#endregion

		#region Allowed Actions

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}
