using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Business
{
	public class Enquiry : Callout
	{
		public Enquiry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void ForceIntoFinanceQueue()
		{
			MoveToQueue(
				CommercialQueueCodeDescriptionPairList.Codes.Finance,
				ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment,
				ZString.Empty,
				"FORCED CONDITION: FINANCE WITH OQ");

			RequiresForceInfoFinanceQueueLogReference = true;
			IsExcludedFromBISIWarning = true;
		}

		protected override Type TypeOfProcessQueue
		{
			get { return typeof(UPECargoReportQueue); }
		}

		protected override void OnCreateAutoAdminLog()
		{
			base.OnCreateAutoAdminLog();
			if (RequiresForceInfoFinanceQueueLogReference)
			{
				ZString referenceToAppend = Logs.AutoCreatedLog.SL_Reference.IsEmpty ? "" : "; ";
				referenceToAppend += "Forced to Finance";
				using (((IUpdateFieldsLock)Logs.AutoCreatedLog).LockForUpdatingKeyFields())
				{
					Logs.AutoCreatedLog.SL_Reference += referenceToAppend;
				}
				RequiresForceInfoFinanceQueueLogReference = false;
			}
		}

		bool RequiresForceInfoFinanceQueueLogReference;
	}
}
