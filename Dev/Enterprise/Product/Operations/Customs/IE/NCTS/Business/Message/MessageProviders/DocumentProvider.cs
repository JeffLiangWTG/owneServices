using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class DocumentProvider : IDocument
	{
		public DocumentProvider(CusSupportingInfo document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}
		protected readonly CusSupportingInfo document;

		public string Type => document.CSI_Code;

		public string Reference => document.CSI_ReferenceNumber;
	}
}
