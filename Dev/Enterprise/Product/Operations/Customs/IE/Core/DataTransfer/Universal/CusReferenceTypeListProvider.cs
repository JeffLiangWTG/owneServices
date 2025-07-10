using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.DataTransfer.Universal
{
	public class CusReferenceTypeListProvider
	{
		public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext)
		{
			ICodeDescriptionPairList result;
			switch (tableCode)
			{
				case CusEntryInstructionSchema.Constants.Prefix:
					result = GetListForEntryInstruction();
					break;
				case CusInBondCargoDescSchema.Constants.Prefix:
					result = GetListForCusInBondCargoDesc();
					break;
				default:
					result = null;
					break;
			}
			return result;
		}

		static CodeDescriptionPairList GetListForEntryInstruction() => new CodeDescriptionPairList()
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
