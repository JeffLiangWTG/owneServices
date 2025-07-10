using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class ExchangedDocumentTypeBuilder : XmlBuilder
	{
		public ExchangedDocumentTypeBuilder(XNamespace xNamespace, AccEInvoicingBatch batch) : base(xNamespace)
		{
			Batch = Argument.NotNull(batch, nameof(batch));
			CommonTypeBuilder = new CommonTypeBuilder(xNamespace);
		}

		AccEInvoicingBatch Batch { get; }

		public XStreamingElement Build(string nodeName)
		{
			return new XStreamingElement(GetTagName(nodeName)
				, BuildID()
				, BuildIssueDateTime()
				, BuildReferencedDocument()
			);
		}

		XElement BuildID()
		{
			return new XElement(GetTagName("ID"), Batch.AIB_BatchNumber);
		}

		XElement BuildIssueDateTime()
		{
			return CommonTypeBuilder.BuildExchangedIssueDateTimeType("IssueDateTime", ZDateTime.Today.ToDateTime());
		}

		XStreamingElement BuildReferencedDocument()
		{
			return new XStreamingElement(GetTagName("ReferencedDocument")
				, new XElement(GetTagName("ID"), ReadyKoreaConstants.BusinessRegistrationNumber)
			);
		}

		CommonTypeBuilder CommonTypeBuilder { get; }
	}
}
