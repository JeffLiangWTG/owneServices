using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public class JobDeclarationValidation : AutoCHJobDeclarationValidation
{
	public JobDeclarationValidation(JobDeclaration parent)
		: base(parent)
	{
	}

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	internal PlausiValidation PlausiValidation => plausiValidation ??= PlausiValidation.New(Parent);
	PlausiValidation plausiValidation;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateDispatchCountryCode();
	}

	protected override void CheckJE_SpecificCircumstanceIndicator()
	{
		base.CheckJE_SpecificCircumstanceIndicator();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_SpecificCircumstanceIndicatorInfo);
		PlausiValidation.CheckNS30108(Parent.JE_SpecificCircumstanceIndicatorInfo, Parent);
	}

	protected override void CheckJE_VehicleType()
	{
		base.CheckJE_VehicleType();
		if (Parent.IsImport && Parent.IsRoad)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_VehicleTypeInfo);
		}
	}

	protected override void CheckJE_ClearanceLocation()
	{
		base.CheckJE_ClearanceLocation();

		if (Parent.IsImport)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_ClearanceLocationInfo);
		}
	}

	protected override void CheckJE_VATPaidBy()
	{
		base.CheckJE_VATPaidBy();

		if (Parent.IsImport)
		{
			var targetInfo = Parent.JE_VATPaidByInfo;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);

			if (!targetInfo.HasNotifications())
			{
				CheckPaidBy(targetInfo, Parent.JE_PaymentMethodInfo, () => Parent.VATPaidByAccountNo, OrgCusCode.SwissCodeTypes.CAV);
			}
		}
	}

	protected override void CheckJE_CustomsOffice()
	{
		base.CheckJE_CustomsOffice();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo);
		PlausiValidation.CheckR271(Parent.JE_CustomsOfficeInfo);
	}

	protected override void CheckJE_DeclarationLanguage()
	{
		base.CheckJE_DeclarationLanguage();
		MessageSendingDeclarationValidationHelper.CheckJE_DeclarationLanguage(Parent.JE_DeclarationLanguageInfo);
	}

	protected void CheckPaidBy(ZPropertyInfo payerPropertyInfo, ZPropertyInfo otherPayerPropertyInfo, Func<ZString> accountNumberGetter, string registrationNumberType)
	{
		ZString paidBy = payerPropertyInfo.Value.ToString();

		if (paidBy == DeclarationPayerList.Codes.Cash && otherPayerPropertyInfo.Value.ToString() != DeclarationPayerList.Codes.Cash)
		{
			payerPropertyInfo.AddMessageError(Res.GetString("22677bda-921e-48f3-8b55-a589aa5c5afb", @"If ""Cash"" is selected, ""Cash"" must also be selected for {0}.", otherPayerPropertyInfo.HumanReadableName));
		}

		var (name, organization) = Parent.GetPaymentOrganization(registrationNumberType, paidBy);
		if (!name.IsEmpty)
		{
			if (organization == null)
			{
				payerPropertyInfo.AddMessageError(Res.GetString("925CD72F-5FB9-432F-A009-0851634599E0", "You have not entered the corresponding address: {0}. Please enter the address or select another option.", name));
			}
			else if (accountNumberGetter().IsEmpty)
			{
				payerPropertyInfo.AddMessageError(Res.GetString("476ED59C-D654-4B52-B567-438CEE68F887", "{0} Account was not found. Please check {1} Type Registration Number in Config/Registration Numbers of the corresponding organization: {2}.", payerPropertyInfo.HumanReadableName, registrationNumberType, name));
			}
		}
	}

	public void ValidateDispatchCountryCode()
	{
		ValidateCalculatedProperty(Parent.DispatchCountryCodeInfo);
	}

	protected virtual void CheckDispatchCountryCode()
	{
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		base.CheckJE_RN_NKTransportNationality();

		ListValidation.MessageErrorIfInvalidCode(Parent.JE_RN_NKTransportNationalityInfo);
	}

	protected override void CheckJE_GS_NKCusAgent()
	{
		base.CheckJE_GS_NKCusAgent();

		if (Parent.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin && Parent.JE_MessageSubType != ActivationTypeList.Codes.Passar)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_GS_NKCusAgentInfo);
		}
	}

	protected void CheckJE_GS_NKCusAgent_Password()
	{
		if (Parent.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin && !Parent.JE_GS_NKCusAgent.IsEmpty)
		{
			var externalPassword = Parent.CHDPassword;
			if (externalPassword == null || externalPassword.GP_UserID.IsEmpty)
			{
				Parent.JE_GS_NKCusAgentInfo.AddMessageError(Res.GetString("0ba51857-e0db-4533-a201-ebec41bebb9b", "Declarant Number is not configured for the specified Broker. Please check Broker Credentials."));
			}
		}
	}

	protected override void CheckJE_TransportMode()
	{
		base.CheckJE_TransportMode();
		MessageSendingDeclarationValidationHelper.CheckJE_TransportMode(PlausiValidation, Parent.JE_TransportModeInfo, Parent);
	}

	protected override void CheckJE_MessageType()
	{
		base.CheckJE_MessageType();

		if (Parent.CustomsEntryHeaders.Any() && !Parent.IsExportDeclarationActivation)
		{
			var genPivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.NctsRelatedExportGenPivot);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, CusInBondMoveHeaderSchema.Constants.Prefix);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusEntryHeaderSchema.Constants.Prefix);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, Parent.CustomsEntryHeaders.FirstOrDefault().PK);
			var result = Parent.Factory.LoadTop1<GenPivot>(genPivotQuery);

			if (result != null)
			{
				Parent.JE_MessageTypeInfo.AddError(Res.GetString("1ED73983-20D4-4143-9C7E-DBB7F0CA9818", "You may not change the Shipment Type because the Entry has been attached to an NCTS Departure: {0}", Parent.JobNumber));
			}
		}
	}
}
