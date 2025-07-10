using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class SupportingDocumentProvider : DocumentProvider, ISupportingDocument
	{
		public SupportingDocumentProvider(CusSupportingInfo cusSupportingInfo, bool isInPhase5TransitionPeriod) : base(cusSupportingInfo, isInPhase5TransitionPeriod)
		{
		}

		public string ComplementOfInformation => document.CSI_ReferenceNumber2;

		public int? DocumentLineItemNumber => document.CSI_ItemNumber.IsDefault ? null : document.CSI_ItemNumber;
	}
}
