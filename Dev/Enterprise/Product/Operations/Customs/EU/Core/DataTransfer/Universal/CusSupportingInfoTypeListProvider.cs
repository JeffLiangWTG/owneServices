using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CusSupportingInfoTypeListProvider
	{
		public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext)
		{
			return TableSpecificCusSupportingInfoTypeListCore(tableCode, dataContext);
		}

		#region Implementation

		protected virtual ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeListCore(ZString tableCode, string dataContext)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
				case JobComInvoiceHeaderSchema.Constants.Prefix:
				case JobComInvoiceLineSchema.Constants.Prefix:
				case CusInBondCargoDescSchema.Constants.Prefix:
				case AsycudaManifestHeaderSchema.Constants.Prefix:
					result = GetListForList();
					break;
				case CusInBondHeaderSchema.Constants.Prefix:
					result = GetListForCusInBondHeader();
					break;
				case CusInBondMoveHeaderSchema.Constants.Prefix:
					result = GetListForCusInBondMoveHeader();
					break;
			}
			return result;
		}

		public static CodeDescriptionPairList GetListForList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusSupportingInfoTypeList.Codes.AdditionalInfo, CusSupportingInfoTypeList.Descriptions.AdditionalInfo);
			result.AddPair(CusSupportingInfoTypeList.Codes.PreviousDocument, CusSupportingInfoTypeList.Descriptions.PreviousDocument);
			result.AddPair(CusSupportingInfoTypeList.Codes.SupportingDocument, CusSupportingInfoTypeList.Descriptions.SupportingDocument);
			result.AddPair(CusSupportingInfoTypeList.Codes.GoodsVehicleMovementSystem, CusSupportingInfoTypeList.Descriptions.GoodsVehicleMovementSystem);
			return result;
		}

		public static CodeDescriptionPairList GetListForCusInBondHeader()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusSupportingInfoTypeList.Codes.AdditionalInfo, CusSupportingInfoTypeList.Descriptions.AdditionalInfo);
			result.AddPair(CusSupportingInfoTypeList.Codes.PreviousDocument, CusSupportingInfoTypeList.Descriptions.PreviousDocument);
			return result;
		}

		public static CodeDescriptionPairList GetListForCusInBondMoveHeader()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusSupportingInfoTypeList.Codes.SupportingDocument, CusSupportingInfoTypeList.Descriptions.SupportingDocument);
			return result;
		}

		#endregion
	}
}
