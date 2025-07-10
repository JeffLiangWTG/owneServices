using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE514MessageProviderTest : DataProviderTestCase<IE514MessageProvider>
	{
		#region IIE514Header
		public void TestExportOperation() => CombineAssertions(() =>
		{
			AssertType<IE514MessageProvider>("Type", Provider.ExportOperation);
			AssertSame("Cached", Provider, Provider.ExportOperation);
		});

		public void TestCustomsOfficeOfExportReferenceNumber()
		{
			declaration.JE_CustomsOffice = "Export123";
			AssertEquals("CustomsOfficeOfExportReferenceNumber", "Export123", Provider.CustomsOfficeOfExportReferenceNumber);
		}

		public void TestExporter()
		{
			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "TestExporter";
			declaration.SupplierDocumentaryAddress.E2_OA_Address = exporter.MainAddress.PK;
			AssertEquals("Exporter", "TestExporter", Provider.Exporter.Name);
		}

		public void TestDeclarant()
		{
			var declarantHeader = Factory.New<OrgHeader>();
			declarantHeader.OH_FullName = "TestDeclarantHeader";
			var declarant = Factory.New<OrgAddress>();
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			declarant.OA_OH = declarantHeader.PK;
			AssertEquals("Declarant", "TestDeclarantHeader", Provider.Declarant.Name);
		}

		public void TestRepresentative()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TestRepresentativeHeader";
			var address = Factory.New<OrgAddress>();
			address.OA_CompanyNameOverride = "TestRepresentativeHeader Override";
			declaration.JE_OA_Representative = address.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			address.OA_OH = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			CombineAssertions("Representantive", () =>
			{
				var representative = Provider.Representative;
				AssertEquals("Status", "2", representative.Status);
				AssertEquals("Contact.Name", "BOB THE BUILDER", representative.Contact.Name);
			});
		}

		#endregion

		#region IIE514ExportOperation Members

		public void TestLRN()
		{
			entryHeader.CH_BGMReference = "LRN2343234242";
			AssertEquals("LRN", "LRN2343234242", Provider.LRN);
		}

		public void TestMRN()
		{
			entryHeader.MovementReferenceNumberSetter("MRN001");
			AssertEquals("MRN", "MRN001", Provider.MRN);
		}

		[TestDate(2022, 5, 15, 16, 45, 35)]
		public void TestInvalidationRequestDateAndTime()
		{
			AssertEquals("InvalidationRequestDateAndTime", new DateTime(2022, 5, 15, 16, 45, 35), Provider.InvalidationRequestDateAndTime);
		}

		public void TestInvalidationReason()
		{
			sendingAction.Annotation = "Reason";
			AssertEquals("InvalidationReason", "Reason", Provider.InvalidationReason);
		}

		#endregion

		protected override IE514MessageProvider GetProvider() => new IE514MessageProvider(sendingAction);

		protected override void SetUp()
		{
			base.SetUp();
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			entryHeaderWrapper = testBizObjs.entryHeaderWrapper;
			declaration = entryHeaderWrapper.Declaration;
			entryHeader = entryHeaderWrapper.EntryHeader;
			sendingAction = new AESMessageSendingAction(entryHeaderWrapper.EntryHeader);
		}
		AESMessageSendingAction sendingAction;
		EntryHeaderWrapper entryHeaderWrapper;
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
	}
}
