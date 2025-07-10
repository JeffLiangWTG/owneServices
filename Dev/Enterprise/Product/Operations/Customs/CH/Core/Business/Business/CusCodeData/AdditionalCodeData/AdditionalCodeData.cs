using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public class AdditionalCodeData : SingleCusCodeData
{
	public AdditionalCodeData(BusinessObjectFactory factory, DataRow row)
	: base(factory, row) { }

	protected override TypeLoaderCollection parentLoaders
	{
		get { return new TypeLoaderCollection(typeof(JobComInvoiceLine)); }
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CY_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		CY_Type = CusCodeDataTypeList.Codes.AdditionalCode;
	}

	protected override CusCodeDataValidation GetNewValidation() => new AdditionalCodeDataValidation(this);

	public new AdditionalCodeDataValidation Validation => (AdditionalCodeDataValidation)base.Validation;

	public override bool SupportsNotes => false;

	public override ZString CY_ParentTableCode
	{
		get => base.CY_ParentTableCode;
		set
		{
			var oldValue = CY_ParentTableCode;
			base.CY_ParentTableCode = value;
			if (!IsCopying && oldValue != CY_ParentTableCode)
			{
				Parent?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CY_Type
	{
		get => base.CY_Type;
		set
		{
			var oldValue = CY_Type;
			base.CY_Type = value;
			if (!IsCopying && oldValue != CY_Type)
			{
				Parent?.MarkAsNeedingValidation();
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
				Parent?.MarkAsNeedingValidation();
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
				Parent?.MarkAsNeedingValidation();
			}
		}
	}

	protected override IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
	{
		yield return CY_CodeInfo;
		yield return CY_IsOverriddenInfo;
	}
}
