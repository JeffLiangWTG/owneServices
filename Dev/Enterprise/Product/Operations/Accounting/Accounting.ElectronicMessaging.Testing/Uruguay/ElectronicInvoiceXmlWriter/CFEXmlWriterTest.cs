using System;
using System.IO;
using System.Xml;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Moq;
using NUnit.Framework;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	[TestedType(typeof(CFEXmlWriter))]
	class CFEXmlWriterTest : TransactionBatchToXmlWriterTest
	{
		[ExpectNoExceptions]
		public void TestWriteXmlToStream()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var builderMock = new Mock<ICFEBuilder>();
			builderMock.Setup(x => x.BuildCFEInfo(transactionInfo)).Returns(new CFEDefType());

			var cfeWriter = new CFEXmlWriter();
			cfeWriter.SubstituteCFEBuilder_ForTestOnly(builderMock.Object);

			var writer = cfeWriter as ITransactionBatchToPayloadWriter;
			using (var stream = new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				writer.WritePayloadToStream(transactionBatch, stream, string.Empty, accBatch, new Common.Logger(), new Common.Logger());
				builderMock.Verify(x => x.BuildCFEInfo(transactionInfo), Times.Once);

				stream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(stream);
				XmlComparison.CompareAndAssertXml(@"<?xml version=""1.0"" encoding=""utf-8""?><CFE xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" version=""1.0"" xmlns=""http://cfe.dgi.gub.uy"" />", xmlDocument.OuterXml);
			}
		}

		public void TestWriteXmlPostProcessorTipoCambioWitheFact()
		{
			var cfe = new CFEDefType()
			{
				Item = new CFEDefTypeEFact()
				{
					Encabezado = new CFEDefTypeEFactEncabezado()
					{
						Totales = new Totales() { TpoMoneda = TipMonType.USD, TpoCambio = 10.50000m, TpoCambioSpecified = true }
					}
				}
			};
			AssertWriteXmlPostProcessor(cfe, "eFact", "10.500");

			(cfe.Item as CFEDefTypeEFact).Encabezado.Totales = new Totales() { TpoMoneda = TipMonType.USD, TpoCambio = 0.51110m, TpoCambioSpecified = true };
			AssertWriteXmlPostProcessor(cfe, "eFact", "0.511");
		}

		public void TestWriteXmlPostProcessorTipoCambioWitheTck()
		{
			var cfe = new CFEDefType()
			{
				Item = new CFEDefTypeETck()
				{
					Encabezado = new CFEDefTypeETckEncabezado()
					{
						Totales = new Totales() { TpoMoneda = TipMonType.USD, TpoCambio = 1.744700m, TpoCambioSpecified = true }
					}
				}
			};
			AssertWriteXmlPostProcessor(cfe, "eTck", "1.745");

			(cfe.Item as CFEDefTypeETck).Encabezado.Totales = new Totales() { TpoMoneda = TipMonType.USD, TpoCambio = 0.523700m, TpoCambioSpecified = true };
			AssertWriteXmlPostProcessor(cfe, "eTck", "0.524");
		}

		void AssertWriteXmlPostProcessor(CFEDefType cfe, string node, string exectedTpCambio)
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var cfeBuilderMock = new Mock<ICFEBuilder>();
			cfeBuilderMock.Setup(x => x.BuildCFEInfo(transactionInfo)).Returns(cfe);
			var xmlWriter = new CFEXmlWriter();
			xmlWriter.SubstituteCFEBuilder_ForTestOnly(cfeBuilderMock.Object);

			var xmlWriterMock = xmlWriter as ITransactionBatchToPayloadWriter;

			using (var testStream = (SubStreamableStream)new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				xmlWriterMock.WritePayloadToStream(transactionBatch, testStream, ZString.Empty, accBatch, new Common.Logger(), new Common.Logger());
				testStream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(testStream);

				var tpoCambio = xmlDocument?.SelectNodes($"/*[local-name()='CFE']/*[local-name()='{node}']/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='TpoCambio']");

				AssertEquals(1, tpoCambio.Count);
				AssertEquals("TpoCambio element should be formatted with 3 decimals.", exectedTpCambio, tpoCambio[0].InnerText);
			}
		}

		public void TestWriteXmlPostProcessorFormatDocumentDateWitheFact()
		{
			var cfe = new CFEDefType()
			{
				Item = new CFEDefTypeEFact()
				{
					Detalle = new Item_Det_Fact[]
					{
						new Item_Det_Fact() { MontoItem = 11.4m },
						new Item_Det_Fact() { MontoItem = 1.189m },
						new Item_Det_Fact() { MontoItem = 8.99999m },
					}
				}
			};

			AssertWriteXmlPostProcessor_FormatDocumentDecimals(cfe, "eFact", "11.40", "1.19", "9.00");
		}

		public void TestWriteXmlPostProcessorFormatDocumentDateWitheTck()
		{
			var cfe = new CFEDefType()
			{
				Item = new CFEDefTypeETck()
				{
					Detalle = new Item_Det_Fact[]
					{
						new Item_Det_Fact() { MontoItem = 21.4m },
						new Item_Det_Fact() { MontoItem = 1.043m },
						new Item_Det_Fact() { MontoItem = 5.348904m },
					}
				}
			};

			AssertWriteXmlPostProcessor_FormatDocumentDecimals(cfe, "eTck", "21.40", "1.04", "5.35");
		}

		void AssertWriteXmlPostProcessor_FormatDocumentDecimals(CFEDefType cfe, string node, string exectedMontoItem1, string exectedMontoItem2, string exectedMontoItem3)
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var cfeBuilderMock = new Mock<ICFEBuilder>();
			cfeBuilderMock.Setup(x => x.BuildCFEInfo(transactionInfo)).Returns(cfe);
			var xmlWriter = new CFEXmlWriter();
			xmlWriter.SubstituteCFEBuilder_ForTestOnly(cfeBuilderMock.Object);

			var xmlWriterMock = xmlWriter as ITransactionBatchToPayloadWriter;

			using (var testStream = (SubStreamableStream)new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				xmlWriterMock.WritePayloadToStream(transactionBatch, testStream, ZString.Empty, accBatch, new Common.Logger(), new Common.Logger());
				testStream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(testStream);

				var montoItem = xmlDocument?.SelectNodes($"/*[local-name()='CFE']/*[local-name()='{node}']/*[local-name()='Detalle']/*[local-name()='Item']/*[local-name()='MontoItem']");

				AssertEquals(3, montoItem.Count);
				AssertEquals("MontoItem element should have two decimals.", exectedMontoItem1, montoItem[0].InnerText);
				AssertEquals("MontoItem element should have two decimals.", exectedMontoItem2, montoItem[1].InnerText);
				AssertEquals("MontoItem element should have two decimals.", exectedMontoItem3, montoItem[2].InnerText);
			}
		}

		#region IVATasaBasicaAndIVATasaMin

		public void TestWriteXmlPostProcessorMntIVATasaBasicaAndMntIVATasaMinWitheFact()
		{
			var cfe = new CFEDefType()
			{
				Item = new CFEDefTypeEFact()
				{
					Encabezado = new CFEDefTypeEFactEncabezado()
					{
						Totales = new Totales() { MntIVATasaBasica = 49.74m, MntIVATasaBasicaSpecified = true, MntIVATasaMin = 5.78m, MntIVATasaMinSpecified = true }
					}
				}
			};

			AssertWriteXmlPostProcessorTotalesIVA(cfe, "eFact", "MntIVATasaBasica", "49.74", 2);
			AssertWriteXmlPostProcessorTotalesIVA(cfe, "eFact", "MntIVATasaMin","5.78", 2);

			(cfe.Item as CFEDefTypeEFact).Encabezado.Totales = new Totales() { MntIVATasaBasica = 123.456m, MntIVATasaBasicaSpecified = true, MntIVATasaMin = 5.789m, MntIVATasaMinSpecified = true };

			AssertWriteXmlPostProcessorTotalesIVA(cfe, "eFact", "MntIVATasaBasica", "123.46", 2);
			AssertWriteXmlPostProcessorTotalesIVA(cfe, "eFact", "MntIVATasaMin", "5.79", 2);
		}

		public void TestWriteXmlPostProcessorMntIVATasaBasicaAndMntIVATasaMinWitheTck()
		{
			var cfe = new CFEDefType()
			{
				Item = new CFEDefTypeETck()
				{
					Encabezado = new CFEDefTypeETckEncabezado()
					{
						Totales = new Totales()	{ MntIVATasaBasica = 49.74m, MntIVATasaBasicaSpecified = true, MntIVATasaMin = 5.78m, MntIVATasaMinSpecified = true }
					}
				}
			};

			AssertWriteXmlPostProcessorTotalesIVA(cfe, "eTck", "MntIVATasaBasica", "49.74", 2);
			AssertWriteXmlPostProcessorTotalesIVA(cfe, "eTck", "MntIVATasaMin", "5.78", 2);

			(cfe.Item as CFEDefTypeETck).Encabezado.Totales = new Totales() { MntIVATasaBasica = 123.456m, MntIVATasaBasicaSpecified = true, MntIVATasaMin = 5.789m, MntIVATasaMinSpecified = true };

			AssertWriteXmlPostProcessorTotalesIVA(cfe, "eTck", "MntIVATasaBasica", "123.46", 2);
			AssertWriteXmlPostProcessorTotalesIVA(cfe, "eTck", "MntIVATasaMin", "5.79", 2);
		}

		void AssertWriteXmlPostProcessorTotalesIVA(CFEDefType cfe, string node, string element, string expected1, int decimals)
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var cfeBuilderMock = new Mock<ICFEBuilder>();
			cfeBuilderMock.Setup(x => x.BuildCFEInfo(transactionInfo)).Returns(cfe);
			var xmlWriter = new CFEXmlWriter();
			xmlWriter.SubstituteCFEBuilder_ForTestOnly(cfeBuilderMock.Object);

			var xmlWriterMock = xmlWriter as ITransactionBatchToPayloadWriter;

			using (var testStream = (SubStreamableStream)new MemoryStream())
			{
				var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				xmlWriterMock.WritePayloadToStream(transactionBatch, testStream, string.Empty, accBatch, new Common.Logger(), new Common.Logger());
				testStream.Position = 0;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(testStream);

				var xmlElement = xmlDocument?.SelectNodes($"/*[local-name()='CFE']/*[local-name()='{node}']/*[local-name()='Encabezado']/*[local-name()='Totales']/*[local-name()='{element}']");

				AssertEquals(xmlElement.Count, 1);
				AssertEquals($"{element} element should be formatted with {decimals} decimals.", expected1, xmlElement[0].InnerText);
			}
		}

		#endregion

		public void TestWriteXmlPostProcessorFormatDocumentTotals()
		{
			var cfe = new CFEDefType()
			{
				Item = new CFEDefTypeEFact()
				{
					Encabezado = new CFEDefTypeEFactEncabezado()
					{
						Totales = new Totales()
						{
							MntNoGrv = 11.4m,
							MntNoGrvSpecified = true,
							MntExpoyAsim = 1.189m,
							MntExpoyAsimSpecified = true,
							MntNetoIvaTasaMin = 8.99999m,
							MntNetoIvaTasaMinSpecified = true,
							MntNetoIVATasaBasica = 0,
							MntNetoIVATasaBasicaSpecified = true
						}
					}
				}
			};

			AssertWriteXmlPostProcessor_FormatDocumentDecimalsTotals("eFact", "11.40", "1.19", "9.00", "0.00");

			cfe = new CFEDefType()
			{
				Item = new CFEDefTypeETck()
				{
					Encabezado = new CFEDefTypeETckEncabezado()
					{
						Totales = new Totales()
						{
							MntNoGrv = 0.1m,
							MntNoGrvSpecified = true,
							MntExpoyAsim = 1.999m,
							MntExpoyAsimSpecified = true,
							MntNetoIvaTasaMin = -5.256m,
							MntNetoIvaTasaMinSpecified = true,
							MntNetoIVATasaBasica = -0.6m,
							MntNetoIVATasaBasicaSpecified = true
						}
					}
				}
			};

			AssertWriteXmlPostProcessor_FormatDocumentDecimalsTotals("eTck", "0.10", "2.00", "-5.26", "-0.60");

			void AssertWriteXmlPostProcessor_FormatDocumentDecimalsTotals(string node, string expectedMntNoGrv, string expectedMntExpoyAsim, string expectedMntNetoIvaTasaMin, string expectedMntNetoIVATasaBasica)
			{
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
				transactionBatch.TransactionCollection.Add(transactionInfo);

				var cfeBuilderMock = new Mock<ICFEBuilder>();
				cfeBuilderMock.Setup(x => x.BuildCFEInfo(transactionInfo)).Returns(cfe);
				var xmlWriter = new CFEXmlWriter();
				xmlWriter.SubstituteCFEBuilder_ForTestOnly(cfeBuilderMock.Object);

				var xmlWriterMock = xmlWriter as ITransactionBatchToPayloadWriter;

				using (var testStream = (SubStreamableStream)new MemoryStream())
				{
					var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
					xmlWriterMock.WritePayloadToStream(transactionBatch, testStream, ZString.Empty, accBatch, new Common.Logger(), new Common.Logger());
					testStream.Position = 0;
					var xmlDocument = new XmlDocument();
					xmlDocument.Load(testStream);

					var total = xmlDocument?.SelectSingleNode($"/*[local-name()='CFE']/*[local-name()='{node}']/*[local-name()='Encabezado']/*[local-name()='Totales']");

					AssertNotNull(total);
					AssertEquals(expectedMntNoGrv, total["MntNoGrv"].InnerText);
					AssertEquals(expectedMntExpoyAsim, total["MntExpoyAsim"].InnerText);
					AssertEquals(expectedMntNetoIvaTasaMin, total["MntNetoIvaTasaMin"].InnerText);
					AssertEquals(expectedMntNetoIVATasaBasica, total["MntNetoIVATasaBasica"].InnerText);
				}
			}
		}

		protected override TransactionBatchToPayloadWriterBase GetTestWriter() => new CFEXmlWriter();

		protected override Type GetExpectedPayloadValidationType() => null;
	}
}
