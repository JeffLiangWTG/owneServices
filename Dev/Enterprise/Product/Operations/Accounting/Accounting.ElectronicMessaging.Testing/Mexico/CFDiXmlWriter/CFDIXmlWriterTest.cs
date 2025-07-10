using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Moq;
using NUnit.Framework;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	[TestedType(typeof(CFDiXmlWriter))]
	class CFDiXmlWriterTest : TransactionBatchToXmlWriterTest
	{
		[ExpectNoExceptions]
		public void TestWriteXmlToStream_GEN()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<ICFDiComprobanteBuilder>();
			builderMock.Setup(x => x.BuildXml(It.IsAny<TransactionInfo>())).Returns(new Comprobante());

			var cfdiWriter = new CFDiXmlWriter();
			cfdiWriter.SubstituteCFDiXmlBuilder_ForTestOnly(builderMock.Object);
			var writer = (ITransactionBatchToPayloadWriter)cfdiWriter;
			using (var stream = new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				writer.WritePayloadToStream(transactionBatch, stream, MexicoEInvoiceMessageTypeProvider.Codes.GenerateInvoiceRequest, accBatch, new Common.Logger(), new Common.Logger());
				builderMock.Verify(x => x.BuildXml(transactionInfo), Times.Once);

				stream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(stream);
				AssertMultilineASCIIEquals(@"<?xml version=""1.0"" encoding=""utf-8""?><cfdi:Comprobante xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd"" Version=""4.0"" Fecha=""0001-01-01T00:00:00"" SubTotal=""0.00"" Moneda=""AED"" Total=""0.00"" TipoDeComprobante=""I"" Exportacion=""01"" xmlns:cfdi=""http://www.sat.gob.mx/cfd/4"" />", xmlDocument.OuterXml);
			}
		}

		public void TestWriteXmlToStream_CAN()
		{
			var cancellationXml = new XStreamingElement("Cancelacion", new XElement("RFCEmisor", "EMI030201001"),
				new XElement("RFCReceptor", "REC030201001"),
				new XElement("Total", "1000.00"),
				new XElement("UUID", "6F8A72CC-1F7A-49BA-8DAE-EFFDA6FEDA14"),
				new XElement("Certificado", "Y2VydGlmaWNhZG8="),
				new XElement("ClavePrivada", "cGFzc3dvcmQ="));

			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var cancellationMock = new Mock<ICFDiCancellationBuilder>();
			cancellationMock.Setup(x => x.BuildXml(transactionInfo, accBatch)).Returns(cancellationXml);

			var mexicoDependencyMock = new Mock<IMexicoEInvoicingDependencyFactory>();
			mexicoDependencyMock.Setup(x => x.GetCFDiCancellationBuilder()).Returns(cancellationMock.Object);

			var eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			eInvoicinigDependencyMock.Setup(x => x.GetMexicoEInvoicingDependencyFactory()).Returns(mexicoDependencyMock.Object);

			ObjectFactory.Substitute(eInvoicinigDependencyMock.Object);

			var writer = new CFDiXmlWriter() as ITransactionBatchToPayloadWriter;

			using (var stream = new MemoryStream())
			{
				AssertExceptionThrown<ArgumentException>(
					"We must have an exception when government allocated number is not populated for CAN transactions",
					"No government allocated number was found for original AR INV transaction.",
					() => writer.WritePayloadToStream(transactionBatch, stream, MexicoEInvoiceMessageTypeProvider.Codes.GenerateCancellationRequest, accBatch, new Common.Logger(), new Common.Logger())
				);

				accBatch.AIB_GovernmentAllocatedNumber = "A101";
				writer.WritePayloadToStream(transactionBatch, stream, MexicoEInvoiceMessageTypeProvider.Codes.GenerateCancellationRequest, accBatch, new Common.Logger(), new Common.Logger());
				cancellationMock.Verify(x => x.BuildXml(transactionInfo, accBatch), Times.Once);

				stream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(stream);
				XmlComparison.CompareAndAssertXml(@"<?xml version=""1.0"" encoding=""utf-8""?><Cancelacion><RFCEmisor>EMI030201001</RFCEmisor><RFCReceptor>REC030201001</RFCReceptor><Total>1000.00</Total><UUID>6F8A72CC-1F7A-49BA-8DAE-EFFDA6FEDA14</UUID><Certificado>Y2VydGlmaWNhZG8=</Certificado><ClavePrivada>cGFzc3dvcmQ=</ClavePrivada></Cancelacion>", xmlDocument.OuterXml);
			}
		}

		[ExpectNoExceptions]
		public void TestBuilderXMLWithCorrectParameter()
		{
			var obtenerPDFMock = new Mock<ICFDiObtenerPDFBuilder>();
			var returnedObject = new XStreamingElement("Any");
			obtenerPDFMock.Setup(x => x.BuildXml(It.IsAny<List<AuthorizationDetails>>())).Returns(returnedObject);
			var mexicoDependencyMock = new Mock<IMexicoEInvoicingDependencyFactory>();
			mexicoDependencyMock.Setup(x => x.GetCFDiObtenerPDFBuilder()).Returns(obtenerPDFMock.Object);

			var eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			eInvoicinigDependencyMock.Setup(x => x.GetMexicoEInvoicingDependencyFactory()).Returns(mexicoDependencyMock.Object);

			ObjectFactory.Substitute(eInvoicinigDependencyMock.Object);

			AssertParameter(null);
			AssertParameter(new List<AuthorizationDetails>());

			void AssertParameter(List<AuthorizationDetails> authorizationDetails)
			{
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transactionInfo.SetAuthorizationDetailCollection(() => authorizationDetails);
				var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

				transactionBatch.TransactionCollection.Add(transactionInfo);

				var writer = new CFDiXmlWriter() as ITransactionBatchToPayloadWriter;
				using (var stream = new MemoryStream())
				{
					var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
					writer.WritePayloadToStream(transactionBatch, stream, MexicoEInvoiceMessageTypeProvider.Codes.RequestPDFDocumentForInvoice, accBatch, new Common.Logger(), new Common.Logger());

					obtenerPDFMock.Verify(x => x.BuildXml(authorizationDetails));
				}
			}
		}

		public void TestWriteXmlToStream_PDF()
		{
			var obtenerPDFXml = new XStreamingElement("ObtenerPDF", new XElement("UUID", "7E28AACE-54C3-48CD-B74B-DE9B9F946320"));

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var obtenerPDFMock = new Mock<ICFDiObtenerPDFBuilder>();
			obtenerPDFMock.Setup(x => x.BuildXml(It.IsAny<List<AuthorizationDetails>>())).Returns(obtenerPDFXml);

			var mexicoDependencyMock = new Mock<IMexicoEInvoicingDependencyFactory>();
			mexicoDependencyMock.Setup(x => x.GetCFDiObtenerPDFBuilder()).Returns(obtenerPDFMock.Object);

			var eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			eInvoicinigDependencyMock.Setup(x => x.GetMexicoEInvoicingDependencyFactory()).Returns(mexicoDependencyMock.Object);

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				var writer = new CFDiXmlWriter() as ITransactionBatchToPayloadWriter;

				using (var stream = new MemoryStream())
				{
					var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
					writer.WritePayloadToStream(transactionBatch, stream, MexicoEInvoiceMessageTypeProvider.Codes.RequestPDFDocumentForInvoice, accBatch, new Common.Logger(), new Common.Logger());

					stream.Position = 0;
					var xmlDocument = new XmlDocument();
					xmlDocument.Load(stream);
					this.AssertXMLEqualsByDiff(@"<?xml version=""1.0"" encoding=""utf-8""?><ObtenerPDF><UUID>7E28AACE-54C3-48CD-B74B-DE9B9F946320</UUID></ObtenerPDF>", xmlDocument.OuterXml);
				}
			}
		}

		public void TestWriteXmlToStream_IncorrectMessageType()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<ICFDiComprobanteBuilder>();
			builderMock.Setup(x => x.BuildXml(transactionInfo)).Returns(new Comprobante());

			var cfdiWriter = new CFDiXmlWriter();
			cfdiWriter.SubstituteCFDiXmlBuilder_ForTestOnly(builderMock.Object);
			var writer = (ITransactionBatchToPayloadWriter)cfdiWriter;

			using (var stream = new MemoryStream())
			{
				AssertExceptionThrown<ArgumentException>("We must have an exception when the messageType is null", "Invalid Message Type.", () => writer.WritePayloadToStream(transactionBatch, stream, null, accBatch, new Common.Logger(), new Common.Logger()));
				AssertExceptionThrown<ArgumentException>("We must have an exception when the messageType is empty", "Invalid Message Type.", () => writer.WritePayloadToStream(transactionBatch, stream, string.Empty, accBatch, new Common.Logger(), new Common.Logger()));
				AssertExceptionThrown<ArgumentException>("We must have an exception when the messageType is invalid value", "Invalid Message Type.", () => writer.WritePayloadToStream(transactionBatch, stream, "XXX", accBatch, new Common.Logger(), new Common.Logger()));
			}
		}

		public void TestPostProcessorFormatInvoiceLinesDecimals()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<ICFDiComprobanteBuilder>();
			builderMock.Setup(x => x.BuildXml(transactionInfo)).Returns(new Comprobante()
			{
				Moneda = c_Moneda.LYD,
				Conceptos = new[]
				{
					new ComprobanteConcepto()
					{
						ValorUnitario = 100.27m,
						Importe = 100.27m
					},
					new ComprobanteConcepto()
					{
						ValorUnitario = 92.758704m,
						Importe = 92.758704m
					}
				}
			});

			var cfdiWriter = new CFDiXmlWriter();
			cfdiWriter.SubstituteCFDiXmlBuilder_ForTestOnly(builderMock.Object);
			var writer = (ITransactionBatchToPayloadWriter)cfdiWriter;

			using (var testStream = (SubStreamableStream)new MemoryStream())
			{
				writer.WritePayloadToStream(transactionBatch, testStream, MexicoEInvoiceMessageTypeProvider.Codes.GenerateInvoiceRequest, accBatch, new Common.Logger(), new Common.Logger());
				testStream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(testStream);

				var xmlConceptos = xmlDocument?.SelectNodes($"//*[local-name()='Conceptos']");
				AssertEquals("We must have at least one element named Concepto.", 1, xmlConceptos.Count);

				var firstChildNode = xmlConceptos.Item(0).ChildNodes[0];
				var lastChildNode = xmlConceptos.Item(0).ChildNodes[1];

				CombineAssertions("Format Currency values for first element with LYD decimal places.", () =>
				{
					var first = xmlConceptos.Item(0).ChildNodes[0].Attributes["Cantidad"].Value;
					AssertEquals("ValorUnitario attribute should be formatted correctly.", "100.270", firstChildNode.Attributes[nameof(ComprobanteConcepto.ValorUnitario)].Value);
					AssertEquals("Importe attribute should be formatted correctly", "100.270", firstChildNode.Attributes[nameof(ComprobanteConcepto.Importe)].Value);
				});

				CombineAssertions("Format Currency values for last element with LYD decimal places.", () =>
				{
					AssertEquals("ValorUnitario attribute should be formatted correctly.", "92.759", lastChildNode.Attributes[nameof(ComprobanteConcepto.ValorUnitario)].Value);
					AssertEquals("Importe attribute should be formatted correctly", "92.759", lastChildNode.Attributes[nameof(ComprobanteConcepto.Importe)].Value);
				});
			}
		}

		public void TestWriteXmlPostProcessorTipoCambio()
		{
			var localTipoCambio = 19.24m;
			var expectedTipoCambio = "19.240000";
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<ICFDiComprobanteBuilder>();
			builderMock.Setup(x => x.BuildXml(transactionInfo)).Returns(new Comprobante() { TipoCambioSpecified = true, TipoCambio = localTipoCambio });

			var cfdiWriter = new CFDiXmlWriter();
			cfdiWriter.SubstituteCFDiXmlBuilder_ForTestOnly(builderMock.Object);
			var writer = (ITransactionBatchToPayloadWriter)cfdiWriter;

			using (var testStream = (SubStreamableStream)new MemoryStream())
			{
				writer.WritePayloadToStream(transactionBatch, testStream, MexicoEInvoiceMessageTypeProvider.Codes.GenerateInvoiceRequest, accBatch, new Common.Logger(), new Common.Logger());
				testStream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(testStream);

				var node = xmlDocument?.SelectSingleNode($"/*[local-name()='Comprobante']/@TipoCambio");

				AssertNotNull("TipoCambio node should not be null", node);
				AssertEquals("TipoCambio attribute should be formatted with six decimals.", expectedTipoCambio, node.InnerText);
			}
		}

		public void TestWriteXmlPostProcessor_Traslados()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<ICFDiComprobanteBuilder>();
			builderMock.Setup(x => x.BuildXml(transactionInfo)).Returns(ComprobanteObjectForTest);

			var cfdiWriter = new CFDiXmlWriter();
			cfdiWriter.SubstituteCFDiXmlBuilder_ForTestOnly(builderMock.Object);
			var writer = (ITransactionBatchToPayloadWriter)cfdiWriter;

			using (var testStream = (SubStreamableStream)new MemoryStream())
			{
				writer.WritePayloadToStream(transactionBatch, testStream, MexicoEInvoiceMessageTypeProvider.Codes.GenerateInvoiceRequest, accBatch, new Common.Logger(), new Common.Logger());
				testStream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(testStream);

				var traslados = xmlDocument?.SelectNodes($"/*[local-name()='Comprobante']/*[local-name()='Conceptos']/*[local-name()='Concepto']/*[local-name()='Impuestos']/*[local-name()='Traslados']/*[local-name()='Traslado']");

				AssertEquals("Base attribute should be formatted with currency decimals.", "110.00", traslados[0].Attributes[nameof(ComprobanteConceptoImpuestosTraslado.Base)].Value);
				AssertEquals("Importe attribute should be formatted with currency decimals.", "11.44", traslados[0].Attributes[nameof(ComprobanteConceptoImpuestosTraslado.Importe)].Value);
				AssertEquals("TasaOCuota attribute should be formatted with six decimals.", "10.400000", traslados[0].Attributes[nameof(ComprobanteConceptoImpuestosTraslado.TasaOCuota)].Value);

				AssertEquals("Base attribute should be formatted with currency decimals.", "100.00", traslados[1].Attributes[nameof(ComprobanteConceptoImpuestosTraslado.Base)].Value);
				AssertEquals("Importe attribute should be formatted with currency decimals.", "0.04", traslados[1].Attributes[nameof(ComprobanteConceptoImpuestosTraslado.Importe)].Value);
				AssertEquals("TasaOCuota attribute should be formatted with six decimals.", "0.040000", traslados[1].Attributes[nameof(ComprobanteConceptoImpuestosTraslado.TasaOCuota)].Value);

				AssertEquals("Base attribute should be formatted with currency decimals.", "1.00", traslados[2].Attributes[nameof(ComprobanteConceptoImpuestosTraslado.Base)].Value);
				AssertEquals("Importe attribute should be formatted with currency decimals.", "0.20", traslados[2].Attributes[nameof(ComprobanteConceptoImpuestosTraslado.Importe)].Value);
				AssertEquals("TasaOCuota attribute should be formatted with six decimals.", "0.340000", traslados[2].Attributes[nameof(ComprobanteConceptoImpuestosTraslado.TasaOCuota)].Value);
			}
		}

		public void TestWriteXmlPostProcessor_Retenciones()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<ICFDiComprobanteBuilder>();
			builderMock.Setup(x => x.BuildXml(transactionInfo)).Returns(new Comprobante()
			{
				Conceptos = new[]
				{
					new ComprobanteConcepto()
					{
						Impuestos = new ComprobanteConceptoImpuestos()
						{
							Retenciones = new[]
							{
								new ComprobanteConceptoImpuestosRetencion() { Base = 110m, Importe = 11.44m, TasaOCuota = 10.4m },
								new ComprobanteConceptoImpuestosRetencion() { Base = 110.23m, Importe = 8.4m, TasaOCuota = 10.3479m }
							}
						}
					}
				}
			});

			var cfdiWriter = new CFDiXmlWriter();
			cfdiWriter.SubstituteCFDiXmlBuilder_ForTestOnly(builderMock.Object);
			var writer = (ITransactionBatchToPayloadWriter)cfdiWriter;

			using (var testStream = (SubStreamableStream)new MemoryStream())
			{
				writer.WritePayloadToStream(transactionBatch, testStream, MexicoEInvoiceMessageTypeProvider.Codes.GenerateInvoiceRequest, accBatch, new Common.Logger(), new Common.Logger());
				testStream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(testStream);

				var retenciones = xmlDocument?.SelectNodes($"/*[local-name()='Comprobante']/*[local-name()='Conceptos']/*[local-name()='Concepto']/*[local-name()='Impuestos']/*[local-name()='Retenciones']/*[local-name()='Retencion']");

				AssertEquals(retenciones.Count, 2);
				AssertWriteXmlPostProcessor(retenciones[0], "110.00", "11.44", "10.400000");
				AssertWriteXmlPostProcessor(retenciones[1], "110.23", "8.40", "10.347900");
			}

			void AssertWriteXmlPostProcessor(XmlNode retencion, string baseExpected, string importeExpected, string tasaExpected)
			{
				AssertEquals("Base attribute should be formatted with currency decimals.", baseExpected, retencion.Attributes[nameof(ComprobanteConceptoImpuestosRetencion.Base)].Value);
				AssertEquals("Importe attribute should be formatted with currency decimals.", importeExpected, retencion.Attributes[nameof(ComprobanteConceptoImpuestosRetencion.Importe)].Value);
				AssertEquals("TasaOCuota attribute should be formatted with six decimals.", tasaExpected, retencion.Attributes[nameof(ComprobanteConceptoImpuestosRetencion.TasaOCuota)].Value);
			}
		}

		public void TestWriteXmlPostProcessor_ComprobanteImpuestosRetenidos()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<ICFDiComprobanteBuilder>();
			builderMock.Setup(x => x.BuildXml(transactionInfo)).Returns(ComprobanteObjectForTestRetencion);

			var cfdiWriter = new CFDiXmlWriter();
			cfdiWriter.SubstituteCFDiXmlBuilder_ForTestOnly(builderMock.Object);
			var writer = (ITransactionBatchToPayloadWriter)cfdiWriter;

			using (var testStream = (SubStreamableStream)new MemoryStream())
			{
				writer.WritePayloadToStream(transactionBatch, testStream, MexicoEInvoiceMessageTypeProvider.Codes.GenerateInvoiceRequest, accBatch, new Common.Logger(), new Common.Logger());
				testStream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(testStream);

				var comprobanteImpuestosRetencion = xmlDocument?.SelectSingleNode($"/*[local-name()='Comprobante']/*[local-name()='Impuestos']");

				AssertNotNull("ComprobanteImpuestosRetencion node should not be null", comprobanteImpuestosRetencion);
				AssertEquals("TotalImpuestosRetenidos attribute should be formatted with six decimals.", "11.60", comprobanteImpuestosRetencion.Attributes[nameof(ComprobanteImpuestos.TotalImpuestosRetenidos)].Value);

				var retenciones = xmlDocument?.SelectNodes($"/*[local-name()='Comprobante']/*[local-name()='Impuestos']/*[local-name()='Retenciones']/*[local-name()='Retencion']");
				AssertEquals(3, retenciones.Count);

				AssertWriteXmlPostProcessor(retenciones[0], "21.44", "002");
				AssertWriteXmlPostProcessor(retenciones[1], "1.04", "002");
				AssertWriteXmlPostProcessor(retenciones[2], "1.20", "002");

				void AssertWriteXmlPostProcessor(XmlNode retencion, string importeExpected, string impuestoxpected)
				{
					AssertEquals("Importe attribute should be formatted with currency decimals.", importeExpected, retencion.Attributes[nameof(ComprobanteImpuestosRetencion.Importe)].Value);
					AssertEquals("Impuesto attribute should be a fixed value '002'", impuestoxpected, retencion.Attributes[nameof(ComprobanteImpuestosRetencion.Impuesto)].Value);
				}
			}
		}

		public void TestWriteXmlPostProcessor_ComprobanteImpuestosTraslado()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<ICFDiComprobanteBuilder>();
			builderMock.Setup(x => x.BuildXml(transactionInfo)).Returns(ComprobanteObjectForTest);

			var cfdiWriter = new CFDiXmlWriter();
			cfdiWriter.SubstituteCFDiXmlBuilder_ForTestOnly(builderMock.Object);
			var writer = (ITransactionBatchToPayloadWriter)cfdiWriter;

			using (var testStream = (SubStreamableStream)new MemoryStream())
			{
				writer.WritePayloadToStream(transactionBatch, testStream, MexicoEInvoiceMessageTypeProvider.Codes.GenerateInvoiceRequest, accBatch, new Common.Logger(), new Common.Logger());
				testStream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(testStream);

				var comprobanteImpuestosTraslado = xmlDocument?.SelectSingleNode($"/*[local-name()='Comprobante']/*[local-name()='Impuestos']");

				AssertNotNull("ComprobanteImpuestosTraslado node should not be null", comprobanteImpuestosTraslado);
				AssertEquals("TotalImpuestosTrasladados attribute should be formatted with six decimals.", "11.60", comprobanteImpuestosTraslado.Attributes[nameof(ComprobanteImpuestos.TotalImpuestosTrasladados)].Value);

				var traslados = xmlDocument?.SelectNodes($"/*[local-name()='Comprobante']/*[local-name()='Impuestos']/*[local-name()='Traslados']/*[local-name()='Traslado']");
				AssertEquals(3, traslados.Count);

				AssertWriteXmlPostProcessor(traslados[0], "21.44", "11.400000");
				AssertWriteXmlPostProcessor(traslados[1], "1.04", "1.040000");
				AssertWriteXmlPostProcessor(traslados[2], "1.20", "1.343456");

				void AssertWriteXmlPostProcessor(XmlNode retencion, string importeExpected, string tasaExpected)
				{
					AssertEquals("Importe attribute should be formatted with currency decimals.", importeExpected, retencion.Attributes[nameof(ComprobanteImpuestosTraslado.Importe)].Value);
					AssertEquals("TasaOCuota attribute should be formatted with six decimals.", tasaExpected, retencion.Attributes[nameof(ComprobanteImpuestosTraslado.TasaOCuota)].Value);
				}
			}
		}

		public void TestSignatureXmlDocument()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<ICFDiComprobanteBuilder>();
			var comprobante_object = ComprobanteObjectForTest;
			comprobante_object.Total = 100.001234m;
			builderMock.Setup(x => x.BuildXml(transactionInfo)).Returns(comprobante_object);

			var credentialCompany = GetCompanyEInvoicingCredential_ForTestOnly(GlbCompany.CurrentCompany.PK);
			var companyCredentialMock = new Mock<ICompanyCredential>();
			companyCredentialMock.Setup(x => x.GetCompanyCredential(transactionInfo)).Returns(credentialCompany);

			var cfdiWriter = new CFDiXmlWriter();
			cfdiWriter.SubstituteCFDiXmlBuilder_ForTestOnly(builderMock.Object);
			cfdiWriter.SubstituteCompanyCredential_ForTestOnly(companyCredentialMock.Object);

			var signMock = new Mock<ICertificateHelper>();
			cfdiWriter.SubstituteSignXmlDocument_ForTestOnly(signMock.Object);

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(CfdiXml_ForTestOnly);

			var writer = cfdiWriter as ITransactionBatchToPayloadWriter;

			using (var tmpStream = new MemoryStream())
			{
				writer.WritePayloadToStream(transactionBatch, tmpStream, MexicoEInvoiceMessageTypeProvider.Codes.GenerateInvoiceRequest, accBatch, new Common.Logger(), new Common.Logger());

				signMock.Verify(x => x.SignCFDiXmlDocument(It.Is<XmlDocument>(p => p.OuterXml == CfdiXml_ForTestOnly), credentialCompany), Times.Once);
				tmpStream.Position = 0;
				xmlDocument.Load(tmpStream);

				var totalAmoutWithSixDecimalPlaces = xmlDocument?.SelectSingleNode($"/*[local-name()='Comprobante']/@Total");
				AssertEquals("If TotalAmount is formated correctly with two decimals, SignCFDiXmlDocument it's run before Post Processor method.", "100.00", totalAmoutWithSixDecimalPlaces.Value);
			}
		}

		#region Implementation

		Comprobante ComprobanteObjectForTest => new Comprobante()
		{
			Conceptos = new[]
			{
				new ComprobanteConcepto()
				{
					ObjetoImp = c_ObjetoImp.ObjetoImpuesto,
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Traslados = new[]
							{
								new ComprobanteConceptoImpuestosTraslado()
								{
									ImporteSpecified = true,
									Base = 110m,
									Importe = 11.44m,
									TipoFactor = c_TipoFactor.Tasa,
									Impuesto = c_Impuesto.Item002,
									TasaOCuota = 10.4m,
									TasaOCuotaSpecified = true
								},
								new ComprobanteConceptoImpuestosTraslado()
								{
									ImporteSpecified = true,
									Base = 100m,
									Importe = 0.04m,
									TipoFactor = c_TipoFactor.Tasa,
									Impuesto = c_Impuesto.Item002,
									TasaOCuota = 0.04m,
									TasaOCuotaSpecified = true
								},
								new ComprobanteConceptoImpuestosTraslado()
								{
									ImporteSpecified = true,
									Base = 1m,
									Importe = 0.20m,
									TipoFactor = c_TipoFactor.Tasa,
									Impuesto = c_Impuesto.Item002,
									TasaOCuota = 0.34m,
									TasaOCuotaSpecified = true
								},
							}
						}
				}
			},
			Impuestos = new ComprobanteImpuestos()
			{
				TotalImpuestosTrasladados = 11.6m,
				TotalImpuestosTrasladadosSpecified = true,
				Traslados = new[]
				{
					new ComprobanteImpuestosTraslado()
					{
						Base = 188.07m,
						ImporteSpecified = true,
						Importe = 21.44m,
						TasaOCuotaSpecified = true,
						TasaOCuota = 11.4m
					},
					new ComprobanteImpuestosTraslado()
					{
						Base = 1m,
						ImporteSpecified = true,
						Importe = 1.043m,
						TasaOCuotaSpecified = true,
						TasaOCuota = 1.04m
					},
					new ComprobanteImpuestosTraslado()
					{
						Base = 89.32m,
						ImporteSpecified = true,
						Importe = 1.2m,
						TasaOCuotaSpecified = true,
						TasaOCuota = 1.3434562m
					},
				}
			},
		};

		Comprobante ComprobanteObjectForTestRetencion => new Comprobante()
		{
			Conceptos = new[]
			{
				new ComprobanteConcepto()
				{
					Impuestos =
						new ComprobanteConceptoImpuestos()
						{
							Retenciones = new[]
							{
								new ComprobanteConceptoImpuestosRetencion()
								{
									Base = 110m,
									Importe = 11.44m,
									TipoFactor = c_TipoFactor.Tasa,
									Impuesto = c_Impuesto.Item002,
									TasaOCuota = 10.4m
								},
								new ComprobanteConceptoImpuestosRetencion()
								{
									Base = 100m,
									Importe = 0.04m,
									TipoFactor = c_TipoFactor.Tasa,
									Impuesto = c_Impuesto.Item002,
									TasaOCuota = 0.04m
								},
								new ComprobanteConceptoImpuestosRetencion()
								{
									Base = 1m,
									Importe = 0.20m,
									TipoFactor = c_TipoFactor.Tasa,
									Impuesto = c_Impuesto.Item002,
									TasaOCuota = 0.34m
								},
							}
						}
				}
			},

			Impuestos = new ComprobanteImpuestos()
			{
				TotalImpuestosRetenidos = 11.6m,
				TotalImpuestosRetenidosSpecified = true,
				Retenciones = new[]
				{
					new ComprobanteImpuestosRetencion() { Importe = 21.44m, Impuesto = c_Impuesto.Item002 },
					new ComprobanteImpuestosRetencion() { Importe = 1.043m, Impuesto = c_Impuesto.Item002 },
					new ComprobanteImpuestosRetencion() { Importe = 1.2m, Impuesto = c_Impuesto.Item002 },
				}
			}
		};

		GlbCompanyEInvoicingCertificateCredential GetCompanyEInvoicingCredential_ForTestOnly(ZGuid companyPK)
		{
			if (companyPK == ZGuid.Empty)
			{
				return null;
			}

			var credentialForTestOnly = Factory.New<GlbCompanyEInvoicingCertificateCredential>();

			credentialForTestOnly.GP_GC = companyPK;
			credentialForTestOnly.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			credentialForTestOnly.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credentialForTestOnly.GP_CertificateSerialNumber = "123456";
			credentialForTestOnly.GP_IssueDate = ZDateTime.Now.AddDays(-1);
			credentialForTestOnly.GP_ExpiryDate = ZDateTime.Now.AddDays(1);

			return credentialForTestOnly;
		}

		#endregion

		protected override TransactionBatchToPayloadWriterBase GetTestWriter() => new CFDiXmlWriter();

		static string CfdiXml_ForTestOnly => @"<?xml version=""1.0"" encoding=""utf-8""?><cfdi:Comprobante xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd"" Version=""4.0"" Fecha=""0001-01-01T00:00:00"" SubTotal=""0.00"" Moneda=""AED"" Total=""100.00"" TipoDeComprobante=""I"" Exportacion=""01"" xmlns:cfdi=""http://www.sat.gob.mx/cfd/4""><cfdi:Conceptos><cfdi:Concepto Cantidad=""0"" ValorUnitario=""0.00"" Importe=""0.00"" ObjetoImp=""02""><cfdi:Impuestos><cfdi:Traslados><cfdi:Traslado Base=""110.00"" Impuesto=""002"" TipoFactor=""Tasa"" TasaOCuota=""10.400000"" Importe=""11.44"" /><cfdi:Traslado Base=""100.00"" Impuesto=""002"" TipoFactor=""Tasa"" TasaOCuota=""0.040000"" Importe=""0.04"" /><cfdi:Traslado Base=""1.00"" Impuesto=""002"" TipoFactor=""Tasa"" TasaOCuota=""0.340000"" Importe=""0.20"" /></cfdi:Traslados></cfdi:Impuestos></cfdi:Concepto></cfdi:Conceptos><cfdi:Impuestos TotalImpuestosTrasladados=""11.60""><cfdi:Traslados><cfdi:Traslado Base=""188.07"" Impuesto=""001"" TipoFactor=""Tasa"" TasaOCuota=""11.400000"" Importe=""21.44"" /><cfdi:Traslado Base=""1.00"" Impuesto=""001"" TipoFactor=""Tasa"" TasaOCuota=""1.040000"" Importe=""1.04"" /><cfdi:Traslado Base=""89.32"" Impuesto=""001"" TipoFactor=""Tasa"" TasaOCuota=""1.343456"" Importe=""1.20"" /></cfdi:Traslados></cfdi:Impuestos></cfdi:Comprobante>";
	}
}
