using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Validators
{
	class CARFRXMessageValidator : CcsukTransmissionMessageValidator
	{
		public CARFRXMessageValidator(IBusiness bizO)
			: base(bizO)
		{
		}

		protected override Dictionary<ZPropertyInfo, Action> GetDictionaryOfFieldsToValidate()
		{
			return GetCommonFieldsToValidate();
		}

		protected override void PerformAdditionalValidation(biz.MessageSendingNotificationCollection notificationCollection)
		{
			base.PerformAdditionalValidation(notificationCollection);
			if (mawb != null && !mawb.IsBasic && (from CusHAWB h in mawb.ChildBills where h.IsLodgedAtCcsuk select h).Any())
			{
				notificationCollection.AddWarning("This MAWB contains child house bills that are still active on CCS-UK. You should delete the houses before deleting the master. Continue anyway?");
			}
			else if (mawb != null && mawb.IsBasic && mawb.HasSplits)
			{
				notificationCollection.AddWarning("This basic AWB is split. You should remove the splits first. Continue anyway?");
			}
		}
	}
}
