using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;
using CodeDescriptionPairCore = Enterprise.ZArchitecture.Core.CodeDescriptionPair;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class CFDiComprobanteBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<CFDiComprobanteBuilder>(new CFDiXmlWriter().CFDiXmlBuilder_ExposedForTestOnly);
		}

		public void TestGetTipoComprobanteTDR()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;
			transaction.ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TDR;

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals(c_TipoDeComprobante.I, comprobante.TipoDeComprobante);
		}

		public void TestGetTipoComprobanteTXI()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals(c_TipoDeComprobante.I, comprobante.TipoDeComprobante);
		}

		public void TestGetTipoComprobanteTCR()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;
			transaction.ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TCR;

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals(c_TipoDeComprobante.E, comprobante.TipoDeComprobante);
		}

		public void TestGetTipoComprobanteNotInformed()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			AssertExceptionThrown<ArgumentException>("Compliance Sub Type must have a value.", () => builder.BuildXml(transaction));
		}

		public void TestGetTipoComprobanteNotAllowed()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;
			transaction.ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.XCL;

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			AssertExceptionThrown<ArgumentException>("Invalid Compliance Sub Type.", () => builder.BuildXml(transaction));
		}

		public void TestBuildComprobanteInfo_WithTransactionInForeignCurrency()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;
			transaction.ExchangeRate = 19.24m;
			transaction.OSCurrency = new Currency { Code = "AUD" };

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals(c_Moneda.AUD, comprobante.Moneda);
			AssertEquals(true, comprobante.TipoCambioSpecified);
			AssertEquals(19.24m, comprobante.TipoCambio);
		}

		public void TestBuildComprobanteInfo_WithTransactionInLocalCurrency()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;
			transaction.OSCurrency = new Currency { Code = "MXN" };

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals(c_Moneda.MXN, comprobante.Moneda);
			AssertEquals(false, comprobante.TipoCambioSpecified);
			AssertEquals(0m, comprobante.TipoCambio);
		}

		public void TestBuildComprobanteInfo_TransactionWithoutOsCurrency()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals("The code XXX is used to denote a 'transaction' involving no currency.", c_Moneda.XXX, comprobante.Moneda);
		}

		public void TestBuildComprobanteInfo_TransactionWithIncorrectOsCurrency()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;
			transaction.OSCurrency = new Currency { Code = "Z1Z" };

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals("The code XXX is used to denote a 'transaction' involving no currency.", c_Moneda.XXX, comprobante.Moneda);
		}

		[TestDate(2020, 5, 26, 14, 36, 00)]
		public void TestBuildComprobanteInfo_WithTransactionInTransactionDateAndPostcode()
		{
			var expectedPostCode = "06100";
			var expectedDate = ZDateTime.Now.AddDays(-3);

			var branchAddress = new OrganizationAddress();
			branchAddress.Postcode = expectedPostCode;

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TDR,
				TransactionDate = expectedDate,
				BranchAddress = branchAddress
			};

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals(expectedDate, comprobante.Fecha);
			AssertEquals(expectedPostCode, comprobante.LugarExpedicion);
		}

		public void TestBuildComprobanteInfo_WithEmptyTransaction()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TDR;

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals(ZDateTime.Empty, comprobante.Fecha);
			AssertNullOrEmpty(comprobante.LugarExpedicion);
			AssertNull(comprobante.Folio);
		}

		public void TestBuildComprobanteInfo_NullBranchAddress()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TDR,
				BranchAddress = new OrganizationAddress()
			};

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertNullOrEmpty("The BranchAddress not can be null or empty for Mexico", comprobante.LugarExpedicion);
		}

		#region MetodoPago

		public void TestMetodoPago_ReturnedValues()
		{
			TransactionInfo transaction = BaseTransactionInfo_ForTestOnly;

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			var testMetodoPagoCases = new[]
			{
				new { expectedMetodoPago = c_MetodoPago.PUE, mockMetodoPagoValue = new CodeDescriptionPairCore("PUE", "Pago en una sola exhibición") },
				new { expectedMetodoPago = c_MetodoPago.PPD, mockMetodoPagoValue = new CodeDescriptionPairCore("PPD","Pago en Parcialidades o Diferido") },
				new { expectedMetodoPago = c_MetodoPago.PUE,mockMetodoPagoValue = new CodeDescriptionPairCore("","") },
				new { expectedMetodoPago = c_MetodoPago.PUE,mockMetodoPagoValue = (CodeDescriptionPairCore)null }
			};

			foreach (var testMetodoPago in testMetodoPagoCases)
			{
				AssertMetodoPago(testMetodoPago.expectedMetodoPago, testMetodoPago.mockMetodoPagoValue);
			}

			void AssertMetodoPago(c_MetodoPago? expectedMetodoPagoCode, CodeDescriptionPairCore mockexpectedMetodoPago)
			{
				using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
				{
					mockIAccountingCountryFactory.As<IInvoicePaymentMethodProvider>().Setup(x => x.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(It.IsAny<ZString>(), It.IsAny<ZDateTime?>(), It.IsAny<ZDateTime?>())).Returns(() => mockexpectedMetodoPago);

					mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

					transaction.InvoiceTerm = InvoiceTermType.MIC;

					var builder = new CFDiComprobanteBuilder() as ICFDiComprobanteBuilder;
					var comprobante = builder.BuildXml(transaction);

					AssertEquals(nameof(comprobante.MetodoPago), expectedMetodoPagoCode, comprobante.MetodoPago);
				}
			}
		}

		public void TestMetodoPago_WithNull_InvoiceTerm()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;
			transaction.InvoiceTerm = null;

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals("MetodoPago should not be informed.", false, comprobante.MetodoPagoSpecified);
		}

		#endregion MetodoPago

		#region FormaPago

		public void TestFormaPago_ValidAndExisitingCodes()
		{
			{
				TransactionInfo transaction = BaseTransactionInfo_ForTestOnly;

				var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
				var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

				var testFormaPagoCases = new[]
				{
					new { expectedFormaPago =c_FormaPago.Efectivo ,mockFormaPagoValue= new CodeDescriptionPairCore("01", "Efectivo") },
					new { expectedFormaPago =c_FormaPago.ChequeNominativo ,mockFormaPagoValue= new CodeDescriptionPairCore("02", "Cheque nominativo") },
					new { expectedFormaPago =c_FormaPago.TransferenciaElectrónicaDeFondos ,mockFormaPagoValue= new CodeDescriptionPairCore("03", "Transferencia electrónica de fondos") },
					new { expectedFormaPago =c_FormaPago.TarjetaDeCredito ,mockFormaPagoValue= new CodeDescriptionPairCore("04", "Tarjeta de crédito") },
					new { expectedFormaPago =c_FormaPago.MonederoElectronico ,mockFormaPagoValue= new CodeDescriptionPairCore("05", "Monedero electrónico") },
					new { expectedFormaPago =c_FormaPago.DineroElectronico ,mockFormaPagoValue= new CodeDescriptionPairCore("06", "Dinero electrónico") },
					new { expectedFormaPago =c_FormaPago.ValesDeDespensa ,mockFormaPagoValue= new CodeDescriptionPairCore("08", "Vales de despensa") },
					new { expectedFormaPago =c_FormaPago.DacionEnPago ,mockFormaPagoValue= new CodeDescriptionPairCore("12", "Dación en pago") },
					new { expectedFormaPago =c_FormaPago.PagoPorSubrogacion ,mockFormaPagoValue= new CodeDescriptionPairCore("13", "Pago por subrogación") },
					new { expectedFormaPago =c_FormaPago.PagoPorConsignacion ,mockFormaPagoValue= new CodeDescriptionPairCore("14", "Pago por consignación") },
					new { expectedFormaPago =c_FormaPago.Condonacion ,mockFormaPagoValue= new CodeDescriptionPairCore("15", "Condonación") },
					new { expectedFormaPago =c_FormaPago.Compensacion ,mockFormaPagoValue= new CodeDescriptionPairCore("17", "Compensación") },
					new { expectedFormaPago =c_FormaPago.Novacion ,mockFormaPagoValue = new CodeDescriptionPairCore("23", "Novación") },
					new { expectedFormaPago =c_FormaPago.Confusion ,mockFormaPagoValue = new CodeDescriptionPairCore("24", "Confusión") },
					new { expectedFormaPago =c_FormaPago.RemisionDeDeuda ,mockFormaPagoValue= new CodeDescriptionPairCore("25", "Remisión de deuda") },
					new { expectedFormaPago =c_FormaPago.PrescipcionOCaducidad ,mockFormaPagoValue= new CodeDescriptionPairCore("26", "Prescripción o caducidad") },
					new { expectedFormaPago =c_FormaPago.ASatisfaccionDelAcreedor ,mockFormaPagoValue= new CodeDescriptionPairCore("27", "A satisfacción del acreedor") },
					new { expectedFormaPago =c_FormaPago.TarjetaDeDebito ,mockFormaPagoValue= new CodeDescriptionPairCore("28", "Tarjeta de débito") },
					new { expectedFormaPago =c_FormaPago.TarjetaDeServicios ,mockFormaPagoValue= new CodeDescriptionPairCore("29", "Tarjeta de servicios") },
					new { expectedFormaPago =c_FormaPago.AmpliacionDeAnticipos ,mockFormaPagoValue= new CodeDescriptionPairCore("30", "Aplicación de anticipos") },
					new { expectedFormaPago =c_FormaPago.IntermediarioDePagos ,mockFormaPagoValue= new CodeDescriptionPairCore("31", "Intermediario pagos") },
					new { expectedFormaPago =c_FormaPago.PorDefinir ,mockFormaPagoValue= new CodeDescriptionPairCore("99", "Por definir") },
					new { expectedFormaPago =c_FormaPago.Efectivo ,mockFormaPagoValue= (CodeDescriptionPairCore)null },
					new { expectedFormaPago =c_FormaPago.Efectivo ,mockFormaPagoValue= new CodeDescriptionPairCore("", "") },
				};

				foreach (var testCase in testFormaPagoCases)
				{
					AssertFormadePago(testCase.expectedFormaPago, testCase.mockFormaPagoValue);
				}

				void AssertFormadePago(c_FormaPago? expectedFormaPago, CodeDescriptionPairCore mockFormaPagoValue)
				{
					using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
					{
						mockIAccountingCountryFactory.As<IEquivalentAgreedPaymentMethodProvider>().Setup(x => x.GetEquivalentAgreedPaymentMethodProvider().GetEquivalentAgreedPaymentMethod(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(() => mockFormaPagoValue);
						mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

						var builder = new CFDiComprobanteBuilder() as ICFDiComprobanteBuilder;
						var comprobante = builder.BuildXml(transaction);

						AssertEquals(nameof(comprobante.FormaPago), expectedFormaPago, comprobante.FormaPago);
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestFormPago_GetEquivalentAgreedPaymentMethod_FromTransactionAgreedPaymentMethodParameter()
		{
			TransactionInfo transaction = BaseTransactionInfo_ForTestOnly;

			var metodoPago = "PUE";
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			AssertTransactionAgreedPaymentMethodParameter("CBC");
			AssertTransactionAgreedPaymentMethodParameter("");

			void AssertTransactionAgreedPaymentMethodParameter(ZString agreedPaymentMethod)
			{
				using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
				{
					transaction.AgreedPaymentMethod = agreedPaymentMethod;

					mockIAccountingCountryFactory.As<IEquivalentAgreedPaymentMethodProvider>().Setup(x => x.GetEquivalentAgreedPaymentMethodProvider().GetEquivalentAgreedPaymentMethod(It.IsAny<ZString>(), It.IsAny<ZString>()));

					mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

					var builder = new CFDiComprobanteBuilder() as ICFDiComprobanteBuilder;
					var comprobante = builder.BuildXml(transaction);

					mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(Core.Constants.CountryCodes.Mexico), Times.Once);

					mockIAccountingCountryFactory.As<IEquivalentAgreedPaymentMethodProvider>().Verify(x => x.GetEquivalentAgreedPaymentMethodProvider()
						.GetEquivalentAgreedPaymentMethod(agreedPaymentMethod, metodoPago), Times.Once());

					mockIGlobalAccountingCountryFactory.Reset();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestFormPago_GetEquivalentAgreedPayment_MethodFromPaymentMethodParameter()
		{
			TransactionInfo transaction = BaseTransactionInfo_ForTestOnly;
			transaction.AgreedPaymentMethod = "";
			transaction.InvoiceTerm = InvoiceTermType.INV;

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			AssertPaymentMethodParameter(new CodeDescriptionPairCore("PPD", "Pago en Parcialidades o Diferido"), "PPD");
			AssertPaymentMethodParameter(new CodeDescriptionPairCore("PUE", "Pago en una sola exhibición"), "PUE");
			AssertPaymentMethodParameter(new CodeDescriptionPairCore("", ""), "PUE");

			void AssertPaymentMethodParameter(CodeDescriptionPairCore mockMetodoPagoValue, string metodoPago)
			{
				using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
				{
					mockIAccountingCountryFactory.As<IInvoicePaymentMethodProvider>().Setup(x => x.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(It.IsAny<ZString>(), It.IsAny<ZDateTime?>(), It.IsAny<ZDateTime?>())).Returns(() => mockMetodoPagoValue);

					mockIAccountingCountryFactory.As<IEquivalentAgreedPaymentMethodProvider>().Setup(x => x.GetEquivalentAgreedPaymentMethodProvider().GetEquivalentAgreedPaymentMethod(transaction.AgreedPaymentMethod.Value, It.IsAny<ZString>()));

					mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

					var builder = new CFDiComprobanteBuilder() as ICFDiComprobanteBuilder;
					var comprobante = builder.BuildXml(transaction);

					mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(Core.Constants.CountryCodes.Mexico), Times.Exactly(2));

					mockIAccountingCountryFactory.As<IEquivalentAgreedPaymentMethodProvider>().Verify(x => x.GetEquivalentAgreedPaymentMethodProvider()
					.GetEquivalentAgreedPaymentMethod(transaction.AgreedPaymentMethod.Value, metodoPago), Times.Once());

					mockIGlobalAccountingCountryFactory.Reset();
					mockIAccountingCountryFactory.Reset();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestFormPago_GetEquivalentAgreedPayment_MethodFromPaymentMethodParameter_When_InvoiceTerm_IsNull()
		{
			TransactionInfo transaction = BaseTransactionInfo_ForTestOnly;
			transaction.AgreedPaymentMethod = "";

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			AssertPaymentMethodParameter(new CodeDescriptionPairCore("PUE", "Pago en una sola exhibición"), "PUE");

			void AssertPaymentMethodParameter(CodeDescriptionPairCore mockMetodoPagoValue, string metodoPago)
			{
				using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
				{
					mockIAccountingCountryFactory.As<IInvoicePaymentMethodProvider>().Setup(x => x.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(It.IsAny<ZString>(), It.IsAny<ZDateTime?>(), It.IsAny<ZDateTime?>())).Returns(() => mockMetodoPagoValue);

					mockIAccountingCountryFactory.As<IEquivalentAgreedPaymentMethodProvider>().Setup(x => x.GetEquivalentAgreedPaymentMethodProvider().GetEquivalentAgreedPaymentMethod(transaction.AgreedPaymentMethod.Value, It.IsAny<ZString>()));

					mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

					var builder = new CFDiComprobanteBuilder() as ICFDiComprobanteBuilder;
					var comprobante = builder.BuildXml(transaction);

					mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(Core.Constants.CountryCodes.Mexico), Times.Once);

					mockIAccountingCountryFactory.As<IEquivalentAgreedPaymentMethodProvider>().Verify(x => x.GetEquivalentAgreedPaymentMethodProvider()
						.GetEquivalentAgreedPaymentMethod(transaction.AgreedPaymentMethod.Value, metodoPago), Times.Once());
				}
			}
		}

		public void TestFormaPago_WithNull_AgreedPaymentMethod()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;
			transaction.AgreedPaymentMethod = null;

			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals("FormaPago should not be informed.", c_FormaPago.PorDefinir, comprobante.FormaPago);
		}

		#endregion FormaPago

		public void TestBuildXml()
		{
			var accBatch = Factory.New<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TXI,
				Branch = new Branch()
			};

			ComprobanteConcepto[] conceptos = null;
			ComprobanteEmisor emisor = null;
			ComprobanteReceptor receptor = null;
			ComprobanteImpuestos impuestos = null;
			ComprobanteCfdiRelacionados[] relacionados = null;

			AssertBuildXml(conceptos, emisor, receptor, impuestos, relacionados);

			conceptos = Array.Empty<ComprobanteConcepto>();
			emisor = new ComprobanteEmisor();
			receptor = new ComprobanteReceptor();
			impuestos = new ComprobanteImpuestos();
			relacionados = new ComprobanteCfdiRelacionados[]
			{
				new ComprobanteCfdiRelacionados()
				{
					CfdiRelacionado = Array.Empty<ComprobanteCfdiRelacionadosCfdiRelacionado>()
				}
			};

			AssertBuildXml(conceptos, emisor, receptor, impuestos, relacionados);

			void AssertBuildXml(ComprobanteConcepto[] expConceptos, ComprobanteEmisor expEmisor, ComprobanteReceptor expReceptor, ComprobanteImpuestos expImpuestos, ComprobanteCfdiRelacionados[] expRelacionados)
			{
				var conceptoMock = new Mock<ICFDiConceptoBuilder>();
				conceptoMock.Setup(x => x.BuildComprobanteConceptoInfo(It.IsAny<TransactionInfo>(), It.IsAny<BusinessObjectFactory>())).Returns(expConceptos);

				var emisorMock = new Mock<ICFDiEmisorBuilder>();
				emisorMock.Setup(x => x.BuildEmisorInfo(It.IsAny<TransactionInfo>())).Returns(expEmisor);

				var receptorMock = new Mock<ICFDiReceptorBuilder>();
				receptorMock.Setup(x => x.BuildReceptorInfo(It.IsAny<TransactionInfo>())).Returns(expReceptor);

				var impuestoMock = new Mock<ICFDiImpuestosBuilder>();
				impuestoMock.Setup(x => x.BuildComprobanteImpuestosInfo(It.IsAny<ComprobanteConcepto[]>())).Returns(expImpuestos);

				var relacionadosMock = new Mock<ICFDiRelacionadosBuilder>();
				relacionadosMock.Setup(x => x.BuildRelacionadosInfo(It.IsAny<TransactionInfo>())).Returns(expRelacionados);

				var mexicoDependencyMock = new Mock<IMexicoEInvoicingDependencyFactory>();
				mexicoDependencyMock.Setup(x => x.GetCFDiRelacionadosBuilder()).Returns(relacionadosMock.Object);

				var eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
				eInvoicinigDependencyMock.Setup(x => x.GetMexicoEInvoicingDependencyFactory()).Returns(mexicoDependencyMock.Object);

				using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
				{
					var cfdiBuilder = new CFDiComprobanteBuilder();

					#region Obsolete, use MexicoEInvocingDependencies for new dependencies

					cfdiBuilder.SubstituteFactory_ForTestOnly(Factory);
					cfdiBuilder.SubstituteCFDiConceptoBuilder_ForTestOnly(conceptoMock.Object);
					cfdiBuilder.SubstituteCFDiEmisorBuilder_ForTestOnly(emisorMock.Object);
					cfdiBuilder.SubstituteCFDiReceptorBuilder_ForTestOnly(receptorMock.Object);
					cfdiBuilder.SubstituteCFDiImpuestosBuilder_ForTestOnly(impuestoMock.Object);

					#endregion Obsolete, use MexicoEInvocingDependencies for new dependencies

					var builder = cfdiBuilder as ICFDiComprobanteBuilder;
					var comprobante = builder.BuildXml(transactionInfo);

					AssertEquals(expConceptos, comprobante.Conceptos);
					AssertEquals(expEmisor, comprobante.Emisor);
					AssertEquals(expReceptor, comprobante.Receptor);
					AssertEquals(expImpuestos, comprobante.Impuestos);
					AssertEquals(expRelacionados, comprobante.CfdiRelacionados);
					AssertEquals(nameof(comprobante.Version), "4.0", comprobante.Version);
					AssertEquals(nameof(comprobante.Exportacion), c_Exportacion.NoAplica, comprobante.Exportacion);
					AssertEquals(nameof(comprobante.schemaLocation), "http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd", comprobante.schemaLocation);

					conceptoMock.Verify(x => x.BuildComprobanteConceptoInfo(transactionInfo, Factory), Times.Once);
					emisorMock.Verify(x => x.BuildEmisorInfo(transactionInfo), Times.Once);
					receptorMock.Verify(x => x.BuildReceptorInfo(transactionInfo), Times.Once);
					impuestoMock.Verify(x => x.BuildComprobanteImpuestosInfo(comprobante.Conceptos), Times.Once);
					relacionadosMock.Verify(x => x.BuildRelacionadosInfo(transactionInfo), Times.Once);

					mexicoDependencyMock.Verify(x => x.GetCFDiRelacionadosBuilder(), Times.Once);
					eInvoicinigDependencyMock.Verify(x => x.GetMexicoEInvoicingDependencyFactory(), Times.Once);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestBuildComprobanteImpuestosInfo()
		{
			var taxID = new TaxID() { TaxType = new CodeDescriptionPair() { Code = "RAT" }, ExtraTaxType = new CodeDescriptionPair() { Code = "REF" } };

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
				ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TXI
			};
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>()
				{
					new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 155.25m , VATTaxID = taxID , OSExtraVATAmount = 10.00m }
			});

			var comprobanteConcepto = Array.Empty<ComprobanteConcepto>();

			var conceptoMock = new Mock<ICFDiConceptoBuilder>();
			var impuestoMock = new Mock<ICFDiImpuestosBuilder>();

			conceptoMock.Setup(x => x.BuildComprobanteConceptoInfo(transactionInfo, Factory)).Returns(comprobanteConcepto);

			var cfdiBuilder = new CFDiComprobanteBuilder();
			cfdiBuilder.SubstituteCFDiConceptoBuilder_ForTestOnly(conceptoMock.Object);
			cfdiBuilder.SubstituteCFDiImpuestosBuilder_ForTestOnly(impuestoMock.Object);

			var builder = cfdiBuilder as ICFDiComprobanteBuilder;
			var comprobante = builder.BuildXml(transactionInfo);

			impuestoMock.Verify(x => x.BuildComprobanteImpuestosInfo(comprobanteConcepto), Times.Once);
		}

		public void TestSubTotalAndTotalAmounts_WithTaxes()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;
			var localComprobanteConceptoObject_ForTestOnly = ComprobanteConceptoObject_ForTestOnly;

			var conceptoMock = new Mock<ICFDiConceptoBuilder>();
			var cfdiBuilder = new CFDiComprobanteBuilder();
			var builder = cfdiBuilder as ICFDiComprobanteBuilder;

			(decimal SubTotal, decimal Total)[] expectedAmountsForAssertions = {
				(6000m, 6488m),
				(2700m, 2792m),
				(1000m, 1160m),
				(0m, 0m),
				(0m, 0m)
			};

			for (var i = 0; i <= 4; i++)
			{
				conceptoMock.Setup(x => x.BuildComprobanteConceptoInfo(transaction, Factory)).Returns(localComprobanteConceptoObject_ForTestOnly.Skip(i).ToArray());
				cfdiBuilder.SubstituteFactory_ForTestOnly(Factory);
				cfdiBuilder.SubstituteCFDiConceptoBuilder_ForTestOnly(conceptoMock.Object);
				var comprobante = builder.BuildXml(transaction);

				AssertEquals(nameof(comprobante.Total), expectedAmountsForAssertions[i].Total, comprobante.Total);
				AssertEquals(nameof(comprobante.SubTotal), expectedAmountsForAssertions[i].SubTotal, comprobante.SubTotal);
			}
		}

		public void TestSubTotalAndTotalAmounts_WithoutTaxes()
		{
			var transaction = BaseTransactionInfo_ForTestOnly;

			var cfdiBuilder = new CFDiComprobanteBuilder();
			var builder = cfdiBuilder as ICFDiComprobanteBuilder;

			var comprobante = builder.BuildXml(transaction);
			AssertEquals(0m, comprobante.SubTotal);
			AssertEquals(0m, comprobante.Total);

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = null }
			});

			comprobante = builder.BuildXml(transaction);
			AssertEquals(0m, comprobante.SubTotal);
			AssertEquals(0m, comprobante.Total);

			transaction.PostingJournalCollection[0].OSAmount = -10.68m;
			comprobante = builder.BuildXml(transaction);
			AssertEquals(-10.68m, comprobante.SubTotal);
			AssertEquals(-10.68m, comprobante.Total);

			transaction.PostingJournalCollection[0].OSAmount = 570.21m;
			comprobante = builder.BuildXml(transaction);
			AssertEquals(570.21m, comprobante.SubTotal);
			AssertEquals(570.21m, comprobante.Total);

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 570.21m },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 29.79m }
			});
			comprobante = builder.BuildXml(transaction);
			AssertEquals(600m, comprobante.SubTotal);
			AssertEquals(600m, comprobante.Total);
		}

		public void TestTransactionFolio()
		{
			var number = "TRN00001";
			var transaction = BaseTransactionInfo_ForTestOnly;
			transaction.Number = number;
			var builder = (ICFDiComprobanteBuilder)new CFDiComprobanteBuilder();
			var comprobante = builder.BuildXml(transaction);

			AssertEquals(number, comprobante.Folio);
		}

		#region Implementation

		TransactionInfo BaseTransactionInfo_ForTestOnly => new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
		{
			TransactionType = TransactionType.INV,
			ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TXI
		};

		ComprobanteConcepto[] ComprobanteConceptoObject_ForTestOnly => new ComprobanteConcepto[]
		{
			new ComprobanteConcepto()
			{
				Importe = 3300m,
				Impuestos = new ComprobanteConceptoImpuestos()
				{
					Retenciones = new[] { new ComprobanteConceptoImpuestosRetencion { Importe = 132m } },
					Traslados = new[] { new ComprobanteConceptoImpuestosTraslado() { Importe = 528m, TasaOCuota = 0.16m } }
				}
			},
			new ComprobanteConcepto()
			{
				Importe = 1700m,
				Impuestos = new ComprobanteConceptoImpuestos()
				{
					Retenciones = new[] { new ComprobanteConceptoImpuestosRetencion() { Importe = 68m, TasaOCuota = 0.04m } }
				}
			},
			new ComprobanteConcepto()
			{
				Importe = 1000m,
				Impuestos = new ComprobanteConceptoImpuestos()
				{
					Traslados = new[] { new ComprobanteConceptoImpuestosTraslado() { Importe = 160m, TasaOCuota = 0.16m } }
				}
			},
			new ComprobanteConcepto()
			{
				Impuestos = null
			},
			new ComprobanteConcepto()
			{
				Impuestos = new ComprobanteConceptoImpuestos()
				{
					Retenciones = null,
					Traslados = null
				}
			},
		};

		#endregion Implementation
	}
}
