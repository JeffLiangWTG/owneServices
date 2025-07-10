using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class SupportingDocumentProvider : DocumentProvider, ISupportingDocument
	{
		public SupportingDocumentProvider(CusSupportingInfo cusSupportingInfo) : base(cusSupportingInfo)
		{
		}

		public string ComplementOfInformation => document.CSI_ReferenceNumber2;

		public int DocumentLineItemNumber => document.CSI_ItemNumber;

		public string IssuingAuthorityName => null;

		public DateTime? ValidityDate => null;

		public decimal Amount => 0;

		public string Currency => null;

		public string MeasurementUnitAndQualifier => null;

		public decimal? Quantity => null;
	}
}
