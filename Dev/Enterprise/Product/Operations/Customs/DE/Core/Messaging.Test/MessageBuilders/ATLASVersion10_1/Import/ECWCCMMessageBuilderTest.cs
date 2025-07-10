using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	sealed class ECWCCMMessageBuilderTest : MessageBuilderTest<ECWCCMMessageBuilder, LECWCG>
	{
		[TestDate(2020, 1, 16, 11, 38, 04)]
		[ExpectNoExceptions]
		public void TestMessageGroup_LAE()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns(ImportMessageSubTypeList.Codes.BondedWarehouseSingleDeclaration);
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().MetaData.MessageGroup, Is.EqualTo(LECWCGMetaDataMessageGroup.LAE));
		}

		[ExpectNoExceptions]
		public void TestMessageGroup_LUE()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns(ImportMessageSubTypeList.Codes.WarehouseStockTransfer);
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().MetaData.MessageGroup, Is.EqualTo(LECWCGMetaDataMessageGroup.LUE));
		}

		[ExpectNoExceptions]
		public void TestMessageGroup_LVE()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns(ImportMessageSubTypeList.Codes.BondedWarehouseSinglePrematureDeclaration);
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().MetaData.MessageGroup, Is.EqualTo(LECWCGMetaDataMessageGroup.LVE));
		}

		[ExpectNoExceptions]
		public void TestHeader_LocalReferenceNumber_Truncated()
		{
			headerMock.Setup(m => m.LocalReferenceNumber).Returns(string.Empty.PadLeft(36, 'A'));
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Header.LRN, Is.EqualTo(string.Empty.PadLeft(22, 'A')));
		}

		[ExpectNoExceptions]
		public void TestHeader_CustomsAuthorisation_Truncated()
		{
			headerMock.Setup(m => m.CustomsAuthorisationCurrentProcedure).Returns(string.Empty.PadLeft(36, 'A'));
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Header.CustomsAuthorisation.CurrentProcedure, Is.EqualTo(string.Empty.PadLeft(35, 'A')));
		}

		[ExpectNoExceptions]
		public void TestHeader_AuthorisationNumber_Truncated()
		{
			messageHeaderMock.Setup(m => m.AuthorisationNumber).Returns(string.Empty.PadLeft(26, 'A'));
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Header.AuthorisationNumber, Is.EqualTo(string.Empty.PadLeft(25, 'A')));
		}

		[ExpectNoExceptions]
		public void TestHeader_AuthorisationNumber_Empty()
		{
			messageHeaderMock.Setup(m => m.AuthorisationNumber).Returns(string.Empty);
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Header.AuthorisationNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestCustomsAuthorisationOwner_Null()
		{
			headerMock.Setup(m => m.CustomsAuthorisationOwner).Returns((IPartyID)null);
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().CustomsAuthorisationOwner, Is.Not.EqualTo(default(LECWCGCustomsAuthorisationOwner)));
		}

		[ExpectNoExceptions]
		public void TestRepresentative_Null()
		{
			headerMock.Setup(m => m.Representative).Returns((IPartyID)null);
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Representative, Is.EqualTo(default(LECWCGRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestBody_InwardMovement_MRN()
		{
			goodsItemMock.Setup(m => m.InwardMovementRegistrationNumber).Returns("23DE586601055987B7");

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Body[0].InwardMovement.MRN, Is.EqualTo("23DE586601055987B7"), "MRN");
				NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Body[0].InwardMovement.RegistrationNumber, Is.EqualTo(default(string)), "RegistrationNumber - should be [null]");
				NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Body[0].InwardMovement.SequenceNumber, Is.EqualTo("1234"), "SequenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestBody_InwardMovement_Empty()
		{
			goodsItemMock.Setup(m => m.InwardMovementRegistrationNumber).Returns(string.Empty);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Body[0].InwardMovement.RegistrationNumber, Is.EqualTo(string.Empty), "RegistrationNumber");
				NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Body[0].InwardMovement.SequenceNumber, Is.EqualTo("1234"), "SequenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestBody_OutwardMovement_Truncated()
		{
			goodsItemMock.Setup(m => m.OutwardMovementCompletionRegistrationNumber).Returns(string.Empty.PadLeft(36, 'A'));
			NUnit.Framework.Assert.That(messageBuilder.GenerateMessage().Body[0].OutwardMovement.CompletionRegistrationNumber, Is.EqualTo(string.Empty.PadLeft(35, 'A')));
		}

		[ExpectNoExceptions]
		public void TestBody_OutwardMovement_Empty()
		{
			goodsItemMock.Setup(m => m.OutwardMovementCompletionRegistrationNumber).Returns(string.Empty);
			goodsItemMock.Setup(m => m.OutwardMovementDecisiveDate).Returns((DateTime?)null);
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Body[0].OutwardMovement.CompletionRegistrationNumber, Is.EqualTo(default(string)), "CompletionRegistrationNumber - should be [null]");
				NUnit.Framework.Assert.That(message.Body[0].OutwardMovement.DecisiveDate, Is.EqualTo(default(DateTime)), "DecisiveDate");
			});
		}

		[TestDate(2020, 10, 22, 10, 25, 04)]
		[ExpectNoExceptions]
		public void TestCompleteMessage()
		{
			NUnit.Framework.Assert.That(messageBuilder.GetXMLMessage().AsString(), Is.EqualTo(new CargoWise.IO.EmbeddedResourceRetriever().GetString($"{TestExtensionsATLASVersion10_1.ImportTestFilesResourcePath}.TestECWCCMMessage.txt")));
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetupGoodsItem();

			var customsAuthorisationOwner = new Mock<IPartyID>();
			customsAuthorisationOwner.Setup(id => id.EoriNumber).Returns("GR234567890");
			customsAuthorisationOwner.Setup(id => id.EoriBranchSuffix).Returns("0001");

			var representative = new Mock<IPartyID>();
			representative.Setup(id => id.EoriNumber).Returns("GR345678901");
			representative.Setup(id => id.EoriBranchSuffix).Returns("0002");

			headerMock = new Mock<IECWCCMHeader>();
			headerMock.Setup(h => h.CustomsAuthorisationCurrentProcedure).Returns("CACP1234");
			headerMock.Setup(h => h.RepresentativeRelationshipFlag).Returns("0");
			headerMock.Setup(h => h.LocalReferenceNumber).Returns("LRN12345");
			headerMock.Setup(h => h.CustomsAuthorisationOwner).Returns(customsAuthorisationOwner.Object);
			headerMock.Setup(h => h.Representative).Returns(representative.Object);
			headerMock.Setup(m => m.Lines).Returns(new IECWCCMLine[] { goodsItemMock.Object, goodsItemMock2.Object });

			var interchangeSender = new Mock<IPartyID>();
			interchangeSender.Setup(i => i.EoriNumber).Returns("DE8999783");
			interchangeSender.Setup(i => i.EoriBranchSuffix).Returns("0000");

			messageHeaderMock = new Mock<IImportMessageHeader>();
			messageHeaderMock.Setup(h => h.InterchangeSender).Returns(interchangeSender.Object);
			messageHeaderMock.Setup(h => h.InterchangeRecipientID).Returns("DE005875");
			messageHeaderMock.Setup(h => h.AuthorisationNumber).Returns("AN123456");
			messageHeaderMock.Setup(h => h.MessageGroup).Returns(MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingBondedWarehouse);
			messageHeaderMock.Setup(h => h.Header).Returns(headerMock.Object);
			messageHeaderMock.Setup(m => m.PreparationDateAndTimeCET).Returns(new CentralEuropeanStandardDateAndTimeProvider(true));

			messageBuilder = new ECWCCMMessageBuilder(messageHeaderMock.Object);
		}
		ECWCCMMessageBuilder messageBuilder;
		Mock<IECWCCMHeader> headerMock;
		Mock<IImportMessageHeader> messageHeaderMock;
		Mock<IECWCCMLine> goodsItemMock;
		Mock<IECWCCMLine> goodsItemMock2;

		void SetupGoodsItem()
		{
			var outwardMovementAmount1 = new Mock<IAmount>();
			outwardMovementAmount1.Setup(p => p.Quantity).Returns(970m);
			outwardMovementAmount1.Setup(p => p.MeasurementUnit).Returns("NAR");
			outwardMovementAmount1.Setup(m => m.Qualifier).Returns("X");

			var outwardMovementAmount2 = new Mock<IAmount>();
			outwardMovementAmount2.Setup(p => p.Quantity).Returns(980m);
			outwardMovementAmount2.Setup(p => p.MeasurementUnit).Returns("KMG");
			outwardMovementAmount2.Setup(m => m.Qualifier).Returns("Y");

			goodsItemMock = new Mock<IECWCCMLine>();
			goodsItemMock.Setup(l => l.SequenceNumber).Returns(1);
			goodsItemMock.Setup(l => l.InwardMovementRegistrationNumber).Returns("IMRN1234");
			goodsItemMock.Setup(l => l.InwardMovementSequenceNumber).Returns(1234);
			goodsItemMock.Setup(l => l.OutwardMovementCompletionType).Returns("12");
			goodsItemMock.Setup(l => l.OutwardMovementCompletionRegistrationNumber).Returns("OMCRN123");
			goodsItemMock.Setup(l => l.OutwardMovementDecisiveDate).Returns(new DateTime(2021, 06, 15));
			goodsItemMock.Setup(l => l.OutwardMovementAmount).Returns(outwardMovementAmount1.Object);

			goodsItemMock2 = new Mock<IECWCCMLine>();
			goodsItemMock2.Setup(l => l.SequenceNumber).Returns(2);
			goodsItemMock2.Setup(l => l.InwardMovementRegistrationNumber).Returns("IMRN5678");
			goodsItemMock2.Setup(l => l.InwardMovementSequenceNumber).Returns(5678);
			goodsItemMock2.Setup(l => l.OutwardMovementCompletionType).Returns("34");
			goodsItemMock2.Setup(l => l.OutwardMovementCompletionRegistrationNumber).Returns("OMCRN456");
			goodsItemMock2.Setup(l => l.OutwardMovementDecisiveDate).Returns(new DateTime(2021, 06, 17));
			goodsItemMock2.Setup(l => l.OutwardMovementAmount).Returns(outwardMovementAmount2.Object);
		}
	}
}
