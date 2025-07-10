using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class MPreviousDocumentProvider : DocumentProvider, IMPreviousDocument
	{
		public MPreviousDocumentProvider(CusSupportingInfo supportingInfo) : base(supportingInfo)
		{
		}

		public DateTime DateOfAcceptance => DateTime.MinValue;

		public string CcQualifier => null;
	}
}
