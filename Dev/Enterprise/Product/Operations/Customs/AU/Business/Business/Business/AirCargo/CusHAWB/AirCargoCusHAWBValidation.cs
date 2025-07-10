using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class AirCargoCusHAWBValidation : CusHAWBValidation
	{
		protected AirCargoCusHAWBValidation(CusHAWB parent)
			: base(parent)
		{
			duplicateHAWBAndCoLoadValidator = new HAWBAndCoLoadHAWBValidator(HAWB);
		}

		protected new CusHAWB Parent
		{
			get { return (CusHAWB)base.Parent; }
		}

		protected override void CheckCS_ConsigneeName()
		{
			base.CheckCS_ConsigneeName();
			var consigneeAddress = HAWB.ConsigneeAddressOnShipment;
			if (consigneeAddress != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(HAWB.CS_ConsigneeNameInfo, consigneeAddress.CompanyName.Left(HAWB.CS_ConsigneeNameInfo.MaxLength));
			}
		}

		protected override void CheckCS_ConsigneeCity()
		{
			base.CheckCS_ConsigneeCity();
			var consigneeAddress = HAWB.ConsigneeAddressOnShipment;
			if (consigneeAddress != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(HAWB.CS_ConsigneeCityInfo, consigneeAddress.City);
			}
		}

		protected override void CheckCS_ConsigneePhone()
		{
			base.CheckCS_ConsigneePhone();
			var consigneeAddress = HAWB.ConsigneeAddressOnShipment;
			if (consigneeAddress != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(HAWB.CS_ConsigneePhoneInfo, consigneeAddress.Phone);
			}
		}

		protected override void CheckCS_ConsigneePostcode()
		{
			base.CheckCS_ConsigneePostcode();
			var consigneeAddress = HAWB.ConsigneeAddressOnShipment;
			if (consigneeAddress != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(HAWB.CS_ConsigneePostcodeInfo, consigneeAddress.PostCode);
			}
		}

		protected override void CheckCS_ConsigneeState()
		{
			base.CheckCS_ConsigneeState();
			var consigneeAddress = HAWB.ConsigneeAddressOnShipment;
			if (consigneeAddress != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(HAWB.CS_ConsigneeStateInfo, consigneeAddress.State);
			}
		}

		protected override void CheckCS_ConsigneeStreet()
		{
			base.CheckCS_ConsigneeStreet();
			var consigneeAddress = HAWB.ConsigneeAddressOnShipment;
			if (consigneeAddress != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(HAWB.CS_ConsigneeStreetInfo, consigneeAddress.Address1);
			}
		}

		protected override void CheckCS_RN_NKConsigneeCountry()
		{
			base.CheckCS_RN_NKConsigneeCountry();

			if (!Parent.CS_RN_NKConsigneeCountryInfo.HasNotifications())
			{
				var consigneeAddress = HAWB.ConsigneeAddressOnShipment;
				if (consigneeAddress != null && consigneeAddress.CountryCode != HAWB.CS_RN_NKConsigneeCountry)
				{
					HAWB.CS_RN_NKConsigneeCountryInfo.AddWarning(HAWB.CS_RN_NKConsigneeCountry + " is different from the freight job data, " + consigneeAddress.CountryCode);
				}
			}
		}

		protected override void CheckCS_ConsignorName()
		{
			base.CheckCS_ConsignorName();
			var consignorAddress = HAWB.ConsignorAddressOnShipment;
			if (consignorAddress != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(HAWB.CS_ConsignorNameInfo, consignorAddress.CompanyName.Left(HAWB.CS_ConsignorNameInfo.MaxLength));
			}
		}

		protected override void CheckCS_ConsignorStreet()
		{
			base.CheckCS_ConsignorStreet();
			var consignorAddress = HAWB.ConsignorAddressOnShipment;
			if (consignorAddress != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(HAWB.CS_ConsignorStreetInfo, consignorAddress.Address1);
			}
		}

		protected override void CheckCS_ConsignorCity()
		{
			base.CheckCS_ConsignorCity();
			var consignorAddress = HAWB.ConsignorAddressOnShipment;
			if (consignorAddress != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(HAWB.CS_ConsignorCityInfo, consignorAddress.City);
			}
		}

		protected override void CheckCS_ConsignorPostcode()
		{
			base.CheckCS_ConsignorPostcode();
			var consignorAddress = HAWB.ConsignorAddressOnShipment;
			if (consignorAddress != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(HAWB.CS_ConsignorPostcodeInfo, consignorAddress.PostCode);
			}
		}

		protected override void CheckCS_ConsignorState()
		{
			base.CheckCS_ConsignorState();
			var consignorAddress = HAWB.ConsignorAddressOnShipment;
			if (consignorAddress != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(HAWB.CS_ConsignorStateInfo, consignorAddress.State);
			}
		}

		protected override void CheckCS_RN_NKConsignorCountry()
		{
			base.CheckCS_RN_NKConsignorCountry();
			if (!Parent.CS_RN_NKConsignorCountryInfo.HasNotifications())
			{
				var consignorAddress = HAWB.ConsignorAddressOnShipment;
				if (consignorAddress != null && consignorAddress.CountryCode != HAWB.CS_RN_NKConsignorCountry)
				{
					HAWB.CS_RN_NKConsignorCountryInfo.AddWarning(HAWB.CS_RN_NKConsignorCountry + " is different from the freight job data, " + consignorAddress.CountryCode);
				}
			}
		}

		protected override void CheckCS_GoodsDescription()
		{
			base.CheckCS_GoodsDescription();

			if (!Parent.CS_GoodsDescriptionInfo.HasMessageErrors() && Shipment != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_GoodsDescriptionInfo, Shipment.FullGoodsDescription);
			}
		}

		protected override void CheckCS_FreightPrepaidCollect()
		{
			base.CheckCS_FreightPrepaidCollect();
			if (Shipment != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_FreightPrepaidCollectInfo, Shipment.JS_PaymentTerm);
			}
		}

		protected override void CheckCS_GoodsValue()
		{
			base.CheckCS_GoodsValue();
			if (!Parent.CS_GoodsValueInfo.HasNotifications() && Shipment != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_GoodsValueInfo, Shipment.JS_GoodsValue);
			}
		}

		protected override void CheckCS_RL_NKDestination()
		{
			base.CheckCS_RL_NKDestination();
			if (!HAWB.CS_RL_NKDestinationInfo.HasNotifications() && Shipment != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_RL_NKDestinationInfo, Shipment.JS_RL_NKDestination);
			}
		}

		protected override void CheckCS_RL_NKOrigin()
		{
			base.CheckCS_RL_NKOrigin();
			if (!HAWB.CS_RL_NKOriginInfo.HasNotifications() && Shipment != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_RL_NKOriginInfo, Shipment.JS_RL_NKOrigin);
			}
		}

		protected override void CheckCS_RX_NKGoodsCurrency()
		{
			base.CheckCS_RX_NKGoodsCurrency();
			if (Parent.CS_RX_NKGoodsCurrency.IsEmpty && IsGoodsCurrencyRequired)
			{
				Parent.CS_RX_NKGoodsCurrencyInfo.AddMessageError("A goods currency must be specified for air cargo messaging.");
			}
			else if (!Parent.CS_RX_NKGoodsCurrency.IsEmpty && Parent.GoodsCurrency == null)
			{
				Parent.CS_RX_NKGoodsCurrencyInfo.AddError("Please enter a valid currency.");
			}
			else if (Shipment != null && Shipment.GoodsValueCurr != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_RX_NKGoodsCurrencyInfo, Shipment.GoodsValueCurr.PK);
			}
		}

		protected override void CheckCS_Weight()
		{
			base.CheckCS_Weight();
			if (!HAWB.CS_WeightInfo.HasNotifications() && Shipment != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_WeightInfo, Shipment.JS_ActualWeight);
			}
		}

		protected override void CheckCS_WeightUQ()
		{
			base.CheckCS_WeightUQ();
			if (Shipment != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_WeightUQInfo, Shipment.JS_UnitOfWeight);
			}
		}

		bool HAWBHasCoLoad
		{
			get
			{
				CusMAWB mAWB = Parent.MAWB;
				if (mAWB != null)
				{
					BusinessObject[] foundHAWbs = mAWB.ChildBills.Find(new ZQuery(CusHAWBSchema.CS_MasterHouseBill, HAWB.CS_HAWB));
					return foundHAWbs.Length > 0;
				}
				else
				{
					return false;
				}
			}
		}

		protected override void CheckCS_IsMasterHouse()
		{
			base.CheckCS_IsMasterHouse();

			if (!Parent.CS_IsMasterHouse && !Parent.CS_HAWB.IsEmpty)
			{
				if (HAWBHasCoLoad)
				{
					Parent.CS_IsMasterHouseInfo.AddMessageError("There are house bills under the master that have this house bill as a co-load.");
				}
				else if (Shipment != null && Shipment.CoLoadShipments.Count > 0)
				{
					Parent.CS_IsMasterHouseInfo.AddWarning("Shipment has co-load sub-shipments.");
				}
			}
			else if (Shipment != null && Shipment.CoLoadMasterShipment != null)
			{
				Parent.CS_IsMasterHouseInfo.AddWarning("Shipment does not have a master shipment.");
			}

			if (Parent.CS_IsPrealerted && HAWB.IsMasterFlagDifferent && HAWB.HasMessageChanges)
			{
				Parent.CS_IsMasterHouseInfo.AddMessageError("You made changes on 'Is Master House' flag and other changes, too.\r\nIf you need to amend 'Is Master House' flag, please change the flag only and save.");
			}
		}

		protected override void CheckCS_PiecesManifested()
		{
			base.CheckCS_PiecesManifested();
			if (!Parent.CS_PiecesManifestedInfo.HasNotifications() && Shipment != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_PiecesManifestedInfo, Shipment.JS_OuterPacks);
			}
		}

		protected override void CheckCS_MasterHouseBill()
		{
			base.CheckCS_MasterHouseBill();

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
			base.CheckCS_HAWB();

			if (Shipment != null)
			{
				MessageValidation.ValidateAirCargoDataDifferentFromFreight(Parent.CS_HAWBInfo, Shipment.JS_HouseBill);
			}
			if (isValidatingAll)
			{
				duplicateHAWBAndCoLoadValidator.ValidateDuplicateHAWBAndCoLoad();
			}
		}

		protected class HAWBAndCoLoadHAWBValidator
		{
			public HAWBAndCoLoadHAWBValidator(CusHAWB houseBill)
			{
				this.houseBill = houseBill;
			}

			readonly CusHAWB houseBill;

			public ZString MessageError;
			public ZString Warning;

			public void ValidateDuplicateHAWBAndCoLoad()
			{
				Validate();

				if (!MessageError.IsEmpty)
				{
					houseBill.CS_HAWBInfo.AddMessageError(MessageError);
				}
				else if (!Warning.IsEmpty)
				{
					houseBill.CS_HAWBInfo.AddWarning(Warning);
				}
			}

			public void Validate()
			{
				var hAWB = houseBill.CS_HAWB;
				var coLoadHAWB = houseBill.AggregatedCoLoadMaster;
				var mAWB = houseBill.CS_MasterBillNum;

				if (!hAWB.IsEmpty && !mAWB.IsEmpty && houseBill.MAWB != null)
				{
					var factory = houseBill.Factory;
					MessageError = "";
					Warning = "";

					var houseBillsWithDuplicateKeys = new StringBuilder();
					bool hasOtherHouseBillsPreAlertedOrMessagePending = false;
					var duplicatedHawbs = new List<CusHAWB>();

					var findDuplicateHAWBDBQuery = new ZDBOnlyQuery(typeof(CusHAWB));
					findDuplicateHAWBDBQuery.AddToFilter(CusHAWBSchema.CS_HAWB, hAWB);
					findDuplicateHAWBDBQuery.AddToFilter(CusHAWBSchema.CS_CM, SQLComparisonOperator.NotEqual, houseBill.CS_CM);
					var subQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
					subQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, houseBill.MAWB.CM_ApplicationCode);
					subQuery.AddToFilter(CusMAWBSchema.CM_IsActive, true);
					subQuery.AddToFilter(CusMAWBSchema.CM_MAWB, mAWB);
					findDuplicateHAWBDBQuery.AddSubQuery(subQuery, JoinCondition.And);
					var duplicatedHawbsInDB = factory.Load<CusHAWB>(findDuplicateHAWBDBQuery);
					duplicatedHawbs.AddRange(duplicatedHawbsInDB);

					var findDuplicateHAWBInMemory = new ZQuery(CusHAWBSchema.CS_HAWB, hAWB);
					findDuplicateHAWBInMemory.AddToFilter(CusHAWBSchema.CS_CM, houseBill.CS_CM);
					findDuplicateHAWBInMemory.AddToFilter(CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, houseBill.PK);
					var duplicatedHawbsInMemory = factory.Load<CusHAWB>(findDuplicateHAWBInMemory);
					duplicatedHawbs.AddRange(duplicatedHawbsInMemory);

					foreach (var hawb in duplicatedHawbs)
					{
						if (coLoadHAWB.EqualsIgnoringCase(hawb.AggregatedCoLoadMaster))
						{
							houseBillsWithDuplicateKeys.Append(hawb.CS_MessageReference + ",");
							if (!hasOtherHouseBillsPreAlertedOrMessagePending)
							{
								hasOtherHouseBillsPreAlertedOrMessagePending = hawb.CS_IsPrealerted || hawb.CS_IsResponsePending;
							}
						}
					}

					var message = new StringBuilder();
					if (houseBillsWithDuplicateKeys.Length > 0)
					{
						message.Append("These are the house bills that have duplicate combination of MAWB and HAWB");
						if (!coLoadHAWB.IsEmpty)
						{
							message.Append(" and Co-Load Master");
						}
						message.Append("\r\n" + houseBillsWithDuplicateKeys.ToString().TrimEnd(','));
					}

					if (hasOtherHouseBillsPreAlertedOrMessagePending)
					{
						MessageError = message.ToString();
					}
					else
					{
						Warning = message.ToString();
					}
				}
			}
		}

		protected virtual bool IsGoodsCurrencyRequired
		{
			get { return false; }
		}

		ForwardingShipment fShipment;
		protected ForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Parent.Factory.Load<ForwardingShipment>(Parent.CS_JS);
				}
				return fShipment;
			}
		}

		public new CusHAWB HAWB
		{
			get { return (CusHAWB)base.HAWB; }
		}

		readonly HAWBAndCoLoadHAWBValidator duplicateHAWBAndCoLoadValidator;
	}
}
