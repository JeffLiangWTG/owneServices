using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Validators
{
	class CARFCSMessageValidator : CcsukTransmissionMessageValidator
	{
		public CARFCSMessageValidator(IBusiness bizO)
			: base(bizO)
		{
		}

		protected override Dictionary<ZPropertyInfo, Action> GetDictionaryOfFieldsToValidate()
		{
			var fieldsToValidate = GetCommonFieldsToValidate();
			if (hawb != null)
			{
				fieldsToValidate.Add(hawb.CS_PiecesManifestedInfo, hawb.Validation.ValidateCS_PiecesManifested);
				fieldsToValidate.Add(hawb.CS_WeightUQInfo, hawb.Validation.ValidateCS_WeightUQ);
				fieldsToValidate.Add(hawb.CS_WeightInfo, hawb.Validation.ValidateCS_Weight);
			}
			else
			{
				fieldsToValidate.Add(mawb.NumberOfPiecesExpectedInfo, mawb.MasterLevelHouseHelper.Validation.ValidateCS_PiecesManifested);
				fieldsToValidate.Add(mawb.WeightCodeInfo, mawb.MasterLevelHouseHelper.Validation.ValidateCS_WeightUQ);
				fieldsToValidate.Add(mawb.WeightInfo, mawb.MasterLevelHouseHelper.Validation.ValidateCS_Weight);
			}
			return fieldsToValidate;
		}
	}
}
