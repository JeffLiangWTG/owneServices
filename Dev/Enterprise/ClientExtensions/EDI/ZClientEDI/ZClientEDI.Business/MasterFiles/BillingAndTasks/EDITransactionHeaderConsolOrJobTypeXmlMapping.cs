using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.DataTransfer.XmlMapping;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDITransactionHeaderConsolOrJobTypeXmlMapping : TransactionLineConsolOrJobTypeXmlMapping
	{
		protected EDITransactionHeaderConsolOrJobTypeXmlMapping()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			return base.GetMappings().Concat(new[]
			{
				new Mapping(EDIJobInvoicingConsumerTypes.Incident.Code, nameof(Xsd.TxnLineConsolOrJobType.INC)),
				new Mapping(EDIJobInvoicingConsumerTypes.PSQuote.Code, nameof(Xsd.TxnLineConsolOrJobType.PSQ)),
			});
		}

		public static EDITransactionHeaderConsolOrJobTypeXmlMapping New()
		{
			return new EDITransactionHeaderConsolOrJobTypeXmlMapping();
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = New;
		}
	}
}

