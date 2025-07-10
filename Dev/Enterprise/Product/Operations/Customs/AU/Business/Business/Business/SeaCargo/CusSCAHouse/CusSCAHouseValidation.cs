using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseValidation : Customs.Business.CusSCAHouseValidation
	{
		public CusSCAHouseValidation(Customs.Business.BaseCusSCAHouse parent)
			: base(parent)
		{
		}

		protected new CusSCAHouse Parent
		{
			get { return (CusSCAHouse)base.Parent; }
		}

		protected ZString HouseBillNumberForErrorMessage
		{
			get
			{
				if (Parent.CA_HouseBill.IsEmpty)
				{
					return "House Bill Number not set";
				}
				return Parent.CA_HouseBill;
			}
		}

		public void RunPreUnderbondValidation()
		{
			preUnderbondValidationRunning = true;
			ValidateCA_MoveUnderbondFrom();
			ValidateCA_MoveUnderbondTo();
			preUnderbondValidationRunning = false;
		}
		bool preUnderbondValidationRunning;

		protected override void CheckCA_HouseBill()
		{
			base.CheckCA_HouseBill();
			MessageValidation.CheckEntered(Parent.CA_HouseBillInfo);
			if (Parent.Pivot.Count == 0)
			{
				Parent.CA_HouseBillInfo.AddMessageError(ContainerCountMessageError + " " + HouseBillNumberForErrorMessage);
			}

			var houseBill = Parent.CA_HouseBill;
			if (!houseBill.IsEmpty)
			{
				var oceanBill = Parent.OceanBill;
				if (oceanBill != null && oceanBill.HouseBills.ByHouseBill[houseBill].Count() > 1)
				{
					Parent.CA_HouseBillInfo.AddMessageError(CusSCAHouse.DuplicateHouseBillNumber + oceanBill.CB_OceanBill);
				}
			}
			new CustomsValidation(Parent.CA_HouseBillInfo).ErrorOnKeyDataWithNoChildren(Parent.CA_MessageStatus);
		}

		protected override void CheckCA_PrepaidCollectOther()
		{
			base.CheckCA_PrepaidCollectOther();
			MessageValidation.CheckEntered(Parent.CA_PrepaidCollectOtherInfo, "Payment type is required on house bill: " + HouseBillNumberForErrorMessage);
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_PrepaidCollectOtherInfo, Parent.Lookups.MethodsOfPayment);
		}

		protected override void CheckCA_RL_NK_PortOfOrigin()
		{
			base.CheckCA_RL_NK_PortOfOrigin();
			MessageValidation.CheckEntered(Parent.CA_RL_NK_PortOfOriginInfo, "Port of origin is required on house bill: " + HouseBillNumberForErrorMessage);
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_RL_NK_PortOfOriginInfo, Parent.PortOfOriginList);
		}

		protected override void CheckCA_RL_NK_PortOfDestination()
		{
			base.CheckCA_RL_NK_PortOfDestination();
			MessageValidation.CheckEntered(Parent.CA_RL_NK_PortOfDestinationInfo, "Port of destination is required on house bill: " + HouseBillNumberForErrorMessage);
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_RL_NK_PortOfDestinationInfo, Parent.PortOfDestinationList);
		}

		protected override void CheckCA_RN_NKGoodsOrigin()
		{
			base.CheckCA_RN_NKGoodsOrigin();
			MessageValidation.CheckEntered(Parent.CA_RN_NKGoodsOriginInfo, "Goods origin is required on house bill: " + HouseBillNumberForErrorMessage);
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_RN_NKGoodsOriginInfo, Parent.CountryOfOriginList);
			if (Parent.CA_RN_NKGoodsOrigin.StartsWith(Enterprise.Core.Constants.CountryCodes.Australia))
			{
				Parent.CA_RN_NKGoodsOriginInfo.AddMessageError("Australia is an invalid goods origin.");
			}
		}

		#region Consignor

		protected override void CheckCA_ConsignorName()
		{
			base.CheckCA_ConsignorName();
			MessageValidation.CheckEntered(Parent.CA_ConsignorNameInfo, "Consignor name is required on house bill: " + HouseBillNumberForErrorMessage);
		}

		#endregion

		#region Consignee

		protected override void CheckCA_ConsigneeName()
		{
			base.CheckCA_ConsigneeName();
			MessageValidation.CheckEntered(Parent.CA_ConsigneeNameInfo, "Consignee name is required on house bill: " + HouseBillNumberForErrorMessage);
		}

		#endregion

		public const string ContainerCountMessageError = "At least one set of container details must be entered for this consignment:";

		public MessageValidation MessageValidation
		{
			get { return new MessageValidation(Parent); }
		}

		protected override void CheckCA_MoveUnderbondFrom()
		{
			base.CheckCA_MoveUnderbondFrom();
			new EstablishmentCodeValidation(Parent).ValidateEstablishmentCode(Parent.CA_MoveUnderbondFromInfo);
			if (preUnderbondValidationRunning && Parent.CA_MoveUnderbondFrom.Length != 5)
			{
				if (Parent.CA_OA_UnderbondFrom.IsValid)
				{
					Parent.CA_MoveUnderbondFromInfo.AddMessageError("The Premise ID must have a length of 5.  Use F3/F4 to edit the selected organisation and modify the Customs Controlled Premise (CCP) code from the config tab");
				}
				else
				{
					Parent.CA_MoveUnderbondFromInfo.AddMessageError("The Premise ID must have a length of 5");
				}
			}
		}

		protected override void CheckCA_MoveUnderbondTo()
		{
			base.CheckCA_MoveUnderbondTo();
			new EstablishmentCodeValidation(Parent).ValidateEstablishmentCode(Parent.CA_MoveUnderbondToInfo);
			if (preUnderbondValidationRunning && Parent.CA_MoveUnderbondTo.Length != 5)
			{
				if (Parent.CA_OA_UnderbondTo.IsValid)
				{
					Parent.CA_MoveUnderbondToInfo.AddMessageError("The Premise ID must have a length of 5.  Use F3/F4 to edit the selected organisation and modify the Customs Controlled Premise (CCP) code from the config tab");
				}
				else
				{
					Parent.CA_MoveUnderbondToInfo.AddMessageError("The Premise ID must have a length of 5");
				}
			}
		}

		protected override void CheckCA_ConsigneeBusinessNumber()
		{
			base.CheckCA_ConsigneeBusinessNumber();
			CargoHelper.CheckBusinessNumberOrIdentifierShouldBeEntered(Parent.CA_ConsigneeBusinessNumberInfo, Parent.CA_ConsigneeBusinessNumber, Parent.CA_ConsigneeIdentifier);
		}

		protected override void CheckCA_ConsigneeIdentifier()
		{
			base.CheckCA_ConsigneeIdentifier();
			CargoHelper.CheckBusinessNumberOrIdentifierShouldBeEntered(Parent.CA_ConsigneeIdentifierInfo, Parent.CA_ConsigneeBusinessNumber, Parent.CA_ConsigneeIdentifier);
		}

		protected override void CheckCA_JS()
		{
			base.CheckCA_JS();

			var duplicate = Parent.FindDuplicate();
			if (duplicate != null)
			{
				Parent.CA_JSInfo.AddError($"An existing House Bill (created by {duplicate.CA_SystemCreateUser} at {duplicate.CA_SystemCreateTimeUtc.ToLocalBranchTime().ToStandardDateTimeString()}) is already linking the Shipment to this Sea Cargo.\r\nTo resolve the problem, delete this House Bill or close and re-open the Consol.");
			}
		}

		protected override void CheckCA_IsMasterHouse()
		{
			base.CheckCA_IsMasterHouse();
			if (Parent.CA_IsMasterHouse && Parent.Pivot.HasAnElementSelfAssessed)
			{
				Parent.CA_IsMasterHouseInfo.AddMessageError("This is a co-load master and it cannot be self-assessed.");
			}

			foreach (CusSCAPivot pivot in Parent.Pivot)
			{
				pivot.Validation.ValidateCV_IsSAC();
			}
		}

		protected override void CheckCA_ConsigneeAddress1()
		{
			base.CheckCA_ConsigneeAddress1();
			MessageValidation.CheckEntered(Parent.CA_ConsigneeAddress1Info, "Consignee address line 1 required on house bill: " + HouseBillNumberForErrorMessage);
		}

		protected override void CheckCA_ConsignorAddress1()
		{
			base.CheckCA_ConsignorAddress1();
			MessageValidation.CheckEntered(Parent.CA_ConsignorAddress1Info, "Consignor address line 1 required on house bill: " + HouseBillNumberForErrorMessage);
		}

		protected override void CheckCA_NotifyAddress1()
		{
			base.CheckCA_NotifyAddress1();
			if (!Parent.CA_NotifyName.IsEmpty)
			{
				MessageValidation.CheckEntered(Parent.CA_NotifyAddress1Info, "Notify address is required when notify name is entered on house bill: " + HouseBillNumberForErrorMessage);
			}
		}

		protected override void CheckCA_RN_NKNotifyCountryCode()
		{
			base.CheckCA_RN_NKNotifyCountryCode();
			if (!Parent.CA_NotifyName.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_RN_NKNotifyCountryCodeInfo, Parent.Lookups.NotifyPartyCountryCodes, "Notify Party country/region code required on house bill: " + HouseBillNumberForErrorMessage);
			}
		}

		protected override void CheckCA_RN_NKConsigneeCountryCode()
		{
			base.CheckCA_RN_NKConsigneeCountryCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_RN_NKConsigneeCountryCodeInfo, Parent.Lookups.ConsigneeCountryCodes, "Consignee country/region code required on house bill: " + HouseBillNumberForErrorMessage);
		}

		protected override void CheckCA_RN_NKConsignorCountryCode()
		{
			base.CheckCA_RN_NKConsignorCountryCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_RN_NKConsignorCountryCodeInfo, Parent.Lookups.ConsignorCountryCodes, "Consignor country/region code required on house bill: " + HouseBillNumberForErrorMessage);
		}

		protected override void CheckCA_ResponsiblePartyID()
		{
			base.CheckCA_ResponsiblePartyID();
			new CustomsValidation(Parent.CA_ResponsiblePartyIDInfo).ErrorOnKeyDataWithNoChildren(Parent.CA_MessageStatus);
		}
	}
}
