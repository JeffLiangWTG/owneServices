using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.EMCS.ServiceTasks.Testing
{
	public class EMCSOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "EMC", "12345123451234", PasswordTypesList.Codes.CDS);
			var declaration = EMCSMessageSenderTestHelper.CreateDeclaration(Factory, GlbBranch.CurrentBranch.PK.ToGuid());
			var message = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
			var futureMessage = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
			futureMessage.EM_HeldUntilDate = ZDateTime.Now.AddMinutes(15);
			futureMessage.EM_MessageText = "Future Held Date";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // so that message.EM_LinkedObject is loaded as GB.EMCSJobDeclaration
			{
				var processor = new EMCSOutgoingMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(CancellationToken.None);
			}

			message.Reload();
			futureMessage.Reload();
			var interchange = Factory.Load<EDIInterchange>(message.EM_EI);

			CombineAssertions(() =>
			{
				AssertEquals("Valid msg Sent", "SNT", message.EM_Status);
				AssertEquals("EM_MessageType", "815", message.EM_MessageType);
				AssertSame("Message Still linked to declaration", declaration, message.EM_LinkedObject);
				AssertNotNull("Interchange created and linked", interchange);
				AssertEquals("Interchange queued", "HQU", interchange.EI_Status);
				AssertEquals("Future message still Qeued", "QUE", futureMessage.EM_Status);
			});
		}

		public void TestMessageFilter()
		{
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "EMC", "12345123451234", PasswordTypesList.Codes.CDS);
			var declaration = EMCSMessageSenderTestHelper.CreateDeclaration(Factory, GlbBranch.CurrentBranch.PK.ToGuid());
			var message = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
			var excludedMessage = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
			excludedMessage.EM_MessageOwner = ZString.Empty;
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var processor = new EMCSOutgoingMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(CancellationToken.None);
			}

			message.Reload();
			excludedMessage.Reload();
			var interchange = Factory.Load<EDIInterchange>(new ZQuery { OrderBy = EDIInterchange.Schema.EI_InterchangeNum });

			CombineAssertions(() =>
			{
				AssertNotNull("Interchange created and linked", interchange);
				AssertEquals(1, interchange.Length);
			});
		}

		public void TestOutgoingMessageTypes()
		{
			var messageTypeList = EMCSOutgoingMessageProcessor.GetOutgoingMessageTypes().ToList();

			AssertContainsExactElementsInAnyOrder("OutgoingMessageTypes", new[]
			{
				EMCSGBOutgoingMessageTypeList.Codes.AlertOrRejectionOfAnEAD,
				EMCSGBOutgoingMessageTypeList.Codes.CancellationOfEAD,
				EMCSGBOutgoingMessageTypeList.Codes.ChangeOfDestination,
				EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnDelayForDelivery,
				EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnReasonForShortage,
				EMCSGBOutgoingMessageTypeList.Codes.PreValidateTrader,
				EMCSGBOutgoingMessageTypeList.Codes.ReportOfReceipt,
				EMCSGBOutgoingMessageTypeList.Codes.Splitting,
				EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD
			}, messageTypeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";
		}
	}
}
