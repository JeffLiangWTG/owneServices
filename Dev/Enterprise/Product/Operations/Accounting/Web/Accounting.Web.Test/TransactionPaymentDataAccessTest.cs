using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Web.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Web.Testing
{
	public class TransactionPaymentDataAccessTest : TestCaseWithFactory
	{
		public void TestGetTransactionsWithTransactionNumber()
		{
			CreateTestData();

			var access = new TransactionPaymentDataAccess(Connection, Transaction);
			var request = GetRequest(testOrg.OH_Code, TargetTransactionNum);

			using (var reader = access.GetTransactionPaymentStatusReader(request))
			{
				AssertMatchTargetInvoice("Should match target invoice", reader, invoiceAP);
			}
		}

		public void TestGetTransactionsWithConsolidatedInvoiceRef()
		{
			CreateTestData();

			var access = new TransactionPaymentDataAccess(Connection, Transaction);
			var request = GetRequest(testOrg.OH_Code, internalReference: TargetConsolidatedInvoiceRef);

			AssertEquals("Pre-condition", LedgerTypes.AccountsPayable, request.AccLedger);

			using (var reader = access.GetTransactionPaymentStatusReader(request))
			{
				AssertMatchTargetInvoice("Should match target invoice", reader, invoiceRefAP);
			}
		}

		public void TestGetTransactionsWithJobTransactionNumber()
		{
			CreateTestData();

			var access = new TransactionPaymentDataAccess(Connection, Transaction);
			var request = GetRequest(testOrg.OH_Code, jobTransactionNumber: TargetConsolidatedInvoiceRef, ledgerType: LedgerTypes.AccountsReceivable);

			AssertEquals("Pre-condition", LedgerTypes.AccountsReceivable, request.AccLedger);

			using (var reader = access.GetTransactionPaymentStatusReader(request))
			{
				AssertMatchTargetInvoice("Should match target invoice", reader, invoiceAR);
			}
		}

		public void TestGetTransactionsWithAllNumberParametersBeEmpty()
		{
			int counter = 0;
			CreateTestData();

			var access = new TransactionPaymentDataAccess(Connection, Transaction);
			var request = GetRequest(testOrg.OH_Code);

			using (var reader = access.GetTransactionPaymentStatusReader(request))
			{
				while (reader.Read())
				{
					counter++;
				}
			}

			AssertEquals("No result found", 0, counter);
		}

		public void TestGetTransactionsWithOrgRelatedParty_DisableSettlementGroupRegistry()
		{
			PrepareOrgRelatedPartyTestData(out OrgHeader relativeOrg1, out OrgHeader relativeOrg2, out OrgHeader orgWithTransactionPosted,
										   out InvoicingBase invoiceRelativeOrg1, out InvoicingBase invoiceRelativeOrg2AP, out InvoicingBase invoiceRelativeOrg2AR, out InvoicingBase invoiceHeaderOrg);
			var access = new TransactionPaymentDataAccess(Connection, Transaction);
			AccountingConfigurationRegistry.Instance.TransactionPaymentStatusWebServiceUsesSettlementGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(relativeOrg1.OH_Code, "000031")))
			{
				AssertEquals("Should match no record because registry is off", false, reader.HasRows);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(relativeOrg2.OH_Code, jobTransactionNumber: "000034")))
			{
				AssertEquals("Should match no record because registry is off", false, reader.HasRows);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(relativeOrg2.OH_Code, "000031")))
			{
				AssertEquals("Should match no record because transaction org / number are not matched", false, reader.HasRows);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(orgWithTransactionPosted.OH_Code, "000033")))
			{
				AssertMatchTargetInvoice("Should match target invoice via header org", reader, invoiceHeaderOrg);
			}
		}

		public void TestGetTransactionsWithOrgRelatedParty_EnableSettlementGroupRegistry()
		{
			PrepareOrgRelatedPartyTestData(out OrgHeader relativeOrg1, out OrgHeader relativeOrg2, out OrgHeader orgWithTransactionPosted,
										   out InvoicingBase invoiceRelativeOrg1, out InvoicingBase invoiceRelativeOrg2AP, out InvoicingBase invoiceRelativeOrg2AR, out InvoicingBase invoiceHeaderOrg);
			var access = new TransactionPaymentDataAccess(Connection, Transaction);
			AccountingConfigurationRegistry.Instance.TransactionPaymentStatusWebServiceUsesSettlementGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(relativeOrg1.OH_Code, "000031")))
			{
				AssertMatchTargetInvoice("Should match target invoice via related party", reader, invoiceRelativeOrg1);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(relativeOrg2.OH_Code, jobTransactionNumber: "B123456", ledgerType: LedgerTypes.AccountsReceivable)))
			{
				AssertMatchTargetInvoice("Should match target invoice via related party and job transaction number", reader, invoiceRelativeOrg2AR);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(relativeOrg2.OH_Code, "000031")))
			{
				AssertEquals("Should find no record, because transaction org / number are not matched", false, reader.HasRows);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(orgWithTransactionPosted.OH_Code, "000033")))
			{
				AssertMatchTargetInvoice("Should match target invoice via header org", reader, invoiceHeaderOrg);
			}
		}

		public void TestGetTransactionsWithSelfRelatedOrgRelatedParty()
		{
			PrepareOrgRelatedPartyTestData(out OrgHeader relativeOrg1, out OrgHeader relativeOrg2, out OrgHeader orgWithTransactionPosted,
										   out InvoicingBase invoiceRelativeOrg1, out InvoicingBase invoiceRelativeOrg2AP, out InvoicingBase invoiceRelativeOrg2AR, out InvoicingBase invoiceHeaderOrg, isSelfRelated: true);
			var access = new TransactionPaymentDataAccess(Connection, Transaction);
			AccountingConfigurationRegistry.Instance.TransactionPaymentStatusWebServiceUsesSettlementGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(relativeOrg1.OH_Code, "000031")))
			{
				AssertMatchTargetInvoice("Should match target invoice via related party", reader, invoiceRelativeOrg1);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(relativeOrg2.OH_Code, jobTransactionNumber: "B123456", ledgerType: LedgerTypes.AccountsReceivable)))
			{
				AssertMatchTargetInvoice("Should match target invoice via related party and job transaction number", reader, invoiceRelativeOrg2AR);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(relativeOrg2.OH_Code, "000031")))
			{
				AssertEquals("Should find no record, because transaction org / number are not matched", false, reader.HasRows);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(orgWithTransactionPosted.OH_Code, "000033")))
			{
				AssertMatchTargetInvoice("Should match target invoice via header org", reader, invoiceHeaderOrg);
			}
		}

		public void TestGetTransactionsWithLegacySystemCode()
		{
			CreateTestData();

			var legacyOrgCode = "TTT";
			var orgWithTransactionPosted = TestObjectCreator.CreateOrgHeader("TS1", true, true);

			var orgWithLSCCode = TestObjectCreator.CreateOrgHeader("TS2", true, true);
			orgWithLSCCode.ConfigOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, legacyOrgCode, GlbCompany.CurrentCompany.Country.Code);
			Factory.Save();

			var invoiceNonRelative1 = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), "000031", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceNonRelative1.AH_OH = orgWithTransactionPosted.PK;

			var invoiceNonRelative2 = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), "000032", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceNonRelative2.AH_OH = orgWithTransactionPosted.PK;

			var invoiceWithLSCOrg = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), "000031", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceWithLSCOrg.AH_OH = orgWithLSCCode.PK;
			Factory.Save();

			var access = new TransactionPaymentDataAccess(Connection, Transaction);

			AccountingConfigurationRegistry.Instance.TransactionPaymentStatusWebServiceUsesSettlementGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(legacyOrgCode, "000031")))
			{
				AssertMatchTargetInvoice("Should match target invoice via LSC code", reader, invoiceWithLSCOrg);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(legacyOrgCode, "000032")))
			{
				AssertEquals("Should match no record because transaction was NOT posted with LSC org", false, reader.HasRows);
			}

			AccountingConfigurationRegistry.Instance.TransactionPaymentStatusWebServiceUsesSettlementGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(legacyOrgCode, "000031")))
			{
				AssertMatchTargetInvoice("Should match target invoice via LSC code", reader, invoiceWithLSCOrg);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(legacyOrgCode, "000032")))
			{
				AssertEquals("Should match no record because transaction was NOT posted with LSC org", false, reader.HasRows);
			}
		}

		public void TestGetTransactionsWithOrgRelatedPartyAndLegacySystemCode()
		{
			CreateTestData();

			var legacyOrgCode = "TTT";

			var parentOrg = TestObjectCreator.CreateOrgHeader("TS1", true, true);
			var relativeOrg = TestObjectCreator.CreateOrgHeader("TS11", true, true);
			relativeOrg.ConfigOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, legacyOrgCode, GlbCompany.CurrentCompany.Country.Code);
			TestObjectCreator.AddOrgRelatedParty(parentOrg, RelatedPartyTypeList.Codes.APSettlementGroup, relativeOrg, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoiceRelativeOrg1 = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), "000031", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceRelativeOrg1.AH_OH = parentOrg.PK;

			var invoiceRelativeOrg2 = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), "000032", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceRelativeOrg2.AH_OH = parentOrg.PK;
			Factory.Save();

			invoiceRelativeOrg2.AH_ConsolidatedInvoiceRef = "B123456";
			Factory.Save();

			var access = new TransactionPaymentDataAccess(Connection, Transaction);

			AccountingConfigurationRegistry.Instance.TransactionPaymentStatusWebServiceUsesSettlementGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(legacyOrgCode, "000031")))
			{
				AssertEquals("Should match no record because registry is off", false, reader.HasRows);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(legacyOrgCode, "000032")))
			{
				AssertEquals("Should match no record because registry is off", false, reader.HasRows);
			}

			AccountingConfigurationRegistry.Instance.TransactionPaymentStatusWebServiceUsesSettlementGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(legacyOrgCode, "000031")))
			{
				AssertMatchTargetInvoice("Should match target invoice: LSC code --> relative org --> parent org --> target invoice", reader, invoiceRelativeOrg1);
			}

			using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(legacyOrgCode, internalReference: "B123456")))
			{
				AssertMatchTargetInvoice("Should match target invoice: LSC code --> relative org --> parent org --> target invoice", reader, invoiceRelativeOrg2);
			}
		}

		public void TestGetTransactionsWithoutTransactionNumberNorConsolidatedInvoiceRef()
		{
			CreateTestData();

			var access = new TransactionPaymentDataAccess(Connection, Transaction);
			var request = GetRequest(testOrg.OH_Code);

			using (var reader = access.GetTransactionPaymentStatusReader(request))
			{
				AssertEquals("The data reader should NOT contain rows.", false, reader.HasRows);
			}
		}

		[SuspendCriticalValidation]
		public void TestGetTransactionPaymentStatusReaderLegacySystemCode()
		{
			GlbCompany aUCompany = TestObjectCreator.CreateNewCompany("AUC", Constants.CountryCodes.Australia);
			GlbBranch aUBranch = TestObjectCreator.CreateNewBranch(aUCompany, "AB1");
			GlbCompany nZCompany = TestObjectCreator.CreateNewCompany("NZC", Constants.CountryCodes.NewZealand);
			GlbBranch nZBranch = TestObjectCreator.CreateNewBranch(nZCompany, "NB1");

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode lSCAU = org.ConfigOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, "AU123", aUCompany.GC_RN_NKCountryCode);
			OrgCusCode lSCNZ = org.ConfigOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, "NZ456", nZCompany.GC_RN_NKCountryCode);

			Factory.Save();

			string transactionNum = TargetTransactionNum;
			using (new Environment.TemporaryUserContext() { BranchPK = aUBranch.PK.ToGuid() }.Set())
			{
				TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), transactionNum, TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, org, TestObjectCreator.CC1.PK);
				Factory.Save();

				TransactionPaymentDataAccess access = new TransactionPaymentDataAccess(Connection, Transaction);

				using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(lSCAU.OK_CustomsRegNo, transactionNum)))
				{
					Assert("AU company should match org because AU Legacy System Code was passed in", reader.HasRows);
				}

				using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(lSCNZ.OK_CustomsRegNo, transactionNum)))
				{
					Assert("AU company should not match org because NZ Legacy System Code was passed in", !reader.HasRows);
				}

				using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(org.OH_Code, transactionNum)))
				{
					Assert("AU company should match org because actual org code was passed in", reader.HasRows);
				}

				OrgHeader aUOrgWithSameCodeAsLecacy = Factory.NewWithValidTestData<OrgHeader>();
				aUOrgWithSameCodeAsLecacy.OH_Code = lSCAU.OK_CustomsRegNo;
				Factory.Save();

				using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(aUOrgWithSameCodeAsLecacy.OH_Code, transactionNum)))
				{
					Assert("AU company should match org code before Legacy Code", !reader.HasRows);
				}
			}

			using (new Environment.TemporaryUserContext() { BranchPK = nZBranch.PK.ToGuid() }.Set())
			{
				TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), transactionNum, TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, org, TestObjectCreator.CC1.PK);
				Factory.Save();

				TransactionPaymentDataAccess access = new TransactionPaymentDataAccess(Connection, Transaction);

				using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(lSCNZ.OK_CustomsRegNo, transactionNum)))
				{
					Assert("NZ company should match org because NZ Legacy System Code was passed in", reader.HasRows);
				}

				using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(lSCAU.OK_CustomsRegNo, transactionNum)))
				{
					Assert("NZ company should not match org because AU Legacy System Code was passed in", !reader.HasRows);
				}

				using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(org.OH_Code, transactionNum)))
				{
					Assert("NZ company should match org because actual org code was passed in", reader.HasRows);
				}

				OrgHeader nZOrgWithSameCodeAsLecacy = Factory.NewWithValidTestData<OrgHeader>();
				nZOrgWithSameCodeAsLecacy.OH_Code = lSCNZ.OK_CustomsRegNo;
				Factory.Save();

				using (var reader = access.GetTransactionPaymentStatusReader(GetRequest(nZOrgWithSameCodeAsLecacy.OH_Code, transactionNum)))
				{
					Assert("NZ company should match org code before Legacy Code", !reader.HasRows);
				}
			}
		}

		public void TestTryGetCompanyCurrencySubUnitRatio()
		{
			var company = TestObjectCreator.CreateNewCompany("ABC", Constants.CountryCodes.Australia);
			company.GC_RX_NKLocalCurrency = Constants.CurrencyCodes.Japan; //Japanese Yen - 0 decimal places
			Factory.Save();

			var dataAccess = new TransactionPaymentDataAccess(Connection, Transaction);

			AssertNull("TryGetCompanyCurrencySubUnitRatio based on an invalid company code should return null", dataAccess.TryGetCompanyCurrencySubUnitRatio("XXX"));

			AssertCompanyCurrencySubUnitRatio(1, company, dataAccess);

			var currencyWithOneDecimalPlace = Factory.New<RefCurrency>(); //No currency yet with 1 decimal place
			currencyWithOneDecimalPlace.RX_Code = "ONE";
			currencyWithOneDecimalPlace.RX_Desc = "Currency with one decimal place";
			currencyWithOneDecimalPlace.RX_SubUnitRatio = 10;
			company.GC_RX_NKLocalCurrency = currencyWithOneDecimalPlace.Code;
			Factory.Save();
			AssertCompanyCurrencySubUnitRatio(10, company, dataAccess);

			company.GC_RX_NKLocalCurrency = Constants.CurrencyCodes.UnitedStates; //US Dollar - 2 decimal places
			Factory.Save();
			AssertCompanyCurrencySubUnitRatio(100, company, dataAccess);

			company.GC_RX_NKLocalCurrency = Constants.CurrencyCodes.Kuwait; //Kuwait Dinar - 3 decimal places
			Factory.Save();
			AssertCompanyCurrencySubUnitRatio(1000, company, dataAccess);
		}

		void AssertCompanyCurrencySubUnitRatio(int expectedSubUnitRatio, GlbCompany company, TransactionPaymentDataAccess dataAccess)
		{
			AssertEquals(expectedSubUnitRatio, company.LocalCurrency.RX_SubUnitRatio);
			AssertEquals(expectedSubUnitRatio, dataAccess.TryGetCompanyCurrencySubUnitRatio(company.GC_Code));
		}

		TransactionPaymentStatusRequest GetRequest(string orgCode, string transactionNum = "", string jobTransactionNumber = "", string internalReference = "", string ledgerType = LedgerTypes.AccountsPayable)
		{
			return new TransactionPaymentStatusRequest()
			{
				AccLedger = ledgerType,
				CompanyCode = GlbCompany.CurrentCompany.GC_Code,
				OrgCode = orgCode,
				TransactionType = TransactionTypes.Invoice,
				TransactionNumber = transactionNum,
				JobTransactionNumber = jobTransactionNumber,
				InternalReference = internalReference,
			};
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
			Connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			Transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
		}

		void AssertMatchTargetInvoice(string assertMsg, System.Data.Common.DbDataReader reader, AccTransactionHeader targetInvoice)
		{
			int counter = 0;

			while (reader.Read())
			{
				counter++;
				AssertEquals(assertMsg, targetInvoice.PK, (Guid)reader["TransactionHeaderPK"]);
			}

			AssertEquals("Should match only one record", 1, counter);
		}

		void PrepareOrgRelatedPartyTestData(out OrgHeader relativeOrg1, out OrgHeader relativeOrg2, out OrgHeader orgWithTransactionPosted,
			out InvoicingBase invoiceRelativeOrg1, out InvoicingBase invoiceRelativeOrg2AP, out InvoicingBase invoiceRelativeOrg2AR, out InvoicingBase invoiceHeaderOrg, bool isSelfRelated = false)
		{
			CreateTestData();

			orgWithTransactionPosted = TestObjectCreator.CreateOrgHeader("TS0", true, true);

			var parentOrg1 = TestObjectCreator.CreateOrgHeader("TS1", true, true);
			relativeOrg1 = isSelfRelated ? parentOrg1 : TestObjectCreator.CreateOrgHeader("TS11", true, true);
			TestObjectCreator.AddOrgRelatedParty(parentOrg1, RelatedPartyTypeList.Codes.APSettlementGroup, relativeOrg1, GlbCompany.CurrentCompany.PK);

			var parentOrg2 = TestObjectCreator.CreateOrgHeader("TS2", true, true);
			relativeOrg2 = isSelfRelated ? parentOrg2 : TestObjectCreator.CreateOrgHeader("TS21", true, true);
			TestObjectCreator.AddOrgRelatedParty(parentOrg2, RelatedPartyTypeList.Codes.ARSettlementGroup, relativeOrg2, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			invoiceRelativeOrg1 = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), "000031", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceRelativeOrg1.AH_OH = parentOrg1.PK;

			invoiceRelativeOrg2AP = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), "000032", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceRelativeOrg2AP.AH_OH = parentOrg2.PK;

			invoiceHeaderOrg = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), "000033", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceHeaderOrg.AH_OH = orgWithTransactionPosted.PK;

			invoiceRelativeOrg2AR = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(ARInvoice), "000034", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceRelativeOrg2AR.IsManuallySetTransactionNumber_ForTestOnly = true;
			invoiceRelativeOrg2AR.AH_ConsolidatedInvoiceRef = "B123456";
			invoiceRelativeOrg2AR.AH_OH = parentOrg2.PK;

			Factory.Save();
		}

		void CreateTestData()
		{
			testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.ConfigOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, "AU123", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			Factory.Save();

			invoiceAP = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), TargetTransactionNum, TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);

			invoiceRefAP = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), "SetupRefAP", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			Factory.Save();
			invoiceRefAP.AH_ConsolidatedInvoiceRef = TargetConsolidatedInvoiceRef;  // ConsolidatedInvoiceRef of AP transactions were auto-evaluated when save new.
			Factory.Save();

			invoiceAR = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(ARInvoice), "SetupAR", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceAR.AH_ConsolidatedInvoiceRef = TargetConsolidatedInvoiceRef;
			invoiceAR.IsManuallySetTransactionNumber_ForTestOnly = true;

			var invoiceNonRelativeAP = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), "Setup04", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);

			var invoiceNonRelativeRefAR = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(ARInvoice), "Setup05", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceNonRelativeRefAR.AH_ConsolidatedInvoiceRef = "A00002";
			invoiceNonRelativeRefAR.IsManuallySetTransactionNumber_ForTestOnly = true;

			var creditNoteAP = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APCreditNote), TargetTransactionNum, TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			Factory.Save();
			creditNoteAP.AH_ConsolidatedInvoiceRef = TargetConsolidatedInvoiceRef;
			Factory.Save();

			var creditNoteAR = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(ARCreditNote), TargetTransactionNum, TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			creditNoteAR.AH_ConsolidatedInvoiceRef = TargetConsolidatedInvoiceRef;
			creditNoteAR.IsManuallySetTransactionNumber_ForTestOnly = true;

			var invoiceNonCurrentBranch = TestObjectCreator.CreateInvoiceWithGLHeaderLine(typeof(APInvoice), TargetTransactionNum, TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, testOrg, TestObjectCreator.GLHeader1.PK);
			invoiceNonCurrentBranch.AH_GC = invoiceNonCurrentBranch.Lines[0].AL_GC = TestObjectCreator.NonCurrentCompany.PK;
			invoiceNonCurrentBranch.AH_GB = invoiceNonCurrentBranch.Lines[0].AL_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			Factory.Save();
			invoiceNonCurrentBranch.AH_ConsolidatedInvoiceRef = TargetConsolidatedInvoiceRef;
			Factory.Save();
		}

		TestObjectCreator TestObjectCreator { get; set; }
		System.Data.Common.DbConnection Connection { get; set; }
		System.Data.Common.DbTransaction Transaction { get; set; }

		OrgHeader testOrg;
		InvoicingBase invoiceAP, invoiceRefAP, invoiceAR;

		const string TargetTransactionNum = "000001";
		const string TargetConsolidatedInvoiceRef = "A00001";

		#endregion
	}
}
