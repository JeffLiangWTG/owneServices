using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	static class AddInfoGroupTypesNeedInsertedToOtherTableListProvider
	{
		public static ICodeDescriptionPairList AddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = GetListForJobDeclaration();
					break;
				case CusAddInfoSchema.Constants.Prefix:
					result = GetListForCusAddInfo();
					break;
			}
			return result;
		}

		#region Implementation

		static CodeDescriptionPairList GetListForJobDeclaration()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.AddInfoKeys.CusCALPCO.CusAddInfoType, Res.GetString("9cf9ccf6-85a1-40c8-baf7-64cc48ef8b53", "LPCO"));
			return result;
		}

		static ICodeDescriptionPairList GetListForCusAddInfo()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.AddInfoKeys.CusCALPCO.CusAddInfoType, Res.GetString("ea8d244e-f948-4676-99c8-fc047360c108", "LPCO"));
			return result;
		}

		#endregion
	}
}
