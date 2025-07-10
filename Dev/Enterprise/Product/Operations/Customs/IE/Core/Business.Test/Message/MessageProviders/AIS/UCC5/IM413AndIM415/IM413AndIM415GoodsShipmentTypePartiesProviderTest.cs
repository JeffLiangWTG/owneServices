using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415GoodsShipmentTypePartiesProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentTypePartiesProvider>
	{
		public void TestIIM415GoodsShipmentTypeParties()
		{
			Assert("Should implement IIM415GoodsShipmentTypeParties", Provider is IGoodsShipmentTypeParties);
		}

		public void TestImporter_EORI()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.CGT, "C001");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.ITX, "I001");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.PYE, "P001");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E001");
			var orgAddress = orgHeader.MainAddress;
			declaration.JE_OH_Importer = orgHeader.PK;

			var importer = Provider.Importer;
			CombineAssertions(() =>
			{
				AssertEquals("Id", "IEE001", importer.ID);
				AssertNull("Address should be null when EORI populated", importer.Address);
			});
		}

		public void TestImporter_PYE()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.CGT, "C001");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.ITX, "I001");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.PYE, "P001");
			var orgAddress = orgHeader.MainAddress;
			declaration.JE_OH_Importer = orgHeader.PK;
			AssertEquals("PYEP001", Provider.Importer.ID);
		}

		public void TestImporter_ITX()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.CGT, "C001");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.ITX, "I001");
			var orgAddress = orgHeader.MainAddress;
			declaration.JE_OH_Importer = orgHeader.PK;
			AssertEquals("ITXI001", Provider.Importer.ID);
		}

		public void TestImporter_CGT()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.CGT, "C001");
			var orgAddress = orgHeader.MainAddress;
			declaration.JE_OH_Importer = orgHeader.PK;
			AssertEquals("CGTC001", Provider.Importer.ID);
		}

		public void TestImporter_NR()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgHeader.OH_FullName = "Importer 1";
			orgAddress.Address1 = "Importer Address 1";
			orgAddress.Address2 = "Address 2";
			orgAddress.OA_RN_NKCountryCode = "IE";
			orgAddress.City = "Dublin";
			orgAddress.Postcode = "D01ABCD";
			declaration.JE_OH_Importer = orgHeader.PK;

			var importer = Provider.Importer;
			CombineAssertions(() =>
			{
				AssertEquals("Id", "NR", importer.ID);
				AssertEquals("Name", "Importer 1", importer.Address.Name);
				AssertEquals("Address - Street and number", "Importer Address 1, Address 2", importer.Address.StreetAndNumber);
				AssertEquals("Address - Country", "IE", importer.Address.Country);
			});
		}

		public void TestSeller()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E002");
			var orgAddress = orgHeader.MainAddress;
			declaration.JE_OA_SellerAddress = orgAddress.PK;

			var seller = Provider.Seller;
			CombineAssertions(() =>
			{
				AssertEquals("Id", "IEE002", seller.ID);
				AssertNull("Address should be null when EORI populated", seller.Address);
			});
		}

		public void TestSellerCompanyNameOverride()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgHeader.OH_FullName = "Seller 1";
			orgAddress.OA_CompanyNameOverride = "Seller 1 Override";
			orgAddress.City = "New York";
			declaration.JE_OA_SellerAddress = orgAddress.PK;

			var seller = Provider.Seller;
			CombineAssertions(() =>
			{
				AssertEquals("Id", string.Empty, seller.ID);
				AssertEquals("Name", "Seller 1 Override", seller.Address.Name);
				AssertEquals("Address - City", "New York", seller.Address.City);
			});
		}

		public void TestBuyer()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E002");
			declaration.JE_OH_Buyer = orgHeader.PK;
			AssertEquals("Id", "IEE002", Provider.Buyer.ID);
		}

		public void TestBuyer_Null()
		{
			SetUpTestData();
			AssertNull("Null Buyer", Provider.Buyer);
		}

		public void TestBuyerCompanyNameOverride()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgHeader.OH_FullName = "Buyer 1";
			orgAddress.OA_CompanyNameOverride = "Buyer 1 Override";
			orgAddress.City = "New York";
			declaration.JE_OH_Buyer = orgHeader.PK;

			var buyer = Provider.Buyer;
			CombineAssertions(() =>
			{
				AssertEquals("Id", string.Empty, buyer.ID);
				AssertEquals("Name", "Buyer 1 Override", buyer.Address.Name);
				AssertEquals("Address - City", "New York", buyer.Address.City);
			});
		}

		public void TestAdditionalSupplyChainActor()
		{
			SetUpTestData();
			entryInstruction.CusSupplyChainActorReferences.AddNew();
			entryInstruction.CusSupplyChainActorReferences.AddNew();
			AssertEquals("2 CusSupplyChainActorReferences", 2, Provider.AdditionalSupplyChainActor.Count);
		}

		protected override IM413AndIM415GoodsShipmentTypePartiesProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentTypePartiesProvider(new EntryHeaderWrapper(entryHeader));
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
