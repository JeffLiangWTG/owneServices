using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CusCodeDataTypeAndCodeListProvider
	{
		public ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext)
		{
			return TableSpecificCusCodeDataTypeListCore(tableCode, dataContext);
		}

		protected virtual ICodeDescriptionPairList TableSpecificCusCodeDataTypeListCore(ZString tableCode, string dataContext)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = GetListForJobDeclaration();
					break;

				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetListForJobComInvoiceLine();
					break;

				case CusInBondCargoDescSchema.Constants.Prefix:
					result = GetListForCusInBondCargoDesc();
					break;
			}
			return result;
		}

		#region Implementation

		protected virtual CodeDescriptionPairList GetListForJobDeclaration()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.OfficeCode, CusCodeDataTypeList.Descriptions.OfficeCode);
			return result;
		}

		protected virtual CodeDescriptionPairList GetListForJobComInvoiceLine()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.SupplementaryCode, CusCodeDataTypeList.Descriptions.SupplementaryCode);
			result.AddPair(CusCodeDataTypeList.Codes.AdditionalProcedureCode, CusCodeDataTypeList.Descriptions.AdditionalProcedureCode);
			return result;
		}

		CodeDescriptionPairList GetListForCusInBondCargoDesc()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.SupplementaryCode, CusCodeDataTypeList.Descriptions.SupplementaryCode);
			return result;
		}

		#endregion
	}
}
