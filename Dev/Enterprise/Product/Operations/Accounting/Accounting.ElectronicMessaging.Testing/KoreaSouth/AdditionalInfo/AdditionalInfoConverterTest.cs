using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class AdditionalInfoConverterTest : TestCaseWithFactory
	{
		public void TestConvertTaxInvoiceAdditionalInfo_GetInvoiceLines_Amounts()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			var arLine = TestObjectCreator.CreateInvoiceLine(arInvoice, testObjectCreator.KRW, 1m, 10.1m);
			arLine.AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			Factory.Save();
			Factory.ReloadAll<ARInvoice>();
			Factory.ReloadAll<ARInvoiceLine>();

			var additionalInfo = GetAdditionalInfoFromARInvoice(arInvoice);

			AssertEquals("Pre-requisite", "10.1000", arLine.AL_LineAmount.ToString());
			AssertEquals("Pre-requisite", "1.0000", arLine.AL_GSTVAT.ToString());
			AssertEquals("Amount should be zero timmed.", "10.1", additionalInfo.Lines[0].InvoiceAmount);
			AssertEquals("Amount should be zero timmed.", "1", additionalInfo.Lines[0].CalculatedAmount);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_GetInvoiceLines_Sequence()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			var arLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, testObjectCreator.KRW, 1m, 10m);
			var arLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, testObjectCreator.KRW, 1m, 10m);
			arLine2.AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			arLine1.AL_Sequence = 100;
			arLine2.AL_Sequence = 101;
			var additionalInfo = GetAdditionalInfoFromARInvoice(arInvoice);
			AssertEquals("Sequence should be re-sequenced if any AL_Sequence is bigger than 99.", 1, additionalInfo.Lines[0].Sequence);
			AssertEquals("Sequence should be re-sequenced if any AL_Sequence is bigger than 99.", 2, additionalInfo.Lines[1].Sequence);

			arLine1.AL_Sequence = 99;
			arLine2.AL_Sequence = 100;
			additionalInfo = GetAdditionalInfoFromARInvoice(arInvoice);
			AssertEquals("Sequence should be re-sequenced if any AL_Sequence is bigger than 99.", 1, additionalInfo.Lines[0].Sequence);
			AssertEquals("Sequence should be re-sequenced if any AL_Sequence is bigger than 99.", 2, additionalInfo.Lines[1].Sequence);

			arLine1.AL_Sequence = 98;
			arLine2.AL_Sequence = 99;
			additionalInfo = GetAdditionalInfoFromARInvoice(arInvoice);
			AssertEquals("Sequence should be AL_Sequence if no AL_Sequence is bigger than 99.", 98, additionalInfo.Lines[0].Sequence);
			AssertEquals("Sequence should be AL_Sequence if no AL_Sequence is bigger than 99.", 99, additionalInfo.Lines[1].Sequence);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_FullTypeCode()
		{
			var registryItem = AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping;
			var newValueForRegistry = registryItem.DefaultValue.Clone(null, null) as CodeDescriptionWithGroupCollection;
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().First(x => x.Code == AccTaxRate.Types.ExcludedFromTheTaxBase).Group = KoreaEInvoicingTypeCodeCategory.Invoice;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValueForRegistry);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.ExcludedTax);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.ExcludedFromTheTaxBase));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var referenceKRI = Factory.New<AccTransactionHeaderReference>();
			referenceKRI.AH1_AH = arInvoice.PK;
			referenceKRI.AH1_Type = AccTransactionHeaderReferenceTypes.KRI;
			referenceKRI.AH1_Reference = "1234567812345678AAAAAAAA";

			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };

			var taxTypes = transactionInfo.PostingJournalCollection?.Select(x => x.VATTaxID?.TaxType?.Code).WhereNotNull();
			AssertEquals("Pre-requisite", 1, taxTypes.Count());
			AssertEquals("Pre-requisite", AccTaxRate.Types.ExcludedFromTheTaxBase, taxTypes.First());

			AssertEquals("Pre-requisite", false, transactionInfo.IsAmendment());

			var additionalInfo1 = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("0301", additionalInfo1.FullTypeCode);
			AssertEquals("0301", ((ITaxInvoiceDocumentTypeAdditionalInfo)additionalInfo1).FullTypeCode);

			transactionInfo.OriginalReference = new OriginalReference();
			transactionInfo.OriginalReference.OriginalTransactionNumber = "1234567890";
			AssertEquals("Pre-requisite", true, transactionInfo.IsAmendment());

			var additionalInfo2 = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("0401", additionalInfo2.FullTypeCode);
			AssertEquals("0401", ((ITaxInvoiceDocumentTypeAdditionalInfo)additionalInfo2).FullTypeCode);

			var mockIFeatureControlManager = new FeatureControlTestDataFactory().CreateKoreaSouthComplianceSubTypeFeatureControlMock();

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				arInvoice.AH_ComplianceSubType = "101";
				var additionalInfo3 = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
				AssertEquals("0101", additionalInfo3.FullTypeCode);
				AssertEquals("0101", ((ITaxInvoiceDocumentTypeAdditionalInfo)additionalInfo3).FullTypeCode);
			}
		}

		public void TestConvertTaxInvoiceAdditionalInfo_IssueID()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var referenceKRI = Factory.New<AccTransactionHeaderReference>();
			referenceKRI.AH1_AH = arInvoice.PK;
			referenceKRI.AH1_Type = AccTransactionHeaderReferenceTypes.KRI;
			referenceKRI.AH1_Reference = "1234567812345678AAAAAAAA";

			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);

			AssertEquals("1234567812345678AAAAAAAA", additionalInfo.IssueID);
			AssertEquals("1234567812345678AAAAAAAA", ((ITaxInvoiceDocumentTypeAdditionalInfo)additionalInfo).IssueID);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_OrigianlIssueID()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var amendInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			amendInvoice.AH_TransactionBelongsToGroup = arInvoice.PK;

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(amendInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var referenceKRI = Factory.New<AccTransactionHeaderReference>();
			referenceKRI.AH1_AH = arInvoice.PK;
			referenceKRI.AH1_Type = AccTransactionHeaderReferenceTypes.KRI;
			referenceKRI.AH1_Reference = "1234567812345678AAAAAAAA";

			var transactionInfo = CreateTransactionInfoWithAdditionalData(amendInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("1234567812345678AAAAAAAA", additionalInfo.OriginalIssueID);
			AssertEquals("1234567812345678AAAAAAAA", ((ITaxInvoiceDocumentTypeAdditionalInfo)additionalInfo).OriginalIssueID);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_AmendStatusCode()
		{
			var krBranch = TestObjectCreator.CreateBranchWithCompany("KR");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, krBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				arInvoice.AH_Calc_AmendStatusCode = "01";
				TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
				var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

				var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
				transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
				var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
				AssertEquals("01", additionalInfo.AmendStatusCode);
				AssertEquals("01", ((ITaxInvoiceDocumentTypeAdditionalInfo)additionalInfo).AmendStatusCode);
			}
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeAlienRegistrationNo()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
			transactionInfo.OrganizationAddress.OrganizationCode = TestObjectCreator.Debtor.OH_Code;
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> {
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner,
					},
					Value = "03_Value",
					CountryOfIssue = new Country { Code = CountryCodes.KoreaSouth }
				}
			});

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("03_Value", additionalInfo.InvoiceeAlienRegistrationNo);
			AssertEquals("03_Value", ((ITaxInvoiceDocumentTypeAdditionalInfo)additionalInfo).InvoiceeAlienRegistrationNo);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceePassportNo()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
			transactionInfo.OrganizationAddress.OrganizationCode = TestObjectCreator.Debtor.OH_Code;
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> {
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = OrgCusCode.CodeTypes.PassportID,
					},
					Value = "PAS_Value",
					CountryOfIssue = new Country { Code = CountryCodes.KoreaSouth }
				}
			});

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("PAS_Value", additionalInfo.InvoiceePassportNo);
			AssertEquals("PAS_Value", ((ITaxInvoiceDocumentTypeAdditionalInfo)additionalInfo).InvoiceePassportNo);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeBusinessTypeCodeCodeAndInvoiceeID()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
			transactionInfo.OrganizationAddress.OrganizationCode = TestObjectCreator.Debtor.OH_Code;

			var additionalInfoEmpty = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertResult("no CustomsCodes registered", additionalInfoEmpty, ZString.Empty, ZString.Empty);

			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> {
				new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner },
					Value = "DummyValue",
					CountryOfIssue = new Country { Code = CountryCodes.KoreaSouth }
				}
			});
			var additionalInfoForeigner = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertResult("Foreigner CustomsCodes is registered", additionalInfoForeigner, KoreaSouthComplianceInfo.ForeignerID, "03");

			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> {
				new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner },
					Value = "DummyValue",
					CountryOfIssue = new Country { Code = CountryCodes.KoreaSouth }
				}
				,new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident },
					Value = "Resident",
					CountryOfIssue = new Country { Code = CountryCodes.KoreaSouth }
				}
			});
			var additionalInfoResident = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertResult("Resident CustomsCodes is more priority than Foreigner", additionalInfoResident, "Resident", "02");

			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> {
				new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner },
					Value = "DummyValue",
					CountryOfIssue = new Country { Code = CountryCodes.KoreaSouth }
				}
				,new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident },
					Value = "Resident",
					CountryOfIssue = new Country { Code = CountryCodes.KoreaSouth }
				}
				,new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = OrgCusCode.CodeTypes.VATCode,
					},
					Value = "VATCode",
					CountryOfIssue = new Country { Code = CountryCodes.KoreaSouth }
				}
			});
			var additionalInfoVATCode = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertResult("VATCode CustomsCodes is more priority than Resident", additionalInfoVATCode, "VATCode", "01");

			void AssertResult(string comment, AdditionalInfo additionalInfo, string expectInvoiceeID, string expectedInvoiceeBusinessTypeCode)
			{
				AssertEquals(comment,expectInvoiceeID, additionalInfo.InvoiceeID);
				AssertEquals(comment,expectInvoiceeID, ((IInvoiceePartyAdditionalInfo)additionalInfo).ID);
				AssertEquals(comment,expectedInvoiceeBusinessTypeCode, additionalInfo.InvoiceeBusinessTypeCode);
				AssertEquals(comment,expectedInvoiceeBusinessTypeCode, ((IInvoiceePartyAdditionalInfo)additionalInfo).BusinessTypeCode);
			}
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeTypeCode()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
			transactionInfo.OrganizationAddress.OrganizationCode = TestObjectCreator.Debtor.OH_Code;
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> {
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = KoreaSouthComplianceInfo.CodeTypes.KBT,
					},
					Value = "KBT_Value",
					CountryOfIssue = new Country { Code = CountryCodes.KoreaSouth }
				}
			});

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("KBT_Value", additionalInfo.InvoiceeTypeCode);
			AssertEquals("KBT_Value", ((IInvoiceePartyAdditionalInfo)additionalInfo).TypeCode);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeClassificationCode()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
			transactionInfo.OrganizationAddress.OrganizationCode = TestObjectCreator.Debtor.OH_Code;
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> {
				new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = KoreaSouthComplianceInfo.CodeTypes.KBC },
					Value = "KBC",
					CountryOfIssue = new Country { Code = CountryCodes.KoreaSouth }
				}
			});

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("KBC", additionalInfo.InvoiceeClassificationCode);
			AssertEquals("KBC", ((IInvoiceePartyAdditionalInfo)additionalInfo).ClassificationCode);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeTaxRegistrationID()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
			transactionInfo.OrganizationAddress.OrganizationCode = TestObjectCreator.Debtor.OH_Code;
			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> {
				new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = KoreaSouthComplianceInfo.CodeTypes.OfficeID },
					Value = "GovBusinessCode",
					CountryOfIssue = new Country { Code = CountryCodes.KoreaSouth }
				}
			});

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("GovBusinessCode", additionalInfo.InvoiceeTaxRegistrationID);
			AssertEquals("GovBusinessCode", ((IInvoiceePartyAdditionalInfo)additionalInfo).TaxRegistrationID);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeNameText()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };

			var address = TestObjectCreator.Debtor.MainAddressCollection[0];
			address.CompanyName = "AAAAAAAAAAA";
			arInvoice.AH_OA_InvoiceAddressOverride = address.PK;

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("AAAAAAAAAAA", additionalInfo.InvoiceeNameText);
			AssertEquals("AAAAAAAAAAA", ((IInvoiceePartyAdditionalInfo)additionalInfo).NameText);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeSpecifiedPersonNameText()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
			transactionInfo.OrganizationAddress.OrganizationCode = TestObjectCreator.Debtor.OH_Code;

			var contact = TestObjectCreator.Debtor.Contacts.AddNew();
			contact.OC_ContactName = "KRC contact";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = ReadyKoreaConstants.KRC;

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("KRC contact", additionalInfo.InvoiceeSpecifiedPersonNameText);
			AssertEquals("KRC contact", ((IInvoiceePartyAdditionalInfo)additionalInfo).SpecifiedPersonNameText);

			var contactKRS = TestObjectCreator.Debtor.Contacts.AddNew();
			contactKRS.OC_ContactName = "KRS contact";
			var allocationKRS = contactKRS.Allocations.AddNew();
			allocationKRS.PC_Type = ReadyKoreaConstants.KRS;

			additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("KRC contact, KRS contact", additionalInfo.InvoiceeSpecifiedPersonNameText);
			AssertEquals("KRC contact, KRS contact", ((IInvoiceePartyAdditionalInfo)additionalInfo).SpecifiedPersonNameText);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeSpecifiedAddressLineOneText()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
			transactionInfo.OrganizationAddress.OrganizationCode = TestObjectCreator.Debtor.OH_Code;

			var address = TestObjectCreator.Debtor.MainAddressCollection[0];
			arInvoice.AH_OA_InvoiceAddressOverride = address.PK;

			address.PrimaryOrgAddressAdditionalInfoDetail = "Additional Address";
			address.OA_Address1 = "184 BOURKE ROADA";
			address.CompanyName = "TEST COMPANY NAMEA";
			address.City = "ACity";
			address.State = "BState";

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("ADDITIONAL ADDRESS\n184 BOURKE ROADA\nACITY BSTATE", additionalInfo.InvoiceeSpecifiedAddressLineOneText);
			AssertEquals("ADDITIONAL ADDRESS\n184 BOURKE ROADA\nACITY BSTATE", ((IInvoiceePartyAdditionalInfo)additionalInfo).SpecifiedAddressLineOneText);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact_OverrideContact()
		{
			var contact = CreateContact(TestObjectCreator.Debtor, "KRC contact", "789654", "17777@789.com");

			var expectedPrimaryDefinedContact = ("KRC contact", "789654", "17777@789.com");
			var expectedSecondaryDefinedContact = (ZString.Empty, ZString.Empty, ZString.Empty);
			AssertConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact(contact, ExportMultipleDebtorOrganizationContactEmailCodes.DEF, expectedPrimaryDefinedContact, expectedSecondaryDefinedContact);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact_NotOverrideContact()
		{
			var contact = CreateContact(TestObjectCreator.Debtor, "KRC contact", "789654", "17777@789.com");

			var expectedPrimaryDefinedContact = (ZString.Empty, ZString.Empty, ZString.Empty);
			var expectedSecondaryDefinedContact = (ZString.Empty, ZString.Empty, ZString.Empty);
			AssertConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact(null, ExportMultipleDebtorOrganizationContactEmailCodes.DEF, expectedPrimaryDefinedContact, expectedSecondaryDefinedContact);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact_WhenMultiARContacts_OverrideContact()
		{
			var overriddenContact = CreateContact(TestObjectCreator.Debtor, "Overridden Contact", "1111111", "1111111@111.com");

			var officialAREmailContact = CreateContact(TestObjectCreator.Debtor, "Official AR Email Contact", "2222222", "2222222@222.com");
			CreateContactDocument(officialAREmailContact, ContactType.Receivables.Code, Core.Constants.ContactNotifyModes.Email, true);

			var otherContact1 = CreateContact(TestObjectCreator.Debtor, "Other Contact 1", "3333333", "3333333@333.com");
			CreateContactDocument(otherContact1, ContactType.Payables.Code, Core.Constants.ContactNotifyModes.Email, true);

			var otherContact2 = CreateContact(TestObjectCreator.Debtor, "Other Contact 2", "4444444", "4444444@444.com");
			CreateContactDocument(otherContact2, ContactType.Receivables.Code, Core.Constants.ContactNotifyModes.Print, true);

			var otherContact3 = CreateContact(TestObjectCreator.Debtor, "Other Contact 3", "5555555", "5555555@555.com");
			CreateContactDocument(otherContact2, ContactType.Receivables.Code, Core.Constants.ContactNotifyModes.Email, false);

			var expectedPrimaryDefinedContact = ("Overridden Contact", "1111111", "1111111@111.com");
			var expectedSecondaryDefinedContact = (ZString.Empty, ZString.Empty, ZString.Empty);
			AssertConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact(overriddenContact, ExportMultipleDebtorOrganizationContactEmailCodes.MAR, expectedPrimaryDefinedContact, expectedSecondaryDefinedContact);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact_WhenMultiARContacts_HasOfficialAndUnofficialAREmailContacts()
		{
			var officialAREmailContact = CreateContact(TestObjectCreator.Debtor, "Official AR Email Contact", "2222222", "2222222@222.com");
			CreateContactDocument(officialAREmailContact, ContactType.Receivables.Code, Core.Constants.ContactNotifyModes.Email, true);

			var unofficialAREmailContact = CreateContact(TestObjectCreator.Debtor, "Unofficial AR Email Contact", "3333333", "3333333@333.com");
			CreateContactDocument(unofficialAREmailContact, ContactType.Receivables.Code, Core.Constants.ContactNotifyModes.Email, false);

			var otherContact1 = CreateContact(TestObjectCreator.Debtor, "Other Contact 1", "4444444", "4444444@444.com");
			CreateContactDocument(otherContact1, ContactType.Payables.Code, Core.Constants.ContactNotifyModes.Email, true);

			var otherContact2 = CreateContact(TestObjectCreator.Debtor, "Other Contact 2", "5555555", "5555555@555.com");
			CreateContactDocument(otherContact2, ContactType.Receivables.Code, Core.Constants.ContactNotifyModes.Print, true);

			var expectedPrimaryDefinedContact = ("Official AR Email Contact", "2222222", "2222222@222.com");
			var expectedSecondaryDefinedContact = ("Unofficial AR Email Contact", "3333333", "3333333@333.com");
			AssertConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact(null, ExportMultipleDebtorOrganizationContactEmailCodes.MAR, expectedPrimaryDefinedContact, expectedSecondaryDefinedContact);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact_WhenMultiARContacts_HasOnlyOfficialAREmailContact()
		{
			var officialAREmailContact = CreateContact(TestObjectCreator.Debtor, "Official AR Email Contact", "2222222", "2222222@222.com");
			CreateContactDocument(officialAREmailContact, ContactType.Receivables.Code, Core.Constants.ContactNotifyModes.Email, true);

			var otherContact1 = CreateContact(TestObjectCreator.Debtor, "Other Contact 1", "4444444", "4444444@444.com");
			CreateContactDocument(otherContact1, ContactType.Payables.Code, Core.Constants.ContactNotifyModes.Email, true);

			var otherContact2 = CreateContact(TestObjectCreator.Debtor, "Other Contact 2", "5555555", "5555555@555.com");
			CreateContactDocument(otherContact2, ContactType.Receivables.Code, Core.Constants.ContactNotifyModes.Print, true);

			var expectedPrimaryDefinedContact = ("Official AR Email Contact", "2222222", "2222222@222.com");
			var expectedSecondaryDefinedContact = (ZString.Empty, ZString.Empty, ZString.Empty);
			AssertConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact(null, ExportMultipleDebtorOrganizationContactEmailCodes.MAR, expectedPrimaryDefinedContact, expectedSecondaryDefinedContact);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact_WhenMultiARContacts_HasOnlyUnofficialAREmailContact()
		{
			var unofficialAREmailContact = CreateContact(TestObjectCreator.Debtor, "Unofficial AR Email Contact", "3333333", "3333333@333.com");
			CreateContactDocument(unofficialAREmailContact, ContactType.Receivables.Code, Core.Constants.ContactNotifyModes.Email, false);

			var otherContact1 = CreateContact(TestObjectCreator.Debtor, "Other Contact 1", "4444444", "4444444@444.com");
			CreateContactDocument(otherContact1, ContactType.Payables.Code, Core.Constants.ContactNotifyModes.Email, true);

			var otherContact2 = CreateContact(TestObjectCreator.Debtor, "Other Contact 2", "5555555", "5555555@555.com");
			CreateContactDocument(otherContact2, ContactType.Receivables.Code, Core.Constants.ContactNotifyModes.Print, true);

			var expectedPrimaryDefinedContact = (ZString.Empty, ZString.Empty, ZString.Empty);
			var expectedSecondaryDefinedContact = (ZString.Empty, ZString.Empty, ZString.Empty);
			AssertConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact(null, ExportMultipleDebtorOrganizationContactEmailCodes.MAR, expectedPrimaryDefinedContact, expectedSecondaryDefinedContact);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact_WhenMultiARContacts_OtherContacts()
		{
			var otherContact1 = CreateContact(TestObjectCreator.Debtor, "Other Contact 1", "4444444", "4444444@444.com");
			CreateContactDocument(otherContact1, ContactType.Payables.Code, Core.Constants.ContactNotifyModes.Email, true);

			var otherContact2 = CreateContact(TestObjectCreator.Debtor, "Other Contact 2", "5555555", "5555555@555.com");
			CreateContactDocument(otherContact2, ContactType.Receivables.Code, Core.Constants.ContactNotifyModes.Print, true);

			var expectedPrimaryDefinedContact = (ZString.Empty, ZString.Empty, ZString.Empty);
			var expectedSecondaryDefinedContact = (ZString.Empty, ZString.Empty, ZString.Empty);
			AssertConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact(null, ExportMultipleDebtorOrganizationContactEmailCodes.MAR, expectedPrimaryDefinedContact, expectedSecondaryDefinedContact);
		}

		void AssertConvertTaxInvoiceAdditionalInfo_InvoiceeDefinedContact(OrgContact overriddenContact, string multiEmailCode, (string Name, string Phone, string Email) expectedPrimaryDefinedContact, (string Name, string Phone, string Email) expectedSecondaryDefinedContact)
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
			transactionInfo.OrganizationAddress.OrganizationCode = TestObjectCreator.Debtor.OH_Code;

			var address = TestObjectCreator.Debtor.MainAddressCollection[0];
			address.OA_Phone = "123456";
			address.OA_Email = "123456@789.com";

			arInvoice.AH_OA_InvoiceAddressOverride = address.PK;
			if (overriddenContact != null)
			{
				arInvoice.AH_OC_InvoiceContactOverride = overriddenContact.PK;
			}

			using (AccountingMasterFilesRegistry.Instance.ExportMultipleDebtorOrganizationContactEmail.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, multiEmailCode))
			{
				var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
				AssertEquals(expectedPrimaryDefinedContact.Name, additionalInfo.InvoiceePrimaryDefinedContactPersonName);
				AssertEquals(expectedPrimaryDefinedContact.Name, ((IInvoiceePartyAdditionalInfo)additionalInfo).PrimaryDefinedContactPersonName);
				AssertEquals(expectedPrimaryDefinedContact.Phone, additionalInfo.InvoiceePrimaryDefinedContactTel);
				AssertEquals(expectedPrimaryDefinedContact.Phone, ((IInvoiceePartyAdditionalInfo)additionalInfo).PrimaryDefinedContactTel);
				AssertEquals(expectedPrimaryDefinedContact.Email, additionalInfo.InvoiceePrimaryDefinedContactURICommunication);
				AssertEquals(expectedPrimaryDefinedContact.Email, ((IInvoiceePartyAdditionalInfo)additionalInfo).PrimaryDefinedContactURICommunication);

				AssertEquals(expectedSecondaryDefinedContact.Name, additionalInfo.InvoiceeSecondaryDefinedContactPersonName);
				AssertEquals(expectedSecondaryDefinedContact.Name, ((IInvoiceePartyAdditionalInfo)additionalInfo).SecondaryDefinedContactPersonName);
				AssertEquals(expectedSecondaryDefinedContact.Phone, additionalInfo.InvoiceeSecondaryDefinedContactTel);
				AssertEquals(expectedSecondaryDefinedContact.Phone, ((IInvoiceePartyAdditionalInfo)additionalInfo).SecondaryDefinedContactTel);
				AssertEquals(expectedSecondaryDefinedContact.Email, additionalInfo.InvoiceeSecondaryDefinedContactURICommunication);
				AssertEquals(expectedSecondaryDefinedContact.Email, ((IInvoiceePartyAdditionalInfo)additionalInfo).SecondaryDefinedContactURICommunication);
			}
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoicerBusinessTypeCodeCodeAndInvoicerID()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };

			var additionalInfoEmpty = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertResult("no CustomsCodes registered", additionalInfoEmpty, ZString.Empty);

			currentCompany.OrgProxy.CustomsCodes.AddNew(KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner, "Foreigner", CountryCodes.KoreaSouth);
			var additionalInfoForeigner = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertResult("Foreigner CustomsCodes is not used for here", additionalInfoForeigner, ZString.Empty);

			currentCompany.OrgProxy.CustomsCodes.AddNew(KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident, "Resident", CountryCodes.KoreaSouth);
			var additionalInfoResident = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertResult("Resident CustomsCodes is not used for here", additionalInfoResident, ZString.Empty);

			currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VATCode", CountryCodes.KoreaSouth);
			var additionalInfoVATCode = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertResult("VATCode CustomsCodes is used here", additionalInfoVATCode, "VATCode");

			void AssertResult(string comment, AdditionalInfo additionalInfo, string expectInvoicerID)
			{
				AssertEquals(comment, expectInvoicerID, additionalInfo.InvoicerID);
				AssertEquals(comment, expectInvoicerID, ((IInvoicerPartyAdditionalInfo)additionalInfo).ID);
			}
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoicerTypeCode()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			currentCompany.OrgProxy.CustomsCodes.AddNew(KoreaSouthComplianceInfo.CodeTypes.KBT, "KBT", CountryCodes.KoreaSouth);

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("KBT", additionalInfo.InvoicerTypeCode);
			AssertEquals("KBT", ((IInvoicerPartyAdditionalInfo)additionalInfo).TypeCode);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoicerClassificationCode()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			currentCompany.OrgProxy.CustomsCodes.AddNew(KoreaSouthComplianceInfo.CodeTypes.KBC, "KBC", CountryCodes.KoreaSouth);

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("KBC", additionalInfo.InvoicerClassificationCode);
			AssertEquals("KBC", ((IInvoicerPartyAdditionalInfo)additionalInfo).ClassificationCode);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoicerTaxRegistrationID()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			currentCompany.OrgProxy.CustomsCodes.AddNew(KoreaSouthComplianceInfo.CodeTypes.OfficeID, "GovBusinessCode", CountryCodes.KoreaSouth);

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("GovBusinessCode", additionalInfo.InvoicerTaxRegistrationID);
			AssertEquals("GovBusinessCode", ((IInvoicerPartyAdditionalInfo)additionalInfo).TaxRegistrationID);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoicerNameText()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };

			currentCompany.OrgProxy.OH_FullName = "AAAAAAAAAAA";

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("AAAAAAAAAAA", additionalInfo.InvoicerNameText);
			AssertEquals("AAAAAAAAAAA", ((IInvoicerPartyAdditionalInfo)additionalInfo).NameText);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoicerSpecifiedPersonNameText()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };

			var contact = currentCompany.OrgProxy.Contacts.AddNew();
			contact.OC_ContactName = "KRC contact";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = ReadyKoreaConstants.KRC;

			var contactKRS = currentCompany.OrgProxy.Contacts.AddNew();
			contactKRS.OC_ContactName = "KRS contact";
			var allocationKRS = contactKRS.Allocations.AddNew();
			allocationKRS.PC_Type = ReadyKoreaConstants.KRS;

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("KRC contact", additionalInfo.InvoicerSpecifiedPersonNameText);
			AssertEquals("KRC contact", ((IInvoicerPartyAdditionalInfo)additionalInfo).SpecifiedPersonNameText);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoicerSpecifiedAddressLineOneText()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };

			currentCompany.OrgProxy.OH_FullName = "OH_FULLNAMEAAA";
			var address = currentCompany.OrgProxy.MainAddressCollection[0];
			address.CompanyName = "TEST COMPANY NAMEA";
			address.PrimaryOrgAddressAdditionalInfoDetail = "Additional Address";
			address.OA_Address1 = "11184OA_Address1";
			address.OA_Address2 = "33347OA_Address2";
			address.Postcode = "1111";
			address.City = "CityA";
			address.State = "StateB";

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("ADDITIONAL ADDRESS\n11184OA_ADDRESS1\n33347OA_ADDRESS2\nCITYA STATEB 1111", additionalInfo.InvoicerSpecifiedAddressLineOneText);
			AssertEquals("ADDITIONAL ADDRESS\n11184OA_ADDRESS1\n33347OA_ADDRESS2\nCITYA STATEB 1111", ((IInvoicerPartyAdditionalInfo)additionalInfo).SpecifiedAddressLineOneText);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoicerDefinedContact()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			transactionInfo.CreateUser = new StaffUsingAttributes { Code = GlbStaff.CurrentUser.GS_Code };

			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_FullName = "KRC contact";
			currentUser.GS_WorkPhone = "123456";
			currentUser.GS_EmailAddress = "123456@789.com";

			var additionalInfo = Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
			AssertEquals("KRC contact", additionalInfo.InvoicerDefinedContactPersonName);
			AssertEquals("KRC contact", ((IInvoicerPartyAdditionalInfo)additionalInfo).DefinedContactPersonName);
			AssertEquals("123456", additionalInfo.InvoicerDefinedContactTel);
			AssertEquals("123456", ((IInvoicerPartyAdditionalInfo)additionalInfo).DefinedContactTel);
			AssertEquals("123456@789.com", additionalInfo.InvoicerDefinedContactURICommunication);
			AssertEquals("123456@789.com", ((IInvoicerPartyAdditionalInfo)additionalInfo).DefinedContactURICommunication);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_NullBranch()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("12345670", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = CreateTransactionInfoWithAdditionalData(arInvoice);
			transactionInfo.Branch = null;
			AssertExceptionThrown<ArgumentException>("Could not determine the transaction branch"
				, () => Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch)
			);
		}

		public void TestConvertTaxInvoiceAdditionalInfo_InvoiceNotFound()
		{
			var pivot = Factory.New<AccEInvoicingTransactionPivot>();
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);
			var transactionInfo = new TransactionInfo();
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			AssertExceptionThrown<InvalidOperationException>("Could not find the transaction"
				, () => Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch)
			);
		}

		AdditionalInfo GetAdditionalInfoFromARInvoice(ARInvoice invoice)
		{
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Sent);

			var transactionInfo = CreateTransactionInfoWithAdditionalData(invoice);
			transactionInfo.Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code };
			return Converter.ConvertTaxInvoiceAdditionalInfo(transactionInfo, batch);
		}

		TransactionInfo CreateTransactionInfoWithAdditionalData(InvoicingBase invoice)
		{
			var result = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			result.ComplianceSubType = invoice.PK.ToString();

			var journals = invoice.Lines.Cast<AccTransactionLines>()
				.Select(x => new PostingJournal()
				{
					VATTaxID = x.TaxRate != null
						? new TaxID() { TaxCode = x.TaxRate.AT_Code, TaxType = new CodeDescriptionPair { Code = x.TaxRate.AT_Type } }
						: null
				})
				.ToList();
			result.SetPostingJournalCollection(() => journals);
			return result;
		}

		OrgContact CreateContact(OrgHeader org, ZString contactName, ZString phone, ZString email)
		{
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			contact.OC_Phone = phone;
			contact.OC_Email = email;

			return contact;
		}

		OrgDocument CreateContactDocument(OrgContact contact, ZString documentGroup, ZString deliverBy, bool isDefaultContact)
		{
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = documentGroup;
			document.OD_DeliverBy = deliverBy;
			document.OD_DefaultContact = isDefaultContact;

			return document;
		}

		AdditionalInfoConverter Converter { get; } = new AdditionalInfoConverter();

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
