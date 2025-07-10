using System;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class ITransactionCollection<T> : BusinessObjectCollection<T> where T : BusinessObject, ITransaction
	{
		public ITransactionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region OrganizationGuid

		public ZGuid OrganizationGuid
		{
			get
			{
				return fOrganizationGuid;
			}
			set
			{
				fOrganizationGuid = value;
			}
		}

		protected ZGuid fOrganizationGuid;

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		#endregion

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("Adding to this collection is not supported.");
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			throw new NotSupportedException("Loading to this collection is not supported.");
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((ITransaction)child).Organization = OrganizationGuid;
		}
	}
}
