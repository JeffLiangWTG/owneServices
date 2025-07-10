using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaManifestUniversalMessagingProcessorTest : TestCaseWithFactory
	{
		public void TestGetManifestFromBusinessObject_ExcludeZAOutturnGateInOut()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RL_NKDischargePort = "VUAUY";
			consol1.JK_UniqueConsignRef = "C456";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RL_NKDischargePort = "VUAUY";
			consol2.JK_UniqueConsignRef = "C789";

			var testHeader1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			testHeader1.AMA_JobReference = "VV1";
			testHeader1.AMA_ParentId = consol1.PK;
			testHeader1.AMA_ParentTableCode = consol1.TablePrefix;

			var testHeader2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			testHeader2.AMA_ApplicationCode = "OUT";
			testHeader2.AMA_JobReference = "VV2";
			testHeader2.AMA_ParentId = consol2.PK;
			testHeader2.AMA_ParentTableCode = consol2.TablePrefix;

			Factory.Save();

			AssertEquals(testHeader1, AsycudaManifestUniversalMessagingProcessor.GetManifestFromBusinessObject(consol1));
			AssertNull(AsycudaManifestUniversalMessagingProcessor.GetManifestFromBusinessObject(consol2));
		}

		public void TestProcessManifestLevel()
		{
			var globalManifestApplicationBusinessProvider = new List<ApplicationBusinessProvider>(new[] { new DummyApplicationBusinessProvider("ASY", new[] { Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Eritrea }) });
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", globalManifestApplicationBusinessProvider))
			{
				//	// Create manifest with manifest level messaging
				//	// Execute processor and check that the EDIInterchange and IDEMessage are in correct format
				//	// Check that AsycudaManifest.AMA_MessageStatus is correctly set
				PrepareCusCodeDataForTesting();

				var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
				var vuPK = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "US", "US", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
				helper.CreateNewOrGetExistingCusCodeListAttribute(vuPK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");

				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				var header = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting("USLAX", "ASY", new BusinessObjectFactory());
				header.Factory.Save();
				header = Factory.Load<AsycudaManifestHeader>(header.PK);

				header.AMA_ManifestType = "ASY";
				var bill1 = header.Bills[0];
				header.SetParent(consol);

				var processor = new AsycudaManifestUniversalMessagingProcessor(consol, MessageRecipientPartyTypeList.Codes.USAirAMS);
				var notification = new NotificationsForTest();

				processor.Process(notification);

				AssertEquals(1, header.Messages.Count);
				var message = header.Messages[0];
				AssertContains(bill1.ABL_BillNumber, message.EM_MessageText);
				AssertMessageFormat(message, "US");

				AssertEquals(MessageStatusCodeList.Codes.Awaiting, header.AMA_MessageStatus);
				AssertNullOrEmpty(bill1.ABL_MessageStatus);
			}
		}

		public void TestProcessBillLevel()
		{
			PrepareCusCodeDataForTesting();
			var consol = Factory.New<ForwardingConsol>();
			var header = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting("USLAX", "IAM", Factory);
			header.SetParent(consol);
			header.AMA_ManifestType = "IAM";
			var bill = header.Bills[0];
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load<ForwardingConsol>(consol.PK);
			header = newFactory.Load<AsycudaManifestHeader>(header.PK);
			bill = newFactory.Load<AsycudaBill>(bill.PK);

			var processor = new AsycudaManifestUniversalMessagingProcessor(consol, MessageRecipientPartyTypeList.Codes.USAirAMS);
			var notification = new NotificationsForTest();

			processor.Process(notification);

			AssertEquals(1, header.Messages.Count);
			var message = header.Messages[0];
			AssertContains(bill.ABL_BillNumber, message.EM_MessageText);
			AssertMessageFormat(message, "US Air AMS");

			AssertEquals(MessageStatusCodeList.Codes.Awaiting, bill.ABL_MessageStatus);
			AssertNullOrEmpty(header.AMA_MessageStatus);
		}

		public void TestProcessBillLevelNewBillsOnly()
		{
			PrepareCusCodeDataForTesting();
			var consol = Factory.New<ForwardingConsol>();
			var header = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting("USLAX", "IAM", new BusinessObjectFactory());
			header.Factory.Save();
			header = Factory.Load<AsycudaManifestHeader>(header.PK);
			header.SetParent(consol);
			header.AMA_ManifestType = "IAM";
			var bill1 = header.Bills[0];

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HAWB56";
			bill2.ABL_MessageStatus = EDIMessage.Status.Sent;
			var bill3 = header.Bills.AddNew();
			bill3.ABL_BillNumber = "HAWB78";

			var processor = new AsycudaManifestUniversalMessagingProcessor(consol, MessageRecipientPartyTypeList.Codes.USAirAMS);
			var notification = new NotificationsForTest();

			processor.Process(notification);

			AssertEquals(1, header.Messages.Count);
			var message = header.Messages[0];
			AssertContains(bill1.ABL_BillNumber, message.EM_MessageText);
			AssertContains(bill3.ABL_BillNumber, message.EM_MessageText);
			AssertNotContains(bill2.ABL_BillNumber, message.EM_MessageText);
			AssertMessageFormat(message, "US Air AMS");
		}

		public void TestProcessBillLevelNoNewBills()
		{
			PrepareCusCodeDataForTesting();
			var consol = Factory.New<ForwardingConsol>();
			var header = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting("USLAX", "IAM", Factory);
			header.SetParent(consol);
			header.AMA_ManifestType = "IAM";
			var bill1 = header.Bills[0];
			bill1.ABL_MessageStatus = EDIMessage.Status.Sent;

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HAWB56";
			bill2.ABL_MessageStatus = EDIMessage.Status.Sent;

			var processor = new AsycudaManifestUniversalMessagingProcessor(consol, MessageRecipientPartyTypeList.Codes.USAirAMS);
			var notification = new NotificationsForTest();

			processor.Process(notification);

			AssertEquals(0, header.Messages.Count);
			AssertContains("No suitable bills", notification.ToString());
		}

		void AssertMessageFormat(EDIMessage message, string eiTo)
		{
			AssertEquals("UDM", message.EM_ApplicationCode);
			AssertEquals("XUS", message.EM_MessageType);
			AssertEquals("XUS", message.EM_MessageSubType);
			AssertEquals("TRX", message.EM_ReceiveTransmit);
			AssertEquals("SNT", message.EM_Status);
			var interchange = message.Interchange;
			AssertEquals("HQU", interchange.EI_Status);
			AssertEquals(eiTo, interchange.EI_To);
		}

		void PrepareCusCodeDataForTesting()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var usVAIR = helper.CreateNewOrGetExistingCusCodeList("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "VAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(usVAIR.PK, "AIR", "VUVLI");

			Factory.Save();
		}
	}
}
