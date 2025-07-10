using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class PDIHeaderWrapperTest : WrapperHelperTest<PDIHeaderWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("No Header", () => new PDIHeaderWrapper(null));
		}

		public void TestCustomsOffice()
		{
			declaration.JE_CustomsOffice = HeaderData.CustomsOffice;
			AssertEquals("Expected filled CustomsOffice", HeaderData.CustomsOfficeCodeComplete, wrapper.CustomsOffice);
		}

		public void TestShipmentType()
		{
			declaration.JE_MessageSubType = HeaderData.MessageSubTypePDI;
			AssertEquals("Expected filled ShipmentType", HeaderData.MessageSubTypePDI, wrapper.ShipmentType);
		}

		public void TestTotalLinesNum()
		{
			_ = entryHeader.MergedLines.AddNew();
			_ = entryHeader.MergedLines.AddNew();
			AssertEquals("Expected 3 TotalLinesNum (1 SetUp + 2 Test)", 3, wrapper.TotalLinesNum);
		}

		public void TestNullImporter()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Importer.ToString());
		}

		public void TestImporter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = orgHeader.PK;
			var importer = wrapper.Importer;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Importer", importer);
				AssertSame("Cached Importer", wrapper.Importer, importer);
			});
		}

		public void TestNullDeclarant()
		{
			declaration.Declarant.OA_OH = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
		}

		public void TestDeclarant()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.Declarant.OA_OH = orgHeader.PK;
			var declarant = wrapper.Declarant;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Declarant", declarant);
				AssertSame("Cached Declarant", wrapper.Declarant, declarant);
			});
		}

		public void TestDeclarationEmail()
		{
			const string clearanceEmail = "mail1.mail@mail.com";
			const string mailboxEmail = "mail2.mail@mail.com";
			CombineAssertions(() =>
			{
				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					AssertEquals("Expected filled DeclarationEmail with clearance email recipient when filled", clearanceEmail, wrapper.DeclarationEmail);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					declaration = Factory.New<JobDeclaration>();
					entryHeader = declaration.CustomsEntryHeaders.AddNew();
					wrapper = new PDIHeaderWrapper(entryHeader);
					AssertEquals("Expected filled DeclarationEmail with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, wrapper.DeclarationEmail);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
				{
					declaration = Factory.New<JobDeclaration>();
					entryHeader = declaration.CustomsEntryHeaders.AddNew();
					wrapper = new PDIHeaderWrapper(entryHeader);
					AssertEquals("Expected empty DeclarationEmail when clearance email recipient and mailbox email address are empty", ZString.Empty, wrapper.DeclarationEmail);
				}
			});
		}

		public void TestOtherEmail()
		{
			declaration.ZG_OtherEmailAddr = HeaderData.OtherEmail;
			AssertEquals("Expected filled OtherEmail", HeaderData.OtherEmail, wrapper.OtherEmail);
		}

		public void TestOriginCountry()
		{
			declaration.JE_GoodsOrigin = HeaderData.CountryOfOrigin;
			AssertEquals("Expected filled OriginCountry", HeaderData.CountryOfOrigin, wrapper.OriginCountry);
		}

		public void TestGoodsLocation()
		{
			CombineAssertions(() =>
			{
				entryInstruction.GoodsLocation.Address.AuthorisationNumber = "9999000002";
				AssertEquals("Expected filled GoodsLocation", "9999", wrapper.GoodsLocation);

				entryInstruction.GoodsLocation.Address.AuthorisationNumber = ZString.Empty;
				AssertEquals("Expected empty GoodsLocation", ZString.Empty, wrapper.GoodsLocation);

				entryInstruction.GoodsLocation.Address.AuthorisationNumber = "ES009999000002";
				AssertEquals("Expected filled GoodsLocation with customs code", "9999", wrapper.GoodsLocation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader);
		}

		JobDeclaration declaration;
		protected CusEntryInstruction entryInstruction;
		protected JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		PDIHeaderWrapper wrapper;

		protected virtual PDIHeaderWrapper GetWrapper(CusEntryHeader cusEntryHeader) => new PDIHeaderWrapper(cusEntryHeader);

		protected override PDIHeaderWrapper GetProvider() => wrapper;
	}
}
