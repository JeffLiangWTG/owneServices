using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class Message813HeaderProviderHelperTest : TestCaseWithFactory
	{
		public void TestJourneyTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default value is correct", "H01", helper.JourneyTime);
				emcsDeclaration.JourneyTimeNumericPart = 11;
				emcsDeclaration.JourneyTimeFormatPart = JourneyTimeUnitList.Codes.Days;
				AssertEquals("Modified Value is Correct", "D11", helper.JourneyTime);
			});
		}

		public void TestTransportModeCode()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("AIR", "4", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("SEA", "1", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				AssertEquals("MAI", "5", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("ROA", "3", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				AssertEquals("RAI", "2", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
				AssertEquals("FIX", "7", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertEquals("IWT", "8", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Other;
				AssertEquals("OTH", "0", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = "ZZZ";
				AssertEquals("Invalid ZZZ", string.Empty, helper.TransportModeCode);
			});
		}

		public void TestInvoiceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", string.Empty, helper.InvoiceNumber);
				emcsDeclaration.InvoiceNumber = "INV1234";
				AssertEquals("Returns Correct value", "INV1234", helper.InvoiceNumber);
			});
		}

		public void TestInvoiceDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", null, helper.InvoiceDate);
				var invoiceDate = new ZDateTime(2018, 09, 09, 09, 09, 09);
				emcsDeclaration.InvoiceDate = invoiceDate;
				AssertEquals("Returns Correct value", invoiceDate, helper.InvoiceDate);
			});
		}

		public void TestGuarantorType()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.ZG_GuarantorType = ZString.Empty;
				AssertEquals("No Error for empty", string.Empty, helper.GuarantorType);
				emcsDeclaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Consignor;
				AssertEquals("Returns Correct value", "1", helper.GuarantorType);
			});
		}

		public void TestDestinationTypeCode()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_MessageSubType = ZString.Empty;
				AssertEquals("No Error for empty", string.Empty, helper.DestinationTypeCode);
				emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
				AssertEquals("Returns Correct value", EMCSDestinationTypeList.Codes.DestinationTaxWarehouse, helper.DestinationTypeCode);
			});
		}

		public void TestChangedTransportArrangement()
		{
			emcsDeclaration.ZG_TransportArrangement = ZString.Empty;
			AssertEquals(string.Empty, helper.ChangedTransportArrangement);
			emcsDeclaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.OwnerOfGoods;
			AssertEquals(EMCSTransportArrangementList.Codes.OwnerOfGoods, helper.ChangedTransportArrangement);
		}

		public void TestDeliveryPlaceCustomsOfficeReferenceNumber()
		{
			var officeCode = emcsDeclaration.CustomsOffices.AddNew();
			officeCode.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDelivery;
			officeCode.CY_Data = "DE01876";
			AssertEquals("DE01876", helper.DeliveryPlaceCustomsOfficeReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			helper = new Message813HeaderProviderHelper(emcsDeclaration);
		}
		EMCSJobDeclaration emcsDeclaration;
		Message813HeaderProviderHelper helper;
	}
}
