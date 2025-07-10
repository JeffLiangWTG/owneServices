using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class SupportingDocumentProvider : DocumentProvider, ISupportingDocument
	{
		public SupportingDocumentProvider(CusSupportingInfo supportingInfo) : base(supportingInfo)
		{
		}

		public string DocumentLineItemNumber => null;

		public string IssuingAuthorityName => null;

		public DateTime DateOfValidity => DateTime.MinValue;

		public string CcQualifier => null;
	}
}
