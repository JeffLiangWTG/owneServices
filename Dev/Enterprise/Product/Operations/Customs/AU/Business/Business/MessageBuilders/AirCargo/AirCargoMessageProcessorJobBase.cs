using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class AirCargoMessageProcessorJobBase : IHouseBillsCargoMessageProcessorJob
	{
		public bool SendChildren => true;

		public bool IsAcceptable
		{
			get
			{
				var result = false;
				if (IsDischargedAtAustralianPort && IsAir && MasterBill != null)
				{
					MasterBill.RunPreSaveValidationExcludingChildren();
					return !MasterBill.HasErrors && !MasterBill.HasMessageErrorsNotIncludingChildren;
				}
				return result;
			}
		}

		public CusMAWB MasterBill => MasterBillCore;

		public string ReasonWhyNotAcceptable
		{
			get
			{
				var result = string.Empty;

				if (!IsDischargedAtAustralianPort)
				{
					result = "The final discharge port is not an Australian port.";
				}
				else if (!IsAir)
				{
					result = "This is not an Air Cargo.";
				}
				else if (MasterBill == null)
				{
					result = "A Customs master bill record cannot be created as someone else is trying to create a master bill for this consol.";
				}
				else if (MasterBill.HasErrors)
				{
					var errors = new ZStringBuilder();
					MasterBill.GetErrors().ForEach(action => errors.Append(action.Message));
					result = errors.ToString();
				}
				else if (MasterBill.HasMessageErrorsNotIncludingChildren)
				{
					result = MasterBill.MessageErrorsString;
				}

				return result;
			}
		}

		public string GetReferenceNumber(BusinessObject child) => GetReferenceNumberCore(child);

		public void SetSACIfRequired(BusinessObject child)
		{
			this.SetSACIfRequiredCore(child);
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		#endregion // IDisposable

		#region Implementation

		protected readonly bool shouldDelaySending;

		protected AirCargoMessageProcessorJobBase(bool shouldDelaySending = false)
		{
			this.shouldDelaySending = shouldDelaySending;
		}

		protected abstract bool IsAir
		{
			get;
		}

		protected abstract bool IsDischargedAtAustralianPort
		{
			get;
		}

		protected abstract CusMAWB MasterBillCore
		{
			get;
		}

		protected abstract IEnumerable<BusinessObject> ChildrenCore
		{
			get;
		}

		protected abstract string GetReferenceNumberCore(BusinessObject child);

		protected abstract void SetSACIfRequiredCore(BusinessObject child);

		protected abstract CMRMessageManager GetMessageManagerCore(BusinessObject child);

		#endregion // Implementation

		#region IHouseCargoMessageProcessorJob

		BusinessObject IHouseBillsCargoMessageProcessorJob.MasterBill => MasterBill;

		IEnumerable<BusinessObject> IHouseBillsCargoMessageProcessorJob.Children => ChildrenCore;

		CMRMessageManager IHouseBillsCargoMessageProcessorJob.GetMessageManager(BusinessObject child) => GetMessageManagerCore(child);

		void IHouseBillsCargoMessageProcessorJob.SetSACIfRequired(BusinessObject houseBill)
		{
			SetSACIfRequired(houseBill);
		}

		string IHouseBillsCargoMessageProcessorJob.GetReferenceNumber(BusinessObject houseBill) => GetReferenceNumber(houseBill);

		#endregion
	}
}
