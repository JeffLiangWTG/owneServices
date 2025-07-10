using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ComplianceSubTypeRuleTest : TestCaseWithFactory
	{
		readonly List<(string TaxInvoiceRule, string SubType)> complianceSubTypeRulesWithPriorityDesc = new List<(string TaxInvoiceRuleCodes, string SubType)>()
			{
				(TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly,"S0"),
				(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT,"S1"),
				(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID,"S2"),
				(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax,"S3"),
				(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax,"S4"),
				(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs,"S5"),
				(TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly,"S6"),
				(TaxInvoiceRuleCodes.ContainsAnAmountOfTax,"S7"),
				(TaxInvoiceRuleCodes.ContainsNoTaxIDs,"S8"),
				(TaxInvoiceRuleCodes.All,"S9"),
			};

		readonly List<string> AllAccTaxRateTypes = new List<string>()
			{
				AccTaxRate.Types.ExcludedFromTheTaxBase,
				AccTaxRate.Types.Rated,
				AccTaxRate.Types.ReverseRated,
				AccTaxRate.Types.Exempt,
				AccTaxRate.Types.CapitalRated,
				AccTaxRate.Types.NotReportable,
				AccTaxRate.Types.Suspended,
				AccTaxRate.Types.RatedInAnotherCountry,
				AccTaxRate.Types.ReportableUnderBusinessTax,
				AccTaxRate.Types.IntegratedGST,
				AccTaxRate.Types.ServiceTax,
			};

		public void TestFindMatchingComplianceSubTypeRuleForEmptyTaxInvoiceRule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertFindMatchingComplianceSubTypeRuleByTaxType(ZString.Empty, new List<(string, string)>() { (ZString.Empty, ZString.Empty) }, AllAccTaxRateTypes, 0, Constants.CountryCodes.Italy, true);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				AssertFindMatchingComplianceSubTypeRuleByTaxType(ZString.Empty, new List<(string, string)>() { (ZString.Empty, ZString.Empty) }, AllAccTaxRateTypes, 0, shouldRemoveAllLines: true);
			}
		}

		public void TestAllCommentLinesWithUnsupportedTaxInvoiceRule()
		{
			AddCommentChargeLine();
			AssertFindMatchingComplianceSubTypeRuleByTaxType(ZString.Empty, new List<(string, string)>() { ("XYZ", ZString.Empty) }, new List<string>(), 0, shouldRemoveAllLines: true);
		}

		public void TestFindMatchingComplianceSubTypeRuleForUnsupportedTaxInvoiceRule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				try
				{
					AssertFindMatchingComplianceSubTypeRuleByTaxType(null, new List<(string, string)>() { ("XYZ", ZString.Empty) }, AllAccTaxRateTypes, 0, shouldRemoveAllLines: true);
					Fail("Should not be executed as the above line is supposed to generate an exception");
				}
				catch (NotSupportedException e)
				{
					AssertEquals("Exception message", "Tax Invoice Rule 'XYZ' is not supported", e.Message);
				}

				AssertFindMatchingComplianceSubTypeRuleByTaxType(ZString.Empty, new List<(string, string)>() { (ZString.Empty, ZString.Empty) }, AllAccTaxRateTypes, 0, shouldRemoveAllLines: true);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForAll()
		{
			var expectedSubType = complianceSubTypeRulesWithPriorityDesc[9].SubType;
			var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.Skip(8).ToList();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				AssertFindMatchingComplianceSubTypeRuleByTaxType(ZString.Empty, taxInvoiceRules, new List<string>(), 0, shouldRemoveAllLines: true);

				AddCommentChargeLine();
				AssertFindMatchingComplianceSubTypeRuleByTaxType(ZString.Empty, taxInvoiceRules, new List<string>(), 0, shouldRemoveAllLines: false);

				var invoicingLine = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				invoicingLine.AL_AC = ZGuid.Empty;
				invoicingLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, new List<string>(), 0, shouldRemoveAllLines: false);

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, AllAccTaxRateTypes, 0, shouldRemoveAllLines: true);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertFindMatchingComplianceSubTypeRuleByTaxType(ZString.Empty, taxInvoiceRules, new List<string>(), 0, Constants.CountryCodes.Italy, true);

				AddCommentChargeLine();
				AssertFindMatchingComplianceSubTypeRuleByTaxType(ZString.Empty, taxInvoiceRules, new List<string>(), 0, Constants.CountryCodes.Italy, false);

				var invoicingLine = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				invoicingLine.AL_AC = ZGuid.Empty;
				invoicingLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, new List<string>(), 0, Constants.CountryCodes.Italy, false);

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, AllAccTaxRateTypes, 0, Constants.CountryCodes.Italy, true);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForContainsNoIDs()
		{
			var expectedSubType = complianceSubTypeRulesWithPriorityDesc[8].SubType;
			var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.Skip(8).ToList();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var invoicingLine = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
				invoicingLine.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
				invoicingLine.AL_AT = ZGuid.Empty;
				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, new List<string>(), 0, shouldRemoveAllLines: false);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForContainsExcludeChargeTaxIDsOnly()
		{
			var expectedSubType = complianceSubTypeRulesWithPriorityDesc[0].SubType;
			var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.GetRange(0, 8).ToList();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				AddCommentChargeLine();
				AssertFindMatchingComplianceSubTypeRuleByTaxType(ZString.Empty, taxInvoiceRules, new List<string>(), 0, shouldRemoveAllLines: false);

				var accTaxRateTypes = new List<string>()
					{
						AccTaxRate.Types.ExcludedFromTheTaxBase
					};

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0);
			}
		}

		void AddCommentChargeLine()
		{
			var invoicingLine = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			AccChargeCode cmtCharge = Factory.NewWithValidTestData<AccChargeCode>();
			cmtCharge.AC_ChargeType = Constants.ChargeType.Comment;
			invoicingLine.AL_AC = cmtCharge.PK;
		}

		public void TestFindMatchingComplianceSubTypeRuleForContainsAtLeastOneTaxIDExcludingNOT()
		{
			var expectedSubType = complianceSubTypeRulesWithPriorityDesc[1].SubType;
			var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.GetRange(1, 8).ToList();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var accTaxRateTypes = new List<string>(AllAccTaxRateTypes);

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0);
				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 5);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForContainsAtLeastOneTaxID()
		{
			var expectedSubType = complianceSubTypeRulesWithPriorityDesc[2].SubType;
			var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.Skip(2).ToList();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var accTaxRateTypes = new List<string>(AllAccTaxRateTypes);

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0);
				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 5);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForContainsAtLeastOneTaxIDAndNoAmountOfTax()
		{
			var expectedSubType = complianceSubTypeRulesWithPriorityDesc[3].SubType;
			var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.Skip(3).ToList();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var accTaxRateTypes = new List<string>(AllAccTaxRateTypes);

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForContainsAtLeastOneTaxIDAndAmountOfTax()
		{
			var expectedSubType = complianceSubTypeRulesWithPriorityDesc[4].SubType;
			var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.Skip(4).ToList();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var accTaxRateTypes = new List<string>(AllAccTaxRateTypes);

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 5);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForContainsAtLeastOneTaxIDExcludingRVS()
		{
			var expectedSubType = complianceSubTypeRulesWithPriorityDesc[5].SubType;
			var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.Skip(5).ToList();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var accTaxRateTypes = new List<string>(AllAccTaxRateTypes);

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0);
				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 5);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForAllLinesContainReverseChargeTaxIDs()
		{
			var expectedSubType = complianceSubTypeRulesWithPriorityDesc[6].SubType;
			var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.Skip(6).ToList();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var accTaxRateTypes = new List<string>()
					{
						AccTaxRate.Types.ReverseRated
					};

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForContainsAnAmountOfTax()
		{
			var expectedSubType = complianceSubTypeRulesWithPriorityDesc[7].SubType;
			var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.Skip(7).ToList();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var accTaxRateTypes = new List<string>(AllAccTaxRateTypes);

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 5);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForAllWithRatedTaxIDAndZeroTaxAmount()
		{
			complianceSubTypeRulesWithPriorityDesc.Insert(0, (TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount, "S10"));

			var expectedSubType = complianceSubTypeRulesWithPriorityDesc[0].SubType;
			var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.ToList();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var accTaxRateCodes = new List<string>()
				{
					"FREEVAT"
				};

				AssertFindMatchingComplianceSubTypeRuleByTaxCode(expectedSubType, taxInvoiceRules, accTaxRateCodes);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var accTaxRateTypes = new List<string>()
					{
						AccTaxRate.Types.IntegratedGST
					};

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0, Constants.CountryCodes.India);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForAllWithRatedTaxIDAndZeroTaxAmountOrderByTaxRegistrationLocationRule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var accTaxRateCodes = new List<string>()
					{
						"FREEVAT"
					};

				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var configuration1 = collection.AddNew();
				configuration1.Country = Constants.CountryCodes.Taiwan;
				configuration1.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
				configuration1.LedgerType = LedgerTypes.AccountsReceivable;
				configuration1.InvoiceType = TransactionTypes.Invoice;
				configuration1.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
				configuration1.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration1.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration1.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;

				var configuration2 = collection.AddNew();
				configuration2.Country = Constants.CountryCodes.Taiwan;
				configuration2.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP;
				configuration2.LedgerType = LedgerTypes.AccountsReceivable;
				configuration2.InvoiceType = TransactionTypes.Invoice;
				configuration2.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
				configuration2.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration2.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration2.TaxRegistrationLocationRule = string.Empty;

				using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					InvoicingBase.Lines.RemoveAll();

					accTaxRateCodes.ForEach(o =>
					{
						var invoicingLine = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
						invoicingLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
						invoicingLine.TaxRate.AT_Code = o;
					});

					var result = ComplianceSubTypeRule.GetMatchingComplianceSubType();
					AssertNotNull("Finding result should not be null", result);
					AssertEquals("Tax invoice rules should be equal to TDP because it doesn't have tax registration location rule", "TDP", result);

					InvoicingBase.Header.ResetCodeForTaxRegistration_ForTestOnly();
					var cusCode3 = InvoicingBase.Header.CustomsCodes.AddNew();
					cusCode3.OK_CodeType = "VAT";
					cusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
					cusCode3.OK_CustomsRegNo = "22222222";

					result = ComplianceSubTypeRule.GetMatchingComplianceSubType();
					AssertNotNull("Finding result should not be null", result);
					AssertEquals("Tax invoice rules should be equal TXC because it has tax registration location rule with TW", "TXC", result);
				}
			}
		}

		public void TestGetMatchingComplianceSubTypeMethod()
		{
			InvoicingBase.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			TestObjectCreator.CreateCustomsCodes(InvoicingBase.Header, CountryCodes.China, "VAT", "12345678");
			TestObjectCreator.CreateCustomsCodes(InvoicingBase.Header, CountryCodes.China, "VAG", "12345679");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				var accTaxRateCode = "FREEVA";
				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var configuration1 = collection.AddNew();
				configuration1.Country = CountryCodes.China;
				configuration1.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;
				configuration1.LedgerType = LedgerTypes.AccountsReceivable;
				configuration1.InvoiceType = TransactionTypes.Invoice;
				configuration1.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
				configuration1.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration1.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration1.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
				configuration1.TaxRegistrationLocationRule = CountryCodes.China;

				var configuration2 = collection.AddNew();
				configuration2.Country = CountryCodes.China;
				configuration2.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA;
				configuration2.LedgerType = LedgerTypes.AccountsReceivable;
				configuration2.InvoiceType = TransactionTypes.Invoice;
				configuration2.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
				configuration2.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration2.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration2.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
				configuration2.TaxRegistrationLocationRule = string.Empty;

				var collection2 = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var configuration3 = collection2.AddNew();
				configuration3.Country = CountryCodes.China;
				configuration3.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
				configuration3.LedgerType = LedgerTypes.AccountsReceivable;
				configuration3.InvoiceType = TransactionTypes.Invoice;
				configuration3.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
				configuration3.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration3.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration3.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
				configuration3.TaxRegistrationLocationRule = CountryCodes.China;

				var configuration4 = collection2.AddNew();
				configuration4.Country = CountryCodes.China;
				configuration4.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
				configuration4.LedgerType = LedgerTypes.AccountsReceivable;
				configuration4.InvoiceType = TransactionTypes.Invoice;
				configuration4.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
				configuration4.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration4.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration4.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
				configuration4.TaxRegistrationLocationRule = string.Empty;

				InvoicingBase.Header.ResetCodeForTaxRegistration_ForTestOnly();
				var cusCode3 = InvoicingBase.Header.CustomsCodes.AddNew();
				cusCode3.OK_CodeType = "VAG";
				cusCode3.OK_RN_NKCodeCountry = CountryCodes.China;
				cusCode3.OK_CustomsRegNo = "22222222";

				using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection2))
				{
					InvoicingBase.Lines.RemoveAll();
					var invoicingLine = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					invoicingLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
					invoicingLine.TaxRate.AT_Code = accTaxRateCode;

					var result = ComplianceSubTypeRule.GetMatchingComplianceSubType(GlbBranch.CurrentBranch.PK.ToGuid());
					AssertNotNull("Finding result should not be null", result);
					AssertEquals("ETB", result);

					using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, collection))
					{
						result = ComplianceSubTypeRule.GetMatchingComplianceSubType(GlbBranch.CurrentBranch.PK.ToGuid());
						AssertNotNull("Finding result should not be null", result);
						AssertEquals("Tax invoice rules should be equal to ETA because it doesn't have tax registration location rule", "ETA", result);
					}
				}
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleWithNoRule()
		{
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				InvoicingBase.Lines.RemoveAll();
				var result = ComplianceSubTypeRule.GetMatchingComplianceSubType();
				Assert("Finding result should be Empty", result.IsEmpty);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForAllWithExemptTaxIDs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				complianceSubTypeRulesWithPriorityDesc.Insert(0, (TaxInvoiceRuleCodes.AllWithExemptTaxIDs, "S10"));

				var expectedSubType = complianceSubTypeRulesWithPriorityDesc[0].SubType;
				var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.ToList();

				var accTaxRateTypes = new List<string>()
					{
						AccTaxRate.Types.Exempt
					};

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0, Constants.CountryCodes.India);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForAllWithNoReportTaxIDs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				complianceSubTypeRulesWithPriorityDesc.Insert(0, (TaxInvoiceRuleCodes.AllWithNoReportTaxIDs, "S10"));

				var expectedSubType = complianceSubTypeRulesWithPriorityDesc[0].SubType;
				var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.ToList();

				var accTaxRateTypes = new List<string>()
					{
						AccTaxRate.Types.NotReportable
					};

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0, Constants.CountryCodes.India);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForAllWithExemptTaxIDsWithCMTForIndia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				complianceSubTypeRulesWithPriorityDesc.Insert(0, (TaxInvoiceRuleCodes.AllWithExemptTaxIDs, "S10"));

				var expectedSubType = complianceSubTypeRulesWithPriorityDesc[0].SubType;
				var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.ToList();

				AddCommentChargeLine();
				var accTaxRateTypes = new List<string>()
				{
						AccTaxRate.Types.Exempt
				};

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0, Constants.CountryCodes.India, shouldRemoveAllLines: false);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForAllWithNoReportTaxIDsWithCMTForIndia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				complianceSubTypeRulesWithPriorityDesc.Insert(0, (TaxInvoiceRuleCodes.AllWithNoReportTaxIDs, "S10"));

				var expectedSubType = complianceSubTypeRulesWithPriorityDesc[0].SubType;
				var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.ToList();

				AddCommentChargeLine();
				var accTaxRateTypes = new List<string>()
				{
						AccTaxRate.Types.NotReportable
				};

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0, Constants.CountryCodes.India, shouldRemoveAllLines: false);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForAllWithExemptTaxIDsWithCMTForMalaysia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Malaysia))
			{
				complianceSubTypeRulesWithPriorityDesc.Insert(0, (TaxInvoiceRuleCodes.AllWithExemptTaxIDs, "S10"));

				var expectedSubType = complianceSubTypeRulesWithPriorityDesc[0].SubType;
				var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.ToList();

				AddCommentChargeLine();
				var accTaxRateTypes = new List<string>()
				{
						AccTaxRate.Types.Exempt
				};

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0, Constants.CountryCodes.Malaysia, shouldRemoveAllLines: false);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForAllWithNoReportTaxIDsWithCMTForMalaysia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Malaysia))
			{
				complianceSubTypeRulesWithPriorityDesc.Insert(0, (TaxInvoiceRuleCodes.AllWithNoReportTaxIDs, "S10"));

				var expectedSubType = complianceSubTypeRulesWithPriorityDesc[0].SubType;
				var taxInvoiceRules = complianceSubTypeRulesWithPriorityDesc.ToList();

				AddCommentChargeLine();
				var accTaxRateTypes = new List<string>()
				{
						AccTaxRate.Types.NotReportable
				};

				AssertFindMatchingComplianceSubTypeRuleByTaxType(expectedSubType, taxInvoiceRules, accTaxRateTypes, 0, Constants.CountryCodes.Malaysia, shouldRemoveAllLines: false);
			}
		}

		void AssertFindMatchingComplianceSubTypeRuleByTaxType(string expectedRule, List<(string TaxInvoiceRule, string SubType)> taxInvoiceRules, List<string> accTaxRateTypes, ZDecimal gSTVAT, string country = Constants.CountryCodes.Taiwan, bool shouldRemoveAllLines = true)
		{
			var collection = CreateComplianceSubTypeAttributionRuleConfigurations(taxInvoiceRules, country);

			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.DataType.SuspendValidation())
			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				if (shouldRemoveAllLines)
				{
					InvoicingBase.Lines.RemoveAll();
				}

				accTaxRateTypes.ForEach(o =>
				{
					var invoicingLine = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					invoicingLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
					invoicingLine.TaxRate.AT_Type = o;
				});

				if (gSTVAT != 0)
				{
					var invoicingLineForGSTVAT = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					invoicingLineForGSTVAT.AL_GSTVAT = gSTVAT;
				}

				var result = ComplianceSubTypeRule.GetMatchingComplianceSubType();
				AssertEquals("Compliance Subtype should be equal", expectedRule, result);
			}
		}

		void AssertFindMatchingComplianceSubTypeRuleByTaxCode(string expectedRule, List<(string TaxInvoiceRule, string SubType)> taxInvoiceRules, List<string> accTaxRateCodes, string country = Constants.CountryCodes.Taiwan, bool shouldRemoveAllLines = true)
		{
			var collection = CreateComplianceSubTypeAttributionRuleConfigurations(taxInvoiceRules, country);

			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.DataType.SuspendValidation())
			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				if (shouldRemoveAllLines)
				{
					InvoicingBase.Lines.RemoveAll();
				}

				accTaxRateCodes.ForEach(o =>
				{
					var invoicingLine = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
					invoicingLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
					invoicingLine.TaxRate.AT_Code = o;
				});

				var result = ComplianceSubTypeRule.GetMatchingComplianceSubType();
				AssertEquals("Compliance Subtype should be equal", expectedRule, result);
			}
		}

		ComplianceSubTypeAttributionRuleConfigurationCollection CreateComplianceSubTypeAttributionRuleConfigurations(List<(string TaxInvoiceRule, string SubType)> taxInvoiceRules, string country)
		{
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();

			List<(string LedgerTypes, string InvoiceType, string BillingRule)> ledgerInvoiceSelfBillingList = new List<(string LedgerTypes, string InvoiceType, string BillingRule)>
			{
				(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, SelfBillingRuleCodes.StandardTransactions),
				(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, string.Empty),
				(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, SelfBillingRuleCodes.StandardTransactions),
				(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, string.Empty),
				(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, SelfBillingRuleCodes.StandardTransactions),
				(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, string.Empty),
				(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, SelfBillingRuleCodes.StandardTransactions),
				(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, string.Empty),
				(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, SelfBillingRuleCodes.SelfBillingTransactions),
				(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, SelfBillingRuleCodes.SelfBillingTransactions),
				(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, SelfBillingRuleCodes.SelfBillingTransactions),
			};
			for (int i = 0; i < taxInvoiceRules.Count; i++)
			{
				var newRule = new ComplianceSubTypeAttributionRuleConfiguration()
				{
					Country = country,
					SubType = taxInvoiceRules[i].SubType,
					LedgerType = ledgerInvoiceSelfBillingList[i].LedgerTypes,
					InvoiceType = ledgerInvoiceSelfBillingList[i].InvoiceType,
					TaxInvoiceRule = taxInvoiceRules[i].TaxInvoiceRule,
					DisbursementRule = DisbursementRuleCodes.AllTransactions,
					OriginalRule = OriginalRuleCodes.AllTransactions,
					SelfBillingRule = ledgerInvoiceSelfBillingList[i].BillingRule
				};
				collection.Add(newRule);
			}

			return collection;
		}

		public void TestFindMatchingComplianceSubTypeRuleForSpecificTaxIDs()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "PPN";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Indonesia))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, IndonesiaComplianceInfo.RuleSetCodes.T01T04));
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123", TestObjectCreator.IDR, 1M, TestObjectCreator.ABIGAS);
				invoice.AH_ComplianceSubType = "";
				var line1 = (InvoicingLineBase)invoice.Lines.AddNew();
				line1.AL_AT = taxRate.PK;
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;

				AssertEquals(ZString.Empty, invoice.AH_ComplianceSubType);

				Factory.Save();

				AssertEquals("T01", invoice.AH_ComplianceSubType);
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForLK_TaxInvoiceRuleHasHigherPriorityThanTaxRegistrationType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SriLanka))
			{
				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var configuration1 = collection.AddNew();
				configuration1.Country = Constants.CountryCodes.SriLanka;
				configuration1.SubType = SriLankaComplianceInfo.ComplianceSubTypeCodes.TXI;
				configuration1.LedgerType = LedgerTypes.AccountsReceivable;
				configuration1.InvoiceType = TransactionTypes.Invoice;
				configuration1.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration1.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration1.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration1.TaxRegistrationType = TaxRegistrationTypeCodes.SVATOrganizations;

				var configuration2 = collection.AddNew();
				configuration2.Country = Constants.CountryCodes.SriLanka;
				configuration2.SubType = SriLankaComplianceInfo.ComplianceSubTypeCodes.STX;
				configuration2.LedgerType = LedgerTypes.AccountsReceivable;
				configuration2.InvoiceType = TransactionTypes.Invoice;
				configuration2.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID;
				configuration2.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration2.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration2.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;

				using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					TestObjectCreator.ABIGAS.CustomsCodes.AddNew(SriLankaOrgCusCodeInfo.OrgCusCodes.SVT, Core.Constants.CountryCodes.SriLanka);
					var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
					invoice1.AH_ComplianceSubType = "";
					var line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
					line1.AL_AT = TestObjectCreator.SVAT1.PK;
					line1.AL_AG = TestObjectCreator.GLHeader1.PK;
					AssertEquals(ZString.Empty, invoice1.AH_ComplianceSubType);
					Factory.Save();
					AssertEquals("STX", invoice1.AH_ComplianceSubType);
				}
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForLK_TaxRegistrationTypePrecedence()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SriLanka))
			{
				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var configuration1 = collection.AddNew();
				configuration1.Country = Constants.CountryCodes.SriLanka;
				configuration1.SubType = SriLankaComplianceInfo.ComplianceSubTypeCodes.TXI;
				configuration1.LedgerType = LedgerTypes.AccountsReceivable;
				configuration1.InvoiceType = TransactionTypes.Invoice;
				configuration1.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration1.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration1.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration1.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;

				var configuration2 = collection.AddNew();
				configuration2.Country = Constants.CountryCodes.SriLanka;
				configuration2.SubType = SriLankaComplianceInfo.ComplianceSubTypeCodes.STX;
				configuration2.LedgerType = LedgerTypes.AccountsReceivable;
				configuration2.InvoiceType = TransactionTypes.Invoice;
				configuration2.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration2.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration2.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration2.TaxRegistrationType = TaxRegistrationTypeCodes.SVATOrganizations;

				var configuration3 = collection.AddNew();
				configuration3.Country = Constants.CountryCodes.SriLanka;
				configuration3.SubType = SriLankaComplianceInfo.ComplianceSubTypeCodes.NTI;
				configuration3.LedgerType = LedgerTypes.AccountsReceivable;
				configuration3.InvoiceType = TransactionTypes.Invoice;
				configuration3.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration3.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration3.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration3.TaxRegistrationType = TaxRegistrationTypeCodes.NotRegisteredOrganizations;

				using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					var codeSVT = TestObjectCreator.ABIGAS.CustomsCodes.AddNew(SriLankaOrgCusCodeInfo.OrgCusCodes.SVT, Core.Constants.CountryCodes.SriLanka);
					var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
					invoice1.AH_ComplianceSubType = "";
					var line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
					line1.AL_AT = TestObjectCreator.GST1.PK;
					line1.AL_AG = TestObjectCreator.GLHeader1.PK;
					AssertEquals(ZString.Empty, invoice1.AH_ComplianceSubType);
					Factory.Save();
					AssertEquals("STX", invoice1.AH_ComplianceSubType);

					codeSVT.Delete();
					var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV234", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
					invoice2.AH_ComplianceSubType = "";
					var line2 = (InvoicingLineBase)invoice2.Lines.AddNew();
					line2.AL_AT = TestObjectCreator.GST1.PK;
					line2.AL_AG = TestObjectCreator.GLHeader1.PK;
					AssertEquals(ZString.Empty, invoice2.AH_ComplianceSubType);
					Factory.Save();
					AssertEquals("NTI", invoice2.AH_ComplianceSubType);

					TestObjectCreator.ABIGAS.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.SriLanka);
					var invoice3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV345", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
					invoice3.AH_ComplianceSubType = "";
					var line3 = (InvoicingLineBase)invoice3.Lines.AddNew();
					line3.AL_AT = TestObjectCreator.GST1.PK;
					line3.AL_AG = TestObjectCreator.GLHeader1.PK;
					AssertEquals(ZString.Empty, invoice3.AH_ComplianceSubType);
					Factory.Save();
					AssertEquals("TXI", invoice3.AH_ComplianceSubType);
				}
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForLK_TaxInvoiceRulePrecedence()
		{
			// Note: This test and all the existing tests above for TW which test the tax invoice rule's precedence should be
			// refactored in a seperate WI, as the current way to put country specific tests in this class is undesired,
			// ideally they should go to each of the specific country's test class.
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SriLanka))
			{
				TestObjectCreator.ABIGAS.CustomsCodes.AddNew(SriLankaOrgCusCodeInfo.OrgCusCodes.SVT, Core.Constants.CountryCodes.SriLanka);
				var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				invoice1.AH_ComplianceSubType = "";
				var line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
				line1.AL_AT = TestObjectCreator.SVAT1.PK;
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;

				var line2 = (InvoicingLineBase)invoice1.Lines.AddNew();
				line2.AL_AT = TestObjectCreator.GST1.PK;
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;

				AssertEquals(ZString.Empty, invoice1.AH_ComplianceSubType);

				Factory.Save();
				AssertEquals("STX", invoice1.AH_ComplianceSubType);
			}
		}

		public void TestEvaluateOriginalRuleForManuallyAmendedInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var configuration1 = collection.AddNew();
				configuration1.Country = Constants.CountryCodes.Taiwan;
				configuration1.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
				configuration1.LedgerType = LedgerTypes.AccountsReceivable;
				configuration1.InvoiceType = TransactionTypes.CreditNote;
				configuration1.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration1.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration1.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;

				var configuration2 = collection.AddNew();
				configuration2.Country = Constants.CountryCodes.Taiwan;
				configuration2.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP;
				configuration2.LedgerType = LedgerTypes.AccountsReceivable;
				configuration2.InvoiceType = TransactionTypes.CreditNote;
				configuration2.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration2.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration2.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;

				var creditNote = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
				ComplianceSubTypeRule = new ComplianceSubTypeRule(Factory, creditNote);

				using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					var line = (InvoicingLineBase)creditNote.Lines.AddNew();
					line.AL_AT = TestObjectCreator.GST1.PK;
					line.AL_GSTVAT = 100m;
					Assert(creditNote.AH_TransactionBelongsToGroup.IsEmpty);
					Assert(creditNote.AH_OriginalTransactionNum.IsEmpty);
					Assert(creditNote.AH_OriginalInvoiceDate.IsEmpty);
					Assert(!((IEvaluateComplianceRule)creditNote).IsAmendingTransaction);
					var result = ComplianceSubTypeRule.GetMatchingComplianceSubType();
					AssertEquals("Expect to use rule for original transaction", "TXC", result);

					creditNote.AH_OriginalTransactionNum = "abcd1234";
					Assert(((IEvaluateComplianceRule)creditNote).IsAmendingTransaction);
					result = ComplianceSubTypeRule.GetMatchingComplianceSubType();
					AssertEquals("Expect to use rule for amending transaction", "TDP", result);

					creditNote.AH_OriginalTransactionNum = ZString.Empty;
					creditNote.AH_OriginalInvoiceDate = ZDate.Today;
					Assert(((IEvaluateComplianceRule)creditNote).IsAmendingTransaction);
					result = ComplianceSubTypeRule.GetMatchingComplianceSubType();
					AssertEquals("Expect to use rule for amending transaction", "TDP", result);
				}
			}
		}

		public void TestFindMatchingComplianceSubTypeRuleForParentTransactionSubType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
				var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
				arInvLine.AL_AG = testObjectCreator.GLHeader1.PK;
				arInvLine.AL_AT = testObjectCreator.GST1.PK;
				var invoiceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "header1", "TXC0001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, "line1", arInvLine, testObjectCreator.Debtor);
				Factory.Save();

				var arCreditNote = (ARCreditNote)(testObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInv)).amendTransaction;
				var arCRDline = arCreditNote.Lines.Cast<InvoicingLineBase>().First();
				var arCreditNoteDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "header2", "", "", "line2", arCRDline, testObjectCreator.Debtor);

				ComplianceSubTypeRule = new ComplianceSubTypeRule(Factory, arCreditNoteDocumentHeader);

				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var configuration1 = collection.AddNew();
				configuration1.Country = Constants.CountryCodes.Taiwan;
				configuration1.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;
				configuration1.LedgerType = LedgerTypes.AccountsReceivable;
				configuration1.InvoiceType = TransactionTypes.CreditNote;
				configuration1.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration1.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration1.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration1.ParentTransactionSubType = "";

				var configuration2 = collection.AddNew();
				configuration2.Country = Constants.CountryCodes.Taiwan;
				configuration2.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR;
				configuration2.LedgerType = LedgerTypes.AccountsReceivable;
				configuration2.InvoiceType = TransactionTypes.CreditNote;
				configuration2.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration2.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration2.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration2.ParentTransactionSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;

				using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					var result = ComplianceSubTypeRule.GetMatchingComplianceSubType();
					var message = "Expect to use the rule with TCR ParentTransactionSubType, if the mathching rule which has empty ParentTransactionSubType should be as the last one.";
					AssertEquals(message, "TCR", result);
				}
			}
		}

		public void TestEvaluateParentTransactionRule() => AssertEvaluateParentTransactionRule();

		public void TestEvaluateParentTransactionRule_ReversalTransactionOnly() => AssertEvaluateParentTransactionRule(OriginalRuleCodes.ReversalTransactionOnly);

		void AssertEvaluateParentTransactionRule(string originalRuleCode = OriginalRuleCodes.AmendingReversalOnly)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Argentina))
			{
				var collection = CreateComplianceSubTypeRule(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA, TransactionTypes.CreditNote, originalRuleCode);

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);
				invoice.AH_ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE;

				var creditNote = TestObjectCreator.ReverseTransaction(invoice, out string reverseError) as ARCreditNote;

				var complianceSubTypeRule = new ComplianceSubTypeRule(Factory, creditNote);
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA);

				collection[0].ParentTransactionSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);

				collection[0].ParentTransactionSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE;
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA);
			}
		}

		public void TestEvaluateParentTransactionRule_AmendingTransactionOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Argentina))
			{
				var collection = CreateComplianceSubTypeRule(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA, TransactionTypes.CreditNote, OriginalRuleCodes.AmendingTransactionOnly);

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);
				invoice.AH_ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE;

				var (creditNote, _) = TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, invoice);

				var complianceSubTypeRule = new ComplianceSubTypeRule(Factory, (IEvaluateComplianceRule)creditNote);
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA);

				collection[0].ParentTransactionSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);

				collection[0].ParentTransactionSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE;
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA);
			}
		}

		public void TestEvaluateTaxSystemRule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Argentina))
			using (AccountingMasterFilesRegistry.Instance.TaxSystems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateTaxSystemConfiguration()))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				var complianceSubTypeRule = new ComplianceSubTypeRule(Factory, invoice);
				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();

				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);

				collection = CreateComplianceSubTypeRule(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE, TransactionTypes.Invoice, OriginalRuleCodes.OriginalTransactionOnly);

				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE);

				var taxTransaction1 = TaxFrameworkTestHelper.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters() { TransactionHeader = invoice });
				var taxTransaction2 = TaxFrameworkTestHelper.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters() { TransactionHeader = invoice });

				collection[0].RequiredTaxSystem = "ISS";
				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);

				taxTransaction1.ATT_TaxSystemCode = "ISS";
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE);

				taxTransaction1.ATT_TaxSystemCode = "QQQ";
				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);

				collection[0].RequiredTaxSystem = string.Empty;
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE);

				collection[0].ExcludedTaxSystem = "INSS";
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE);

				taxTransaction2.ATT_TaxSystemCode = "INSS";
				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);

				taxTransaction2.ATT_TaxSystemCode = "XYZ";
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE);

				collection[0].RequiredTaxSystem = "ISS";
				collection[0].ExcludedTaxSystem = "INSS";
				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);

				taxTransaction1.ATT_TaxSystemCode = "ISS";
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE);

				taxTransaction2.ATT_TaxSystemCode = "INSS";
				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);
			}
		}

		public void TestEvaluateRegistrationRule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Argentina))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				var complianceSubTypeRule = new ComplianceSubTypeRule(Factory, invoice);
				var collection = CreateComplianceSubTypeRule(ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE, TransactionTypes.Invoice, OriginalRuleCodes.OriginalTransactionOnly);

				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE);

				string codeType1 = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
				string codeType2 = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL;
				string codeType3 = ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI;

				invoice.Header.CustomsCodes.AddNew(codeType1, "1111");
				invoice.Header.CustomsCodes.AddNew(codeType2, "2222");

				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE);

				collection[0].RequiredRegistrationCode = codeType3;
				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);

				collection[0].RequiredRegistrationCode = codeType1;
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE);

				collection[0].RequiredRegistrationCode = ZString.Empty;
				collection[0].ExcludedRegistrationCode = codeType3;
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE);

				collection[0].ExcludedRegistrationCode = codeType2;
				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);

				collection[0].RequiredRegistrationCode = codeType3;
				collection[0].ExcludedRegistrationCode = codeType2;
				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);

				collection[0].RequiredRegistrationCode = codeType1;
				collection[0].ExcludedRegistrationCode = codeType3;
				AssertMatchedSubType(collection, complianceSubTypeRule, ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE);

				collection[0].ExcludedRegistrationCode = codeType2;
				AssertMatchedSubType(collection, complianceSubTypeRule, string.Empty);
			}
		}

		void AssertMatchedSubType(ComplianceSubTypeAttributionRuleConfigurationCollection collection, ComplianceSubTypeRule complianceSubTypeRule, string expectedSubTypeCode)
		{
			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals(expectedSubTypeCode, complianceSubTypeRule.GetMatchingComplianceSubType());
			}
		}

		public void TestIsContainsAtLeastOneTaxIDAndAmountOfTax()
		{
			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_GSTVAT = 100m;
			Assert(ComplianceSubTypeRule.ContainsAtLeastOneTaxIDAndAmountOfTax_ForTestOnly);

			line.AL_GSTVAT = 0m;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDAndAmountOfTax_ForTestOnly);

			line.AL_AT = ZGuid.Empty;
			line.AL_GSTVAT = 100m;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDAndAmountOfTax_ForTestOnly);

			line.AL_AT = ZGuid.Empty;
			line.AL_GSTVAT = 0m;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDAndAmountOfTax_ForTestOnly);
		}

		public void TestIsContainsAtLeastOneTaxIDAndNoAmountOfTax()
		{
			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_GSTVAT = 100m;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDAndNoAmountOfTax_ForTestOnly);

			line.AL_GSTVAT = 0m;
			Assert(ComplianceSubTypeRule.ContainsAtLeastOneTaxIDAndNoAmountOfTax_ForTestOnly);

			line.AL_AT = ZGuid.Empty;
			line.AL_GSTVAT = 100m;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDAndAmountOfTax_ForTestOnly);

			line.AL_GSTVAT = 0m;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDAndAmountOfTax_ForTestOnly);
		}

		public void TestIsContainsAnAmountOfTax()
		{
			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_GSTVAT = 100m;
			Assert(ComplianceSubTypeRule.ContainsAnAmountOfTax_ForTestOnly);

			line.AL_GSTVAT = 0m;
			Assert(!ComplianceSubTypeRule.ContainsAnAmountOfTax_ForTestOnly);
		}

		public void TestIsContainsAtLeastOneTaxID()
		{
			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AT = TestObjectCreator.GST1.PK;
			Assert(ComplianceSubTypeRule.IsContainsAtLeastOneTaxID_ForTestOnly);

			line.AL_AT = ZGuid.Empty;
			Assert(!ComplianceSubTypeRule.IsContainsAtLeastOneTaxID_ForTestOnly);

			InvoicingBase.Lines.RemoveAll();
			Assert(!ComplianceSubTypeRule.IsContainsAtLeastOneTaxID_ForTestOnly);
		}

		public void TestIsContainsAtLeastOneTaxIDExcludingNOT()
		{
			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			TestObjectCreator.GST1.AT_Type = "NOT";
			line.AL_AT = TestObjectCreator.GST1.PK;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingNOT_ForTestOnly);

			TestObjectCreator.GST1.AT_Type = "CAP";
			Assert(ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingNOT_ForTestOnly);

			line.AL_AT = ZGuid.Empty;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingNOT_ForTestOnly);

			InvoicingBase.Lines.RemoveAll();
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingNOT_ForTestOnly);
		}

		public void TestIsContainsAtLeastOneTaxIDExcludingNOTAndEXL()
		{
			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			TestObjectCreator.GST1.AT_Type = "NOT";
			line.AL_AT = TestObjectCreator.GST1.PK;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingNOTAndEXL_ForTestOnly);

			TestObjectCreator.GST1.AT_Type = "EXL";
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingNOTAndEXL_ForTestOnly);

			TestObjectCreator.GST1.AT_Type = "CAP";
			Assert(ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingNOTAndEXL_ForTestOnly);

			line.AL_AT = ZGuid.Empty;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingNOTAndEXL_ForTestOnly);

			InvoicingBase.Lines.RemoveAll();
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingNOTAndEXL_ForTestOnly);
		}

		public void TestIsAllWithTaxIDAndZeroTaxAmount()
		{
			InvoicingLineBase line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_GSTVAT = 100m;
			Assert(!ComplianceSubTypeRule.AllWithTaxIDAndZeroTaxAmount_ForTestOnly);

			line.AL_GSTVAT = 0m;
			Assert(!ComplianceSubTypeRule.AllWithTaxIDAndZeroTaxAmount_ForTestOnly);

			TestObjectCreator.GST1.AT_Type = "NOT";
			line.AL_AT = TestObjectCreator.GST1.PK;
			Assert(!ComplianceSubTypeRule.AllWithTaxIDAndZeroTaxAmount_ForTestOnly);

			TestObjectCreator.GST1.AT_Type = "RAT";
			Assert(ComplianceSubTypeRule.AllWithTaxIDAndZeroTaxAmount_ForTestOnly);

			TestObjectCreator.GST1.AT_Type = "CAP";
			Assert(ComplianceSubTypeRule.AllWithTaxIDAndZeroTaxAmount_ForTestOnly);

			TestObjectCreator.GST1.AT_Type = "INT";
			Assert(ComplianceSubTypeRule.AllWithTaxIDAndZeroTaxAmount_ForTestOnly);

			var line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_GSTVAT = 0m;
			line2.AL_AT = Guid.Empty;
			Assert(!ComplianceSubTypeRule.AllWithTaxIDAndZeroTaxAmount_ForTestOnly);

			line2.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			Assert("Comment charge will be ignore, AllWithTaxIDAndZeroTaxAmount will be true", ComplianceSubTypeRule.AllWithTaxIDAndZeroTaxAmount_ForTestOnly);
		}

		public void TestAllContainedInSpecifiedTaxIDCodes_IgnoreCommentChargge()
		{
			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_GSTVAT = 100m;
			line.AL_AT = TestObjectCreator.GST1.PK;
			IReadOnlyCollection<ZString> splitTaxIDCodes = new[] { new ZString("ZZGST1") };
			Assert("AllContainedInSpecifiedTaxIDCodes is true", ComplianceSubTypeRule.AllContainedInSpecifiedTaxIDCodes_TestOnly(splitTaxIDCodes));

			var line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_GSTVAT = 0m;
			line2.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			line2.AL_AT = TestObjectCreator.GST2.PK;
			AssertEquals("TaxIDCode of comment line is not contained in splitTaxIDCodes", false, splitTaxIDCodes.Contains(line2.TaxRate.AT_Code));
			Assert("Comment charge will be ignore, AllContainedInSpecifiedTaxIDCodes is true", ComplianceSubTypeRule.AllContainedInSpecifiedTaxIDCodes_TestOnly(splitTaxIDCodes));
		}

		public void TestIsContainsAtLeastOneSuspendedTaxID()
		{
			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AT = TestObjectCreator.SVAT1.PK;
			Assert(ComplianceSubTypeRule.ContainsAtLeastOneSuspendedTaxID_ForTestOnly);

			line.AL_AT = ZGuid.Empty;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneSuspendedTaxID_ForTestOnly);
		}

		public void TestIsContainsAtLeastOneTaxIDExcludingSuspended()
		{
			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_AT = TestObjectCreator.SVAT1.PK;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingSuspended_ForTestOnly);

			var line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line2.AL_AT = TestObjectCreator.GST1.PK;
			Assert(ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingSuspended_ForTestOnly);

			line2.AL_AT = ZGuid.Empty;
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingSuspended_ForTestOnly);

			InvoicingBase.Lines.RemoveAll();
			Assert(!ComplianceSubTypeRule.ContainsAtLeastOneTaxIDExcludingSuspended_ForTestOnly);
		}

		public void TestIsAllWithExemptTaxIDs()
		{
			var line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			var line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();

			TestObjectCreator.GST1.AT_Type = "NOT";
			line1.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.GST2.AT_Type = "EXT";
			line2.AL_AT = TestObjectCreator.GST2.PK;
			Assert(!ComplianceSubTypeRule.AllWithExemptTaxIDs_ForTestOnly);

			TestObjectCreator.GST1.AT_Type = "EXT";
			TestObjectCreator.GST2.AT_Type = "EXT";
			Assert(ComplianceSubTypeRule.AllWithExemptTaxIDs_ForTestOnly);
		}

		public void TestIsAllWithNoReportTaxIDs()
		{
			var line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			var line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();

			TestObjectCreator.GST1.AT_Type = "RAT";
			line1.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.GST2.AT_Type = "NOT";
			line2.AL_AT = TestObjectCreator.GST2.PK;
			Assert(!ComplianceSubTypeRule.AllWithNoReportTaxIDs_ForTestOnly);

			TestObjectCreator.GST1.AT_Type = "NOT";
			TestObjectCreator.GST2.AT_Type = "NOT";
			Assert(ComplianceSubTypeRule.AllWithNoReportTaxIDs_ForTestOnly);
		}

		public void TestApplySubTypeThresholdNotMet_WithoutThresholdApplies()
		{
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Argentina))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);
				line1.AL_AT = TestObjectCreator.SVAT1.PK;
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;

				var complianceSubTypeRule = new ComplianceSubTypeRule(Factory, invoice);
				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();

				using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					AssertEquals("There is not matched rule, compliance subtype should be empty.", string.Empty, complianceSubTypeRule.GetMatchingComplianceSubType());
				}

				var configuration = collection.AddNew();
				configuration.Country = Constants.CountryCodes.Argentina;
				configuration.SubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
				configuration.OrganisationLocation = ZString.Empty;
				configuration.RuleSetCode = ArgentinaComplianceInfo.RuleSetCodes.TipoABE;

				using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					var result = complianceSubTypeRule.GetMatchingComplianceSubType();
					AssertEquals("There is matched rule but ThresholdApplies is false, compliance subtype should be the Rule.SubType.", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE, result);
				}

				mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
				collection[0].ThresholdApplies = true;
				collection[0].SubTypeThresholdNotMet = ArgentinaComplianceInfo.ComplianceSubTypeCodes.CUS;

				using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					var result = complianceSubTypeRule.GetMatchingComplianceSubType();
					mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.Country.Code), Times.Exactly(3));
					AssertEquals("There is matched rule and ThresholdApplies is true but country does not implement IThresholdProvider", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE, result);
				}
			}
		}

		public void TestApplySubTypeThresholdNotMet_WithThresholdApplies()
		{
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IThresholdProvider>().Setup(x => x.GetThresholdAmount()).Returns(500);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Argentina))
				{
					var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
					var configuration = collection.AddNew();
					configuration.Country = Constants.CountryCodes.Argentina;
					configuration.SubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE;
					configuration.LedgerType = LedgerTypes.AccountsReceivable;
					configuration.InvoiceType = TransactionTypes.Invoice;
					configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
					configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
					configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
					configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
					configuration.OrganisationLocation = ZString.Empty;
					configuration.ThresholdApplies = ZBool.True;
					configuration.SubTypeThresholdNotMet = ArgentinaComplianceInfo.ComplianceSubTypeCodes.CUS;

					var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV123", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
					var line1 = TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);
					line1.AL_AT = TestObjectCreator.SVAT1.PK;
					line1.AL_AG = TestObjectCreator.GLHeader1.PK;

					var complianceSubTypeRule = new ComplianceSubTypeRule(Factory, invoice);

					using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
					{
						var result = complianceSubTypeRule.GetMatchingComplianceSubType();
						mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(Constants.CountryCodes.Argentina), Times.Exactly(3));
						mockIAccountingCountryFactory.As<IThresholdProvider>().Verify(x => x.GetThresholdAmount(), Times.Once);
						AssertEquals("The country implements IThresholdProvider and threshold was not exceeded", ArgentinaComplianceInfo.ComplianceSubTypeCodes.CUS, result);

						mockIAccountingCountryFactory.Invocations.Clear();
						mockIAccountingCountryFactory.As<IThresholdProvider>().Setup(x => x.GetThresholdAmount()).Returns(10);

						result = complianceSubTypeRule.GetMatchingComplianceSubType();
						mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(Constants.CountryCodes.Argentina), Times.Exactly(3));
						mockIAccountingCountryFactory.As<IThresholdProvider>().Verify(x => x.GetThresholdAmount(), Times.Once);
						AssertEquals("The country implements IThresholdProvider and threshold was exceeded", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE, result);

						mockIAccountingCountryFactory.Invocations.Clear();
						mockIAccountingCountryFactory.As<IThresholdProvider>().Setup(x => x.GetThresholdAmount()).Returns(110);

						result = complianceSubTypeRule.GetMatchingComplianceSubType();
						mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(Constants.CountryCodes.Argentina), Times.Exactly(3));
						mockIAccountingCountryFactory.As<IThresholdProvider>().Verify(x => x.GetThresholdAmount(), Times.Once);
						AssertEquals("The country implements IThresholdProvider and threshold is equal to LocalTotalAmount", ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE, result);
					}
				}
			}
		}

		public void TestMeetVNOrganizationLocationRule()
		{
			var mockIEvaluateComplianceRule = new Mock<IEvaluateComplianceRule>();
			var testOrgHeader = Factory.New<OrgHeader>();
			var testUNLOCO = Factory.New<RefUNLOCO>();
			testUNLOCO.RL_Code = "TZ";
			testOrgHeader.OH_RL_NKClosestPort = testUNLOCO.RL_Code;
			mockIEvaluateComplianceRule.Setup(x => x.Header).Returns(testOrgHeader);
			var complianceSubTypeRule = new ComplianceSubTypeRule(Factory, mockIEvaluateComplianceRule.Object);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Argentina))
			{
				Assert(!complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(Constants.CountryCodes.VietNam));
				Assert(!complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(Constants.CountryCodes.Argentina));
				Assert(!complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(ZString.Empty));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
			{
				testUNLOCO.RL_RN_NKCountryCode = ZString.Empty;
				Assert(!complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(Constants.CountryCodes.VietNam));
				Assert(!complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(Constants.CountryCodes.Argentina));
				Assert(complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(ZString.Empty));

				testUNLOCO.RL_RN_NKCountryCode = "TZ";
				Assert(!complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(Constants.CountryCodes.VietNam));
				Assert(!complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(Constants.CountryCodes.Argentina));
				Assert(complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(ZString.Empty));

				testUNLOCO.RL_RN_NKCountryCode = Constants.CountryCodes.VietNam;
				Assert(complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(Constants.CountryCodes.VietNam));
				Assert(!complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(Constants.CountryCodes.Argentina));
				Assert(!complianceSubTypeRule.MeetVNOrganizationLocationRule_ForTestOnly(ZString.Empty));
			}
		}

		public void TestEvaluateOrganizationCategoryRule()
		{
			var mockIEvaluateComplianceRule = new Mock<IEvaluateComplianceRule>();
			var testOrgHeader = Factory.New<OrgHeader>();
			testOrgHeader.OH_Category = "GOV";
			mockIEvaluateComplianceRule.Setup(x => x.Header).Returns(testOrgHeader);
			var complianceSubTypeRule = new ComplianceSubTypeRule(Factory, mockIEvaluateComplianceRule.Object);

			var cfg = new ComplianceSubTypeAttributionRuleConfiguration();
			cfg.OrganisationCategory = "";
			AssertEquals(true, complianceSubTypeRule.EvaluateOrganisationCategoryRule_ForTestOnly(cfg));
			cfg.OrganisationCategory = "GOV";
			AssertEquals(true, complianceSubTypeRule.EvaluateOrganisationCategoryRule_ForTestOnly(cfg));
			cfg.OrganisationCategory = "BUS";
			AssertEquals(false, complianceSubTypeRule.EvaluateOrganisationCategoryRule_ForTestOnly(cfg));
		}

		ComplianceSubTypeAttributionRuleConfigurationCollection CreateComplianceSubTypeRule(ZString subType, ZString invoiceType, ZString originalRule)
		{
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var configuration = collection.AddNew();

			configuration.Country = Constants.CountryCodes.Argentina;
			configuration.SubType = subType;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = invoiceType;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = originalRule;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = ArgentinaComplianceInfo.RuleSetCodes.TipoABE;

			return collection;
		}

		TaxSystemsConfigurationCollection CreateTaxSystemConfiguration()
		{
			var taxSystemISS = TaxFrameworkTestHelper.CreateTaxSystem("ISS");
			taxSystemISS.Country = Core.Constants.CountryCodes.Argentina;
			var taxSystemINSS = TaxFrameworkTestHelper.CreateTaxSystem("INSS");
			taxSystemINSS.Country = Core.Constants.CountryCodes.Argentina;

			return new TaxSystemsConfigurationCollection() { taxSystemISS, taxSystemINSS };
		}

		protected override void SetUp()
		{
			base.SetUp();

			InvoicingBase = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			InvoicingBase.AH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			ComplianceSubTypeRule = new ComplianceSubTypeRule(Factory, InvoicingBase);
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}
		TestObjectCreator testObjectCreator;

		InvoicingBase InvoicingBase;
		ComplianceSubTypeRule ComplianceSubTypeRule;

		TaxFrameworkTestObjectCreator TaxFrameworkTestHelper
		{
			get { return testHelper ?? (testHelper = new TaxFrameworkTestObjectCreator(Factory)); }
		}
		TaxFrameworkTestObjectCreator testHelper;
	}
}
