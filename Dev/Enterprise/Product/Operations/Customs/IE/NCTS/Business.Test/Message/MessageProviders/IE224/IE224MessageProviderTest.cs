using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AES;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE224MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE224MessageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("Sending Action missing", () => new IE224MessageProvider(null));
			});
		}

		public void TestHolderOfTheTransitProcedure()
		{
			var orgHeader = NCTSTestHelper.CreateOrgHeaderForTest(Factory);
			NCTSTestHelper.CreateEoriForTest(orgHeader, "GB12345678", "GB");
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Address1 = "Street and No";
			orgAddress.OA_City = "Milton Keynes";
			orgAddress.OA_PostCode = "MK16 XX";
			sendingAction.HolderOfTransitProcedure = orgAddress.Header.OH_Code;

			var holder = Provider.HolderOfTheTransitProcedure;
			AssertType<HolderOfTransitProcedureProvider>("HolderOfTheTransit", holder);
			CombineAssertions(() =>
			{
				AssertEquals("Identification number", "GB12345678", holder.Id);
				AssertEquals("Name", "Oscorp Industries", holder.Name);
				AssertEquals("Address Line", "Street and No", holder.Address.StreetAndNumber);
				AssertEquals("Address Postcode", "MK16 XX", holder.Address.Postcode);
				AssertEquals("Address City", "Milton Keynes", holder.Address.City);
				AssertEquals("Address Country", "GB", holder.Address.Country);
			});
		}

		public void TestGuarantor()
		{
			var orgHeader = NCTSTestHelper.CreateOrgHeaderForTest(Factory, "Test Company Limited", "PC1", "IEDUB");
			NCTSTestHelper.CreateEoriForTest(orgHeader, "IE12345678", "IE");
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Address1 = "123 Test Street";
			orgAddress.OA_City = "City";
			orgAddress.OA_PostCode = "A12B3C4";
			cusGuaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;

			var guarantor = Provider.Guarantor;
			AssertType<PartyProvider>("Guarantor", guarantor);
			CombineAssertions(() =>
			{
				AssertEquals("Identification number", "IE12345678", guarantor.Id);
				AssertEquals("Name", "Test Company Limited", guarantor.Name);
				AssertEquals("Address Line", "123 Test Street", guarantor.Address.StreetAndNumber);
				AssertEquals("Address Postcode", "A12B3C4", guarantor.Address.Postcode);
				AssertEquals("Address City", "City", guarantor.Address.City);
				AssertEquals("Address Country", "IE", guarantor.Address.Country);
			});
		}

		public void TestCustomsOfficeOfGuaranteeReferenceNumber()
		{
			sendingAction.CustomsOfficeOfGuarantee = "CUSTOFFG";
			AssertEquals("CUSTOFFG", Provider.CustomsOfficeOfGuaranteeReferenceNumber);
		}

		public void TestGuaranteeReference()
		{
			var guaranteeReferences = Provider.GuaranteeReference;
			AssertType<List<IIE224GuaranteeReference>>(guaranteeReferences);
			AssertEquals(1, guaranteeReferences.Count);
		}

		protected override IE224MessageProvider GetProvider() => new IE224MessageProvider(sendingAction);

		protected override void SetUp()
		{
			base.SetUp();
			cusGuaranteeHeader = Factory.New<CusGuaranteeHeader>();
			sendingAction = new GuaranteeVoucherSoldSendingAction(cusGuaranteeHeader);
		}

		CusGuaranteeHeader cusGuaranteeHeader;
		GuaranteeVoucherSoldSendingAction sendingAction;
	}
}
