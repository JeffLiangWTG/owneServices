using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Validators
{
	class FSRMessageValidator : CcsukTransmissionMessageValidator
	{
		public FSRMessageValidator(IBusiness bizO)
			: base(bizO)
		{
		}

		protected override Dictionary<ZPropertyInfo, Action> GetDictionaryOfFieldsToValidate()
		{
			var fieldsToValidate = new Dictionary<ZPropertyInfo, Action>();
			if (hawb != null)
			{
				fieldsToValidate.Add(hawb.CS_HAWBInfo, hawb.Validation.ValidateCS_HAWB);
				if (hawb.MAWB != null)
				{
					fieldsToValidate.Add(hawb.MAWB.CM_MAWBInfo, hawb.MAWB.Validation.ValidateCM_MAWB);
				}
			}
			else
			{
				fieldsToValidate.Add(mawb.CM_MAWBInfo, mawb.Validation.ValidateCM_MAWB);
			}
			return fieldsToValidate;
		}
	}
}
