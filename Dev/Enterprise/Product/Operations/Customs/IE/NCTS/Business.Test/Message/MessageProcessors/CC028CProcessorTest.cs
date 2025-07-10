using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC028C;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC028CProcessor))]
	sealed class CC028CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<CC028CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC028CProvider>
	{
		[TestDate(2024, 5, 22, 19, 0, 0)]
		public void TestGenerationOfCustomsRegistry_Representative()
		{
			var orgHeaderRepresentative = Factory.New<OrgHeader>();
			orgHeaderRepresentative.OH_Code = "ORG001";
			orgHeaderRepresentative.OH_FullName = "INTRISNV";

			var orgHeaderPrincipal = Factory.New<OrgHeader>();
			orgHeaderPrincipal.OH_Code = "ORG002";
			orgHeaderPrincipal.OH_FullName = "CARGOWISE";

			var (nctsHeader, incomingMessage) = CreateNctsHeader(orgHeaderPrincipal, orgHeaderRepresentative);
			AssertGenerationOfCustomsRegistry(nctsHeader, incomingMessage);
		}

		[TestDate(2024, 5, 22, 19, 0, 0)]
		public void TestGenerationOfCustomsRegistry_Principal()
		{
			var orgHeaderPrincipal = Factory.New<OrgHeader>();
			orgHeaderPrincipal.OH_Code = "ORG001";
			orgHeaderPrincipal.OH_FullName = "INTRISNV";

			var (nctsHeader, incomingMessage) = CreateNctsHeader(orgHeaderPrincipal);
			AssertGenerationOfCustomsRegistry(nctsHeader, incomingMessage);
		}

		[TestDate(2024, 5, 22, 19, 0, 0)]
		public void TestGenerationOfCustomsRegistry_NotSimplifiedProcedure()
		{
			var orgHeaderPrincipal = Factory.New<OrgHeader>();
			orgHeaderPrincipal.OH_Code = "ORG001";
			orgHeaderPrincipal.OH_FullName = "INTRISNV";

			var (nctsHeader, incomingMessage) = CreateNctsHeader(orgHeaderPrincipal);
			nctsHeader.MovementHeader.BM_GONumber = ZString.Empty;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var entryNumber = CusEntryNumber.Load(nctsHeader, CusEntryNumberTypes.EU.CustomsRegistry, Core.Constants.CountryCodes.Ireland);
			AssertNull("No entry num should have been created on the NCTS header", entryNumber);
		}

		(NctsHeader nctsHeader, NCTSInboundEDIMessage incomingMessage) CreateNctsHeader(OrgHeader principal, OrgHeader representative = null)
		{
			var (nctsHeader, _, _, incomingMessage) = CreateSetupData();
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			nctsHeader.Principal.OrganisationPK = principal.PK;
			if (representative != null)
			{
				nctsHeader.MovementHeader.Representative.OrganisationPK = representative.PK;
			}
			movementHeader.BM_GONumber = NctsControlResult.Codes.AuthorizedTrader;

			return (nctsHeader, incomingMessage);
		}

		void AssertGenerationOfCustomsRegistry(NctsHeader nctsHeader, NCTSInboundEDIMessage incomingMessage)
		{
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var entryNumber = CusEntryNumber.Load(nctsHeader, CusEntryNumberTypes.EU.CustomsRegistry, Core.Constants.CountryCodes.Ireland);
			CombineAssertions(() =>
			{
				AssertNotNull("A new entry num should have been created on the NCTS header", entryNumber);
				AssertEquals("The category of the entry num should be CUS", "CUS", entryNumber.CE_Category);
				AssertEquals("The number of the entry num should be equal to LRN", nctsHeader.LocalReferenceNumber, entryNumber.CE_EntryNum);
				AssertEquals("The reference of the entry num should be TD-ORG001", "TD-ORG001", entryNumber.CE_EntryLineReference);
				AssertEquals("The issue date should be the current date", new ZDateTime(2024, 5, 22, 19, 0, 0), entryNumber.CE_IssueDate);
			});
		}

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, messageAttachee.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, messageAttachee.BM_CustomsStatus);
			AssertEquals("MovementReferenceNumber", "21IEDUB11A782454R2", messageAttachee.Header.MovementReferenceNumber);
			AssertEquals("Declaration acceptance date", ZDateTime.BrettsBirthday, messageAttachee.BM_EntryDate);
			AssertMessageInterpretation(incomingMessage, @"A MRN Allocated (IE028) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Accepted Date</td><td>18-Sep-71</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A MRN Allocated (IE028) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
			AssertEquals("Guarantee count", 1, messageAttachee.Header.MovementHeader.Guarantees.Count);
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE028;

		protected override ZString MessageFriendlyName => "CC028C: MRN ALLOCATED";

		protected override CC028CProcessor Processor => new CC028CProcessor(logger, typeof(Cc028CType));

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC028CText("LRN123456789", "21IEDUB11A782454R2");

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var (declaration, messageAttachee, outgoingMessage, incomingMessage) = base.CreateSetupData(incomingMessageText);
			declaration.MovementHeader.Guarantees.AddNew();

			return (declaration, messageAttachee, outgoingMessage, incomingMessage);
		}
	}
}
