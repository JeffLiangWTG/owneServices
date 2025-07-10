using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.CDSResponse
{
	public static class ResponseExtension
	{
		public static ZDateTime GetIssueDate(this Response response)
		{
			return response.IssueDateTime?.Item?.ToZDateTime() ?? ZDateTime.Empty;
		}

		public static ZString GetMovementReferenceNumber(this Response response)
		{
			return response.Declaration.ID?.Value ?? ZString.Empty; // will be empty for inventory pre-check failure DMS-link messages from CSPs
		}

		public static ZString GetDeclarationFunctionalReferenceID(this Response response)
		{
			return response.Declaration.FunctionalReferenceID?.Value ?? ZString.Empty;
		}

		public static ZString GetFunctionCode(this Response response)
		{
			return response.FunctionCode.Value;
		}

		public static ResponseBank GetBank(this Response response)
		{
			return response.Bank;
		}

		public static ResponseFunction GetResponseFunction(this Response response)
		{
			return ResponseFunction.New(response.GetFunctionCode());
		}

		public static ResponseDeclarationGovernmentAgencyGoodsItem[] GetGovernmentAgencyGoodsItem(this Response response)
		{
			return response.Declaration.GoodsShipment;
		}

		public static ZString GetStatusNameCode(this Response response)
		{
			return response.Status?.FirstOrDefault()?.NameCode?.Value ?? ZString.Empty;
		}

		public static bool HasErrors(this Response response)
		{
			return response?.Error?.Any() ?? false;
		}
	}
}
