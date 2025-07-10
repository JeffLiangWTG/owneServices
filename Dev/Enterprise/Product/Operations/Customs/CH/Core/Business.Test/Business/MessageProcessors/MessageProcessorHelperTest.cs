using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business.Testing;

public class MessageProcessorHelperTest : TestCaseWithFactory
{
	public void TestGetOutgoingMessageFromSessionId()
	{
		var sessionGuid = ZGuid.NewZGuid();
		var sentEdiMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, MessageTypeCodeList.Codes.MSG,
			MessageSubTypeCodeList.Codes.Document, ApplicationCodeList.Codes.CHCustomsPassar,
			EDIMessage.Direction.Transmit, EDIMessage.Status.Sent);
		MessageProcessorTestHelper.CreateEDIInterchange(Factory, sessionGuid,
			ApplicationCodeList.Codes.CHCustomsPassar, EDIInterchange.Direction.Transmit, sentEdiMessage);
		Factory.Save();

		var message = MessageProcessorHelper.GetOutgoingMessageFromSessionId(Factory, sessionGuid);
		AssertEquals(message.PK, sentEdiMessage.PK);
	}

	public void TestGetIncomingMessageFromApplicationReference()
	{
		const string applicationReference = "application_reference";
		var receivedEdiMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, MessageTypeCodeList.Codes.MSG,
			MessageSubTypeCodeList.Codes.PassarDocumentNotification, ApplicationCodeList.Codes.CHCustomsPassar,
			EDIMessage.Direction.Receive, EDIMessage.Status.Received);

		receivedEdiMessage.EM_ApplicationReference = applicationReference;
		MessageProcessorTestHelper.CreateEDIInterchange(Factory, ZGuid.NewZGuid(),
			ApplicationCodeList.Codes.CHCustomsPassar, EDIInterchange.Direction.Transmit, receivedEdiMessage);

		var message = MessageProcessorHelper.GetIncomingMessageFromApplicationReference(Factory,
			ApplicationCodeList.Codes.CHCustomsPassar, applicationReference);

		AssertEquals(applicationReference, message.EM_ApplicationReference);
		AssertEquals(receivedEdiMessage.PK, message.PK);
	}

	public void TestGetOutgoingMessageFromApplicationReference()
	{
		const string applicationReference = "application_reference";
		var sentEdiMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, MessageTypeCodeList.Codes.MSG,
			MessageSubTypeCodeList.Codes.Document, ApplicationCodeList.Codes.CHCustomsPassar,
			EDIMessage.Direction.Transmit, EDIMessage.Status.Sent);

		sentEdiMessage.EM_ApplicationReference = applicationReference;
		MessageProcessorTestHelper.CreateEDIInterchange(Factory, ZGuid.NewZGuid(),
			ApplicationCodeList.Codes.CHCustomsPassar, EDIInterchange.Direction.Transmit, sentEdiMessage);

		var message = MessageProcessorHelper.GetOutgoingMessageFromApplicationReference(Factory,
			ApplicationCodeList.Codes.CHCustomsPassar, applicationReference);

		AssertEquals(applicationReference, message.EM_ApplicationReference);
		AssertEquals(sentEdiMessage.PK, message.PK);
	}

	public void TestGetOutgoingInterchangeFromSessionId()
	{
		var sessionGuid = ZGuid.NewZGuid();

		var trxInterchange = Factory.New<EDIInterchange>();
		trxInterchange.EI_From = "CW1";
		trxInterchange.EI_To = "Customs";
		trxInterchange.EI_SessionGUID = sessionGuid;
		trxInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		Factory.Save();

		var interchange = MessageProcessorHelper.GetOutgoingInterchangeFromSessionId(Factory, sessionGuid);
		AssertEquals(sessionGuid, interchange.EI_SessionGUID);
		AssertEquals(trxInterchange.PK, interchange.PK);
	}

	public void TestUtcToLocalBranchTime() => CombineAssertions(() =>
	{
		AssertEquals("Winter time", new ZDateTime(2000, 1, 1, 13, 0, 0), MessageProcessorHelper.UtcToLocalBranchTime(new DateTime(2000, 1, 1, 12, 0, 0)));
		AssertEquals("Summer time", new ZDateTime(2000, 8, 1, 14, 0, 0), MessageProcessorHelper.UtcToLocalBranchTime(new DateTime(2000, 8, 1, 12, 0, 0)));
	});

	public static void TestAppendEntryNumVersion()
	{
		AssertEquals("24CH202405280003J3.2", MessageProcessorHelper.AppendEntryNumVersion("24CH202405280003J3", "2"));
	}

	[TestDate(2023, 1, 1)]
	public void TestFindCusEntryNum(Func<BusinessObject> createJobCore) => CombineAssertions(() =>
	{
		var company1 = MessageProcessorTestHelper.CreateCompany(Factory, "C01", "B01");
		var company2 = MessageProcessorTestHelper.CreateCompany(Factory, "C02", "B02");

		CreateJobWithMRN("MRN1", company: company1);
		Factory.Save();
		TestDateAttribute.AddMinutes(1);
		var (_, cusEntryNum1) = CreateJobWithMRN("MRN1", company: company1);
		CreateJobWithMRN("MRN2", company: company2);
		CreateJobWithMRN("MRN3", company: company1, entryNumType: CusEntryNumberTypes.EU.ArrivalReferenceNumber);
		CreateJobWithMRN("MRN4", company: company1, entryNumCountry: Core.Constants.CountryCodes.Italy);
		CreateJobWithMRN("MRN5.1", company: company1);
		CreateJobWithMRN("MRN61", company: company1);
		Factory.Save();

		AssertSame("Valid job", cusEntryNum1, MessageProcessorHelper.FindCusEntryNumParent(company1, "MRN1"));
		AssertSame("With document type", cusEntryNum1, MessageProcessorHelper.FindCusEntryNumParent(company1, "MRN1.2.3"));
		AssertNull("Other company", MessageProcessorHelper.FindCusEntryNumParent(company1, "MRN2"));
		AssertNull("Other EntryNumType", MessageProcessorHelper.FindCusEntryNumParent(company1, "MRN3"));
		AssertNull("Other EntryNumCountry", MessageProcessorHelper.FindCusEntryNumParent(company1, "MRN4"));
		AssertNotNull("With version", MessageProcessorHelper.FindCusEntryNumParent(company1, "MRN5"));
		AssertNull("Only partial match", MessageProcessorHelper.FindCusEntryNumParent(company1, "MRN6"));
		AssertNotNull("With different version", MessageProcessorHelper.FindCusEntryNumParent(company1, "MRN5.2"));
		AssertNotNull("With different version and document type", MessageProcessorHelper.FindCusEntryNumParent(company1, "MRN5.2.3"));

		(BusinessObject job, CusEntryNumber entryNum) CreateJobWithMRN(ZString mrn, GlbCompany company = null, string entryNumType = CusEntryNumberTypes.Standard.MovementReferenceNumber, string entryNumCountry = CountryCodes.Switzerland)
		{
			var job = CreateJobCore(company);
			var cusEntryNum = CusEntryNumber.LoadOrCreate(job, entryNumType, entryNumCountry);
			cusEntryNum.CE_EntryNum = mrn;
			return (job, cusEntryNum);
		}
	});

	protected virtual BusinessObject CreateJobCore(GlbCompany company)
	{
		var declaration = Factory.New<JobDeclaration>();
		if (company != null)
		{
			declaration.JE_GC = company.PK;
		}
		return declaration.CustomsEntryHeaders.AddNew();
	}
}
