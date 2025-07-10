using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business
{
	internal class UPECMRCusHAWBValidation : CMRCusHAWBValidation
	{
		public UPECMRCusHAWBValidation(UPECusHAWB hAWB)
			: base(hAWB)
		{
			DuplicateHAWBAndCoLoadValidator = new HAWBAndCoLoadHAWBValidator(HAWB);
		}

		public new UPECusHAWB HAWB
		{
			get { return (UPECusHAWB)base.HAWB; }
		}

		protected override void CheckCS_MasterHouseBill()
		{
			if (Shipment != null)
			{
				if (Shipment.CoLoadMasterShipment != null)
				{
					MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_MasterHouseBillInfo, Shipment.CoLoadMasterShipment.JS_HouseBill);
				}
				else if (!Parent.CS_MasterHouseBill.IsEmpty)
				{
					Parent.CS_MasterHouseBillInfo.AddWarning("Shipment does not have a master shipment");
				}
			}

			if (Parent.CS_IsPrealerted && HAWB.IsMasterHouseBillDifferent && HAWB.HasMessageChanges)
			{
				Parent.CS_MasterHouseBillInfo.AddMessageError("You made changes on the Co-Load master number with other changes, too.\r\nIf you need to amend the Co-Load master number, please change the Co-Load master number only and save.");
			}
		}

		protected override void CheckCS_HAWB()
		{
			ValidateCS_HAWBFromCusHAWBValidation();
			ValidateCS_HAWBFromAirCargoCusHAWBValidation();
			ValidateCS_HAWBFromCMRCusHAWBValidation();
		}

		void ValidateCS_HAWBFromCusHAWBValidation()
		{
			if (Parent.CS_HAWB.IsEmpty)
			{
				Parent.CS_HAWBInfo.AddMessageError("Housebill number is required for air cargo messaging.");
			}
		}

		HAWBAndCoLoadHAWBValidator DuplicateHAWBAndCoLoadValidator { get; }
		void ValidateCS_HAWBFromAirCargoCusHAWBValidation()
		{
			if (Shipment != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_HAWBInfo, Shipment.JS_HouseBill);
			}
			if (isValidatingAll)
			{
				DuplicateHAWBAndCoLoadValidator.ValidateDuplicateHAWBAndCoLoad();
			}
		}

		void ValidateCS_HAWBFromCMRCusHAWBValidation()
		{
			if (HAWB.CS_HAWB.IsEmpty && !HAWB.IsDirect)
			{
				Parent.CS_HAWBInfo.AddError("Housebill number is required for air cargo messaging.");
			}
			else
			{
				new CustomsValidation(HAWB.CS_HAWBInfo).ErrorOnKeyDataWithNoChildren(HAWB.CMRMessageStatus.Code);
			}
		}
	}
}
