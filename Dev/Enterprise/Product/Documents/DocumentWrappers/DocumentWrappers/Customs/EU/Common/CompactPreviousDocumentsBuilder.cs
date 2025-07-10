using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class CompactPreviousDocumentsBuilder
	{
		public ZString GetPreviousDocumentsFormatted(IEnumerable<PreviousDocument> previousDocuments, bool isPhase5Departure = false)
		{
			var result = new ZStringBuilder();

			if (previousDocuments != null)
			{
				foreach (var prevDoc in previousDocuments)
				{
					IEnumerable<(ZString, ZString)> documentFields;

					documentFields = isPhase5Departure
						? GetPreviousDocumentFieldsForPhase5(prevDoc).Where(x => !x.value.IsEmpty)
						: GetCountrySpecificFields(prevDoc).Where(x => !x.value.IsEmpty);

					var prevDocString = ZString.Empty;
					foreach (var (separator, value) in documentFields)
					{
						prevDocString += ZString.Format("{0}{1}", separator, value);
					}

					result.Append(RemoveFirstSeparatorIfNecessary(prevDocString));
				}
			}

			return result.ToStringWithDelimiterBetweenAppends(DocumentsDelimiter);
		}

		protected virtual IEnumerable<(ZString separator, ZString value)> GetCountrySpecificFields(PreviousDocument prevDoc)
		{
			ZString hyphen = DocumentWrapperConstants.Delimiters.Hyphen;

			var codes = RefCusCodeListTypes.GetCachedList(prevDoc.Factory,
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS,
				ZDateTime.Today);

			var prevDocumentCode = prevDoc.CSI_Code;
			var codeDescription = codes.GetDescriptionFromCode(prevDocumentCode);

			yield return (hyphen, string.IsNullOrEmpty(codeDescription) ? prevDocumentCode : codeDescription);
			yield return (hyphen, prevDoc.CSI_ReferenceNumber);
			yield return (hyphen, prevDoc.CSI_Description);
		}

		IEnumerable<(ZString separator, ZString value)> GetPreviousDocumentFieldsForPhase5(PreviousDocument prevDoc)
		{
			yield return (FieldDelimiter, prevDoc.CSI_Code);
			yield return (FieldDelimiter, prevDoc.CSI_ReferenceNumber);
		}

		ZString RemoveFirstSeparatorIfNecessary(ZString value)
		{
			if(value.StartsWith(" - "))
			{
				return value.SubstringSafe(3);
			}
			else if (value.StartsWith("-") || value.StartsWith(" "))
			{
				return value.SubstringSafe(1);
			}
			return value;
		}

		protected virtual ZString DocumentsDelimiter => DocumentWrapperConstants.Delimiters.SemiColonAndspace;

		protected virtual ZString FieldDelimiter => DocumentWrapperConstants.Delimiters.Dash;
	}
}
