using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class CusEntryLineTest<T, TJobComInvoiceLine> : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
		where T : CusEntryLine
		where TJobComInvoiceLine : JobComInvoiceLine
	{
		public void TestAmountAndTypeToBeGuaranteeds()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "Ye";
			procedure.ZZ6_PreviousProcedureCode = "12";
			procedure.ZZ6_ShipmentType = "EXP";
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = currentCountry;
			procedure.ZZ6_Concession = "367";
			procedure.ZZ6_Description = "description";

			var dty = helper.CreateNewOrGetExistingRateType(currentCountry, Constants.RateTypes.Duty, "DTY");
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dty.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, dty.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, dty.PK);
			Factory.Save();

			var declaration = GetJobDeclarationForTestWithValidTestData();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "Ye12367";
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals(true, entryHeader.HasConsumingGuaranteeProcedure);
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var fee1 = entryLine1.Fees.AddNew();
			fee1.CF_ChargeAmount = 10;
			fee1.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.Vat;
			var fee2 = entryLine1.Fees.AddNew();
			fee2.CF_ChargeAmount = 20;
			fee2.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			var fee3 = entryLine1.Fees.AddNew();
			fee3.CF_ChargeAmount = 30;
			fee3.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
			var fee4 = entryLine1.Fees.AddNew();
			fee4.CF_ChargeAmount = 40;
			fee4.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty;
			var fee5 = entryLine1.Fees.AddNew();
			fee5.CF_ChargeAmount = 50;
			fee5.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ClimateChangeLevy;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var fee6 = entryLine2.Fees.AddNew();
			fee6.CF_ChargeAmount = 50;
			fee6.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ClimateChangeLevy;

			AssertEquals(1, entryLine1.AmountAndTypeToBeGuaranteeds.Count());
			AssertEquals(GuaranteeDebitType.NORMAL, entryLine1.AmountAndTypeToBeGuaranteeds.ToArray()[0].DebitType);
			AssertEquals(100m, entryLine1.AmountAndTypeToBeGuaranteeds.ToArray()[0].AmountInDeclarationCurrency);

			AssertEquals(1, entryLine2.AmountAndTypeToBeGuaranteeds.Count());
			AssertEquals(GuaranteeDebitType.NORMAL, entryLine2.AmountAndTypeToBeGuaranteeds.ToArray()[0].DebitType);
			AssertEquals(0m, entryLine2.AmountAndTypeToBeGuaranteeds.ToArray()[0].AmountInDeclarationCurrency);
		}

		public void TestCountryOfSupply()
		{
			var entryLine = Factory.New<T>();
			AssertEquals("", entryLine.CountryOfSupply);

			var invoiceLine = entryLine.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.ZG_CountryOfSupply = "FR";
			AssertEquals("FR", entryLine.CountryOfSupply);
		}

		public void TestCountryOfExport()
		{
			var mockEntryLineConfiguration = new Mock<EntryLineConfiguration>();
			var entryLineConfiguration = mockEntryLineConfiguration.Object;
			mockEntryLineConfiguration.Protected().Setup<ZBool>("MergeJI_RN_NKCountryOfExportCore", ItExpr.IsAny<JobDeclaration>()).Returns(ZBool.True);

			var dec = GetJobDeclarationForTestWithValidTestData();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(dec, "GetNewEntryLineConfiguration", entryLineConfiguration))
			{
				dec.JE_MessageType = MessageTypeList.Codes.Export;
				var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
				DoMerge(dec);
				var entryLine = dec.CustomsEntryHeaders[0].MergedLines[0];
				AssertEquals(ZString.Empty, entryLine.CountryOfExport);

				invLine.JI_RN_NKCountryOfExport = "IE";
				AssertEquals("IE", entryLine.CountryOfExport);
			}
		}

		public void TestCountryOfDestination()
		{
			var entryLine = Factory.New<T>();
			AssertEquals("", entryLine.CountryOfDestination);

			var invoiceLine = entryLine.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.ZG_CountryOfDestination = "FR";
			AssertEquals("FR", entryLine.CountryOfDestination);
		}

		public virtual void TestAdditionalInfos()
		{
			CusEntryHeaderTestHelper.CreateAdditionalInfos(Factory);

			var dec = GetJobDeclarationForTestWithValidTestData();
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var inv = dec.Invoices.AddNew();
			var invLine1 = inv.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var invLine2 = inv.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";

			var addInfo1 = invLine1.AdditionalInfos.AddNew();
			addInfo1.CSI_Code = "9001";
			addInfo1.CSI_Description = "9001 Desc";

			var addInfo2 = invLine2.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = "9001";
			addInfo2.CSI_Description = "9001 Desc";

			var addInfo3 = inv.AdditionalInfos.AddNew();
			addInfo3.CSI_Code = "9003";
			addInfo3.CSI_Description = "9003 Desc";

			DoMerge(dec);

			AssertEquals(1, dec.ActiveEntryHeaders.Count);
			AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);
			AssertEquals(2, ((T)dec.ActiveEntryHeaders[0].MergedLines[0]).AdditionalInfos.Count());

			addInfo2.CSI_Code = "12345";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);
		}

		public void TestTaxes()
		{
			var dec = GetJobDeclarationForTestWithValidTestData();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice1 = dec.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			var tax1 = line1.Taxes.AddNew();
			tax1.JLT_Type = "A00";
			tax1.JLT_Amount = 10m;
			tax1.JLT_BaseValue = 100m;
			var line2 = invoice1.JobComInvoiceLines.AddNew();
			var tax2 = line2.Taxes.AddNew();
			tax2.JLT_Type = "A00";
			tax2.JLT_Amount = 15m;
			tax2.JLT_BaseValue = 150m;
			DoMerge(dec);
			var entryLine = dec.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Pre Req - merged to one entry line", 1, dec.CustomsEntryHeaders[0].MergedLines.Count);
			var taxes = entryLine.Taxes;
			AssertEquals("Correctly sums base amount for box 47b without inflating first line", "250.00", taxes[0].Box47b);
		}

		public virtual void TestMultiInvoiceMergeShowsInvoicesOnBothLines()
		{
			var dec = GetJobDeclarationForTestWithValidTestData();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice1 = dec.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JobComInvoiceLines.AddNew().JI_Tariff = "22030010";
			invoice1.JobComInvoiceLines.AddNew().JI_Tariff = "22030020";
			var invoice2 = dec.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.JobComInvoiceLines.AddNew().JI_Tariff = "22030010";
			invoice2.JobComInvoiceLines.AddNew().JI_Tariff = "22030020";

			DoMerge(dec);

			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.CustomsEntryHeaders[0].MergedLines.Count);
			var docs1 = new List<PreviousDocument>(dec.CustomsEntryHeaders[0].MergedLines[0].PreviousDocuments);
			AssertEquals(0, docs1.Count);
			var docs2 = new List<PreviousDocument>(dec.CustomsEntryHeaders[0].MergedLines[1].PreviousDocuments);
			AssertEquals(0, docs2.Count);
		}

		public void TestMultiInvoiceMergeWhenHastMultiEntryInstructions()
		{
			var dec = GetJobDeclarationForTestWithValidTestData();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice1 = dec.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JobComInvoiceLines.AddNew().JI_Tariff = "22030010";
			invoice1.JobComInvoiceLines.AddNew().JI_Tariff = "22030020";
			var invoice2 = dec.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.JobComInvoiceLines.AddNew().JI_Tariff = "22030010";
			invoice2.JobComInvoiceLines.AddNew().JI_Tariff = "22030020";

			DoMerge(dec);
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.CustomsEntryHeaders[0].MergedLines.Count);

			var entryInstruction1 = dec.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = dec.CustomsEntryInstructions.AddNew();

			invoice1.JobComInvoiceLines[0].JI_CEI = entryInstruction1.PK;
			invoice1.JobComInvoiceLines[1].JI_CEI = entryInstruction2.PK;
			invoice2.JobComInvoiceLines[0].JI_CEI = entryInstruction1.PK;
			invoice2.JobComInvoiceLines[1].JI_CEI = entryInstruction2.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals(1, dec.CustomsEntryHeaders[1].MergedLines.Count);

			invoice1.JobComInvoiceLines[0].JI_CEI = entryInstruction1.PK;
			invoice1.JobComInvoiceLines[1].JI_CEI = entryInstruction1.PK;
			invoice2.JobComInvoiceLines[0].JI_CEI = entryInstruction1.PK;
			invoice2.JobComInvoiceLines[1].JI_CEI = entryInstruction2.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, dec.CustomsEntryHeaders.Count);
			Assert(dec.CustomsEntryHeaders.Any(x => x.MergedLines.Count == 2));
			Assert(dec.CustomsEntryHeaders.Any(x => x.MergedLines.Count == 1));

			invoice1.JobComInvoiceLines[0].JI_CEI = entryInstruction1.PK;
			invoice1.JobComInvoiceLines[1].JI_CEI = entryInstruction1.PK;
			invoice2.JobComInvoiceLines[0].JI_CEI = entryInstruction2.PK;
			invoice2.JobComInvoiceLines[1].JI_CEI = entryInstruction2.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals(2, dec.CustomsEntryHeaders[1].MergedLines.Count);

			invoice1.JobComInvoiceLines[0].JI_CEI = entryInstruction1.PK;
			invoice1.JobComInvoiceLines[1].JI_CEI = entryInstruction1.PK;
			invoice2.JobComInvoiceLines[0].JI_CEI = entryInstruction1.PK;
			invoice2.JobComInvoiceLines[1].JI_CEI = entryInstruction1.PK;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestBox47Contents()
		{
			var declaration = GetJobDeclarationForTestWithValidTestData();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234567890";
			var tax1 = invoiceLine1.Taxes.AddNew().Data;
			tax1.G4_Type = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			tax1.G4_BaseAmount = 2000m;
			tax1.G4_RateDuty = TaxRateCustomsDutyListImport.Codes.DutyTheGoodsAreLiableToDutyAtTheFullRateThisIncludesGoodsBeingEnteredForATariffQuotaReliefToWhichNoneOfTheCodesBelowApply;
			tax1.G4_RateSuspension = "D";
			tax1.G4_RateOverride = "DTY";
			tax1.G4_Amount = "44.55";
			tax1.G4_MethodOfPayment = "A";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1234567890";
			var tax2 = invoiceLine1.Taxes.AddNew().Data;
			tax2.G4_Type = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			tax2.G4_BaseAmount = 1000m;
			tax2.G4_RateDuty = TaxRateCustomsDutyListImport.Codes.DutyTheGoodsAreLiableToDutyAtTheFullRateThisIncludesGoodsBeingEnteredForATariffQuotaReliefToWhichNoneOfTheCodesBelowApply;
			tax2.G4_RateSuspension = "D";
			tax2.G4_RateOverride = "DTY";
			tax2.G4_Amount = "55.44";
			tax2.G4_MethodOfPayment = "A";

			var tax3 = invoiceLine1.Taxes.AddNew().Data;
			tax3.G4_Type = UniversalReferenceConstants.RefCusRateCodes.Vat;
			tax3.G4_BaseAmount = 1500m;
			tax3.G4_RateDuty = TaxRateVATDutyListImport.Codes.VATTheGoodsAreLiableToVATAtTheStandardRate;
			tax3.G4_RateSuspension = "";
			tax3.G4_RateOverride = "VAX";
			tax3.G4_Amount = "66.77";
			tax3.G4_MethodOfPayment = "G";

			DoMerge(declaration);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			AssertEquals(2, entryLine.Taxes.Count);

			AssertEquals("Line 1 47a - Type", "A00", entryLine.Taxes[0].G4_Type);
			AssertEquals("Line 1 47b - Tax Base", "3000.00", entryLine.Taxes[0].Box47b);
			AssertEquals("Line 1 47c1 - Rate Part 1", "F D", entryLine.Taxes[0].Box47c1);
			AssertEquals("Line 1 47c2 - Rate Part 2", "DTY", entryLine.Taxes[0].G4_RateOverride);
			AssertEquals("Line 1 47d - Amount", "99.99", entryLine.Taxes[0].G4_Amount);
			AssertEquals("Line 1 47e - Method of Payment", "A", entryLine.Taxes[0].G4_MethodOfPayment);

			AssertEquals("Line 2 47a - Type", "B00", entryLine.Taxes[1].G4_Type);
			AssertEquals("Line 2 47b - Tax Base", "1500.00", entryLine.Taxes[1].Box47b);
			AssertEquals("Line 2 47c1 - Rate Part 1", "S", entryLine.Taxes[1].Box47c1);
			AssertEquals("Line 2 47c2 - Rate Part 2", "VAX", entryLine.Taxes[1].G4_RateOverride);
			AssertEquals("Line 2 47d - Amount", "66.77", entryLine.Taxes[1].G4_Amount);
			AssertEquals("Line 2 47e - Method of Payment", "G", entryLine.Taxes[1].G4_MethodOfPayment);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.CusEntryLine to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryLine)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestStatisticalValue()
		{
			var entryLine = Factory.New<T>();
			var mock1 = Factory.NewMoq<JobComInvoiceLine>();
			mock1.Setup(m => m.ZG_StatisticalValue).Returns(100);
			mock1.Object.JI_CL = entryLine.PK;

			AssertEquals(100m, entryLine.StatisticalValue);
		}

		public void TestThirdQuantity()
		{
			var entryLine = Factory.New<T>();
			var mock1 = Factory.NewMoq<JobComInvoiceLine>();
			mock1.Setup(m => m.JI_CustomsThirdQuantity).Returns(100);
			mock1.Setup(m => m.JI_CustomsThirdUnitQty).Returns("HLT");
			mock1.Object.JI_CL = entryLine.PK;
			var mock2 = Factory.NewMoq<JobComInvoiceLine>();
			mock2.Setup(m => m.JI_CustomsThirdQuantity).Returns(200);
			mock2.Object.JI_CL = entryLine.PK;

			AssertEquals("Entry line third quantity should be the sum of its invoice lines third quantities.", 300m, entryLine.ThirdQuantity);
			AssertEquals("Entry line third quantity unit should be the third quantity unit of any of its invoice lines.", "HLT", entryLine.ThirdUQ);
		}

		public void TestPreviousDocuments() => CombineAssertions(() => // Box40
		{
			var declaration = GetJobDeclarationForTestWithValidTestData();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var aPrevDocOnInvHeader = invoice.PreviousDocuments.AddNew();
			aPrevDocOnInvHeader.CSI_Code = "380";
			aPrevDocOnInvHeader.CSI_SubType = "X";
			aPrevDocOnInvHeader.CSI_ReferenceNumber = "PREVDOC INVHDR";

			var aPrevDocOnInvLine1 = invoiceLine.PreviousDocuments.AddNew();
			aPrevDocOnInvLine1.CSI_Code = "123";
			aPrevDocOnInvLine1.CSI_SubType = "Y";
			aPrevDocOnInvLine1.CSI_ReferenceNumber = "PREVDOC INVLINE";

			var aPrevDocOnInvLine2 = invoiceLine.PreviousDocuments.AddNew();
			aPrevDocOnInvLine2.CSI_Code = "456";
			aPrevDocOnInvLine2.CSI_SubType = "K";
			aPrevDocOnInvLine2.CSI_ReferenceNumber = "PREVDOC INVLINE";

			var aPrevDocOnGroupHeader = declaration.PreviousDocuments.AddNew();
			aPrevDocOnGroupHeader.CSI_Code = "987";
			aPrevDocOnGroupHeader.CSI_SubType = "Z";
			aPrevDocOnGroupHeader.CSI_ReferenceNumber = "PREVDOC GRPHDR";

			DoMerge(declaration);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			var aPrevDocToTest = entryLine.PreviousDocuments.GetEnumerator();
			var aPrevDocExpectedToBeFound = ExpectedPreviousDocumentCodesAndSubTypes.GetEnumerator();

			AssertEquals("Expected Previous Documents found", ExpectedPreviousDocumentCodesAndSubTypes.Count, entryLine.PreviousDocuments.Count());
			while (aPrevDocToTest.MoveNext())
			{
				aPrevDocExpectedToBeFound.MoveNext();
				AssertEquals($"aPrevDocExpectedToBeFound.CSI_Code = {aPrevDocExpectedToBeFound.Current.CSI_Code}", aPrevDocExpectedToBeFound.Current.CSI_Code, aPrevDocToTest.Current.CSI_Code);
				if (!string.IsNullOrEmpty(aPrevDocExpectedToBeFound.Current.CSI_SubType))
				{
					AssertEquals($"aPrevDocExpectedToBeFound.CSI_SubType = {aPrevDocExpectedToBeFound.Current.CSI_SubType}", aPrevDocExpectedToBeFound.Current.CSI_SubType, aPrevDocToTest.Current.CSI_SubType);
				}
			}
		});

		protected virtual List<(string CSI_Code, string CSI_SubType)> ExpectedPreviousDocumentCodesAndSubTypes => new List<(string, string)>
		{
			new ("987", null),
			new ("380", null),
			new ("123", "Y"),
			new ("456", "K"),
		};

		public void TestGetPreviousDocuments_CheckForNullInvoiceHeader()
		{
			var jobDeclaration = GetJobDeclarationForTestWithValidTestData();

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var aPrevDocOnInvHeader = invoiceHeader.PreviousDocuments.AddNew();
			aPrevDocOnInvHeader.CSI_Code = "380";
			aPrevDocOnInvHeader.CSI_SubType = "X";
			aPrevDocOnInvHeader.CSI_ReferenceNumber = "PREVDOC INVHDR";

			var aPrevDocOnInvLine = invoiceLine.PreviousDocuments.AddNew();
			aPrevDocOnInvLine.CSI_Code = "123";
			aPrevDocOnInvLine.CSI_SubType = "Y";
			aPrevDocOnInvLine.CSI_ReferenceNumber = "PREVDOC INVLINE";

			var aPrevDocOnGroupHeader = jobDeclaration.PreviousDocuments.AddNew();
			aPrevDocOnGroupHeader.CSI_Code = "987";
			aPrevDocOnGroupHeader.CSI_SubType = "Z";
			aPrevDocOnGroupHeader.CSI_ReferenceNumber = "PREVDOC GRPHDR";

			DoMerge(jobDeclaration);

			var entryLine = jobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			invoiceLine.JI_JZ = ZGuid.Empty;
			invoiceLine.JI_JZ = ZGuid.Empty;
			AssertNoExceptionThrown(() => entryLine.PreviousDocuments.GetEnumerator());
		}

		public void TestGetPreviousDocuments_CheckForNullGroupHeader()
		{
			var jobDeclaration = GetJobDeclarationForTestWithValidTestData();

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var aPrevDocOnInvHeader = invoiceHeader.PreviousDocuments.AddNew();
			aPrevDocOnInvHeader.CSI_Code = "380";
			aPrevDocOnInvHeader.CSI_SubType = "X";
			aPrevDocOnInvHeader.CSI_ReferenceNumber = "PREVDOC INVHDR";

			var aPrevDocOnInvLine = invoiceLine.PreviousDocuments.AddNew();
			aPrevDocOnInvLine.CSI_Code = "123";
			aPrevDocOnInvLine.CSI_SubType = "Y";
			aPrevDocOnInvLine.CSI_ReferenceNumber = "PREVDOC INVLINE";

			var aPrevDocOnGroupHeader = jobDeclaration.PreviousDocuments.AddNew();
			aPrevDocOnGroupHeader.CSI_Code = "987";
			aPrevDocOnGroupHeader.CSI_SubType = "Z";
			aPrevDocOnGroupHeader.CSI_ReferenceNumber = "PREVDOC GRPHDR";

			DoMerge(jobDeclaration);

			var entryLine = jobDeclaration.CustomsEntryHeaders[0].MergedLines[0];
			invoiceHeader.JZ_JZ_GroupInvoiceFK = ZGuid.Empty;
			AssertNoExceptionThrown(() => entryLine.PreviousDocuments.GetEnumerator());
		}

		public void TestGetPreviousDocuments_CheckForNullPreviousDocumentKeys()
		{
			var entryLine = Factory.New<CusEntryLine>();
			AssertNoExceptionThrown(() => entryLine.PreviousDocuments.GetEnumerator());
		}

		public void TestSupplementaryDocuments()
		{
			var latviaCountryCode = Core.Constants.CountryCodes.Latvia;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(latviaCountryCode))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
				var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				var attributeNameValuePairs = new Dictionary<string, string[]>();

				var levelAttributeName = Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.Level;
				var headerAttributeValue = UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header;
				var itemAttributeValue = UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item;
				attributeNameValuePairs.Add(levelAttributeName, new string[] { itemAttributeValue });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(latviaCountryCode, new string[] { importCodeType, exportCodeType }, "123", "Test 123"
					, attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

				helper.CreateCusCodeListsForMultipleTypesWithAttributes(latviaCountryCode, new string[] { importCodeType, exportCodeType }, "380", "Test 380"
					, attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

				helper.CreateCusCodeListsForMultipleTypesWithAttributes(latviaCountryCode, new string[] { importCodeType, exportCodeType }, "456", "Test 456"
					, attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

				attributeNameValuePairs.Clear();
				attributeNameValuePairs.Add(levelAttributeName, new string[] { headerAttributeValue });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(latviaCountryCode, new string[] { importCodeType, exportCodeType }, "789", "Test 789"
					, attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

				attributeNameValuePairs.Clear();
				attributeNameValuePairs.Add(levelAttributeName, new string[] { itemAttributeValue, headerAttributeValue });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(latviaCountryCode, new string[] { importCodeType, exportCodeType }, "999", "Test 999"
					, attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

				Factory.Save();

				AssertSupplementaryDocuments_ShouldFilterSupportingDocumentsByMergeKeys(true);
				AssertSupplementaryDocuments_ShouldFilterSupportingDocumentsByMergeKeys(false);
			}
		}

		void AssertSupplementaryDocuments_ShouldFilterSupportingDocumentsByMergeKeys(ZBool shouldFilterSupportingDocumentsByMergeKeys)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var aSuppDoc = invoice.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "123";

			aSuppDoc = invoice.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "999";

			aSuppDoc = invoiceLine.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "999";

			aSuppDoc = invoiceLine.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "380";

			aSuppDoc = invoiceLine.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "456";

			aSuppDoc = declaration.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "789";

			Factory.Save();

			var mockEntryLineConfiguration = new Mock<EntryLineConfiguration>();
			var entryLineConfiguration = mockEntryLineConfiguration.Object;
			mockEntryLineConfiguration.Protected().Setup<ZBool>("ShouldFilterSupportingDocumentsByMergeKeysCore").Returns(shouldFilterSupportingDocumentsByMergeKeys);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "GetNewEntryLineConfiguration", entryLineConfiguration))
			{
				DoMerge(declaration);

				var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

				var aSuppDocToTest = entryLine.SupportingDocuments.GetEnumerator();

				CombineAssertions($"ShouldFilterSupportingDocumentsByMergeKeys : {shouldFilterSupportingDocumentsByMergeKeys}", () =>
				{
					aSuppDocToTest.MoveNext();
					AssertEquals("aSuppDocToTest.Current.CSI_Code = 123", aSuppDocToTest.Current.CSI_Code, "123");

					if (!shouldFilterSupportingDocumentsByMergeKeys)
					{
						aSuppDocToTest.MoveNext();
						AssertEquals("aSuppDocToTest.Current.CSI_Code = 999", aSuppDocToTest.Current.CSI_Code, "999");
					}

					aSuppDocToTest.MoveNext();
					AssertEquals("aSuppDocToTest.Current.CSI_Code = 380", aSuppDocToTest.Current.CSI_Code, "380");
					aSuppDocToTest.MoveNext();
					AssertEquals("aSuppDocToTest.Current.CSI_Code = 456", aSuppDocToTest.Current.CSI_Code, "456");
				});
			}
		}

		protected override ZString ExpectedFallbackEntrylineDescription => ZString.Empty;

		[TestDate(2005, 6, 2)]
		public override void TestMoneyInLocalCurrency()
		{
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			var from = new ZDateTime(2005, 6, 1);
			var to = new ZDateTime(2005, 6, 5);
			newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m);
			var declaration = SetUpDeclarationAndInvLinesForMoneyTest(newCurrency);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			// Do not assert FOB.  FOB in base is wrong. 
			AssertEquals("CIF", 690.0m, entryLine.CIFInLocalCurrency.Amount);
			AssertEquals("OFT", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
			AssertEquals("ONS", 30.0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
			AssertEquals("T&I", 90.0m, entryLine.TAndIInLocalCurrency.Amount);
		}

		public virtual BaseJobDeclaration SetUpDeclarationForMoneyTest() => ImportJobDeclaration;

		protected virtual BaseJobDeclaration SetUpDeclarationAndInvLinesForMoneyTest(RefCurrency currency)
		{
			var declaration = SetUpDeclarationForMoneyTest();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = currency.RX_Code;

			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2203.10.10 10";
			line2.JI_Tariff = "2203.10.10 10";
			line1.JI_LinePrice = 100.0m;
			line2.JI_LinePrice = 200.0m;

			var line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line1ONS.J7_Amount = 5.0m;
			line1ONS.J7_IsDutiable = true;
			var line1OFT = line1.Charges.AddNew(OverseasFreightCode);
			line1OFT.J7_Amount = 10.0m;
			line1OFT.J7_IsDutiable = true;
			var line2ONS = line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line2ONS.J7_Amount = 10.0m;
			line2ONS.J7_IsDutiable = true;
			var line2OFT = line2.Charges.AddNew(OverseasFreightCode);
			line2OFT.J7_IsDutiable = true;
			line2OFT.J7_Amount = 20.0m;

			AssertEquals("PreReq, line 1 CIF is 115", 115.0m, line1.JI_CIF.Amount);
			AssertEquals("PreReq, line 2 CIF is 230", 230.0m, line2.JI_CIF.Amount);
			AssertEquals("PreReq, line 1 CIF currency is invoice currency", currency.Code, line1.JI_CIF.Currency.Code);
			AssertEquals("PreReq, line 2 CIF currency is invoice currency", currency.Code, line2.JI_CIF.Currency.Code);
			AssertEquals("PreReq, line 1 CIF is 115", 115.0m, line1.JI_Calc_CIF);
			AssertEquals("PreReq, line 2 CIF is 230", 230.0m, line2.JI_Calc_CIF);
			DoMerge(declaration);
			return declaration;
		}

		[TestDate(2005, 6, 2)]
		public virtual void TestMoneyInLocalCurrencyWhenAllChargesAreInLocalCurrency()
		{
			var declaration = SetUpDeclarationAndInvLinesForMoneyTest(GlbCompany.CurrentCompany.LocalCurrency);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			// Do not assert FOB.  FOB in base is wrong. 
			AssertEquals("CIF", 345.0m, entryLine.CIFInLocalCurrency.Amount);
			AssertEquals("OFT", 30.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
			AssertEquals("ONS", 15.0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
			AssertEquals("T&I", 45.0m, entryLine.TAndIInLocalCurrency.Amount);
		}

		protected virtual string OverseasFreightCode => Common.CustomsChargeTypeList.Codes.OverseasFreight;

		public void TestGrossWeightForCommericalPurpose()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Weight = 100.00m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			var testWeight = new ZArchitecture.ZWeight(100.00m, Core.Constants.Weight.Kilograms);
			AssertEquals(testWeight, entryLine.GrossWeightForCommericalPurpose);
		}

		public override void TestDutyRateDescription()
		{
			var cusEntryLine = Factory.New<T>();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = cusEntryLine.PK;
			AssertEquals(ZDecimal.Zero, cusEntryLine.GSTRate);
			AssertEquals(ZString.Empty, cusEntryLine.DutyRateDescription);
			AssertEquals(ZString.Empty, invoiceLine.DutyAmountsAsString);
			var b00Fee = cusEntryLine.Fees.AddNew();
			b00Fee.CF_Rate = 17.5;
			b00Fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.Vat;
			var a30Fee = cusEntryLine.Fees.AddNew();
			a30Fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
			a30Fee.CF_ChargeAmount = 202m;
			a30Fee.CF_BaseValue = 404m;
			a30Fee.CF_Rate = 50;
			var a00Fee = cusEntryLine.Fees.AddNew();
			a00Fee.CF_ChargeAmount = 33m;
			a00Fee.CF_BaseValue = 100m;
			a00Fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			a00Fee.CF_Rate = 33;
			AssertEquals("GetGSTRate(): GST/VAT rates pulls only the B00 & B05 tax lines and looks at its Calculated Percentage", 17.5m, cusEntryLine.GSTRate);
			AssertEquals("GetDutyRateDescription(): Pulls all but VAT, sorted by code, formatted % per line", "A00:33%\r\nA30:50%", cusEntryLine.DutyRateDescription);
			AssertEquals("DutyAmountsAsString(Core): Pulls all but VAT, sorted by code, formatted £ per line", TestDutyRateDescriptionExpectedDutyAmountsAsStringA00A30_1, invoiceLine.DutyAmountsAsString);

			b00Fee.CF_ChargeAmount = 363.63m;
			b00Fee.CF_BaseValue = 1818.17m;
			b00Fee.CF_Rate = 19.999977999;

			a30Fee.CF_ChargeAmount = 170.96m;
			a30Fee.CF_BaseValue = 569.87;
			a30Fee.CF_Rate = 29.9998245;

			a00Fee.CF_ChargeAmount = 35.33m;
			a00Fee.CF_BaseValue = 1766.82m;
			a00Fee.CF_Rate = 1.9996377;

			AssertEquals("GetGSTRate():  GST/VAT rates pulls only the B00 & B05 tax lines and looks at its Calculated Percentage and should be suitably rounded", 20.0m, cusEntryLine.GSTRate);
			AssertEquals("GetDutyRateDescription(): Pulls all but VAT, sorted by code, formatted % per line", "A00:2%\r\nA30:30%", cusEntryLine.DutyRateDescription);
			AssertEquals("DutyAmountsAsString(Core): Pulls all but VAT, sorted by code, formatted £ per line", TestDutyRateDescriptionExpectedDutyAmountsAsStringA00A30_2, invoiceLine.DutyAmountsAsString);

			var a00Fee2 = cusEntryLine.Fees.AddNew();
			a00Fee2.CF_ChargeAmount = 50m;
			a00Fee2.CF_BaseValue = 200m;
			a00Fee2.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			a00Fee2.CF_Rate = 25;

			AssertEquals("GetGSTRate():  GST/VAT rates pulls only the B00 & B05 tax lines and looks at its Calculated Percentage and should be suitably rounded", 20.0m, cusEntryLine.GSTRate);
			AssertEquals("GetDutyRateDescription(): Multiple A00 Fees so only show A30", "A30:30%", cusEntryLine.DutyRateDescription);
			AssertEquals("DutyAmountsAsString(Core): Multiple A00 Fees so only show A30", TestDutyRateDescriptionExpectedDutyAmountsAsStringA30_1, invoiceLine.DutyAmountsAsString);

			var b00Fee2 = cusEntryLine.Fees.AddNew();
			b00Fee2.CF_Rate = 17.5;
			b00Fee2.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.Vat;

			AssertEquals("GetGSTRate():  GST/VAT rates pulls only the B00 & B05 tax lines and looks at its Calculated Percentage and should be suitably rounded", 0m, cusEntryLine.GSTRate);
			AssertEquals("GetDutyRateDescription(): Multiple A00 Fees so only show A30", "A30:30%", cusEntryLine.DutyRateDescription);
			AssertEquals("DutyAmountsAsString(Core): Multiple A00 Fees so only show A30", TestDutyRateDescriptionExpectedDutyAmountsAsStringA30_2, invoiceLine.DutyAmountsAsString);

			var a30Fee2 = cusEntryLine.Fees.AddNew();
			a30Fee2.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
			a30Fee2.CF_ChargeAmount = 202m;
			a30Fee2.CF_BaseValue = 404m;
			a30Fee2.CF_Rate = 50;

			AssertEquals("GetGSTRate():  Multiple B00 fees so is 0", 0m, cusEntryLine.GSTRate);
			AssertEquals("GetDutyRateDescription(): Multiple A00 and A30 Fees so is blank", "", cusEntryLine.DutyRateDescription);
			AssertEquals("DutyAmountsAsString(Core): Multiple A00 and A30 Fees so is blank", "", invoiceLine.DutyAmountsAsString);
		}

		protected virtual string TestDutyRateDescriptionExpectedDutyAmountsAsStringA00A30_1 => "A00:33.00\r\nA30:202.00";
		protected virtual string TestDutyRateDescriptionExpectedDutyAmountsAsStringA00A30_2 => "A00:35.33\r\nA30:170.96";
		protected virtual string TestDutyRateDescriptionExpectedDutyAmountsAsStringA30_1 => "A30:170.96";
		protected virtual string TestDutyRateDescriptionExpectedDutyAmountsAsStringA30_2 => "A30:170.96";

		public void TestDutyAmountAndVATAmount()
		{
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, "Netherlands", euGrouping);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Netherlands))
			{
				var declaration = GetJobDeclarationForTest();
				var header = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = header.AllEntryLines.AddNew();
				entryLine.Fees.AddOrUpdate("A00", 100m);
				entryLine.Fees.AddOrUpdate("B00", 250m);
				entryLine.Fees.AddOrUpdate("B05", 300m);

				AssertEquals(100m, entryLine.DutyAmount);
				AssertEquals(550m, entryLine.GSTVATAmount);
			}

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "UnitedKingdom", euGrouping);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = GetJobDeclarationForTest();
				declaration.JE_ApplicationCode = "CHF";
				var header = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = header.AllEntryLines.AddNew();
				entryLine.Fees.AddOrUpdate("A00", 110m);
				entryLine.Fees.AddOrUpdate("B00", 240m);
				entryLine.Fees.AddOrUpdate("B05", 300m);
				AssertEquals(110m, entryLine.DutyAmount);
				AssertEquals(540m, entryLine.GSTVATAmount);
			}
		}

		public void TestDutyDetails()
		{
			// Create rate codes of different types
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty, UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, UniversalReferenceConstants.RefCusRateCodes.Vat);

			var declaration = GetJobDeclarationForTest();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var fee1 = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
			fee1.CF_ChargeAmount = 200m;

			CombineAssertions("EntryLine with only 1 fee: A00.", () =>
			{
				AssertEquals("Customs Duty On Industrial Products (A00) fee", 200m, entryLine.Fees.GetAmount(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts));
				AssertEquals("DutyDetails", 200m, entryLine.DutyDetails);
			});

			var fee2 = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
			fee2.CF_ChargeAmount = 100m;

			CombineAssertions("EntryLine with 2 DTY type fees: A00, EA.", () =>
			{
				AssertEquals("Agricultural Component (EA) fee", 100m, entryLine.Fees.GetAmount(UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent));
				AssertEquals("DutyDetails", 300m, entryLine.DutyDetails);
			});

			var fee3 = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.Vat);
			fee3.CF_ChargeAmount = 50m;

			CombineAssertions("After adding non DTY type fee VAT, not included in the duty amount calculation.", () =>
			{
				AssertEquals("VAT fee", 50m, entryLine.Fees.GetAmount(UniversalReferenceConstants.RefCusRateCodes.Vat));
				AssertEquals("DutyDetails", 300m, entryLine.DutyDetails);
			});

			var fee4 = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty);
			fee4.CF_ChargeAmount = 70m;

			CombineAssertions("EntryLine with 4 fees, 3 of which Duty types (DTY, ADD, CVD): A00, EA, A40.", () =>
			{
				AssertEquals("Definitive Countervailing Duty (A40) fee", 70m, entryLine.Fees.GetAmount(UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty));
				AssertEquals("DutyDetails", 370m, entryLine.DutyDetails);
			});

			var fee5 = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);
			fee5.CF_ChargeAmount = 40m;

			CombineAssertions("EntryLine with 5 fees, 4 of which Duty types (DTY, ADD, CVD): A00, EA, A40, A35.", () =>
			{
				AssertEquals("Provisional Anti-Dumping Duty (A35) fee", 40m, entryLine.Fees.GetAmount(UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty));
				AssertEquals("DutyDetails", 410m, entryLine.DutyDetails);
			});

			var fee6 = entryLine.Fees.AddNew();
			fee6.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee6.CF_ChargeAmount = 60;
			fee6.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;

			CombineAssertions("EntryLine with 6 fees, 5 of which Duty types (DTY, ADD, CVD): A00, EA, A40, A35, A00.", () =>
			{
				AssertEquals("Sum of Customs Duty On Industrial Products (A00) fees", 260m, entryLine.Fees.GetTotalAmount(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, false));
				AssertEquals("DutyDetails", 470m, entryLine.DutyDetails);
			});
		}

		public void TestDutyDetailsWithProcedure()
		{
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty, UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, UniversalReferenceConstants.RefCusRateCodes.Vat);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IM", "A", "", "   ", "Procedure A", "IMP", "10P");
			procedure1.ZZ6_CalculateDuty = true;
			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IM", "B", "", "   ", "Procedure B", "IMP", "10P");
			procedure2.ZZ6_CalculateDuty = false;
			Factory.Save();

			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var declaration = GetJobDeclarationForTest();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_Procedure = procedure1.ZZ6_ProcedureCode;
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_Procedure = procedure2.ZZ6_ProcedureCode;

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine1 = entryHeader.MergedLines.AddNew();
				invoiceLine1.JI_CL = entryLine1.PK;
				var entryLine2 = entryHeader.MergedLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;

				foreach (var entryLine in new[] { entryLine1, entryLine2 })
				{
					entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts).CF_ChargeAmount = 10m;
					entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent).CF_ChargeAmount = 20m;
					entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty).CF_ChargeAmount = 30m;
					entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty).CF_ChargeAmount = 40m;

					entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.Vat).CF_ChargeAmount = 1000m;
				}

				CombineAssertions(() =>
				{
					AssertEquals("DutyAmount ZZ6_CalculateDuty = true", 100m, entryLine1.DutyAmount);
					AssertEquals("DutyAmount ZZ6_CalculateDuty = false", 0m, entryLine2.DutyAmount);

					AssertEquals("GSTVATAmount ZZ6_CalculateDuty = true", 1000m, entryLine1.GSTVATAmount);
					AssertEquals("GSTVATAmount ZZ6_CalculateDuty = false", 1000m, entryLine2.GSTVATAmount);
				});
			}
		}

		public void TestDutyDetailsForVAT()
		{
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty, UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, UniversalReferenceConstants.RefCusRateCodes.Vat);

			var declaration = GetJobDeclarationForTest();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var fee1 = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
			fee1.CF_ChargeAmount = 200m;

			CombineAssertions("EntryLine with only 1 fee: A00.", () =>
			{
				AssertEquals("Customs Duty On Industrial Products (A00) fee", 200m, entryLine.Fees.GetAmount(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts));
				AssertEquals("DutyDetailsForVAT", 200m, entryLine.DutyDetailsForVAT);
			});

			var fee2 = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
			fee2.CF_ChargeAmount = 100m;

			CombineAssertions("EntryLine with 2 DTY type fees: A00, EA.", () =>
			{
				AssertEquals("Agricultural Component (EA) fee", 100m, entryLine.Fees.GetAmount(UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent));
				AssertEquals("DutyDetailsForVAT", 300m, entryLine.DutyDetailsForVAT);
			});

			var fee3 = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.Vat);
			fee3.CF_ChargeAmount = 50m;

			CombineAssertions("After adding non DTY type fee VAT, not included in the duty amount calculation.", () =>
			{
				AssertEquals("VAT fee", 50m, entryLine.Fees.GetAmount(UniversalReferenceConstants.RefCusRateCodes.Vat));
				AssertEquals("DutyDetailsForVAT", 300m, entryLine.DutyDetailsForVAT);
			});

			var fee4 = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty);
			fee4.CF_ChargeAmount = 70m;

			CombineAssertions("EntryLine with 4 fees, 3 of which Duty types (DTY, ADD, CVD): A00, EA, A40.", () =>
			{
				AssertEquals("Definitive Countervailing Duty (A40) fee", 70m, entryLine.Fees.GetAmount(UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty));
				AssertEquals("DutyDetailsForVAT", 370m, entryLine.DutyDetailsForVAT);
			});

			var fee5 = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);
			fee5.CF_ChargeAmount = 40m;

			CombineAssertions("EntryLine with 5 fees, 4 of which Duty types (DTY, ADD, CVD): A00, EA, A40, A35.", () =>
			{
				AssertEquals("Provisional Anti-Dumping Duty (A35) fee", 40m, entryLine.Fees.GetAmount(UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty));
				AssertEquals("DutyDetailsForVAT", 410m, entryLine.DutyDetailsForVAT);
			});

			var fee6 = entryLine.Fees.AddNew();
			fee6.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee6.CF_ChargeAmount = 60;
			fee6.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;

			CombineAssertions("EntryLine with 6 fees, 5 of which Duty types (DTY, ADD, CVD): A00, EA, A40, A35, A00.", () =>
			{
				AssertEquals("Sum of Customs Duty On Industrial Products (A00) fees", 260m, entryLine.Fees.GetTotalAmount(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, false));
				AssertEquals("DutyDetailsForVAT", 470m, entryLine.DutyDetailsForVAT);
			});
		}

		public void TestDutyDetailsForDutyRateCodesDataGrouping()
		{
			var declaration = Factory.New<JobDeclaration>();

			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes), Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var fee1 = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
			fee1.CF_ChargeAmount = 100m;

			CombineAssertions("EntryLine with 2 DTY type fees: A00, EA.", () =>
			{
				AssertEquals("Agricultural Component (EA) fee", 100m, entryLine.Fees.GetAmount(UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent));
				AssertEquals("DutyDetails", 100m, entryLine.DutyDetails);
			});
		}

		public void TestDutyDetailsReturnsZeroWithNoDeclaration()
		{
			var entryLine = Factory.New<T>();
			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee1.CF_ChargeAmount = 100m;
			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee2.CF_ChargeAmount = 50m;

			CombineAssertions(() =>
			{
				AssertEquals("GetTotalAmount (total amount of specific rate type)", 150m, entryLine.Fees.GetTotalAmount(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, true));
				AssertEquals("DutyDetails (when CusEntryLine has duty fees but is not attached to a Declaration)", 0m, entryLine.DutyDetails);
			});
		}

		protected virtual string StatisticalValueApplicableCharge => "ONS";

		public void TestCalculateStatisticalValue()
		{
			/*
			--Declaration
				--Invoice1
					--InvoiceLine1_1 (LinePrice = 1m) => Contribute 1m to StatisticalValue.
						--Charges
							--Charge1_1_1 (ONS, Amount = 10m EUR) => Contribute 10m to StatisticalValue.
							--Charge1_1_2 (ABC, Amount = 100m EUR) => Exclusive as wrong charge code.
					--InvoiceLine1_2 (LinePrice = 1000m) => Contribute 1000m to StatisticalValue
				--Invoice2
					--InvoiceLine2_1 (LinePrice = 10000m) => Contribute 10000m to StatisticalValue
			*/

			var declaration = SetUpDeclarationForCalculateStatisticalValueTest();
			var localCurrencyCode = declaration.LocalCurrencyCode;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = localCurrencyCode;
			var invoiceLine1_1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1_1.JI_LinePrice = 1m;

			var charge1_1_1 = invoiceLine1_1.Charges.AddNew();
			charge1_1_1.J7_ChargeType = StatisticalValueApplicableCharge;
			charge1_1_1.J7_RX_NKCurrency = localCurrencyCode;
			charge1_1_1.J7_Amount = 10m;

			var charge1_1_2 = invoiceLine1_1.Charges.AddNew();
			charge1_1_2.J7_ChargeType = "ABC";
			charge1_1_2.J7_RX_NKCurrency = localCurrencyCode;
			charge1_1_2.J7_Amount = 100m;

			var invoiceLine1_2 = invoice1.InvoiceLines.AddNew();
			invoiceLine1_2.JI_LinePrice = 1000m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = localCurrencyCode;
			var invoiceLine2_1 = invoice2.InvoiceLines.AddNew();
			invoiceLine2_1.JI_LinePrice = 10000m;

			AssertEquals(1m + 10m, invoiceLine1_1.JI_Calc_StatisticalValue);
			AssertEquals(1000m, invoiceLine1_2.JI_Calc_StatisticalValue);
			AssertEquals(10000m, invoiceLine2_1.JI_Calc_StatisticalValue);

			DoMerge(declaration);

			AssertEquals(1m + 10m + 1000m + 10000m, ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0].CL_StatisticalValue);
		}

		public void TestICanBeImportOrExport()
		{
			var declaration = GetJobDeclarationForTest();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();

			DoMerge(declaration);

			var iore = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0] as ICanBeImportOrExport;
			AssertNotNull("invoiceLine as ICanBeImportOrExport", iore);
			AssertEquals("Item", iore.Level);
			AssertEquals("Country Code", declaration.CountryCode, iore.TrueCountryCode);
			AssertEquals("Data Grouping", declaration.GetDefaultDataGroupingCode(), iore.DataGroupingCode);

			declaration.JE_MessageType = ZString.Empty;
			AssertEquals("iore.IsImport", false, iore.IsImport);
			AssertEquals("iore.IsExport", false, iore.IsExport);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("iore.IsImport", true, iore.IsImport);
			AssertEquals("iore.IsExport", false, iore.IsExport);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("iore.IsImport", false, iore.IsImport);
			AssertEquals("iore.IsExport", true, iore.IsExport);

			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("iore.IsImport", false, iore.IsImport);
			AssertEquals("iore.IsExport", false, iore.IsExport);
		}

		public void TestTransportChargesMethodOfPayment()
		{
			var jobDeclaration = GetJobDeclarationForTest();

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			DoMerge(jobDeclaration);
			var entryLine = (T)jobDeclaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals("No MOP", ZString.Empty, entryLine.TransportChargesMethodOfPayment);

			invoiceHeader.ZG_TransportChargesMethodOfPayment = "X";
			DoMerge(jobDeclaration);
			entryLine = (T)jobDeclaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals("Should be set", "X", entryLine.TransportChargesMethodOfPayment);
		}

		protected void SetSupportingDocumentsRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "1234", "1234", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();
		}

		public void TestSupportingDocsAggregationFromMerge()
		{
			SetSupportingDocumentsRefData();

			var declaration = GetJobDeclarationForTest();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoiceHeader1 = declaration.Invoices.AddNew();

			var supportingDocTestHelper = GetSupportingDocTestHelper();

			foreach (var sd in supportingDocTestHelper.GetSupportingDocsForAggregationMergeTest())
			{
				var invLine = invoiceHeader1.JobComInvoiceLines.AddNew();
				invLine.SupportingDocuments.Add(sd);
			}

			DoMerge(declaration);
			AssertEquals("Expecting 1 entry", 1, declaration.ActiveEntryHeaders.Count);
			var lines = declaration.ActiveEntryHeaders[0].MergedLines.Cast<T>();

			var mergedSupDocs = lines.SelectMany(x => x.SupportingDocuments).ToList();
			var expectedSupDocs = supportingDocTestHelper.GetExpectedDocsAfterAggregationMerge();

			AssertEquals("Merged count vs Expected Count", expectedSupDocs.Count, mergedSupDocs.Count);
			foreach (var merged in mergedSupDocs)
			{
				var expected = supportingDocTestHelper.MatchExpected(merged);

				AssertNotNull(supportingDocTestHelper.GetErrorMsg(merged), expected);

				CombineAssertions($"Values for {merged.CSI_ReferenceNumber} {merged.CSI_SubType}", () =>
				{
					supportingDocTestHelper.AssertSupportingDoc(expected, merged);
				});
			}
		}

		public void TestReadOnlySupportingDocs()
		{
			SetSupportingDocumentsRefData();

			var declaration = GetJobDeclarationForTest();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoiceHeader1 = declaration.Invoices.AddNew();

			var supportingDocTestHelper = GetSupportingDocTestHelper();

			foreach (var sd in supportingDocTestHelper.GetSupportingDocsForAggregationMergeTest())
			{
				var invLine = invoiceHeader1.JobComInvoiceLines.AddNew();
				invLine.SupportingDocuments.Add(sd);
			}

			DoMerge(declaration);
			AssertEquals("Expecting 1 entry", 1, declaration.ActiveEntryHeaders.Count);
			var lines = declaration.ActiveEntryHeaders[0].MergedLines.Cast<T>().ToArray();
			foreach (var line in lines)
			{
				AssertEquals("ReadOnly Supporting Documents count", 1, line.ReadOnlySupportingDocuments.Count);
			}
		}

		protected virtual int ExpectedReadOnlySupportingDocumentsCount => 5;

		public void TestResetReadOnlySupportingDocuments()
		{
			SetSupportingDocumentsRefData();

			var declaration = GetJobDeclarationForTest();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			AssertEquals("ReadOnly Supporting Documents count is 0", 0, entryLine.ReadOnlySupportingDocuments.Count);

			var supportingDocTestHelper = GetSupportingDocTestHelper();

			foreach (var sd in supportingDocTestHelper.GetSupportingDocsForAggregationMergeTest())
			{
				invoiceLine1.SupportingDocuments.Add(sd);
			}

			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly Supporting Documents count is 0 before reset", 0, entryLine.ReadOnlySupportingDocuments.Count);
				entryLine.ResetReadOnlySupportingDocuments();
				AssertEquals("ReadOnly Supporting Documents count is correct after reset", ExpectedReadOnlySupportingDocumentsCount, entryLine.ReadOnlySupportingDocuments.Count);
			});
		}

		public void TestContainers()
		{
			var entryLine = Factory.New<T>();
			AssertArrayEqualsByElements(Array.Empty<ZString>(), entryLine.Containers.ToArray());

			var declaration = Factory.New<JobDeclaration>();
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CNT1";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CNT2";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CNT2";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "";

			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;

			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[1].PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[2].PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[3].PK;
			AssertArrayEqualsByElements(new ZString[] { "CNT1", "CNT2" }, entryLine.Containers.ToArray());
		}

		public void TestGetTaxBoxSupporterList()
		{
			var entryLine = GetCusEntryLineForTestGetTaxBoxSupporterList();
			var taxBoxSupporters = entryLine.GetTaxBoxSupporterList();
			CombineAssertions("[PRE-CONDITION]", () =>
			{
				AssertNotNull("Supporter list should not be null", taxBoxSupporters);
				AssertEquals("Expected one and only one supporter", 1, taxBoxSupporters.Count());
			});
			AssertEquals("Expected TaxBoxSupporter type", GetExpectedTaxBoxSupporterType(), taxBoxSupporters.ElementAt(0).GetType());
		}

		protected virtual T GetCusEntryLineForTestGetTaxBoxSupporterList()
		{
			var entryLine = Factory.New<T>();
			entryLine.Fees.AddNew();
			return entryLine;
		}

		protected virtual Type GetExpectedTaxBoxSupporterType() => typeof(CusEntryLineFee);

		protected virtual SupportingDocTestHelper GetSupportingDocTestHelper() => new SupportingDocTestHelper(Factory);

		#region SupportingDocTestHelper
		public class SupportingDocTestHelper
		{
			public SupportingDocTestHelper(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}
			protected BusinessObjectFactory factory;
			protected List<SupportingDocument> expectedSupportingDocs;

			protected virtual Type GetSupportingDocumentType() => typeof(SupportingDocument);

			public virtual List<SupportingDocument> GetSupportingDocsForAggregationMergeTest()
			{
				return new List<SupportingDocument>
				{
					GetSupportingDoc(1, "REF111", false),
					GetSupportingDoc(2, "REF222", false),
					GetSupportingDoc(3, "REF222", true),
					GetSupportingDoc(4, "REF444", false),
					GetSupportingDoc(5, "REF555", false),
					GetSupportingDoc(6, "REF222", false),
					GetSupportingDoc(7, "REF444", false),
					GetSupportingDoc(8, "REF222", true)
				};
			}

			public virtual List<SupportingDocument> GetExpectedDocsAfterAggregationMerge()
			{
				if (expectedSupportingDocs == null)
				{
					expectedSupportingDocs = new List<SupportingDocument>
					{
						GetSupportingDoc(1, "REF111", false),
						GetSupportingDoc(8, "REF222", false, 20),
						GetSupportingDoc(11, "REF222", true, 20),
						GetSupportingDoc(11, "REF444", false, 20),
						GetSupportingDoc(5, "REF555", false)
					};
				}

				return expectedSupportingDocs;
			}

			public virtual SupportingDocument MatchExpected(SupportingDocument merged)
			{
				return expectedSupportingDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == merged.CSI_ReferenceNumber && x.CSI_SubType == merged.CSI_SubType);
			}

			public virtual void AssertSupportingDoc(SupportingDocument expected, SupportingDocument actual)
			{
				Assertion.AssertEquals("Qty1", expected.CSI_Quantity, actual.CSI_Quantity);
				Assertion.AssertEquals("Qty2", expected.CSI_Quantity2, actual.CSI_Quantity2);
				Assertion.AssertEquals("Qty3", expected.CSI_Quantity3, actual.CSI_Quantity3);
				Assertion.AssertEquals("Value", expected.CSI_Value, actual.CSI_Value);
			}

			protected virtual SupportingDocument GetSupportingDoc(int i, ZString refNumber, bool alternateSubType, decimal qty3 = 10.0m, int flag = 0)
			{
				var supDoc = (SupportingDocument)factory.New(GetSupportingDocumentType());
				supDoc.SuspendValidation();

				supDoc.CSI_Code = "1234";
				supDoc.CSI_ReferenceNumber = refNumber;
				supDoc.CSI_SubType = alternateSubType ? "B" : "A"; //Part

				supDoc.CSI_Quantity = i * 10;
				supDoc.CSI_UnitOfQuantity = "BAG";
				supDoc.CSI_Quantity2 = i * 10.1;
				supDoc.CSI_UnitOfQuantity2 = "PKT";
				supDoc.CSI_Value = i * 1000;
				supDoc.CSI_RX_NKCurrency = "GBP";
				supDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
				supDoc.CSI_DateOfExpiry = new ZDateTime(2020, 12, 31);
				supDoc.CSI_Quantity3 = qty3;

				supDoc.CSI_Description = "Testing"; //reason
				supDoc.CSI_ReferenceNumber2 = "REFNUM2"; //Issueing Authority
				supDoc.CSI_AdditionalDescription = "AddDescr";
				supDoc.CSI_CustomsOffice = "ABC";
				supDoc.CSI_Procedure = "XYZ";
				supDoc.CSI_RN_NKCountryCode = "GB";
				supDoc.CSI_Status = "QWE";
				supDoc.CSI_Tariff = "12345";
				supDoc.CSI_Type = "SUP";
				supDoc.CSI_UnitOfQuantity3 = "U3";

				return supDoc;
			}

			public virtual string GetErrorMsg(SupportingDocument merged)
			{
				var msg = $"EU Expected {SupportingDocument.Schema.CSI_ReferenceNumber}: {merged.CSI_ReferenceNumber} {SupportingDocument.Schema.CSI_SubType}: {merged.CSI_SubType}\nPossible Options:\n";

				foreach (var exp in expectedSupportingDocs)
				{
					msg += $"{SupportingDocument.Schema.CSI_ReferenceNumber}: {exp.CSI_ReferenceNumber} {SupportingDocument.Schema.CSI_SubType}: {exp.CSI_SubType}\n";
				}

				return msg;
			}
		}
		#endregion

		public virtual JobDeclaration GetJobDeclarationForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			return declaration;
		}

		public virtual JobDeclaration GetJobDeclarationForTestWithValidTestData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			return declaration;
		}

		protected virtual JobDeclaration SetUpDeclarationForCalculateStatisticalValueTest() => GetJobDeclarationForTest();
		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			PopulateDutyCusRateCodeTestReferenceData();
		}

		void PopulateDutyCusRateCodeTestReferenceData()
		{
			helper = new UniversalReferenceTestDataHelper(Factory);
			euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Description, euGrouping);
			Factory.Save();

			var euDtyRateType = helper.CreateNewOrGetExistingRateType(euGrouping.ZZZ_DataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
			Factory.Save();
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, euDtyRateType.PK);
			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;
		RefDataGrouping euGrouping;
		#endregion
	}
}
