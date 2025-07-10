using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC028CMessageProcessor))]
	sealed class CC028CMessageProcessorTest : NctsMessageProcessorTestCase<CC028CMessageProcessor, ICC028CDataProvider>
	{
		public void TestNCTFOLCreatedWhenProcessed_Simplified()
		{
			const string lrn = "2204528148060XXXXXX";
			MockProvider.Setup(x => x.LRN).Returns(lrn);
			var nctsHeader = CreateNctsHeader(lrn, NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
			nctsHeader.MovementHeader.BM_GONumber = "xx";
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var query = new ZQuery();
			query.AddToFilter(new ZQuery(EDIMessageSchema.EM_MessageType, "NCT"));
			query.AddToFilter(new ZQuery(EDIMessageSchema.EM_MessageSubType, "FOL"));

			AssertNull(Factory.LoadTop1<EDIMessage>(query));
		}

		public void TestNCTFOLCreatedWhenProcessed_NotSimplified()
		{
			const string lrn = "2204528148060XXXXXX";
			MockProvider.Setup(x => x.LRN).Returns(lrn);
			var nctsHeader = CreateNctsHeader(lrn, NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
			nctsHeader.MovementHeader.BM_GONumber = ZString.Empty;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var query = new ZQuery();
			query.AddToFilter(new ZQuery(EDIMessageSchema.EM_MessageType, "NCT"));
			query.AddToFilter(new ZQuery(EDIMessageSchema.EM_MessageSubType, "FOL"));

			AssertNotNull(Factory.LoadTop1<EDIMessage>(query));
		}

		public void TestPreProcessMessage_CheckMessageSequenceIsValid()
		{
			const string lrn = "2204528148060XXXXXX";
			var currentDateTime = DateTime.Now;
			MockProvider.Setup(x => x.LRN).Returns(lrn);
			MockProvider.Setup(x => x.EntryDate).Returns(currentDateTime);
			var nctsHeader = CreateNctsHeader(lrn, NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;

			movementHeader.BM_CustomsStatus = ZString.Empty;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			incomingMessage.EM_RetryCount = 4;
			Factory.Save();

			Processor.PreProcessMessage(incomingMessage);

			AssertEquals("CheckMessageSequenceIsValid", false, CheckMessageSequenceIsValid(incomingMessage, Logger));
		}

		public void TestProcessCC028CMessageDeclaration()
		{
			const string mrn = "22BE000000000012J1";
			const string lrn = "2204528148060XXXXXX";
			var currentDateTime = DateTime.Now;
			MockProvider.Setup(x => x.MRN).Returns(mrn);
			MockProvider.Setup(x => x.LRN).Returns(lrn);
			MockProvider.Setup(x => x.EntryDate).Returns(currentDateTime);
			var nctsHeader = CreateNctsHeader(lrn, NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.ProcessedOK}", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondMoveHeader Customs Status Should BE 'MRN'", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, movementHeader.BM_CustomsStatus);
				AssertEquals("CusInBondMoveHeader Phase Should BE '015'", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Message Status Should BE 'ACC'", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertEquals("CusInBondMoveHeader Declaration Type Should BE A", NctsTypeOfAdditionalDeclarationList.Codes.A, movementHeader.BM_AdditionalDeclarationType);
				AssertEquals($"CusInBondHeader MRN Should BE {mrn}", mrn, nctsHeader.MovementReferenceEntryNumber.CE_EntryNum);
				AssertEquals("CusInBondMoveHeader Entry Date Should BE " + currentDateTime, currentDateTime, movementHeader.BM_EntryDate);
				AssertNotNull("CusInBondMoveHeader Should have a CES event logged", movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).SingleOrDefault());
				AssertEquals("Only 1 CES-event should have been created", 1, movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).Count());
				AssertContains("ST_NoteText", "New declaration status: Declaration MRN Allocated", incomingMessage.EM_MessageInterpretation);
			});
		}

		public void TestGenerationOfCustomsRegistry_Representative()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG001";
			orgHeader.OH_FullName = "INTRISNV";

			var orgHeaderPrincipal = Factory.New<OrgHeader>();
			orgHeaderPrincipal.OH_Code = "ORG002";
			orgHeaderPrincipal.OH_FullName = "CARGOWISE";

			var nctsHeader = CreateNctsHeader("2204528148060XXXXXX", NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			nctsHeader.MovementHeader.Representative.OrganisationPK = orgHeader.PK;
			nctsHeader.Principal.OrganisationPK = orgHeaderPrincipal.PK;

			AssertGenerationOfCustomsRegistry(nctsHeader, orgHeader.PK);
		}

		public void TestGenerationOfCustomsRegistry_Principal()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG001";
			orgHeader.OH_FullName = "INTRISNV";

			var nctsHeader = CreateNctsHeader("2204528148060XXXXXX", NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			nctsHeader.Principal.OrganisationPK = orgHeader.PK;

			AssertGenerationOfCustomsRegistry(nctsHeader, orgHeader.PK);
		}

		public void TestGenerationOfCustomsRegistry_NoSimplifidProcedure()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG001";
			orgHeader.OH_FullName = "INTRISNV";

			var nctsHeader = CreateNctsHeader("2204528148060XXXXXX", NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			nctsHeader.Principal.OrganisationPK = orgHeader.PK;

			var listOfValues = new CustomsRegistryCollection();
			var customsRegistryEntry = new CustomsRegistry();
			customsRegistryEntry.Organization = orgHeader.PK;
			customsRegistryEntry.DeclarationType = "TD";
			customsRegistryEntry.StartingDate = new DateTime(2023, 07, 20);
			customsRegistryEntry.StartingNo = 21;

			listOfValues.Add(customsRegistryEntry);

			nctsHeader.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;

			Factory.Save();

			using (BECustomsRegistry.Instance.CustomsRegistry.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, listOfValues))
			{
				var currentDateTime = DateTime.Now;
				MockProvider.Setup(x => x.LRN).Returns(nctsHeader.MovementHeader.BM_PaperlessInbondNum);
				MockProvider.Setup(x => x.EntryDate).Returns(currentDateTime);

				movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;

				Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
				Factory.Save();
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				Factory.Save();

				CombineAssertions(() =>
				{
					var entryNumber = CusEntryNumber.Load(nctsHeader, CusEntryNumberTypes.EU.CustomsRegistry, Core.Constants.CountryCodes.Belgium);
					AssertNull("No new entry num should have been created on the NCTS header", entryNumber);
				});
			}
		}

		void AssertGenerationOfCustomsRegistry(NctsHeader header, ZGuid orgHeaderPK)
		{
			var listOfValues = new CustomsRegistryCollection();
			var customsRegistryEntry = new CustomsRegistry();
			customsRegistryEntry.Organization = orgHeaderPK;
			customsRegistryEntry.DeclarationType = "TD";
			customsRegistryEntry.StartingDate = new DateTime(2023, 07, 20);
			customsRegistryEntry.StartingNo = 21;

			listOfValues.Add(customsRegistryEntry);

			header.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			header.MovementHeader.BM_GONumber = "A3";

			Factory.Save();

			using (BECustomsRegistry.Instance.CustomsRegistry.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, listOfValues))
			{
				var currentDateTime = DateTime.Now;
				MockProvider.Setup(x => x.LRN).Returns(header.MovementHeader.BM_PaperlessInbondNum);
				MockProvider.Setup(x => x.EntryDate).Returns(currentDateTime);

				var movementHeader = header.MovementHeader;
				movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;

				Extensions.CreateMovementReferenceNumber(header, "22BE000000000012J1");
				Factory.Save();
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				Factory.Save();

				CombineAssertions(() =>
				{
					var entryNumber = CusEntryNumber.Load(movementHeader, CusEntryNumberTypes.EU.CustomsRegistry, Core.Constants.CountryCodes.Belgium);
					AssertNotNull("A new entry num should have been created on the NCTS header", entryNumber);
					AssertEquals("The category of the entry num should be CUS", "CUS", entryNumber.CE_Category);
					AssertEquals("The number of the entry num should be 21", "21", entryNumber.CE_EntryNum);
					AssertEquals("The reference of the entry num should be TD-ORG001", "TD-ORG001", entryNumber.CE_EntryLineReference);
					AssertEquals("The reference of the entry num should be 20-07-2023", new DateTime(2023, 07, 20), entryNumber.CE_IssueDate);
					customsRegistryEntry = BECustomsRegistry.Instance.CustomsRegistry.Value[0];
					AssertEquals("The number of the BECUSTOMSREGISTRIES should hold the current generated number", 21, customsRegistryEntry.CurrentNo);

					customsRegistryEntry.StartingNo = 500;
					BECustomsRegistry.Instance.CustomsRegistry.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, listOfValues);
					Factory.Save();
					var entryNumber2 = Factory.New<CusEntryNumberForCC028CMessage>();
					entryNumber2.Parent = movementHeader;
					Factory.Save();
					AssertEquals("The number of the entry num after changing starting num should be 22", "22", entryNumber2.CE_EntryNum);
					customsRegistryEntry = BECustomsRegistry.Instance.CustomsRegistry.Value[0];
					AssertEquals("The number of the BECUSTOMSREGISTRIES after changing starting num should hold the current generated number", 22, customsRegistryEntry.CurrentNo);
				});
			}
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => true;

		protected override bool SupportsSearchByMRN => false;

		protected override Mock<ICC028CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC028CDataProvider>());
		Mock<ICC028CDataProvider> mockProvider;

		protected override Mock<CC028CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC028CMessageProcessor>(Logger));
		Mock<CC028CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC028C;

		protected override string ExpectedMessageFriendlyName => "MRN Allocated";

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged };

		protected override Type ExpectedMessageInterpreterType => typeof(CC028CMessageInterpreter);

		LoggingInformation Logger => logger ?? (logger = new LoggingInformation());
		LoggingInformation logger;
	}
}
