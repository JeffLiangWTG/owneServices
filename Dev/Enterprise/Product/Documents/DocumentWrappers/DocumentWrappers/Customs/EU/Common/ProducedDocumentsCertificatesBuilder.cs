using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using EUDocumentWrapperConstants = Enterprise.DocumentWrappers.Customs.EU.DocumentWrapperConstants;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class ProducedDocumentsCertificatesBuilder
	{
		public ZString GetProducedDocumentsCertificatesFormatted(IEnumerable<SupportingDocument> supportingDocuments, bool isPhase5Departure = false)
		{
			Argument.NotNull(supportingDocuments, nameof(supportingDocuments));

			ZStringBuilder supportingDocs = new ZStringBuilder();
			AppendSupportingDocumentInfo(supportingDocs, supportingDocuments, isPhase5Departure);
			return supportingDocs.ToStringWithDelimiterBetweenAppends(DocumentsDelimiter);
		}

		public void AppendSupportingDocumentInfo(ZStringBuilder result, IEnumerable<SupportingDocument> supportingDocuments, bool isPhase5Departure = false)
		{
			Argument.NotNull(result, nameof(result));
			Argument.NotNull(supportingDocuments, nameof(supportingDocuments));

			foreach (var supportingDocument in supportingDocuments)
			{
				AppendSupportingDocument(result, supportingDocument, isPhase5Departure);
			}
		}

		protected void AppendSupportingDocument(ZStringBuilder result, SupportingDocument supportingDocument, bool isPhase5Departure = false)
		{
			Argument.NotNull(result, nameof(result));
			Argument.NotNull(supportingDocument, nameof(supportingDocument));

			var lineSb = new ZStringBuilder();

			AppendSupportingDocumentCore(lineSb, supportingDocument, isPhase5Departure);

			if (!lineSb.IsEmpty)
			{
				result.Append(lineSb.ToStringWithDelimiterBetweenAppends(FieldDelimiter));
			}
		}

		protected virtual void AppendSupportingDocumentCore(ZStringBuilder lineSb, SupportingDocument supportingDocument, bool isPhase5Departure = false)
		{
			lineSb.Append(GetSupportingDocumentCode(supportingDocument));
			lineSb.AppendIfNotEmpty(supportingDocument.CSI_ReferenceNumber);
			if (!supportingDocument.CSI_SubType.IsEmpty)
			{
				lineSb.Append("P=" + supportingDocument.CSI_SubType);
			}

			if (!supportingDocument.CSI_Quantity.IsEmpty)
			{
				lineSb.Append("Q=" + supportingDocument.CSI_Quantity.ToString());
			}

			if (!supportingDocument.CSI_Description.IsEmpty)
			{
				lineSb.Append("\"" + supportingDocument.CSI_Description + "\"");
			}
		}

		protected virtual ZString GetSupportingDocumentCode(SupportingDocument supportingDocument) => supportingDocument.CSI_Code;

		protected virtual ZString DocumentsDelimiter => EUDocumentWrapperConstants.Delimiters.CommaAndSpace;

		protected virtual ZString FieldDelimiter => EUDocumentWrapperConstants.Delimiters.Space;
	}
}
