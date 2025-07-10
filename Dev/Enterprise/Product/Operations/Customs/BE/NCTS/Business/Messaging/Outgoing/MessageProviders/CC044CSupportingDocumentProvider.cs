using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC044CSupportingDocumentProvider : CC044CDocumentProvider, ISupportingDocument
	{
		public CC044CSupportingDocumentProvider(CusSupportingInfo document) : base(document)
		{
		}

		public string ComplementOfInformation => StatusIsNew ? document.CSI_ReferenceNumber2.ToString() : string.Empty;

		public int DocumentLineItemNumber => 0;

		public string IssuingAuthorityName => null;

		public DateTime? ValidityDate => null;

		public decimal Amount => 0;

		public string Currency => null;

		public string MeasurementUnitAndQualifier => null;

		public decimal? Quantity => null;
	}
}
