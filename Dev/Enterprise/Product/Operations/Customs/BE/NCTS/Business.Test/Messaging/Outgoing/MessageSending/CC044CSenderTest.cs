using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Moq;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class CC044CSenderTest : NCTSMessageSenderTest<CC044CSender, ICC044C>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC044CSender(null));
		}

		public void TestFillDeclarationGoodsItemNumberOnSending()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			bill.B0_Weight = 1;
			bill.B0_WeightUQ = "KG";
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_Description = "Test";

			CombineAssertions(() =>
			{
				AssertEquals("Initial situation: no Declaration Goods Item Number generated.", 0, goodsItem.BY_DeclarationGoodsItemNumber);
				var declarationSenderProvider = new CC044CSender(new MessageSendingAction(header.ArrivalMovementHeader));
				declarationSenderProvider.Send();
				AssertEquals("After creation of DeclarationMessageSenderProvider: Declaration Goods Item Number is generated", 1, goodsItem.BY_DeclarationGoodsItemNumber);
			});
		}

		protected override string MovementType => NctsMovementType.Codes.Arrival;

		protected override ZString EntryType => NctsMessageTypeList.Codes.UnloadingRemarks;

		protected override ZString ExpectedCustomsStatus => NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;

		protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;

		protected override void SetUpMockProviderData(Mock<ICC044C> mockProvider)
		{
			var currentDateTime = new DateTime(2023, 01, 31, 15, 47, 45);
			mockProvider.Setup(m => m.MessageType).Returns(Constants.MessageTypes.CC044C);
			mockProvider.Setup(m => m.PreparationDateTime).Returns(currentDateTime);
			mockProvider.Setup(m => m.MessageRecipient).Returns("NCTS.BE");
			mockProvider.Setup(m => m.MessageIdentification).Returns("SENDERS REFERENCE PLACE HOLDER");
			mockProvider.Setup(m => m.MessageSender).Returns("CW1@");

			mockProvider.Setup(m => m.CorrelationIdentifier).Returns("CorrelationID");
			mockProvider.Setup(m => m.MRN).Returns("MRN123");
			mockProvider.Setup(m => m.Conform).Returns(true);
			mockProvider.Setup(m => m.Consignment).Returns(Mock.Of<IConsignmentType06>(t =>
				t.AdditionalReferences == new List<IDocument> { Mock.Of<IDocument>(u =>
					u.ReferenceNumber == "ReferenceNumber" &&
					u.SequenceNumber == 29 &&
					u.Type == "T") } &&
				t.DepartureTransportMeans == new List<IDepartureTransportMeans> { Mock.Of<IDepartureTransportMeans>(u =>
					u.IdentificationNumber == "IdentificationNumber" &&
					u.Nationality == "BE" &&
					u.SequenceNumber == 34 &&
					u.TypeOfIdentification == 35) } &&
				t.GrossMass == 36 &&
				t.HouseConsignments == new List<IHouseConsignmentType05> { Mock.Of<IHouseConsignmentType05>(u =>
					u.AdditionalReferences == new List<IDocument> { Mock.Of<IDocument>(v =>
						v.ReferenceNumber == "ReferenceNumber" &&
						v.SequenceNumber == 40 &&
						v.Type == "T") } &&
					u.ConsignmentItems == new List<IConsignmentItemType05> { Mock.Of<IConsignmentItemType05>(v =>
						v.AdditionalReferences == new List<IDocument> { Mock.Of<IDocument>(w =>
							w.ReferenceNumber == "A44" &&
							w.SequenceNumber == 45 &&
							w.Type == "V") } &&
						v.Commodity == Mock.Of<ICommodityType03>(w =>
							w.CombinedNomenclatureCode == "CombinedNomenclatureCode" &&
							w.CusCode == "CusCode" &&
							w.DescriptionOfGoods == "DescriptionOfGoods" &&
							w.GrossMass == 55 &&
							w.HarmonizedSystemSubHeadingCode == "HarmonizedSystemSubHeadingCode" &&
							w.NetMass == 57) &&
						v.DeclarationGoodsItemNumber == 58 &&
						v.GoodsItemNumber == 59 &&
						v.Packagings == new List<IPackaging> { Mock.Of<IPackaging>(w =>
							w.NumberOfPackages == 61 &&
							w.SequenceNumber == 62 &&
							w.ShippingMarks == "ShippingMarks" &&
							w.TypeOfPackages == "TypeOfPackages") } &&
						v.SupportingDocuments == new List<ISupportingDocument> { Mock.Of<ISupportingDocument>(w =>
							w.ComplementOfInformation == "ComplementOfInformation" &&
							w.ReferenceNumber == "ReferenceNumber" &&
							w.SequenceNumber == 69 &&
							w.Type == "Type") } &&
						v.TransportDocuments == new List<ITransportDocument> { Mock.Of<ITransportDocument>(w =>
							w.ReferenceNumber == "ReferenceNumber" &&
							w.SequenceNumber == 73 &&
							w.Type == "Type") }) } &&
					u.DepartureTransportMeans == new List<IDepartureTransportMeans> { Mock.Of<IDepartureTransportMeans>(v =>
						v.IdentificationNumber == "IdentificationNumber" &&
						v.Nationality == "BE" &&
						v.SequenceNumber == 78 &&
						v.TypeOfIdentification == 79) } &&
					u.GrossMass == 80 &&
					u.SequenceNumber == 81 &&
					u.SupportingDocuments == new List<ISupportingDocument>() { Mock.Of<ISupportingDocument>(v =>
						v.ComplementOfInformation == "ComplementOfInformation" &&
						v.ReferenceNumber == "ReferenceNumber" &&
						v.SequenceNumber == 86 &&
						v.Type == "Type") } &&
					u.TransportDocuments == new List<ITransportDocument> { Mock.Of<ITransportDocument>(v =>
						v.ReferenceNumber == "ReferenceNumber" &&
						v.SequenceNumber == 90 &&
						v.Type == "Type") } ) } &&
				t.SupportingDocuments == new List<ISupportingDocument> { Mock.Of<ISupportingDocument>(u =>
					u.ComplementOfInformation == "ComplementOfInformation" &&
					u.ReferenceNumber == "ReferenceNumber" &&
					u.SequenceNumber == 96 &&
					u.Type == "Type") } &&
				t.TransportDocuments == new List<ITransportDocument> { Mock.Of<ITransportDocument>(u =>
					u.ReferenceNumber == "ReferenceNumber" &&
					u.SequenceNumber == 100 &&
					u.Type == "Type") } &&
				t.TransportEquipments == new List<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> { Mock.Of<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment>(u =>
					u.ContainerIdentificationNumber == "ContainerIdentificationNumber" &&
					u.GoodsReferences == new List<IGoodsReference> { Mock.Of<IGoodsReference>(v =>
						v.DeclarationGoodsItemNumber == 105 &&
						v.SequenceNumber == 106) } &&
					u.NumberOfSeals == 107 &&
					u.Seals == new List<ISeal> { Mock.Of<ISeal>(v =>
						v.Identifier == "Identifier" &&
						v.SequenceNumber == 110) } &&
					u.SequenceNumber == 111) }));
			mockProvider.Setup(m => m.CustomsOfficeOfDestination).Returns("CustomsOfficeOfDestination");
			mockProvider.Setup(m => m.OtherThingsToReport).Returns("OtherThingsToReport");
			mockProvider.Setup(m => m.StateOfSeals).Returns("1");
			mockProvider.Setup(m => m.TraderIdentificationNumber).Returns("TraderIdentificationNumber");
			mockProvider.Setup(m => m.UnloadingCompletion).Returns(true);
			mockProvider.Setup(m => m.Unloadingdate).Returns(new DateTime(2023, 03, 07, 17, 00, 49));
			mockProvider.Setup(m => m.UnloadingRemark).Returns("UnloadingRemark");
		}
	}
}
