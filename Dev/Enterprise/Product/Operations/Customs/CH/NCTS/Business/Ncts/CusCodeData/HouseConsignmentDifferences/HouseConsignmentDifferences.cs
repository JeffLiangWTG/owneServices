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

public sealed class HouseConsignmentDifferences : SingleCusCodeData
{
	public new class Schema : CusCodeData.Schema
	{
		public new const int CY_CodeMaxLength = 2;
	}

	public HouseConsignmentDifferences(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(NctsBill));

	public new NctsBill Parent => (NctsBill)base.Parent;

	protected override Customs.Business.CusCodeDataLookups GetNewLookups() => new HouseConsignmentDifferencesLookups(this);

	public new HouseConsignmentDifferencesLookups Lookups => (HouseConsignmentDifferencesLookups)base.Lookups;

	protected override CusCodeDataValidation GetNewValidation() => new HouseConsignmentDifferencesValidation(this);

	public new HouseConsignmentDifferencesValidation Validation => (HouseConsignmentDifferencesValidation)base.Validation;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		CY_ParentTableCode = CusInBondBillSchema.Constants.Prefix;
		CY_Type = CH.Business.CusCodeDataTypeList.Codes.UnloadingRemarks;
	}

	protected override IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
	{
		yield return CY_CodeInfo;
		yield return CY_DataInfo;
	}

	#region Properties

	public bool IsNotMissingGoodsItem => (Parent?.MovementDetail?.B9_UnloadedState ?? ZString.Empty) != NctsUnloadedStateList.Codes.MIS;

	[List(nameof(Lookups) + "." + nameof(HouseConsignmentDifferencesLookups.CY_CodeList))]
	[MaxLength(Schema.CY_CodeMaxLength)]
	[ResourceStringData("Enterprise.Customs.CH.Business.HouseConsignmentDifferences|CY_Code", Caption = "Unloading Code")]
	[ReadOnlyMember(nameof(IsNotMissingGoodsItem))]
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

	[ResourceStringData("Enterprise.Customs.CH.Business.HouseConsignmentDifferences|CY_Data", Caption = "Unloading Remarks")]
	[ReadOnlyMember(nameof(IsNotMissingGoodsItem))]
	public override ZString CY_Data
	{
		get => base.CY_Data;
		set => base.CY_Data = value;
	}
	#endregion
}
