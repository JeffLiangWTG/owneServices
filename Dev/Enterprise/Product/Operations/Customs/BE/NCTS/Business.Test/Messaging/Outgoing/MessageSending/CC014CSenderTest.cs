using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Moq;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class CC014CSenderTest : NCTSMessageSenderTest<CC014CSender, ICC014C>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC014CSender(null));
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override ZString EntryType => NctsMessageTypeList.Codes.InvalidationCancellation;

		protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Cancellation;

		protected override ZString ParentTableName => Customs.Business.AutoCusInBondMoveHeader.Schema.TableName;

		protected override void SetUpMockProviderData(Mock<ICC014C> mockProvider)
		{
			var currentTime = new DateTime(2023, 02, 16, 14, 09, 36);
			mockProvider.Setup(x => x.MessageSender).Returns("MessageSender");
			mockProvider.Setup(x => x.MessageRecipient).Returns("MessageRecipient");
			mockProvider.Setup(x => x.CorrelationIdentifier).Returns("CorrelationIdentifier");
			mockProvider.Setup(x => x.MessageIdentification).Returns("MessageIdentification");
			mockProvider.Setup(x => x.InvalidationRequestDateAndTime).Returns(currentTime);
			mockProvider.Setup(x => x.InvalidationDecisionDateAndTime).Returns(currentTime);
			mockProvider.Setup(x => x.InvalidationDecision).Returns(false);
			mockProvider.Setup(x => x.InvalidationInitiatedByCustoms).Returns(false);
			mockProvider.Setup(x => x.InvalidationJustification).Returns("Justification of invalidation");
			mockProvider.Setup(x => x.MessageType).Returns(Constants.MessageTypes.CC014C);
			mockProvider.Setup(x => x.HolderOfTheTransitProcedure).Returns(Mock.Of<IHolderOfTheTransitProcedure>(h =>
				h.TirHolderIdentificationNumber == "TIRHolderIDNumber" &&
				h.IdentificationNumber == "BE0446101317" &&
				h.Name == "Holder Name" &&
				h.Address == Mock.Of<IAddress>(a =>
					a.StreetAndNumber == "Holder street 5" &&
					a.Country == "BE" &&
					a.City == "ANTWERPEN" &&
					a.Postcode == "2060" &&
					a.StreetAndNumberMaxLength == 35) &&
				h.ContactPerson == Mock.Of<IContactPerson>(c =>
					c.Name == "Contactperson name" &&
					c.PhoneNumber == "+32031234567" &&
					c.EMailAddress == "name@domain.be")));
			mockProvider.Setup(x => x.CustomsOfficeOfDeparture).Returns("BE01234");
			mockProvider.Setup(x => x.LRN).Returns("LRN123456789");
			mockProvider.Setup(x => x.MRN).Returns("MRN123456789");
		}
	}
}
