using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	class CFDiXmlWriter : TransactionBatchToXmlWriter
	{
		readonly IMexicoEInvoicingDependencyFactory MexicoEInvocingDependencies;

		public CFDiXmlWriter()
		{
			cfDiXmlBuilder_constructorInitializedOnly = new CFDiComprobanteBuilder();
			signatureProvider_constructorInitializedOnly = new CertificateHelper();
			companyCredential_constructorInitializedOnly = new CompanyCredential();
			MexicoEInvocingDependencies = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetMexicoEInvoicingDependencyFactory();
		}

		#region Obsolete, we must use MexicoEInvocingDependencies

		ICFDiComprobanteBuilder CFDiXmlBuilder => cfDiXmlBuilder_constructorInitializedOnly;
		ICFDiComprobanteBuilder cfDiXmlBuilder_constructorInitializedOnly;

		ICertificateHelper SignatureProvider => signatureProvider_constructorInitializedOnly;
		ICertificateHelper signatureProvider_constructorInitializedOnly;

		ICompanyCredential CompanyCredential => companyCredential_constructorInitializedOnly;
		ICompanyCredential companyCredential_constructorInitializedOnly;

#if DEBUG
		public void SubstituteCFDiXmlBuilder_ForTestOnly(ICFDiComprobanteBuilder replacement) => cfDiXmlBuilder_constructorInitializedOnly = replacement;
		public ICFDiComprobanteBuilder CFDiXmlBuilder_ExposedForTestOnly => CFDiXmlBuilder;

		public void SubstituteSignXmlDocument_ForTestOnly(ICertificateHelper replacement) => signatureProvider_constructorInitializedOnly = replacement;
		public ICertificateHelper SignXmlDocument_ExposedForTestOnly => SignatureProvider;

		public void SubstituteCompanyCredential_ForTestOnly(ICompanyCredential replacement) => companyCredential_constructorInitializedOnly = replacement;
		public ICompanyCredential CompanyCredential_ExposedForTestOnly => CompanyCredential;
#endif

		#endregion

		protected override XmlWriterSettings Settings()
		{
			var settings = base.Settings();
			settings.Indent = true;
			settings.IndentChars = "\t";
			settings.NewLineChars = "\r\n";

			return settings;
		}

		#region DocumentBody

		protected override void WriteDocumentBody(XmlWriter writer, TransactionInfo transactionInfo, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			switch (messageType)
			{
				case MexicoEInvoiceMessageTypeProvider.Codes.GenerateCancellationRequest:
					CancellationDocumentBody(writer, transactionInfo, accBatch);
					break;
				case MexicoEInvoiceMessageTypeProvider.Codes.GenerateInvoiceRequest:
					GENDocumentBody(writer, transactionInfo);
					break;
				case MexicoEInvoiceMessageTypeProvider.Codes.RequestPDFDocumentForInvoice:
					PdfDocumentBody(writer, transactionInfo.AuthorizationDetailCollection);
					break;
				default:
					throw new ArgumentException("Invalid Message Type.");
			}
		}

		void CancellationDocumentBody(XmlWriter writer, TransactionInfo transactionInfo, AccEInvoicingBatch accBatch)
		{
			var cancellationXML = MexicoEInvocingDependencies.GetCFDiCancellationBuilder().BuildXml(transactionInfo, accBatch);
			cancellationXML.WriteTo(writer);
		}

		void GENDocumentBody(XmlWriter writer, TransactionInfo transactionInfo)
		{
			var ns = new XmlSerializerNamespaces();
			ns.Add((NoResString)"cfdi", "http://www.sat.gob.mx/cfd/4"); // Constant String
			ns.Add((NoResString)"xsi", "http://www.w3.org/2001/XMLSchema-instance"); // Constant String

			var eInvoiceInfoCFDI = CFDiXmlBuilder.BuildXml(transactionInfo);
			var serializer = ZXmlSerializer.New(eInvoiceInfoCFDI.GetType());
			serializer.Serialize(writer, eInvoiceInfoCFDI, ns);
		}

		void PdfDocumentBody(XmlWriter writer, List<AuthorizationDetails> authorizationDetails)
		{
			var pdfXML = MexicoEInvocingDependencies.GetCFDiObtenerPDFBuilder().BuildXml(authorizationDetails);
			pdfXML.WriteTo(writer);
		}

		#endregion

		#region XMLToStream

		protected override void WriteXmlToStreamCore(TransactionInfo transactionInfo, Stream stream, ZString messageType, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			switch (messageType)
			{
				case MexicoEInvoiceMessageTypeProvider.Codes.GenerateCancellationRequest:
					CancellationToStream(transactionInfo, stream, messageType, accBatch);
					break;
				case MexicoEInvoiceMessageTypeProvider.Codes.GenerateInvoiceRequest:
					GENToStream(transactionInfo, stream, messageType, accBatch);
					break;
				case MexicoEInvoiceMessageTypeProvider.Codes.RequestPDFDocumentForInvoice:
					PDFToStream(transactionInfo, stream, messageType, accBatch);
					break;
				default:
					throw new ArgumentException("Invalid Message Type.");
			}
		}

		void CancellationToStream(TransactionInfo transactionInfo, Stream stream, ZString messageType, AccEInvoicingBatch accBatch)
		{
			if (accBatch.AIB_GovernmentAllocatedNumber.IsEmpty)
			{
				throw new ArgumentException("No government allocated number was found for original AR INV transaction.");
			}

			base.WriteXmlToStreamCore(transactionInfo, stream, messageType, accBatch, null, null);
		}

		void PDFToStream(TransactionInfo transactionInfo, Stream stream, ZString messageType, AccEInvoicingBatch accBatch)
		{
			base.WriteXmlToStreamCore(transactionInfo, stream, messageType, accBatch, null, null);
		}

		void GENToStream(TransactionInfo transactionInfo, Stream stream, ZString messageType, AccEInvoicingBatch accBatch)
		{
			var doc = new XmlDocument();
			using (var tmpStream = new MemoryStream())
			{
				base.WriteXmlToStreamCore(transactionInfo, tmpStream, messageType, accBatch, null, null);

				tmpStream.Position = 0;
				using (var reader = new StreamReader(tmpStream, true))
				{
					var xml = reader.ReadToEnd();
					doc.LoadXml(xml);
				}
			}

			PostProcessorFormatDocumentDecimals(doc);
			SignatureProvider.SignCFDiXmlDocument(doc, CompanyCredential.GetCompanyCredential(transactionInfo));

			using (XmlWriter writer = XmlWriter.Create(stream, Settings()))
			{
				doc.WriteContentTo(writer);
			}
		}

		#endregion

		#region PayloadValidation

		protected override ZString[] XsdResourceNames => new ZString[]
		{
			"Enterprise.Accounting.ElectronicMessaging.Mexico.CFDiXmlWriter.catCFDI.xsd",
			"Enterprise.Accounting.ElectronicMessaging.Mexico.CFDiXmlWriter.cfdv40.xsd",
			"Enterprise.Accounting.ElectronicMessaging.Mexico.CFDiXmlWriter.tdCFDI.xsd"
		};

		#endregion

		#region SuppressResourceStringsCheckRegion

		void PostProcessorFormatDocumentDecimals(XmlDocument doc)
		{
			var contextNode = doc.DocumentElement?.SelectSingleNode($"/*[local-name()='{nameof(Comprobante)}']");
			var documentCurrencyCode = contextNode?.Attributes.GetNamedItem(nameof(Comprobante.Moneda));

			if (documentCurrencyCode != null)
			{
				var mappedCurrency = new BusinessObjectFactory().Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, documentCurrencyCode.Value)).FirstOrDefault();

				var currencyFormat = $"F{mappedCurrency.Decimals}";
				var trasladoXpath = $"/*[local-name()='Comprobante']/*[local-name()='Conceptos']/*[local-name()='Concepto']/*[local-name()='Impuestos']/*[local-name()='Traslados']/*[local-name()='Traslado']";
				var retencionXpath = $"/*[local-name()='Comprobante']/*[local-name()='Conceptos']/*[local-name()='Concepto']/*[local-name()='Impuestos']/*[local-name()='Retenciones']/*[local-name()='Retencion']";
				var comprobanteImpuestosXpath = $"/*[local-name()='Comprobante']/*[local-name()='Impuestos']";
				var comprobanteImpuestosRetencionXpath = $"/*[local-name()='Comprobante']/*[local-name()='Impuestos']/*[local-name()='Retenciones']/*[local-name()='Retencion']";
				var comprobanteImpuestosTrasladoXpath = $"/*[local-name()='Comprobante']/*[local-name()='Impuestos']/*[local-name()='Traslados']/*[local-name()='Traslado']";

				var searchingFields = new List<(string xPathField, string xPathFormat)> {
					($"//*[local-name()='Concepto']/@ValorUnitario", currencyFormat),
					($"//*[local-name()='Concepto']/@Importe", currencyFormat),
					($"/*[local-name()='Comprobante']/@TipoCambio", "F6"),
					($"{trasladoXpath}/@{nameof(ComprobanteConceptoImpuestosTraslado.Base)}", currencyFormat),
					($"{trasladoXpath}/@{nameof(ComprobanteConceptoImpuestosTraslado.Importe)}", currencyFormat),
					($"{trasladoXpath}/@{nameof(ComprobanteConceptoImpuestosTraslado.TasaOCuota)}", "F6"),
					($"{retencionXpath}/@{nameof(ComprobanteConceptoImpuestosRetencion.Base)}", currencyFormat),
					($"{retencionXpath}/@{nameof(ComprobanteConceptoImpuestosRetencion.Importe)}", currencyFormat),
					($"{retencionXpath}/@{nameof(ComprobanteConceptoImpuestosRetencion.TasaOCuota)}", "F6"),
					($"{comprobanteImpuestosXpath}/@{nameof(ComprobanteImpuestos.TotalImpuestosRetenidos)}", currencyFormat),
					($"{comprobanteImpuestosRetencionXpath}/@{nameof(ComprobanteImpuestosRetencion.Importe)}", currencyFormat),
					($"{comprobanteImpuestosXpath}/@{nameof(ComprobanteImpuestos.TotalImpuestosTrasladados)}", currencyFormat),
					($"{comprobanteImpuestosTrasladoXpath}/@{nameof(ComprobanteImpuestosTraslado.Importe)}", currencyFormat),
					($"{comprobanteImpuestosTrasladoXpath}/@{nameof(ComprobanteImpuestosTraslado.TasaOCuota)}", "F6"),
					($"{comprobanteImpuestosTrasladoXpath}/@{nameof(ComprobanteImpuestosTraslado.Base)}", currencyFormat),
					($"/*[local-name()='Comprobante']/@{nameof(Comprobante.SubTotal)}", currencyFormat),
					($"/*[local-name()='Comprobante']/@{nameof(Comprobante.Total)}", currencyFormat),
				};

				foreach (var field in searchingFields)
				{
					var nodes = doc.DocumentElement?.SelectNodes(field.xPathField);
					foreach (XmlNode node in nodes)
					{
						if (decimal.TryParse(node.Value, out decimal decimalValue))
						{
							node.Value = decimalValue.ToString(field.xPathFormat, CultureInfo.InvariantCulture);
						}
					}
				}
			}
		}

		#endregion
	}
}
