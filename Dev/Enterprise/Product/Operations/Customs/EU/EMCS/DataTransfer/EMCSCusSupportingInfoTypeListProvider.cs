using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.DataTransfer
{
	public class EMCSCusSupportingInfoTypeListProvider
	{
		public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode)
		{
			ICodeDescriptionPairList result;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = EMCSJobDeclarationTableSpecificCusSupportingInfoTypeList;
					break;
				//case JobComInvoiceLineSchema.Constants.Prefix:
				//	result = EMCSInvoiceLineTableSpecificCusSupportingInfoTypeList;
				//	break;
				default:
					result = null;
					break;
			}
			return result;
		}

		CodeDescriptionPairList EMCSJobDeclarationTableSpecificCusSupportingInfoTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(CusSupportingInfoTypeList.Codes.ImportSad, CusSupportingInfoTypeList.Descriptions.ImportSad);
				result.AddPair(CusSupportingInfoTypeList.Codes.Certificate, CusSupportingInfoTypeList.Descriptions.Certificate);
				return result;
			}
		}

		//	CodeDescriptionPairList EMCSInvoiceLineTableSpecificCusSupportingInfoTypeList
		//	{
		//		get
		//		{
		//			var result = new CodeDescriptionPairList();
		//			result.AddPair(CusSupportingInfoTypeList.Codes.Packages, CusSupportingInfoTypeList.Descriptions.Packages);
		//			return result;
		//		}
		//	}
	}
}
