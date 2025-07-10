using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ForwardingShipment = Enterprise.Freight.Forwarding.Business.ForwardingShipment;

namespace Enterprise.Customs.CA.Business.Testing
{
	public static class TransactionNumberTestHelper
	{
		const string DefaultASECNumberForTest = "32450";

		public static IDisposable SetupCompanyASECNumberForTest(string securityNo = DefaultASECNumberForTest, Guid? companyPk = null)
		{
			companyPk = companyPk ?? GlbCompany.CurrentCompany.PK.ToGuid();
			return CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(companyPk.Value, Guid.Empty, Guid.Empty, securityNo);
		}

		public static void SetupTransactionNumberFountainForTest(BusinessObjectFactory factory, string securityNo = DefaultASECNumberForTest, long minValue = 1, long maxValue = 99999999)
		{
			var numberFountainKey = TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator(securityNo, null, null));

			if (Globals.IsTest)
			{
				Enterprise.ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess(Env.NumberFountains.GetKey("C", numberFountainKey), Guid.Empty);
			}

			var numberFountain = TransactionNumber.GetNumberFountain(numberFountainKey);
			numberFountain.SetValues(factory, minValue: minValue, nextValue: minValue, maxValue: maxValue);
		}
	}

	[TestedType(typeof(TransactionNumber))]
	sealed class TransactionNumberTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetSequentialNumber_DoNotReuseManuallyEnteredTransactionNumber()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				declaration1.TransactionNumber.SequentialNumber = ZString.Empty;
				Factory.Save();
				AssertEquals("00000001", declaration1.TransactionNumber.SequentialNumber);

				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				declaration2.TransactionNumber.SequentialNumber = "00000002";
				Factory.Save();

				var declaration3 = Factory.New<JobDeclaration>();
				declaration3.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				declaration3.TransactionNumber.SequentialNumber = ZString.Empty;
				Factory.Save();
				AssertEquals("Manually entered transaction number 00000002 from another declaration should not be used.", "00000003", declaration3.TransactionNumber.SequentialNumber);
			}
		}

		public void TestGetSequentialNumber_DoNotSilentlyOverrideManuallyEnteredTransactionNumber()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				declaration1.TransactionNumber.SequentialNumber = "00000002";
				Factory.Save();
				AssertEquals("00000002", declaration1.TransactionNumber.SequentialNumber);

				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				declaration2.TransactionNumber.SequentialNumber = "00000002";
				var ex = AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());
				AssertContains("already used in B00001000", ex.Message);
			}
		}

		public void TestGetSequentialNumber_NoSecurityNumberIsSet()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, null);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			TransactionNumber transactionNumber = declaration.TransactionNumber;
			AssertEquals("pre-condition", ZString.Empty, transactionNumber.AccountSecurityCode);
			AssertEquals("pre-condition", TransactionNumber.SequentialNumberDefault, transactionNumber.SequentialNumber);

			Factory.Save();
			AssertEquals("sequential number should not be generated when ASEC number is not set", TransactionNumber.SequentialNumberDefault, transactionNumber.SequentialNumber);
			AssertEquals("sequential number should not be generated when ASEC number is not set", TransactionNumber.SequentialNumberDefault, declaration.TransactionNumber.SequentialNumber);
		}

		public void TestGetSequentialNumber_FailIfFountainIsNotSetup()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "76389");

			var buyerOrg = Factory.New<OrgHeader>();
			buyerOrg.OH_Code = "INBUYER";
			Factory.Save();

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			var fakeDec = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			invoice.JZ_OH_Buyer = buyerOrg.PK;
			AssertNoExceptionThrown(() => Factory.Save());

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "76319";

			AssertEquals("pre-condition", "76319", declaration.TransactionNumber.AccountSecurityCode);
			AssertEquals("pre-condition", TransactionNumber.SequentialNumberDefault, declaration.TransactionNumber.SequentialNumber);

			var ex = AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());
			AssertContains("Transaction number range for ASEC Number 76319 is not set.", ex.Message);
		}

		public void TestGetSequentialNumber_FailIfFountainHasDriedUp()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("13759"))
			{
				CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "13759");

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var fountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey("13759" + declaration.Branch.PK + "IMP"));
				fountain.SetValues(Factory, minValue: 1, nextValue: 10000001, maxValue: 10000100);

				var numberFountain = TransactionNumber.GetNumberFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("13759", declaration.Branch, "IMP")));
				long lastAllocatedNumber = 0;
				for (int i = 0; i < 100; i++)
				{
					lastAllocatedNumber = numberFountain.GetNext(Factory);
				}

				var fountainDefault = Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey("13759"));
				fountainDefault.SetValues(Factory, minValue: 1, nextValue: 10000001, maxValue: 10000100);
				for (int i = 0; i < 100; i++)
				{
					lastAllocatedNumber = fountainDefault.GetNext(Factory);
				}

				AssertEquals("pre-condition", 10000100, lastAllocatedNumber);
				AssertEquals("pre-condition", "13759", declaration.TransactionNumber.AccountSecurityCode);
				AssertEquals("pre-condition", TransactionNumber.SequentialNumberDefault, declaration.TransactionNumber.SequentialNumber);
				declaration.TransactionNumber.SequentialNumber = "10000100";

				AssertHasError(declaration.TransactionNumber.FormattedTransactionNumberInfo, TransactionNumberMessages.TransactionNumberRangeIsEmpty());
			}
		}

		public void TestGetSequentialNumber_SkipGenerationForEmptySequentialNumber()
		{
			// this test covers scenario from RegularProcessingServiceTaskTest, that initializes empty transaction number for declaration that already have messages

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntrySubmittedDate = new ZDateTime(2016, 8, 1);

			var entryNumber = declaration.AdditionalReferenceNumbers.AddNew();
			entryNumber.CE_EntryNum = "CCN123 456";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = "REL";
			entryHeader.CH_Status = "AWO";
			entryHeader.CH_EntrySubmittedDate = new ZDateTime(2016, 8, 1);

			var message = entryHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageType = "REL";
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-3);

			AssertEquals("pre-condition", 1, entryHeader.Messages.Count);
			AssertEquals("pre-condition", false, declaration.TransactionNumber.CanChange);
			AssertEquals("pre-condition", ZString.Empty, declaration.TransactionNumber.AccountSecurityCode);
			AssertEquals("pre-condition", ZString.Empty, declaration.TransactionNumber.SequentialNumber);

			Factory.Save();
			AssertEquals(ZString.Empty, declaration.TransactionNumber.SequentialNumber);
		}

		public void TestTransactionNumberForIM2()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				Factory.Save();
				AssertEquals("00000001", declaration.TransactionNumber.SequentialNumber);

				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				Factory.Save();
				AssertEquals("00000002", declaration.TransactionNumber.SequentialNumber);
			}
		}

		public void TestTransactionNumberForB3X()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				Factory.Save();
				AssertEquals("00000001", declaration.TransactionNumber.SequentialNumber);

				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				Factory.Save();
				AssertEquals("00000002", declaration.TransactionNumber.SequentialNumber);
			}
		}

		public void TestTransactionNumberIsDeletedWhenChangedFromImport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.Validation.ValidateAll(); //Expose TransactionNumber;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			Factory.Save();
			AssertNull(CusEntryNumber.Load(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada));
		}

		public void TestTransactionNumberIsNotDeletedWhenMessagingIsActiveWhenImport()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				Factory.RefreshEnabled = false;
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.TransactionNumber.Validation.ValidateAll(); //Expose TransactionNumber;
				var entry = declaration.ActiveEntryHeaders.AddNew();
				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(factory2);
				factory2.RefreshEnabled = false;
				var declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
				declaration2.TransactionNumber.Validation.ValidateAll(); //Expose TransactionNumber;
				Assert("touch messages collections in second factory", !declaration2.DeclarationMessagesHaveBeenSent());

				var message = entry.Messages.AddNew();
				Factory.Save();
				AssertNotNull(CusEntryNumber.Load(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.ResetMessageTypeChangeLogs();
				Factory.Save();
				AssertNull(CusEntryNumber.Load(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada));
			}
		}

		public void TestFrezeeingAccountSecurityNumberWhenStartMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("Can change", declaration.TransactionNumber.CanChange);
			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			Assert("Can change", declaration.TransactionNumber.CanChange);
			entry1.Messages.AddNew();
			Assert("Can NOT change", !declaration.TransactionNumber.CanChange);

			declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			Assert("Can change", declaration.TransactionNumber.CanChange);
			entry1 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry1.Messages.AddNew();
			Assert("Can NOT change", !declaration.TransactionNumber.CanChange);
		}

		public void TestUseImporterAccountSecurityNumber_SeparateInputs()
		{
			TestUseImporterAccountSecurityNumber(true);
		}

		public void TestUseImporterAccountSecurityNumber_SingleInput()
		{
			TestUseImporterAccountSecurityNumber(false);
		}

		void TestUseImporterAccountSecurityNumber(bool separateTxInputs)
		{
			using (CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "54321"))
			using (CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFGH"))
			using (CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, separateTxInputs))
			{
				var importer = Factory.New<OrgHeader>();
				var addInfo = OrgImpAddInfo.Get(importer);
				addInfo.ZO_AccountSecurityNumber = "12345";
				addInfo.ZO_AccountSecirityPassword = "12345678";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				declaration.CA_UseImporterAccountSecurityNumber = false;
				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("Broker's Acc Security No", "54321", declaration.TransactionNumber.AccountSecurityCode);
				AssertEquals("Broker's Acc Security Password", "ABCDEFGH", declaration.TransactionNumber.AccountSecurityPassword);

				declaration.CA_UseImporterAccountSecurityNumber = true;
				AssertEquals("Importer's Acc Security No", "12345", declaration.TransactionNumber.AccountSecurityCode);
				AssertEquals("Broker's Acc Security Password", "12345678", declaration.TransactionNumber.AccountSecurityPassword);

				declaration.CA_UseImporterAccountSecurityNumber = false;
				AssertEquals("Broker's Acc Security No", "54321", declaration.TransactionNumber.AccountSecurityCode);
				AssertEquals("Broker's Acc Security Password", "ABCDEFGH", declaration.TransactionNumber.AccountSecurityPassword);
			}
		}

		public void TestDeleteTransactionWhenNotIsImportIncludingB2()
		{
			var testHelper = new DeclarationTestHelper(Factory, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntrySubmittedDate = new ZDateTime(2025, 1, 1);
			testHelper.CreateTransportLeg(declaration.Transports, testHelper.AUBNE.Code, testHelper.AUMEL.Code, ZDateTime.Now, ZDateTime.Now.AddDays(1));
			var importLeg = testHelper.CreateTransportLeg(declaration.Transports, testHelper.AUMEL.Code, testHelper.CATOR.Code, ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(3));
			testHelper.CreateTransportLeg(declaration.Transports, testHelper.CATOR.Code, testHelper.CAVAR.Code, ZDateTime.Now.AddDays(4), ZDateTime.Now.AddDays(5));

			var firstInvoice = declaration.Invoices.AddNew();
			AssertEquals("CA_RL_NKLastPort", testHelper.AUMEL.Code, firstInvoice.CA_RL_NKLastPort);
			AssertEquals("JZ_ValuationDateOverride", ZDateTime.TruncateToDay(importLeg.JW_ETD), firstInvoice.JZ_ValuationDateOverride);
			AssertEquals("CA_TreatmentCode", "02", firstInvoice.CA_TreatmentCode);

			var shipment = Factory.New<ForwardingShipment>();
			testHelper.CreateTransportLeg(shipment.Transports, testHelper.AUBNE.Code, testHelper.AUMEL.Code, ZDateTime.Now, ZDateTime.Now.AddDays(1));
			importLeg = testHelper.CreateTransportLeg(shipment.Transports, testHelper.AUMEL.Code, testHelper.CATOR.Code, ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(3));
			testHelper.CreateTransportLeg(shipment.Transports, testHelper.CATOR.Code, testHelper.CAVAR.Code, ZDateTime.Now.AddDays(4), ZDateTime.Now.AddDays(5));
			declaration.JE_JS = shipment.PK;

			var importer = testHelper.CreateOrganisation("IMPORTER NAME", "CATOR");
			var importerAddInfo = OrgImpAddInfo.Get(importer);
			importerAddInfo.ZO_AccountSecurityNumber = "30215";
			importerAddInfo.ZO_AccountSecirityPassword = "1234567";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = importer.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000m;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = "REL";
			entryHeader.CH_Status = "AWO";
			entryHeader.CH_EntrySubmittedDate = new ZDateTime(2016, 8, 1);

			var entryNumber = declaration.AdditionalReferenceNumbers.AddNew();
			entryNumber.CE_EntryNum = "CCN123 444";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			var entryNumber2 = declaration.AdditionalReferenceNumbers.AddNew();
			entryNumber2.CE_EntryNum = "CTN123 999";
			entryNumber2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CTN;

			var entryNumber3 = declaration.AdditionalReferenceNumbers.AddNew();
			entryNumber3.CE_EntryNum = "CCN123 777";
			entryNumber3.CE_EntryType = "REL";

			var message = entryHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageType = "REL";
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-3);

			AssertEquals("pre-condition", 1, entryHeader.Messages.Count);
			AssertEquals("pre-condition", false, declaration.TransactionNumber.CanChange);
			AssertEquals("pre-condition", ZString.Empty, declaration.TransactionNumber.AccountSecurityCode);
			AssertEquals("pre-condition", ZString.Empty, declaration.TransactionNumber.SequentialNumber);

			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_GoodsDescription = "changed";
			AssertEquals("REL type Before Save()", true, declaration.AdditionalReferenceNumbers.OfType<CusEntryNumber>().Any(x => x.CE_EntryType == "REL"));

			Factory.Save();

			AssertEquals("REL type deleted", false, declaration.AdditionalReferenceNumbers.OfType<CusEntryNumber>().Any(x => x.CE_EntryType == "REL"));
		}

		public void TestForceManualInputOfTransactionNumber()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var dec = Factory.NewWithValidTestData<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				CACustomsDataRegistry.Instance.ForceManualInputOfTransactionNumber.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				using (dec.SuspendValidationTesting())
				{
					Factory.Save();
					AssertEquals("Sequential number is not allocated on save", TransactionNumber.SequentialNumberDefault, dec.TransactionNumber.SequentialNumber);
				}

				CACustomsDataRegistry.Instance.ForceManualInputOfTransactionNumber.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				foreach (var messageType in new[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.LowValueShipments, JobMessageTypeList.Codes.B2Adjustments })
				{
					dec = Factory.NewWithValidTestData<JobDeclaration>();
					dec.JE_MessageType = messageType;
					AssertEquals(TransactionNumber.SequentialNumberDefault, dec.TransactionNumber.SequentialNumber);
					Factory.Save();
					AssertNotEquals(TransactionNumber.SequentialNumberDefault, dec.TransactionNumber.SequentialNumber);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCalculateTransactionNumber_SeparateInputs()
		{
			TestCalculateTransactionNumber(true);
		}

		[UseSnapshotProtection]
		public void TestCalculateTransactionNumber_SingleInput()
		{
			TestCalculateTransactionNumber(false);
		}

		void TestCalculateTransactionNumber(bool separateTxInputs)
		{
			using (CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, separateTxInputs))
			{
				var helper = new DeclarationTestHelper(Factory, true);
				var importer = helper.CreateOrganisation("IMPORTER NAME", "CATOR");
				var importerAddInfo = OrgImpAddInfo.Get(importer);
				importerAddInfo.ZO_AccountSecurityNumber = "30215";
				importerAddInfo.ZO_AccountSecirityPassword = "1234567";
				var importer2 = helper.CreateOrganisation("IMPORTER NAME2", "CATOR");
				var importerAddInfo2 = OrgImpAddInfo.Get(importer2);
				importerAddInfo2.ZO_AccountSecurityNumber = "54321";
				importerAddInfo2.ZO_AccountSecirityPassword = "1234567";
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var company = Factory.New<GlbCompany>();
				company.GC_Code = "CA1";
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				var branch = Factory.New<GlbBranch>();
				branch.GB_Code = "GB1";
				branch.GB_GC = company.PK;
				declaration.JE_GB = branch.PK;

				using (CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, separateTxInputs))
				using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345"))
				{
					TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

					//AccountSecurityCode is obtained from registry
					AssertTransactionNumber(declaration, "12345000000000");

					//AccountSecurityCode is obtained from importer
					declaration.CA_UseImporterAccountSecurityNumber = true;
					declaration.JE_OH_Importer = importer.PK;
					declaration.TransactionNumber.SequentialNumber = "00001234";
					AssertTransactionNumber(declaration, "30215000012346");

					//AccountSecurityCode is obtained from importer of record
					declaration.ImporterOfRecordAddress.OrganisationPK = importer2.PK;
					declaration.TransactionNumber.SequentialNumber = "00001234";
					AssertTransactionNumber(declaration, "54321000012345");
					declaration.ImporterOfRecordAddress.OrganisationPK = Guid.Empty;

					//CheckDigid when AccountSecurityCode invalid
					importerAddInfo.ZO_AccountSecurityNumber = ZString.Empty;
					importerAddInfo.ZO_AccountSecirityPassword = ZString.Empty;
					declaration.TransactionNumber.AccountSecurityCode = "123";
					AssertEquals("CheckDigit", 0, declaration.TransactionNumber.CheckDigit);

					//CheckDigid when SequentialNumber invalid
					declaration.TransactionNumber.SequentialNumber = "0000123";
					AssertEquals("CheckDigit", 0, declaration.TransactionNumber.CheckDigit);

					//TransactionNumber is editable child of Declaration
					declaration.TransactionNumber.SetAccountSecurityNo();
					Factory.Save();
					AssertEquals("Pre-Condition: Declaration.HasChanges", false, declaration.HasChanges);
					declaration.TransactionNumber.SetAccountSecurityNo();
					declaration.TransactionNumber.SequentialNumber = "12345678";
					AssertEquals("Declaration.HasChanges", true, declaration.HasChanges);

					//TransactionNumber doesn't set has changes on load
					Factory.Save();
					AssertTransactionNumber(declaration, "12345123456785");
					var declaration1 = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
					AssertEquals("TransactionNumber.HasChanges", false, declaration1.TransactionNumber.HasChanges);
					AssertEquals("Declaration.HasChanges", false, declaration1.HasChanges);
					AssertTransactionNumber(declaration, "12345123456785");

					//TransactionNumber stored in CusEntryNumber
					var transactionNumber = CusEntryNumber.Load(declaration1, CusEntryNumber.EntryType.CATransactionNumber, declaration1.CountryCode);
					AssertEquals("TransactionNumber should be stored in CusEntryNumber", declaration1.TransactionNumber.ToString(), transactionNumber.CE_EntryNum);

					TestConnection.BeginTransaction();
					try
					{
						//SequentialNumber generated on saving if it is default
						declaration.TransactionNumber.AccountSecurityCode = ZString.Empty;
						Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator(declaration.TransactionNumber.AccountSecurityCode, branch, "IMP"))).SetNext(Factory, 6789);
						declaration.TransactionNumber.SequentialNumber = ZString.Empty;
						Factory.Save();
						AssertTransactionNumber(declaration, "12345000067897");

						//SequentialNumber generated to keep TransactionNumber unique
						Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator(declaration1.TransactionNumber.AccountSecurityCode, branch, "IMP"))).SetNext(Factory, 6789);
						declaration1 = Factory.New<JobDeclaration>();
						declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
						declaration1.TransactionNumber.SequentialNumber = ZString.Empty;
						declaration1.JE_GB = branch.PK;
						Factory.Save();
						AssertTransactionNumber(declaration1, "12345000067900");
					}
					finally
					{
						TestConnection.RollbackTransaction();
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestTransactionNumberFountain()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

				var company = Factory.New<GlbCompany>();
				company.GC_Code = "CA1";
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				var branch1 = Factory.New<GlbBranch>();
				branch1.GB_Code = "GB1";
				branch1.GB_GC = company.PK;
				var branch2 = Factory.New<GlbBranch>();
				branch2.GB_Code = "GB2";
				branch2.GB_GC = company.PK;

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				Factory.Save();
				AssertEquals("00000001", declaration.TransactionNumber.SequentialNumber);

				Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("12345", null, null))).SetNext(Factory, 5);
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				Factory.Save();
				AssertEquals("00000005", declaration.TransactionNumber.SequentialNumber);

				Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("12345", branch1, "IMP"))).SetNext(Factory, 8);
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				declaration.JE_GB = branch1.PK;
				Factory.Save();
				AssertEquals("00000008", declaration.TransactionNumber.SequentialNumber);

				Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("12345", branch1, "LVS"))).SetNext(Factory, 10);
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				declaration.JE_GB = branch1.PK;
				Factory.Save();
				AssertEquals("00000010", declaration.TransactionNumber.SequentialNumber);

				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				declaration.JE_GB = branch1.PK;
				Factory.Save();
				AssertEquals("00000011", declaration.TransactionNumber.SequentialNumber);

				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				declaration.JE_GB = branch1.PK;
				Factory.Save();
				AssertEquals("00000006", declaration.TransactionNumber.SequentialNumber);

				var importer = Factory.New<OrgHeader>();
				var addInfo = OrgImpAddInfo.Get(importer);
				importer.OH_Code = "IM1";
				addInfo.ZO_AccountSecurityNumber = "23456";
				addInfo.ZO_AccountSecirityPassword = "222222";
				Factory.Save();

				Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("23456", branch2, "IMP"))).SetNext(Factory, 12);
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_UseImporterAccountSecurityNumber = true;
				declaration.JE_OH_Importer = importer.PK;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				declaration.JE_GB = branch2.PK;
				Factory.Save();
				AssertEquals("00000012", declaration.TransactionNumber.SequentialNumber);

				using (DisposableEnvironment.ForBranch(branch2.GB_Code))
				using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("67890", companyPk: company.PK.ToGuid()))
				{
					TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "67890", minValue: 777);
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.TransactionNumber.SequentialNumber = ZString.Empty;
					Factory.Save();
					AssertEquals("00000777", declaration.TransactionNumber.SequentialNumber);
					Env.NumberFountains.GetCAEntryNumberGeneratorFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("67890", branch2, "IMP"))).SetNext(Factory, 20);
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.CA_UseImporterAccountSecurityNumber = true;
					declaration.TransactionNumber.SequentialNumber = ZString.Empty;
					declaration.JE_GB = branch2.PK;
					Factory.Save();
					AssertEquals("00000020", declaration.TransactionNumber.SequentialNumber);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestIsUnique()
		{
			var numberFountain = TransactionNumber.GetNumberFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("16907", null, null)));
			numberFountain.SetValues(Factory, minValue: 10_000_000, nextValue: 0, maxValue: 19_999_999);

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.TransactionNumber.AccountSecurityCode = "16907";
			declaration1.TransactionNumber.SequentialNumber = "00007044";
			Factory.Save();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.TransactionNumber.AccountSecurityCode = "16907";
			declaration2.TransactionNumber.SequentialNumber = "00007044";
			AssertEquals("TransactionNumber", "16907000070447", declaration2.TransactionNumber.ToString());
			Assert("IsUnique", !declaration2.TransactionNumber.IsUnique);
			declaration2.TransactionNumber.AccountSecurityCode = "92247";
			declaration2.TransactionNumber.SequentialNumber = "00007044";
			AssertEquals("TransactionNumber", "92247000070447", declaration2.TransactionNumber.ToString());
			Assert("IsUnique", declaration2.TransactionNumber.IsUnique);
		}

		public void TestAllocateFormattedTransactionNumber()
		{
			using (CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (CACustomsDataRegistry.Instance.ForceManualInputOfTransactionNumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("54325"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "54325", 10000000, 19999999);

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.TransactionNumber.Validation.ValidateAll(); //Expose TransactionNumber;
				AssertEquals("SequentialNumber before save", "00000000", declaration.TransactionNumber.SequentialNumber);
				AssertEquals("FormattedTransactionNumber before save", ZString.Empty, declaration.TransactionNumber.FormattedTransactionNumber);
				Factory.Save();
				AssertEquals("AccountSecurityCode after save", "54325", declaration.TransactionNumber.AccountSecurityCode);
				AssertEquals("SequentialNumber after save", "00000000", declaration.TransactionNumber.SequentialNumber);
				AssertEquals("FormattedTransactionNumber after save", ZString.Empty, declaration.TransactionNumber.FormattedTransactionNumber);

				using (CACustomsDataRegistry.Instance.ForceManualInputOfTransactionNumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("SequentialNumber before save", "00000000", declaration.TransactionNumber.SequentialNumber);
					AssertEquals("FormattedTransactionNumber before save", ZString.Empty, declaration.TransactionNumber.FormattedTransactionNumber);
					Factory.Save();
					AssertEquals("AccountSecurityCode after save", "54325", declaration.TransactionNumber.AccountSecurityCode);
					AssertEquals("SequentialNumber after save", "10000000", declaration.TransactionNumber.SequentialNumber);
					AssertEquals("FormattedTransactionNumber after save", "54325100000007", declaration.TransactionNumber.FormattedTransactionNumber);

					declaration.TransactionNumber.SequentialNumber = ZString.Empty;
					declaration.TransactionNumber.FormattedTransactionNumber = ZString.Empty;
				}

				using (CACustomsDataRegistry.Instance.ForceManualInputOfTransactionNumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("SequentialNumber before save", "00000000", declaration.TransactionNumber.SequentialNumber);
					AssertEquals("FormattedTransactionNumber before save", ZString.Empty, declaration.TransactionNumber.FormattedTransactionNumber);
					Factory.Save();
					AssertEquals("AccountSecurityCode after save", "54325", declaration.TransactionNumber.AccountSecurityCode);
					AssertEquals("SequentialNumber after save", "10000001", declaration.TransactionNumber.SequentialNumber);
					AssertEquals("FormattedTransactionNumber after save", "54325100000018", declaration.TransactionNumber.FormattedTransactionNumber);

					declaration.TransactionNumber.FormattedTransactionNumber = ZString.Empty;
					AssertEquals("SequentialNumber before save", "00000000", declaration.TransactionNumber.SequentialNumber);
					AssertEquals("FormattedTransactionNumber before save", ZString.Empty, declaration.TransactionNumber.FormattedTransactionNumber);
					Factory.Save();
					AssertEquals("AccountSecurityCode after save", "54325", declaration.TransactionNumber.AccountSecurityCode);
					AssertEquals("SequentialNumber after save", "10000002", declaration.TransactionNumber.SequentialNumber);
					AssertEquals("FormattedTransactionNumber after save", "54325100000029", declaration.TransactionNumber.FormattedTransactionNumber);
				}
			}
		}

		public void TestCanBeChangedOrDeleted()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.TransactionNumber.SequentialNumber = ZString.Empty;
				Factory.Save();

				Assert(declaration.TransactionNumber.CanBeChangedOrDeleted(out var _));

				var newFactory = new BusinessObjectFactory();
				var reloadDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
				var entryHeader = reloadDeclaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				entryHeader.Messages.AddNew();

				newFactory.Save();

				Assert(!declaration.TransactionNumber.CanBeChangedOrDeleted(out var _));
			}
		}

		public void TestEntryNumberChanged()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				declaration.TransactionNumber.SequentialNumber = "00001796";
				Factory.Save();
				Assert(!declaration.TransactionNumber.EntryNumberChanged);

				declaration.TransactionNumber.SequentialNumber = "00001797";
				Assert(declaration.TransactionNumber.EntryNumberChanged);
			}
		}

		public void TestPropertiesReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var transactionNumber = declaration.TransactionNumber;
			AssertEquals(true, transactionNumber.AccountSecurityCodeInfo.ReadOnly);
			AssertEquals(!transactionNumber.CanChange, transactionNumber.SequentialNumberInfo.ReadOnly);
			AssertEquals(!transactionNumber.CanChange, transactionNumber.FormattedTransactionNumberInfo.ReadOnly);
		}

		void AssertTransactionNumber(JobDeclaration declaration, ZString tranNumber)
		{
			AssertEquals("TransactionNumber", tranNumber, declaration.TransactionNumber.ToString());
			AssertEquals("TransactionNumber", tranNumber.Right(9), declaration.TransactionNumber.UniqueIdentifier);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TransactionNumber(Factory.New<JobDeclaration>());
		}

		#endregion
	}
}
