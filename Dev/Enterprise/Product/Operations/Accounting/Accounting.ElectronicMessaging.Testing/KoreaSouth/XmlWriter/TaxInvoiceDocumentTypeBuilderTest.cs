using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Environment;
using WTG.TestHelpers.Xml;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class TaxInvoiceDocumentTypeBuilderTest : TestCaseWithFactory
	{
		#region BuildDescriptionText

		public void TestBuildDescriptionText_AllLines_WhenEvaluateResultIsEmpty()
		{
			var invoiceType = EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice;
			var collection = new KoreaSouthEInvoicingDataElementConfigurationCollection()
			{
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1, "[AAA: <AAA>]"),
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2, "[BBB: <BBB>]"),
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3, "[CCC: <CCC>]")
			};

			using (AccountingConfigurationRegistry.Instance.ElectronicInvoiceDataElementsConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					TransactionType = TransactionType.INV,
					TransactionDate = new ZDateTime(2022, 03, 04),
					Description = "AAAAAA",
				};
				SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

				var additionalInfo = new AdditionalInfo()
				{
					InvoiceeAlienRegistrationNo = "AlienNo01",
					InvoiceePassportNo = "PassportNo01",
					FullTypeCode = GetFullTypeCode(transactionInfo)
				};

				var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, additionalInfo);
				var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID></IssueID>
  <TypeCode>0102</TypeCode>
  <IssueDateTime>20220304</IssueDateTime>
  <PurposeCode>02</PurposeCode>
</TaxInvoiceDocumentType>";
				XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
			}
		}

		public void TestBuildDescriptionText_AllLines_ForOriginalInvoice()
		{
			var invoiceType = EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice;
			var collection = new KoreaSouthEInvoicingDataElementConfigurationCollection()
			{
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1, "[AAA: <InvoiceHeaderDescription>]"),
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2, "[BBB: <ForeignerRegistrationNumber>]"),
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3, "[CCC: <PassportNumber>]")
			};

			using (AccountingConfigurationRegistry.Instance.ElectronicInvoiceDataElementsConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					TransactionType = TransactionType.INV,
					TransactionDate = new ZDateTime(2022, 03, 04),
					Description = "AAAAAA",
				};
				SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

				var additionalInfo = new AdditionalInfo()
				{
					InvoiceeAlienRegistrationNo = "AlienNo01",
					InvoiceePassportNo = "PassportNo01",
					FullTypeCode = GetFullTypeCode(transactionInfo)
				};

				var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, additionalInfo);
				var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID></IssueID>
  <TypeCode>0102</TypeCode>
  <DescriptionText>AAA: AAAAAA</DescriptionText>
  <DescriptionText>BBB: AlienNo01</DescriptionText>
  <DescriptionText>CCC: PassportNo01</DescriptionText>
  <IssueDateTime>20220304</IssueDateTime>
  <PurposeCode>02</PurposeCode>
</TaxInvoiceDocumentType>";
				XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
			}
		}

		public void TestBuildDescriptionText_AllLines_ForAmendment()
		{
			var invoiceType = EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment;
			var collection = new KoreaSouthEInvoicingDataElementConfigurationCollection()
			{
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1, "[AAA: <InvoiceHeaderDescription>]"),
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2, "[BBB: <ForeignerRegistrationNumber>]"),
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3, "[CCC: <PassportNumber>]")
			};

			using (AccountingConfigurationRegistry.Instance.ElectronicInvoiceDataElementsConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					TransactionType = TransactionType.INV,
					TransactionDate = new ZDateTime(2022, 03, 04),
					Description = "AAAAAA",
					OriginalReference = new OriginalReference
					{
						OriginalTransactionNumber = "TEST001"
					}
				};
				SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

				var additionalInfo = new AdditionalInfo()
				{
					InvoiceeAlienRegistrationNo = "AlienNo01",
					InvoiceePassportNo = "PassportNo01",
					OriginalIssueID = "2022030412345678aAAAAAAA",
					AmendStatusCode = "01",
					FullTypeCode = GetFullTypeCode(transactionInfo)
				};

				var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, additionalInfo);
				var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID></IssueID>
  <TypeCode>0202</TypeCode>
  <DescriptionText>AAA: AAAAAA</DescriptionText>
  <DescriptionText>BBB: AlienNo01</DescriptionText>
  <DescriptionText>CCC: PassportNo01</DescriptionText>
  <IssueDateTime>20220304</IssueDateTime>
  <AmendmentStatusCode>01</AmendmentStatusCode>
  <PurposeCode>02</PurposeCode>
  <OriginalIssueID>2022030412345678aAAAAAAA</OriginalIssueID>
</TaxInvoiceDocumentType>";
				XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
			}
		}

		public void TestBuildDescriptionText_DefaultLine1_ForOriginalInvoice()
		{
			foreach (KoreaSouthEInvoicingDataElementConfiguration config in AccountingConfigurationRegistry.Instance.ElectronicInvoiceDataElementsConfiguration.Value)
			{
				if (config.InvoiceType == EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment && config.DataElement == EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1)
				{
					AssertEquals("Precondition", $"[당초승인번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.OriginalApprovalNumber)}>][/][당초작성일자: <{nameof(KoreaSouthEInvoicingDataElementProvider.OriginalApprovalDateForCode020304)}>]", config.Configuration);
				}
				else if (config.InvoiceType == EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice && config.DataElement == EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1)
				{
					AssertEquals("Precondition", $"[외국인등록번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.ForeignerRegistrationNumber)}>][/][여권번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.PassportNumber)}>]", config.Configuration);
				}
				else
				{
					AssertNullOrEmpty("Precondition", config.Configuration);
				}
			}

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
				TransactionDate = new ZDateTime(2022, 03, 04),
				Description = "AAAAAA",
			};
			SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

			AssertWithAmendStatusCode("<DescriptionText>외국인등록번호: AAA/여권번호: BBB</DescriptionText>");

			void AssertWithAmendStatusCode(string expectedDescriptionText)
			{
				var additionalInfo = new AdditionalInfo()
				{
					InvoiceeAlienRegistrationNo = "AAA",
					InvoiceePassportNo = "BBB",
					FullTypeCode = GetFullTypeCode(transactionInfo)
				};

				var expectedXmlResult = $@"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID></IssueID>
  <TypeCode>0102</TypeCode>
  {expectedDescriptionText}
  <IssueDateTime>20220304</IssueDateTime>
  <PurposeCode>02</PurposeCode>
</TaxInvoiceDocumentType>";
				var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, additionalInfo);
				XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
			}
		}

		public void TestBuildDescriptionText_DefaultLine1_ForAmendment()
		{
			foreach (KoreaSouthEInvoicingDataElementConfiguration config in AccountingConfigurationRegistry.Instance.ElectronicInvoiceDataElementsConfiguration.Value)
			{
				if (config.InvoiceType == EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment && config.DataElement == EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1)
				{
					AssertEquals("Precondition", $"[당초승인번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.OriginalApprovalNumber)}>][/][당초작성일자: <{nameof(KoreaSouthEInvoicingDataElementProvider.OriginalApprovalDateForCode020304)}>]", config.Configuration);
				}
				else if (config.InvoiceType == EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice && config.DataElement == EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1)
				{
					AssertEquals("Precondition", $"[외국인등록번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.ForeignerRegistrationNumber)}>][/][여권번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.PassportNumber)}>]", config.Configuration);
				}
				else
				{
					AssertNullOrEmpty("Precondition", config.Configuration);
				}
			}

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
				TransactionDate = new ZDateTime(2022, 03, 04),
				Description = "AAAAAA",
				OriginalReference = new OriginalReference
				{
					OriginalTransactionNumber = "TEST001"
				}
			};
			SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

			AssertWithAmendStatusCode("01", "<DescriptionText>당초승인번호: 2022030412345678aAAAAAAA</DescriptionText>");
			AssertWithAmendStatusCode("02", "<DescriptionText>당초승인번호: 2022030412345678aAAAAAAA/당초작성일자: 2022-03-04</DescriptionText>");
			AssertWithAmendStatusCode("03", "<DescriptionText>당초승인번호: 2022030412345678aAAAAAAA/당초작성일자: 2022-03-04</DescriptionText>");
			AssertWithAmendStatusCode("04", "<DescriptionText>당초승인번호: 2022030412345678aAAAAAAA/당초작성일자: 2022-03-04</DescriptionText>");
			AssertWithAmendStatusCode("05", "<DescriptionText>당초승인번호: 2022030412345678aAAAAAAA</DescriptionText>");
			AssertWithAmendStatusCode("06", "<DescriptionText>당초승인번호: 2022030412345678aAAAAAAA</DescriptionText>");

			void AssertWithAmendStatusCode(string amendStatusCode, string expectedDescriptionText)
			{
				var additionalInfo = new AdditionalInfo()
				{
					InvoiceeAlienRegistrationNo = "AlienNo01",
					InvoiceePassportNo = "PassportNo01",
					OriginalIssueID = "2022030412345678aAAAAAAA",
					AmendStatusCode = amendStatusCode,
					FullTypeCode = GetFullTypeCode(transactionInfo)
				};

				var expectedXmlResult = $@"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID></IssueID>
  <TypeCode>0202</TypeCode>
  {expectedDescriptionText}
  <IssueDateTime>20220304</IssueDateTime>
  <AmendmentStatusCode>{amendStatusCode}</AmendmentStatusCode>
  <PurposeCode>02</PurposeCode>
  <OriginalIssueID>2022030412345678aAAAAAAA</OriginalIssueID>
</TaxInvoiceDocumentType>";
				var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, additionalInfo);
				XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
			}
		}

		#endregion

		public void TestBuildXML()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LocalVATAmount = 0,
				TransactionType = TransactionType.INV,
				TransactionDate = new ZDateTime(2022, 03, 04),
				FullyPaidDate = null,
			};
			SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, GetMockAdditionalInfo(transactionInfo));
			var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID>2022030412345678abcdefgh</IssueID>
  <TypeCode>0102</TypeCode>
  <IssueDateTime>20220304</IssueDateTime>
  <PurposeCode>02</PurposeCode>
</TaxInvoiceDocumentType>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
		}

		public void TestBuildTypeCode_ForNullTaxRateType()
		{
			var registryItem = AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping;
			var newValueForRegistry = registryItem.DefaultValue.Clone(null, null) as CodeDescriptionWithGroupCollection;

			var taxType = NullTaxRateType.Code;

			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == taxType).Group = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValueForRegistry);
			AssertXML("<TypeCode></TypeCode>", TransactionInfoWithoutTaxAmount, taxType);
			AssertXML("<TypeCode></TypeCode>", TransactionInfoWithoutTaxAmount_Amendment, taxType);
			AssertXML("<TypeCode></TypeCode>", TransactionInfoWithTaxAmount, taxType);
			AssertXML("<TypeCode></TypeCode>", TransactionInfoWithTaxAmount_Amendment, taxType);

			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == taxType).Group = KoreaEInvoicingTypeCodeCategory.Invoice;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValueForRegistry);
			AssertXML("<TypeCode>0301</TypeCode>", TransactionInfoWithoutTaxAmount, taxType);
			AssertXML("<TypeCode>0401</TypeCode>", TransactionInfoWithoutTaxAmount_Amendment, taxType);
			AssertXML("<TypeCode>0301</TypeCode>", TransactionInfoWithTaxAmount, taxType);
			AssertXML("<TypeCode>0401</TypeCode>", TransactionInfoWithTaxAmount_Amendment, taxType);

			var expectedMessage = "Tax Invoice Document/Type Code can not be '01XX/02XX' when Code is '<Null>'.";
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == taxType).Group = KoreaEInvoicingTypeCodeCategory.TaxInvoice;
			AssertExceptionThrown<RegistryValidationException>(expectedMessage, () => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValueForRegistry));
		}

		public void TestBuildTypeCode_WhenAllGroupsAreTaxInvoiceExceptNullTaxRateType()
		{
			var expectedGroup = KoreaEInvoicingTypeCodeCategory.TaxInvoice;
			var registryItem = AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping;

			var newValueForRegistry = registryItem.DefaultValue.Clone(null, null) as CodeDescriptionWithGroupCollection;
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Where(x => x.Code != NullTaxRateType.Code).ForEach(x => x.Group = expectedGroup);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValueForRegistry);

			Assert($"Precondition, all groups are {expectedGroup} except {NullTaxRateType.Code}", registryItem.Value.Cast<CodeDescriptionWithGroup>().Where(x => x.Code != NullTaxRateType.Code).All(x => x.Group == expectedGroup));
			AssertEquals($"Group can not be '{expectedGroup}' when Code is '{NullTaxRateType.Code}'.", KoreaEInvoicingTypeCodeCategory.NotApplicable, registryItem.Value.GetGroupFromCode(NullTaxRateType.Code));

			foreach (var mapping in registryItem.Value.Cast<CodeDescriptionWithGroup>().Where(x => x.Code != NullTaxRateType.Code))
			{
				CombineAssertions($"When Tax Type is {mapping.Code} - {mapping.Description}", () =>
				{
					AssertXML("<TypeCode>0102</TypeCode>", TransactionInfoWithoutTaxAmount, mapping.Code);
					AssertXML("<TypeCode>0202</TypeCode>", TransactionInfoWithoutTaxAmount_Amendment, mapping.Code);
					AssertXML("<TypeCode>0101</TypeCode>", TransactionInfoWithTaxAmount, mapping.Code);
					AssertXML("<TypeCode>0201</TypeCode>", TransactionInfoWithTaxAmount_Amendment, mapping.Code);
				});
			}
		}

		public void TestBuildTypeCode_WhenAllGroupsAreNotApplicable()
		{
			var expectedGroup = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			var registryItem = AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping;

			var newValueForRegistry = registryItem.DefaultValue.Clone(null, null) as CodeDescriptionWithGroupCollection;
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().ForEach(x => x.Group = expectedGroup);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValueForRegistry);
			Assert($"Precondition, all groups are {expectedGroup}", registryItem.Value.Cast<CodeDescriptionWithGroup>().All(x => x.Group == expectedGroup));

			foreach (var mapping in registryItem.Value.Cast<CodeDescriptionWithGroup>())
			{
				CombineAssertions($"When Tax Type is {mapping.Code} - {mapping.Description}", () =>
				{
					AssertXML("<TypeCode></TypeCode>", TransactionInfoWithoutTaxAmount, mapping.Code);
					AssertXML("<TypeCode></TypeCode>", TransactionInfoWithoutTaxAmount_Amendment, mapping.Code);
					AssertXML("<TypeCode></TypeCode>", TransactionInfoWithTaxAmount, mapping.Code);
					AssertXML("<TypeCode></TypeCode>", TransactionInfoWithTaxAmount_Amendment, mapping.Code);
				});
			}
		}

		public void TestBuildTypeCode_WhenAllGroupsAreInvoice()
		{
			var expectedGroup = KoreaEInvoicingTypeCodeCategory.Invoice;
			var registryItem = AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping;

			var newValueForRegistry = registryItem.DefaultValue.Clone(null, null) as CodeDescriptionWithGroupCollection;
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().ForEach(x => x.Group = expectedGroup);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValueForRegistry);
			Assert($"Precondition, all groups are {expectedGroup}", registryItem.Value.Cast<CodeDescriptionWithGroup>().All(x => x.Group == expectedGroup));

			foreach (var mapping in registryItem.Value.Cast<CodeDescriptionWithGroup>())
			{
				CombineAssertions($"When Tax Type is {mapping.Code} - {mapping.Description}", () =>
				{
					AssertXML("<TypeCode>0301</TypeCode>", TransactionInfoWithoutTaxAmount, mapping.Code);
					AssertXML("<TypeCode>0401</TypeCode>", TransactionInfoWithoutTaxAmount_Amendment, mapping.Code);
					AssertXML("<TypeCode>0301</TypeCode>", TransactionInfoWithTaxAmount, mapping.Code);
					AssertXML("<TypeCode>0401</TypeCode>", TransactionInfoWithTaxAmount_Amendment, mapping.Code);
				});
			}
		}

		public void TestBuildTypeCode_AllTypeCodes()
		{
			var registryItemValue = AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping.Value.Cast<CodeDescriptionWithGroup>();

			AssertEquals("Precondition", 12, registryItemValue.Count());
			AssertEquals("Precondition", 9, registryItemValue.Count(x => x.Group == KoreaEInvoicingTypeCodeCategory.NotApplicable));
			AssertEquals("Precondition", KoreaEInvoicingTypeCodeCategory.TaxInvoice, registryItemValue.Single(x => x.Code == AccTaxRate.Types.CapitalRated).Group);
			AssertEquals("Precondition", KoreaEInvoicingTypeCodeCategory.TaxInvoice, registryItemValue.Single(x => x.Code == AccTaxRate.Types.Rated).Group);
			AssertEquals("Precondition", KoreaEInvoicingTypeCodeCategory.Invoice, registryItemValue.Single(x => x.Code == AccTaxRate.Types.Exempt).Group);

			CombineAssertions($"TypeCode should be empty when all Lines have Null Tax Type", () =>
			{
				AssertXML("<TypeCode></TypeCode>", TransactionInfoWithoutTaxAmount_Amendment);
				AssertXML("<TypeCode></TypeCode>", TransactionInfoWithoutTaxAmount);
				AssertXML("<TypeCode></TypeCode>", TransactionInfoWithoutTaxAmount_Amendment, NullTaxRateType.Code);
				AssertXML("<TypeCode></TypeCode>", TransactionInfoWithoutTaxAmount, NullTaxRateType.Code);
				AssertXML("<TypeCode></TypeCode>", TransactionInfoWithoutTaxAmount_Amendment, NullTaxRateType.Code, NullTaxRateType.Code);
				AssertXML("<TypeCode></TypeCode>", TransactionInfoWithoutTaxAmount, NullTaxRateType.Code, NullTaxRateType.Code);
			});

			CombineAssertions($"TypeCode should be 0401 when all Lines have a Tax Type that mapped to {KoreaEInvoicingTypeCodeCategory.Invoice}", () =>
			{
				var expectedXml = "<TypeCode>0401</TypeCode>";
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount_Amendment, AccTaxRate.Types.Exempt);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount_Amendment, AccTaxRate.Types.Exempt, NullTaxRateType.Code);
			});

			CombineAssertions($"TypeCode should be 0301 when all Lines have a Tax Type that mapped to {KoreaEInvoicingTypeCodeCategory.Invoice}", () =>
			{
				var expectedXml = "<TypeCode>0301</TypeCode>";
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount, AccTaxRate.Types.Exempt);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount, AccTaxRate.Types.Exempt, NullTaxRateType.Code);
			});

			CombineAssertions($"TypeCode should be 0202 when at least one Line has a Tax Type that mapped to {KoreaEInvoicingTypeCodeCategory.TaxInvoice}", () =>
			{
				var expectedXml = "<TypeCode>0202</TypeCode>";
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount_Amendment, AccTaxRate.Types.CapitalRated);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount_Amendment, AccTaxRate.Types.Rated);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount_Amendment, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount_Amendment, AccTaxRate.Types.CapitalRated, NullTaxRateType.Code);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount_Amendment, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated, AccTaxRate.Types.Exempt);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount_Amendment, AccTaxRate.Types.ExcludedFromTheTaxBase, AccTaxRate.Types.NotReportable, AccTaxRate.Types.CapitalRated);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount_Amendment, AccTaxRate.Types.ExcludedFromTheTaxBase, AccTaxRate.Types.NotReportable, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated, AccTaxRate.Types.Exempt);
			});

			CombineAssertions($"TypeCode should be 0102 when at least one Line has a Tax Type that mapped to {KoreaEInvoicingTypeCodeCategory.TaxInvoice}", () =>
			{
				var expectedXml = "<TypeCode>0102</TypeCode>";
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount, AccTaxRate.Types.CapitalRated);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount, AccTaxRate.Types.Rated);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount, AccTaxRate.Types.CapitalRated, NullTaxRateType.Code);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated, AccTaxRate.Types.Exempt);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount, AccTaxRate.Types.ExcludedFromTheTaxBase, AccTaxRate.Types.NotReportable, AccTaxRate.Types.CapitalRated);
				AssertXML(expectedXml, TransactionInfoWithoutTaxAmount, AccTaxRate.Types.ExcludedFromTheTaxBase, AccTaxRate.Types.NotReportable, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated, AccTaxRate.Types.Exempt);
			});

			CombineAssertions($"TypeCode should be 0201 when at least one Line has a Tax Type that mapped to {KoreaEInvoicingTypeCodeCategory.TaxInvoice}", () =>
			{
				var expectedXml = "<TypeCode>0201</TypeCode>";
				AssertXML(expectedXml, TransactionInfoWithTaxAmount_Amendment, AccTaxRate.Types.CapitalRated);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount_Amendment, AccTaxRate.Types.Rated);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount_Amendment, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount_Amendment, AccTaxRate.Types.CapitalRated, NullTaxRateType.Code);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount_Amendment, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated, AccTaxRate.Types.Exempt);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount_Amendment, AccTaxRate.Types.ExcludedFromTheTaxBase, AccTaxRate.Types.NotReportable, AccTaxRate.Types.CapitalRated);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount_Amendment, AccTaxRate.Types.ExcludedFromTheTaxBase, AccTaxRate.Types.NotReportable, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated, AccTaxRate.Types.Exempt);
			});

			CombineAssertions($"TypeCode should be 0101 when at least one Line has a Tax Type that mapped to {KoreaEInvoicingTypeCodeCategory.TaxInvoice}", () =>
			{
				var expectedXml = "<TypeCode>0101</TypeCode>";
				AssertXML(expectedXml, TransactionInfoWithTaxAmount, AccTaxRate.Types.CapitalRated);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount, AccTaxRate.Types.Rated);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount, AccTaxRate.Types.CapitalRated, NullTaxRateType.Code);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated, AccTaxRate.Types.Exempt);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount, AccTaxRate.Types.ExcludedFromTheTaxBase, AccTaxRate.Types.NotReportable, AccTaxRate.Types.CapitalRated);
				AssertXML(expectedXml, TransactionInfoWithTaxAmount, AccTaxRate.Types.ExcludedFromTheTaxBase, AccTaxRate.Types.NotReportable, AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated, AccTaxRate.Types.Exempt);
			});
		}

		void AssertXML(string expectedXml, TransactionInfo info, params string[] taxTypes)
		{
			SetPostingJournalCollection(info, taxTypes);

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", info, GetMockAdditionalInfo(info, "01", "2022030412345678aAAAAAAA"));
			var xml = builder.BuildXML("TaxInvoiceDocumentType").ToString();
			AssertContains(expectedXml, xml);
		}

		public void TestBuildTypeCode_WhenIsNotAmendment_AndTaxTypeIsNull()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.INV,
				TransactionDate = new ZDateTime(2022, 03, 04),
			};

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, GetMockAdditionalInfo(transactionInfo));
			var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID>2022030412345678abcdefgh</IssueID>
  <TypeCode></TypeCode>
  <IssueDateTime>20220304</IssueDateTime>
  <PurposeCode>02</PurposeCode>
</TaxInvoiceDocumentType>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
		}

		public void TestBuildTypeCode_WhenIsAmendment_AndTaxTypeIsNull()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.CRD,
				TransactionDate = new ZDateTime(2022, 03, 04),
				OriginalReference = new OriginalReference
				{
					OriginalTransactionNumber = "TEST001"
				}
			};

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, GetMockAdditionalInfo(transactionInfo, "01", "2022030412345678aAAAAAAA"));
			var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID>2022030412345678abcdefgh</IssueID>
  <TypeCode></TypeCode>
  <DescriptionText>당초승인번호: 2022030412345678aAAAAAAA</DescriptionText>
  <IssueDateTime>20220304</IssueDateTime>
  <AmendmentStatusCode>01</AmendmentStatusCode>
  <PurposeCode>02</PurposeCode>
  <OriginalIssueID>2022030412345678aAAAAAAA</OriginalIssueID>
</TaxInvoiceDocumentType>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
		}

		public void TestBuildXML_FullyPaid()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LocalVATAmount = 0,
				TransactionType = TransactionType.INV,
				TransactionDate = new ZDateTime(2022, 03, 04),
				FullyPaidDate = new ZDateTime(2022, 02, 01),
			};
			SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, GetMockAdditionalInfo(transactionInfo));
			var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID>2022030412345678abcdefgh</IssueID>
  <TypeCode>0102</TypeCode>
  <IssueDateTime>20220304</IssueDateTime>
  <PurposeCode>01</PurposeCode>
</TaxInvoiceDocumentType>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
		}

		public void TestBuildXML_TaxAmount()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LocalVATAmount = 1,
				TransactionType = TransactionType.INV,
				TransactionDate = new ZDateTime(2022, 03, 04),
				FullyPaidDate = null,
			};
			SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, GetMockAdditionalInfo(transactionInfo));
			var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID>2022030412345678abcdefgh</IssueID>
  <TypeCode>0101</TypeCode>
  <IssueDateTime>20220304</IssueDateTime>
  <PurposeCode>02</PurposeCode>
</TaxInvoiceDocumentType>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
		}

		public void TestBuildXML_AmendInvoice()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LocalVATAmount = 0,
				TransactionType = TransactionType.INV,
				TransactionDate = new ZDateTime(2022, 03, 04),
				FullyPaidDate = null,
				OriginalReference = new OriginalReference
				{
					OriginalTransactionNumber = "TEST001"
				}
			};
			SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, GetMockAdditionalInfo(transactionInfo, "01", "2022030412345678aAAAAAAA"));
			var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID>2022030412345678abcdefgh</IssueID>
  <TypeCode>0202</TypeCode>
  <DescriptionText>당초승인번호: 2022030412345678aAAAAAAA</DescriptionText>
  <IssueDateTime>20220304</IssueDateTime>
  <AmendmentStatusCode>01</AmendmentStatusCode>
  <PurposeCode>02</PurposeCode>
  <OriginalIssueID>2022030412345678aAAAAAAA</OriginalIssueID>
</TaxInvoiceDocumentType>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
		}

		public void TestBuildXML_AmendInvoice_WithTaxAmount()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LocalVATAmount = -1,
				TransactionType = TransactionType.INV,
				TransactionDate = new ZDateTime(2022, 03, 04),
				FullyPaidDate = null,
				OriginalReference = new OriginalReference
				{
					OriginalTransactionNumber = "TEST001"
				}
			};
			SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, GetMockAdditionalInfo(transactionInfo, "01", "2022030412345678aAAAAAAA"));
			var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID>2022030412345678abcdefgh</IssueID>
  <TypeCode>0201</TypeCode>
  <DescriptionText>당초승인번호: 2022030412345678aAAAAAAA</DescriptionText>
  <IssueDateTime>20220304</IssueDateTime>
  <AmendmentStatusCode>01</AmendmentStatusCode>
  <PurposeCode>02</PurposeCode>
  <OriginalIssueID>2022030412345678aAAAAAAA</OriginalIssueID>
</TaxInvoiceDocumentType>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
		}

		public void TestBuildXML_AmendCreditNote()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LocalVATAmount = 0,
				TransactionType = TransactionType.CRD,
				TransactionDate = new ZDateTime(2022, 03, 04),
				FullyPaidDate = null,
				OriginalReference = new OriginalReference
				{
					OriginalTransactionNumber = "TEST001"
				}
			};
			SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, GetMockAdditionalInfo(transactionInfo, "01", "2022030412345678aAAAAAAA"));
			var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID>2022030412345678abcdefgh</IssueID>
  <TypeCode>0202</TypeCode>
  <DescriptionText>당초승인번호: 2022030412345678aAAAAAAA</DescriptionText>
  <IssueDateTime>20220304</IssueDateTime>
  <AmendmentStatusCode>01</AmendmentStatusCode>
  <PurposeCode>02</PurposeCode>
  <OriginalIssueID>2022030412345678aAAAAAAA</OriginalIssueID>
</TaxInvoiceDocumentType>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
		}

		public void TestBuildXML_AmendCreditNote_NoAmendStatusCode()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionType = TransactionType.CRD,
				TransactionDate = new ZDateTime(2022, 03, 04),
				OriginalReference = new OriginalReference
				{
					OriginalTransactionNumber = "TEST001"
				}
			};
			SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

			var additionalInfo = GetMockAdditionalInfo(transactionInfo, "01", "2022030412345678aAAAAAAA");

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, additionalInfo);
			var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID>2022030412345678abcdefgh</IssueID>
  <TypeCode>0202</TypeCode>
  <DescriptionText>당초승인번호: 2022030412345678aAAAAAAA</DescriptionText>
  <IssueDateTime>20220304</IssueDateTime>
  <AmendmentStatusCode>01</AmendmentStatusCode>
  <PurposeCode>02</PurposeCode>
  <OriginalIssueID>2022030412345678aAAAAAAA</OriginalIssueID>
</TaxInvoiceDocumentType>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
		}

		public void TestBuildXML_AmendCreditNote_NoOriginalReference()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LocalVATAmount = 0,
				TransactionType = TransactionType.CRD,
				TransactionDate = new ZDateTime(2022, 03, 04),
				FullyPaidDate = null,
			};
			SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, GetMockAdditionalInfo(transactionInfo, "01", "2022030412345678aAAAAAAA"));
			var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID>2022030412345678abcdefgh</IssueID>
  <TypeCode>0102</TypeCode>
  <IssueDateTime>20220304</IssueDateTime>
  <AmendmentStatusCode>01</AmendmentStatusCode>
  <PurposeCode>02</PurposeCode>
</TaxInvoiceDocumentType>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
		}

		public void TestBuildXML_AmendCreditNote_WithTaxAmount()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LocalVATAmount = -1,
				TransactionType = TransactionType.CRD,
				TransactionDate = new ZDateTime(2022, 03, 04),
				FullyPaidDate = null,
				OriginalReference = new OriginalReference
				{
					OriginalTransactionNumber = "TEST001"
				}
			};
			SetPostingJournalCollection(transactionInfo, AccTaxRate.Types.CapitalRated);

			var builder = new TaxInvoiceDocumentTypeBuilder("TestNameSpace", transactionInfo, GetMockAdditionalInfo(transactionInfo, "01", "2022030412345678aAAAAAAA"));
			var expectedXmlResult = @"<TaxInvoiceDocumentType xmlns=""TestNameSpace"">
  <IssueID>2022030412345678abcdefgh</IssueID>
  <TypeCode>0201</TypeCode>
  <DescriptionText>당초승인번호: 2022030412345678aAAAAAAA</DescriptionText>
  <IssueDateTime>20220304</IssueDateTime>
  <AmendmentStatusCode>01</AmendmentStatusCode>
  <PurposeCode>02</PurposeCode>
  <OriginalIssueID>2022030412345678aAAAAAAA</OriginalIssueID>
</TaxInvoiceDocumentType>";
			XmlComparison.CompareAndAssertXml(expectedXmlResult, builder.BuildXML("TaxInvoiceDocumentType").ToString());
		}

		void SetPostingJournalCollection(TransactionInfo transactionInfo, params string[] taxTypes)
		{
			var postingJournals = new List<PostingJournal>();
			foreach (var taxType in taxTypes)
			{
				PostingJournal postingJournal;

				if (taxType == AccountingMasterFilesConstants.NullTaxRateType.Code)
				{
					postingJournal = new PostingJournal();
					AssertNull("Precondition", postingJournal.VATTaxID?.TaxType?.Code);
				}
				else
				{
					postingJournal = new PostingJournal { VATTaxID = new TaxID() { TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = taxType } } };
					AssertEquals("Precondition", taxType, postingJournal.VATTaxID?.TaxType?.Code);
				}

				postingJournals.Add(postingJournal);
			}

			transactionInfo.SetPostingJournalCollection(() => postingJournals);
		}

		AdditionalInfo GetMockAdditionalInfo(TransactionInfo transactionInfo, string amendStatusCode = null, string originalIssueID = null)
		{
			var additionalInfo = new AdditionalInfo()
			{
				IssueID = "2022030412345678abcdefgh",
				OriginalIssueID = originalIssueID,
				AmendStatusCode = amendStatusCode,
				FullTypeCode = GetFullTypeCode(transactionInfo)
			};

			return additionalInfo;
		}

		ZString GetFullTypeCode(TransactionInfo transactionInfo)
		{
			return KoreaSouthEInvoicingHelper.GetTaxInvoiceDocumentTypeCode(
					"",
					transactionInfo.PostingJournalCollection?.Select(x => x.VATTaxID?.TaxType?.Code).WhereNotNull() ?? Array.Empty<ZString>(),
					transactionInfo.IsAmendment,
					transactionInfo.LocalVATAmount);
		}

		TransactionInfo TransactionInfoWithoutTaxAmount => new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
		{
			LocalVATAmount = 0,
			TransactionType = TransactionType.INV,
			TransactionDate = new ZDateTime(2022, 03, 04),
		};

		TransactionInfo TransactionInfoWithoutTaxAmount_Amendment => new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
		{
			LocalVATAmount = 0,
			TransactionType = TransactionType.INV,
			TransactionDate = new ZDateTime(2022, 03, 04),
			OriginalReference = new OriginalReference
			{
				OriginalTransactionNumber = "TEST001"
			}
		};

		TransactionInfo TransactionInfoWithTaxAmount => new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
		{
			LocalVATAmount = 1,
			TransactionType = TransactionType.INV,
			TransactionDate = new ZDateTime(2022, 03, 04),
		};

		TransactionInfo TransactionInfoWithTaxAmount_Amendment => new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
		{
			LocalVATAmount = 1,
			TransactionType = TransactionType.INV,
			TransactionDate = new ZDateTime(2022, 03, 04),
			OriginalReference = new OriginalReference
			{
				OriginalTransactionNumber = "TEST001"
			}
		};
	}
}
