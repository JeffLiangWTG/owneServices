using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.EInvoicing.Egypt.Testing
{
	sealed class InvoicingBaseToXmlConverterTest : TestCaseWithFactory
	{
		[TestDate(2021, 5, 5, 17, 51, 3)]
		public void TestConvertInvoiceAUD()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Egypt))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Constants.CurrencyCodes.Egypt))
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				SetupControlAccounts();
				SetupBranch();
				TestObjectCreator.ABIGAS.MainAddress.OA_State = "QLD";
				TestObjectCreator.ABIGAS.MainAddress.OA_City = "MAYNE";
				TestObjectCreator.ABIGAS.MainAddress.Address2 = "Unit 3a";

				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
				var rate = job.ExchangeRates.AddNew();
				rate.JF_RX_NKRateCurrency = TestObjectCreator.AUD.Code;
				rate.JF_BaseRate = 0.5m;

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge", TestObjectCreator.AUD, 200m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, TestObjectCreator.ABIGAS);
				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "1001", TestObjectCreator.AUD, 0.5m, TestObjectCreator.ABIGAS);
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
				arInvoice.Lines[0].AL_GovtChargeCode = "GOVCC";

				Factory.Save();

				AssertConverter(arInvoice, "InvoiceAUD.xml");
			}
		}

		[TestDate(2021, 5, 5, 17, 51, 3)]
		public void TestConvertInvoiceEGP()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Egypt))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Constants.CurrencyCodes.Egypt))
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				SetupControlAccounts();
				SetupBranch();
				SetupABIGAS();

				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge 1", TestObjectCreator.EGP, 200m, TestObjectCreator.Creditor1, TestObjectCreator.EGP, 200m, TestObjectCreator.ABIGAS);
				charge1.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				var taxMsg = TestObjectCreator.CreateTaxMsg("MSGE01", "Tax Message Description", "Tax Message with E01 Group", "Tax Message with E01 Group");
				taxMsg.A9_TaxGroupCode = "E01";
				charge1.JR_A9_SellVATClass = taxMsg.PK;
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge 2", TestObjectCreator.EGP, 150m, TestObjectCreator.Creditor1, TestObjectCreator.EGP, 150m, TestObjectCreator.ABIGAS);

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "1001", TestObjectCreator.EGP, 1m, TestObjectCreator.ABIGAS);
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, arInvoice.PK));
				arInvoice.Lines[0].AL_A9_VATClass = taxMsg.PK;
				arInvoice.Lines[0].AL_GovtChargeCode = "GOVCC1";
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, arInvoice.PK));
				arInvoice.Lines[1].AL_Sequence = 2; // to ensure the correct order
				arInvoice.Lines[1].AL_GovtChargeCode = "GOVCC2";

				Factory.Save();

				AssertConverter(arInvoice, "InvoiceEGP.xml");
			}
		}

		[TestDate(2021, 5, 5, 17, 51, 3)]
		public void TestConvertCreditNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Egypt))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Constants.CurrencyCodes.Egypt))
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				SetupControlAccounts();
				SetupBranch();
				SetupABIGAS();

				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge 1", TestObjectCreator.EGP, -200m, TestObjectCreator.Creditor1, TestObjectCreator.EGP, -200m, TestObjectCreator.ABIGAS);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge 2", TestObjectCreator.EGP, -100m, TestObjectCreator.Creditor1, TestObjectCreator.EGP, -100m, TestObjectCreator.ABIGAS);
				charge1.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				charge2.JR_AT_SellGSTRate = TestObjectCreator.GST2.PK;

				var taxMsg1 = TestObjectCreator.CreateTaxMsg("MSGE01", "Tax Message Description", "Tax Message with E01 Group", "Tax Message with E01 Group");
				taxMsg1.A9_TaxGroupCode = "E01";
				var taxMsg2 = TestObjectCreator.CreateTaxMsg("MSGE02", "Tax Message Description", "Tax Message with E02 Group", "Tax Message with E02 Group");
				taxMsg2.A9_TaxGroupCode = "E02";
				charge1.JR_A9_SellVATClass = taxMsg1.PK;
				charge2.JR_A9_SellVATClass = taxMsg2.PK;

				var arCreditNote = TestObjectCreator.CreateARCreditNote("1001", TestObjectCreator.ABIGAS, TestObjectCreator.EGP, 1m);
				var line1 = TestObjectCreator.CreateARCreditNoteLine(arCreditNote, job, TestObjectCreator.CC1, 200m, TestObjectCreator.EGP, 1m, "charge 1");
				var line2 = TestObjectCreator.CreateARCreditNoteLine(arCreditNote, job, TestObjectCreator.CC1, 100m, TestObjectCreator.EGP, 1m, "charge 2");
				charge1.JR_AL_ARLine = line1.PK;
				charge2.JR_AL_ARLine = line2.PK;
				line1.AL_AT = charge1.JR_AT_SellGSTRate;
				line2.AL_AT = charge2.JR_AT_SellGSTRate;
				line1.AL_A9_VATClass = charge1.JR_A9_SellVATClass;
				line2.AL_A9_VATClass = charge2.JR_A9_SellVATClass;
				line1.AL_GovtChargeCode = "GOVCC1";
				line2.AL_GovtChargeCode = "GOVCC2";

				Factory.Save();

				AssertConverter(arCreditNote, "CreditNote.xml");
			}
		}

		[TestDate(2021, 5, 5, 17, 51, 3)]
		public void TestConvertVariousTransactions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Egypt))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Constants.CurrencyCodes.Egypt))
			{
				SetupControlAccounts();
				SetupBranch();
				SetupABIGAS();

				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge", TestObjectCreator.EGP, 200m, TestObjectCreator.Creditor1, TestObjectCreator.EGP, 200m, TestObjectCreator.ABIGAS);

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "1001", TestObjectCreator.EGP, 1m, TestObjectCreator.ABIGAS);
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
				Factory.Save();

				var amendingCreditNote = (arInvoice as IAmending)?.GenerateAmendingTransaction<ARCreditNote>();
				AssertNotNull(amendingCreditNote);
				amendingCreditNote.AH_ReceiptType = "IDE";
				AssertEquals(arInvoice.AH_OSTotal, -amendingCreditNote.AH_OSTotal);
				Factory.Save();

				AssertConverterFailed(amendingCreditNote, "Missing government UUID for the original transaction of the AR CRD 00001000");

				var authRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
				authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
				authRecord.AHF_ParentId = arInvoice.PK;
				authRecord.AHF_RecordType = AccTransactionHeaderAuthorisationRecordTypes.Egypt;
				authRecord.AHF_Number = "ABC1234567412";
				Factory.Save();

				AssertConverter(amendingCreditNote, "AmendingCreditNote.xml");

				var reverser = new ReversingFactory().NewReversing(amendingCreditNote);
				reverser.Reverse();
				amendingCreditNote.ReverseInvoice.AH_TransactionNum = "REVAMEND1001";
				amendingCreditNote.ReverseInvoice.AH_ReceiptType = "IDE";

				authRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
				authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
				authRecord.AHF_ParentId = amendingCreditNote.PK;
				authRecord.AHF_RecordType = AccTransactionHeaderAuthorisationRecordTypes.Egypt;
				authRecord.AHF_Number = "CCD98765430147";
				Factory.Save();

				AssertConverter(amendingCreditNote.ReverseInvoice, "ReversingInvoice.xml");

				reverser = new ReversingFactory().NewReversing(arInvoice);
				reverser.Reverse();
				arInvoice.ReverseInvoice.AH_TransactionNum = "REVINV1001";
				arInvoice.ReverseInvoice.AH_ReceiptType = "WOR";
				Factory.Save();

				AssertConverter(arInvoice.ReverseInvoice, "ReversingCreditNote.xml");
			}
		}

		void AssertConverter(InvoicingBase transaction, string fileName)
		{
			var converter = new InvoicingBaseToXmlConverter();
			var notifications = new NotificationBuffer();

			var result = converter.ConvertToXml(transaction, notifications);
			Assert(result.Success);
			Assert("No errors", !notifications.HasErrors);
			AssertXml(result.Xml, fileName);
		}

		void AssertConverterFailed(InvoicingBase transaction, string errorMessage)
		{
			var converter = new InvoicingBaseToXmlConverter();
			var notifications = new NotificationBuffer();

			var result = converter.ConvertToXml(transaction, notifications);
			Assert("Failed", !result.Success);
			Assert("Has errors", notifications.HasErrors);
			AssertContains(errorMessage, notifications.AsString);
		}

		void AssertXml(string actualXml, string xmlFile)
		{
			var file = embeddedResourceRetriever.SaveResourceToFile(xmlFile);

			using (var streamReader = File.OpenText(file))
			{
				var expectedXml = streamReader.ReadToEnd();
				streamReader.Close();

				this.AssertXMLEqualsByDiff("Converted XML should match XML file", expectedXml, actualXml);
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		void SetupControlAccounts()
		{
			AccGLHeader aRSuspenseControlAccount = TestObjectCreator.CreateARSuspenseControlAccount();
			AccGLHeader aPSuspenseControlAccount = TestObjectCreator.CreateAPSuspenseControlAccount();
			AccGLHeader jobRevenueJournalControlAccount = TestObjectCreator.CreateJobRevenueJournalControlAccount();
			AccGLHeader cFXAccount = TestObjectCreator.CreateCFXAccount();
			AccGLHeader pendingInputTaxAccount = null;
			AccGLHeader pendingOutputTaxAccount = null;
			if (GlbCompany.CurrentCompany.GC_IsGSTCashBasis)
			{
				pendingInputTaxAccount = TestObjectCreator.CreateInputTaxReceivablePendingAccount();
				pendingOutputTaxAccount = TestObjectCreator.CreateOutputTaxPayablePendingAccount();
			}
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cFXAccount.PK.ToGuid());
			if (pendingInputTaxAccount != null)
			{
				AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingInputTaxAccount.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingOutputTaxAccount.PK.ToGuid());
			}
		}

		void SetupBranch()
		{
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			branch.OrgProxy.OH_RL_NKClosestPort = "EGCAI"; // Setting UNLOCO is the only way to switch Country
			var address = branch.OrgProxy.MainAddress;
			address.OA_City = "Cairo";
			address.OA_State = "C";
			var registrations = branch.OrgProxy.CustomsCodes;
			registrations.AddNew(OrgCusCode.CodeTypes.CorporationCode, "BRNGCR0001", Constants.CountryCodes.Egypt);
			registrations.AddNew(OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber, "BRNCOM0001", Constants.CountryCodes.Egypt);
			registrations.AddNew(OrgCusCode.CodeTypes.GovBusinessCode, "BRNGBR0001", Constants.CountryCodes.Egypt);
		}

		void SetupABIGAS()
		{
			TestObjectCreator.ABIGAS.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Egypt;
			TestObjectCreator.ABIGAS.MainAddress.OA_State = "lx";
			TestObjectCreator.ABIGAS.MainAddress.OA_City = "Luxor";
			TestObjectCreator.ABIGAS.MainAddress.Address1 = "2 Pharaon Dr";
			TestObjectCreator.ABIGAS.MainAddress.Address2 = "";
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();

			embeddedResourceRetriever = new ();
		}

		protected override void TearDown()
		{
			base.TearDown();

			embeddedResourceRetriever?.Dispose();
			embeddedResourceRetriever = null;
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;
		#endregion
	}
}
