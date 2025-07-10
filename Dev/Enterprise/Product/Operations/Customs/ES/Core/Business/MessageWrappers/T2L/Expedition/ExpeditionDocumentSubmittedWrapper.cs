using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ExpeditionDocumentSubmittedWrapper : IExpeditionDocumentSubmitted
	{
		public ExpeditionDocumentSubmittedWrapper(SupportingDocument doc)
		{
			document = Argument.NotNull(doc, "SupportingDocument cannot be null");
		}
		readonly SupportingDocument document;

		public ZString Code => document.CSI_Code;

		public ZString Number => document.CSI_ReferenceNumber;

		public ZDateTime Date => document.CSI_DateOfIssue;
	}
}
