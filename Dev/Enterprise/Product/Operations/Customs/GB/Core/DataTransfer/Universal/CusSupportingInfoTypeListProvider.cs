using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class CusSupportingInfoTypeListProvider : EU.DataTransfer.Universal.CusSupportingInfoTypeListProvider
	{
		protected override ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeListCore(ZString tableCode, string dataContext)
		{
			var result = base.TableSpecificCusSupportingInfoTypeListCore(tableCode, dataContext);
			if (result == null)
			{
				switch (tableCode)
				{
					case CusEntryInstructionSchema.Constants.Prefix:
						result = GetListForEntryInstruction();
						break;
				}
			}
			return result;
		}

		public static CodeDescriptionPairList GetListForEntryInstruction()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusSupportingInfoTypeList.Codes.AdditionalInfo, CusSupportingInfoTypeList.Descriptions.AdditionalInfo);
			result.AddPair(CusSupportingInfoTypeList.Codes.PreviousDocument, CusSupportingInfoTypeList.Descriptions.PreviousDocument);
			result.AddPair(CusSupportingInfoTypeList.Codes.SupportingDocument, CusSupportingInfoTypeList.Descriptions.SupportingDocument);
			return result;
		}
	}
}
