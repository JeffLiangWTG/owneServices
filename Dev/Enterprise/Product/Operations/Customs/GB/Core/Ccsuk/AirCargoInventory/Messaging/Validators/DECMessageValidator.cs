using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Validators
{
	class DECMessageValidator : CcsukTransmissionMessageValidator
	{
		public DECMessageValidator(IBusiness bizO) : base(bizO)
		{
			underbond = bizO as CusUnderbond;
		}

		protected override Dictionary<ZPropertyInfo, Action> GetDictionaryOfFieldsToValidate()
		{
			var fieldsToValidate = GetCommonFieldsToValidate();
			if (hawb != null)
			{
				fieldsToValidate.Add(hawb.CS_RL_NKDischargePortInfo, hawb.Validation.ValidateCS_RL_NKDischargePort);
			}
			else
			{
				fieldsToValidate.Add(mawb.CM_RL_NKDischargePortInfo, mawb.Validation.ValidateCM_RL_NKDischargePort);
			}

			if (underbond != null)
			{
				underbond.Validation.AddFieldsToValidateForMessageSending(fieldsToValidate);
			}
			return fieldsToValidate;
		}

		readonly CusUnderbond underbond;
	}
}
