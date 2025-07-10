using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Moq;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class CC141CSenderTest : NCTSMessageSenderTest<CC141CSender, ICC141C>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC141CSender(null));
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override ZString EntryType => NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;

		protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.NonArrivedMovement;

		protected override ZString ParentTableName => Customs.Business.AutoCusInBondMoveHeader.Schema.TableName;

		protected override void SetUpMockProviderData(Mock<ICC141C> mockProvider)
		{
			var currentDateTime = new DateTime(2023, 01, 31, 15, 47, 45);
			mockProvider.Setup(m => m.MessageType).Returns(Constants.MessageTypes.CC141C);
			mockProvider.Setup(m => m.PreparationDateTime).Returns(currentDateTime);
			mockProvider.Setup(m => m.MessageRecipient).Returns("NCTS.BE");
			mockProvider.Setup(m => m.MessageIdentification).Returns("SENDERS REFERENCE PLACE HOLDER");
			mockProvider.Setup(m => m.MessageSender).Returns("CW1@");

			mockProvider.Setup(m => m.MRN).Returns("01234567891");
			mockProvider.Setup(m => m.CustomsOfficeOfDestination).Returns("BE10100");
			mockProvider.Setup(m => m.CustomsOfficeOfEnquiryAtDeparture).Returns("BE00001");
			mockProvider.Setup(m => m.EnquiryText).Returns("EnquiryText");
			mockProvider.Setup(m => m.EnquiryTC11DeliveryDate).Returns(currentDateTime);
			mockProvider.Setup(m => m.ConsignmentConsignee).Returns(Mock.Of<IParty>(p =>
				p.Name == "Consignee" &&
				p.NameMaxlength == 35 &&
				p.Address == Mock.Of<IAddress>(a =>
					a.StreetAndNumber == "StreetAndNumber" &&
					a.Postcode == "1000" &&
					a.Country == Core.Constants.CountryCodes.Belgium &&
					a.City == "Brussels" &&
					a.StreetAndNumberMaxLength == 35)));
			mockProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns(Mock.Of<IHolderOfTheTransitProcedure>(h =>
				h.Name == "Holder" &&
				h.NameMaxlength == 35 &&
				h.IdentificationNumber == "ID" &&
				h.TirHolderIdentificationNumber == "TIR" &&
				h.ContactPerson == Mock.Of<IContactPerson>(c =>
					c.Name == "person" &&
					c.PhoneNumber == "123456" &&
					c.EMailAddress == "person@email") &&
				h.Address == Mock.Of<IAddress>(a =>
					a.StreetAndNumber == "StreetAndNumber" &&
					a.Postcode == "1000" &&
					a.Country == Core.Constants.CountryCodes.Belgium &&
					a.City == "Brussels" &&
					a.StreetAndNumberMaxLength == 35)));
		}
	}
}
