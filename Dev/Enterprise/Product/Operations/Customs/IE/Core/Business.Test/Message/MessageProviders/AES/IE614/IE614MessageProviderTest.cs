using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE614MessageProviderTest : DataProviderTestCase<IE614MessageProvider>
	{
		#region IIE614Header Members
		public void TestExportOperation() => CombineAssertions(() =>
		{
			AssertType<IE614MessageProvider>("Type", Provider.ExportOperation);
			AssertSame("Cached", Provider, Provider.ExportOperation);
		});

		public void TestCustomsOfficeOfExitReferenceNumber()
		{
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "Exit222");
			AssertEquals("CustomsOfficeOfExitReferenceNumber", "Exit222", Provider.CustomsOfficeOfExitReferenceNumber);
		}

		public void TestDeclarant()
		{
			AssertType<PartyProvider>("Declarant", Provider.Declarant);
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

		#region IIE614ExportOperation Members
		public void TestInvalidationReason()
		{
			sendingAction.Annotation = "Reason";
			AssertEquals("InvalidationReason", "Reason", Provider.InvalidationReason);
		}

		[TestDate(2022, 5, 15, 16, 45, 35)]
		public void TestInvalidationRequestDateAndTime()
		{
			AssertEquals("InvalidationRequestDateAndTime", new DateTime(2022, 5, 15, 16, 45, 35), Provider.InvalidationRequestDateAndTime);
		}

		public void TestMRN()
		{
			entryHeader.MovementReferenceNumberSetter("MRN001");
			AssertEquals("MRN", "MRN001", Provider.MRN);
		}
		#endregion

		protected override IE614MessageProvider GetProvider() => new IE614MessageProvider(sendingAction);

		protected override void SetUp()
		{
			base.SetUp();
			(entryHeaderWrapper, _) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			sendingAction = new AESMessageSendingAction(entryHeaderWrapper.EntryHeader);
			entryHeader = entryHeaderWrapper.EntryHeader;
			declaration = entryHeaderWrapper.Declaration;
		}

		AESMessageSendingAction sendingAction;
		EntryHeaderWrapper entryHeaderWrapper;
		CusEntryHeader entryHeader;
		JobDeclaration declaration;
	}
}
