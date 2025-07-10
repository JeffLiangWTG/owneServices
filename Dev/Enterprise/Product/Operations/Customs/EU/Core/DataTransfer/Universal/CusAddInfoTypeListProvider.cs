using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CusAddInfoTypeListProvider
	{
		public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext)
		{
			return TableSpecificCusAddInfoTypeListCore(tableCode, dataContext);
		}

		protected virtual ICodeDescriptionPairList TableSpecificCusAddInfoTypeListCore(ZString tableCode, string dataContext)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = GetListForJobDeclaration();
					break;
				case JobComInvoiceHeaderSchema.Constants.Prefix:
					result = GetListForJobComInvoiceHeader(dataContext);
					break;
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetListForJobComInvoiceLine();
					break;
				case CusInBondCargoDescSchema.Constants.Prefix:
					result = GetListForCusInBondCargoDesc();
					break;
				case CusInBondHeaderSchema.Constants.Prefix:
					result = GetListForCusInBondHeader();
					break;
			}
			return result;
		}

		#region Implementation

		protected virtual CodeDescriptionPairList GetListForJobComInvoiceHeader(string dataContext)
		{
			// Currently both InvoiceGroupHeader and InvoiceHeader have the same list so we don't need to do anything specific
			return new CodeDescriptionPairList();
		}

		protected virtual CodeDescriptionPairList GetListForJobDeclaration()
		{
			return new CodeDescriptionPairList();
		}

		public static class CusAddInfoTypeAttributeDescriptions
		{
			public static string GBAdditionalInfo => Res.GetString("4592911A-A38F-480F-9AE2-01D7D140AE8A", "Additional Info");

			public static string GBPreviousDocument => Res.GetString("D795E281-D9C0-4E86-A583-11DE43D1A826", "Previous Document");

			public static string GBSupportingDocument => Res.GetString("35B02633-A891-49B9-8903-9A294B9BAB9C", "Supporting Document");

			public static string GBTax => Res.GetString("034847DD-849C-493E-9575-CE9E71EFB0D0", "Tax");

			public static string EuNctsResultsOfControl => Res.GetString("3267914B-B1B5-47F1-B7EE-15A8931D3F1D", "Results of Control");

			public static string EuNctsUnloadingRemark => Res.GetString("111182D4-57F6-43F6-BB14-5DD49E7F2AFD", "Unloading Remark");

			public static string GbGuarantee => Res.GetString("457BF8B7-D4E2-4242-8EB5-632D87BEF1AA", "Guarantee");
		}

		protected virtual CodeDescriptionPairList GetListForJobComInvoiceLine()
		{
			return new CodeDescriptionPairList();
		}

		protected virtual CodeDescriptionPairList GetListForCusInBondHeader()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.EuNctsResultsOfControl, CusAddInfoTypeAttributeDescriptions.EuNctsResultsOfControl);
			result.AddPair(CusAddInfoTypeAttribute.Codes.EuNctsUnloadingRemark, CusAddInfoTypeAttributeDescriptions.EuNctsUnloadingRemark);
			return result;
		}

		protected virtual CodeDescriptionPairList GetListForCusInBondCargoDesc()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.EuNctsResultsOfControl, CusAddInfoTypeAttributeDescriptions.EuNctsResultsOfControl);
			return result;
		}

		#endregion
	}
}
