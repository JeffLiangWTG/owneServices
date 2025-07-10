using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore
{
	public static class NonSupportedDeliveryMethodMessageBuilder
	{
		public static string GetMessage(IEnumerable<IDocument> documents, string selectedDeliveryMethod)
		{
			string result = string.Empty;

			if (documents != null)
			{
				selectedDeliveryMethod = string.IsNullOrEmpty(selectedDeliveryMethod) ? "" : selectedDeliveryMethod.ToUpperInvariant();

				var nonSupportedDocuments = documents.Where(document => document != null && !document.GetSupportedDeliveryMethodDespiteOfPrintCopyType().Contains(selectedDeliveryMethod)).ToList();

				result = (nonSupportedDocuments.Count > 0) ? GetMessageCore(nonSupportedDocuments, selectedDeliveryMethod) : string.Empty;
			}

			return result;
		}

		static string GetMessageCore(IEnumerable<IDocument> nonSupportedDocuments, string selectedDeliveryMethod)
		{
			var result = new StringBuilder();
			if (!string.IsNullOrEmpty(selectedDeliveryMethod))
			{
				var notifyModeList = new CodeDescriptionPairList(OLookUpEditType.NotifyMode);
				foreach (var document in nonSupportedDocuments)
				{
					if (result.Length > 0)
					{
						result.AppendLine();
					}

					var allPossibleDeliveryMethods = document.GetSupportedDeliveryMethodDespiteOfPrintCopyType();
					result.Append(Res.GetString("c441c713-9921-4d6a-bb3c-f8840ca692db", "'{0}' cannot be delivered because it has been restricted for {1} delivery only.", document.DocumentName,
						ListFormatter.GetCommaSeparatedText(allPossibleDeliveryMethods.Select(method => notifyModeList.GetDescriptionFromCode(method) ?? method))));
				}
			}
			return result.ToString();
		}
	}
}
