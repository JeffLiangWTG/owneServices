using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public partial class OriginalReference : IDataObject
	{
		public OriginalReference()
		{
		}

		public OriginalReference(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(38)]
		public ZString? OriginalTransactionNumber { get; set; }
		[MaxLength(38)]
		public ZString? OriginalTransactionJobInvoiceNumber { get; set; }
		public ZDateTime? OriginalTransactionDate { get; set; }
		[MaxLength(20)]
		public ZString? OriginalTransactionReference { get; set; }
		[MaxLength(3)]
		public ZString? OriginalTransactionComplianceSubType { get; set; }
		public CodeDescriptionPair OriginalTransactionAmendingReversingReason { get; set; }
		public List<AuthorizationDetails> AuthorizationDetailCollection { get; private set; }

		public void Dispose()
		{
			if (AuthorizationDetailCollection != null)
			{
				foreach (var authDetails in AuthorizationDetailCollection)
				{
					authDetails?.Dispose();
				}
			}
		}
	}
}
