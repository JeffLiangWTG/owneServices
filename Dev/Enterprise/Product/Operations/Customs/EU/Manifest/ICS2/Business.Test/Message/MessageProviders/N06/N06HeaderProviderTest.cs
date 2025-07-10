using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	class N06HeaderProviderTest : ICS2BaseMessageProviderTest<N06HeaderProvider>
	{
		public void TestNotifyPartyIdentificationNumber_Null()
		{
			AssertEquals("NotifyPartyIdentificationNumber", string.Empty, Provider.NotifyParty.FirstOrDefault().IdentificationNumber);
		}

		public void TestNotifyParty()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.France);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "654321", Core.Constants.CountryCodes.UnitedKingdom);
			var addressWithEORI = orgHeader.Addresses.AddNew();
			addressWithEORI.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			manifestHeader.AMA_OA_ShippingAgent = addressWithEORI.PK;
			AssertEquals("NotifyPartyIdentificationNumber", "FR123456", Provider.NotifyParty.FirstOrDefault().IdentificationNumber);
		}

		public void TestActiveBorderTransportMeansIdentificationNumber()
		{
			Assert("Haven't populate ActiveBorderTransportMeans IdentificationNumber now", true);
		}

		public void TestActiveBorderTransportMeansIdentificationType()
		{
			Assert("Haven't populate ActiveBorderTransportMeans TypeofIdentification now", true);
		}

		public void TestModeofTransport()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty ModeofTransport", string.Empty, Provider.ModeofTransport);

				manifestHeader.AMA_TransportMode = "SEA";
				AssertEquals("SEA", "1", Provider.ModeofTransport);

				manifestHeader.AMA_TransportMode = "AIR";
				AssertEquals("AIR", "4", Provider.ModeofTransport);
			});
		}

		public void TestActualArrivalDate()
		{
			manifestHeader.AMA_A_ARV = new ZDateTime(2023, 01, 18);
			var actualArrivalDate = Provider.ActualArrivalDate;
			AssertEquals("Actual arrival date", new DateTime(2023, 01, 18).ToUniversalTime(), actualArrivalDate);
		}

		public void TestEstimatedArrivalDate()
		{
			manifestHeader.AMA_E_ARV = new ZDateTime(2023, 01, 18);
			var estimatedArrivalDate = Provider.EstimatedArrivalDate;
			AssertEquals("Estimated arrival date", new DateTime(2023, 01, 18).ToUniversalTime(), estimatedArrivalDate);
		}

		public void TestConveyanceReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("empty ConveyanceReferenceNumber", string.Empty, Provider.ConveyanceReferenceNumber);

				manifestHeader.AMA_Voyage = "Conveyance";
				AssertEquals("ConveyanceReferenceNumber not empty", "Conveyance", Provider.ConveyanceReferenceNumber);
			});
		}

		public void TestRelatedTransportDocument()
		{
			manifestHeader.AMA_MasterBill = "Num123";
			manifestHeader.MasterBill.TransportDocumentType = "N722";
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "ABL456";
			bill.TransportDocumentType = "XYZ";
			AssertContainsExactElementsInExactOrder("RelatedTransportDocument Number", new[] { "Num123", "ABL456" }, Provider.RelatedTransportDocument.Select(transportDocument => transportDocument.Identifier));
			AssertContainsExactElementsInExactOrder("RelatedTransportDocument Type", new[] { "N722", "XYZ" }, Provider.RelatedTransportDocument.Select(transportDocument => transportDocument.Type));
		}

		public void TestRelatedMRN()
		{
			manifestHeader.RegistrationNumber = "Num123";
			AssertEquals("MRN not empty", "Num123", Provider.RelatedMRN.FirstOrDefault());
		}

		public void TestPersonNotifyingArrival()
		{
			AssertEquals("PersonNotifyingArrival IdentificationNumber", "DE654321", Provider.PersonNotifyingArrival.IdentificationNumber);
		}

		public void TestCustomsOfficeReferenceNumber()
		{
			manifestHeader.AMA_CustomsOffice = "Office123";
			AssertEquals("CustomsOfficeOfFirstEntry", "Office123", Provider.CustomsOfficeReferenceNumber);
		}

		protected override IEnumerable<Expression<Func<N06HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.RelatedTransportDocument;
			yield return x => x.RelatedMRN;
		}
	}
}
