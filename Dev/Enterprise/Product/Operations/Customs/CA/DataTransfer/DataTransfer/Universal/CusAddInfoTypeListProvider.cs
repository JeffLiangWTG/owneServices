using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	static class CusAddInfoTypeListProvider
	{
		public static ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetListForJobComInvoiceLine();
					break;
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

		static CodeDescriptionPairList GetListForJobComInvoiceLine()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.CADutyAndTax, Res.GetString("3a81bc12-6b12-4bf0-9046-1f810577adc3", "Duty And Tax"));
			result.AddPair(CusAddInfoTypeAttribute.Codes.CACFIAPGAHeader, Res.GetString("b2c203ed-ab58-456d-8dc4-b1cd67dd1470", "Canadian Food Inspection Agency PGA"));
			result.AddPair(CusAddInfoTypeAttribute.Codes.CACNSCPGAHeader, Res.GetString("85e4ce72-1231-4543-b900-509487f10f9e", "Canadian Nuclear Safety Commission PGA"));
			result.AddPair(CusAddInfoTypeAttribute.Codes.CADFOPGAHeader, Res.GetString("b371affe-bf1d-43a7-a10b-56d932d698ca", "Department of Fisheries and Oceans PGA"));
			result.AddPair(CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader, Res.GetString("f84780c1-a0ea-404e-a869-63bb684f893a", "Environment And Climate Change Canada PGA"));
			result.AddPair(CusAddInfoTypeAttribute.Codes.CAGACPGAHeader, Res.GetString("edfef685-7430-4316-8492-d67f18e2a4f6", "Global Affairs Canada PGA"));
			result.AddPair(CusAddInfoTypeAttribute.Codes.CAHCPGAHeader, Res.GetString("c3a545d2-814d-4dcf-ad54-56ceadf02965", "Health Canada PGA"));
			result.AddPair(CusAddInfoTypeAttribute.Codes.CANRCanPGAHeader, Res.GetString("77c2f142-9208-4b40-a797-421f0885ecf6", "Natural Resources Canada PGA"));
			result.AddPair(CusAddInfoTypeAttribute.Codes.CAPHACPGAHeader, Res.GetString("6b45cfa4-b270-4be4-9770-cbc814619a9b", "Public Health Agency of Canada PGA"));
			result.AddPair(CusAddInfoTypeAttribute.Codes.CATCPGAHeader, Res.GetString("bb33c7c9-d3e7-4777-9d66-ea5b0cd56185", "Transport Canada PGA"));
			return result;
		}

		static CodeDescriptionPairList GetListForJobDeclaration()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.CACCN, Res.GetString("36f20d7a-51dc-457d-a0f5-8a47c31afed4", "Cargo Control Number"));
			return result;
		}

		static ICodeDescriptionPairList GetListForCusAddInfo()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.CAComponent, Res.GetString("3606c6cd-8131-4941-8487-74945da25680", "Component"));
			return result;
		}

		#endregion
	}
}
