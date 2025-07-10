using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public abstract class DeleteDetails
	{
		public ZBool CanDelete
		{
			get { return this is Allow; }
		}

		public class Allow : DeleteDetails
		{
		}

		public class Disallow : DeleteDetails
		{
			public Disallow(ZString reason)
			{
				this.Reason = reason;
			}
		}

		public ZString Reason;
	}

	public abstract class DeleteChecker : IBusinessObjectStrategy
	{
		/// <summary>
		/// Return either an DeleteDetails.Allow or DeleteDetails.Disallow object
		/// </summary>
		/// <param name="businessObject"></param>
		/// <returns></returns>
		public abstract DeleteDetails DeleteDetails(BusinessObject businessObject);

		/// <summary>
		/// Called immediately before the object is successfully deleted
		/// </summary>
		public virtual void BeforeSuccessfulDelete(BusinessObject businessObject)
		{
		}

		public virtual void AddFetchHint(BusinessObject businessObject)
		{
		}

		#region IBusinessObjectStrategy Unused Members

		void IBusinessObjectStrategy.OnSaving(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnSavingInObjectsWithLateChanges(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnSaved(BusinessObject businessObject, bool saveSucceeded)
		{
		}

		void IBusinessObjectStrategy.OnFactorySaving(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnFactorySaved(BusinessObject businessObject, bool saveSucceeded)
		{
		}

		void IBusinessObjectStrategy.OnSaveRollback(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.OnDelete(BusinessObject businessObject)
		{
		}

		void IBusinessObjectStrategy.FetchForLoad(BusinessObject businessObject)
		{
		}

		#endregion
	}
}
