using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.CH;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.DataTransfer;

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
			case CusEntryInstructionSchema.Constants.Prefix:
				result = GetListForEntryInstruction();
				break;
			case JobComInvoiceHeaderSchema.Constants.Prefix:
				result = GetListForInvoiceHeader();
				break;
			case JobComInvoiceLineSchema.Constants.Prefix:
				result = GetListForInvoiceLine();
				break;
		}
		return result;
	}

	public static CodeDescriptionPairList GetListForEntryInstruction()
	{
		var result = new CodeDescriptionPairList();
		result.AddPair(CusSupportingInfoTypeList.Codes.PreviousDocument, CusSupportingInfoTypeList.Descriptions.PreviousDocument);
		result.AddPair(CusSupportingInfoTypeList.Codes.SupportingDocument, CusSupportingInfoTypeList.Descriptions.SupportingDocument);
		result.AddPair(CusSupportingInfoTypeList.Codes.TransportDocument, CusSupportingInfoTypeList.Descriptions.TransportDocument);
		result.AddPair(CusSupportingInfoTypeList.Codes.AdditionalInformation, CusSupportingInfoTypeList.Descriptions.AdditionalInformation);
		return result;
	}

	public static CodeDescriptionPairList GetListForInvoiceHeader()
	{
		var result = new CodeDescriptionPairList();
		result.AddPair(CusSupportingInfoTypeList.Codes.PreviousDocument, CusSupportingInfoTypeList.Descriptions.PreviousDocument);
		result.AddPair(CusSupportingInfoTypeList.Codes.SupportingDocument, CusSupportingInfoTypeList.Descriptions.SupportingDocument);
		result.AddPair(CusSupportingInfoTypeList.Codes.TransportDocument, CusSupportingInfoTypeList.Descriptions.TransportDocument);
		return result;
	}

	public static CodeDescriptionPairList GetListForInvoiceLine()
	{
		var result = new CodeDescriptionPairList();
		result.AddPair(CusSupportingInfoTypeList.Codes.PreviousDocument, CusSupportingInfoTypeList.Descriptions.PreviousDocument);
		result.AddPair(CusSupportingInfoTypeList.Codes.SupportingDocument, CusSupportingInfoTypeList.Descriptions.SupportingDocument);
		result.AddPair(CusSupportingInfoTypeList.Codes.Restriction, CusSupportingInfoTypeList.Descriptions.Restriction);
		result.AddPair(CusSupportingInfoTypeList.Codes.InAndOutwardProcessing, CusSupportingInfoTypeList.Descriptions.InAndOutwardProcessing);
		result.AddPair(CusSupportingInfoTypeList.Codes.AdditionalInformation, CusSupportingInfoTypeList.Descriptions.AdditionalInformation);
		return result;
	}

	#endregion
}
