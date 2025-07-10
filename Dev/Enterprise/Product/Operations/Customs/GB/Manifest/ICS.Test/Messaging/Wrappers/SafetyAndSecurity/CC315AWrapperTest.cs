using System.Collections.Generic;
using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.CC315A;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.GB.SafetyAndSecurity.Messaging.CC315A;
using Moq.Protected;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeader;
using ICustomsOffice = CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.ICustomsOffice;
using IGoodsItem = CargoWise.Customs.GB.MessageContracts.Interfaces.SafetyAndSecurity.IGoodsItem;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging.Testing
{
	[TestedType(typeof(DeclarationWrapper))]
	class CC315AWrapperTest : DeclarationWrapperTest
	{
		public void TestHeader_ReferenceNumber()
		{
			manifest.AMA_JobReference = "JOB123";
			AssertEquals("Header Reference Number", "JOB123", wrapper.Header.ReferenceNumber);
		}

		public void TestHeader_TotalNumberOfPackagesWithBulk()
		{
			goodsItem1.Packs.RemoveAndDeleteAll();
			var pack1 = Factory.NewMoq<AsycudaPack>();
			pack1.Setup(p => p.APA_PackQty).Returns(1);
			pack1.Protected().Setup<bool>("IsBulkCore").Returns(false);
			goodsItem1.Packs.Add(pack1.Object);

			var pack2 = Factory.NewMoq<AsycudaPack>();
			pack2.Setup(p => p.APA_PackQty).Returns(2);
			pack2.Protected().Setup<bool>("IsBulkCore").Returns(true);
			goodsItem1.Packs.Add(pack2.Object);

			var pack3 = Factory.NewMoq<AsycudaPack>();
			pack3.Setup(p => p.APA_PackQty).Returns(3);
			pack3.Protected().Setup<bool>("IsBulkCore").Returns(false);
			goodsItem1.Packs.Add(pack3.Object);

			var pack4 = Factory.NewMoq<AsycudaPack>();
			pack4.Setup(p => p.APA_PackQty).Returns(4);
			pack4.Protected().Setup<bool>("IsBulkCore").Returns(true);
			goodsItem2.Packs.Add(pack4.Object);
			AssertEquals("Header Total Number Of Packages", "6", wrapper.Header.TotalNumberOfPackages);
		}

		public void TestHeader_DeclarationPlace()
		{
			AssertEquals("Header Declaration Place", "BranchAddress", wrapper.Header.DeclarationPlace);
		}

		public void TestHeader_DeclarationPlaceLNG()
		{
			AssertEquals("Header Declaration Place Language is default", "", wrapper.Header.DeclarationPlaceLNG);
		}

		[TestDate(2024, 11, 6, 12, 11, 10)]
		public void TestHeader_DeclarationDateAndTime()
		{
			AssertEquals("Header Declaration Date And Time populated in message builder", "202411061211", wrapper.Header.DeclarationDateAndTime);
		}

		public void TestMessageType()
		{
			AssertEquals("Message Type CC315A", "CC315A", wrapper.MessageType);
		}

		protected override void CreateWrapper(AsycudaManifestHeader manifest)
		{
			wrapper = new DeclarationWrapper(manifest);
		}

		protected override IEnumerable<IGoodsItem> GoodsItems => wrapper.GoodsItems;
		protected override string MessageSender => wrapper.MessageSender;
		protected override IFirstEntryCustomsOffice FirstEntry => wrapper.FirstEntry;
		protected override IEnumerable<ISealsID> SealsIds => wrapper.SealsIds;
		protected override ITrader LodgingSummaryDeclarationPerson => wrapper.LodgingSummaryDeclarationPerson;
		protected override ITrader Representative => wrapper.Representative;
		protected override ICustomsOffice LodgementCustomsOffice => wrapper.LodgementCustomsOffice;
		protected override IEnumerable<IItinerary> Itineraries => wrapper.Itineraries;
		protected override ITrader NotifyParty => wrapper.NotifyParty;
		protected override IEnumerable<ICustomsOffice> SubsequentEntries => wrapper.SubsequentEntries;
		protected override ITrader Consignee => wrapper.Consignee;
		protected override string HeaderTransportModeAtBorder => wrapper.Header.TransportModeAtBorder;
		protected override string HeaderIdentityOfMeansOfTransportCrossingBorder => wrapper.Header.IdentityOfMeansOfTransportCrossingBorder;
		protected override string HeaderIdentityOfMeansOfTransportCrossingBorderLNG => wrapper.Header.IdentityOfMeansOfTransportCrossingBorderLNG;
		protected override string HeaderNationalityOfMeansOfTransportCrossingBorder => wrapper.Header.NationalityOfMeansOfTransportCrossingBorder;
		protected override string HeaderTotalNumberOfItems => wrapper.Header.TotalNumberOfItems;
		protected override string HeaderTotalNumberOfPackages => wrapper.Header.TotalNumberOfPackages;
		protected override decimal HeaderTotalGrossMass => wrapper.Header.TotalGrossMass;
		protected override string HeaderSpecificCircumstanceIndicator => wrapper.Header.SpecificCircumstanceIndicator;
		protected override string HeaderTransportChargesMethodOfPayment => wrapper.Header.TransportChargesMethodOfPayment;
		protected override string HeaderCommercialReferenceNumber => wrapper.Header.CommercialReferenceNumber;
		protected override string HeaderConveyanceReferenceNumber => wrapper.Header.ConveyanceReferenceNumber;
		protected override string HeaderPlaceOfLoading => wrapper.Header.PlaceOfLoading;
		protected override string HeaderPlaceOfLoadingLNG => wrapper.Header.PlaceOfLoadingLNG;
		protected override string HeaderPlaceOfUnloading => wrapper.Header.PlaceOfUnloading;
		protected override string HeaderPlaceOfUnloadingLNG => wrapper.Header.PlaceOfUnloadingLNG;
		protected override string CorrelationIdentifier => wrapper.CorrelationIdentifier;
		protected override string MessageIdentification => wrapper.MessageIdentification;
		protected override string Priority => wrapper.Priority;
		protected override string TimeOfPreparation => wrapper.TimeOfPreparation;
		protected override string DateOfPreparation => wrapper.DateOfPreparation;
		protected override string MessageRecipient => wrapper.MessageRecipient;
		protected override ITrader Consignor => wrapper.Consignor;
		protected override ITrader EntryCarrier => wrapper.EntryCarrier;

		IDeclaration wrapper;
	}
}
