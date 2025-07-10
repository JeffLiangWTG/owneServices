using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class DependentJob<T> : Job where T : BusinessObject
	{
		protected DependentJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		T fMaster;
		internal T Master
		{
			get
			{
				if (fMaster == null)
				{
					foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						if (parentCollection is DependentJobCollection<T>)
						{
							fMaster = ((DependentJobCollection<T>)parentCollection).Master;
							break;
						}
					}
				}

				return fMaster;
			}
		}

		#region Methods

		public abstract void RefreshCachedValues();

		#endregion
	}
}
