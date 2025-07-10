using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EHubNativeOrgXmlSenderTest : TestCaseWithFactory
	{
		public void TestSendMessageToEHub()
		{
			using (Factory.AddDisposableService())
			{
				var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
				orgHeader.OH_Code = "ORGTOSEND";
				orgHeader.MainAddress.OA_RN_NKCountryCode = "US";
				orgHeader.MainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
				var code = orgHeader.CustomsCodes.AddNew("EIN", "12345");
				code.OK_RN_NKCodeCountry = "US";
				Factory.Save();

				var result = EHubNativeOrgXmlSenderHelper.SendMessageToEHub(orgHeader);
				AssertEquals(true, result.Succeeded);

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);
				var message = messages[0];
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.NativeDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlNativeOrganization, message.EM_MessageSubType);

				AssertStartsWith("message.EM_MessageTextDetail", string.Format(@"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{0}"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Orga".Trim(), NativeXmlInfo.Version_2011_11), message.EM_MessageTextDetail);

				var interchange = message.Interchange;
				AssertNotNull("interchange", interchange);
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.NativeDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIMessageTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_HeaderText", "<EDIDelivery><FileName></FileName><EmailSubject></EmailSubject></EDIDelivery>", interchange.EI_HeaderText);
				AssertStartsWith("interchange.EI_BodyText beginning should have Interchange Header", string.Format(@"
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""{0}"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
    <RecipientID>XHUB_US_CERTCAPTURE</RecipientID>
  </Header>
  <Body>
    <Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{1}"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Orga".Trim(), UniversalXmlInfo.Version_2011_11, NativeXmlInfo.Version_2011_11), interchange.EI_BodyText);

				AssertEndsWith("interchange.EI_BodyText end should have Interchange Footer", @"
      </OrgHeader>
    </Organization>
  </Body>
</Native>
  </Body>
</UniversalInterchange>
".Trim(), interchange.EI_BodyText);

				AssertEquals(false, message.IsInDatabase);
				AssertEquals(false, interchange.IsInDatabase);

				Factory.Save();
				AssertEquals(true, message.IsInDatabase);
				AssertEquals(true, interchange.IsInDatabase);
			}
		}

		public void TestTakeSnapshotIfNeeded()
		{
			var orgHeader = InitDataForTest();
			orgHeader.OH_FullName = "New Name";
			orgHeader.MainAddress.OA_Phone = "090";
			orgHeader.MainAddress.OA_Fax = "010";
			orgHeader.MainAddress.OA_Address1 = "Address 1";
			orgHeader.MainAddress.OA_Address2 = "Address 2";
			orgHeader.MainAddress.OA_City = "New York";
			orgHeader.MainAddress.OA_State = "NY";
			orgHeader.MainAddress.OA_PostCode = "AAA";

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "Test Contact 1";
			contact1.OC_Email = "Test.Contact1@edi.com";

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Test Contact 2";
			contact2.OC_Email = "Test.Contact2@edi.com";

			var contact3 = orgHeader.Contacts.AddNew();
			contact3.OC_ContactName = "Test Contact 3";
			contact3.OC_Email = "Test.Contact3@edi.com";

			var doc1 = contact1.Documents.AddNew();
			doc1.OD_DocumentGroup = ContactType.Receivables.Code;
			doc1.OD_DefaultContact = true;

			var doc2 = contact2.Documents.AddNew();
			doc2.OD_DocumentGroup = ContactType.All.Code;
			doc2.OD_DefaultContact = true;

			var doc3 = contact3.Documents.AddNew();
			doc3.OD_DocumentGroup = ContactType.Receivables.Code;
			doc3.OD_DefaultContact = false;

			var code2 = orgHeader.CustomsCodes.AddNew("DUM", "111222");
			code2.OK_RN_NKCodeCountry = "US";

			var code3 = orgHeader.CustomsCodes.AddNew("FIN", "333444");
			code3.OK_RN_NKCodeCountry = "AU";

			Factory.Save();

			AssertEquals("Precondition", false, orgHeader.HasChanges);

			using (EDIDataRegistry.Instance.SendOrganizationDataToCertCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SendOrganizationDataToCertCapture { EnableSend = false }))
			{
				orgHeader.TakeSnapshotIfNeeded();
				AssertNull(orgHeader.Snapshot);
			}

			using (EDIDataRegistry.Instance.SendOrganizationDataToCertCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SendOrganizationDataToCertCapture { EnableSend = true }))
			{
				orgHeader.OH_IsActive = false;
				orgHeader.TakeSnapshotIfNeeded();
				AssertNull(orgHeader.Snapshot);

				orgHeader.OH_IsActive = true;
				orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
				orgHeader.TakeSnapshotIfNeeded();
				AssertNull(orgHeader.Snapshot);

				orgHeader.MainAddress.OA_RN_NKCountryCode = "US";
				orgHeader.CompanyData.OB_IsDebtor = false;
				orgHeader.TakeSnapshotIfNeeded();
				AssertNull(orgHeader.Snapshot);

				orgHeader.CompanyData.OB_IsDebtor = true;
				orgHeader.TakeSnapshotIfNeeded();
				AssertNotNull(orgHeader.Snapshot);

				var currentSnapshot = new SnapshotForEHubNativeOrg
				{
					CustomerNumber = "ORGTOSEND",
					CustomerName = "New Name",
					ContactName = "Test Contact 1",
					Phone = "090",
					Fax = "010",
					EmailAddress = "Test.Contact1@edi.com",
					FEIN = "12345",
					Address1 = "Address 1",
					Address2 = "Address 2",
					City = "New York",
					State = "NY",
					Country = "US",
					Zip = "AAA",
				};

				Assert(currentSnapshot.AreEqual(orgHeader.Snapshot));

				var newOrgHeader = Factory.New<EDIOrgHeader>();
				newOrgHeader.TakeSnapshotIfNeeded();
				AssertNull(newOrgHeader.Snapshot);
			}
		}

		public void TestSetSnapshotCompanyDataChangedIfNeeded()
		{
			var orgHeader = InitDataForTest();
			Factory.Save();

			using (EDIDataRegistry.Instance.SendOrganizationDataToCertCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SendOrganizationDataToCertCapture { EnableSend = true }))
			{
				AssertNull(orgHeader.Snapshot);
				AssertNoExceptionThrown(() => orgHeader.SetSnapshotCompanyDataChangedIfNeeded());
				AssertNull(orgHeader.Snapshot);

				orgHeader.TakeSnapshotIfNeeded();
				AssertEquals(false, orgHeader.Snapshot.CompanyDataChanged);

				orgHeader.SetSnapshotCompanyDataChangedIfNeeded();
				AssertEquals(false, orgHeader.Snapshot.CompanyDataChanged);

				orgHeader.CompanyData.OB_APPaymentTerms = "CCC";
				orgHeader.SetSnapshotCompanyDataChangedIfNeeded();
				AssertEquals(true, orgHeader.Snapshot.CompanyDataChanged);

				orgHeader.CompanyData.OB_IsDebtor = false;
				orgHeader.SetSnapshotCompanyDataChangedIfNeeded();
				AssertEquals(false, orgHeader.Snapshot.CompanyDataChanged);
			}
		}

		public void TestSendMessageToEHubAndUpdateSnapshotIfNeeded()
		{
			using (Factory.AddDisposableService())
			{
				var orgHeader = InitDataForTest();
				Factory.Save();

				using (EDIDataRegistry.Instance.SendOrganizationDataToCertCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SendOrganizationDataToCertCapture { EnableSend = false }))
				{
					orgHeader.SendMessageToEHubAndUpdateSnapshotIfNeeded();
					AssertNull(orgHeader.Snapshot);
					var ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(0, ediMessages.Length);
				}

				using (EDIDataRegistry.Instance.SendOrganizationDataToCertCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SendOrganizationDataToCertCapture { EnableSend = true }))
				{
					orgHeader.SendMessageToEHubAndUpdateSnapshotIfNeeded();
					AssertNotNull(orgHeader.Snapshot);
					var ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(1, ediMessages.Length);

					orgHeader.OH_IsActive = false;
					orgHeader.SendMessageToEHubAndUpdateSnapshotIfNeeded();
					AssertNull(orgHeader.Snapshot);
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(1, ediMessages.Length);

					orgHeader.OH_IsActive = true;

					orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
					orgHeader.SendMessageToEHubAndUpdateSnapshotIfNeeded();
					AssertNull(orgHeader.Snapshot);
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(1, ediMessages.Length);

					orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
					orgHeader.SendMessageToEHubAndUpdateSnapshotIfNeeded();
					AssertNull(orgHeader.Snapshot);
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(1, ediMessages.Length);

					orgHeader.MainAddress.OA_RN_NKCountryCode = "US";
					orgHeader.SendMessageToEHubAndUpdateSnapshotIfNeeded();
					AssertNotNull(orgHeader.Snapshot);
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(2, ediMessages.Length);
				}
			}
		}

		public void TestSendMessageToEHubForAllUsReceivableOrganizations()
		{
			using (Factory.AddDisposableService())
			{
				Db.Connection.ExecuteNonQuery("UPDATE dbo.OrgCompanyData SET OB_IsDebtor = 0");

				var orgHeader1 = Factory.NewWithValidTestData<EDIOrgHeader>();
				orgHeader1.OH_Code = "ORGTOSEND1";
				orgHeader1.MainAddress.OA_RN_NKCountryCode = "US";
				orgHeader1.MainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
				var code1 = orgHeader1.CustomsCodes.AddNew("EIN", "12345");
				code1.OK_RN_NKCodeCountry = "US";

				var orgHeader2 = Factory.NewWithValidTestData<EDIOrgHeader>();
				orgHeader2.OH_Code = "ORGTOSEND2";
				orgHeader2.MainAddress.OA_RN_NKCountryCode = "AU";
				orgHeader2.MainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
				var code2 = orgHeader2.CustomsCodes.AddNew("EIN", "12345");
				code2.OK_RN_NKCodeCountry = "US";

				var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
				companyData1.OB_IsDebtor = true;
				companyData1.OB_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
				companyData1.OB_OH = orgHeader1.PK;

				var companyData2 = Factory.NewWithValidTestData<OrgCompanyData>();
				companyData2.OB_IsDebtor = true;
				companyData2.OB_GC = companyData1.OB_GC;
				companyData2.OB_OH = orgHeader2.PK;

				Factory.Save();

				var messages = new List<(string, int)>();

				var result = new EHubNativeOrgXmlSender().SendMessageToEHubForAllUsReceivableOrganizations((u, v) =>
				{
					messages.Add((u, v));
				});

				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(1, ediMessages.Length);
				Assert(ediMessages.All(u => u.IsInDatabase));
				AssertEquals("Successfully sent 1 organization(s), failed to sent 0 organization(s)", result);
				AssertContainsExactElementsInExactOrder(new[]
				{
				("Send organization(s) starts", 0),
				("Found 1 organization(s)", 0),
				($"Send {orgHeader1.OH_Code} - {orgHeader1.OH_FullName} finished", 100),
				("Saving to database", 100),
				("Send organization(s) ends", 100),
			}, messages);

				var orgHeader3 = Factory.NewWithValidTestData<EDIOrgHeader>();
				orgHeader3.OH_Code = "ORGTOSEND3";
				orgHeader3.MainAddress.OA_RN_NKCountryCode = "US";
				orgHeader3.MainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);

				var companyData3 = Factory.NewWithValidTestData<OrgCompanyData>();
				companyData3.OB_IsDebtor = true;
				companyData3.OB_GC = companyData1.OB_GC;
				companyData3.OB_OH = orgHeader3.PK;

				var orgHeader4 = Factory.NewWithValidTestData<EDIOrgHeader>();
				orgHeader4.OH_Code = "ORGTOSEND4";
				orgHeader4.MainAddress.OA_RN_NKCountryCode = "US";
				orgHeader4.MainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);

				var companyData4 = Factory.NewWithValidTestData<OrgCompanyData>();
				companyData4.OB_IsDebtor = true;
				companyData4.OB_GC = companyData1.OB_GC;
				companyData4.OB_OH = orgHeader4.PK;

				Factory.Save();

				messages.Clear();

				result = new EHubNativeOrgXmlSenderForBatchTest().SendMessageToEHubForAllUsReceivableOrganizations((u, v) =>
				{
					messages.Add((u, v));
				});

				AssertEquals("Successfully sent 3 organization(s), failed to sent 0 organization(s)", result);
				AssertContainsExactElementsInAnyOrder(new[]
				{
				"Send organization(s) starts",
				"Found 3 organization(s)",
				$"Send {orgHeader1.OH_Code} - {orgHeader1.OH_FullName} finished",
				$"Send {orgHeader3.OH_Code} - {orgHeader3.OH_FullName} finished",
				"Saving to database",
				$"Send {orgHeader4.OH_Code} - {orgHeader4.OH_FullName} finished",
				"Saving to database",
				"Send organization(s) ends",
			}, messages.Select(u => u.Item1));

				ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(4, ediMessages.Length);
				Assert(ediMessages.All(u => u.IsInDatabase));
			}
		}

		public void TestSendMessageToEHubForAllUsReceivableOrganizations_SomeOrganizationsFailedToSend()
		{
			Db.Connection.ExecuteNonQuery("UPDATE dbo.OrgCompanyData SET OB_IsDebtor = 0");

			var orgHeader1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgHeader1.OH_Code = "ORGTOSEND1";
			orgHeader1.MainAddress.OA_RN_NKCountryCode = "US";
			orgHeader1.MainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);

			var orgHeader2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgHeader2.OH_Code = "ORGTOSEND2";
			orgHeader2.MainAddress.OA_RN_NKCountryCode = "US";
			orgHeader2.MainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);

			var orgHeader3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgHeader3.OH_Code = "ORGTOSEND3";
			orgHeader3.MainAddress.OA_RN_NKCountryCode = "US";
			orgHeader3.MainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_IsDebtor = true;
			companyData1.OB_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			companyData1.OB_OH = orgHeader1.PK;

			var companyData2 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData2.OB_IsDebtor = true;
			companyData2.OB_GC = companyData1.OB_GC;
			companyData2.OB_OH = orgHeader2.PK;

			var companyData3 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData3.OB_IsDebtor = true;
			companyData3.OB_GC = companyData1.OB_GC;
			companyData3.OB_OH = orgHeader3.PK;

			Factory.Save();

			var messages = new List<(string, int)>();

			using (Factory.AddDisposableService())
			{
				var result = new EHubNativeOrgXmlSenderForFailedTest().SendMessageToEHubForAllUsReceivableOrganizations((u, v) =>
				{
					messages.Add((u, v));
				});

				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertStartsWith("Start with", @"Successfully sent 2 organization(s), failed to sent 1 organization(s)
Send ORGTOSEND2 -  failed:  Dummy", result);
				AssertEquals(2, ediMessages.Length);
				Assert(ediMessages.All(u => u.IsInDatabase));
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"Send organization(s) starts",
					"Found 3 organization(s)",
					$"Send {orgHeader1.OH_Code} - {orgHeader1.OH_FullName} finished",
					$"Send {orgHeader2.OH_Code} - {orgHeader2.OH_FullName} failed:  Dummy",
					$"Send {orgHeader3.OH_Code} - {orgHeader3.OH_FullName} finished",
					"Saving to database",
					"Send organization(s) ends",
				}, messages.Select(u => u.Item1));
			}
		}

		public void TestSendMessageToEHubAndUpdateSnapshotIfNeeded_ValueChanged()
		{
			using (Factory.AddDisposableService())
			{
				var orgHeader = InitDataForTest();
				var code = orgHeader.CustomsCodes[0];
				var contact = orgHeader.Contacts.AddNew();
				contact.OC_ContactName = "Test Contact";
				contact.OC_Email = "Test.Contact@edi.com";

				var doc = contact.Documents.AddNew();
				doc.OD_DocumentGroup = ContactType.Receivables.Code;
				doc.OD_DefaultContact = true;

				var companyData = orgHeader.CompanyDataCollection.AddNew();
				companyData.OB_IsDebtor = false;
				companyData.OB_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
				companyData.OB_OH = orgHeader.PK;

				Factory.Save();

				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(0, ediMessages.Length);

				using (EDIDataRegistry.Instance.SendOrganizationDataToCertCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SendOrganizationDataToCertCapture { EnableSend = true }))
				{
					DoSend(() => orgHeader.OH_FullName = "New Name");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(1, ediMessages.Length);
					AssertEquals("New Name", orgHeader.Snapshot?.CustomerName);

					DoSend(() => orgHeader.OH_FullName = "New Name");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(1, ediMessages.Length);
					AssertEquals("New Name", orgHeader.Snapshot?.CustomerName);

					DoSend(() => orgHeader.OH_Code = "NEWCODE");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(2, ediMessages.Length);
					AssertEquals("NEWCODE", orgHeader.Snapshot?.CustomerNumber);

					DoSend(() => orgHeader.OH_FullName = "Another Name");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(3, ediMessages.Length);
					AssertEquals("Another Name", orgHeader.Snapshot?.CustomerName);

					DoSend(() => code.OK_CustomsRegNo = "54321");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(4, ediMessages.Length);

					DoSend(() => orgHeader.MainAddress.OA_Phone = "0555");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(5, ediMessages.Length);

					DoSend(() => orgHeader.MainAddress.OA_Fax = "010");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(6, ediMessages.Length);

					DoSend(() => orgHeader.MainAddress.OA_Address1 = "New Address 1");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(7, ediMessages.Length);

					DoSend(() => orgHeader.MainAddress.OA_Address2 = "New Address 2");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(8, ediMessages.Length);

					DoSend(() => orgHeader.MainAddress.OA_City = "New City");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(9, ediMessages.Length);

					DoSend(() => orgHeader.MainAddress.OA_State = "NEW");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(10, ediMessages.Length);

					DoSend(() => orgHeader.MainAddress.OA_RN_NKCountryCode = "AU");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals("Not create message when country is not US", 10, ediMessages.Length);
					AssertNull(orgHeader.Snapshot);

					DoSend(() => orgHeader.MainAddress.OA_RN_NKCountryCode = "US");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals("Create message when country is US", 11, ediMessages.Length);
					AssertNotNull(orgHeader.Snapshot);

					DoSend(() => orgHeader.MainAddress.OA_PostCode = "0555");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(12, ediMessages.Length);

					DoSend(() => contact.OC_ContactName = "New Name");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(13, ediMessages.Length);

					DoSend(() => contact.OC_Email = "Test.Contact.New@edi.com");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(14, ediMessages.Length);

					var lastMessage = ediMessages.OrderByDescending(u => u.EM_MessageNum).First();
					AssertContains(contact.OC_Email, lastMessage.EM_MessageTextDetail);

					DoSend(() => companyData.OB_APPaymentTerms = "CCC");
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(14, ediMessages.Length);

					DoSend(() => companyData.OB_IsDebtor = true);
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(15, ediMessages.Length);
				}

				void DoSend(Action changeValue)
				{
					orgHeader.TakeSnapshotIfNeeded();
					changeValue();
					orgHeader.SetSnapshotCompanyDataChangedIfNeeded();
					Factory.Save();
					orgHeader.SendMessageToEHubAndUpdateSnapshotIfNeeded();
				}
			}
		}

		EDIOrgHeader InitDataForTest()
		{
			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgHeader.OH_Code = "ORGTOSEND";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "US";
			orgHeader.MainAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			var code = orgHeader.CustomsCodes.AddNew("EIN", "12345");
			code.OK_RN_NKCodeCountry = "US";
			orgHeader.CompanyData.OB_IsDebtor = true;

			return orgHeader;
		}

		class EHubNativeOrgXmlSenderForBatchTest : EHubNativeOrgXmlSender
		{
			protected override FilteredBusinessObjectReader<EDIOrgHeader> GetReader(ZDBOnlyQuery query)
			{
				var reader = base.GetReader(query);
				reader.BatchSize = 2;

				return reader;
			}
		}

		class EHubNativeOrgXmlSenderForFailedTest : EHubNativeOrgXmlSender
		{
			protected override IDeliveryResult SendMessageToEHub(EDIOrgHeader orgHeader)
			{
				if (orgHeader.OH_Code == "ORGTOSEND2")
				{
					return DeliveryResult.Error(new Exception("Dummy"));
				}

				return base.SendMessageToEHub(orgHeader);
			}
		}
	}
}
