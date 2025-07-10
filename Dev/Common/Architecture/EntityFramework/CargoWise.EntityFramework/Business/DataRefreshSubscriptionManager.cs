using System;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// This class allows communication between objects when updated by a DataRefreshBus operation.
	/// A good example is during the editing of a commercial invoice.
	/// If the user pops up the maintenance form for a part and changes some data,
	/// the currently selected invoice line should automatically update its data to match the changed part.
	/// Without using this class, the part will update, but the commercial invoice will not.
	/// </summary>
	public class DataRefreshSubscriptionManager
	{
		/// <summary>
		/// Pass an EventHandler instance to the constructor that you want called whenever the business object is modified.
		/// The method being called should be the same method that is called when setting the property manually.
		/// eg: When hooking the Part object the Part setter code should be called by MethodToCall.
		/// </summary>
		/// <param name="methodToCall"></param>
		public DataRefreshSubscriptionManager(EventHandler methodToCall)
		{
			this.MethodToCall = methodToCall;
		}

		public readonly EventHandler MethodToCall;

		/// <summary>
		/// Set the business object that you are interested in watching here.
		/// Connection and disconnection is handled automatically.
		/// </summary>
		public IBusinessObjectState BusinessObject
		{
			get { return fBusinessObject; }
			set
			{
				if (fBusinessObject != value)
				{
					DetachMethodCall();
					fBusinessObject = value;
					AttachMethodCall();
				}
			}
		}

		/// <summary>
		/// The connection can be enabled/disabled using this property.
		/// </summary>
		public bool Enabled
		{
			get { return fEnabled; }
			set
			{
				if (fEnabled != value)
				{
					DetachMethodCall();
					fEnabled = value;
					AttachMethodCall();
				}
			}
		}

		#region Implementation

		void DetachMethodCall()
		{
			if (BusinessObject != null)
			{
#pragma warning disable
				BusinessObject.UpdatedByDataRefreshIncludingChildren -= MethodToCall;
#pragma warning restore
			}
		}

		void AttachMethodCall()
		{
			if (BusinessObject != null && Enabled)
			{
#pragma warning disable
				BusinessObject.UpdatedByDataRefreshIncludingChildren += MethodToCall;
#pragma warning restore
			}
		}

		IBusinessObjectState fBusinessObject;
		bool fEnabled = true;

		#endregion
	}
}
