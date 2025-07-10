using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class GoodsLocationProviderTest : DataProviderTestCase<GoodsLocationProvider>
	{
		public void TestIGoodsLocation()
		{
			Assert("Should implement IGoodsLocation", Provider is IGoodsLocation);
		}

		public void TestTypeOfLocation()
		{
			SetUpTestData();
			goodsLocation.CGL_Type = "A";
			AssertEquals("A", Provider.TypeOfLocation);
		}

		public void TestQualifierOfIdentification()
		{
			SetUpTestData();
			goodsLocation.CGL_Qualifier = "T";
			AssertEquals("T", Provider.QualifierOfIdentification);
		}

		public void TestAuthorisationNumber()
		{
			AssertNull("Should return CGL_Authorisation when available", Provider.AuthorisationNumber);
		}

		public void TestAdditionalIdentifier()
		{
			SetUpTestData();
			goodsLocation.CGL_AdditionalIdentifier = "AID";
			AssertEquals("AID", Provider.AdditionalIdentifier);
		}

		public void TestUNLOCODE()
		{
			SetUpTestData();
			goodsLocation.CGL_AdditionalIdentifier = "IEDUB";
			AssertEquals("IEDUB", Provider.UNLOCODE);
		}

		public void TestCustomsOffice()
		{
			SetUpTestData();
			AssertEquals("When CGL_Qualifier is not V", string.Empty, Provider.CustomsOffice);

			entryHeader = null;
			var provider = GetProvider();
			goodsLocation.CGL_Qualifier = "V";
			goodsLocation.CGL_AdditionalIdentifier = "IEDUB";
			var customsOffice = provider.CustomsOffice;
			CombineAssertions("When CGL_Qualifier is V", () =>
			{
				AssertEquals("customsOffice", "IEDUB", customsOffice);
				AssertSame("Cached", customsOffice, provider.CustomsOffice);
			});
		}

		public void TestGNSS()
		{
			SetUpTestData();
			AssertNull("When CGL_Qualifier is not W", Provider.GNSS);

			entryHeader = null;
			var provider = GetProvider();
			goodsLocation.CGL_Qualifier = "W";
			var address = goodsLocation.Address;
			address.E2_Latitude = 52.3142599m;
			address.E2_Longitude = -66.9437914m;
			var gnss = provider.GNSS;
			CombineAssertions("When CGL_Qualifier is W", () =>
			{
				AssertEquals("Latitude", "52.3142599", gnss.Latitude);
				AssertEquals("Longitude", "-66.9437914", gnss.Longitude);
				AssertSame("Cached", gnss, provider.GNSS);
			});
		}

		public void TestEconomicOperator()
		{
			SetUpTestData();
			AssertNull("When CGL_Qualifier is not X", Provider.GNSS);

			entryHeader = null;
			var provider = GetProvider();
			goodsLocation.CGL_Qualifier = "X";
			goodsLocation.Address.E2_GovRegNum = "AEOC";
			var economicOperator = provider.EconomicOperator;
			CombineAssertions("When CGL_Qualifier is X", () =>
			{
				AssertEquals("Id", "AEOC", economicOperator);
				AssertSame("Cached", economicOperator, provider.EconomicOperator);
			});
		}

		public void TestAddress()
		{
			SetUpTestData();
			AssertNull("When CGL_Qualifier is not Z", Provider.Address);

			entryHeader = null;
			var provider = GetProvider();
			goodsLocation.CGL_Qualifier = "Z";
			goodsLocation.Address.E2_Postcode = "D18";
			var address = provider.Address;
			CombineAssertions("When CGL_Qualifier is Z", () =>
			{
				AssertEquals("Has been constructed with goodsLocation.Address", "D18", address.Postcode);
				AssertSame("Cached", address, provider.Address);
			});
		}

		public void TestPostcodeAddress()
		{
			SetUpTestData();
			AssertNull("When CGL_Qualifier is not T", Provider.Address);

			entryHeader = null;
			var provider = GetProvider();
			goodsLocation.CGL_Qualifier = "T";
			goodsLocation.CGL_AdditionalIdentifier = "AID";
			goodsLocation.Address.E2_RN_NKCountryCode = "IE";
			goodsLocation.Address.E2_Postcode = "D18";
			var postcodeAddress = provider.PostcodeAddress;
			CombineAssertions("When CGL_Qualifier is T", () =>
			{
				AssertEquals("HouseNumber", "AID", postcodeAddress.HouseNumber);
				AssertEquals("Postcode", "D18", postcodeAddress.Postcode);
				AssertEquals("Country", "IE", postcodeAddress.Country);
				AssertSame("Cached", postcodeAddress, provider.PostcodeAddress);
			});
		}

		protected override GoodsLocationProvider GetProvider()
		{
			SetUpTestData();
			return new GoodsLocationProvider(goodsLocation);
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				goodsLocation = entryInstruction.GoodsLocation;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusGoodsLocation goodsLocation;
	}
}
