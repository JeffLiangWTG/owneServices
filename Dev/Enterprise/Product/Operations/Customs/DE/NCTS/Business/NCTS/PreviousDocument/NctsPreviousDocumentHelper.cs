using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public static class NctsPreviousDocumentHelper
	{
		public static bool IsAvailable(ZString csiProcedure, string fieldName)
		{
			var procedureCode = ConvertNctsPreviousProcedureCodeToDeclarationProcedureCode(csiProcedure);
			return new PreviousDocumentConfiguration().GetAvailableFieldsFromProcedureCode(procedureCode).Contains(fieldName);
		}

		public static bool NctsProcedureCodeSupportsMultiplePreviousDocuments(ZString csiProcedure)
		{
			var procedureCode = ConvertNctsPreviousProcedureCodeToDeclarationProcedureCode(csiProcedure);
			return new PreviousDocumentConfiguration().NctsProcedureCodeSupportsMultiplePreviousDocuments(procedureCode);
		}

		public static IEnumerable<(string ColumnName, string Caption)> GetAvailableNctsColumns(ZString csiProcedure)
		{
			var procedureCode = ConvertNctsPreviousProcedureCodeToDeclarationProcedureCode(csiProcedure);
			return new PreviousDocumentConfiguration().GetAvailableNctsColumns(procedureCode);
		}

		public static string[] GetUnavailableColumns(IEnumerable<string> availableColumns)
		{
			return new PreviousDocumentConfiguration().GetUnavailableColumns(availableColumns);
		}

		public static ZString ConvertNctsPreviousProcedureCodeToDeclarationProcedureCode(ZString csiProcedure)
		{
			var result = ZString.Empty;
			switch (csiProcedure)
			{
				case NctsPreviousProcedureList.Codes._N337:
					result = PreviousProcedureList.Codes._ATNEU;
					break;
				case NctsPreviousProcedureList.Codes._9DEZ:
					result = PreviousProcedureList.Codes._ATZL;
					break;
				case NctsPreviousProcedureList.Codes._9DEY:
					result = PreviousProcedureList.Codes._ATAV;
					break;
			}
			return result;
		}
	}
}
