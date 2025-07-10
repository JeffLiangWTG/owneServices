using CargoWise.Definitions;
using CargoWise.Schema;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DailyNoticeWorkflowDescriptor))]
	sealed class DailyNoticeWorkflowDescriptorTest : WorkflowDescriptorTestCase<DailyNoticeWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "DNC", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Description", "Daily Notice Canada", WorkflowDescriptor.Description);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.CustomsStatementHdr, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public override void TestSubTypes()
		{
			Assert(true);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[]
				{
					CusStatementHeaderSchema.B2_ProcessDate,
					CusStatementHeaderSchema.B2_ProcessPort,
					CusStatementHeaderSchema.B2_PaymentType,
					CusStatementHeaderSchema.B2_PrintDate,
					CusStatementHeaderSchema.B2_DueDate,
					CusStatementHeaderSchema.B2_PaymentAuthorizationDate,
					CusStatementHeaderSchema.B2_Status
				};
			}
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.OrgProxy;
			}
		}

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			var statement = (BaseCusStatementHeader)workflowProvider;
			var mode = statement.Importer.EDICommunicationsModes.AddNew();
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			mode.EK_Module = "DNC";
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_Destination = "@notificationemail.cargowise.com";
		}

		protected override string EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlNativeCountry;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				GetStatement(),
			};
		}

		BaseCusStatementHeader GetStatement()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			statement.B2_IsMonthlyStatement = true;

			return statement;
		}
	}
}
