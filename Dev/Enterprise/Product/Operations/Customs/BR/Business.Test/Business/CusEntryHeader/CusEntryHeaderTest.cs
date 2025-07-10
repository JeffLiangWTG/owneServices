using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		#region TypeSafeCusEntryHeader Tests

		public void TestEntryInstruction()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			Assert(declaration.CustomsEntryInstructions.Contains(entry.EntryInstruction));
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			base.DoMerge(declaration);
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		}

		#endregion

		public void TestCanSendRectification()
		{
			CusEntryHeader oHeader = Factory.New<CusEntryHeader>();
			oHeader.CH_EntryStatus = ZString.Empty;
			oHeader.EntryNumber = "123";
			AssertEquals("HasBeenLodgedAtCustoms is true", true, oHeader.HasBeenLodgedAtCustoms);
			AssertEquals("Can be send rectification", true, oHeader.CanSendRectification);
			oHeader.CH_EntryStatus = "EST";
			oHeader.EntryNumber = ZString.Empty;
			AssertEquals("HasBeenLodgedAtCustoms is true", true, oHeader.HasBeenLodgedAtCustoms);
			AssertEquals("Cannot be send rectification", false, oHeader.CanSendRectification);
			oHeader.CH_EntryStatus = ZString.Empty;
			oHeader.EntryNumber = ZString.Empty;
			AssertEquals("HasBeenLodgedAtCustoms is false", false, oHeader.HasBeenLodgedAtCustoms);
			AssertEquals("Cannot be send rectification", false, oHeader.CanSendRectification);
			oHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			AssertEquals("HasBeenLodgedAtCustoms is false", false, oHeader.HasBeenLodgedAtCustoms);
			AssertEquals("Cannot be send rectification", false, oHeader.CanSendRectification);
		}

		public void TestUniqueConsignmentReference()
		{
			var entry = Factory.New<CusEntryHeader>();
			var entryNum = CusEntryNumber.New(entry, CusEntryNumberTypes.Standard.UniqueConsignementReference, Core.Constants.CountryCodes.Brazil);
			entryNum.CE_EntryNum = "TESTUCR";
			AssertEquals("TESTUCR", entry.UniqueConsignmentReference);
			var cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(EntryNumberQueryGenerator.GetEntryNumberByEntryTypeQuery(entry.PK, "BR", CusEntryNumberTypes.Standard.UniqueConsignementReference, true));
			AssertNotNull(cusEntryNumber);
			entry.UniqueConsignmentReference = string.Empty;
			cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(EntryNumberQueryGenerator.GetEntryNumberByEntryTypeQuery(entry.PK, "BR", CusEntryNumberTypes.Standard.UniqueConsignementReference, true));
			AssertNull(cusEntryNumber);
		}

		public void TestCargoStatusAndDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.CH_CargoStatus = BRCargoStatusList.Codes.CargoFullyExported;
			AssertEquals("CH_CargoStatus", BRCargoStatusList.Codes.CargoFullyExported, header.CH_CargoStatus);
			AssertEquals("CargoStatusDescription", BRCargoStatusList.Descriptions.CargoFullyExported, header.CargoStatusDescription);
			header.CH_CargoStatus = BRCargoStatusList.Codes.Stored;
			AssertEquals("CH_CargoStatus", BRCargoStatusList.Codes.Stored, header.CH_CargoStatus);
			AssertEquals("CargoStatusDescription", BRCargoStatusList.Descriptions.Stored, header.CargoStatusDescription);
			header.CH_CargoStatus = BRCargoStatusList.Codes.InTransit;
			AssertEquals("CH_CargoStatus", BRCargoStatusList.Codes.InTransit, header.CH_CargoStatus);
			AssertEquals("CargoStatusDescription", BRCargoStatusList.Descriptions.InTransit, header.CargoStatusDescription);
		}

		public void TestCH_AuthorityVersion()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.CH_AuthorityVersion = "0001";
			AssertEquals("CH_AuthorityVersion", "0001", header.CH_AuthorityVersion);
			header.CH_AuthorityVersion = ZString.Empty;
			AssertEquals("CH_AuthorityVersion", ZString.Empty, header.CH_AuthorityVersion);
		}

		public void TestAdministrativeStatusAndDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			AssertNotNull(header.CH_AdministrativeStatus);
			AssertNotNull(header.CH_AdministrativeStatusInfo);
			header.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.Deferred;
			AssertEquals("CH_AdministrativeStatus", BRAdministrativeStatusList.Codes.Deferred, header.CH_AdministrativeStatus);
			AssertEquals("AdministrativeStatusDescription", BRAdministrativeStatusList.Descriptions.Deferred, header.AdministrativeStatusDescription);
			header.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.Blocked;
			AssertEquals("CH_AdministrativeStatus", BRAdministrativeStatusList.Codes.Blocked, header.CH_AdministrativeStatus);
			AssertEquals("AdministrativeStatusDescription", BRAdministrativeStatusList.Descriptions.Blocked, header.AdministrativeStatusDescription);
			header.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.Dispensed;
			AssertEquals("CH_AdministrativeStatus", BRAdministrativeStatusList.Codes.Dispensed, header.CH_AdministrativeStatus);
			AssertEquals("AdministrativeStatusDescription", BRAdministrativeStatusList.Descriptions.Dispensed, header.AdministrativeStatusDescription);
			header.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.InProcess;
			AssertEquals("CH_AdministrativeStatus", BRAdministrativeStatusList.Codes.InProcess, header.CH_AdministrativeStatus);
			AssertEquals("AdministrativeStatusDescription", BRAdministrativeStatusList.Descriptions.InProcess, header.AdministrativeStatusDescription);
			header.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.Pending;
			AssertEquals("CH_AdministrativeStatus", BRAdministrativeStatusList.Codes.Pending, header.CH_AdministrativeStatus);
			AssertEquals("AdministrativeStatusDescription", BRAdministrativeStatusList.Descriptions.Pending, header.AdministrativeStatusDescription);
		}

		public void TestEntryAccessKey()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = CusEntryNumber.New(header, CusEntryNumberTypes.Brazil.EAK, Core.Constants.CountryCodes.Brazil);
			entryNum.CE_EntryNum = ZString.Empty;
			AssertEquals(true, header.EntryAccessKeyInfo.ReadOnly);
			AssertEquals(CusEntryHeader.Schema.EntryAccessKeyMaxLength, header.EntryAccessKeyInfo.MaxLength);
			header.EntryAccessKey = "20QPF044184276876457";
			AssertEquals("EntryAccessKey is not empty", "20QPF044184276876457", header.EntryAccessKey);
			var cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(EntryNumberQueryGenerator.GetEntryNumberByEntryTypeQuery(header.PK, "BR", CusEntryNumberTypes.Brazil.EAK, true));
			AssertEquals("CE_EntryNum is not empty", "20QPF044184276876457", cusEntryNumber.CE_EntryNum);
			header.EntryAccessKey = string.Empty;
			cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(EntryNumberQueryGenerator.GetEntryNumberByEntryTypeQuery(header.PK, "BR", CusEntryNumberTypes.Brazil.EAK, true));
			AssertEquals("CE_EntryNum is empty", ZString.Empty, cusEntryNumber.CE_EntryNum);
			Factory.Save();
			Assert("EAK number is deleted when CE_EntryNum is empty", cusEntryNumber.IsDeleted);
		}

		public void TestIsWaitingForResponse()
		{
			var entry = Factory.New<CusEntryHeader>();
			Assert("IsWaitingForResponse", !entry.IsWaitingForResponse);
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Assert("IsWaitingForResponse", entry.IsWaitingForResponse);
		}

		public void TestIsNotSent()
		{
			var entry = Factory.New<CusEntryHeader>();
			Assert("IsNotSent", entry.IsNotSent);
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Assert("IsNotSent", !entry.IsNotSent);
		}

		public void TestDefaultStatusDescription()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals(ZString.Empty, entryHeader.DefaultStatusDescription);
		}

		public void TestIMessageAttachee()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals(ZGuid.Empty, ((IMessageAttachee)entryHeader).BranchPK);

			var branch = Factory.New<GlbBranch>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.CustomsEntryHeaders.Add(entryHeader);
			AssertEquals(branch.PK, ((IMessageAttachee)entryHeader).BranchPK);
		}

		protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new ChargesCurrencyTestSetup();

		class ChargesCurrencyTestSetup : IChargesCurrencyTestSetup
		{
			void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
			{
				declaration.AutoCreateChargesBasedOnIncoTerm = false;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 10000m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invoiceHeader.JZ_NetWeight = 100m;

				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;
				invoiceLine.JI_NetWeight = 100m;

				var nonDutiableCharge = invoiceHeader.Charges.AddNew();
				nonDutiableCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.InternationalLoadingUnloadingHandling;
				nonDutiableCharge.J7_Amount = 200m;
				nonDutiableCharge.J7_IsDutiable = false;
				nonDutiableCharge.J7_IsGSTApplicable = true;
				nonDutiableCharge.J7_IsIncludedInITOT = true;

				var oft = invoiceHeader.Charges.AddNew();
				oft.J7_ChargeType = ImportCommonChargesProvider.OverseasFreightCollect.Code;
				oft.J7_Amount = 500m;
				oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
				oft.J7_IsDutiable = false;
				oft.J7_IsGSTApplicable = true;
				oft.J7_IsIncludedInITOT = true;
			}

			ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 9300m;
			ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 9800m;
		}

		public void TestCH_BGMReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals("B00001000-1", entry1.CH_BGMReference);
			AssertEquals("B00001000-2", entry2.CH_BGMReference);

			var entry3 = declaration.ActiveEntryHeaders.AddNew();
			var entry4 = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals("B00001000-3", entry3.CH_BGMReference);
			AssertEquals("B00001000-4", entry4.CH_BGMReference);

			var entry5 = declaration.ActiveEntryHeaders.AddNew();
			entry5.CH_CEI_Instruction = ZGuid.NewZGuid();
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals("CH_BGMReference should not be set when saving failed", ZString.Empty, entry5.CH_BGMReference);

			entry5.CH_CEI_Instruction = ZGuid.Empty;
			Factory.Save();
			AssertEquals("B00001000-5", entry5.CH_BGMReference);

			var entry6 = declaration.ActiveEntryHeaders.AddNew();
			entry6.CH_MessageType = MessageTypeList.Codes.SUF;
			Factory.Save();
			Assert(entry6.CH_BGMReference.IsEmpty);
		}

		public void TestIsFormalEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			entryHeader.CH_MessageType = MessageTypeList.Codes.SUF;
			Assert(!entryHeader.IsFormalEntry);

			entryHeader.CH_MessageType = MessageTypeList.Codes.CDI;
			Assert(entryHeader.IsFormalEntry);
		}

		public void TestIsImportOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.LIC;
			var header = declaration.CustomsEntryHeaders.AddNew();
			Assert(!header.IsImportOnly);

			declaration.JE_MessageType = MessageTypeList.Codes.ISW;
			Assert(!header.IsImportOnly);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Assert(!header.IsImportOnly);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Assert(header.IsImportOnly);
		}

		public void TestRiskChannelAndDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			AssertNotNull(header.CH_RiskChannel);

			header.CH_RiskChannel = RiskChannelList.Codes.Green;
			AssertEquals("CH_RiskChannel", RiskChannelList.Codes.Green, header.CH_RiskChannel);
			AssertEquals("RiskChannelDescription", RiskChannelList.Descriptions.Green, header.RiskChannelDescription);

			header.CH_RiskChannel = RiskChannelList.Codes.Gray;
			AssertEquals("CH_RiskChannel", RiskChannelList.Codes.Gray, header.CH_RiskChannel);
			AssertEquals("RiskChannelDescription", RiskChannelList.Descriptions.Gray, header.RiskChannelDescription);

			header.CH_RiskChannel = RiskChannelList.Codes.Red;
			AssertEquals("CH_RiskChannel", RiskChannelList.Codes.Red, header.CH_RiskChannel);
			AssertEquals("RiskChannelDescription", RiskChannelList.Descriptions.Red, header.RiskChannelDescription);

			header.CH_RiskChannel = RiskChannelList.Codes.Yellow;
			AssertEquals("CH_RiskChannel", RiskChannelList.Codes.Yellow, header.CH_RiskChannel);
			AssertEquals("RiskChannelDescription", RiskChannelList.Descriptions.Yellow, header.RiskChannelDescription);

			header.CH_RiskChannel = ZString.Empty;
			AssertEquals("CH_RiskChannel", ZString.Empty, header.CH_RiskChannel);
			AssertEquals("RiskChannelDescription", ZString.Empty, header.RiskChannelDescription);
		}

		public void TestEntryNumberType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST1");
			AssertEquals("EntryNumberType should be MRN", CusEntryNumberTypes.Standard.MovementReferenceNumber, entryHeader.CusEntryNumber.CE_EntryType);
		}

		public void TestEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST1");
			AssertEquals("EntryNumber and MovementReferenceNumber should be equal", entryHeader.EntryNumber, entryHeader.MovementReferenceNumber);
		}

		public void TestImportLicenseIdentifierSetter()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = BRJobMessageTypeList.Codes.Import;
			Factory.Save();
			AssertEquals("Must NOT have ImportLicenseIdentifierNumber when job NOT LIC", 0, declaration.CustomsEntryHeaders.Count(t => t.ImportLicenseIdentifierNumber != null && !t.ImportLicenseIdentifierNumber.CE_EntryNum.IsEmpty));

			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryHeader4 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader4.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();
			AssertEquals("Must have 2 ImportLicenseIdentifierNumber", 2, declaration.CustomsEntryHeaders.Count(t => t.ImportLicenseIdentifierNumber != null && !t.ImportLicenseIdentifierNumber.CE_EntryNum.IsEmpty));

			var entryHeader5 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader5.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();
			AssertEquals("Must have 3 ImportLicenseIdentifierNumber", 3, declaration.CustomsEntryHeaders.Count(t => t.ImportLicenseIdentifierNumber != null && !t.ImportLicenseIdentifierNumber.CE_EntryNum.IsEmpty));
		}

		public void TestImportLicenseIdentifierNumber()
		{
			Env.NumberFountains.GetBRLicenseEntryNumber().SetNext(Factory, 85);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Must have 2 ImportLicenseIdentifierNumber", 2, declaration.CustomsEntryHeaders.Count(t => t.ImportLicenseIdentifierNumber != null && !t.ImportLicenseIdentifierNumber.CE_EntryNum.IsEmpty));
				AssertEquals("CE_EntryNum must be equal to LI0000085001", "LI0000085001", entryHeader1.ImportLicenseIdentifierNumber.CE_EntryNum);
				AssertEquals("CE_EntryType must be equal to LIC", BRJobMessageTypeList.Codes.ImportLicense, entryHeader1.ImportLicenseIdentifierNumber.CE_EntryType);
				AssertEquals("CE_EntryNum must be equal to LI0000085002", "LI0000085002", entryHeader2.ImportLicenseIdentifierNumber.CE_EntryNum);
				AssertEquals("CE_EntryType must be equal to LIC", BRJobMessageTypeList.Codes.ImportLicense, entryHeader2.ImportLicenseIdentifierNumber.CE_EntryType);
			});

			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryHeader4 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader4.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Must have 4 ImportLicenseIdentifierNumber", 4, declaration.CustomsEntryHeaders.Count(t => t.ImportLicenseIdentifierNumber != null && !t.ImportLicenseIdentifierNumber.CE_EntryNum.IsEmpty));
				AssertEquals("CE_EntryNum must be equal to LI0000085001", "LI0000085001", entryHeader1.ImportLicenseIdentifierNumber.CE_EntryNum);
				AssertEquals("CE_EntryType must be equal to LIC", BRJobMessageTypeList.Codes.ImportLicense, entryHeader1.ImportLicenseIdentifierNumber.CE_EntryType);
				AssertEquals("CE_EntryNum must be equal to LI0000085002", "LI0000085002", entryHeader2.ImportLicenseIdentifierNumber.CE_EntryNum);
				AssertEquals("CE_EntryType must be equal to LIC", BRJobMessageTypeList.Codes.ImportLicense, entryHeader2.ImportLicenseIdentifierNumber.CE_EntryType);
				AssertEquals("CE_EntryNum must be equal to LI0000085003", "LI0000085003", entryHeader3.ImportLicenseIdentifierNumber.CE_EntryNum);
				AssertEquals("CE_EntryType must be equal to LIC", BRJobMessageTypeList.Codes.ImportLicense, entryHeader3.ImportLicenseIdentifierNumber.CE_EntryType);
				AssertEquals("CE_EntryNum must be equal to LI0000085004", "LI0000085004", entryHeader4.ImportLicenseIdentifierNumber.CE_EntryNum);
				AssertEquals("CE_EntryType must be equal to LIC", BRJobMessageTypeList.Codes.ImportLicense, entryHeader4.ImportLicenseIdentifierNumber.CE_EntryType);
			});
			entryHeader4.Delete();
			Factory.Save();
			var entryHeader5 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader5.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Must have 4 ImportLicenseIdentifierNumber", 4, declaration.CustomsEntryHeaders.Count(t => t.ImportLicenseIdentifierNumber != null && !t.ImportLicenseIdentifierNumber.CE_EntryNum.IsEmpty));
				AssertEquals("CE_EntryNum must be equal to LI0000085001", "LI0000085001", entryHeader1.ImportLicenseIdentifierNumber.CE_EntryNum);
				AssertEquals("CE_EntryType must be equal to LIC", BRJobMessageTypeList.Codes.ImportLicense, entryHeader1.ImportLicenseIdentifierNumber.CE_EntryType);
				AssertEquals("CE_EntryNum must be equal to LI0000085002", "LI0000085002", entryHeader2.ImportLicenseIdentifierNumber.CE_EntryNum);
				AssertEquals("CE_EntryType must be equal to LIC", BRJobMessageTypeList.Codes.ImportLicense, entryHeader2.ImportLicenseIdentifierNumber.CE_EntryType);
				AssertEquals("CE_EntryNum must be equal to LI0000085003", "LI0000085003", entryHeader3.ImportLicenseIdentifierNumber.CE_EntryNum);
				AssertEquals("CE_EntryType must be equal to LIC", BRJobMessageTypeList.Codes.ImportLicense, entryHeader3.ImportLicenseIdentifierNumber.CE_EntryType);
				AssertEquals("CE_EntryNum must be equal to LI0000085004", "LI0000085004", entryHeader5.ImportLicenseIdentifierNumber.CE_EntryNum);
				AssertEquals("CE_EntryType must be equal to LIC", BRJobMessageTypeList.Codes.ImportLicense, entryHeader5.ImportLicenseIdentifierNumber.CE_EntryType);
				AssertNull("ImportLicenseIdentifierNumber must be null", entryHeader4.ImportLicenseIdentifierNumber);
			});
		}

		public void TestImportLicenseIdentifier()
		{
			var entryHeader1 = Factory.New<CusEntryHeader>();
			AssertNull("Must be null", entryHeader1.ImportLicenseIdentifierNumber);

			entryHeader1.ImportLicenseIdentifier = "TEST_LIC";
			AssertEquals("Must be TEST_LIC", "TEST_LIC", entryHeader1.ImportLicenseIdentifier);
			AssertNotNull("Must NOT be null", entryHeader1.ImportLicenseIdentifierNumber);

			entryHeader1.ImportLicenseIdentifier = ZString.Empty;
			AssertNullOrEmpty("EntryHeader1 must be empty", entryHeader1.ImportLicenseIdentifier);
			AssertNull("EntryHeader1 must be null", entryHeader1.ImportLicenseIdentifierNumber);
		}

		public void TestOnSavedFailure()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			entryHeader1.OnSaving();
			entryHeader2.OnSaving();
			CombineAssertions(() =>
			{
				AssertEquals("Must have 2 ImportLicenseIdentifierNumber", 2, declaration.CustomsEntryHeaders.Count(t => t.ImportLicenseIdentifierNumber != null && !t.ImportLicenseIdentifierNumber.CE_EntryNum.IsEmpty));

				entryHeader1.OnSaved(false);
				AssertEquals("Must have 1 ImportLicenseIdentifierNumber", 1, declaration.CustomsEntryHeaders.Count(t => t.ImportLicenseIdentifierNumber != null && !t.ImportLicenseIdentifierNumber.CE_EntryNum.IsEmpty));
				AssertNullOrEmpty("EntryHeader1 must be empty", entryHeader1.ImportLicenseIdentifier);

				Factory.Save();
				entryHeader1.OnSaved(false);
				entryHeader2.OnSaved(false);
				AssertEquals("Must have 2 ImportLicenseIdentifierNumber", 2, declaration.CustomsEntryHeaders.Count(t => t.ImportLicenseIdentifierNumber != null && !t.ImportLicenseIdentifierNumber.CE_EntryNum.IsEmpty));
			});
		}

		public void TestFindImportLicenceMessage()
		{
			CusEntryHeader oHeader = Factory.New<CusEntryHeader>();
			var message = oHeader.Messages.AddNew(typeof(BREDIMessage));
			PrepareMessage(message);
			message.EM_ApplicationReference = "teste0001";
			message.EM_MessageText = "message1";

			var message2 = oHeader.Messages.AddNew(typeof(BREDIMessage));
			PrepareMessage(message2);
			message2.EM_ApplicationReference = "teste0002";
			message2.EM_MessageText = "message2";

			var message3 = oHeader.Messages.AddNew(typeof(BREDIMessage));
			PrepareMessage(message3);
			message3.EM_ApplicationReference = "teste0003";
			message3.EM_MessageText = "message3";

			AssertEquals("Message should be message1", "message1", oHeader.FindImportLicenceMessage("teste0001")?.EM_MessageText);
			AssertEquals("Message should be message2", "message2", oHeader.FindImportLicenceMessage("teste0002")?.EM_MessageText);
			AssertEquals("Message should be message3", "message3", oHeader.FindImportLicenceMessage("teste0003")?.EM_MessageText);
			AssertNull("No message should be find", oHeader.FindImportLicenceMessage("teste0004"));
		}

		public void TestReseToOriginal()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
			entryHeader.CH_EntryStatus = Constants.EntryStatus.Registered;
			entryHeader.EntryNumber = "123";
			entryHeader.MovementReferenceNumberSetter("TST1");
			entryHeader.EntryAccessKey = "20QPF044184276876457";
			entryHeader.CH_EntryReleaseDate = ZDateTime.Today;
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Today;
			entryHeader.CH_AdministrativeStatus = "2";
			entryHeader.CH_CargoStatus = "1";
			entryHeader.CH_RiskChannel = "2";
			Factory.Save();

			entryHeader.ResetToOriginal();

			CombineAssertions(() =>
			{
				AssertEquals("CH_Status shoud be", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("CH_EntryStatus shoud be", ZString.Empty, entryHeader.CH_EntryStatus);
				AssertEquals("MovementReferenceNumber shoud be", ZString.Empty, entryHeader.MovementReferenceNumber);
				AssertEquals("EntryAccessKey shoud be", ZString.Empty, entryHeader.EntryAccessKey);
				AssertEquals("CH_EntryReleaseDate shoud be", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
				AssertEquals("CH_EntrySubmittedDate shoud be", ZDateTime.Empty, entryHeader.CH_EntrySubmittedDate);
				AssertEquals("CH_AdministrativeStatus shoud be", ZString.Empty, entryHeader.CH_AdministrativeStatus);
				AssertEquals("CH_CargoStatus shoud be", ZString.Empty, entryHeader.CH_CargoStatus);
				AssertEquals("CH_RiskChannel shoud be", ZString.Empty, entryHeader.CH_RiskChannel);

				var log = entryHeader.Logs.MostRecentLog;
				AssertEquals("A CES Log should be added", Events.CustomsEntryStatus.Code, log.SL_SE_NKEvent);
				AssertEquals("CES Log Reference", Constants.EntryStatus.NotSent, log.SL_Reference);
			});
		}

		public void TestShouldLogEntryStatus()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Assert(!entryHeader.ShouldLogEntryStatus);
		}

		public void TestImportDeclarationNumber()
		{
			var licDeclaration = Factory.New<JobDeclaration>();
			licDeclaration.JE_DeclarationReference = "LIC_TEST1";
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var entryHeader = licDeclaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.LIC;
			Assert("Import Declaration JOB must be empty", entryHeader.ImportDeclarationNumber.IsEmpty);

			var entryInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_LIC1";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Assert("Import Declaration JOB must be empty", entryHeader.ImportDeclarationNumber.IsEmpty);

			var iswDeclaration = Factory.New<JobDeclaration>();
			iswDeclaration.JE_DeclarationReference = "ISW_TEST";
			iswDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			iswDeclaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(entryInstruction) });
			Assert("Import Declaration JOB must be empty", entryHeader.ImportDeclarationNumber.IsEmpty);

			Factory.Save();
			AssertEquals("Import Declaration JOB must be ISW_TEST", "ISW_TEST", entryHeader.ImportDeclarationNumber);
		}

		void PrepareMessage(EDIMessage message)
		{
			message.EM_MessageType = MessageTypeList.Codes.LIC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}

		public void TestEntryReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "JOB_TEST";
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "TEST1";

			AssertEquals("EntryReferenceNumber shoud be JOB_TEST/TEST1", "JOB_TEST/TEST1", entryHeader.EntryReferenceNumber);
		}

		public void TestAddCustomsUpdateLog()
		{
			var date = ZDateTimeOffset.Now;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			AssertCustomsUpdateLog(entryHeader.AddCustomsUpdateLog(date, customsDeclarationNumber: "123456789"), "|CDN=123456789");
			AssertCustomsUpdateLog(entryHeader.AddCustomsUpdateLog(date, customsStatus: "STU_TEST"), "|STU=STU_TEST");
			AssertCustomsUpdateLog(entryHeader.AddCustomsUpdateLog(date, description: "DES_TEST"), "|DES=DES_TEST");
			AssertCustomsUpdateLog(entryHeader.AddCustomsUpdateLog(date, reason: "RES_TEST"), "|RES=RES_TEST");
			AssertCustomsUpdateLog(entryHeader.AddCustomsUpdateLog(date, "123456789", "STU_TEST", "DES_TEST", "RES_TEST"), "|CDN=123456789|DES=DES_TEST|RES=RES_TEST|STU=STU_TEST");

			void AssertCustomsUpdateLog(StmALog log, ZString expectedEventReference) => CombineAssertions(() =>
			{
				AssertEquals("SL_Parent", entryHeader.PK, log.SL_Parent);
				AssertEquals("SL_SE_NKEvent", Events.CustomsUpdate.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", expectedEventReference, log.SL_Reference);
				AssertEquals("SL_EventTime", date.ToDateTime(), log.SL_EventTime.ToDateTime());
			});
		}
		public void TestAllMergedLinesFees()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate(ChargeTypesList.Codes.DTY, 10.10m);
			entryLine1.Fees.AddOrUpdate(Constants.RateTypes.PIS, 20.10m);
			AssertEquals(2, entryHeader1.AllMergedLinesFees.Count());

			var entryLine2 = entryHeader1.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 22.10m);
			AssertEquals(3, entryHeader1.AllMergedLinesFees.Count());

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(0, entryHeader2.AllMergedLinesFees.Count());

			var entryLine3 = entryHeader2.MergedLines.AddNew();
			entryLine3.Fees.AddOrUpdate(Constants.RateTypes.IPI, 22.10m);
			AssertEquals(1, entryHeader2.AllMergedLinesFees.Count());
			AssertEquals(3, entryHeader1.AllMergedLinesFees.Count());
		}

		public void TestChangeEntryLineStatusOnDeletingInvoiceLine_ImportOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			declaration.DoMerge();

			var entry = declaration.ActiveEntryHeaders[0];
			var entryLine1 = invoiceLine1.CusEntryLine;
			var entryLine2 = invoiceLine2.CusEntryLine;

			AssertEquals(CustomsPostedStatusList.Codes.Active, entryLine1.CL_CustomsPostedStatus);
			AssertEquals(CustomsPostedStatusList.Codes.Active, entryLine2.CL_CustomsPostedStatus);

			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Assert(entry.CH_HighestLineNumber.IsEmpty);

			entry.CH_Status = BRMessageStatusList.Codes.Accepted;
			AssertEquals((short)2, entry.CH_HighestLineNumber);

			invoiceLine1.Delete();
			declaration.DoMerge();
			AssertEquals(CustomsPostedStatusList.Codes.DeletePending, entryLine1.CL_CustomsPostedStatus);
			AssertEquals(CustomsPostedStatusList.Codes.Active, entryLine2.CL_CustomsPostedStatus);

			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			entry.CH_Status = CustomsPostedStatusList.Codes.Accepted;
			AssertEquals(CustomsPostedStatusList.Codes.Deleted, entryLine1.CL_CustomsPostedStatus);
			AssertEquals(CustomsPostedStatusList.Codes.Active, entryLine2.CL_CustomsPostedStatus);
		}

		public void TestDeleteEntryLineOnDeletingInvoiceLine_NonImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			declaration.DoMerge();

			var entry = declaration.ActiveEntryHeaders[0];
			var entryLine1 = invoiceLine1.CusEntryLine;
			var entryLine2 = invoiceLine2.CusEntryLine;

			AssertEquals(CustomsPostedStatusList.Codes.Active, entryLine1.CL_CustomsPostedStatus);
			AssertEquals(CustomsPostedStatusList.Codes.Active, entryLine2.CL_CustomsPostedStatus);

			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Assert(entry.CH_HighestLineNumber.IsEmpty);

			entry.CH_Status = BRMessageStatusList.Codes.Accepted;
			AssertEquals((short)2, entry.CH_HighestLineNumber);

			invoiceLine1.Delete();
			declaration.DoMerge();
			AssertEquals(true, entryLine1.IsDeleted);
			AssertEquals(CustomsPostedStatusList.Codes.Active, entryLine2.CL_CustomsPostedStatus);
		}

		public void TestSiscomexUsageFees()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeaderCDI = declaration.CustomsEntryHeaders.AddNew();
			entryHeaderCDI.CH_MessageType = MessageTypeList.Codes.CDI;
			entryHeaderCDI.CH_CEI_Instruction = declaration.CustomsEntryInstructions.AddNew().PK;

			var entryHeaderSUF = declaration.CustomsEntryHeaders.AddNew();
			entryHeaderSUF.CH_MessageType = MessageTypeList.Codes.SUF;
			entryHeaderSUF.CH_CEI_Instruction = entryHeaderCDI.CH_CEI_Instruction;

			var entryLine1 = entryHeaderSUF.AllEntryLines.AddNew();
			entryLine1.Fees.AddOrUpdate("SUF", 22.10m);
			Factory.Save();
			AssertEquals(1, entryHeaderCDI.SiscomexUsageFees.Count);

			var entryHeaderCDD = declaration.CustomsEntryHeaders.AddNew();
			entryHeaderCDD.CH_MessageType = MessageTypeList.Codes.CDD;
			entryHeaderCDD.CH_CEI_Instruction = entryHeaderCDI.CH_CEI_Instruction;

			var entryHeaderSUF2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeaderSUF2.CH_MessageType = MessageTypeList.Codes.SUF;
			entryHeaderSUF2.CH_CEI_Instruction = entryHeaderCDI.CH_CEI_Instruction;

			var entryLine3 = entryHeaderSUF2.AllEntryLines.AddNew();
			entryLine3.Fees.AddOrUpdate("SUF", 22.10m);
			Factory.Save();
			AssertEquals(0, entryHeaderCDD.SiscomexUsageFees.Count);
		}

		public void TestResetMessageStatusIfNeeded()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDI;
			entryHeader.CH_CEI_Instruction = declaration.CustomsEntryInstructions.AddNew().PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			Factory.Save();
			AssertEquals("New Entry Header added", ZString.Empty, entryHeader.CH_Status);

			entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
			entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			Factory.Save();
			AssertEquals("MRN is empty", BRMessageStatusList.Codes.Accepted, entryHeader.CH_Status);

			entryHeader.MovementReferenceNumberSetter("CDI123");
			entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Active;
			entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			Factory.Save();
			AssertEquals("Entry Header accepted", BRMessageStatusList.Codes.Accepted, entryHeader.CH_Status);

			entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
			Factory.Save();
			AssertEquals("CH_Status should be reset when CH_CustomsPostedStatus set to UpdatePending", ZString.Empty, entryHeader.CH_Status);

			entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();
			AssertEquals("CH_Status should NOT be reset when CH_Status set to AWA", BRMessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

			entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
			entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			Factory.Save();
			AssertEquals("Entry Header update accepted", BRMessageStatusList.Codes.Accepted, entryHeader.CH_Status);

			entryLine1.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			entryLine2.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			Factory.Save();
			AssertEquals("Entry Lines accepted", BRMessageStatusList.Codes.Accepted, entryHeader.CH_Status);

			entryLine1.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
			Factory.Save();
			AssertEquals("CH_Status should be reset when CL_CustomsPostedStatus set to UpdatePending", ZString.Empty, entryHeader.CH_Status);

			entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();
			AssertEquals("CH_Status should NOT be reset when CH_Status set to AWA", BRMessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);

			entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
			entryLine1.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			Factory.Save();
			AssertEquals("Entry Line update accepted", BRMessageStatusList.Codes.Accepted, entryHeader.CH_Status);

			entryLine2.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.DeletePending;
			Factory.Save();
			AssertEquals("CH_Status should be reset when CL_CustomsPostedStatus set to DeletePending", ZString.Empty, entryHeader.CH_Status);

			entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
			entryLine2.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Deleted;
			AssertEquals("Entry Line deletion accepted", BRMessageStatusList.Codes.Accepted, entryHeader.CH_Status);
			Factory.Save();

			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			Factory.Save();
			AssertEquals("CH_Status should be reset when a new Entry Line added (CL_CustomsPostedStatus set to Active)", ZString.Empty, entryHeader.CH_Status);
		}

		public void TestLogMessageRejectedIfRejected()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var messageRejectedLogs = entryHeader.Logs.GetAllLogs().Cast<StmALog>().Where(log => log.SL_SE_NKEvent == Events.MessageRejected.Code);
			AssertEquals("No MessageRejected logs", 0, messageRejectedLogs.Count());

			entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
			Factory.Save();
			AssertEquals("One MessageRejected log added", 1, messageRejectedLogs.Count());

			entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();
			AssertEquals("No MessageRejected log added", 1, messageRejectedLogs.Count());

			entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
			Factory.Save();
			AssertEquals("Another MessageRejected log added", 2, messageRejectedLogs.Count());

			entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
			Factory.Save();
			AssertEquals("No MessageRejected log added", 2, messageRejectedLogs.Count());

			entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
			entryHeader.Logs.AddNew(Events.MessageRejected);
			Factory.Save();
			AssertEquals("Another MessageRejected log added", 3, messageRejectedLogs.Count());
		}

		protected override void OverrideValuationDate(BaseJobComInvoiceHeader invoice, ZDateTime date)
		{
			base.OverrideValuationDate(invoice, date);
			((JobComInvoiceHeader)invoice).ExchangeRateDate = date;
		}

		protected override ZString ImportJobMessage => BRJobMessageTypeList.Codes.ImportSiscomex;

		protected override (ZString OverseasFreightChargeCode, ZString OverseasInsuranceChargeCode, ZString NotIncludedChargeCode) GetChargeCodesForTotalTAndI()
			=> (ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, Enterprise.Customs.Common.CustomsChargeTypeList.Codes.PackingCost);

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);
	}

	[TestedType(typeof(CusEntryHeader.Loader))]
	class CusEntryHeaderLoaderTest : LoaderTestCase
	{
		public void TestGetEntryHeaderByMRNQuery()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryHeaderCDE = declaration.CustomsEntryHeaders.AddNew();
			entryHeaderCDE.MovementReferenceNumberSetter("CDE123");
			entryHeaderCDE.CH_MessageType = MessageTypeList.Codes.CDE;

			var entryHeaderLIC = declaration.CustomsEntryHeaders.AddNew();
			entryHeaderLIC.MovementReferenceNumberSetter("LIC123");
			entryHeaderLIC.CH_MessageType = MessageTypeList.Codes.LIC;

			Factory.Save();
			var loader = GetNewLoaderToTest() as CusEntryHeader.Loader;
			CombineAssertions(() =>
			{
				AssertEquals("Loader should be equal to EntryHeaderCDE", entryHeaderCDE, loader.GetEntryHeaderByMRNQuery("CDE123", MessageTypeList.Codes.CDE));
				AssertEquals("Loader should be equal to EntryHeaderLIC", entryHeaderLIC, loader.GetEntryHeaderByMRNQuery("LIC123", MessageTypeList.Codes.LIC));
				AssertNull("Should be NULL when Empty", loader.GetEntryHeaderByMRNQuery(string.Empty, MessageTypeList.Codes.LIC));
				AssertNull("Loader should be NULL when CDE123 and LIC", loader.GetEntryHeaderByMRNQuery("CDE123", MessageTypeList.Codes.LIC));
				AssertNull("Loader should be NULL when LIC123 and CDE", loader.GetEntryHeaderByMRNQuery("LIC123", MessageTypeList.Codes.CDE));
				AssertNull("Loader should be NULL when WRONG_NUMBER", loader.GetEntryHeaderByMRNQuery("WRONG_NUMBER", MessageTypeList.Codes.LIC));
			});
		}

		public void TestGetEntryHeaderByImportLicenseIdentifier()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.ImportLicenseIdentifier = "LI00000001";
			entryHeader1.CH_MessageType = MessageTypeList.Codes.LIC;

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.ImportLicenseIdentifier = "LI00000002";
			entryHeader2.CH_MessageType = MessageTypeList.Codes.LIC;

			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader3.ImportLicenseIdentifier = "LI00000003";
			entryHeader3.CH_MessageType = MessageTypeList.Codes.CDI;

			Factory.Save();
			var loader = GetNewLoaderToTest() as CusEntryHeader.Loader;
			CombineAssertions(() =>
			{
				AssertEquals("Loader should be equal to EntryHeader1", entryHeader1, loader.GetEntryHeaderByImportLicenseIdentifier("LI00000001"));
				AssertEquals("Loader should be equal to EntryHeader2", entryHeader2, loader.GetEntryHeaderByImportLicenseIdentifier("LI00000002"));
				AssertNull("Loader should be NULL when LI00000003", loader.GetEntryHeaderByImportLicenseIdentifier("LI00000003"));
				AssertNull("Should be NULL when Empty", loader.GetEntryHeaderByImportLicenseIdentifier(string.Empty));
				AssertNull("Loader should be NULL when WRONG_NUMBER", loader.GetEntryHeaderByImportLicenseIdentifier("WRONG_NUMBER"));
			});
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<CusEntryHeader>();
			AssertEquals(EntryLineStatusList.Codes.Active, header.CH_CustomsPostedStatus);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusEntryHeader.Loader(Factory);
	}
}
