using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class OfficeCode : EuOfficeCode
{
	public OfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public JobDeclaration Declaration => Parent as JobDeclaration;

	public override ZString CY_Data
	{
		get => base.CY_Data;
		set
		{
			var oldValue = CY_Data;
			base.CY_Data = value;
			if (!IsCopying && oldValue != CY_Data)
			{
				ResetIsPartOfEuropeanUnion();
				MarkDeclarationAndTransitAsNeedingValidationIncludingChildrenIfNecessary();
				TriggerPortTaxRateRefreshIfOfficeOfExit();
			}
		}
	}

	public override ZString CY_Code
	{
		get => base.CY_Code;
		set
		{
			var oldValue = CY_Code;
			base.CY_Code = value;
			if (!IsCopying && oldValue != CY_Code)
			{
				MarkDeclarationAndTransitAsNeedingValidationIncludingChildrenIfNecessary();
			}
		}
	}

	public override ZGuid CY_ParentID
	{
		get => base.CY_ParentID;
		set
		{
			var oldValue = CY_ParentID;
			base.CY_ParentID = value;
			if (!IsCopying && oldValue != CY_ParentID)
			{
				MarkDeclarationAndTransitAsNeedingValidationIncludingChildrenIfNecessary();
			}
		}
	}

	public override ZString CY_ParentTableCode
	{
		get => base.CY_ParentTableCode;
		set
		{
			var oldValue = CY_ParentTableCode;
			base.CY_ParentTableCode = value;
			if (!IsCopying && oldValue != CY_ParentTableCode)
			{
				MarkDeclarationAndTransitAsNeedingValidationIncludingChildrenIfNecessary();
			}
		}
	}

	public ZBool IsOfficeOfTransit => CY_Code == EuOfficeCodesTypes.Codes.OfficeOfTransit;
	public ZBool IsOfficeOfDestination => CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination;

	public ZBool IsPartOfEuropeanUnion => (isPartOfEuropeanUnion ?? (isPartOfEuropeanUnion = GetIsPartOfEuropeanUnion())).Value;
	ZBool? isPartOfEuropeanUnion;

	public new OfficeCodeValidation Validation => (OfficeCodeValidation)base.Validation;

	protected override CusCodeDataValidation GetNewValidation() => new OfficeCodeValidation(this);

	ZBool GetIsPartOfEuropeanUnion()
	{
		if (!CY_Data.IsEmpty && Office != null)
		{
			return RefCountry.LoadFromCountryCode(Factory, Office.ZZD_CountryOrGrouping).IsPartOfEuropeanUnion;
		}

		return false;
	}

	void ResetIsPartOfEuropeanUnion() => isPartOfEuropeanUnion = null;

	void MarkDeclarationAndTransitAsNeedingValidationIncludingChildrenIfNecessary()
	{
		Declaration?.MarkAsNeedingValidation();
	}

	void TriggerPortTaxRateRefreshIfOfficeOfExit()
	{
		if (CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit)
		{
			Declaration.RefreshPortTaxRateBinding();
		}
	}
}
