using System;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CC044CSupportingDocumentProvider : CC044CDocumentProvider, ISupportingDocument
	{
		public CC044CSupportingDocumentProvider(CusSupportingInfo document, bool isInPhase5TransitionPeriod) : base(document, isInPhase5TransitionPeriod)
		{
		}

		public string ComplementOfInformation => StatusIsNew ? document.CSI_ReferenceNumber2.ToString() : string.Empty;

		public int? DocumentLineItemNumber => 0;

		public string IssuingAuthorityName => null;

		public DateTime? ValidityDate => null;

		public decimal Amount => 0;

		public string Currency => null;

		public string MeasurementUnitAndQualifier => null;

		public decimal? Quantity => null;
	}
}
