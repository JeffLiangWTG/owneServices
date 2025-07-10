using System.Xml.Linq;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	class ExportEInvoiceXmlBuilder : IEInvoiceXmlBuilder
	{
		public ExportEInvoiceXmlBuilder()
		{
			clsFEXAuthRequestBuilder_constructorInitializedOnly = new ClsFEXAuthRequestBuilder();
			clsFEXRequestBuilder_contructorInitializedOnly = new ClsFEXRequestBuilder();
			transactionInfoHelper_constructorInitializedOnly = new TransactionInfoHelper();
		}

		IClsFEXAuthRequestBuilder ClsFEXAuthRequestBuilder => clsFEXAuthRequestBuilder_constructorInitializedOnly;
		IClsFEXAuthRequestBuilder clsFEXAuthRequestBuilder_constructorInitializedOnly;
		IClsFEXRequestBuilder ClsFEXRequestBuilder => clsFEXRequestBuilder_contructorInitializedOnly;
		IClsFEXRequestBuilder clsFEXRequestBuilder_contructorInitializedOnly;
		ITransactionInfoHelper TransactionInfoHelper => transactionInfoHelper_constructorInitializedOnly;
		ITransactionInfoHelper transactionInfoHelper_constructorInitializedOnly;

#if DEBUG
		public void SubstituteClsFEXAuthRequestBuilder_ForTestOnly(IClsFEXAuthRequestBuilder replacement) => clsFEXAuthRequestBuilder_constructorInitializedOnly = replacement;
		public IClsFEXAuthRequestBuilder ClsFEXAuthRequestBuilder_ExposedForTestOnly => ClsFEXAuthRequestBuilder;
		public void SubstituteClsFEXRequestBuilder_ForTestOnly(IClsFEXRequestBuilder replacement) => clsFEXRequestBuilder_contructorInitializedOnly = replacement;
		public IClsFEXRequestBuilder ClsFEXRequestBuilder_ExposedForTestOnly => ClsFEXRequestBuilder;
		public void SubstituteTransactionInfoHelper_ForTestOnly(ITransactionInfoHelper replacement) => transactionInfoHelper_constructorInitializedOnly = replacement;
		public ITransactionInfoHelper TransactionInfoHelper_ExposedForTestOnly => TransactionInfoHelper;
#endif

		XStreamingElement IEInvoiceXmlBuilder.BuildXml(TransactionInfo transaction, AccEInvoicingBatch accBatch)
		{
			#region SuppressResourceStringsCheckRegion

			XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
			XNamespace xsd = "http://www.w3.org/2001/XMLSchema";
			XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";

			return new XStreamingElement(soap + "Envelope",
				new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
				new XAttribute(XNamespace.Xmlns + "xsd", xsd.NamespaceName),
				new XAttribute(XNamespace.Xmlns + "soap", soap.NamespaceName),
				BuildXMLExportEInvoice());

			XStreamingElement BuildXMLExportEInvoice()
			{
				var registrationNumber = TransactionInfoHelper.GetRegistrationCode(transaction.BranchAddress, Core.Constants.CountryCodes.Argentina,
				ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT);

				XNamespace fexv1 = "http://ar.gov.afip.dif.fexv1/";
				return new XStreamingElement(soap + "Body",
							new XElement(fexv1 + "FEXAuthorize",
									ClsFEXAuthRequestBuilder.BuildXML(transaction, fexv1, registrationNumber),
									ClsFEXRequestBuilder.BuildXML(transaction, fexv1, registrationNumber, accBatch.Factory)));
			}
		}

		#endregion
	}
}
