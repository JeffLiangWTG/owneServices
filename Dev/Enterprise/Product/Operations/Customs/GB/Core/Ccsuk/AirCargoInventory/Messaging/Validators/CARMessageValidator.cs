using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Validators
{
	class CARMessageValidator : CcsukTransmissionMessageValidator
	{
		public CARMessageValidator(IBusiness bizO)
			: base(bizO)
		{
		}

		protected override Dictionary<ZPropertyInfo, Action> GetDictionaryOfFieldsToValidate()
		{
			var fieldsToValidate = GetCommonFieldsToValidate();
			if (hawb != null)
			{
				fieldsToValidate.Add(hawb.CS_RL_NKLoadPortInfo, hawb.Validation.ValidateCS_RL_NKLoadPort);
				fieldsToValidate.Add(hawb.CS_RL_NKDischargePortInfo, hawb.Validation.ValidateCS_RL_NKDischargePort);
				fieldsToValidate.Add(hawb.ShipmentDescriptionCodeInfo, hawb.Validation.ValidateShipmentDescriptionCode);
				fieldsToValidate.Add(hawb.CS_PiecesManifestedInfo, hawb.Validation.ValidateCS_PiecesManifested);
				fieldsToValidate.Add(hawb.CS_WeightUQInfo, hawb.Validation.ValidateCS_WeightUQ);
				fieldsToValidate.Add(hawb.CS_WeightInfo, hawb.Validation.ValidateCS_Weight);
				fieldsToValidate.Add(hawb.CS_GoodsDescriptionInfo, hawb.Validation.ValidateCS_GoodsDescription);
				fieldsToValidate.Add(hawb.CS_PiecesLandedInfo, hawb.Validation.ValidateCS_PiecesLanded);
			}
			else
			{
				fieldsToValidate.Add(mawb.CM_RL_NKLoadPortInfo, mawb.MasterLevelHouseHelper.Validation.ValidateCS_RL_NKLoadPort);
				fieldsToValidate.Add(mawb.CM_RL_NKDischargePortInfo, mawb.Validation.ValidateCM_RL_NKDischargePort);
				fieldsToValidate.Add(mawb.ShipmentDescriptionCodeInfo, mawb.MasterLevelHouseHelper.Validation.ValidateShipmentDescriptionCode);
				fieldsToValidate.Add(mawb.NumberOfPiecesExpectedInfo, mawb.MasterLevelHouseHelper.Validation.ValidateCS_PiecesManifested);
				fieldsToValidate.Add(mawb.WeightCodeInfo, mawb.MasterLevelHouseHelper.Validation.ValidateCS_WeightUQ);
				fieldsToValidate.Add(mawb.WeightInfo, mawb.MasterLevelHouseHelper.Validation.ValidateCS_Weight);
				fieldsToValidate.Add(mawb.DescriptionOfGoodsInfo, mawb.MasterLevelHouseHelper.Validation.ValidateCS_GoodsDescription);
				fieldsToValidate.Add(mawb.CM_FlightNoInfo, mawb.Validation.ValidateCM_FlightNo);
				fieldsToValidate.Add(mawb.CM_ArrivalDateInfo, mawb.Validation.ValidateCM_ArrivalDate);
				fieldsToValidate.Add(mawb.NumberOfPiecesReceivedInfo, mawb.MasterLevelHouseHelper.Validation.ValidateCS_PiecesLanded);
			}
			return fieldsToValidate;
		}
	}
}
