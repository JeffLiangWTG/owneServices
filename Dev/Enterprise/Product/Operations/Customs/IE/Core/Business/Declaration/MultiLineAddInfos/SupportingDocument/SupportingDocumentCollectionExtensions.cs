using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	static class SupportingDocumentCollectionExtensions
	{
		public static bool HasAuthorisationInwardProcessingProcedure(this SupportingDocumentCollection collection) => collection.Cast<SupportingDocument>().Any(IsAuthorisationInwardProcessingProcedure);

		public static bool IsAuthorisationInwardProcessingProcedure(this SupportingDocument supportingDocument) => supportingDocument.CSI_Code.EqualsIgnoringCase(Constants.SupportingDocumentCodes.AuthorisationInwardProcessingProcedure);

		internal static bool HasU164OrU166OrN865SupportingDocument(SupportingDocumentCollection collection)
		{
			return collection.Cast<SupportingDocument>().Any(docs =>
				docs.CSI_Code.Equals(Constants.SupportingDocumentCodes._U164) ||
				docs.CSI_Code.Equals(Constants.SupportingDocumentCodes._U166) ||
				docs.CSI_Code.Equals(Constants.SupportingDocumentCodes._N865));
		}

		internal static bool HasU165OrU167SupportingDocument(SupportingDocumentCollection collection)
		{
			return collection.Cast<SupportingDocument>().Any(docs =>
				docs.CSI_Code.Equals(Constants.SupportingDocumentCodes._U165) ||
				docs.CSI_Code.Equals(Constants.SupportingDocumentCodes._U167));
		}

		public static bool HasN018SupportingDocument(SupportingDocumentCollection collection)
		{
			return collection.Cast<SupportingDocument>().Any(docs => docs.CSI_Code == Constants.SupportingDocumentCodes._N018);
		}

		public static bool HasMutuallyExclusiveSupportingDocument(SupportingDocumentCollection supportingDocuments)
		{
			var validSupportingDocuments = supportingDocuments.Select(x => x.CSI_Code).Where(x => IsValidForBR20312(x)).Distinct().ToList();
			if (validSupportingDocuments.Count > 2)
			{
				return true;
			}
			else if (validSupportingDocuments.Count == 2 &&
				(validSupportingDocuments[0] == Constants.SupportingDocumentCodes._U164 || validSupportingDocuments[0] == Constants.SupportingDocumentCodes._U166 ||
				 validSupportingDocuments[1] == Constants.SupportingDocumentCodes._U164 || validSupportingDocuments[1] == Constants.SupportingDocumentCodes._U166))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool IsValidForBR20312(ZString code)
		{
			switch (code)
			{
				case Constants.SupportingDocumentCodes._U164:
				case Constants.SupportingDocumentCodes._U165:
				case Constants.SupportingDocumentCodes._U166:
				case Constants.SupportingDocumentCodes._U167:
					return true;
				default:
					return false;
			}
		}
	}
}
