using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Resources;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLine))]
	sealed class CusEntryLineBaseOnlyTest : CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestGrossWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Grams;
			invoiceLine1.JI_Weight = 500000.00;
			invoiceLine1.JI_CL = entryLine1.PK;

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Milligrams;
			invoiceLine2.JI_Weight = 500000000.00m;
			invoiceLine2.JI_CL = entryLine1.PK;

			AssertEquals("Gross Weight must be equal to sum of invoice lines gross Weight", 1000.00m, entryLine1.GrossWeight.InKilograms);
		}

		public void TestFeesAndTaxes()
		{
			var entry = CusEntryHeaderTestHelper.SetupEntryForFeesTest<CusEntryHeader>(Factory);
			entry.CH_JE = Factory.New<JobDeclaration>().PK;
			var line1 = entry.MergedLines[0];
			var line2 = entry.MergedLines[1];
			var line3 = entry.MergedLines[2];
			AssertEquals(0m, line1.VATDetails);
			AssertEquals(200m, line1.DutyDetails);
			AssertEquals("", line1.AllOtherFeeDetails);
			AssertEquals(100m, line2.VATDetails);
			AssertEquals(300m, line2.DutyDetails);
			AssertEquals("", line2.AllOtherFeeDetails);
			AssertEquals(50m, line3.VATDetails);
			AssertEquals(0m, line3.DutyDetails);
			AssertEquals(@"DJC: $10.00 X
LSC: $5.00 Y
JL: $20.00 Y
JNO: $30.00 Z", line3.AllOtherFeeDetails);
		}

		public void TestConsignorConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			var consignor = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Consignor not set", null, entryLine.Consignor);
				AssertEquals("Consignee not set", null, entryLine.Consignee);
				invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
				invoiceLine.JI_OA_ExporterAddress = consignor.MainAddress.PK;
				AssertEquals("Consignor set", consignor.MainAddress, entryLine.Consignor);
				AssertEquals("Consignee set", consignee.MainAddress, entryLine.Consignee);
			});
		}

		public void TestConfirmedFeesReadOnly()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var confirmedFee = Factory.New<CusEntryLineFee>();
			entryLine.ConfirmedFees.Add(confirmedFee);

			AssertEquals("ConfirmedFees count", 1, entryLine.ConfirmedFeesReadOnly.Count);
			AssertEquals("ConfirmedFee", confirmedFee.PK, entryLine.ConfirmedFeesReadOnly[0].PK);
		}

		public void TestConfirmedFeesReadOnly_Cached()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var confirmedFee = Factory.New<CusEntryLineFee>();
			entryLine.ConfirmedFees.Add(confirmedFee);

			var confirmedFeesReadonly = entryLine.ConfirmedFeesReadOnly;
			AssertSame("ConfirmedFeesReadonly same object", confirmedFeesReadonly, entryLine.ConfirmedFeesReadOnly);
		}

		public void TestConfirmedFeesReadOnly_CacheUpdated()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var confirmedFee = Factory.New<CusEntryLineFee>();

			CombineAssertions(() =>
			{
				AssertEquals("ConfirmedFeesReadonly count 0", 0, entryLine.ConfirmedFeesReadOnly.Count);
				entryLine.ConfirmedFees.Add(confirmedFee);
				AssertEquals("ConfirmedFeesReadonly count 1", 1, entryLine.ConfirmedFeesReadOnly.Count);
			});
		}

		public void TestCusSupplyChainActorReferences()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine1Reference1 = invoiceLine1.CusSupplyChainActorReferences.AddNew();
			invoiceLine1Reference1.CFR_Code = "FR1";
			invoiceLine1Reference1.CFR_Reference = "REF1";
			var invoiceLine1Reference2 = invoiceLine1.CusSupplyChainActorReferences.AddNew();
			invoiceLine1Reference2.CFR_Code = "FR1";
			invoiceLine1Reference2.CFR_Reference = "REF2";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var invoiceLine2Reference1 = invoiceLine2.CusSupplyChainActorReferences.AddNew();
			invoiceLine2Reference1.CFR_Code = "FR1";
			invoiceLine2Reference1.CFR_Reference = "REF1";
			var invoiceLine2Reference2 = invoiceLine2.CusSupplyChainActorReferences.AddNew();
			invoiceLine2Reference2.CFR_Code = "FR2";
			invoiceLine2Reference2.CFR_Reference = "REF1";
			var data = entryLine.CusSupplyChainActorReferences;
			AssertSame(data, entryLine.CusSupplyChainActorReferences);
			var refs = data.ToArray();
			AssertEquals("No of data", 3, refs.Length);
			AssertNoExceptionThrown(() =>
			{
				refs.Single(x => x.CFR_Code == "FR1" && x.CFR_Reference == "REF1");
				refs.Single(x => x.CFR_Code == "FR1" && x.CFR_Reference == "REF2");
				refs.Single(x => x.CFR_Code == "FR2" && x.CFR_Reference == "REF1");
			});
		}

		public void TestCusAuthorizationUsages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine1Reference1 = invoiceLine1.CusAuthorizationUsages.AddNew();
			invoiceLine1Reference1.AGC_Code = "FR1";
			invoiceLine1Reference1.AGC_Number = "REF1";
			var invoiceLine1Reference2 = invoiceLine1.CusAuthorizationUsages.AddNew();
			invoiceLine1Reference2.AGC_Code = "FR1";
			invoiceLine1Reference2.AGC_Number = "REF2";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var invoiceLine2Reference1 = invoiceLine2.CusAuthorizationUsages.AddNew();
			invoiceLine2Reference1.AGC_Code = "FR1";
			invoiceLine2Reference1.AGC_Number = "REF1";
			var invoiceLine2Reference2 = invoiceLine2.CusAuthorizationUsages.AddNew();
			invoiceLine2Reference2.AGC_Code = "FR2";
			invoiceLine2Reference2.AGC_Number = "REF1";
			var data = entryLine.CusAuthorizationUsages;
			AssertSame(data, entryLine.CusAuthorizationUsages);
			var refs = data.ToArray();
			AssertEquals("No of data", 3, refs.Length);
			AssertNoExceptionThrown(() =>
			{
				refs.Single(x => x.AGC_Code == "FR1" && x.AGC_Number == "REF1");
				refs.Single(x => x.AGC_Code == "FR1" && x.AGC_Number == "REF2");
				refs.Single(x => x.AGC_Code == "FR2" && x.AGC_Number == "REF1");
			});
		}

		public void TestAddInfo_IsConnectedtoAutoClass()
		{
			AssertEquals(true, typeof(CusEntryLine).IsSubclassOf(typeof(AutoCusEntryLine)));
		}

		public void TestAddInfo_IsAutoGenerated()
		{
			AssertNotNull(typeof(AutoCusEntryLine).GetCustomAttribute<AutoGeneratedSourceCodeAttribute>(inherit: false));
		}

		public void TestAddInfo_HasUseAddInfoPropertyDescriptorsTrue()
		{
			AssertNotNull(typeof(AutoCusEntryLine).GetCustomAttribute<PropertyDescriptorCollectionAttribute>(inherit: false));
		}

		public void TestAddInfoType()
		{
			AssertEquals(typeof(AddInfoCusEntryLine), AutoCusEntryLine.AddInfoType);
		}

		public void TestIsGuaranteeDeferredPaymentUsedWhenMopDetailLevelIsEntryLineFee()
		{
			var entryLine = Factory.New<CusEntryLineFeeForGuaranteeTest_EntryLineFeeMopDetailLevel>();
			AssertEquals(entryLine.MopPaymentDetailsLevel_Exposed, CusEntryLine.MoPLevel.EntryLineFee);

			var entryLineFee = entryLine.Fees.AddNew();
			entryLineFee.CF_MethodOfPayment = "X";
			AssertEquals("IsGuaranteeDeferredPaymentUsed should only be true if there are fees with method of payment 'B' or 'N'.", false, entryLine.IsGuaranteeDeferredPaymentUsed);

			entryLineFee.CF_MethodOfPayment = "B";
			AssertEquals("IsGuaranteeDeferredPaymentUsed should be true if there are fees with method of payment 'B' or 'N'.", true, entryLine.IsGuaranteeDeferredPaymentUsed);
		}

		public void TestIsGuaranteeDeferredPaymentUsedWhenMopDetailLevelIsInvoiceLine()
		{
			var entryLine = Factory.New<CusEntryLineFeeForGuaranteeTest_InvoiceLineMopDetailLevel>();

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals(entryLine.MopPaymentDetailsLevel_Exposed, CusEntryLine.MoPLevel.InvoiceLine);

			invoiceLine.ZG_MethodOfPayment = "X";
			AssertEquals("IsGuaranteeDeferredPaymentUsed should only be true if there is an invoice line with method of payment 'B' or 'N'.", false, entryLine.IsGuaranteeDeferredPaymentUsed);

			invoiceLine.ZG_MethodOfPayment = "B";
			AssertEquals("IsGuaranteeDeferredPaymentUsed should be true if there is an invoice line with method of payment 'B' or 'N'.", true, entryLine.IsGuaranteeDeferredPaymentUsed);
		}

		public void TestIsGuaranteeDeferredPaymentUsedWhenMopDetailLevelIsInvoiceLineTaxes()
		{
			var declaration = GetJobDeclarationForTest();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = Factory.New<CusEntryLineFeeForGuaranteeTest_InvoiceLineTaxMopDetailLevel>();
			entryLine.CL_CH = entryHeader.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var invoiceLineTax = invoiceLine.Taxes.AddNew();

			AssertEquals(entryLine.MopPaymentDetailsLevel_Exposed, CusEntryLine.MoPLevel.InvoiceLineTaxes);

			invoiceLineTax.G4_MethodOfPayment = "X";
			AssertEquals("IsGuaranteeDeferredPaymentUsed should only be true if there is an invoice line tax with method of payment 'B' or 'N'.", false, entryLine.IsGuaranteeDeferredPaymentUsed);

			invoiceLineTax.G4_MethodOfPayment = "B";
			AssertEquals("IsGuaranteeDeferredPaymentUsed should be true if there is an invoice line tax with method of payment 'B' or 'N'.", true, entryLine.IsGuaranteeDeferredPaymentUsed);
		}

		public void TestIsGuaranteeDeferredPaymentUsedWhenMopDetailLevelISEntryLineFee()
		{
			var declaration = GetJobDeclarationForTest();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = Factory.New<CusEntryLineFeeForGuaranteeTest_DeclarationMopDetailLevel>();
			entryLine.CL_CH = entryHeader.PK;

			AssertEquals(entryLine.MopPaymentDetailsLevel_Exposed, CusEntryLine.MoPLevel.Declaration);

			declaration.ZG_MethodOfPayment = "X";
			AssertEquals("IsGuaranteeDeferredPaymentUsed should only be true if the declaration method of payment is 'B' or 'N'.", false, entryLine.IsGuaranteeDeferredPaymentUsed);

			declaration.ZG_MethodOfPayment = "B";
			AssertEquals("IsGuaranteeDeferredPaymentUsed should be true if the declaration method of payment is 'B' or 'N'.", true, entryLine.IsGuaranteeDeferredPaymentUsed);
		}

		public void TestStatisticalValueSTA()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.ActiveEntryHeaders.AddNew();
			var entryLine = Factory.New<CusEntryLineFeeForGuaranteeTest_DeclarationMopDetailLevel>();
			entryLine.CL_CH = entryHeader.PK;

			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals(invoiceLine.JI_Calc_StatisticalValue, 1000m);
			AssertEquals(entryLine.CL_Calc_StatisticalBasisExcludingSTACharge, 1000m);
			invoiceLine.ZG_StatisticalValueManualOverride = false;

			var insuranceCharge = invoiceLine.Charges.AddNew();
			insuranceCharge.J7_ChargeType = "STA";
			insuranceCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			insuranceCharge.J7_Amount = 100m;
			Assert("Pre-Req - insurance is STAT-able", insuranceCharge.J7_IsStatisticalValueApplicable);

			AssertEquals("Stat value  = 1000+100= 1100", 1100m, invoiceLine.JI_Calc_StatisticalValue);
			AssertEquals(invoiceLine.JI_Calc_StatisticalBasisExcludingSTACharge, 1000m);
			AssertEquals(entryLine.CL_Calc_StatisticalBasisExcludingSTACharge, 1000m);
		}

		public void TestClearReadOnlySupportingDocumentsWhenDelete()
		{
			var entryLine = GetEntryLineWithSupportingDocuments();
			var readOnlySupportingDocuments = entryLine.ReadOnlySupportingDocuments;
			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly Supporting Documents count is 5", 5, readOnlySupportingDocuments.Count);
				entryLine.Delete();
				AssertEquals("ReadOnly Supporting Documents cleared after entryLine is deleted", 0, readOnlySupportingDocuments.Count);
			});
		}

		public void TestNoLoadReadOnlySupportingDocumentsForDeletedEntryLine()
		{
			var entryLine = GetEntryLineWithSupportingDocuments();
			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly Supporting Documents count is 5", 5, entryLine.ReadOnlySupportingDocuments.Count);
				entryLine.ResetReadOnlySupportingDocuments();
				entryLine.Delete();
				AssertEquals("ReadOnly Supporting Documents do not load after entryLine is deleted", 0, entryLine.ReadOnlySupportingDocuments.Count);
			});
		}

		CusEntryLine GetEntryLineWithSupportingDocuments()
		{
			SetSupportingDocumentsRefData();
			var declaration = GetJobDeclarationForTest();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var supportingDocTestHelper = GetSupportingDocTestHelper();
			foreach (var sd in supportingDocTestHelper.GetSupportingDocsForAggregationMergeTest())
			{
				invoiceLine.SupportingDocuments.Add(sd);
			}

			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			return entryLine;
		}

		public void TestSupportingDocumentsDifferentMergeKeySingleValue()
		{
			AssertSupportingDocumentsDifferentMergeKeySingleValue_ShouldFilterSupportingDocumentsByMergeKeys(true);
			AssertSupportingDocumentsDifferentMergeKeySingleValue_ShouldFilterSupportingDocumentsByMergeKeys(false);
		}

		public void AssertSupportingDocumentsDifferentMergeKeySingleValue_ShouldFilterSupportingDocumentsByMergeKeys(ZBool shouldFilterSupportingDocumentsByMergeKeys)
		{
			CombineAssertions($"ShouldFilterSupportingDocumentsByMergeKeys: {shouldFilterSupportingDocumentsByMergeKeys}", () =>
			{
				var entryLine1 = SetSupportingDocumentsOfEntryLineWithDifferentCSIReferenceNumber2(shouldFilterSupportingDocumentsByMergeKeys, "declaration", "invoiceLine", "invoice");
				var expectedDocumentCount = shouldFilterSupportingDocumentsByMergeKeys ? 2 : 3;
				AssertEquals("When invoice or invoice Lines' supporting document merge key duplicate with declaration level, the duplicate one shouldn't be added", expectedDocumentCount, entryLine1.SupportingDocuments.Count());

				var entryLine2 = SetSupportingDocumentsOfEntryLineWithDifferentCSIReferenceNumber2(shouldFilterSupportingDocumentsByMergeKeys, "declaration", "invoiceLine", "invoiceLine");
				expectedDocumentCount = shouldFilterSupportingDocumentsByMergeKeys ? 1 : 2;
				AssertEquals("When invoice or invoice Lines' supporting document merge duplicate with each other, the duplicate one shouldn't be added", expectedDocumentCount, entryLine2.SupportingDocuments.Count());
			});

			CusEntryLine SetSupportingDocumentsOfEntryLineWithDifferentCSIReferenceNumber2(ZBool shouldFilterSupportingDocumentsByMergeKeys, string suppDoc1_CSI_ReferenceNumber2, string suppDoc2_CSI_ReferenceNumber2, string suppDoc3_CSI_ReferenceNumber2)
			{
				SetSupportingDocumentsRefData();

				var declarationMock = Factory.NewMoq<JobDeclaration>();
				var declaration = declarationMock.Object;
				var entryCreationStrategyMock = new Mock<EntryCreationStrategy>(declarationMock.Object);
				declarationMock.Setup(d => d.CreateEntryCreationStrategy()).Returns(entryCreationStrategyMock.Object);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				entryCreationStrategyMock.Protected().Setup<string[]>("GetSupportingDocumentKeysCore").Returns(new string[] { SupportingDocument.Schema.CSI_ReferenceNumber2 });

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				var suppDoc1 = declaration.SupportingDocuments.AddNew();
				suppDoc1.CSI_Code = "1234";
				suppDoc1.CSI_ReferenceNumber2 = suppDoc1_CSI_ReferenceNumber2;

				var suppDoc2 = invoice.SupportingDocuments.AddNew();
				suppDoc2.CSI_Code = "1234";
				suppDoc2.CSI_ReferenceNumber2 = suppDoc2_CSI_ReferenceNumber2;

				var suppDoc3 = invoiceLine.SupportingDocuments.AddNew();
				suppDoc3.CSI_Code = "1234";
				suppDoc3.CSI_ReferenceNumber2 = suppDoc3_CSI_ReferenceNumber2;

				var suppDoc4 = invoiceLine.SupportingDocuments.AddNew();
				suppDoc4.CSI_Code = "1234";
				suppDoc4.CSI_ReferenceNumber2 = suppDoc1_CSI_ReferenceNumber2;

				var mockEntryLineConfiguration = new Mock<EntryLineConfiguration>();
				var entryLineConfiguration = mockEntryLineConfiguration.Object;
				mockEntryLineConfiguration.Protected().Setup<ZBool>("ShouldFilterSupportingDocumentsByMergeKeysCore").Returns(shouldFilterSupportingDocumentsByMergeKeys);

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "GetNewEntryLineConfiguration", entryLineConfiguration))
				{
					entryLine.MergeInvoiceLine(invoiceLine);
				}
				return entryLine;
			}
		}

		public void TestSupportingDocumentsMergeKeyMultipleValues()
		{
			CombineAssertions(() =>
			{
				var entryLine1 = SetSupportingDocumentsOfEntryLineWithDifferentCSIReferenceNumber2("declaration", "invoiceLine", "invoice");
				AssertEquals("When invoice or invoice Lines' supporting document merge key duplicate with declaration level, the duplicate one shouldn't be added", 2, entryLine1.SupportingDocuments.Count());

				var entryLine2 = SetSupportingDocumentsOfEntryLineWithDifferentCSIReferenceNumber2("declaration", "invoiceLine", "invoiceLine");
				AssertEquals("When invoice or invoice Lines' supporting document merge duplicate with each other, the duplicate one shouldn't be added", 1, entryLine2.SupportingDocuments.Count());
			});

			CusEntryLine SetSupportingDocumentsOfEntryLineWithDifferentCSIReferenceNumber2(string suppDoc1_CSI_ReferenceNumber, string suppDoc2_CSI_ReferenceNumber, string suppDoc3_CSI_ReferenceNumber)
			{
				SetSupportingDocumentsRefData();

				var declarationMock = Factory.NewMoq<JobDeclaration>();
				var declaration = declarationMock.Object;
				var entryCreationStrategyMock = new Mock<EntryCreationStrategy>(declarationMock.Object);
				declarationMock.Setup(d => d.CreateEntryCreationStrategy()).Returns(entryCreationStrategyMock.Object);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				entryCreationStrategyMock.Protected().Setup<string[]>("GetSupportingDocumentKeysCore").Returns(new string[] { SupportingDocument.Schema.CSI_ReferenceNumber, SupportingDocument.Schema.CSI_ReferenceNumber2 });

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				var suppDoc1 = declaration.SupportingDocuments.AddNew();
				suppDoc1.CSI_Code = "1234";
				suppDoc1.CSI_ReferenceNumber2 = "dejavu";
				suppDoc1.CSI_ReferenceNumber = suppDoc1_CSI_ReferenceNumber;

				var suppDoc2 = invoice.SupportingDocuments.AddNew();
				suppDoc2.CSI_Code = "1234";
				suppDoc1.CSI_ReferenceNumber2 = "dejavu";
				suppDoc2.CSI_ReferenceNumber = suppDoc2_CSI_ReferenceNumber;

				var suppDoc3 = invoiceLine.SupportingDocuments.AddNew();
				suppDoc3.CSI_Code = "1234";
				suppDoc1.CSI_ReferenceNumber2 = "dejavu";
				suppDoc3.CSI_ReferenceNumber = suppDoc3_CSI_ReferenceNumber;

				var suppDoc4 = declaration.SupportingDocuments.AddNew();
				suppDoc4.CSI_Code = "1234";
				suppDoc4.CSI_ReferenceNumber2 = "dejavu";
				suppDoc4.CSI_ReferenceNumber = suppDoc1_CSI_ReferenceNumber;

				entryLine.MergeInvoiceLine(invoiceLine);
				return entryLine;
			}
		}

		public void TestCusNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (CusEntryLine)entryHeader.MergedLines.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			invoiceLine1.ZG_CusNumber = "CUS1";
			AssertEquals("CusNumber", "CUS1", entryLine.CusNumber);
		}

		public void TestMergeInvLinesForHeaderContainsDifferentValuationIndicators()
		{
			var declaration = GetJobDeclarationForTest();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				var invHeader1 = declaration.Invoices.AddNew();
				invHeader1.RelatedIndicator = ZBool.True;
				var invLine1Header1 = invHeader1.InvoiceLines.AddNew();
				invLine1Header1.JI_NetWeight = 10;

				var invHeader2 = declaration.Invoices.AddNew();
				invHeader2.ZG_RelatedIndicator2 = "Y";
				var invLine1Header2 = invHeader2.InvoiceLines.AddNew();
				invLine1Header2.JI_NetWeight = 20;

				DoMerge(declaration);

				AssertEquals(1, declaration.CustomsEntryHeaders.Count);
				AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
				AssertEquals(30m, declaration.CustomsEntryHeaders[0].TotalNetWeightInKG);
			}
		}

		class CusEntryLineFeeForGuaranteeTest_EntryLineFeeMopDetailLevel : CusEntryLine
		{
			public CusEntryLineFeeForGuaranteeTest_EntryLineFeeMopDetailLevel(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override IEnumerable<ZString> GuaranteeDeferredMethodsOfPayment => new ZString[] { "B", "N" };

			public MoPLevel MopPaymentDetailsLevel_Exposed => MoPDetailsLevel;
		}

		class CusEntryLineFeeForGuaranteeTest_InvoiceLineMopDetailLevel : CusEntryLineFeeForGuaranteeTest_EntryLineFeeMopDetailLevel
		{
			public CusEntryLineFeeForGuaranteeTest_InvoiceLineMopDetailLevel(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override MoPLevel MoPDetailsLevel => MoPLevel.InvoiceLine;
		}

		class CusEntryLineFeeForGuaranteeTest_InvoiceLineTaxMopDetailLevel : CusEntryLineFeeForGuaranteeTest_EntryLineFeeMopDetailLevel
		{
			public CusEntryLineFeeForGuaranteeTest_InvoiceLineTaxMopDetailLevel(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override MoPLevel MoPDetailsLevel => MoPLevel.InvoiceLineTaxes;
		}

		class CusEntryLineFeeForGuaranteeTest_DeclarationMopDetailLevel : CusEntryLineFeeForGuaranteeTest_EntryLineFeeMopDetailLevel
		{
			public CusEntryLineFeeForGuaranteeTest_DeclarationMopDetailLevel(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override MoPLevel MoPDetailsLevel => MoPLevel.Declaration;
		}
	}
}
