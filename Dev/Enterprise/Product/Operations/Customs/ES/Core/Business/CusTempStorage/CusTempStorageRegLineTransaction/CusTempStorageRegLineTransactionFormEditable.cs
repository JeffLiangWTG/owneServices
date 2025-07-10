using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using CusTempStorageRegLineTransactionInternalReferenceTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionInternalReferenceTypeList;
using CusTempStorageRegLineTransactionReferenceTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionReferenceTypeList;
using CusTempStorageRegPremisesTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class CusTempStorageRegLineTransactionFormEditable : AutoCusTempStorageRegLineTransactionFormEditable
{
	public CusTempStorageRegLineTransactionFormEditable(CusTempStorageRegLine regLine)
	{
		RegLine = regLine;
	}
	readonly internal CusTempStorageRegLine RegLine;

	public override ZDecimal SRT_GrossWeight
	{
		get => base.SRT_GrossWeight;
		set
		{
			var oldValue = SRT_GrossWeight;
			base.SRT_GrossWeight = value;
			if (oldValue != SRT_GrossWeight && !IsValidationSuspended)
			{
				Validation.ValidateSRT_PackageQty();
			}
		}
	}

	public override ZInt SRT_PackageQty
	{
		get => base.SRT_PackageQty;
		set
		{
			var oldValue = SRT_PackageQty;
			base.SRT_PackageQty = value;
			if (oldValue != SRT_PackageQty && !IsValidationSuspended)
			{
				Validation.ValidateSRT_GrossWeight();
			}
		}
	}

	[List(nameof(InternalReferenceTypeList))]
	public override ZString SRT_InternalReferenceType { get => base.SRT_InternalReferenceType; set => base.SRT_InternalReferenceType = value; }

	[List(nameof(ReferenceTypeList))]
	public override ZString SRT_ReferenceType { get => base.SRT_ReferenceType; set => base.SRT_ReferenceType = value; }

	public CodeDescriptionPairList InternalReferenceTypeList => RegLine.Factory.GetCachedValue("ES.InternalReferenceTypeList_" + RegLine.RegHeader.Premises.SRP_Type, GetInternalReferenceTypeList);

	CodeDescriptionPairList GetInternalReferenceTypeList()
	{
		var premisesType = RegLine.RegHeader.Premises.SRP_Type;
		CodeDescriptionPairList result = new CusTempStorageRegLineTransactionInternalReferenceTypeList();
		if (premisesType == CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility)
		{
			result.RemoveCode(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			result.RemoveCode(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			result.RemoveCode(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			result.RemoveCode(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements);
			result.RemoveCode(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.H7LowValue);
			result.RemoveCode(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration);
			result.RemoveCode(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry);
		}
		else if (premisesType == CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse)
		{
			result.RemoveCode(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			result.RemoveCode(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.T2lDeclaration);
			result.RemoveCode(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry);
		}
		result.Sort();
		return result;
	}

	public CodeDescriptionPairList ReferenceTypeList => RegLine.Factory.GetCachedValue<CusTempStorageRegLineTransactionReferenceTypeList>();
}
