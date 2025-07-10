using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class StatusRequestHeaderProviderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new StatusRequestHeaderProviderForTest(null));
		}

		public void TestMRN()
		{
			statusRequest.MovementReferenceNumber = "MRN";
			AssertEquals("MRN", provider.MRN);
		}

		public void TestParty_Empty()
		{
			AssertNull(provider.Party);
		}

		public void TestParty()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			statusRequest.Identification = org.PK;
			CombineAssertions(() =>
			{
				var party = provider.Party;
				AssertNotNull("Not Null", party);
				AssertSame("Cached", party, provider.Party);
			});
		}

		public void TestInterchangeSender()
		{
			CombineAssertions(() =>
			{
				var interchangeSender = provider.InterchangeSender;
				AssertType<StatusRequestInterchangeSenderProvider>("Sender", provider.InterchangeSender);
				AssertSame("Cached", interchangeSender, provider.InterchangeSender);
			});
		}

		public void TestAuthorizationNumber()
		{
			AssertEquals("Default", ZString.Empty, provider.AuthorizationNumber);
		}

		public void TestAuthorizationNumber_Branch()
		{
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001123456789012345678901"))
			{
				AssertEquals("0001123456789012345678901", provider.AuthorizationNumber);
			}
		}

		public void TestAuthorizationNumber_Company()
		{
			using (DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "0001123456789012345678901"))
			{
				AssertEquals("0001123456789012345678901", provider.AuthorizationNumber);
			}
		}

		[TestDate(2022, 5, 3, 16, 35, 22)]
		public void TestPreparationDateAndTimeCET()
		{
			AssertEquals(new DateTime(2022, 5, 3, 18, 35, 00), provider.PreparationDateAndTimeCET.DateAndTime);
		}

		[TestDate(2022, 5, 3, 16, 35, 22)]
		[TestTimeZone]
		public void TestPreparationDateAndTimeUtc()
		{
			AssertEquals(new DateTime(2022, 5, 3, 16, 35, 00), provider.PreparationDateAndTimeUtc.DateAndTime);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statusRequest = Factory.New<StatusRequest>();
			provider = new StatusRequestHeaderProviderForTest(statusRequest);
		}
		StatusRequest statusRequest;
		IStatusRequestHeader provider;
	}

	class StatusRequestHeaderProviderForTest : StatusRequestHeaderProvider
	{
		public StatusRequestHeaderProviderForTest(StatusRequest statusRequest) : base(statusRequest)
		{
		}

		public override string InterchangeRecipientID => string.Empty;

		public override PartyType PartyType => PartyType.Unknown;
	}
}
