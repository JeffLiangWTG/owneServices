using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;
using AsycudaContainer = Enterprise.Customs.EU.Manifest.Business.AsycudaContainer;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderSS))]
	sealed class AsycudaManifestHeaderSSTest : AsycudaManifestHeaderBaseTest
	{
		public void TestIsRoadTransportMode()
		{
			var header = GetManifestHeader();
			header.AMA_TransportMode = "";
			AssertEquals(false, header.IsRoad);
			header.AMA_TransportMode = GBSSTransportTypeList.Codes.SeaFreight;
			AssertEquals(false, header.IsRoad);
			header.AMA_TransportMode = GBSSTransportTypeList.Codes.RoadFreight;
			AssertEquals(true, header.IsRoad);
			header.AMA_TransportMode = GBSSTransportTypeList.Codes.RoroAccompanied;
			AssertEquals(true, header.IsRoad);
			header.AMA_TransportMode = GBSSTransportTypeList.Codes.RoroUnaccompanied;
			AssertEquals(true, header.IsRoad);
			header.AMA_TransportMode = GBSSTransportTypeList.Codes.AirFreight;
			AssertEquals(false, header.IsRoad);
		}

		protected override AsycudaManifestHeaderBase GetManifestHeader() => Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();

		public void TestHumanReadableNamePrefix()
		{
			var header = (AsycudaManifestHeaderSS)GetNewBusinessObject();
			AssertEquals(ICSManifestTypes.Descriptions.SAS, header.HumanReadableNamePrefix);
		}

		public void TestBillType()
		{
			var header = (AsycudaManifestHeaderSS)GetNewBusinessObject();
			AssertEquals(typeof(AsycudaBillSS), header.GetBillType());
		}

		public void TestAMA_OA_Carrier()
		{
			var header = (AsycudaManifestHeaderSS)GetManifestHeader();
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(header.AMA_OA_CarrierInfo);
			AssertEquals("Caption", "Carrier", captionResourceString.Caption);
			AssertEquals("FullDescription", "Carrier must be entered if the carrier is different to the organization lodging the summary declaration", captionResourceString.FullDescription);
		}

		public void TestAMA_CustomsOfficeCaptions()
		{
			var header = GetManifestHeader();
			AssertEquals("Customs office of Lodgement", DataBoundResourceStrings.GetDataForProperty(header.AMA_CustomsOfficeInfo).Caption);
			AssertEquals("Lodgement Office", DataBoundResourceStrings.GetDataForProperty(header.AMA_CustomsOfficeInfo).MediumCaption);
			AssertEquals("Lodgement Office", DataBoundResourceStrings.GetDataForProperty(header.AMA_CustomsOfficeInfo).ShortCaption);
		}

		public void TestAMA_CustomsOffice()
		{
			var header = GetManifestHeader();
			header.AMA_CustomsOffice = "GB123456";
			AssertEquals(0, header.EUCustomsOffices.Count);
		}

		[ExpectNoExceptions]
		public void TestPopuplateItinerary()
		{
			var header = GetManifestHeader();
			header.AMA_RL_NKPortOfLoading = "GB123";
			header.AMA_RL_NKPortOfFirstArrival = ZString.Empty;
			header.AMA_RL_NKPortOfDischarge = ZString.Empty;
			AssertEquals(1, header.Itinerary.Count);
			var itinerary = header.Itinerary[0];
			AssertEquals("GB", itinerary.CY_Data);
			AssertEquals((short)1, itinerary.CY_Order);

			header.AMA_RL_NKPortOfLoading = ZString.Empty;
			AssertEquals(0, header.Itinerary.Count);

			header.AMA_RL_NKPortOfDischarge = "FR123";
			AssertEquals(1, header.Itinerary.Count);
			itinerary = header.Itinerary[0];
			AssertEquals("FR", itinerary.CY_Data);
			AssertEquals((short)1, itinerary.CY_Order);

			header.AMA_RL_NKPortOfFirstArrival = "IT123";
			AssertEquals(2, header.Itinerary.Count);
			itinerary = header.Itinerary.Where(x => x.CY_Order == 1).FirstOrDefault();
			AssertEquals("IT", itinerary.CY_Data);
			itinerary = header.Itinerary.Where(x => x.CY_Order == 2).FirstOrDefault();
			AssertEquals("FR", itinerary.CY_Data);

			header.AMA_RL_NKPortOfLoading = "GB123";
			AssertEquals(3, header.Itinerary.Count);
			itinerary = header.Itinerary.Where(x => x.CY_Order == 1).FirstOrDefault();
			AssertEquals("GB", itinerary.CY_Data);
			itinerary = header.Itinerary.Where(x => x.CY_Order == 2).FirstOrDefault();
			AssertEquals("IT", itinerary.CY_Data);
			itinerary = header.Itinerary.Where(x => x.CY_Order == 3).FirstOrDefault();
			AssertEquals("FR", itinerary.CY_Data);

			header.AMA_RL_NKPortOfFirstArrival = "FR123";
			AssertEquals(2, header.Itinerary.Count);
			itinerary = header.Itinerary.Where(x => x.CY_Order == 1).FirstOrDefault();
			AssertEquals("GB", itinerary.CY_Data);
			itinerary = header.Itinerary.Where(x => x.CY_Order == 2).FirstOrDefault();
			AssertEquals("FR", itinerary.CY_Data);

			Factory.Save();
		}

		public void TestChildBill()
		{
			var header = GetManifestHeader();
			header.AMA_RL_NKPortOfLoading = "GB123";
			header.AMA_RL_NKPortOfDischarge = "FR123";
			var bill = header.Bills.AddNew();
			CombineAssertions("New Bill should inherit default values from the header", () =>
			{
				AssertEquals(nameof(bill.ABL_RL_NKOrigin), "GB123", bill.ABL_RL_NKOrigin);
				AssertEquals(nameof(bill.ABL_RL_NKFinalDestination), "FR123", bill.ABL_RL_NKFinalDestination);
			});
		}

		public void TestUpdatePortOfLoadingUpdatesBills()
		{
			var header = GetManifestHeader();
			header.AMA_RL_NKPortOfLoading = "GB123";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			header.AMA_RL_NKPortOfLoading = "GB456";
			CombineAssertions("Bills should be updated when the header is changed", () =>
			{
				AssertEquals("bill1", "GB456", bill1.ABL_RL_NKOrigin);
				AssertEquals("bill2", "GB456", bill2.ABL_RL_NKOrigin);
			});
		}

		public void TestUpdatePortOfUnloadingUpdatesBills()
		{
			var header = GetManifestHeader();
			header.AMA_RL_NKPortOfDischarge = "FR123";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			header.AMA_RL_NKPortOfDischarge = "FR456";
			CombineAssertions("Bills should be updated when the header is changed", () =>
			{
				AssertEquals("bill1", "FR456", bill1.ABL_RL_NKFinalDestination);
				AssertEquals("bill2", "FR456", bill2.ABL_RL_NKFinalDestination);
			});
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeaderBase>);

		protected override Type ExpectedBillsType => typeof(AsycudaBillCollectionSS);
	}
}

