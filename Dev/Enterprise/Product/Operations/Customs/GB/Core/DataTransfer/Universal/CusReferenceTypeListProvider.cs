using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	static class CusReferenceTypeListProvider
	{
		public static ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case CusEntryInstructionSchema.Constants.Prefix:
					result = GetListForEntryInstruction();
					break;
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetListForInvoiceLine();
					break;
				case CusInBondCargoDescSchema.Constants.Prefix:
					result = GetListForCusInBondCargoDesc();
					break;
			}
			return result;
		}

		static CodeDescriptionPairList GetListForEntryInstruction() => GetFiscalReferenceCodeDescriptionPairList();

		static CodeDescriptionPairList GetListForInvoiceLine() => GetFiscalReferenceCodeDescriptionPairList();

		static CodeDescriptionPairList GetFiscalReferenceCodeDescriptionPairList() => new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(CusReferenceTypeList.Codes.FiscalReference, CusReferenceTypeList.Descriptions.FiscalReference)
		};

		static CodeDescriptionPairList GetListForCusInBondCargoDesc()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, Customs.Business.CusReferenceTypeList.Descriptions.SupplyChainActor);
			return result;
		}
	}
}
