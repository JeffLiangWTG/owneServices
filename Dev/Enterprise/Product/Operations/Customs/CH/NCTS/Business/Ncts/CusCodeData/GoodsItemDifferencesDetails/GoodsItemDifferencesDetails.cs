using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class GoodsItemDifferencesDetails : SingleCusCodeData
{
	public new class Schema : CusCodeData.Schema
	{
		public new const int CY_CodeMaxLength = 2;
	}

	public GoodsItemDifferencesDetails(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(NctsArrivalCargoDesc));

	public new NctsArrivalCargoDesc Parent => (NctsArrivalCargoDesc)base.Parent;

	protected override Customs.Business.CusCodeDataLookups GetNewLookups() => new GoodsItemDifferencesDetailsLookups(this);

	public new GoodsItemDifferencesDetailsLookups Lookups => (GoodsItemDifferencesDetailsLookups)base.Lookups;

	protected override CusCodeDataValidation GetNewValidation() => new GoodsItemDifferencesDetailsValidation(this);

	public new GoodsItemDifferencesDetailsValidation Validation => (GoodsItemDifferencesDetailsValidation)base.Validation;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		CY_ParentTableCode = CusInBondCargoDescSchema.Constants.Prefix;
		CY_Type = CH.Business.CusCodeDataTypeList.Codes.UnloadingRemarks;
	}

	protected override IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
	{
		yield return CY_CodeInfo;
		yield return CY_DataInfo;
	}

	#region Properties

	public bool IsUnloadingStateOfGoodsItemDeclared => Parent != null && Parent.BY_UnloadedState == NctsUnloadedStateList.Codes.DEC;

	[List(nameof(Lookups) + "." + nameof(GoodsItemDifferencesDetailsLookups.CY_CodeList))]
	[MaxLength(Schema.CY_CodeMaxLength)]
	[ResourceStringData("Enterprise.Customs.CH.Business.GoodsItemDifferencesDetails|CY_Code", Caption = "Unloading Code")]
	[ReadOnlyMember(nameof(IsUnloadingStateOfGoodsItemDeclared))]
	public override ZString CY_Code
	{
		get => base.CY_Code;
		set
		{
			base.CY_Code = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateCY_Code();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.GoodsItemDifferencesDetails|CY_Data", Caption = "Unloading Remarks")]
	[ReadOnlyMember(nameof(IsUnloadingStateOfGoodsItemDeclared))]
	public override ZString CY_Data
	{
		get => base.CY_Data;
		set => base.CY_Data = value;
	}
	#endregion
}
