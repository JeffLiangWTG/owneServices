using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class CFDiConceptoBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<CFDiConceptoBuilder>(new CFDiComprobanteBuilder().CFDiConceptoBuilder_ExposedForTestOnly);
		}

		public void TestTransactionsWith_NullableTransactionTypeAndOSAmount()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = null,
			};

			var taxRate = MexicoEInvoicingTestHelper.CreateTaxID(MexicoConstants.TaxCodes.IVAREC, 10, "RAT", "RET");
			var line = MexicoEInvoicingTestHelper.CreatePostingJournal(taxRate, 100m, 10);

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
					line
			});

			var comprobanteConcepto = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory).First();
			AssertEquals(nameof(ComprobanteConcepto.Importe), 0m, comprobanteConcepto.Importe);
			AssertEquals(nameof(ComprobanteConcepto.ValorUnitario), 0m, comprobanteConcepto.ValorUnitario);
			AssertNull(nameof(ComprobanteConcepto.Impuestos), comprobanteConcepto.Impuestos);

			transaction.TransactionType = TransactionType.INV;
			comprobanteConcepto = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory).First();
			AssertEquals(nameof(ComprobanteConcepto.Importe), 100m, comprobanteConcepto.Importe);
			AssertEquals(nameof(ComprobanteConcepto.ValorUnitario), 100m, comprobanteConcepto.ValorUnitario);
			AssertNotNull(nameof(ComprobanteConcepto.Impuestos), comprobanteConcepto.Impuestos);

			transaction.PostingJournalCollection[0].OSAmount = null;
			comprobanteConcepto = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory).First();
			AssertEquals(nameof(ComprobanteConcepto.Importe), 0m, comprobanteConcepto.Importe);
			AssertEquals(nameof(ComprobanteConcepto.ValorUnitario), 0m, comprobanteConcepto.ValorUnitario);
			AssertNotNull(nameof(ComprobanteConcepto.Impuestos), comprobanteConcepto.Impuestos);
		}

		public void TestBuildComprobanteConceptoInfo_TransactionWithNullPostingJournalCollection()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertNull((new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory));
		}

		public void TestBuildComprobanteConceptoInfo_TransactionWithEmptyPostingJournalCollection()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			AssertNull((new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory));
		}

		public void TestBuildComprobanteConceptoInfo_ImmutableFields()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
			};
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
				{
					new PostingJournal() { OSAmount = 1m , OSGSTVATAmount = 10.00m  }
				});
			var immutableConcepto = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory);

			AssertEquals(nameof(ComprobanteConcepto.Cantidad), 1m, immutableConcepto.First().Cantidad);
			AssertEquals(nameof(ComprobanteConcepto.ClaveUnidad), "E48", immutableConcepto.First().ClaveUnidad);
		}

		public void TestBuildComprobanteConceptoInfo_TransactionWithTwoPostingJournalsInCollection()
		{
			var taxID = new TaxID() { TaxType = new CodeDescriptionPair() { Code = "REF" }, ExtraTaxType = new CodeDescriptionPair() { Code = "REF" } };
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
			};
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
				{
					new PostingJournal() { OSAmount = -50.0000m, VATTaxID = taxID , OSGSTVATAmount = 10.00m },
					new PostingJournal() { OSAmount = 155.25m , VATTaxID = taxID , OSGSTVATAmount = 10.00m }
			});

			var conceptos = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory);
			AssertEquals("Conceptos array should have two elements.", 2, conceptos.Length);

			var firstConcepto = conceptos.First();
			var secondConcepto = conceptos.Last();

			CombineAssertions("First one element of Conceptos array", () =>
			{
				AssertEquals(nameof(ComprobanteConcepto.Importe), -50.00m, firstConcepto.Importe);
				AssertEquals(nameof(ComprobanteConcepto.ValorUnitario), -50.00m, firstConcepto.ValorUnitario);
			});

			CombineAssertions("Second one element of Conceptos array", () =>
			{
				AssertEquals(nameof(ComprobanteConcepto.Importe), 155.25m, secondConcepto.Importe);
				AssertEquals(nameof(ComprobanteConcepto.ValorUnitario), 155.25m, secondConcepto.ValorUnitario);
			});
		}

		public void Test_CreditNote_MultiplesPostingJournal_NegativeAndPositiveOSAmountsAndDistinctDecimals()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.CRD,
			};
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
				{
					new PostingJournal() { OSAmount = -75.1800m , OSGSTVATAmount = 10.00m },
					new PostingJournal() { OSAmount = 34.810m , OSGSTVATAmount = 10.00m },
					new PostingJournal() { OSAmount = -91.2m , OSGSTVATAmount = 10.00m }
				});

			var builderMock = new Mock<ICFDiConceptoBuilder>();
			builderMock.Setup(x => x.BuildComprobanteConceptoInfo(transaction, Factory)).Returns(new ComprobanteConcepto[]
			{
				new ComprobanteConcepto() { ValorUnitario = 75.18m, Importe = 75.18m },
				new ComprobanteConcepto() { ValorUnitario = -34.81m, Importe = -31.81m },
				new ComprobanteConcepto() { ValorUnitario = 91.20m, Importe = 91.20m },
			});

			var conceptos = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory);
			AssertEquals("Conceptos array should have three elements in row.", 3, conceptos.Length);

			var firstResult = conceptos.First();
			var secondResult = conceptos.Skip(1).Take(1).Single();
			var thirdResult = conceptos.Skip(2).Take(1).Single();

			CombineAssertions("First one element of Conceptos array", () =>
			{
				AssertEquals(nameof(ComprobanteConcepto.Importe), 75.18m, firstResult.Importe);
				AssertEquals(nameof(ComprobanteConcepto.ValorUnitario), 75.18m, firstResult.ValorUnitario);
			});

			CombineAssertions("Second one element of Conceptos array", () =>
			{
				AssertEquals(nameof(ComprobanteConcepto.Importe), -34.81m, secondResult.Importe);
				AssertEquals(nameof(ComprobanteConcepto.ValorUnitario), -34.81m, secondResult.ValorUnitario);
			});

			CombineAssertions("Last element of Conceptos array", () =>
			{
				AssertEquals(nameof(ComprobanteConcepto.Importe), 91.20m, thirdResult.Importe);
				AssertEquals(nameof(ComprobanteConcepto.ValorUnitario), 91.20m, thirdResult.Importe);
			});
		}

		public void TestBuildComprobanteConceptoInfo_TransactionWithPostingJournalCollection_WithMixedCommentChargeCode()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode.ChargeType = new CodeDescriptionPair() { Code = "CC1" };
			line1.LocalAmount = 100m;

			var line2 = CreatePostingJournal();
			line2.Sequence = 2;
			line2.ChargeCode.ChargeType = new CodeDescriptionPair() { Code = "CMT" };
			line2.LocalAmount = 10m;

			transaction.PostingJournalCollection.Add(line1);
			transaction.PostingJournalCollection.Add(line2);

			var conceptos = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory);
			AssertEquals("Conceptos array should have only one element.", 1, conceptos.Length);

			AssertEquals(100m, line1.LocalAmount);
			AssertEquals("CC1", line1.ChargeCode.ChargeType.Code);
		}

		public void TestBuildComprobanteConceptoInfo_TransactionWithPostingJournalCollection_OnlyCommentChargeCodeAndNullOrEmptyCode()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			var line1 = CreatePostingJournal();
			line1.Sequence = 1;
			line1.ChargeCode.ChargeType = new CodeDescriptionPair() { Code = "CMT" };
			line1.LocalAmount = 10m;

			var line2 = CreatePostingJournal();
			line2.Sequence = 2;
			line2.ChargeCode.ChargeType = new CodeDescriptionPair() { Code = null };
			line2.LocalAmount = 22m;

			var line3 = CreatePostingJournal();
			line3.Sequence = 3;
			line3.ChargeCode.ChargeType = new CodeDescriptionPair() { Code = string.Empty };
			line3.LocalAmount = 33m;

			var line4 = CreatePostingJournal();
			line4.Sequence = 4;
			line4.ChargeCode = null;
			line4.LocalAmount = 44m;

			transaction.PostingJournalCollection.Add(line1);
			transaction.PostingJournalCollection.Add(line2);
			transaction.PostingJournalCollection.Add(line3);
			transaction.PostingJournalCollection.Add(line4);

			var conceptos = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory);
			AssertEquals("Conceptos array should have only three empty or null elements.", 3, conceptos.Length);
		}

		[ExpectNoExceptions]
		public void TestBuildConcepto()
		{
			var journal = new PostingJournal();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
			};
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>() { journal });

			var conceptoImpuestoMock = new Mock<ICFDiConceptoImpuestosBuilder>();
			var expectedDecimals = 5;
			var conceptoBuilder = new CFDiConceptoBuilder();
			conceptoBuilder.SubstituteConceptoImpuestoBuilder_ForTestOnly(conceptoImpuestoMock.Object);

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			transactionInfoHelperMock.Setup(x => x.GetOSCurrencyDecimals(Factory, transactionInfo)).Returns(5);
			conceptoBuilder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			var builder = conceptoBuilder as ICFDiConceptoBuilder;
			builder.BuildComprobanteConceptoInfo(transactionInfo, Factory);

			conceptoImpuestoMock.Verify(x => x.BuildComprobanteConceptoImpuestosInfo(journal, TransactionType.INV, expectedDecimals), Times.Once);
			transactionInfoHelperMock.Verify(x => x.GetOSCurrencyDecimals(Factory, transactionInfo), Times.Once);
		}

		#region ObjetoImp Attribute

		public void TestComprobanteConcepto_ObjetoImpAttribute()
		{
			var listEnum = Enum.GetValues(typeof(c_ObjetoImp)).ToList<c_ObjetoImp>();
			AssertEquals("c_ObjetoImp Enum should have 3 elements.", 3, listEnum.Count);
		}

		public void TestBuildComprobanteConceptoInfo_WithTaxes_WithTransladosAndRetenciones_IsObjetoImp()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
			};

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				MexicoEInvoicingTestHelper.CreatePostingJournal(
					MexicoEInvoicingTestHelper.CreateTaxID("IVAREF", 16, "RAT", "REF"), 951.42M, 152.23M, 101.48M)
			});

			var conceptos = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory);
			AssertEquals("If the charge line has taxes, ObjetoImp attribute must be 02.", c_ObjetoImp.ObjetoImpuesto, conceptos[0].ObjetoImp);
		}

		public void TestBuildComprobanteConceptoInfo_WithTaxes_WithTranslados_IsObjetoImp()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
			};

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				MexicoEInvoicingTestHelper.CreatePostingJournal(
					MexicoEInvoicingTestHelper.CreateTaxID("IVA", 16, "RAT"), 110M, 16M)
			});

			var conceptos = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory);
			AssertEquals("If the charge line has taxes, ObjetoImp attribute should be 02.", c_ObjetoImp.ObjetoImpuesto, conceptos[0].ObjetoImp);
		}

		public void TestBuildComprobanteConceptoInfo_WithoutTaxes_IsNoObjetoImp()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
			};

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				MexicoEInvoicingTestHelper.CreatePostingJournal(
					MexicoEInvoicingTestHelper.CreateTaxID("NOTREPORT", 0, "NOT"), 100, 0)
			});

			var conceptos = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory);
			AssertEquals("If the charge line has not taxes, ObjetoImp attribute should be 01.", c_ObjetoImp.NoObjetoImpuesto, conceptos[0].ObjetoImp);
		}

		public void TestBuildComprobanteConceptoInfo_MoreThanOneChargeLine_ObjetoImp()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
			};

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				MexicoEInvoicingTestHelper.CreatePostingJournal(MexicoEInvoicingTestHelper.CreateTaxID("IVAREF", 16, "RAT", "REF"), 951.42M, 152.23M, 101.48M),
				MexicoEInvoicingTestHelper.CreatePostingJournal(MexicoEInvoicingTestHelper.CreateTaxID("NOTREPORT", 0, "NOT"), 100, 0M),
				MexicoEInvoicingTestHelper.CreatePostingJournal(MexicoEInvoicingTestHelper.CreateTaxID("IVA", 16, "RAT"), 110M, 16M),
				MexicoEInvoicingTestHelper.CreatePostingJournal(MexicoEInvoicingTestHelper.CreateTaxID("EXEMPT", 0, "EXT"), 80M, 0M),
			});

			var conceptos = (new CFDiConceptoBuilder() as ICFDiConceptoBuilder).BuildComprobanteConceptoInfo(transaction, Factory);
			for (int index = 0; index < conceptos.Length; index++)
			{
				if (conceptos[index].Impuestos != null)
				{
					AssertEquals("If the charge line has retenciones or translados, ObjetoImp attribute should be 02.", c_ObjetoImp.ObjetoImpuesto, conceptos[index].ObjetoImp);
				}
				else
				{
					AssertEquals("If the charge line has not Taxes, ObjetoImp attribute should be 01.", c_ObjetoImp.NoObjetoImpuesto, conceptos[index].ObjetoImp);
				}
			}
		}

		#endregion

		PostingJournal CreatePostingJournal()
		{
			var result = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			result.ChargeCode = new ChargeCode();
			return result;
		}
	}
}
