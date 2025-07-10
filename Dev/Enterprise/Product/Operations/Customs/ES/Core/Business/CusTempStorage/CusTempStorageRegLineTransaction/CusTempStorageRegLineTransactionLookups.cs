using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Core;
using CusTempStorageRegPremisesTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class CusTempStorageRegLineTransactionLookups : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionLookups
	{
		public CusTempStorageRegLineTransactionLookups(CusTempStorageRegLineTransaction parent) : base(parent)
		{
		}

		public new CusTempStorageRegLineTransaction Parent => (CusTempStorageRegLineTransaction)base.Parent;

		public override CodeDescriptionPairList InternalReferenceTypeList => Factory.GetCachedValue("ES.InternalReferenceTypeList_" + (Parent.RegLine.RegHeader.Premises?.SRP_Type ?? ZString.Empty), () => GetInternalReferenceTypeList());

		CodeDescriptionPairList GetInternalReferenceTypeList()
		{
			var premisesType = Parent.RegLine.RegHeader.Premises?.SRP_Type ?? ZString.Empty;
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
	}
}
