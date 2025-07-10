using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.Interfaces.SafetyAndSecurity;
using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgCusCode;
using AsycudaBill = Enterprise.Customs.EU.Manifest.Business.AsycudaBill;
using AsycudaManifestHeader = Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging.Testing
{
	[TestsSubclassesOf(typeof(DeclarationWrapperSS))]
	abstract class DeclarationWrapperTest : TestCaseWithFactory
	{
		public void TestMessageSender()
		{
			manifest.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);

			CombineAssertions(() =>
			{
				manifest.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(UnitedKingdomCodeTypes.EoriBranchSuffix, "0123456789", Core.Constants.CountryCodes.UnitedKingdom);
				AssertEquals("MessageSender composed as EORI/EORIBranchSuffix", "GB999999999888/0123456789", MessageSender);

				manifest.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(UnitedKingdomCodeTypes.EoriBranchSuffix, "12345", Core.Constants.CountryCodes.UnitedKingdom);
				AssertEquals("EORIBranchSuffix is 0-padded to 10 characters", "GB999999999888/0000012345", MessageSender);

				manifest.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(UnitedKingdomCodeTypes.EoriBranchSuffix, ZString.Empty, Core.Constants.CountryCodes.UnitedKingdom);
				AssertEquals("Default EORIBranchSuffix is used if branch does not have one", "GB999999999888/0000000000", MessageSender);
			});
		}

		public void TestFirstEntry()
		{
			manifest.EUCustomsOffices.RemoveAndDeleteAll();
			var entry = manifest.AddOfficeOfFirstEntry("GB123456");

			var entryDate = new ZDateTime(2024, 9, 1, 15, 32, 47, 999);
			entry.CY_Date = entryDate;

			CombineAssertions(() =>
			{
				AssertEquals("First Entry Reference Number", "GB123456", FirstEntry.ReferenceNumber);
				AssertEquals("First Entry Expected Date and Time of Arrival", "202409011532", FirstEntry.ExpectedDateAndTimeOfArrival);
			});
		}

		public void TestSealsIds()
		{
			var container1 = manifest.Containers.AddNew();
			container1.ACN_Seal1 = "SEAL1";
			container1.ACN_Seal2 = "SEAL2";
			container1.ACN_Seal3 = "SEAL3";
			var container2 = manifest.Containers.AddNew();
			container2.ACN_Seal1 = "SEAL4";

			AssertContainsExactElementsInAnyOrder("Seal Ids", new[] { "SEAL1", "SEAL2", "SEAL3", "SEAL4" }, SealsIds.Select(x => x.SealsIdentity));
		}

		public void TestLodgingSummaryDeclarationPerson()
		{
			var user = GlbStaff.CurrentUser;
			user.GS_FullName = "LodgingSummaryDeclarationPerson";
			user.Address1 = "Street";
			user.Postcode = "PC12";
			user.City = "City";
			user.GS_RN_NKCountryCode = "GB";

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.Branch.Address1 = "BranchAddress";
			manifest.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");

			CreateWrapper(manifest);
			var lodgingSummaryDeclarationPersonWrapper = LodgingSummaryDeclarationPerson;
			CombineAssertions("GB EORI", () =>
			{
				AssertEquals("Lodging Summary Declaration Person Company name", string.Empty, lodgingSummaryDeclarationPersonWrapper.Name);
				AssertEquals("Lodging Summary Declaration Person Street", string.Empty, lodgingSummaryDeclarationPersonWrapper.StreetAndNumber);
				AssertEquals("Lodging Summary Declaration Person Post code", string.Empty, lodgingSummaryDeclarationPersonWrapper.PostalCode);
				AssertEquals("Lodging Summary Declaration Person City", string.Empty, lodgingSummaryDeclarationPersonWrapper.City);
				AssertEquals("Lodging Summary Declaration Person Country code", string.Empty, lodgingSummaryDeclarationPersonWrapper.CountryCode);
				AssertEquals("Lodging Summary Declaration Person Language code", string.Empty, lodgingSummaryDeclarationPersonWrapper.LanguageCode);
				AssertEquals("Lodging Summary Declaration Person EORI", "GB999999999888", lodgingSummaryDeclarationPersonWrapper.ConfigCode);
			});

			manifest.Branch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			CreateWrapper(manifest);
			lodgingSummaryDeclarationPersonWrapper = LodgingSummaryDeclarationPerson;
			CombineAssertions("Name and Address, no EORI", () =>
			{
				AssertEquals("Lodging Summary Declaration Person Company name", "LodgingSummaryDeclarationPerson", lodgingSummaryDeclarationPersonWrapper.Name);
				AssertEquals("Lodging Summary Declaration Person Street", "Street", lodgingSummaryDeclarationPersonWrapper.StreetAndNumber);
				AssertEquals("Lodging Summary Declaration Person Post code", "PC12", lodgingSummaryDeclarationPersonWrapper.PostalCode);
				AssertEquals("Lodging Summary Declaration Person City", "City", lodgingSummaryDeclarationPersonWrapper.City);
				AssertEquals("Lodging Summary Declaration Person Country code", "GB", lodgingSummaryDeclarationPersonWrapper.CountryCode);
				AssertEquals("Lodging Summary Declaration Person Language code", string.Empty, lodgingSummaryDeclarationPersonWrapper.LanguageCode);
				AssertEquals("Lodging Summary Declaration Person EORI", string.Empty, lodgingSummaryDeclarationPersonWrapper.ConfigCode);
			});

			manifest.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "999999999888", "IT");
			CreateWrapper(manifest);
			lodgingSummaryDeclarationPersonWrapper = LodgingSummaryDeclarationPerson;
			CombineAssertions("Italy EORI", () =>
			{
				AssertEquals("Lodging Summary Declaration Person Company name", string.Empty, lodgingSummaryDeclarationPersonWrapper.Name);
				AssertEquals("Lodging Summary Declaration Person Street", string.Empty, lodgingSummaryDeclarationPersonWrapper.StreetAndNumber);
				AssertEquals("Lodging Summary Declaration Person Post code", string.Empty, lodgingSummaryDeclarationPersonWrapper.PostalCode);
				AssertEquals("Lodging Summary Declaration Person City", string.Empty, lodgingSummaryDeclarationPersonWrapper.City);
				AssertEquals("Lodging Summary Declaration Person Country code", string.Empty, lodgingSummaryDeclarationPersonWrapper.CountryCode);
				AssertEquals("Lodging Summary Declaration Person Language code", string.Empty, lodgingSummaryDeclarationPersonWrapper.LanguageCode);
				AssertEquals("Lodging Summary Declaration Person EORI", "IT999999999888", lodgingSummaryDeclarationPersonWrapper.ConfigCode);
			});
		}

		public void TestRepresentative()
		{
			AssertEquals("Representative not set", null, Representative);
		}

		public void TestLodgementCustomsOffice()
		{
			manifest.AMA_CustomsOffice = "GB000006";
			AssertEquals("Lodgement Customs Office", "GB000006", LodgementCustomsOffice.ReferenceNumber);
		}

		public void TestItineraries()
		{
			var route1 = manifest.Itinerary.AddNew();
			route1.CY_Data = "GB";
			var route2 = manifest.Itinerary.AddNew();
			route2.CY_Data = "NL";
			var route3 = manifest.Itinerary.AddNew();
			route3.CY_Data = "DE";

			AssertContainsExactElementsInExactOrder("Itinerary", new[] { "GB", "NL", "DE" }, Itineraries.Select(x => x.CountryOfRoutingCode));
		}

		public void TestGoodsItems()
		{
			AssertEquals("Two Goods Items", 2, GoodsItems.Count());
			AssertType<GoodsItemWrapper>(GoodsItems.ElementAt(0));
		}

		public void TestNotifyParty()
		{
			var bill = (AsycudaBill)manifest.MasterBill;
			bill.ABL_NotifyPartyName = "NotifyParty";
			bill.ABL_NotifyPartyStreet1 = "Street";
			bill.ABL_NotifyPartyPostcode = "PC12";
			bill.ABL_NotifyPartyCity = "City";
			bill.ABL_RN_NKNotifyPartyCountry = "GB";
			bill.ABL_NotifyPartyRegNo = "GB12345678";
			var notifyParty = NotifyParty;

			CombineAssertions(() =>
			{
				AssertEquals("Notify Party Company name", "NotifyParty", notifyParty.Name);
				AssertEquals("Notify Party Street", "Street", notifyParty.StreetAndNumber);
				AssertEquals("Notify Party Post code", "PC12", notifyParty.PostalCode);
				AssertEquals("Notify Party City", "City", notifyParty.City);
				AssertEquals("Notify Party Country code", "GB", notifyParty.CountryCode);
				AssertEquals("Notify Party Language code", string.Empty, notifyParty.LanguageCode);
				AssertEquals("Notify Party EORI", "GB12345678", notifyParty.ConfigCode);
			});
		}

		public void TestSubsequentEntries()
		{
			var entryOffice1 = manifest.AddOfficeOfSubsequentEntry("NL123456");
			var entryOffice2 = manifest.AddOfficeOfSubsequentEntry("DE123456");
			AssertContainsExactElementsInAnyOrder("Subsequent Entries", new[] { "NL123456", "DE123456" }, SubsequentEntries.Cast<ICustomsOffice>().Select(x => x.ReferenceNumber));
		}

		public void TestConsignee()
		{
			var bill = (AsycudaBill)manifest.MasterBill;
			bill.ABL_ConsigneeName = "Consignee";
			bill.ABL_ConsigneeStreet1 = "Street";
			bill.ABL_ConsigneePostcode = "PC12";
			bill.ABL_ConsigneeCity = "City";
			bill.ABL_RN_NKConsigneeCountry = "GB";
			bill.ABL_ConsigneeRegNo = "GB12345678";
			var consignee = Consignee;

			CombineAssertions(() =>
			{
				AssertEquals("Consignee Company name", "Consignee", consignee.Name);
				AssertEquals("Consignee Street", "Street", consignee.StreetAndNumber);
				AssertEquals("Consignee Post code", "PC12", consignee.PostalCode);
				AssertEquals("Consignee City", "City", consignee.City);
				AssertEquals("Consignee Country code", "GB", consignee.CountryCode);
				AssertEquals("Consignee Language code", string.Empty, consignee.LanguageCode);
				AssertEquals("Consignee EORI", "GB12345678", consignee.ConfigCode);
			});
		}

		public void TestHeader_TransportModeAtBorder()
		{
			manifest.AMA_TransportMode = "ROA";
			AssertEquals("Header Transport Mode At Border", "3", HeaderTransportModeAtBorder);
		}

		public void TestHeader_IdentityOfMeansOfTransportCrossingBorder()
		{
			AssertEquals("Header Identity Of Means Of Transport Crossing Border", "", HeaderIdentityOfMeansOfTransportCrossingBorder);
			manifest.AMA_VehicleRegistration = "regno";
			manifest.AMA_LloydsNumber = "lloyds";
			manifest.AMA_TransportMode = "IWT";
			AssertEquals("Header Identity Of Means Of Transport Crossing Border", "lloyds", HeaderIdentityOfMeansOfTransportCrossingBorder);
			manifest.AMA_TransportMode = "ROA";
			AssertEquals("Header Identity Of Means Of Transport Crossing Border", "regno", HeaderIdentityOfMeansOfTransportCrossingBorder);
			manifest.AMA_TransportMode = "ROR";
			AssertEquals("Header Identity Of Means Of Transport Crossing Border", "regno", HeaderIdentityOfMeansOfTransportCrossingBorder);
			manifest.AMA_TransportMode = "ROU";
			AssertEquals("Header Identity Of Means Of Transport Crossing Border", "regno", HeaderIdentityOfMeansOfTransportCrossingBorder);
			manifest.AMA_TransportMode = "SEA";
			AssertEquals("Header Identity Of Means Of Transport Crossing Border", "lloyds", HeaderIdentityOfMeansOfTransportCrossingBorder);
			manifest.AMA_TransportMode = "RAI";
			AssertEquals("Header Identity Of Means Of Transport Crossing Border", "", HeaderIdentityOfMeansOfTransportCrossingBorder);
		}

		public void TestHeader_IdentityOfMeansOfTransportCrossingBorderLNG()
		{
			AssertEquals("Header Identity Of Means Of Transport Crossing Border Language is default", string.Empty, HeaderIdentityOfMeansOfTransportCrossingBorderLNG);
		}

		public void TestHeader_NationalityOfMeansOfTransportCrossingBorder()
		{
			manifest.AMA_RN_NKConveyanceNationality = "PL";

			CombineAssertions(() =>
			{
				manifest.AMA_TransportMode = "ROA";
				AssertEquals("Header Nationality Of Means Of Transport Crossing Border (ROA)", "PL", HeaderNationalityOfMeansOfTransportCrossingBorder);
				manifest.AMA_TransportMode = "SEA";
				AssertEquals("Header Nationality Of Means Of Transport Crossing Border (SEA)", "", HeaderNationalityOfMeansOfTransportCrossingBorder);
				manifest.AMA_TransportMode = "AIR";
				AssertEquals("Header Nationality Of Means Of Transport Crossing Border (AIR)", "", HeaderNationalityOfMeansOfTransportCrossingBorder);
				manifest.AMA_TransportMode = "";
				AssertEquals("Header Nationality Of Means Of Transport Crossing Border (N/A)", "", HeaderNationalityOfMeansOfTransportCrossingBorder);
			});
		}

		public void TestHeader_TotalNumberOfItems()
		{
			AssertEquals("Header Total Number Of Items", "2", HeaderTotalNumberOfItems);
		}

		public void TestHeader_TotalNumberOfPackages()
		{
			var pack1 = goodsItem1.Packs.AddNew();
			pack1.APA_PackQty = 1;
			var pack2 = goodsItem1.Packs.AddNew();
			pack2.APA_PackQty = 2;
			var pack3 = goodsItem2.Packs.AddNew();
			pack3.APA_PackQty = 3;
			var pack4 = goodsItem2.Packs.AddNew();
			pack4.APA_PackQty = 4;
			AssertEquals("Header Total Number Of Packages", "10", HeaderTotalNumberOfPackages);
		}

		public void TestHeader_TotalGrossMass()
		{
			goodsItem1.ABL_GrossWeight = 1.111m;
			goodsItem2.ABL_GrossWeight = 2.222m;
			AssertEquals("Header Total Gross Mass", 3.333m, HeaderTotalGrossMass);
		}

		public void TestHeader_SpecificCircumstanceIndicator()
		{
			manifest.SpecificCircumstanceIndicator = "A";
			AssertEquals("Header Specific Circumstance Indicator", "A", HeaderSpecificCircumstanceIndicator);
		}

		public void TestHeader_TransportChargesMethodOfPayment()
		{
			manifest.MethodOfPayment = "C";
			AssertEquals("Header Transport Charges Method Of Payment", "C", HeaderTransportChargesMethodOfPayment);
		}

		public void TestHeader_CommercialReferenceNumber()
		{
			manifest.AMA_JobReference = "JOB123456789";
			AssertEquals("Header Commercial Reference Number", "JOB123456789", HeaderCommercialReferenceNumber);
		}

		public void TestHeader_ConveyanceReferenceNumber()
		{
			manifest.AMA_MasterBill = "MB123456789";
			AssertEquals("Header Conveyance Reference Number", "MB123456789", HeaderConveyanceReferenceNumber);
		}

		public void TestHeader_PlaceOfLoading()
		{
			manifest.AMA_RL_NKPortOfLoading = "GBNRW";
			AssertEquals("Header Place Of Loading", string.Empty, HeaderPlaceOfLoading);
		}

		public void TestHeader_PlaceOfLoadingLNG()
		{
			AssertEquals("Header Place Of Loading Language", string.Empty, HeaderPlaceOfLoadingLNG);
		}

		public void TestHeader_PlaceOfUnloading()
		{
			manifest.AMA_RL_NKPortOfDischarge = "NLROT";
			AssertEquals("Header Place Of Unloading", string.Empty, HeaderPlaceOfUnloading);
		}

		public void TestHeader_PlaceOfUnloadingLNG()
		{
			AssertEquals("Header Place Of Unloading Language", string.Empty, HeaderPlaceOfUnloadingLNG);
		}

		public void TestCorrelationIdentifier()
		{
			AssertEquals("Correlation Identifier", IcsSsGreatBritainEDIMessage.MessageNumberPlaceHolderXml, CorrelationIdentifier);
		}

		public void TestMessageIdentification()
		{
			AssertEquals("Message Identification", IcsSsGreatBritainEDIMessage.MessageNumberPlaceHolderXml, MessageIdentification);
		}

		public void TestPriority()
		{
			AssertEquals("We don't have a priority", string.Empty, Priority);
		}

		[TestDate(2024, 11, 6, 12, 11, 10)]
		public void TestTimeOfPreparation()
		{
			AssertEquals("Time Of Preparation is generated by message builder", "1211", TimeOfPreparation);
		}

		[TestDate(2024, 11, 6, 12, 11, 10)]
		public void TestDateOfPreparation()
		{
			AssertEquals("Date Of Preparation is generated by message builder", "241106", DateOfPreparation);
		}

		public void TestMessageRecipient()
		{
			AssertEquals("Message Recipient", string.Empty, MessageRecipient);
		}

		public void TestConsignor()
		{
			var bill = (AsycudaBill)manifest.MasterBill;
			bill.ABL_ShipperName = "Consignor";
			bill.ABL_ShipperStreet1 = "Street";
			bill.ABL_ShipperPostcode = "PC12";
			bill.ABL_ShipperCity = "City";
			bill.ABL_RN_NKShipperCountry = "GB";
			bill.ABL_ShipperRegNo = "GB12345678";
			var consignor = Consignor;

			CombineAssertions(() =>
			{
				AssertEquals("Consignor Company name", "Consignor", consignor.Name);
				AssertEquals("Consignor Street", "Street", consignor.StreetAndNumber);
				AssertEquals("Consignor Post code", "PC12", consignor.PostalCode);
				AssertEquals("Consignor City", "City", consignor.City);
				AssertEquals("Consignor Country code", "GB", consignor.CountryCode);
				AssertEquals("Consignor Language code", string.Empty, consignor.LanguageCode);
				AssertEquals("Consignor EORI", "GB12345678", consignor.ConfigCode);
			});
		}

		public void TestEntryCarrier()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgAddress>();
			carrier1.CompanyName = "EntryCarrier1";
			carrier1.OA_Address1 = "1 The Street";
			carrier1.OA_PostCode = "PC11";
			carrier1.OA_City = "City";
			carrier1.OA_RN_NKCountryCode = "GB";
			carrier1.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "999999999777", Core.Constants.CountryCodes.UnitedKingdom);

			var carrier2 = Factory.NewWithValidTestData<OrgAddress>();
			carrier2.CompanyName = "EntryCarrier2";
			carrier2.OA_Address1 = "2 The Street";
			carrier2.OA_PostCode = "PC12";
			carrier2.OA_City = "City";
			carrier2.OA_RN_NKCountryCode = "GB";
			carrier2.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "999999999776", Core.Constants.CountryCodes.France);

			var carrier3 = Factory.NewWithValidTestData<OrgAddress>();
			carrier3.CompanyName = "EntryCarrier2";
			carrier3.OA_Address1 = "2 The Street";
			carrier3.OA_PostCode = "PC12";
			carrier3.OA_City = "City";
			carrier3.OA_RN_NKCountryCode = "GB";
			carrier3.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "999999999775", Core.Constants.CountryCodes.Belgium);
			carrier3.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "999999999774", Core.Constants.CountryCodes.UnitedKingdom);

			manifest.AMA_OA_Carrier = carrier1.PK;
			AssertEntryCarrier("GB EORI", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "GB999999999777");

			manifest.AMA_OA_Carrier = carrier2.PK;
			CreateWrapper(manifest);
			AssertEntryCarrier("FR EORI", "EntryCarrier2", "2 The Street", "PC12", "City", "GB", "FR999999999776");

			manifest.AMA_OA_Carrier = carrier3.PK;
			CreateWrapper(manifest);
			AssertEntryCarrier("GB + other EORI, should use GB EORI", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "GB999999999774");

			carrier3.Header.CustomsCodes.RemoveAndDeleteAll();
			CreateWrapper(manifest);
			AssertEntryCarrier("No EORI", "EntryCarrier2", "2 The Street", "PC12", "City", "GB", string.Empty);

			void AssertEntryCarrier(string message, string expectedName, string expectedAddress, string expectedPostCode, string expectedCity, string expectedCountry, string expectedEori)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("Carrier Company name", expectedName, EntryCarrier.Name);
					AssertEquals("Carrier Street", expectedAddress, EntryCarrier.StreetAndNumber);
					AssertEquals("Carrier Post code", expectedPostCode, EntryCarrier.PostalCode);
					AssertEquals("Carrier City", expectedCity, EntryCarrier.City);
					AssertEquals("Carrier Country code", expectedCountry, EntryCarrier.CountryCode);
					AssertEquals("Carrier Language code", string.Empty, EntryCarrier.LanguageCode);
					AssertEquals("Carrier EORI", expectedEori, EntryCarrier.ConfigCode);
				});
			}
		}

		protected abstract void CreateWrapper(AsycudaManifestHeader manifest);
		protected abstract IEnumerable<IGoodsItem> GoodsItems { get; }

		protected abstract string MessageSender { get; }
		protected abstract IFirstEntryCustomsOffice FirstEntry { get; }
		protected abstract IEnumerable<ISealsID> SealsIds { get; }
		protected abstract ITrader LodgingSummaryDeclarationPerson { get; }
		protected abstract ITrader Representative { get; }
		protected abstract ICustomsOffice LodgementCustomsOffice { get; }
		protected abstract IEnumerable<IItinerary> Itineraries { get; }
		protected abstract ITrader NotifyParty { get; }
		protected abstract IEnumerable<ICustomsOffice> SubsequentEntries { get; }
		protected abstract ITrader Consignee { get; }
		protected abstract string HeaderTransportModeAtBorder { get; }
		protected abstract string HeaderIdentityOfMeansOfTransportCrossingBorder { get; }
		protected abstract string HeaderIdentityOfMeansOfTransportCrossingBorderLNG { get; }
		protected abstract string HeaderNationalityOfMeansOfTransportCrossingBorder { get; }
		protected abstract string HeaderTotalNumberOfItems { get; }
		protected abstract string HeaderTotalNumberOfPackages { get; }
		protected abstract decimal HeaderTotalGrossMass { get; }
		protected abstract string HeaderSpecificCircumstanceIndicator { get; }
		protected abstract string HeaderTransportChargesMethodOfPayment { get; }
		protected abstract string HeaderCommercialReferenceNumber { get; }
		protected abstract string HeaderConveyanceReferenceNumber { get; }
		protected abstract string HeaderPlaceOfLoading { get; }
		protected abstract string HeaderPlaceOfLoadingLNG { get; }
		protected abstract string HeaderPlaceOfUnloading { get; }
		protected abstract string HeaderPlaceOfUnloadingLNG { get; }
		protected abstract string CorrelationIdentifier { get; }
		protected abstract string MessageIdentification { get; }
		protected abstract string Priority { get; }
		protected abstract string TimeOfPreparation { get; }
		protected abstract string DateOfPreparation { get; }
		protected abstract string MessageRecipient { get; }
		protected abstract ITrader Consignor { get; }
		protected abstract ITrader EntryCarrier { get; }

		protected override void SetUp()
		{
			base.SetUp();
			manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.Branch.Address1 = "BranchAddress";
			manifest.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");
			goodsItem1 = manifest.Bills.AddNew();
			goodsItem2 = manifest.Bills.AddNew();
			CreateWrapper(manifest);
			wrapperGoodsItem1 = GoodsItems.ElementAt(0);
			wrapperGoodsItem2 = GoodsItems.ElementAt(1);
		}

		protected AsycudaManifestHeader manifest;
		protected AsycudaBill goodsItem1;
		protected AsycudaBill goodsItem2;
		protected IGoodsItem wrapperGoodsItem1;
		protected IGoodsItem wrapperGoodsItem2;
	}
}
