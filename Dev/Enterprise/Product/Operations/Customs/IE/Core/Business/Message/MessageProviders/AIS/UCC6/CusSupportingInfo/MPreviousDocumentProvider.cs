using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class MPreviousDocumentProvider : IMPreviousDocument
	{
		public MPreviousDocumentProvider(PreviousDocument previousDocument)
		{
			this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		}
		readonly PreviousDocument previousDocument;

		public string Type => previousDocument.CSI_Code;

		public string Reference => previousDocument.CSI_ReferenceNumber;

		public DateTime DateOfAcceptance => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(previousDocument.CSI_DateOfIssue);

		public string CcQualifier => null;
	}
}
