using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class ImportSiscomexJobDeclarationValidation : JobDeclarationValidation
	{
		public ImportSiscomexJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckEntranceOfficeCode()
		{
			base.CheckEntranceOfficeCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.EntranceOfficeCodeInfo);
		}

		protected override void CheckOperationType()
		{
			base.CheckOperationType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.OperationTypeInfo);
		}

		protected override void CheckDeclarantType()
		{
			base.CheckDeclarantType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.DeclarantTypeInfo);
		}

		protected override void CheckPaymentBankAccountPK()
		{
			base.CheckPaymentBankAccountPK();
			if (Parent.PaymentBankAccountPK.IsEmpty && Parent.JE_PaymentMethod == PaymentPartyCodeDescriptionList.Codes.Broker)
			{
				Parent.PaymentBankAccountPKInfo.AddMessageError(Res.GetString("EA850ADE-AB6A-49B7-9A49-7450B794B20A", "You have not entered a Bank Account in Registry > Brazil > Import (SISCOMEX Web) > Tax and Fee Payment Bank Account."));
			}
		}

		protected override void CheckVesselCountry()
		{
			base.CheckVesselCountry();
			if (Parent.IsTransportByWater && Parent.VesselCountry.IsEmpty)
			{
				Parent.VesselCountryInfo.AddMessageError(Res.GetString("2D13E1B0-FFA3-4507-805B-554696D4B47C", "Vessel Country not found."));
			}
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CustomsOfficeInfo);
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOfGoodsInfo);
		}

		protected override void CheckJE_SubLocationOfGoods()
		{
			if (Parent.Lookups.SubLocationOfGoodsList.Count > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_SubLocationOfGoodsInfo);
			}
		}

		protected override void CheckJE_OH_Consignee()
		{
			base.CheckJE_OH_Consignee();
			if (!Parent.JE_OH_ConsigneeReadOnly)
			{
				var targetInfo = Parent.JE_OH_ConsigneeInfo;
				var registrationNumber = Parent.IntermConsignee?.PrimaryRegistrationNumber;
				var orgName = Parent.ConsigneeCaption.Caption;

				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

				CheckRegistrationNumberEntered(targetInfo, registrationNumber, orgName);
				CheckRegistrationNumberIsCJN(targetInfo, registrationNumber, orgName);
			}
		}

		protected override void CheckJE_GoodsOrigin()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_GoodsOriginInfo);
			base.CheckJE_GoodsOrigin();
		}

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_MessageSubTypeInfo);
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			var targetInfo = Parent.JE_OH_ImporterInfo;
			var registrationNumber = Parent.Importer?.PrimaryRegistrationNumber;

			CheckRegistrationNumberEntered(targetInfo, registrationNumber);

			if (Parent.DeclarantType == DeclarantTypeList.Codes.LegalPerson)
			{
				CheckRegistrationNumberIsCJN(targetInfo, registrationNumber);
			}
			if (Parent.DeclarantType == DeclarantTypeList.Codes.NaturalPerson)
			{
				CheckRegistrationNumberIsCPF(targetInfo, registrationNumber);
			}
		}
		protected override void CheckBRTransportModeIsMandatory()
		{
			if (Parent.RequiresTransportDetails)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BRTransportModeInfo);
			}
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			if (Parent.IsTransportByWater)
			{
				CheckMandatoryIfRequiresDepartureDetails(Parent.JE_VesselNameInfo);
			}

			if (Parent.IsRoad && Declaration.RequiresDepartureDetails)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VesselNameInfo, (NoResString)"Vehicle Plate");
			}
		}

		protected override void CheckJE_OH_ShippingLine()
		{
			base.CheckJE_OH_ShippingLine();
			if (Parent.RequiresShippingLine)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ShippingLineInfo);
			}
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			if (!Parent.JE_TransportMode.IsEmpty)
			{
				CheckMandatoryIfRequiresDepartureDetails(Parent.JE_ExportDateInfo);
			}
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();
			if (!Parent.JE_TransportMode.IsEmpty)
			{
				CheckMandatoryIfRequiresDepartureDetails(Parent.JE_RL_NKPortOfLoadingInfo);
			}
		}

		protected override void CheckJE_HouseBill()
		{
			base.CheckJE_HouseBill();
			if (Parent.IsRoad && Parent.RequiresTransportDetails)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_HouseBillInfo);
			}
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();
			ValidationHelper.CheckPaymentMethod(Parent);
		}

		protected override void CheckJE_UCR()
		{
			base.CheckJE_UCR();
			if ((Parent.IsMail || Parent.IsTransportByWater) && Parent.RequiresTransportDetails)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_UCRInfo);
			}
		}

		protected override void CheckJE_DispatchModality()
		{
			base.CheckJE_DispatchModality();
			if (Declaration.RequiresDispatchModality)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_DispatchModalityInfo);
			}
		}

		protected override void CheckJE_CargoArrivalDocumentType()
		{
			if (Declaration.RequiresTransportDetails && Declaration.IsCargoArrivalDocumentApplicable)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CargoArrivalDocumentTypeInfo);
			}
		}

		protected override void CheckJE_CargoArrivalDocumentUtilization()
		{
			if (Declaration.RequiresTransportDetails && Declaration.IsCargoArrivalDocumentApplicable)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CargoArrivalDocumentUtilizationInfo);
			}
		}

		protected override void CheckJE_CargoArrivalDocumentNumber()
		{
			if (Declaration.RequiresTransportDetails && Declaration.IsCargoArrivalDocumentApplicable)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CargoArrivalDocumentNumberInfo);
			}
		}

		protected override void CheckJE_TotalWeight()
		{
			base.CheckJE_TotalWeight();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TotalWeightInfo);
		}

		void CheckMandatoryIfRequiresDepartureDetails(ZPropertyInfo propertyInfo)
		{
			if (Declaration.RequiresDepartureDetails)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		void CheckRegistrationNumberIsCPF(ZPropertyInfo targetInfo, OrgRegistrationNumber registrationNumber, string orgName = null)
		{
			if (registrationNumber != null && registrationNumber.NumberType != BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration)
			{
				targetInfo.AddMessageError(Res.GetString("0D48F3DA-F7A0-4A8B-ADD3-9749D918DE32", "The {0} Registration Number differs from CPF - Individual Taxpayer Registration.", orgName ?? targetInfo.HumanReadableName));
			}
		}
	}
}
