using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public sealed class CusGoodsLocation : EU.Business.CusGoodsLocation, Integration.Customs.NL.ICusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

	protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

	protected override bool CGL_QualifierReadonlyCore => IsInAuthorisationMode || (Parent is CusEntryInstruction parent && parent.JobDeclaration.IsExport);

	protected override bool CGL_TypeReadonlyCore => IsInAuthorisationMode;

	public override bool ContactPersonDataVisible => !IsInAuthorisationMode && base.ContactPersonDataVisible;

	public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

	protected override Type AddressType => typeof(CusGoodsLocationAddress);

	public new CusGoodsLocationValidation Validation => (CusGoodsLocationValidation)base.Validation;

	protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

	protected override void SetDefaultsForNew()
	{
		base.SetDefaultsForNew();
		if (Parent is CusEntryInstruction parent)
		{
			var jobDeclaration = parent.JobDeclaration;
			if (parent.CusAuthorizationUsages.Any())
			{
				CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
				CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			}
			else if (jobDeclaration.IsExport)
			{
				SetDefaultsForNew_ForExportDeclaration(parent, jobDeclaration);
			}
		}
		else if (IsInAuthorisationMode)
		{
			CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
			CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			Address.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		}
	}

	public override ZString DisplayText
	{
		get
		{
			var returnValue = ZString.Empty;
			if (!IsInAuthorisationMode)
			{
				returnValue = base.DisplayText;
			}
			else
			{
				var zStringBuilder = new ZStringBuilder();
				zStringBuilder.Append(CGL_Qualifier);
				zStringBuilder.Append(CGL_Type);
				zStringBuilder.Append(Address.E2_Postcode);
				zStringBuilder.Append(CGL_AdditionalIdentifier);
				zStringBuilder.Append(Address.E2_RN_NKCountryCode);
				returnValue = zStringBuilder.ToStringWithDelimiterBetweenAppends(";");
			}

			return returnValue;
		}
	}

	public ZBool IsInAuthorisationMode => CGL_ParentTableCode == Customs.Business.CusGoodsLocationUseList.Codes.CustomsPermitRule;

	public void SetDefaultsFromAuthorizationIfNeeded(Customs.Business.CusAuthorisationHeader cusAuthorisationHeader)
	{
		if (cusAuthorisationHeader != null
			&& cusAuthorisationHeader.CPH_Type.In<ZString>(NLCusAuthorisationHeaderTypeList.Codes.CentralizedClearance, NLCusAuthorisationHeaderTypeList.Codes.CustomsWarehousingCW2, NLCusAuthorisationHeaderTypeList.Codes.CustomsWarehousingCWP,
														   NLCusAuthorisationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, NLCusAuthorisationHeaderTypeList.Codes.InwardProcessing, NLCusAuthorisationHeaderTypeList.Codes.OutwardProcessing)
			&& cusAuthorisationHeader.CusAuthorisationRules.Count(x => x.CPR_RuleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location) == 1)
		{
			var location = Factory.Load<CusAuthorisationRule>(cusAuthorisationHeader.CusAuthorisationRules.First(x => x.CPR_RuleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location).PK).GoodsLocation;

			CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
			CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			if (CGL_AdditionalIdentifier.IsEmpty)
			{
				CGL_AdditionalIdentifier = location.CGL_AdditionalIdentifier;
			}
			if (Address.E2_Postcode.IsEmpty)
			{
				Address.E2_Postcode = location.Address.E2_Postcode;
			}
			if (Address.E2_RN_NKCountryCode.IsEmpty)
			{
				Address.E2_RN_NKCountryCode = location.Address.E2_RN_NKCountryCode;
			}
		}
	}

	void SetDefaultsForNew_ForExportDeclaration(CusEntryInstruction parent, JobDeclaration jobDeclaration)
	{
		CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
		CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.DesignatedLocation;
		if (!parent.CEI_OA_Warehouse.IsEmpty)
		{
			CGL_AdditionalIdentifier = parent.Warehouse.Address1;
			Address.E2_Postcode = parent.Warehouse.Postcode;
			Address.E2_RN_NKCountryCode = parent.Warehouse.Country.Code;
		}
		else if (parent.CEI_OA_Warehouse.IsEmpty && !jobDeclaration.SupplierDocumentaryAddress.OrganisationPK.IsEmpty)
		{
			CGL_AdditionalIdentifier = jobDeclaration.SupplierDocumentaryAddress.Address1;
			Address.E2_Postcode = jobDeclaration.SupplierDocumentaryAddress.Postcode;
			Address.E2_RN_NKCountryCode = jobDeclaration.SupplierDocumentaryAddress.Country.Code;
		}
	}
}
