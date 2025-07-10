using System.Collections.Generic;
using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.CC313A;
using Enterprise.Customs.GB.SafetyAndSecurity.Messaging.CC313A;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeader;
using ICustomsOffice = CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.ICustomsOffice;
using IGoodsItem = CargoWise.Customs.GB.MessageContracts.Interfaces.SafetyAndSecurity.IGoodsItem;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging.Testing
{
	[TestedType(typeof(DeclarationWrapper))]
	class CC313AWrapperTest : DeclarationWrapperTest
	{
		#region Header

		public void TestHeader_ReferenceNumber()
		{
			manifest.RegistrationNumber = "MRN123";
			AssertEquals("Document Reference Number - MRN", "MRN123", wrapper.Header.DocumentReferenceNumber);
		}

		public void TestHeader_AmendmentPlace()
		{
			AssertEquals("Header Amendment Place", "BranchAddress", wrapper.Header.AmendmentPlace);
		}

		public void TestHeader_AmendmentPlaceLNG()
		{
			AssertEquals("Header Amendment Place Language is default", "", wrapper.Header.AmendmentPlaceLNG);
		}

		[TestDate(2024, 11, 6, 12, 11, 10)]
		public void TestHeader_DateAndTimeOfAmendment()
		{
			AssertEquals("Header Amendment Date And Time populated in message builder", "202411061211", wrapper.Header.DateAndTimeOfAmendment);
		}

		#endregion

		public void TestMessageType()
		{
			AssertEquals("Message Type CC313A", "CC313A", wrapper.MessageType);
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
