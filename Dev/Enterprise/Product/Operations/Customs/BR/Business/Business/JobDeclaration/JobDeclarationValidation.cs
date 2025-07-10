using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class JobDeclarationValidation : AutoBRJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration
		{
			get { return Parent; }
		}

		protected new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		protected JobDeclarationLookups Lookups => Parent.Lookups;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEntranceOfficeCode();
			ValidateBoardingOfficeCode();
			ValidateBoardingEnclosureCode();
			ValidateDeclarantType();
			ValidateOperationType();
			ValidatePaymentBankAccount();
			ValidateVesselCountry();
			ValidateBRTransportMode();
		}

		public void ValidateEntranceOfficeCode()
		{
			ValidateCalculatedProperty(Parent.EntranceOfficeCodeInfo);
		}

		protected virtual void CheckEntranceOfficeCode()
		{
		}

		public void ValidateBoardingOfficeCode()
		{
			ValidateCalculatedProperty(Parent.BoardingOfficeCodeInfo);
		}

		protected virtual void CheckBoardingOfficeCode()
		{
		}

		public void ValidateBoardingEnclosureCode()
		{
			ValidateCalculatedProperty(Parent.BoardingEnclosureCodeInfo);
		}

		protected virtual void CheckBoardingEnclosureCode()
		{
		}

		public void ValidateOperationType()
		{
			ValidateCalculatedProperty(Parent.OperationTypeInfo);
		}

		protected virtual void CheckOperationType()
		{
		}

		public void ValidateDeclarantType()
		{
			ValidateCalculatedProperty(Parent.DeclarantTypeInfo);
		}

		protected virtual void CheckDeclarantType()
		{
		}

		public void ValidatePaymentBankAccount()
		{
			ValidateCalculatedProperty(Parent.PaymentBankAccountPKInfo);
		}

		protected virtual void CheckPaymentBankAccountPK()
		{
		}

		public void ValidateVesselCountry()
		{
			ValidateCalculatedProperty(Parent.VesselCountryInfo);
		}

		protected virtual void CheckVesselCountry()
		{
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo);
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOfGoodsInfo);
		}

		protected override void CheckJE_MessageTypeIsEnteredOrValid()
		{
			if (!Parent.JE_MessageTypeInfo.HasErrors())
			{
				base.CheckJE_MessageTypeIsEnteredOrValid();
			}
		}

		protected override void CheckJE_OH_ConsigneeIsValidZGuid()
		{
			if (!Parent.JE_OH_Consignee.IsEmpty && !Parent.JE_OH_Consignee.IsValid)
			{
				Parent.JE_OH_ConsigneeInfo.AddError(Res.GetString("6A5F3806-0D00-4E98-9824-E4F5049760AB", "The selected {0} is not valid. Please choose a new {0} or amend it using F3.", Parent.ConsigneeCaption.Caption));
			}
		}

		protected override void CheckJE_ContainerMode()
		{
			if (Parent.ContainerModeVisible)
			{
				base.CheckJE_ContainerMode();
			}
		}

		protected override INotificationType NotificationTypeForContainerModeMandatory => CargoWise.EntityFramework.NotificationType.Warning;

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			ValidateVesselCountry();
		}

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();

			if (!Parent.IsImportSiscomex && Parent.AttachedImportLicenseEntries.Count > 0)
			{
				Parent.JE_MessageTypeInfo.AddError(Res.GetString("BAAF5CEF-B368-4F20-A182-82060155095C", "Licenses should not be attached to any jobs but ISW. Please either detach the Licenses(s) or keep the Shipment Type as ISW."));
			}
		}

		protected void CheckRegistrationNumberEntered(ZPropertyInfo targetInfo, OrgRegistrationNumber registrationNumber, string orgName = null)
		{
			if (registrationNumber != null && registrationNumber.Number.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("A8A12028-1C44-43CB-B07F-588BCF7C4ED9", "{0} Registration Number not found.", orgName ?? targetInfo.HumanReadableName));
			}
		}

		protected void CheckRegistrationNumberIsCJN(ZPropertyInfo targetInfo, OrgRegistrationNumber registrationNumber, string orgName = null)
		{
			if (registrationNumber != null && registrationNumber.NumberType != BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ)
			{
				targetInfo.AddMessageError(Res.GetString("0040D97D-7DBF-45A1-98D0-BA4AEB206B97", "The {0} Registration Number differs from CJN - Business Taxpayer Registration.", orgName ?? targetInfo.HumanReadableName));
			}
		}

		protected override void CheckJE_LocationQualifier()
		{
			base.CheckJE_LocationQualifier();

			if (Parent.IsExport)
			{
				Parent.ClearanceLocalInvolvedParty.Validation.ValidateAll();
			}
		}

		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.JE_ApplicationCodeInfo);
		}

		public void ValidateBRTransportMode()
		{
			ValidateCalculatedProperty(Parent.BRTransportModeInfo);
		}

		protected virtual void CheckBRTransportMode()
		{
			CheckBRTransportModeIsMandatory();
			ListValidation.ErrorIfInvalidCode(Parent.BRTransportModeInfo);
		}

		protected virtual void CheckBRTransportModeIsMandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BRTransportModeInfo);
		}

		protected override void CheckJE_TransportModeMandatory()
		{ }

		protected override void CheckJE_GS_NKCusAgent()
		{
			base.CheckJE_GS_NKCusAgent();

			if (Parent.IsImportOnly || Parent.IsExport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_GS_NKCusAgentInfo);
				if (!Parent.JE_GS_NKCusAgent.IsEmpty)
				{
					GlbExternalPasswordValidation_CCT.CheckValidCertificate(Parent.BrokerCertificate, Parent.JE_GS_NKCusAgentInfo);
				}
			}
		}
	}
}
