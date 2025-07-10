using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class RF415GoodsLocationProviderTest : DataProviderTestCase<RF415GoodsLocationProvider>
	{
		public void TestQualifierIdentification()
		{
			SetUpTestData();
			location.CGL_Qualifier = "T";
			AssertEquals("QualifierIdentification", "T", Provider.QualifierIdentification);
		}

		public void TestIdentification()
		{
			SetUpTestData();
			location.CGL_AdditionalIdentifier = "MAL";
			AssertEquals("Identification", "MAL", Provider.Identification);
		}

		public void TestAdditionalIdentifier()
		{
			SetUpTestData();
			AssertEquals("AdditionalIdentifier", string.Empty, Provider.AdditionalIdentifier);
		}

		public void TestLocationTypeCode()
		{
			SetUpTestData();
			location.CGL_Type = "A";
			AssertEquals("LocationTypeCode", "A", Provider.LocationTypeCode);
		}

		public void TestPostcode()
		{
			SetUpTestData();
			AssertEquals("Postcode", "A12B3C4", Provider.Postcode);
		}

		public void TestStreetAndNumber()
		{
			SetUpTestData();
			AssertEquals("StreetAndNumber", "Address 1, Address 2", Provider.StreetAndNumber);
		}

		public void TestCountry()
		{
			SetUpTestData();
			declaration.JE_ApplicationCode = "V1";
			AssertEquals("Country", "IE", Provider.Country);

			declaration.JE_ApplicationCode = "V2";
			AssertEquals("Country", "AD", Provider.Country);
		}

		public void TestCity()
		{
			SetUpTestData();
			AssertEquals("City", "Dublin", Provider.City);
		}

		protected override RF415GoodsLocationProvider GetProvider()
		{
			SetUpTestData();
			return new RF415GoodsLocationProvider(location);
		}

		void SetUpTestData()
		{
			if (sendingAction == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				sendingAction = new RefundApplicationMessageSendingAction(entryHeader);

				location = entryInstruction.GoodsLocation;
				var address = Factory.New<OrgAddress>();
				address.OA_Address1 = "Address 1";
				address.OA_Address2 = "Address 2";
				address.OA_City = "Dublin";
				address.OA_PostCode = "A12B3C4";
				address.OA_RN_NKCountryCode = "AD";
				location.Address.E2_OA_Address = address.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		RefundApplicationMessageSendingAction sendingAction;
		CusGoodsLocation location;
	}
}
