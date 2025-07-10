using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.CountryCompliance.UruguayComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	public class IdDocumentoBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<IdDocumentoBuilder>(new EncabezadoBuilder().IdDocumentoBuilder_ExposedForTestOnly);
		}

		public void TestBuildIdDocumento_InvalidComplianceSubType()
		{
			var idDocBuilder = new IdDocumentoBuilder() as IIdDocumentoBuilder;

			TransactionInfo transactionInfo = null;
			AssertInvalidComplianceSubType_eFactura();
			AssertInvalidComplianceSubType_eTicket();

			transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertInvalidComplianceSubType_eFactura();
			AssertInvalidComplianceSubType_eTicket();

			transactionInfo.ComplianceSubType = ComplianceSubTypeCodes.TXI;
			var idFactura = idDocBuilder.BuildEFacIdDocumento(transactionInfo);
			AssertNotNull(nameof(IdDoc_Fact.TipoCFE), idFactura);

			transactionInfo.ComplianceSubType = ComplianceSubTypeCodes.TKT;
			var idTick = idDocBuilder.BuildETicketIdDocumento(transactionInfo);
			AssertNotNull(nameof(IdDoc_Fact.TipoCFE), idTick);

			transactionInfo.ComplianceSubType = "XXX";
			AssertInvalidComplianceSubType_eFactura();
			AssertInvalidComplianceSubType_eTicket();

			void AssertInvalidComplianceSubType_eFactura()
			{
				idFactura = idDocBuilder.BuildEFacIdDocumento(transactionInfo);
				AssertNull(nameof(IdDoc_Fact.TipoCFE), idFactura);
			}

			void AssertInvalidComplianceSubType_eTicket()
			{
				idTick = idDocBuilder.BuildETicketIdDocumento(transactionInfo);
				AssertNull(nameof(IdDoc_Fact.TipoCFE), idTick);
			}
		}

		public void TestBuildEFacIdDocumento_TransactionWithComplianceSubType()
		{
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.TXI, IdDoc_FactTipoCFE.Item111);
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.TCR, IdDoc_FactTipoCFE.Item112);
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.TCD, IdDoc_FactTipoCFE.Item113);
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.YXI, IdDoc_FactTipoCFE.Item211);
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.YCR, IdDoc_FactTipoCFE.Item212);
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.YCD, IdDoc_FactTipoCFE.Item213);

			void AssertEqualsTipoCFE(string subType, IdDoc_FactTipoCFE expectedTipoCFE)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { ComplianceSubType = subType };
				var idDocumentoBuilder = (new IdDocumentoBuilder() as IIdDocumentoBuilder).BuildEFacIdDocumento(transaction);

				AssertEquals(nameof(IdDoc_Fact.TipoCFE), expectedTipoCFE, idDocumentoBuilder.TipoCFE);
			}
		}

		public void TestBuildETicketIdDocumento_TransactionWithComplianceSubType()
		{
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.TKT, IdDoc_TckTipoCFE.Item101);
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.TKC, IdDoc_TckTipoCFE.Item102);
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.TKD, IdDoc_TckTipoCFE.Item103);
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.YKT, IdDoc_TckTipoCFE.Item201);
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.YKR, IdDoc_TckTipoCFE.Item202);
			AssertEqualsTipoCFE(ComplianceSubTypeCodes.YKD, IdDoc_TckTipoCFE.Item203);

			void AssertEqualsTipoCFE(string subType, IdDoc_TckTipoCFE expectedTipoCFE)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { ComplianceSubType = subType };
				var idDocumentoBuilder = (new IdDocumentoBuilder() as IIdDocumentoBuilder).BuildETicketIdDocumento(transaction);

				AssertEquals(nameof(IdDoc_Fact.TipoCFE), expectedTipoCFE, idDocumentoBuilder.TipoCFE);
			}
		}

		public void TestInvoiceSerieAndNumber()
		{
			var cfeHelperMock = new Mock<ICFEHelper>();
			cfeHelperMock.Setup(x => x.GetInvoiceSerieAndNumber(It.IsAny<ZString>())).Returns(("A", "1007"));

			var docBuilder = new IdDocumentoBuilder();
			docBuilder.SubstituteCFEHelper_ForTestOnly(cfeHelperMock.Object);
			var builder = docBuilder as IIdDocumentoBuilder;

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionReference = "A1007";
			var uruguayCompSubTypes = typeof(ComplianceSubTypeCodes).GetConstantValues();

			AssertInvoiceSerieAndNumber_eFactura();
			AssertInvoiceSerieAndNumber_eTicket();

			void AssertInvoiceSerieAndNumber_eFactura()
			{
				var eFacturaContingencySubTypes = new string[] { "YXI", "YCR", "YCD" };

				foreach (var subType in uruguayCompSubTypes)
				{
					cfeHelperMock.Invocations.Clear();

					transaction.ComplianceSubType = subType;
					transaction.TransactionReference = transactionReference;

					var idDocumento = builder.BuildEFacIdDocumento(transaction);

					if (eFacturaContingencySubTypes.Contains(subType))
					{
						cfeHelperMock.Verify(x => x.GetInvoiceSerieAndNumber(transactionReference), Times.Once);

						AssertEquals("A", idDocumento.Serie);
						AssertEquals("1007", idDocumento.Nro);
					}
					else
					{
						cfeHelperMock.Verify(x => x.GetInvoiceSerieAndNumber(transactionReference), Times.Never);

						AssertNull(idDocumento?.Serie);
						AssertNull(idDocumento?.Nro);
					}
				}
			}

			void AssertInvoiceSerieAndNumber_eTicket()
			{
				var eTicketContingencySubTypes = new string[] { "YKT", "YKR", "YKD" };

				foreach (var subType in uruguayCompSubTypes)
				{
					transaction.ComplianceSubType = subType;
					transaction.TransactionReference = transactionReference;

					var idDocumento = builder.BuildETicketIdDocumento(transaction);

					if (eTicketContingencySubTypes.Contains(subType))
					{
						AssertEquals("A", idDocumento?.Serie);
						AssertEquals("1007", idDocumento?.Nro);
					}
					else
					{
						AssertNull(idDocumento?.Serie);
						AssertNull(idDocumento?.Nro);
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestEFacBuildXml()
		{
			var cfeHelperMock = new Mock<ICFEHelper>();

			var transactionFac = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { ComplianceSubType = ComplianceSubTypeCodes.YXI };
			var idDocumentoBuilder = new IdDocumentoBuilder();

			idDocumentoBuilder.SubstituteCFEHelper_ForTestOnly(cfeHelperMock.Object);

			var builder = (IIdDocumentoBuilder)idDocumentoBuilder;
			builder.BuildEFacIdDocumento(transactionFac);

			cfeHelperMock.Verify(x => x.GetInvoiceSerieAndNumber(It.IsAny<ZString>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestETicketBuildXml()
		{
			var cfeHelperMock = new Mock<ICFEHelper>();

			var transactionTick = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { ComplianceSubType = ComplianceSubTypeCodes.YKT };
			var idDocumentoBuilder = new IdDocumentoBuilder();

			idDocumentoBuilder.SubstituteCFEHelper_ForTestOnly(cfeHelperMock.Object);

			var builder = (IIdDocumentoBuilder)idDocumentoBuilder;
			builder.BuildETicketIdDocumento(transactionTick);

			cfeHelperMock.Verify(x => x.GetInvoiceSerieAndNumber(It.IsAny<ZString>()), Times.Once);
		}

		public void TestIdDocumentoBuilder_FchEmis()
		{
			var date = new ZDateTime(2020, 5, 26, 14, 36, 10);

			AssertEquals_FchEmis_eFactura(date, date);
			AssertEquals_FchEmis_eTicket(date, date);

			AssertEquals_FchEmis_eFactura(null, ZDateTime.Empty);
			AssertEquals_FchEmis_eTicket(null, ZDateTime.Empty);

			void AssertEquals_FchEmis_eFactura(ZDateTime? transactionDate, ZDateTime? expectedDate)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionDate = transactionDate, ComplianceSubType = ComplianceSubTypeCodes.TXI };
				var idDocumentoBuilder = (new IdDocumentoBuilder() as IIdDocumentoBuilder).BuildEFacIdDocumento(transaction);

				AssertEquals(expectedDate, idDocumentoBuilder.FchEmis);
			}

			void AssertEquals_FchEmis_eTicket(ZDateTime? transactionDate, ZDateTime? expectedDate)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionDate = transactionDate, ComplianceSubType = ComplianceSubTypeCodes.TKC };
				var idDocumentoBuilder = (new IdDocumentoBuilder() as IIdDocumentoBuilder).BuildETicketIdDocumento(transaction);

				AssertEquals(expectedDate, idDocumentoBuilder.FchEmis);
			}
		}

		public void TestIdDocumentoBuilder_FmaPago()
		{
			AssertEquals_FchEmis_eFactura(InvoiceTermType.COD, IdDoc_FactFmaPago.Item1);
			AssertEquals_FchEmis_eFactura(InvoiceTermType.PIA, IdDoc_FactFmaPago.Item1);
			AssertEquals_FchEmis_eFactura(InvoiceTermType.INV, IdDoc_FactFmaPago.Item2);
			AssertEquals_FchEmis_eFactura(InvoiceTermType.DPC, IdDoc_FactFmaPago.Item2);
			AssertEquals_FchEmis_eFactura(null, IdDoc_FactFmaPago.Item1);

			AssertEquals_FchEmis_eTicket(InvoiceTermType.COD, IdDoc_TckFmaPago.Item1);
			AssertEquals_FchEmis_eTicket(InvoiceTermType.PIA, IdDoc_TckFmaPago.Item1);
			AssertEquals_FchEmis_eTicket(InvoiceTermType.INV, IdDoc_TckFmaPago.Item2);
			AssertEquals_FchEmis_eTicket(InvoiceTermType.DPC, IdDoc_TckFmaPago.Item2);
			AssertEquals_FchEmis_eTicket(null, IdDoc_TckFmaPago.Item1);

			void AssertEquals_FchEmis_eFactura(InvoiceTermType? invoiceTermType, IdDoc_FactFmaPago expectedIdDocFactFmaPago)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { InvoiceTerm = invoiceTermType, ComplianceSubType = ComplianceSubTypeCodes.TXI };
				var idDocumentoBuilder = (new IdDocumentoBuilder() as IIdDocumentoBuilder).BuildEFacIdDocumento(transaction);

				AssertEquals(expectedIdDocFactFmaPago, idDocumentoBuilder.FmaPago);
			}

			void AssertEquals_FchEmis_eTicket(InvoiceTermType? invoiceTermType, IdDoc_TckFmaPago expectedIdDocFactFmaPago)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { InvoiceTerm = invoiceTermType, ComplianceSubType = ComplianceSubTypeCodes.TKC };
				var idDocumentoBuilder = (new IdDocumentoBuilder() as IIdDocumentoBuilder).BuildETicketIdDocumento(transaction);

				AssertEquals(expectedIdDocFactFmaPago, idDocumentoBuilder.FmaPago);
			}
		}

		public void TestIdDocumentoBuilder_IndPagCta3rosSpecified()
		{
			var postingJournals = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					VATTaxID = new TaxID() { TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } }
				},
			};

			AssertEquals_FchEmis_eFactura(postingJournals, false);
			AssertEquals_FchEmis_eTicket(postingJournals, false);

			postingJournals = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					VATTaxID = new TaxID() { TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } }
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					VATTaxID = new TaxID()
					{
						TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.ExcludedFromTheTaxBase }
					}
				}
			};

			AssertEquals_FchEmis_eFactura(postingJournals, true);
			AssertEquals_FchEmis_eTicket(postingJournals, true);

			postingJournals = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					VATTaxID = new TaxID()
					{
						TaxType = new CodeDescriptionPair() { Code = null }
					}
				}
			};

			AssertEquals_FchEmis_eFactura(postingJournals, false);
			AssertEquals_FchEmis_eTicket(postingJournals, false);

			postingJournals = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					VATTaxID = new TaxID() { }
				}
			};

			AssertEquals_FchEmis_eFactura(postingJournals, false);
			AssertEquals_FchEmis_eTicket(postingJournals, false);

			postingJournals = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { }
			};

			AssertEquals_FchEmis_eFactura(postingJournals, false);
			AssertEquals_FchEmis_eTicket(postingJournals, false);

			postingJournals = null;

			AssertEquals_FchEmis_eFactura(postingJournals, false);
			AssertEquals_FchEmis_eTicket(postingJournals, false);

			void AssertEquals_FchEmis_eFactura(List<PostingJournal> postingJournalCollection, bool espectedIndPagCta3rosSpecified)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { ComplianceSubType = ComplianceSubTypeCodes.TXI };
				transaction.SetPostingJournalCollection(() => postingJournalCollection);
				var idDocumentoBuilder = (new IdDocumentoBuilder() as IIdDocumentoBuilder).BuildEFacIdDocumento(transaction);

				AssertEquals(espectedIndPagCta3rosSpecified, idDocumentoBuilder.IndPagCta3rosSpecified);
			}

			void AssertEquals_FchEmis_eTicket(List<PostingJournal> postingJournalCollection, bool espectedIndPagCta3rosSpecified)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { ComplianceSubType = ComplianceSubTypeCodes.TKC };
				transaction.SetPostingJournalCollection(() => postingJournalCollection);
				var idDocumentoBuilder = (new IdDocumentoBuilder() as IIdDocumentoBuilder).BuildETicketIdDocumento(transaction);

				AssertEquals(espectedIndPagCta3rosSpecified, idDocumentoBuilder.IndPagCta3rosSpecified);
			}
		}

		public void TestIdDocumentoBuilder_FchVenc()
		{
			AssertEquals_FchVenc_eFactura(null, ZDateTime.Empty, false);
			AssertEquals_FchVenc_eTicket(null, ZDateTime.Empty, false);

			var expectedDate = new DateTime(2022, 6, 16, 15, 44, 10);

			AssertEquals_FchVenc_eFactura(expectedDate, expectedDate, true);
			AssertEquals_FchVenc_eTicket(expectedDate, expectedDate, true);

			void AssertEquals_FchVenc_eFactura(ZDateTime? dueDate, ZDateTime expectedFchVenc, bool expectedFchVencSpecified)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { ComplianceSubType = ComplianceSubTypeCodes.YXI, DueDate = dueDate };
				var idDoc = (new IdDocumentoBuilder() as IIdDocumentoBuilder).BuildEFacIdDocumento(transaction);

				AssertEquals(nameof(idDoc.FchVencSpecified), expectedFchVencSpecified, idDoc.FchVencSpecified);
				AssertEquals(nameof(idDoc.FchVenc), expectedFchVenc, idDoc.FchVenc);
			}

			void AssertEquals_FchVenc_eTicket(ZDateTime? dueDate, ZDateTime expectedFchVenc, bool expectedFchVencSpecified)
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { ComplianceSubType = ComplianceSubTypeCodes.YKT, DueDate = dueDate };
				var idDoc = (new IdDocumentoBuilder() as IIdDocumentoBuilder).BuildETicketIdDocumento(transaction);

				AssertEquals(nameof(idDoc.FchVencSpecified), expectedFchVencSpecified, idDoc.FchVencSpecified);
				AssertEquals(nameof(idDoc.FchVenc), expectedFchVenc, idDoc.FchVenc);
			}
		}
	}
}
