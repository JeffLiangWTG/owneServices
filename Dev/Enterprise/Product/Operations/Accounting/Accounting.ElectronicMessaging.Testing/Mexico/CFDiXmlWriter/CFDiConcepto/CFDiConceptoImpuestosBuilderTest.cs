using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class CFDiConceptoImpuestosBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<CFDiConceptoImpuestosBuilder>(new CFDiConceptoBuilder().ConceptoImpuestoBuilder_ExposedForTestOnly);
		}

		public void TestComprobanteConceptoImpuestosWithOutTaxCodeIVA4()
		{
			var testPossibleCases = new[]
			{
				new { TaxTypeCode = "CAP", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="CAPIVA", TransactionType = TransactionType.INV, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "CAP", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="CAPIVA", TransactionType = TransactionType.INV, OSAmount = -100, oSGSTVATAmount = -10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "CAP", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="CAPIVA", TransactionType = TransactionType.INV, OSAmount = 100, oSGSTVATAmount = -10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "CAP", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="CAPIVA", TransactionType = TransactionType.INV, OSAmount = -100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },

				new { TaxTypeCode = "CAP", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="CAPIVA", TransactionType = TransactionType.CRD, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "CAP", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="CAPIVA", TransactionType = TransactionType.CRD, OSAmount = -100, oSGSTVATAmount = -10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "CAP", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="CAPIVA", TransactionType = TransactionType.CRD, OSAmount = 100, oSGSTVATAmount = -10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "CAP", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="CAPIVA", TransactionType = TransactionType.CRD, OSAmount = -100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },

				new { TaxTypeCode = "EXT", ExtraTaxTypeCode = "", TaxRate = 0, TaxCode="EXEMPT", TransactionType = TransactionType.INV, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 0, ExpectedTasaOcuota = 0.0,  ExpectedcTipoFactor= c_TipoFactor.Exento,  ExpectedSpecified = false },
				new { TaxTypeCode = "EXT", ExtraTaxTypeCode = "", TaxRate = 0, TaxCode="EXEMPT", TransactionType = TransactionType.INV, OSAmount = -100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = 0, ExpectedTasaOcuota = 0.0,  ExpectedcTipoFactor= c_TipoFactor.Exento,  ExpectedSpecified = false },

				new { TaxTypeCode = "EXT", ExtraTaxTypeCode = "", TaxRate = 0, TaxCode="EXEMPT", TransactionType = TransactionType.CRD, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = 0, ExpectedTasaOcuota = 0.0,  ExpectedcTipoFactor= c_TipoFactor.Exento,  ExpectedSpecified = false },
				new { TaxTypeCode = "EXT", ExtraTaxTypeCode = "", TaxRate = 0, TaxCode="EXEMPT", TransactionType = TransactionType.CRD, OSAmount = -100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 0, ExpectedTasaOcuota = 0.0,  ExpectedcTipoFactor= c_TipoFactor.Exento,  ExpectedSpecified = false },

				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "RET", TaxRate = 16, TaxCode="IVAREB", TransactionType = TransactionType.INV, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "RET", TaxRate = 16, TaxCode="IVAREB", TransactionType = TransactionType.INV, OSAmount = -100, oSGSTVATAmount = 10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = 30, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "RET", TaxRate = 16, TaxCode="IVAREB", TransactionType = TransactionType.INV, OSAmount = 100, oSGSTVATAmount = -10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "RET", TaxRate = 16, TaxCode="IVAREB", TransactionType = TransactionType.INV, OSAmount = -100, oSGSTVATAmount = -10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = -30, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },

				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "RET", TaxRate = 16, TaxCode="IVAREB", TransactionType = TransactionType.CRD, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "RET", TaxRate = 16, TaxCode="IVAREB", TransactionType = TransactionType.CRD, OSAmount = -100, oSGSTVATAmount = 10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = -30, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "RET", TaxRate = 16, TaxCode="IVAREB", TransactionType = TransactionType.CRD, OSAmount = 100, oSGSTVATAmount = -10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "RET", TaxRate = 16, TaxCode="IVAREB", TransactionType = TransactionType.CRD, OSAmount = -100, oSGSTVATAmount = -10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 30, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },

				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "REF", TaxRate = 8, TaxCode="IVAREF", TransactionType = TransactionType.INV, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.08,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "REF", TaxRate = 8, TaxCode="IVAREF", TransactionType = TransactionType.INV, OSAmount = -100, oSGSTVATAmount = 10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = 30, ExpectedTasaOcuota = 0.08,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "REF", TaxRate = 8, TaxCode="IVAREF", TransactionType = TransactionType.INV, OSAmount = 100, oSGSTVATAmount = -10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.08,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "REF", TaxRate = 8, TaxCode="IVAREF", TransactionType = TransactionType.INV, OSAmount = -100, oSGSTVATAmount = -10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = -30, ExpectedTasaOcuota = 0.08,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },

				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "REF", TaxRate = 16, TaxCode="IVAREF", TransactionType = TransactionType.CRD, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "REF", TaxRate = 16, TaxCode="IVAREF", TransactionType = TransactionType.CRD, OSAmount = -100, oSGSTVATAmount = 10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = -30, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "REF", TaxRate = 16, TaxCode="IVAREF", TransactionType = TransactionType.CRD, OSAmount = 100, oSGSTVATAmount = -10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "REF", TaxRate = 16, TaxCode="IVAREF", TransactionType = TransactionType.CRD, OSAmount = -100, oSGSTVATAmount = -10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 30, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },

				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "", TaxRate = 0, TaxCode="FREEIVA", TransactionType = TransactionType.INV, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.0,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "", TaxRate = 0, TaxCode="FREEIVA", TransactionType = TransactionType.INV, OSAmount = -100, oSGSTVATAmount = -10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.0,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },

				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "", TaxRate = 0, TaxCode="FREEIVA", TransactionType = TransactionType.CRD, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.0,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "", TaxRate = 0, TaxCode="FREEIVA", TransactionType = TransactionType.CRD, OSAmount = -100, oSGSTVATAmount = -10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.0,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },

				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="IVA", TransactionType = TransactionType.INV, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="IVA", TransactionType = TransactionType.INV, OSAmount = -100, oSGSTVATAmount = -10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },

				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="IVA", TransactionType = TransactionType.CRD, OSAmount = 100, oSGSTVATAmount = 10, oSExtraVATAmount = 20 , ExpectedCount = 1, ExpectedBase = -100, ExpectedImporte = -10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
				new { TaxTypeCode = "RAT", ExtraTaxTypeCode = "", TaxRate = 16, TaxCode="IVA", TransactionType = TransactionType.CRD, OSAmount = -100, oSGSTVATAmount = -10, oSExtraVATAmount = -20 , ExpectedCount = 1, ExpectedBase = 100, ExpectedImporte = 10, ExpectedTasaOcuota = 0.16,  ExpectedcTipoFactor= c_TipoFactor.Tasa,  ExpectedSpecified = true },
			};

			for (int index = 0; index < testPossibleCases.Length; index++)
			{
				var testConfiguration = testPossibleCases[index];
				var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
				var taxRate = CreateTaxID(testConfiguration.TaxCode, testConfiguration.TaxRate, testConfiguration.TaxTypeCode, testConfiguration.ExtraTaxTypeCode);
				var line = CreatePostingJournal(taxRate, testConfiguration.OSAmount, testConfiguration.oSGSTVATAmount, testConfiguration.oSExtraVATAmount);
				var comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, testConfiguration.TransactionType, 2);
				AssertComprobanteConceptoImpuestos($"REC #{index}", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], testConfiguration.ExpectedCount, testConfiguration.ExpectedBase, testConfiguration.ExpectedcTipoFactor, testConfiguration.ExpectedTasaOcuota, testConfiguration.ExpectedImporte, testConfiguration.ExpectedSpecified);
			}
		}

		public void TestComprobanteConceptoImpuestosWithTaxCodeIVA4andIVAREC()
		{
			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
			var taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVA4, 10, "RAT");
			var line = CreatePostingJournal(taxRate, 100, 10);
			var comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, 25, c_TipoFactor.Tasa, 0.16, 10, true);
			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, 75, c_TipoFactor.Tasa, 0, 0, true);

			taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVA4, 10, "RAT");
			line = CreatePostingJournal(taxRate, -100, -10);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, -25, c_TipoFactor.Tasa, 0.16, -10, true);
			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, -75, c_TipoFactor.Tasa, 0, 0, true);

			taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVA4, 10, "RAT");
			line = CreatePostingJournal(taxRate, 100, 10);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.CRD, 2);

			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, -25, c_TipoFactor.Tasa, 0.16, -10, true);
			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, -75, c_TipoFactor.Tasa, 0, 0, true);

			taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVA4, 10, "RAT");
			line = CreatePostingJournal(taxRate, -100, -10);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.CRD, 2);

			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, 25, c_TipoFactor.Tasa, 0.16, 10, true);
			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, 75, c_TipoFactor.Tasa, 0, 0, true);

			taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVAREC, 10, "RAT", "RET");
			line = CreatePostingJournal(taxRate, 100, 10);

			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertComprobanteConceptoImpuestos("IVAREC:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, 25, c_TipoFactor.Tasa, 0.16, 10, true);
			AssertComprobanteConceptoImpuestos("IVAREC:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, 75, c_TipoFactor.Tasa, 0, 0, true);

			taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVAREC, 10, "RAT", "RET");
			line = CreatePostingJournal(taxRate, -100, -10);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertComprobanteConceptoImpuestos("IVAREC:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, -25, c_TipoFactor.Tasa, 0.16, -10, true);
			AssertComprobanteConceptoImpuestos("IVAREC:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, -75, c_TipoFactor.Tasa, 0, 0, true);

			taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVAREC, 10, "RAT", "RET");
			line = CreatePostingJournal(taxRate, 100, 10);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.CRD, 2);

			AssertComprobanteConceptoImpuestos("IVAREC:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, -25, c_TipoFactor.Tasa, 0.16, -10, true);
			AssertComprobanteConceptoImpuestos("IVAREC:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, -75, c_TipoFactor.Tasa, 0, 0, true);

			taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVAREC, 10, "RAT", "RET");
			line = CreatePostingJournal(taxRate, -100, -10);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.CRD, 2);

			AssertComprobanteConceptoImpuestos("IVAREC:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, 25, c_TipoFactor.Tasa, 0.16, 10, true);
			AssertComprobanteConceptoImpuestos("IVAREC:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, 75, c_TipoFactor.Tasa, 0, 0, true);
		}

		public void TestComprobanteConceptoImpuestosWithNoExistTaxTypeCode()
		{
			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
			var taxRate = CreateTaxID("XXXX", 10, "XXX");
			var line = CreatePostingJournal(taxRate, 100, 50, -10);
			var comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertNull(comprobanteConceptoImpuestos);
		}

		public void TestComprobanteConceptoImpuestosWithDataNull_Traslados()
		{
			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;

			var line = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			var comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertNull(comprobanteConceptoImpuestos);

			var taxRate = CreateTaxID("XXXX", 10, "XX", null);
			line = CreatePostingJournal(taxRate, 100, 50, -10);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertNull(comprobanteConceptoImpuestos);

			taxRate = CreateTaxID("XXXX", 10, null, "XXX");
			line = CreatePostingJournal(taxRate, 100, 50, -10);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertNull(comprobanteConceptoImpuestos);

			taxRate = CreateTaxID(null, 21, "CAP");
			line = CreatePostingJournal(taxRate, 100, 50, -10);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertNotNull(comprobanteConceptoImpuestos.Traslados);
			AssertEquals("Count:", 1, comprobanteConceptoImpuestos.Traslados.Length);

			taxRate = CreateTaxID(null, 21, "RAT", "REF");
			line = CreatePostingJournal(taxRate, null, null, null);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertEquals("Count:", 1, comprobanteConceptoImpuestos.Traslados.Length);
			AssertEquals(0m, comprobanteConceptoImpuestos.Traslados[0].Base);
			AssertEquals(0m, comprobanteConceptoImpuestos.Traslados[0].Importe);

			taxRate = CreateTaxID("BAD", 21, "EXT", "RET");
			line = CreatePostingJournal(taxRate, 100, 50, -10);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertNull(comprobanteConceptoImpuestos);
		}

		void AssertComprobanteConceptoImpuestos(string message, ComprobanteConceptoImpuestos comprobanteConceptoImpuestos, ComprobanteConceptoImpuestosTraslado comprobanteConceptoImpuestosTraslado, int expectedCount, ZDecimal expectedBase, c_TipoFactor cTipoFactor, ZDecimal expectedTasaOcuota, ZDecimal expectedImporte, bool expectedSpecified)
		{
			AssertEquals(message + " Count:", expectedCount, comprobanteConceptoImpuestos.Traslados.Length);
			AssertEquals(message + nameof(ComprobanteConceptoImpuestosTraslado.Base), expectedBase, comprobanteConceptoImpuestosTraslado.Base);
			AssertEquals(message + nameof(ComprobanteConceptoImpuestosTraslado.Impuesto), c_Impuesto.Item002, comprobanteConceptoImpuestosTraslado.Impuesto);
			AssertEquals(message + nameof(ComprobanteConceptoImpuestosTraslado.TipoFactor), cTipoFactor, comprobanteConceptoImpuestosTraslado.TipoFactor);
			AssertEquals(message + nameof(ComprobanteConceptoImpuestosTraslado.TasaOCuota), expectedTasaOcuota, comprobanteConceptoImpuestosTraslado.TasaOCuota);
			AssertEquals(message + nameof(ComprobanteConceptoImpuestosTraslado.Importe), expectedImporte, comprobanteConceptoImpuestosTraslado.Importe);
			AssertEquals(message + nameof(ComprobanteConceptoImpuestosTraslado.ImporteSpecified), expectedSpecified, comprobanteConceptoImpuestosTraslado.ImporteSpecified);
			AssertEquals(message + nameof(ComprobanteConceptoImpuestosTraslado.TasaOCuotaSpecified), expectedSpecified, comprobanteConceptoImpuestosTraslado.TasaOCuotaSpecified);
		}

		public void TestComprobanteConceptoImpuestos_With_Null_Traslados_And_Null_Retenciones()
		{
			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
			var line = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			var comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertNull(comprobanteConceptoImpuestos);
		}

		public void TestComprobanteConceptoImpuestos_With_Value_Traslados_And_Null_Retenciones()
		{
			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
			var taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVA4, 10, "RAT");
			var line = CreatePostingJournal(taxRate, 100.28m, 10);
			var comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertForTrasladosAndRetenciones(comprobanteConceptoImpuestos, false, true, 2, null);
			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, 25.07m, c_TipoFactor.Tasa, 0.16, 10, true);
			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, 75.21m, c_TipoFactor.Tasa, 0, 0, true);
			AssertEquals(100.28m, comprobanteConceptoImpuestos.Traslados[0].Base + comprobanteConceptoImpuestos.Traslados[1].Base);
		}

		public void TestComprobanteConceptoImpuestos_With_Null_Traslados_And_Value_Retenciones()
		{
			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
			var taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVAREC, 10, "RAT", "RET");
			var line = CreatePostingJournal(taxRate, 100, 10);
			var comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.ACR, 2);

			AssertForTrasladosAndRetenciones(comprobanteConceptoImpuestos, true, false, null, 1);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Base), 25m, comprobanteConceptoImpuestos.Retenciones[0].Base);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Impuesto), c_Impuesto.Item002, comprobanteConceptoImpuestos.Retenciones[0].Impuesto);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.TipoFactor), c_TipoFactor.Tasa, comprobanteConceptoImpuestos.Retenciones[0].TipoFactor);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.TasaOCuota), MexicoConstants.PercentageTax.Perc0060, comprobanteConceptoImpuestos.Retenciones[0].TasaOCuota);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Importe), 0m, comprobanteConceptoImpuestos.Retenciones[0].Importe);
		}

		public void TestComprobanteConceptoImpuestos_With_Value_Traslados_And_Value_Retenciones()
		{
			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
			var taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVAREC, 10, "RAT", "RET");
			var line = CreatePostingJournal(taxRate, 100, 10);
			var comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertForTrasladosAndRetenciones(comprobanteConceptoImpuestos, false, false, 2, 1);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Base), 25m, comprobanteConceptoImpuestos.Retenciones[0].Base);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Impuesto), c_Impuesto.Item002, comprobanteConceptoImpuestos.Retenciones[0].Impuesto);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.TipoFactor), c_TipoFactor.Tasa, comprobanteConceptoImpuestos.Retenciones[0].TipoFactor);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.TasaOCuota), MexicoConstants.PercentageTax.Perc0060, comprobanteConceptoImpuestos.Retenciones[0].TasaOCuota);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Importe), 0m, comprobanteConceptoImpuestos.Retenciones[0].Importe);
			AssertComprobanteConceptoImpuestos("IVAREC:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, 25m, c_TipoFactor.Tasa, 0.16, 10, true);
			AssertComprobanteConceptoImpuestos("IVAREC:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, 75m, c_TipoFactor.Tasa, 0, 0, true);
		}

		public void TestGettingRetencion_WithoutCorrect_TaxID()
		{
			var taxID = CreateTaxID("", 0, "REF", "REF", 10.665m);
			var journal = CreatePostingJournal(taxID, 300m, 0, 32.50m);

			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
			var conceptoImpuesto = builder.BuildComprobanteConceptoImpuestosInfo(journal, TransactionType.INV, 2);

			AssertNull(nameof(ComprobanteConceptoImpuestos), conceptoImpuesto);
		}

		public void TestGettingRetencion_WithCorrect_TaxID_TransactionType()
		{
			var extraTaxTypes = new List<string> { "REF", "RET" };
			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;

			var taxID = CreateTaxID("", 0, "RAT", "", 10.665m);
			var journal = CreatePostingJournal(taxID, 300m, 0, -32.50m);

			journal.VATTaxID.ExtraTaxType = new CodeDescriptionPair() { Code = "REX" };
			var conceptoImpuesto = builder.BuildComprobanteConceptoImpuestosInfo(journal, TransactionType.INV, 2);
			AssertNull(nameof(ComprobanteConceptoImpuestos), conceptoImpuesto);

			foreach (var extraTaxType in extraTaxTypes)
			{
				journal.VATTaxID.ExtraTaxType = new CodeDescriptionPair() { Code = extraTaxType };
				conceptoImpuesto = builder.BuildComprobanteConceptoImpuestosInfo(journal, TransactionType.INV, 2);

				AssertEquals(nameof(ComprobanteConceptoImpuestos.Retenciones), 1, conceptoImpuesto.Retenciones.Length);
				AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Base), 300m, conceptoImpuesto.Retenciones[0].Base);
				AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Impuesto), c_Impuesto.Item002, conceptoImpuesto.Retenciones[0].Impuesto);
				AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.TipoFactor), c_TipoFactor.Tasa, conceptoImpuesto.Retenciones[0].TipoFactor);
				AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.TasaOCuota), 0.10665m, conceptoImpuesto.Retenciones[0].TasaOCuota);
				AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Importe), 32.50m, conceptoImpuesto.Retenciones[0].Importe);
			}
		}

		public void TestGettingRetencion_Signs()
		{
			var taxID = CreateTaxID("", 0, "RAT", "REF", 10.665m);
			var journal = CreatePostingJournal(taxID, 100m, 0, -32.50m);

			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
			var conceptoImpuesto = builder.BuildComprobanteConceptoImpuestosInfo(journal, TransactionType.INV, 2);

			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Base), 100m, conceptoImpuesto.Retenciones[0].Base);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.TasaOCuota), 0.10665m, conceptoImpuesto.Retenciones[0].TasaOCuota);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Importe), 32.50m, conceptoImpuesto.Retenciones[0].Importe);

			journal.OSAmount = -100m;
			journal.OSExtraVATAmount = 32.50m;
			conceptoImpuesto = builder.BuildComprobanteConceptoImpuestosInfo(journal, TransactionType.CRD, 2);

			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Base), 100m, conceptoImpuesto.Retenciones[0].Base);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.TasaOCuota), 0.10665m, conceptoImpuesto.Retenciones[0].TasaOCuota);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Importe), 32.50m, conceptoImpuesto.Retenciones[0].Importe);
		}

		public void TestGettingRetencion_TaxID_IVAREC()
		{
			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
			var line = CreatePostingJournal(null, 100, 10);

			var conceptoImpuesto = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);
			AssertNull(nameof(ComprobanteConceptoImpuestos), conceptoImpuesto);

			var taxRate = CreateTaxID("", 10, "RAT", "");
			line = CreatePostingJournal(taxRate, 100, 10);

			conceptoImpuesto = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);
			AssertNull(nameof(ComprobanteConceptoImpuestos.Retenciones), conceptoImpuesto.Retenciones);

			line.VATTaxID.ExtraTaxType = new CodeDescriptionPair() { Code = "RET" };
			line.VATTaxID.TaxCode = MexicoConstants.TaxCodes.IVAREC;
			conceptoImpuesto = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertEquals(nameof(ComprobanteConceptoImpuestos.Retenciones), 1, conceptoImpuesto.Retenciones.Length);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Base), 25m, conceptoImpuesto.Retenciones[0].Base);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Impuesto), c_Impuesto.Item002, conceptoImpuesto.Retenciones[0].Impuesto);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.TipoFactor), c_TipoFactor.Tasa, conceptoImpuesto.Retenciones[0].TipoFactor);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.TasaOCuota), MexicoConstants.PercentageTax.Perc0060, conceptoImpuesto.Retenciones[0].TasaOCuota);
			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Importe), 0m, conceptoImpuesto.Retenciones[0].Importe);
		}

		public void TestComprobanteConceptoImpuestosWithTaxCodeIVA4andIVARECCalculateItemExempt()
		{
			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
			var taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVA4, 10, "RAT");
			var line = CreatePostingJournal(taxRate, 100.28m, 10);
			var comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, 25.07m, c_TipoFactor.Tasa, 0.16, 10, true);
			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, 75.21m, c_TipoFactor.Tasa, 0, 0, true);

			AssertEquals(100.28m, comprobanteConceptoImpuestos.Traslados[0].Base + comprobanteConceptoImpuestos.Traslados[1].Base);

			taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVAREC, 10, "RAT", "RET");
			line = CreatePostingJournal(taxRate, 145.82m, 10);
			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, 36.46m, c_TipoFactor.Tasa, 0.16, 10, true);
			AssertComprobanteConceptoImpuestos("IVA4:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[0], 2, 109.36m, c_TipoFactor.Tasa, 0, 0, true);

			AssertEquals(145.82m, comprobanteConceptoImpuestos.Traslados[0].Base + comprobanteConceptoImpuestos.Traslados[1].Base);
		}

		public void TestComprobanteConceptoImpuestoTestRouding()
		{
			var builder = new CFDiConceptoImpuestosBuilder() as ICFDiConceptoImpuestosBuilder;
			var taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVAREC, 10, "RAT", "RET");
			var line = CreatePostingJournal(taxRate, 158.69m, 10);

			var comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertComprobanteConceptoImpuestos("IVA16:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, 39.67m, c_TipoFactor.Tasa, 0.16, 10, true);

			taxRate = CreateTaxID(MexicoConstants.TaxCodes.IVAREC, 10, "RAT", "RET");
			line = CreatePostingJournal(taxRate, 190.46m, 10);

			comprobanteConceptoImpuestos = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 1);

			AssertComprobanteConceptoImpuestos("IVA16:", comprobanteConceptoImpuestos, comprobanteConceptoImpuestos.Traslados[1], 2, 47.6m, c_TipoFactor.Tasa, 0.16, 10, true);

			taxRate = CreateTaxID("", 0, "RAT", "REF", 10.665m);
			line = CreatePostingJournal(taxRate, 100.226m, 0, -32.50m);

			var conceptoImpuesto = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 2);

			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Base), 100.23m, conceptoImpuesto.Retenciones[0].Base);

			taxRate = CreateTaxID("", 0, "RAT", "REF", 10.665m);
			line = CreatePostingJournal(taxRate, 100.226m, 0, -32.50m);

			conceptoImpuesto = builder.BuildComprobanteConceptoImpuestosInfo(line, TransactionType.INV, 1);

			AssertEquals(nameof(ComprobanteConceptoImpuestosRetencion.Base), 100.2m, conceptoImpuesto.Retenciones[0].Base);
		}

		void AssertForTrasladosAndRetenciones(ComprobanteConceptoImpuestos comprobanteImpuestos, bool trasladosNull, bool retencionesNull, int? expectedTrasladosLength, int? expectedRetencionesLength)
		{
			AssertNotNull(comprobanteImpuestos);

			if (trasladosNull)
			{
				AssertNull(comprobanteImpuestos.Traslados);
			}
			else
			{
				AssertNotNull(comprobanteImpuestos.Traslados);
				AssertEquals(expectedTrasladosLength, comprobanteImpuestos.Traslados.Length);
			}

			if (retencionesNull)
			{
				AssertNull(comprobanteImpuestos.Retenciones);
			}
			else
			{
				AssertNotNull(comprobanteImpuestos.Retenciones);
				AssertEquals(expectedRetencionesLength, comprobanteImpuestos.Retenciones.Length);
			}
		}

		#region Implementation		
		TaxID CreateTaxID(string taxCode, decimal rate, string taxTypeCode, string extraTaxTypeCode = "", decimal extraTaxRate = 0) => MexicoEInvoicingTestHelper.CreateTaxID(taxCode, rate, taxTypeCode, extraTaxTypeCode, extraTaxRate);
		PostingJournal CreatePostingJournal(TaxID taxRate, decimal? oSAmount, decimal? oSGSTVATAmount, decimal? oSExtraVATAmount = 0) => MexicoEInvoicingTestHelper.CreatePostingJournal(taxRate, oSAmount, oSGSTVATAmount, oSExtraVATAmount);
		#endregion
	}
}
