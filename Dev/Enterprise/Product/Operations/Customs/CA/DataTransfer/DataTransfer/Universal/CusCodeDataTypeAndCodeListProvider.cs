using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	static class CusCodeDataTypeAndCodeListProvider
	{
		public static ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = GetTypeListForJobDeclaration();
					break;
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetTypeListForJobComInvoiceLine();
					break;
				case CusAddInfoSchema.Constants.Prefix:
					result = GetTypeListForCusAddInfo();
					break;
			}
			return result;
		}

		static ICodeDescriptionPairList GetTypeListForCusAddInfo()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.AIRSNumber, CusCodeDataTypeList.Descriptions.AIRSNumber);
			return result;
		}

		public static ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetCodeListForJobComInvoiceLine();
					break;
			}
			return result;
		}

		#region Implementation

		static CodeDescriptionPairList GetTypeListForJobDeclaration()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.Permit, CusCodeDataTypeList.Descriptions.Permit);
			return result;
		}

		static CodeDescriptionPairList GetTypeListForJobComInvoiceLine()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.Permit, CusCodeDataTypeList.Descriptions.Permit);
			result.AddPair(CusCodeDataTypeList.Codes.SITTNumber, CusCodeDataTypeList.Descriptions.SITTNumber);
			result.AddPair(CusCodeDataTypeList.Codes.CFIANumber, CusCodeDataTypeList.Descriptions.CFIANumber);

			return result;
		}

		static ICodeDescriptionPairList GetCodeListForJobComInvoiceLine()
		{
			return GetTypeListForJobComInvoiceLine();
		}

		#endregion
	}
}
