using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.BatchProcessor.Accounting.Testing
{
	public class ComplianceDocumentCreatorTest : TestCaseWithFactory
	{
		public void TestCreateComplianceDocumentWithPCDSettingAndEmptyComplianceDocumentNumber()
		{
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				creator.ABIGAS.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;

				var invoice1 = creator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line1 = creator.CreateInvoiceLine(invoice1, TestObjectCreator.TWD, 1M, 100M);
				line1.AL_AT = TestObjectCreator.GST1.PK;
				line1.ComplianceDocumentNumber = ZString.Empty;

				AssertNoExceptionThrown(() =>
				{
					newFactory.Save();
				});
				var documentHeaders = newFactory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals(1, documentHeaders.Length);
			}
		}

		public void TestCreateComplianceDocumentWithoutRollupWithCorrectDescription()
		{
			var originalDesc = "";
			try
			{
				originalDesc = line1WithCC1.AL_Desc;
				line1WithCC1.AL_Desc = "Description Test";
				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();

				var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				var pivots = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
				var line = Factory.Load<AccComplianceDocumentLine>(pivots.First(x => x.ADP_AL == line1WithCC1.PK).ADP_ADL);
				AssertEquals(line1WithCC1.AL_Desc, line.ADL_Description);
			}
			finally
			{
				line1WithCC1.AL_Desc = originalDesc;
			}
		}

		public void TestCreateComplianceDocumentWithRollupWithCorrectDescription()
		{
			var originalLocalDesc = "";
			var originalLocalDesc1 = "";
			var originalACDesc = "";
			var originalGLDesc = "";
			try
			{
				originalLocalDesc = testObjectCreator.CC1.AC_LocalLanguageDescription;
				originalACDesc = testObjectCreator.CC2.AC_Desc;
				originalGLDesc = testObjectCreator.CC2.AC_Desc;
				originalLocalDesc1 = testObjectCreator.CC2.AC_LocalLanguageDescription;

				testObjectCreator.CC1.AC_LocalLanguageDescription = "Local Language Charge Code Description";
				testObjectCreator.CC2.AC_LocalLanguageDescription = "";
				testObjectCreator.CC2.AC_Desc = "Charge Code Description Test";
				testObjectCreator.GLHeader1.AG_Description = "GLHeader Description Test";
				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				var pivots = Factory.Load<AccComplianceDocumentPivot>(new ZQuery());
				var line = Factory.Load<AccComplianceDocumentLine>(pivots.First(x => x.ADP_AL == line1WithCC1.PK).ADP_ADL);
				AssertEquals(testObjectCreator.CC1.AC_LocalLanguageDescription, line.ADL_Description);

				var line1 = Factory.Load<AccComplianceDocumentLine>(pivots.First(x => x.ADP_AL == line3WithCC2.PK).ADP_ADL);
				AssertEquals(testObjectCreator.CC2.AC_Desc, line1.ADL_Description);

				var line2 = Factory.Load<AccComplianceDocumentLine>(pivots.First(x => x.ADP_AL == line4WithGlHeader.PK).ADP_ADL);
				AssertEquals(testObjectCreator.GLHeader1.AG_Description, line2.ADL_Description);
			}
			finally
			{
				testObjectCreator.CC1.AC_LocalLanguageDescription = originalLocalDesc;
				testObjectCreator.CC2.AC_LocalLanguageDescription = originalLocalDesc1;
				testObjectCreator.CC2.AC_Desc = originalACDesc;
				testObjectCreator.GLHeader1.AG_Description = originalGLDesc;
			}
		}

		public void TestCreateComplianceDocumentWithRollup()
		{
			new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

			var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
			AssertEquals(2, headers.Length);
			foreach (var complianceDocumentHeader in headers)
			{
				AssertAccComplianceDocumentHeader(complianceDocumentHeader);

				var complianceDocumentLines = complianceDocumentHeader.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>();
				switch (complianceDocumentLines.Count())
				{
					case 1:
						AssertAccComplianceDocumentLine(complianceDocumentLines, TestObjectCreator.GLHeader1.AG_Description);
						AssertAccComplianceDocumentPivot(complianceDocumentLines, line4WithGlHeader);
						AssertAccComplianceDocumentPivot(complianceDocumentLines, line5WithGlHeader);
						break;
					case 2:
						AssertAccComplianceDocumentLine(complianceDocumentLines, TestObjectCreator.CC1.AC_Desc);
						AssertAccComplianceDocumentLine(complianceDocumentLines, TestObjectCreator.CC2.AC_Desc);
						AssertAccComplianceDocumentPivot(complianceDocumentLines, line1WithCC1);
						AssertAccComplianceDocumentPivot(complianceDocumentLines, line2WithCC1);
						AssertAccComplianceDocumentPivot(complianceDocumentLines, line3WithCC2);
						break;
				}
			}
		}

		public void TestCreateComplianceDocumentWithoutRollup()
		{
			new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();

			var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
			AssertEquals(2, headers.Length);
			foreach (var complianceDocumentHeader in headers)
			{
				AssertAccComplianceDocumentHeader(complianceDocumentHeader);

				var complianceDocumentLines = complianceDocumentHeader.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>();
				switch (complianceDocumentLines.Count())
				{
					case 2:
						AssertAccComplianceDocumentLine(complianceDocumentLines, line4WithGlHeader.AL_Desc);
						AssertAccComplianceDocumentLine(complianceDocumentLines, line5WithGlHeader.AL_Desc);
						AssertAccComplianceDocumentPivot(complianceDocumentLines, line4WithGlHeader);
						AssertAccComplianceDocumentPivot(complianceDocumentLines, line5WithGlHeader);
						break;
					case 3:
						AssertAccComplianceDocumentLine(complianceDocumentLines, TestObjectCreator.CC1.AC_Desc);
						AssertAccComplianceDocumentLine(complianceDocumentLines, TestObjectCreator.CC2.AC_Desc);
						AssertAccComplianceDocumentPivot(complianceDocumentLines, line1WithCC1);
						AssertAccComplianceDocumentPivot(complianceDocumentLines, line2WithCC1);
						AssertAccComplianceDocumentPivot(complianceDocumentLines, line3WithCC2);
						break;
				}
			}
		}

		void AssertAccComplianceDocumentHeader(AccComplianceDocumentHeader complianceDocumentHeader)
		{
			AssertEquals("organisation", invoice.AH_OH, complianceDocumentHeader.ADH_OH_Organisation);
			AssertEquals("ledger", invoice.AH_Ledger, complianceDocumentHeader.ADH_Ledger);
			AssertEquals("description", invoice.AH_Desc, complianceDocumentHeader.ADH_Description);
			AssertEquals("company", invoice.AH_GC, complianceDocumentHeader.ADH_GC_Company);
		}

		void AssertAccComplianceDocumentLine(IEnumerable<AccComplianceDocumentLine> complianceDocumentLines, string description)
		{
			AssertCollectionContains($"document line should exists with {description}", complianceDocumentLines, (AccComplianceDocumentLine line) => line.ADL_Description == description);
		}

		void AssertAccComplianceDocumentPivot(IEnumerable<AccComplianceDocumentLine> complianceDocumentLines, InvoicingLineBase line)
		{
			var query = new ZQuery(AccComplianceDocumentPivotSchema.ADP_AL, line.PK);
			query.AddToFilter(AccComplianceDocumentPivotSchema.ADP_ADL, complianceDocumentLines.Select(x => x.PK));
			var pivot = Factory.LoadTop1<AccComplianceDocumentPivot>(query);
			AssertNotNull($"pivot should exists with line charge {line.ChargeCode?.AC_Code ?? line.GLHeader.AG_AccountNum} and tax {line.TaxRate.AT_Code}", pivot);
		}

		#region Negative Compliances & Lines

		public void TestAllShouldBeCreated()
		{
			AssertAllShouldBeCreated(true);
			AssertAllShouldBeCreated(false);
		}

		void AssertAllShouldBeCreated(bool flag)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				var arInvLine1 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
				arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine1.AL_OSExTaxAmount = 100m;
				arInvLine1.AL_AC = TestObjectCreator.DSBChargeCode.PK;
				arInvLine1.AL_AT = TestObjectCreator.GST1.PK;
				var arInvLine2 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
				arInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine2.AL_OSExTaxAmount = 100m;
				arInvLine2.AL_AC = TestObjectCreator.DSBChargeCode1.PK;
				arInvLine2.AL_AT = TestObjectCreator.GST2.PK;
				Factory.Save();

				var result = new ComplianceDocumentCreator(new[] { arInvoice1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				AssertEquals(2, result.Length);
			}
		}

		public void TestLinePivotsIsNotNullWhenDeleteNegativeCompliances()
		{
			AssertLinePivotsIsNotNullWhenDeleteNegativeCompliances("Inv001", true);
			AssertLinePivotsIsNotNullWhenDeleteNegativeCompliances("Inv002", false);
		}

		void AssertLinePivotsIsNotNullWhenDeleteNegativeCompliances(string transactionNum, bool flag)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";

				var invoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), transactionNum, TestObjectCreator.TWD, 1M, TestObjectCreator.AALSHI);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.TWD, 1M, 100M);
				line1.AL_AT = TestObjectCreator.GST1.PK;
				line1.CreateComplianceDocumentRecordOnPosting = false;

				AssertNoExceptionThrown(() => Factory.Save());

				var documentHeaders = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				Assert(!documentHeaders.Any());
			}
		}

		public void TestAllShouldBeCreatedWithMultipleInvoies()
		{
			AssertAllShouldBeCreatedWithMultipleInvoices(true);
			AssertAllShouldBeCreatedWithMultipleInvoices(false);
		}

		void AssertAllShouldBeCreatedWithMultipleInvoices(bool flag)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				var arInvLine1 = (ARInvoiceLine)arInvoice.Lines.AddNew();
				arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine1.AL_OSExTaxAmount = 100m;
				arInvLine1.AL_AC = TestObjectCreator.DSBChargeCode.PK;
				arInvLine1.AL_AT = TestObjectCreator.GST1.PK;

				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				var arInvLine2 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
				arInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine2.AL_OSExTaxAmount = 100m;
				arInvLine2.AL_AC = TestObjectCreator.DSBChargeCode.PK;
				arInvLine2.AL_AT = TestObjectCreator.GST1.PK;
				Factory.Save();

				var result = new ComplianceDocumentCreator(new[] { arInvoice, arInvoice1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				AssertEquals(1, result.Length);
				AssertEquals(arInvoice.PK, result.First().TransactionHeaders.First().PK);
			}
		}

		public void TestNoneShouldBeCreated()
		{
			AssertNoneShouldBeCreated(true);
			AssertNoneShouldBeCreated(false);
		}

		void AssertNoneShouldBeCreated(bool flag)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				var arInvLine1 = (ARInvoiceLine)arInvoice.Lines.AddNew();
				arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine1.AL_OSExTaxAmount = -100m;
				arInvLine1.AL_AC = TestObjectCreator.DSBChargeCode.PK;
				arInvLine1.AL_AT = TestObjectCreator.GST1.PK;
				var arInvLine2 = (ARInvoiceLine)arInvoice.Lines.AddNew();
				arInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine2.AL_OSExTaxAmount = -100m;
				arInvLine2.AL_AC = TestObjectCreator.DSBChargeCode1.PK;
				arInvLine2.AL_AT = TestObjectCreator.GST2.PK;
				Factory.Save();

				var result = new ComplianceDocumentCreator(new[] { arInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				AssertEquals(0, result.Length);
			}
		}

		public void TestNoneShouldBeCreatedWhenNegativeComplianceLinesNested()
		{
			AssertNoneShouldBeCreatedWhenNegativeComplianceLinesNested(true);
			AssertNoneShouldBeCreatedWhenNegativeComplianceLinesNested(false);
		}

		void AssertNoneShouldBeCreatedWhenNegativeComplianceLinesNested(bool flag)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				var arInvLine1 = (ARInvoiceLine)arInvoice.Lines.AddNew();
				arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine1.AL_OSExTaxAmount = 100m;
				arInvLine1.AL_AC = TestObjectCreator.DSBChargeCode.PK;
				arInvLine1.AL_AT = TestObjectCreator.GST1.PK;
				var arInvLine2 = (ARInvoiceLine)arInvoice.Lines.AddNew();
				arInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine2.AL_OSExTaxAmount = -100m;
				arInvLine2.AL_AC = TestObjectCreator.DSBChargeCode1.PK;
				arInvLine2.AL_AT = TestObjectCreator.GST2.PK;

				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				var arInvLine3 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
				arInvLine3.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine3.AL_OSExTaxAmount = 100m;
				arInvLine3.AL_AC = TestObjectCreator.DSBChargeCode.PK;
				arInvLine3.AL_AT = TestObjectCreator.GST1.PK;
				var arInvLine4 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
				arInvLine4.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine4.AL_OSExTaxAmount = 100m;
				arInvLine4.AL_AC = TestObjectCreator.DSBChargeCode.PK;
				arInvLine4.AL_AT = TestObjectCreator.GST2.PK;
				Factory.Save();

				var result = new ComplianceDocumentCreator(new[] { arInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				AssertEquals(0, result.Length);
			}
		}

		public void TestPartShoudBeCreatedWhenHasDifferentDebtors()
		{
			AssertPartShoudBeCreatedWhenHasDifferentDebtors(true);
			AssertPartShoudBeCreatedWhenHasDifferentDebtors(false);
		}

		void AssertPartShoudBeCreatedWhenHasDifferentDebtors(bool flag)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				var arInvLine1 = (ARInvoiceLine)arInvoice.Lines.AddNew();
				arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine1.AL_OSExTaxAmount = 100m;
				arInvLine1.AL_AC = TestObjectCreator.DSBChargeCode.PK;
				arInvLine1.AL_AT = TestObjectCreator.GST1.PK;

				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor1);
				var arInvLine2 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
				arInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine2.AL_OSExTaxAmount = -100m;
				arInvLine2.AL_AC = TestObjectCreator.DSBChargeCode.PK;
				arInvLine2.AL_AT = TestObjectCreator.GST1.PK;
				Factory.Save();

				var result = new ComplianceDocumentCreator(new[] { arInvoice, arInvoice1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				AssertEquals(1, result.Length);
				AssertEquals(arInvoice.PK, result.First().TransactionHeaders.First().PK);
			}
		}

		public void TestPartShouldBeCreatedWhenHasDifferentTransactionTypes()
		{
			AssertPartShouldBeCreatedWhenHasDifferentTransactionTypes(true);
			AssertPartShouldBeCreatedWhenHasDifferentTransactionTypes(false);
		}

		void AssertPartShouldBeCreatedWhenHasDifferentTransactionTypes(bool flag)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				var arInvLine1 = (ARInvoiceLine)arInvoice.Lines.AddNew();
				arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine1.AL_OSExTaxAmount = -100m;
				arInvLine1.AL_AC = TestObjectCreator.DSBChargeCode.PK;
				arInvLine1.AL_AT = TestObjectCreator.GST1.PK;

				var arCreditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.Debtor, TestObjectCreator.TWD, 1m, "");
				var arCRDLine1 = (ARCreditNoteLine)arCreditNote.Lines.AddNew();
				arCRDLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
				arCRDLine1.AL_OSExTaxAmount = 100m;
				arCRDLine1.AL_AC = TestObjectCreator.DSBChargeCode.PK;
				arCRDLine1.AL_AT = TestObjectCreator.GST1.PK;
				Factory.Save();

				var result = new ComplianceDocumentCreator(new[] { arInvoice, arCreditNote as InvoicingBase }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				AssertEquals(1, result.Length);
				AssertEquals(arCreditNote.PK, result.First().TransactionHeaders.First().PK);
			}
		}

		#endregion

		public void TestComplianceSubType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line1.AL_AT = TestObjectCreator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				var documentHeaders = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals(1, documentHeaders.Length);
				AssertEquals("TDP", documentHeaders[0].ADH_ComplianceSubType);
			}
		}

		public void TestComplianceSequenceAndDocumentNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var sequenceAAA = CreateComplianceSequence("AAA", "TDP", ComplianceBookAllocationLevel.Counter, true);
				sequenceAAA.XD_Prefix = "TDP";
				var sequenceBBB = CreateComplianceSequence("BBB", "TCD", ComplianceBookAllocationLevel.BranchDepartment);
				sequenceBBB.XD_Prefix = "TCD";
				var sequenceCCC = CreateComplianceSequence("CCC", "TXC", ComplianceBookAllocationLevel.Branch);
				sequenceCCC.XD_Prefix = "TXC";
				var sequenceDDD = CreateComplianceSequence("DDD", "TCR", ComplianceBookAllocationLevel.Company);
				sequenceDDD.XD_Prefix = "TCR";

				var org1 = TestObjectCreator.CreateOrgHeader("Org1", true, true);
				var org2 = TestObjectCreator.CreateOrgHeader("Org2", true, true);
				var org3 = TestObjectCreator.CreateOrgHeader("Org3", true, true);
				var org4 = TestObjectCreator.CreateOrgHeader("Org4", true, true);
				org3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345675", CountryCodes.Taiwan);
				org4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345675", CountryCodes.Taiwan);

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, org1);
				var line1 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.RevenueChargeCode.PK);

				var arCreditNote = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "INV002", TestObjectCreator.TWD, 1M, org2);
				var line2 = TestObjectCreator.CreateInvoiceLine(arCreditNote, TestObjectCreator.TWD, 1M, 200M, 20M, 0M, TestObjectCreator.RevenueChargeCode.PK);

				var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV003", TestObjectCreator.TWD, 1M, org3);
				var line3 = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.TWD, 1M, 300M, 30M, 0M, TestObjectCreator.OverheadChargeCode.PK);

				var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "INV004", TestObjectCreator.TWD, 1M, org4);
				var line4 = TestObjectCreator.CreateInvoiceLine(apCreditNote, TestObjectCreator.TWD, 1M, 400M, 40M, 0M, TestObjectCreator.OverheadChargeCode.PK);

				line1.AL_AT = TestObjectCreator.GST1.PK;
				line2.AL_AT = TestObjectCreator.GST1.PK;
				line3.AL_AT = TestObjectCreator.GST1.PK;
				line4.AL_AT = TestObjectCreator.GST1.PK;

				Factory.Save();

				var complianceDocumentHeaders = new ComplianceDocumentCreator(new[] { arInvoice, arCreditNote, apInvoice, apCreditNote }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				complianceDocumentHeaders.ForEach(header => header.ComplianceDocumentLines[0].ADL_Description = "desc");

				Factory.Save();

				var documentHeaders = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals(4, documentHeaders.Length);

				var documentHeader = documentHeaders.FirstOrDefault(x => x.ADH_OH_Organisation == org1.PK);
				AssertNotNull("compliance document header with org1 exists", documentHeader);
				AssertEquals("compliance document header with org1 has TDP sub type", "TDP", documentHeader.ADH_ComplianceSubType);
				AssertEquals("compliance document header with org1 has AAA sequence book", sequenceAAA.PK, documentHeader.ADH_XD_ComplianceBook);
				AssertEquals("compliance document header with org1 has document number", "TDP000000001", documentHeader.ADH_DocumentNumber);

				documentHeader = documentHeaders.FirstOrDefault(x => x.ADH_OH_Organisation == org2.PK);
				AssertNotNull("compliance document header with org2 exists", documentHeader);
				AssertEquals("compliance document header with org2 has TCD sub type", "TCD", documentHeader.ADH_ComplianceSubType);
				AssertEquals("compliance document header with org2 has BBB sequence book", sequenceBBB.PK, documentHeader.ADH_XD_ComplianceBook);
				AssertEquals("compliance document header with org2 has document number", "TCD000000001", documentHeader.ADH_DocumentNumber);

				documentHeader = documentHeaders.FirstOrDefault(x => x.ADH_OH_Organisation == org3.PK);
				AssertNotNull("compliance document header with org3 exists", documentHeader);
				AssertEquals("compliance document header with org3 has TXC sub type", "TXC", documentHeader.ADH_ComplianceSubType);
				AssertEquals("compliance document header with org3 has no sequence book because ledger is AP", ZGuid.Empty, documentHeader.ADH_XD_ComplianceBook);
				AssertEquals("compliance document header with org3 has no document number because ledger is AP", ZString.Empty, documentHeader.ADH_DocumentNumber);

				documentHeader = documentHeaders.FirstOrDefault(x => x.ADH_OH_Organisation == org4.PK);
				AssertNotNull("compliance document header with org4 exists", documentHeader);
				AssertEquals("compliance document header with org4 has TCR sub type", "TCR", documentHeader.ADH_ComplianceSubType);
				AssertEquals("compliance document header with org4 has no sequence book because ledger is AP", ZGuid.Empty, documentHeader.ADH_XD_ComplianceBook);
				AssertEquals("compliance document header with org4 has no document number because ledger is AP", ZString.Empty, documentHeader.ADH_DocumentNumber);
			}
		}

		public void TestComplianceAddress_UsePremisesAddressIfAvailable()
		{
			var orgHeader = TestObjectCreator.CreateOrgHeader("Org", true, true);
			var premisesAddress = TestObjectCreator.CreateAddress(orgHeader, "PremisesAddress");
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = "VAT";
			orgCusCode.OK_CustomsRegNo = "TEST";
			orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orgCusCode.OK_OA_PremisesAddress = premisesAddress.PK;
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, orgHeader);
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			line.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();

			AssertComplianceAddress(new InvoicingBase[] { arInvoice }, orgHeader.PK, premisesAddress.PK);
		}

		public void TestComplianceAddress_UseInvoiceAddressWhenAllInvoicesHaveSameAddress()
		{
			var orgHeader = TestObjectCreator.CreateOrgHeader("Org", true, true);
			var address1 = TestObjectCreator.CreateAddress(orgHeader, "Address1");
			var address2 = TestObjectCreator.CreateAddress(orgHeader, "Address2");
			var arInvoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, orgHeader);
			var arInvoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV002", TestObjectCreator.AUD, 1M, orgHeader);
			arInvoice1.AH_OA_InvoiceAddressOverride = address1.PK;
			arInvoice2.AH_OA_InvoiceAddressOverride = address1.PK;
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			line.AL_AT = TestObjectCreator.GST1.PK;
			line = TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC2.PK);
			line.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();

			AssertComplianceAddress(new InvoicingBase[] { arInvoice1, arInvoice2 }, orgHeader.PK, address1.PK);
		}

		public void TestComplianceAddress_UseInvoiceDefaultAddressWhenInvoicesHaveDifferentAddress()
		{
			var orgHeader = TestObjectCreator.CreateOrgHeader("Org1", true, true);
			var address1 = TestObjectCreator.CreateAddress(orgHeader, "Address1");
			var address2 = TestObjectCreator.CreateAddress(orgHeader, "Address2");
			var armAddress = TestObjectCreator.CreateAddress(orgHeader, "ARM main office");
			armAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			var postalAddress = testObjectCreator.CreateAddress(orgHeader, "Postal address");
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Postal);

			var arInvoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, orgHeader);
			var arInvoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV002", TestObjectCreator.AUD, 1M, orgHeader);
			arInvoice1.AH_OA_InvoiceAddressOverride = address1.PK;
			arInvoice2.AH_OA_InvoiceAddressOverride = address2.PK;
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			line.AL_AT = TestObjectCreator.GST1.PK;
			line = TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.AUD, 1M, 200M, 20M, 0M, TestObjectCreator.CC2.PK);
			line.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();
			AssertComplianceAddress(new InvoicingBase[] { arInvoice1, arInvoice2 }, orgHeader.PK, armAddress.PK, true);

			orgHeader.Addresses.Remove(armAddress);
			AssertComplianceAddress(new InvoicingBase[] { arInvoice1, arInvoice2 }, orgHeader.PK, postalAddress.PK, true);

			orgHeader.Addresses.Remove(postalAddress);
			AssertComplianceAddress(new InvoicingBase[] { arInvoice1, arInvoice2 }, orgHeader.PK, orgHeader.MainAddress.PK);
		}

		public void TestComplianceAddress_UseDefaultAddressWhenAllHaveEmptyAH_OA_InvoiceAddressOverride()
		{
			var orgHeader = TestObjectCreator.CreateOrgHeader("Org1", true, true);
			var arInvoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, orgHeader);
			var arInvoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV002", TestObjectCreator.AUD, 1M, orgHeader);
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			line.AL_AT = TestObjectCreator.GST1.PK;
			line = TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC2.PK);
			line.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();

			AssertEquals(ZGuid.Empty, arInvoice1.AH_OA_InvoiceAddressOverride);
			AssertEquals(ZGuid.Empty, arInvoice2.AH_OA_InvoiceAddressOverride);
			AssertComplianceAddress(new InvoicingBase[] { arInvoice1, arInvoice2 }, orgHeader.PK, orgHeader.MainAddress.PK);
		}

		public void TestCreateCRDComplianceDocumentRecords_AllocationCodeIsPRN()
		{
			AssertCreateCRDComplianceDocumentRecords(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print);
		}

		public void TestCreateCRDComplianceDocumentRecords_AllocationCodeIsPST()
		{
			AssertCreateCRDComplianceDocumentRecords(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
		}

		void AssertCreateCRDComplianceDocumentRecords(string allocationCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var sequenceAAA = CreateComplianceSequence("AAA", "TDP", ComplianceBookAllocationLevel.Counter, true);
				var sequenceBBB = CreateComplianceSequence("BBB", "TCD", ComplianceBookAllocationLevel.BranchDepartment);

				var org1 = TestObjectCreator.CreateOrgHeader("Org1", true, true);
				var org2 = TestObjectCreator.CreateOrgHeader("Org2", true, true);
				Factory.Save();

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, org1);
				var line1 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line1.AL_AT = TestObjectCreator.GST1.PK;

				new ComplianceDocumentCreator(new[] { arInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();

				var invDocumentheader = Factory.LoadTop1<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals("compliance document header with org1 has document number", "000000001", invDocumentheader.ADH_DocumentNumber);

				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allocationCode);

				var arCreditNote = Factory.New<ARCreditNote>();
				arCreditNote.AH_OH = arInvoice.AH_OH;
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				var arCreditNoteLine = TestObjectCreator.CreateARCreditNoteLine(arCreditNote, null, TestObjectCreator.FRT, 2000.00m, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Test");
				arCreditNoteLine.CopiedFromPK = line1.PK;

				Factory.Save();

				var crdDocumentheader = Factory.LoadTop1<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.PK, SQLComparisonOperator.NotEqual, invDocumentheader.PK));
				AssertEquals(invDocumentheader.ADH_DocumentNumber, crdDocumentheader.ADH_DocumentNumber);
			}
		}

		void AssertComplianceAddress(InvoicingBase[] invoices, ZGuid orgHeaderPK, ZGuid expectAddressPK, bool shouldDeleteHeader = false)
		{
			new ComplianceDocumentCreator(invoices, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();
			var headers = Factory.Load<ARComplianceDocumentHeader>(new ZQuery()).Where(x => x.ADH_OH_Organisation == orgHeaderPK);
			foreach (var header in headers)
			{
				AssertEquals(expectAddressPK, header.ADH_OA_AddressOverride);

				if (shouldDeleteHeader)
				{
					header.Delete();
				}
			}
		}

		public AccComplianceSequence CreateComplianceSequence(string code, string sequenceClass, string allocationLevel, bool lockBy = false, BusinessObjectFactory factory = null)
		{
			factory = factory ?? Factory;
			var sequence = factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = sequenceClass;
			sequence.XD_Code = code;
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 1;
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_AllocationLevel = allocationLevel;
			if (sequence.AllocationStrategy.IsBranchApplicable)
			{
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			}
			if (sequence.AllocationStrategy.IsDepartmentApplicable)
			{
				sequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			}
			if (lockBy)
			{
				sequence.XD_LockBy = GlbStaff.CurrentUser.PK;
			}
			return sequence;
		}

		protected override void SetUp()
		{
			base.SetUp();

			invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			line1WithCC1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			line2WithCC1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M, 20M, 0M, TestObjectCreator.CC1.PK);
			line3WithCC2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 300M, 30M, 0M, TestObjectCreator.CC2.PK);
			line4WithGlHeader = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 400M, 40M, TestObjectCreator.GLHeader1.PK);
			line5WithGlHeader = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 500M, 50M, TestObjectCreator.GLHeader1.PK);
			line1WithCC1.AL_AT = TestObjectCreator.GST1.PK;
			line2WithCC1.AL_AT = TestObjectCreator.GST1.PK;
			line3WithCC2.AL_AT = TestObjectCreator.GST1.PK;
			line4WithGlHeader.AL_AT = TestObjectCreator.GST2.PK;
			line5WithGlHeader.AL_AT = TestObjectCreator.GST2.PK;
			TestObjectCreator.GST1.AT_PostingGroupId = 1;
			TestObjectCreator.GST2.AT_PostingGroupId = 2;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		InvoicingBase invoice;
		InvoicingLineBase line1WithCC1, line2WithCC1, line3WithCC2, line4WithGlHeader, line5WithGlHeader;
	}

	public class ComplianceDocumentGroupTest : TestCaseWithFactory
	{
		public void TestComplianceDocumentHeaderDetail()
		{
			var headerDetail = new ComplianceDocumentHeaderDetail(apLine1);

			AssertEquals("SubType", apLine1.ComplianceSubType, headerDetail.ComplianceSubType);
			AssertEquals("DocumentNumber", apLine1.ComplianceDocumentNumber, headerDetail.ComplianceDocumentNumber);
			AssertEquals("VATRegistrationNum", apLine1.ComplianceDocumentVATRegistrationNum, headerDetail.ComplianceDocumentVATRegistrationNum);
			AssertEquals("DocumentDate", apLine1.ComplianceDocumentDate, headerDetail.ComplianceDocumentDate);
			AssertEquals("ReportingPeriod", apLine1.ComplianceDocumentReportingPeriod, headerDetail.ComplianceDocumentReportingPeriod);
			AssertEquals("SupportingReason", apLine1.ComplianceDocumentSupportingReason, headerDetail.ComplianceDocumentSupportingReason);
			AssertEquals("DocumentType", apLine1.ComplianceSupportingDocumentType, headerDetail.ComplianceSupportingDocumentType);
			AssertEquals("DocumentNumber", apLine1.ComplianceSupportingDocumentNumber, headerDetail.ComplianceSupportingDocumentNumber);
			AssertEquals("DocumentOrganization", apInvoice.AH_OH, headerDetail.ComplianceDocumentOrganization);

			apLine1.ComplianceDocumentOrganization = TestObjectCreator.ZECTRA.PK;
			headerDetail = new ComplianceDocumentHeaderDetail(apLine1);
			AssertEquals("DocumentOrganization", apLine1.ComplianceDocumentOrganization, headerDetail.ComplianceDocumentOrganization);
		}

		public void TestGroupKeyForAPInvoiceLineWithPCDSetting()
		{
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var complianceDocumentGroup1 = new ComplianceDocumentGroup(apLine1);
			var complianceDocumentGroup2 = new ComplianceDocumentGroup(apLine2);
			var complianceDocumentGroup3 = new ComplianceDocumentGroup(apLine3);

			AssertEquals("line1 and line2 have same group key", complianceDocumentGroup1.GroupKey, complianceDocumentGroup2.GroupKey);
			AssertNotEquals("line1 and line2 have different group key", complianceDocumentGroup1.GroupKey, complianceDocumentGroup3.GroupKey);
		}

		public void TestGroupKey()
		{
			var complianceDocumentGroup1 = new ComplianceDocumentGroup(line1);
			var complianceDocumentGroup2 = new ComplianceDocumentGroup(line2);
			var complianceDocumentGroup3 = new ComplianceDocumentGroup(line3);

			AssertEquals("line1 and line2 have same group key", complianceDocumentGroup1.GroupKey, complianceDocumentGroup2.GroupKey);
			AssertNotEquals("line1 and line3 have different group key", complianceDocumentGroup1.GroupKey, complianceDocumentGroup3.GroupKey);
		}

		public void TestProperties()
		{
			var complianceDocumentGroup = new ComplianceDocumentGroup(line1);

			AssertEquals("Ledger", invoice.AH_Ledger, complianceDocumentGroup.Ledger);
			AssertEquals("HeaderDescription", invoice.AH_Desc, complianceDocumentGroup.HeaderDescription);
			AssertEquals("CompanyPK", invoice.AH_GC, complianceDocumentGroup.CompanyPK);
			AssertEquals("LinePK", line1.PK, complianceDocumentGroup.LinePK);
			AssertEquals("ChargeCodePK", TestObjectCreator.CC1.PK, complianceDocumentGroup.ChargeCodePK);
			AssertEquals("ChargeDescription", TestObjectCreator.CC1.AC_Desc, complianceDocumentGroup.ChargeDescription);
			AssertEquals("GlAccountPK", line1.AL_AG, complianceDocumentGroup.GlAccountPK);
			AssertEquals("GlAccountDescription", line1.GLHeader.AG_Description, complianceDocumentGroup.GlAccountDescription);

			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			apLine1.CreateComplianceDocumentRecordOnPosting = true;
			complianceDocumentGroup = new ComplianceDocumentGroup(apLine1);

			AssertEquals("Ledger", apInvoice.AH_Ledger, complianceDocumentGroup.Ledger);
			AssertEquals("HeaderDescription", apInvoice.AH_Desc, complianceDocumentGroup.HeaderDescription);
			AssertEquals("CompanyPK", apInvoice.AH_GC, complianceDocumentGroup.CompanyPK);
			AssertEquals("LinePK", apLine1.PK, complianceDocumentGroup.LinePK);
			AssertEquals("ChargeCodePK", TestObjectCreator.CC1.PK, complianceDocumentGroup.ChargeCodePK);
			AssertEquals("ChargeDescription", TestObjectCreator.CC1.AC_Desc, complianceDocumentGroup.ChargeDescription);
			AssertEquals("GlAccountPK", apLine1.AL_AG, complianceDocumentGroup.GlAccountPK);
			AssertEquals("GlAccountDescription", apLine1.GLHeader.AG_Description, complianceDocumentGroup.GlAccountDescription);

			var headerDetail = complianceDocumentGroup.ComplianceDocumentHeaderDetail;
			var expectHeaderDetail = new ComplianceDocumentHeaderDetail(apLine1);

			AssertEquals(expectHeaderDetail.ComplianceDocumentDate, headerDetail.ComplianceDocumentDate);
			AssertEquals(expectHeaderDetail.ComplianceDocumentNumber, headerDetail.ComplianceDocumentNumber);
			AssertEquals(expectHeaderDetail.ComplianceDocumentOrganization, headerDetail.ComplianceDocumentOrganization);
			AssertEquals(expectHeaderDetail.ComplianceDocumentReportingPeriod, headerDetail.ComplianceDocumentReportingPeriod);
			AssertEquals(expectHeaderDetail.ComplianceDocumentSupportingReason, headerDetail.ComplianceDocumentSupportingReason);
			AssertEquals(expectHeaderDetail.ComplianceDocumentVATRegistrationNum, headerDetail.ComplianceDocumentVATRegistrationNum);
			AssertEquals(expectHeaderDetail.ComplianceSubType, headerDetail.ComplianceSubType);
			AssertEquals(expectHeaderDetail.ComplianceSupportingDocumentNumber, headerDetail.ComplianceSupportingDocumentNumber);
			AssertEquals(expectHeaderDetail.ComplianceSupportingDocumentType, headerDetail.ComplianceSupportingDocumentType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.GST1.AT_PostingGroupId = 1;
			TestObjectCreator.GST2.AT_PostingGroupId = 2;

			invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			line1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M, 20M, 0M, TestObjectCreator.CC2.PK);
			line3 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 300M, 30M, 0M, TestObjectCreator.CC3.PK);
			line1.AL_AT = TestObjectCreator.GST1.PK;
			line2.AL_AT = TestObjectCreator.GST1.PK;
			line3.AL_AT = TestObjectCreator.GST2.PK;

			apInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);

			apLine1 = (APInvoiceLine)TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
			apLine2 = (APInvoiceLine)TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1M, 200M, 20M, 0M, TestObjectCreator.CC2.PK);
			apLine3 = (APInvoiceLine)TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1M, 300M, 30M, 0M, TestObjectCreator.CC3.PK);
			apLine1.AL_AT = TestObjectCreator.GST1.PK;
			apLine2.AL_AT = TestObjectCreator.GST1.PK;
			apLine3.AL_AT = TestObjectCreator.GST2.PK;
			apLine1.ComplianceDocumentNumber = "111";
			apLine2.ComplianceDocumentNumber = "111";
			apLine3.ComplianceDocumentNumber = "333";
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		InvoicingBase invoice;
		InvoicingLineBase line1, line2, line3;
		APInvoice apInvoice;
		APInvoiceLine apLine1, apLine2, apLine3;
	}
}
