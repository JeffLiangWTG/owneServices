using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Moq;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class CC054CSenderTest : NCTSMessageSenderTest<CC054CSender, ICC054C>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC054CSender(null));
		}

		public void TestReleaseRequestedPassedCorrectly() => CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var messageSendingAction = new MessageSendingAction(nctsHeader.MovementHeader);

			messageSendingAction.AgreeWithMinorDiscrepancies = true;
			AssertEquals(true, ((ICC054C)new CC054CSender(messageSendingAction).DataProvider).ReleaseRequested);
			messageSendingAction.AgreeWithMinorDiscrepancies = false;
			AssertEquals(false, ((ICC054C)new CC054CSender(messageSendingAction).DataProvider).ReleaseRequested);
		});

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override ZString EntryType => NctsMessageTypeList.Codes.RequestARelease;

		protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override ZString ParentTableName => Customs.Business.AutoCusInBondMoveHeader.Schema.TableName;

		protected override void SetUpMockProviderData(Mock<ICC054C> mockProvider)
		{
			var currentDateTime = new DateTime(2023, 01, 31, 15, 47, 45);
			mockProvider.Setup(m => m.MessageType).Returns(Constants.MessageTypes.CC054C);
			mockProvider.Setup(m => m.PreparationDateTime).Returns(currentDateTime);
			mockProvider.Setup(m => m.MessageRecipient).Returns("NCTS.BE");
			mockProvider.Setup(m => m.MessageIdentification).Returns("SENDERS REFERENCE PLACE HOLDER");
			mockProvider.Setup(m => m.MessageSender).Returns("CW1@");

			mockProvider.Setup(m => m.CorrelationIdentifier).Returns("CorrelationID");
			mockProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns("DepId");
			mockProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns(Mock.Of<IHolderOfTheTransitProcedure>(t =>
				t.IdentificationNumber == "BEHolderID" &&
				t.TirHolderIdentificationNumber == "TIR" &&
				t.ContactPerson == Mock.Of<IContactPerson>(p =>
					p.Name == "HolderContact" &&
					p.PhoneNumber == "PhoneNumber" &&
					p.EMailAddress == "EMailAddress")));
			mockProvider.Setup(m => m.MRN).Returns("MRN123");
			mockProvider.Setup(m => m.ReleaseRequestDateAndTime).Returns(new DateTime(2023, 02, 09, 14, 0, 54));
			mockProvider.Setup(m => m.ReleaseRequested).Returns(true);
		}
	}
}
