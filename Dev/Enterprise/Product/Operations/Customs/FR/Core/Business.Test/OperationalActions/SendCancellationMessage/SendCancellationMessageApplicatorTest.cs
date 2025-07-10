using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Services.OperationalActions.Support.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(SendCancellationMessageApplicator))]
	public class SendCancellationMessageApplicatorTest : OperationalActionMethodApplicatorTest//
	{
		public void TestBuild_WhenEntriesSelected()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			Factory.Save();
			var applicator = (SendCancellationMessageApplicator)Applicator;
			AssertEquals("No targets assigned yet", 0, applicator.CancellationMessage.Targets.Length);
			Applicator.Build(new ZGuid[] { entryHeader1_1.PK, entryHeader1_2.PK, entryHeader2_1.PK });
			AssertEquals(3, applicator.CancellationMessage.Targets.Length);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { entryHeader1_1.PK, entryHeader1_2.PK, entryHeader2_1.PK }, applicator.CancellationMessage.Targets.Select(x => x.PK));
		}

		public void TestBuild_WhenDeclarationsSelected()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			Factory.Save();
			var applicator = (SendCancellationMessageApplicator)Applicator;
			AssertEquals("No targets assigned yet", 0, applicator.CancellationMessage.Targets.Length);
			Applicator.Build(new ZGuid[] { declaration1.PK, declaration2.PK });
			AssertEquals(2, applicator.CancellationMessage.Targets.Length);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { declaration1.PK, declaration2.PK }, applicator.CancellationMessage.Targets.Select(x => x.PK));
		}

		[TestDate(2023, 9, 5, 10, 0, 0)]
		public void TestApply_WhenEntriesSelected()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			representative.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			declaration1.JE_OA_Representative = representative.MainAddress.PK;

			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			declaration1.JE_DeclarantType = "DIR";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var representative1 = Factory.NewWithValidTestData<OrgHeader>();
			representative1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			representative1.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			declaration2.JE_OA_Representative = representative1.MainAddress.PK;

			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			declaration2.JE_DeclarantType = "DIR";
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ABC", declaration1.CountryCode);

			Factory.Save();
			var applicator = (SendCancellationMessageApplicator)Applicator;
			AssertEquals("No targets assigned yet", 0, applicator.CancellationMessage.Targets.Length);
			Applicator.Build(new ZGuid[] { entryHeader1_1.PK, entryHeader1_2.PK, entryHeader2_1.PK });

			using (applicator.CancellationMessage.GetValidationSuspender())
			{
				applicator.CancellationMessage.InvalidationMotivation = "AAA";
				applicator.CancellationMessage.InvalidationReason = "BBB";

				var log = new DummyOperationalActionSectionLog();
				Applicator.Apply(log, null);

				var ediMessages = Factory.Load<EDIMessage>(new ZQuery()).ToArray();
				AssertEquals(3, ediMessages.Length);
				ediMessages = ediMessages.OrderBy(s => s.EM_MessageNum).ToArray();

				var expectedHeaders = new List<CusEntryHeader>()
				{
					entryHeader1_1, entryHeader1_2, entryHeader2_1
				};

				ValidateMessage(expectedHeaders, ediMessages[0]);
				ValidateMessage(expectedHeaders, ediMessages[1]);
				ValidateMessage(expectedHeaders, ediMessages[2]);

				var logMessages = new List<string>()
				{
					@"INFO: Cancellation message for entry 3B00001000 queued for sending successfully",
					@"INFO: Cancellation message for entry 3B00001000/1 queued for sending successfully",
					@"INFO: Cancellation message for entry 3B00001001 queued for sending successfully"
				};
				AssertContainsExactElementsInAnyOrder("Log messages must be as expected", logMessages, log.messages);
			}
		}

		[TestDate(2023, 9, 5, 10, 0, 0)]
		public void TestApply_WhenDeclarationsSelected()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			representative.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			declaration1.JE_OA_Representative = representative.MainAddress.PK;

			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			declaration1.JE_DeclarantType = "DIR";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var representative1 = Factory.NewWithValidTestData<OrgHeader>();
			representative1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			representative1.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			declaration2.JE_OA_Representative = representative1.MainAddress.PK;

			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			declaration2.JE_DeclarantType = "DIR";
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ABC", declaration1.CountryCode);

			Factory.Save();
			var applicator = (SendCancellationMessageApplicator)Applicator;
			AssertEquals("No targets assigned yet", 0, applicator.CancellationMessage.Targets.Length);

			Applicator.Build(new ZGuid[] { declaration1.PK, declaration2.PK });

			using (applicator.CancellationMessage.GetValidationSuspender())
			{
				applicator.CancellationMessage.InvalidationMotivation = "AAA";
				applicator.CancellationMessage.InvalidationReason = "BBB";

				var log = new DummyOperationalActionSectionLog();
				Applicator.Apply(log, null);

				var ediMessages = Factory.Load<EDIMessage>(new ZQuery()).ToArray();
				AssertEquals(3, ediMessages.Length);
				ediMessages = ediMessages.OrderBy(s => s.EM_MessageNum).ToArray();

				var expectedHeaders = new List<CusEntryHeader>()
				{
					entryHeader1_1, entryHeader1_2, entryHeader2_1
				};

				ValidateMessage(expectedHeaders, ediMessages[0]);
				ValidateMessage(expectedHeaders, ediMessages[1]);
				ValidateMessage(expectedHeaders, ediMessages[2]);

				var logMessages = new List<string>()
				{
					@"INFO: Cancellation message for entry 3B00001000 queued for sending successfully",
					@"INFO: Cancellation message for entry 3B00001000/1 queued for sending successfully",
					@"INFO: Cancellation message for entry 3B00001001 queued for sending successfully"
				};
				AssertContainsExactElementsInAnyOrder("Log messages must be as expected", logMessages, log.messages);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SendCancellationMessageApplicator(Factory, Mock.Of<IApplicatorValidationSupport>(m => m.IsValid));
		}

		void ValidateMessage(List<CusEntryHeader> headers, EDIMessage message)
		{
			AssertEquals("Message status must be QUE", EDIMessageStatusList.Codes.Queued, message.EM_Status);
			var header = headers.FirstOrDefault(h => h.PK == message.EM_LinkedObject.PK);
			headers.Remove(header);
			AssertNotNull("Linked object must be from expected headers", header);
			AssertContains("Message text must match", $@"{{""Request"":{{""operatorRequestReference"":""{header.CorrelationID + "-20230905100000"}""}},""ImportOperation"":[{{""sequenceNumber"":""1"",""LRN"":""{header.CorrelationID}"",""MRN"":""{header.MRN}"",""invalidationRequestDateAndTime"":""2023-09-05T10:00:00"",""invalidationMotivation"":""AAA"",""invalidationReason"":""BBB""}}],""Declarant"":{{""identificationNumber"":""FRABC""}},""Representative"":{{""identificationNumber"":""FR12345678900001"",""status"":""2""}}}}", message.EM_MessageText.Replace(System.Environment.NewLine, "").Replace(" ", ""));
		}
	}
}
