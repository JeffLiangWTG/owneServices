using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class PeriodCollection : AccPeriodManagementCollection
	{
		readonly PeriodManager fPeriodManager;

		public PeriodCollection(BusinessObjectFactory factory, PeriodManager periodManager)
			: base(factory)
		{
			fPeriodManager = periodManager;
		}

		public PeriodCollection(BusinessObjectFactory factory, ZQuery sQLFilter, PeriodManager periodManager)
			: base(factory, sQLFilter)
		{
			fPeriodManager = periodManager;
		}

		public new Period this[int index]
		{
			get { return (Period)Elements[index]; }
		}

		public new Period AddNew()
		{
			return (Period)base.AddNew();
		}

		public new Period AddNew(Type typeToAdd)
		{
			return (Period)base.AddNew(typeToAdd);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (fPeriodManager != null)
			{
				((Period)bizOAdded).PeriodManager = fPeriodManager;
			}
		}
	}
}
