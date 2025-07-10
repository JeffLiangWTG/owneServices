
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderValidation : BaseCusSeaManOBLHeaderValidation
	{
		public CusSeaManOBLHeaderValidation(CusSeaManOBLHeader parent)
			: base(parent)
		{
			this.header = parent;
		}

		protected override void CheckBO_RN_NKGoodsCountryOfOrigin()
		{
			base.CheckBO_RN_NKGoodsCountryOfOrigin();

			ListValidation.MessageErrorIfInvalidCode(header.BO_RN_NKGoodsCountryOfOriginInfo, header.Lookups.GoodsCountryOfOrigins);
			MessageValidation.CheckEntered(header.BO_RN_NKGoodsCountryOfOriginInfo);
		}

		protected override void CheckBO_OceanBill()
		{
			base.CheckBO_OceanBill();

			MessageValidation.CheckEntered(header.BO_OceanBillInfo);
		}

		protected override void CheckBO_PaymentMethod()
		{
			base.CheckBO_PaymentMethod();

			ListValidation.MessageErrorIfInvalidCode(header.BO_PaymentMethodInfo, header.Lookups.MethodsOfPayment);
			MessageValidation.CheckEntered(header.BO_PaymentMethodInfo);
		}

		protected override void CheckBO_RL_NKDischargePort()
		{
			base.CheckBO_RL_NKDischargePort();

			MessageValidation.CheckEntered(header.BO_RL_NKDischargePortInfo);
			ListValidation.MessageErrorIfInvalidCode(header.BO_RL_NKDischargePortInfo, header.Lookups.DischargePorts);
			ZString portValidation = MessageValidation.ValidatePortType(Parent.BO_RL_NKDischargePort, false, true);
			if (!portValidation.IsEmpty)
			{
				Parent.BO_RL_NKDischargePortInfo.AddNotification(NotificationType.Warning, portValidation);
			}
		}

		protected override void CheckBO_RL_NKOriginPort()
		{
			base.CheckBO_RL_NKOriginPort();

			MessageValidation.CheckEntered(header.BO_RL_NKOriginPortInfo);
			ListValidation.MessageErrorIfInvalidCode(header.BO_RL_NKOriginPortInfo, header.Lookups.OriginPorts);
			ZString portValidation = MessageValidation.ValidatePortType(Parent.BO_RL_NKOriginPort, false, true);
			if (!portValidation.IsEmpty)
			{
				Parent.BO_RL_NKOriginPortInfo.AddNotification(NotificationType.Warning, portValidation);
			}
		}

		protected override void CheckBO_FreightForwarderIndicator()
		{
			base.CheckBO_FreightForwarderIndicator();

			foreach (CusSeaManOBLDetail detail in header.Details)
			{
				detail.Validation.ValidateBD_SACIndicator();
			}
		}

		#region Consignee Validation

		protected override void CheckBO_ConsigneeName()
		{
			base.CheckBO_ConsigneeName();

			MessageValidation.CheckEntered(header.BO_ConsigneeNameInfo);
			if (!header.BO_ConsigneeName.IsEmpty && IsToOrder(header.BO_ConsigneeName))
			{
				header.BO_ConsigneeNameInfo.AddMessageError("TO ORDER is not a valid consignee, and will be rejected by Customs. Please enter in the correct consignee details.");
			}
		}

		protected override void CheckBO_ConsigneeAddress1()
		{
			base.CheckBO_ConsigneeAddress1();
			if (!ConsigneeHasOnePieceOfInfoApartFromName())
			{
				Parent.BO_ConsigneeAddress1Info.AddMessageError("Consignee details require at least one piece of information entered, apart from the name.");
			}

			if (!header.BO_ConsigneeAddress1.IsEmpty && IsToOrder(header.BO_ConsigneeAddress1))
			{
				header.BO_ConsigneeAddress1Info.AddMessageError("TO ORDER is not a valid consignee, and will be rejected by Customs. Please enter in the correct consignee details.");
			}
		}

		protected override void CheckBO_ConsigneeAddress2()
		{
			base.CheckBO_ConsigneeAddress2();
			ValidateBO_ConsigneeAddress1();
		}

		protected override void CheckBO_ConsigneeCity()
		{
			base.CheckBO_ConsigneeCity();
			ValidateBO_ConsigneeAddress1();
		}

		protected override void CheckBO_ConsigneePostCode()
		{
			base.CheckBO_ConsigneePostCode();
			ValidateBO_ConsigneeAddress1();
		}

		protected override void CheckBO_ConsigneeState()
		{
			base.CheckBO_ConsigneeState();
			ValidateBO_ConsigneeAddress1();
		}

		bool ConsigneeHasOnePieceOfInfoApartFromName()
		{
			return HasOneOf(Parent.BO_ConsigneeAddress1Info, Parent.BO_ConsigneeAddress2Info, Parent.BO_ConsigneeCityInfo, Parent.BO_ConsigneePostCodeInfo, Parent.BO_ConsigneeStateInfo);
		}

		#endregion

		#region Consignor Validation

		protected override void CheckBO_ConsignorName()
		{
			base.CheckBO_ConsignorName();

			MessageValidation.CheckEntered(header.BO_ConsignorNameInfo);
			if (!header.BO_ConsignorName.IsEmpty && IsToOrder(header.BO_ConsignorName))
			{
				header.BO_ConsignorNameInfo.AddMessageError("TO ORDER is not a valid consignor, and will be rejected by Customs. Please enter in the correct consignor details.");
			}
		}

		protected override void CheckBO_ConsignorAddress1()
		{
			base.CheckBO_ConsignorAddress1();
			if (!ConsignorHasOnePieceOfInfoApartFromName())
			{
				Parent.BO_ConsignorAddress1Info.AddMessageError("Consignor details require at least one piece of information entered, apart from the name.");
			}

			if (!header.BO_ConsignorAddress1.IsEmpty && IsToOrder(header.BO_ConsignorAddress1))
			{
				header.BO_ConsignorAddress1Info.AddMessageError("TO ORDER is not a valid consignor, and will be rejected by Customs. Please enter in the correct consignor details.");
			}
		}

		protected override void CheckBO_ConsignorAddress2()
		{
			base.CheckBO_ConsignorAddress2();
			ValidateBO_ConsignorAddress1();
		}

		protected override void CheckBO_ConsignorCity()
		{
			base.CheckBO_ConsignorCity();
			ValidateBO_ConsignorAddress1();
		}

		protected override void CheckBO_ConsignorPostCode()
		{
			base.CheckBO_ConsignorPostCode();
			ValidateBO_ConsignorAddress1();
		}

		protected override void CheckBO_ConsignorState()
		{
			base.CheckBO_ConsignorState();
			ValidateBO_ConsignorAddress1();
		}

		bool ConsignorHasOnePieceOfInfoApartFromName()
		{
			return HasOneOf(Parent.BO_ConsignorAddress1Info, Parent.BO_ConsignorAddress2Info, Parent.BO_ConsignorCityInfo, Parent.BO_ConsignorPostCodeInfo, Parent.BO_ConsignorStateInfo);
		}

		#endregion

		protected override void CheckBO_RL_NKDestinationPort()
		{
			base.CheckBO_RL_NKDestinationPort();

			bool hasTransshipmentUnderbond = false;
			foreach (CusSeaManOBLDetail detail in header.Details)
			{
				ICusUnderbondDependentCollectionParent underbondParentDetail = detail;

				foreach (CusUnderbond underbond in underbondParentDetail.Underbonds)
				{
					if (underbond.C4_MovementReason == CMRUnderbondRequestCodes.Codes.Transshipment)
					{
						hasTransshipmentUnderbond = true;
						break;
					}
				}

				if (hasTransshipmentUnderbond)
				{
					break;
				}
			}

			if (hasTransshipmentUnderbond && header.BO_RL_NKDestinationPort.StartsWith(Core.Constants.CountryCodes.Australia))
			{
				header.BO_RL_NKDestinationPortInfo.AddMessageError("There is a transhipment underbond movement under this ocean bill, the destination port must be an overseas port.");
			}
		}

		#region Implementation

		bool HasOneOf(params ZPropertyInfo[] infos)
		{
			foreach (ZPropertyInfo info in infos)
			{
				if (!info.Value.IsEmpty)
				{
					return true;
				}
			}

			return false;
		}

		bool IsToOrder(ZString candidateString)
		{
			bool result = false;
			ZString first;
			ZString second;

			if (candidateString.Length > 2 && candidateString[2] != ' ')
			{
				first = candidateString.SubstringSafe(0, 2);
				second = candidateString.SubstringSafe(2, 5);
			}
			else
			{
				ZString[] bits = candidateString.Split(' ');
				if (bits.Length < 2)
				{
					return false;
				}

				first = bits[0];
				second = bits[1];
			}
			result = toSoundex == OrgPatternLanguageSetting.Get(Core.Constants.Languages.English).GetSoundex(first);
			result &= orderSoundex == OrgPatternLanguageSetting.Get(Core.Constants.Languages.English).GetSoundex(second);

			return result;
		}

		static readonly string toSoundex = OrgPatternLanguageSetting.Get(Core.Constants.Languages.English).GetSoundex("TO");
		static readonly string orderSoundex = OrgPatternLanguageSetting.Get(Core.Constants.Languages.English).GetSoundex("ORDER");

		readonly CusSeaManOBLHeader header;

		#endregion
	}
}
