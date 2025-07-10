using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Accounting.ElectronicMessaging.China.Testing
{
	class ChinaEInvoicePayloadCreatorTest : TestCaseWithFactory
	{
		public void TestCreatePayloadAsJson()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var invoice = SetupInvoice();
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("11111"), TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
				var taxRate = invoice.Lines[0].AL_AT;

				var discountedLine = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "discounted line", 50m);
				var discountLine = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "discount line", -10m);

				var discountedLineWithAnotherJob = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "discounted line 2", 500m);
				var discountLineWithAnotherJob = TestObjectCreator.CreateARInvoiceLine((ARInvoice)invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "discount line 2", -100m);

				discountedLine.AL_AT = taxRate;
				discountLine.AL_AT = taxRate;
				discountedLineWithAnotherJob.AL_AT = taxRate;
				discountLineWithAnotherJob.AL_AT = taxRate;

				invoice.Lines[0].AL_Sequence = 1;
				discountedLine.AL_Sequence = 2;
				discountLine.AL_Sequence = 3;
				discountedLineWithAnotherJob.AL_Sequence = 2;
				discountLineWithAnotherJob.AL_Sequence = 3;

				invoice.Lines.Add(discountedLine);
				invoice.Lines.Add(discountLine);
				invoice.Lines.Add(discountedLineWithAnotherJob);
				invoice.Lines.Add(discountLineWithAnotherJob);

				var creator = new ChinaEInvoicePayloadCreator(invoice) as IChinaPayloadCreator;

				AssertNotNull(creator);

				AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.SetValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				var generatedJson = creator.CreatePayloadAsJson();

				AssertPayload(invoice, generatedJson);

				AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.SetValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				generatedJson = creator.CreatePayloadAsJson();

				AssertPayload(invoice, generatedJson);
			}
		}

		public void TestCreatePayloadAsJson_ShouldHaveForeignCurrencyBankAccount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var invoice = SetupInvoice();
				var creator = new ChinaEInvoicePayloadCreator(invoice) as IChinaPayloadCreator;

				var aRAccount = invoice.Branch.OrgProxy.CompanyData.ARAccountDetailsCollection.AddNew();
				aRAccount.A1_IsDefaultAccount = true;
				aRAccount.A1_PaymentMethod = AccARAccountDetails.ARBankAccPayment;
				aRAccount.A1_RX_NKAccountCurrency = invoice.AH_RX_NKTransactionCurrency;
				aRAccount.A1_BankName = "TestBank";
				aRAccount.A1_BankAccount = "TestAccount";
				aRAccount.A1_RN_NKCountryCode = Constants.CountryCodes.China;

				var generatedJson = creator.CreatePayloadAsJson();
				var eInvoiceJson = (JObject)JsonConvert.DeserializeObject(generatedJson);
				var dataJson = (JObject)eInvoiceJson["data"];
				var sellerJson = (JObject)dataJson["seller"];

				AssertEquals("bank", "TestBank", (string)sellerJson["bank"]);
				AssertEquals("bankAcc", "TestAccount", (string)sellerJson["bankAcc"]);
			}
		}

		public void TestRemarkMapping()
		{
			var remarkCofig = new ZStringBuilder();
			remarkCofig.Append("[Master Bill： <ConsolMasterBill> ~ ]");
			remarkCofig.Append("[House Bill: <ShipmentHouseBill> ~ ]");
			remarkCofig.Append("[Vessel: <ConsolVessel> ] ~ ");
			remarkCofig.Append("[Voyage/Flight: <ConsolVoyFlt> ~ ]");
			remarkCofig.Append("[Load Port: <ConsolLoadPort> ]");
			remarkCofig.Append("[Discharge Port: <ConsolDischargePort> ~ ]");
			remarkCofig.Append("[Job Invoice Number: <JobInvNumber> ~ ]");
			remarkCofig.Append("[Invoie Amount: <InvoiceAmount> ~ ]");
			remarkCofig.Append("[Invoice Currency: <InvoiceCurrency> ~ ]");
			remarkCofig.Append("[Transaction Number: <TransNumber> ~ ]");
			remarkCofig.Append("[ExchangeRate： <InvoiceExchangeRate> ~ ]");
			remarkCofig.Append("[Operator Full Name: <JobOperatorFullName> ~ ]");
			remarkCofig.Append("[Operator Preferred Name: <JobOperatorPreferredName> ~ ]");
			remarkCofig.Append("[Sales Rep Full Name: <JobSalesRepFullName> ~ ]");
			remarkCofig.Append("[Sales Rep Preferred Name: <JobSalesRepPreferredName> ~ ]");
			remarkCofig.Append("[Transaction Description: <TransDescription> ~ ]");
			remarkCofig.Append("[Estimate Time of Departure: <ConsolETD> ~ ]");
			remarkCofig.Append("[Estimate Time of Arrival: <ConsolETA>]");

			using (var stream = new MemoryStream())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			using (AccountingConfigurationRegistry.Instance.AccountingWebServiceRemarks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, remarkCofig.ToString()))
			{
				var invoice = GetInvoiceForRemark();
				var remark = GetRemark(invoice);

				var isRemarkAsExpected = remark.Equals("Master Bill： M001/M002 ~ House Bill: H001 ~ Vessel: Vessel1/Vessel2  ~ Voyage/Flight: Flight1/Flight2 ~ Load Port: AUBNE/AUSYD Discharge Port: AUSYD/NZAKL ~ Job Invoice Number: TST_REF ~ Invoie Amount: 100.00 ~ Invoice Currency: AUD ~ Transaction Number: 00001000 ~ ExchangeRate： 1.000000 ~ Operator Full Name: Operational Full Name ~ Operator Preferred Name: Operational Friendly Name ~ Sales Rep Full Name: Sale Full Name ~ Sales Rep Preferred Name: Sale Friendly Name ~ Transaction Description: Test Invoice ~ Estimate Time of Departure: 2017-01-02 00:00:00/2017-01-04 00:00:00 ~ Estimate Time of Arrival: 2017-01-03 00:00:00/2017-01-05 00:00:00");
				isRemarkAsExpected = isRemarkAsExpected || remark.Equals("Master Bill： M002/M001 ~ House Bill: H001 ~ Vessel: Vessel2/Vessel1  ~ Voyage/Flight: Flight2/Flight1 ~ Load Port: AUSYD/AUBNE Discharge Port: NZAKL/AUSYD ~ Job Invoice Number: TST_REF ~ Invoie Amount: 100.00 ~ Invoice Currency: AUD ~ Transaction Number: 00001000 ~ ExchangeRate： 1.000000 ~ Operator Full Name: Operational Full Name ~ Operator Preferred Name: Operational Friendly Name ~ Sales Rep Full Name: Sale Full Name ~ Sales Rep Preferred Name: Sale Friendly Name ~ Transaction Description: Test Invoice ~ Estimate Time of Departure: 2017-01-04 00:00:00/2017-01-02 00:00:00 ~ Estimate Time of Arrival: 2017-01-05 00:00:00/2017-01-03 00:00:00");

				Assert("Test full elements", isRemarkAsExpected);
			}
		}

		public void TestRemarkMapping_BranchLevel()
		{
			using (var stream = new MemoryStream())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var invoice = GetInvoiceForRemark();

				var remarkCofig = new ZStringBuilder();
				remarkCofig.Append("System level");
				AccountingConfigurationRegistry.Instance.AccountingWebServiceRemarks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, remarkCofig.ToString());

				remarkCofig = new ZStringBuilder();
				remarkCofig.Append("Company level");
				AccountingConfigurationRegistry.Instance.AccountingWebServiceRemarks.SetValue(invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, remarkCofig.ToString());

				remarkCofig = new ZStringBuilder();
				remarkCofig.Append("branch level");
				AccountingConfigurationRegistry.Instance.AccountingWebServiceRemarks.SetValue(Guid.Empty, invoice.AH_GB.ToGuid(), Guid.Empty, remarkCofig.ToString());

				var remark = GetRemark(invoice);

				Assert("Test full elements", remark.Equals("branch level"));
			}
		}

		public void TestLengthOfRemark()
		{
			var warningTestLogger = new NotificationBuffer();
			var errorTestLogger = new NotificationBuffer();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var invoice = GetInvoiceForRemark();

				var invalidLengthOfStr = @"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345";
				AssertGreaterThan(invalidLengthOfStr.Length, 400);

				AssertLengthOfRemark(invoice, invalidLengthOfStr);

				var validLengthOfStr = @"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234";
				AssertEquals(validLengthOfStr.Length, 400);

				AssertLengthOfRemark(invoice, validLengthOfStr);
			}

			void AssertLengthOfRemark(InvoicingBase invoice, string remarkStr)
			{
				var jsonResourceName = "Enterprise.Accounting.ElectronicMessaging.China.EInvoice.SendInvoice.ChinaEInvoiceSchema.json";

				var remarkConfig = new ZStringBuilder();
				remarkConfig.Append(remarkStr);
				AccountingConfigurationRegistry.Instance.AccountingWebServiceRemarks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, remarkConfig.ToString());

				var creator = new ChinaEInvoicePayloadCreator(invoice) as IChinaPayloadCreator;
				var generatedJson = creator.CreatePayloadAsJson();
				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(generatedJson)))
				{
					var xsdValidation = new Common.DataValidation.JsonValidation(jsonResourceName);
					xsdValidation.Validate(stream, errorTestLogger, warningTestLogger);

					if (remarkStr.Length > 400)
					{
						AssertContains("exceeds maximum length of 400.", errorTestLogger.AsString);
					}
					else
					{
						AssertNotContains("exceeds maximum length of 400.", errorTestLogger.AsString);
					}

					errorTestLogger.Clear();
				}
			}
		}

		InvoicingBase GetInvoiceForRemark()
		{
			var saleStaff = Factory.NewWithValidTestData<GlbStaff>();
			saleStaff.GS_IsSalesRep = true;
			saleStaff.GS_FullName = "Sale Full Name";
			saleStaff.GS_FriendlyName = "Sale Friendly Name";
			var opStaff = Factory.NewWithValidTestData<GlbStaff>();
			opStaff.GS_IsOperational = true;
			opStaff.GS_FullName = "Operational Full Name";
			opStaff.GS_FriendlyName = "Operational Friendly Name";

			var consol1 = TestObjectCreator.CreateConsol("AUBNE", "AUSYD", "C001");
			consol1.JK_MasterBillNum = "M001";

			var transport1 = consol1.Transports[0];
			transport1.JW_IsLinked = false;
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_Vessel = "Vessel1";
			transport1.JW_VoyageFlight = "Flight1";
			transport1.JW_ETD = new ZDateTime(2017, 1, 2, 0, 0, 0, DateTimeKind.Utc);
			transport1.JW_ETA = new ZDateTime(2017, 1, 3, 0, 0, 0, DateTimeKind.Utc);

			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C002");
			consol2.JK_MasterBillNum = "M002";

			var transport2 = consol2.Transports[0];
			transport2.JW_IsLinked = false;
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "Vessel2";
			transport2.JW_VoyageFlight = "Flight2";
			transport2.JW_ETD = new ZDateTime(2017, 1, 4, 0, 0, 0, DateTimeKind.Utc);
			transport2.JW_ETA = new ZDateTime(2017, 1, 5, 0, 0, 0, DateTimeKind.Utc);

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "AUSYD", consol1);
			shipment.Consols.Add(consol2);
			shipment.JS_E_DEP = new ZDateTime(2017, 12, 11, 0, 0, 0, DateTimeKind.Utc);
			shipment.JS_E_ARV = new ZDateTime(2017, 12, 25, 0, 0, 0, DateTimeKind.Utc);
			shipment.JS_HouseBill = "H001";

			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0,
				TestObjectCreator.Agent, 0);
			job.JH_GS_NKRepOps = opStaff.GS_Code;
			job.JH_GS_NKRepSales = saleStaff.GS_Code;

			var invoice = SetupInvoice();
			invoice.AH_PostDate = new ZDateTime(2009, 06, 16);
			invoice.AH_JH = job.PK;
			invoice.AH_ConsolidatedInvoiceRef = "TST_REF";

			Factory.Save();

			return invoice;
		}

		string GetRemark(InvoicingBase invoice)
		{
			var creator = new ChinaEInvoicePayloadCreator(invoice) as IChinaPayloadCreator;

			AssertNotNull(creator);

			var actualJson = creator.CreatePayloadAsJson();

			var eInvoiceJson = (JObject)JsonConvert.DeserializeObject(actualJson);
			var remark = (string)eInvoiceJson["data"]["order"]["remark"];

			return remark;
		}

		public void TestCreatePayloadAsJson_NotContainCommentLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var invoice = SetupInvoice();
				var creator = new ChinaEInvoicePayloadCreator(invoice) as IChinaPayloadCreator;

				AssertNotNull(creator);

				var generatedJson = creator.CreatePayloadAsJson();

				var eInvoice = JsonConvert.DeserializeObject<ChinaEInvoice>(generatedJson);
				AssertEquals("OrderDetails is not empty", true, eInvoice.Data.Order.OrderDetails.Any());

				invoice.Lines[0].ChargeCode.AC_ChargeType = Constants.ChargeType.Comment;
				creator = new ChinaEInvoicePayloadCreator(invoice);

				AssertNotNull(creator);

				generatedJson = creator.CreatePayloadAsJson();

				eInvoice = JsonConvert.DeserializeObject<ChinaEInvoice>(generatedJson);
				AssertEquals("OrderDetails is empty because only have comment line.", false, eInvoice.Data.Order.OrderDetails.Any());
			}
		}

		public void TestCreatePayload_SubTypeFDAFDB()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var invoice = SetupInvoice();
				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA;

				var creator = new ChinaEInvoicePayloadCreator(invoice) as IChinaPayloadCreator;

				AssertNotNull(creator);

				var generatedJson = creator.CreatePayloadAsJson();
				var eInvoiceJson = (JObject)JsonConvert.DeserializeObject(generatedJson);
				var dataJson = (JObject)eInvoiceJson["data"];

				AssertEquals("invType", "13", (string)dataJson["invType"]);

				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
				creator = new ChinaEInvoicePayloadCreator(invoice);

				AssertNotNull(creator);

				generatedJson = creator.CreatePayloadAsJson();
				eInvoiceJson = (JObject)JsonConvert.DeserializeObject(generatedJson);
				dataJson = (JObject)eInvoiceJson["data"];

				AssertEquals("invType", "14", (string)dataJson["invType"]);
			}
		}

		public void TestCreatePayload_DoNotSendTheseInfomationForFullyDigitalizedEInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var invoice = SetupInvoice();
				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA;
				AssertExcludedData(invoice, "", "133099822", "Bank name1", "Bank account1");

				AccountingConfigurationRegistry.Instance.DoNotSendTheseInfomationForFullyDigitalizedEInvoice.SetValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new ExcludedFullyDigitalizedElectronicInvoiceData() { BuyerPhoneNumber = true, BuyerBankAccount = true });
				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
				AssertExcludedData(invoice, "state1-CN city1 address1 address2", "", "", "");

				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
				AssertExcludedData(invoice, "state1-CN city1 address1 address2", "133099822", "Bank name1", "Bank account1");

				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				AssertExcludedData(invoice, "state1-CN city1 address1 address2", "133099822", "Bank name1", "Bank account1");

				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;
				AssertExcludedData(invoice, "state1-CN city1 address1 address2", "133099822", "Bank name1", "Bank account1");

				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
				AssertExcludedData(invoice, "state1-CN city1 address1 address2", "133099822", "Bank name1", "Bank account1");

				void AssertExcludedData(InvoicingBase invoice, string address, string telephoneNo, string bank, string bankAcc)
				{
					var creator = new ChinaEInvoicePayloadCreator(invoice) as IChinaPayloadCreator;

					AssertNotNull(creator);

					var generatedJson = creator.CreatePayloadAsJson();
					var eInvoiceJson = (JObject)JsonConvert.DeserializeObject(generatedJson);
					var dataJson = (JObject)eInvoiceJson["data"];
					var orderJson = (JObject)dataJson["order"];
					var buyerJson = (JObject)orderJson["buyer"];

					AssertEquals(address, (string)buyerJson["address"]);
					AssertEquals(telephoneNo, (string)buyerJson["telephoneNo"]);
					AssertEquals(bank, (string)buyerJson["bank"]);
					AssertEquals(bankAcc, (string)buyerJson["bankAcc"]);
				}
			}
		}

		public void TestCreatePayload_Autoexec()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var invoice = SetupInvoice();
				AssertChinaTransmitAndIssueFapiao(true, "1");
				AssertChinaTransmitAndIssueFapiao(false, "0");

				void AssertChinaTransmitAndIssueFapiao(bool chinaTransmitAndIssueFapiao, string expectAutoexec)
				{
					using (AccountingMasterFilesRegistry.Instance.ChinaTransmitAndIssueFapiao.SetTemporaryValue(Guid.Empty, invoice.Branch.PK.ToGuid(), Guid.Empty, chinaTransmitAndIssueFapiao))
					{
						var creator = new ChinaEInvoicePayloadCreator(invoice) as IChinaPayloadCreator;

						AssertNotNull(creator);

						var generatedJson = creator.CreatePayloadAsJson();
						var eInvoiceJson = (JObject)JsonConvert.DeserializeObject(generatedJson);

						AssertEquals("autoexec", expectAutoexec, (string)eInvoiceJson["autoexec"]);
					}
				}
			}
		}

		InvoicingBase SetupInvoice()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV0001", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;

			TestObjectCreator.CC1.AC_LocalLanguageDescription = "费用";

			var testBranch = TestObjectCreator.CreateBranch("TBN", GlbCompany.CurrentCompany, TestObjectCreator.Debtor1);
			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetValue(Guid.Empty, testBranch.PK.ToGuid(), Guid.Empty, "0001");

			TestObjectCreator.Debtor1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345678901234567890", Constants.CountryCodes.China);
			TestObjectCreator.Debtor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789012345", Constants.CountryCodes.China);

			var testTaxRate = CreateTaxRate("EXEMPT", "EXT", 0, 1, Core.Constants.CountryCodes.China);

			invoice.AH_GB = testBranch.PK;
			invoice.AH_OH = TestObjectCreator.Debtor.PK;
			invoice.Lines[0].AL_AT = testTaxRate.PK;

			var refCountryStates = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates.RW_Code = "ST1";
			refCountryStates.RW_RN_NKCountryCode = "CN";
			refCountryStates.RW_Description = "state1-CN";

			var address = TestObjectCreator.Debtor.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables);
			address.Language = "ZH-CN";
			address.OA_CompanyNameOverride = "Test company name/\"";
			address.OA_State = refCountryStates.RW_Code;
			address.OA_City = "city1";
			address.OA_Address1 = "address1";
			address.OA_Address2 = "address2";
			address.OA_Phone = "133099822";
			address.OA_Mobile = "18795898160";
			address.OA_Email = "12345678@qq.com";

			var account = TestObjectCreator.Debtor.CompanyData.ARAccountDetailsCollection.AddNew();
			account.A1_IsDefaultAccount = true;
			account.A1_PaymentMethod = AccARAccountDetails.ARBankAccPayment;
			account.A1_RN_NKCountryCode = Constants.CountryCodes.China;
			account.A1_RX_NKAccountCurrency = Constants.CurrencyCodes.China;
			account.A1_BankName = "Bank name1";
			account.A1_BankAccount = "Bank account1";

			return invoice;
		}

		void AssertPayload(InvoicingBase invoice, string generatedJson)
		{
			var eInvoiceJson = (JObject)JsonConvert.DeserializeObject(generatedJson);

			AssertEquals("reqType", "02", (string)eInvoiceJson["reqType"]);
			AssertEquals("TaxNo", "12345678901234567890", (string)eInvoiceJson["taxNo"]);
			AssertEquals("ClientNo", "0001", (string)eInvoiceJson["clientNo"]);

			var dataJson = (JObject)eInvoiceJson["data"];

			AssertEquals("serialNumber", invoice.PK.ToString() + "|EDIEDIDAT", (string)dataJson["serialNumber"]);
			AssertEquals("extend", "PDF", (string)dataJson["extend"]);
			AssertEquals("invType", "11", (string)dataJson["invType"]);
			AssertEquals("version", "", (string)dataJson["version"]);
			AssertEquals("drawer", "", (string)dataJson["drawer"]);
			AssertEquals("payee", "", (string)dataJson["payee"]);
			AssertEquals("reviewer", "", (string)dataJson["reviewer"]);

			var sellerJson = (JObject)dataJson["seller"];

			AssertEquals("identifier", "", (string)sellerJson["identifier"]);
			AssertEquals("name", "", (string)sellerJson["name"]);
			AssertEquals("address", "", (string)sellerJson["address"]);
			AssertEquals("telephoneNo", "", (string)sellerJson["telephoneNo"]);
			AssertEquals("bank", "", (string)sellerJson["bank"]);
			AssertEquals("bankAcc", "", (string)sellerJson["bankAcc"]);

			var orderJson = (JObject)dataJson["order"];

			AssertEquals("orderNo", "INV0001", (string)orderJson["orderNo"]);
			AssertEquals("invoiceList", "", (string)orderJson["invoiceList"]);
			AssertEquals("invoiceSplit", "0", (string)orderJson["invoiceSplit"]);
			AssertEquals("invoiceSfdy", "0", (string)orderJson["invoiceSfdy"]);
			AssertEquals("orderDate", invoice.AH_PostDate.ToString("yyyy-MM-dd HH:mm:ss"), (string)orderJson["orderDate"]);
			AssertEquals("chargeTaxWay", "0", (string)orderJson["chargeTaxWay"]);
			AssertEquals("totalAmount", "540.0", (string)orderJson["totalAmount"]);
			AssertEquals("taxMark", "0", (string)orderJson["taxMark"]);
			AssertEquals("totalDiscount", "", (string)orderJson["totalDiscount"]);
			AssertEquals("remark", "", (string)orderJson["remark"]);
			AssertEquals("extractedCode", "", (string)orderJson["extractedCode"]);

			var buyerJson = (JObject)orderJson["buyer"];

			AssertEquals("customerType", "1", (string)buyerJson["customerType"]);
			AssertEquals("identifier", "123456789012345", (string)buyerJson["identifier"]);
			AssertEquals("name", "Test company name/\"", (string)buyerJson["name"]);
			AssertEquals("address", "state1-CN city1 address1 address2", (string)buyerJson["address"]);
			AssertEquals("telephoneNo", "133099822", (string)buyerJson["telephoneNo"]);
			AssertEquals("bank", "Bank name1", (string)buyerJson["bank"]);
			AssertEquals("bankAcc", "Bank account1", (string)buyerJson["bankAcc"]);
			AssertEquals("email", "12345678@qq.com", (string)buyerJson["email"]);
			AssertEquals("memberId", "", (string)buyerJson["memberId"]);
			AssertEquals("isSend", "1", (string)buyerJson["isSend"]);
			AssertEquals("mobilePhone", null, (string)buyerJson["mobilePhone"]);
			AssertEquals("recipient", "", (string)buyerJson["recipient"]);
			AssertEquals("reciAddress", "", (string)buyerJson["reciAddress"]);
			AssertEquals("zip", "", (string)buyerJson["zip"]);

			var orderDetail = ((JArray)orderJson["orderDetails"])[0];
			AssertEquals("venderOwnCode", "ZZCC1", (string)orderDetail["venderOwnCode"]);
			AssertEquals("productCode", "", (string)orderDetail["productCode"]);
			AssertEquals("productName", "费用", (string)orderDetail["productName"]);
			AssertEquals("rowType", "0", (string)orderDetail["rowType"]);
			AssertEquals("spec", "", (string)orderDetail["spec"]);
			AssertEquals("unit", "", (string)orderDetail["unit"]);
			AssertEquals("quantity", "1", (string)orderDetail["quantity"]);
			AssertEquals("unitPrice", "100", (string)orderDetail["unitPrice"]);
			AssertEquals("amount", "100", (string)orderDetail["amount"]);
			AssertEquals("deductAmount", "", (string)orderDetail["deductAmount"]);
			AssertEquals("taxRate", "0", (string)orderDetail["taxRate"]);
			AssertEquals("taxAmount", "0", (string)orderDetail["taxAmount"]);
			AssertEquals("mxTotalAmount", "100", (string)orderDetail["mxTotalAmount"]);
			AssertEquals("taxRateMark", "1", (string)orderDetail["taxRateMark"]);
			AssertEquals("policyMark", "1", (string)orderDetail["policyMark"]);
			AssertEquals("policyName", "免税", (string)orderDetail["policyName"]);

			var alwaysTransmitNegativeChargesAsDiscount = AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.GetFallBackValueAtAllLevels(invoice.Company.PK.ToGuid(), invoice.Branch.PK.ToGuid(), Guid.Empty);
			var orderDetailDiscountedLine = ((JArray)orderJson["orderDetails"])[1];
			var orderDetailDiscountLine = ((JArray)orderJson["orderDetails"])[2];
			AssertEquals("rowType", alwaysTransmitNegativeChargesAsDiscount ? "2" : "0", (string)orderDetailDiscountedLine["rowType"]);
			AssertEquals("rowType", alwaysTransmitNegativeChargesAsDiscount ? "1" : "0", (string)orderDetailDiscountLine["rowType"]);

			var oderDetailDiscountedLineWithAnotherJob = ((JArray)orderJson["orderDetails"])[3];
			var orderDetailDiscountLineWithAnotherJob = ((JArray)orderJson["orderDetails"])[4];
			AssertEquals("rowType", alwaysTransmitNegativeChargesAsDiscount ? "2" : "0", (string)oderDetailDiscountedLineWithAnotherJob["rowType"]);
			AssertEquals("rowType", alwaysTransmitNegativeChargesAsDiscount ? "1" : "0", (string)orderDetailDiscountLineWithAnotherJob["rowType"]);
		}

		AccTaxRate CreateTaxRate(string code, string type, int rateNum, int rateDenom, string country)
		{
			var query = new ZQuery(AccTaxRateSchema.AT_Code, code);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, country);
			var result = Factory.LoadTop1<AccTaxRate>(query);

			if (result == null)
			{
				result = new BusinessObjectFactory().New<AccTaxRate>();
				result.AT_Code = code;
				result.AT_Description = code + " Desc";
				result.AT_IsActive = true;
				result.AT_RN_NKCountry = country;
				result.AT_Type = type;
				result.SetRate_ForTestOnly(rateNum, rateDenom);
				result.Factory.Save();
				result = Factory.Load<AccTaxRate>(result.PK);
			}
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;
	}
}
