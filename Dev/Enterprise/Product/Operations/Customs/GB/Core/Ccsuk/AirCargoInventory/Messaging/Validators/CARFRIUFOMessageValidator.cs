using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Validators
{
	class CARFRIUFOMessageValidator : CARMessageValidator
	{
		public CARFRIUFOMessageValidator(CusMAWB mawb)
			: base(mawb)
		{
		}

		protected override Dictionary<ZPropertyInfo, Action> GetDictionaryOfFieldsToValidate()
		{
			var fieldsToValidate = GetCommonFieldsToValidate();
			if (mawb != null)
			{
				fieldsToValidate.Add(mawb.CM_FlightNoInfo, mawb.Validation.ValidateCM_FlightNo);
				fieldsToValidate.Add(mawb.CM_ArrivalDateInfo, mawb.Validation.ValidateCM_ArrivalDate);
				fieldsToValidate.Add(mawb.NumberOfPiecesReceivedInfo, mawb.MasterLevelHouseHelper.Validation.ValidateCS_PiecesLanded);
			}
			return fieldsToValidate;
		}
	}
}
