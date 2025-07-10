using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNJobDeclarationUniversalMessagingHelperTest : JobDeclarationUniversalMessagingHelperTest
	{
		public void TestEventReferenceAndCHStatus()
		{
			using (Factory.AddDisposableService())
			{
				AssertEventReferenceAndCHStatus(false, ClearanceModeList.Codes.Integrated, JobMessageStatusList.Codes.ClearedPreliminaryDeclaration, "SW0", expectedStatus: JobMessageStatusList.Codes.AwaitingResponseIntegratedDeclaration);
				AssertEventReferenceAndCHStatus(true, ClearanceModeList.Codes.Integrated, JobMessageStatusList.Codes.ClearedPreliminaryDeclaration, "SW0", expectedStatus: JobMessageStatusList.Codes.AwaitingResponseIntegratedDeclaration);
				AssertEventReferenceAndCHStatus(false, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.ClearedPreliminaryDeclaration, "SW0", expectedStatus: JobMessageStatusList.Codes.AwaitingResponseIntegratedDeclaration);
				AssertEventReferenceAndCHStatus(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.ClearedPreliminaryDeclaration, "SW2", expectedStatus: JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration);
				AssertEventReferenceAndCHStatus(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration, "SW2", expectedStatus: JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration);
				AssertEventReferenceAndCHStatus(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.AcknowledgedCompletedDeclaration, "SW2", expectedStatus: JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration);
				AssertEventReferenceAndCHStatus(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.ErrorCompletedDeclaration, "SW2", expectedStatus: JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration);
				AssertEventReferenceAndCHStatus(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.ClearedCompletedDeclaration, "SW2", expectedStatus: JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration);
				AssertEventReferenceAndCHStatus(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.AcknowledgedPreliminaryDeclaration, "SW1", expectedStatus: JobMessageStatusList.Codes.AwaitingResponsePreliminaryDeclaration);
				AssertEventReferenceAndCHStatus(true, ClearanceModeList.Codes.TwoStep, JobMessageStatusList.Codes.AwaitingResponsePreliminaryDeclaration, "SW1", expectedStatus: JobMessageStatusList.Codes.AwaitingResponsePreliminaryDeclaration);
			}
		}

		void AssertEventReferenceAndCHStatus(bool twoStepActivate, ZString xcClearanceMode, ZString chStatus, ZString expectedMST, ZString expectedStatus, string chMessageType = EntryTypeList.Codes.CustomsEntry)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ClearanceMode = xcClearanceMode;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_JE = declaration.PK;
			entry.CH_MessageType = chMessageType;
			entry.CH_Status = chStatus;
			entry.CH_BGMReference = "CUS202109030000001";
			Factory.Save();
			var wrapper = new CNJobDeclarationMessageSendingObjectParent(declaration);
			var helper = new CNJobDeclarationUniversalMessagingHelper(wrapper);
			foreach (JobDeclarationMessageSendingObject sendingObject in wrapper.SendingObjectsCollection)
			{
				sendingObject.ShouldSend = true;
			}

			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, twoStepActivate))
			{
				helper.SendUniversalMessage();
			}

			Factory.Save();
			var createdMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entry.PK));
			var interchange = createdMessage.Interchange;
			var expectedEventReference = $"<EventReference>MST={expectedMST}|RFN={createdMessage.EM_MessageNum}</EventReference>";
			AssertTextContainsDispiteBlanks(expectedEventReference, interchange.EI_BodyText);
			AssertEquals("CH_Status after sending.", expectedStatus, entry.CH_Status);
			AssertEquals("Should add an MSN event", "Send to Customs;CUS202109030000001", entry.Logs.MostRecentLogByEventTime(Events.MessageSent).SL_Reference);
		}

		public void TestShouldPopulateAttachedDocumentCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = JobMessageStatusList.Codes.AcknowledgedIntegratedDeclaration;
			var wrapper = new CNJobDeclarationMessageSendingObjectParent(declaration);
			var helper = new CNJobDeclarationUniversalMessagingHelper(wrapper);
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shouldPopulate = (ZBool)typeof(CNJobDeclarationUniversalMessagingHelper).GetProperty("ShouldPopulateAttachedDocumentCollection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(helper);
				AssertEquals("Status not fit, ShouldPopulateAttachedDocumentCollection should return false.", false, shouldPopulate);
				entryHeader.CH_Status = JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration;
				shouldPopulate = (ZBool)typeof(CNJobDeclarationUniversalMessagingHelper).GetProperty("ShouldPopulateAttachedDocumentCollection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(helper);
				AssertEquals("None selected,  ShouldPopulateAttachedDocumentCollection should return false.", false, shouldPopulate);
				(wrapper.SendingObjectsCollection.FirstOrDefault() as CNJobDeclarationMessageSendingObject).ShouldSend = true;
				shouldPopulate = (ZBool)typeof(CNJobDeclarationUniversalMessagingHelper).GetProperty("ShouldPopulateAttachedDocumentCollection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(helper);
				AssertEquals(true, shouldPopulate);
			}
		}

		public void TestLocalAddress()
		{
			using (Factory.AddDisposableService())
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				var entry = declaration.ActiveEntryHeaders.AddNew();
				entry.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
				var supplier = declaration.Supplier;
				if (supplier == null)
				{
					supplier = Factory.NewWithValidTestData<OrgHeader>();
					declaration.JE_OH_Supplier = supplier.PK;
				}

				var supplierAddress = supplier.Addresses.Cast<OrgAddress>().FirstOrDefault();
				if (supplierAddress == null)
				{
					supplierAddress = supplier.Addresses.AddNew();
					supplierAddress.OA_OH = supplier.PK;
				}

				supplierAddress.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
				var supplierAddTrans = supplierAddress.TranslatedAddresses.AddNew();
				supplierAddTrans.Language = Core.SharedConstants.Languages.ChineseSimplified;
				supplierAddTrans.OTA_CompanyName = "测试公司1";
				supplierAddTrans.OTA_Address1 = "Test Address 1";
				supplierAddTrans.OTA_Address2 = "测试地址1";
				supplierAddTrans.City = "测试城市1";
				supplierAddTrans.State = "中国";
				supplierAddTrans.Postcode = "100001";
				var importer = declaration.Importer;
				if (importer == null)
				{
					importer = Factory.NewWithValidTestData<OrgHeader>();
					declaration.JE_OH_Importer = importer.PK;
				}

				var importerAddress = importer.Addresses.Cast<OrgAddress>().FirstOrDefault();
				if (importerAddress == null)
				{
					importerAddress = importer.Addresses.AddNew();
					importerAddress.OA_OH = importer.PK;
				}

				importerAddress.OA_Language = Core.SharedConstants.Languages.English;
				var importerAddTrans = importerAddress.TranslatedAddresses.AddNew();
				importerAddTrans.Language = Core.SharedConstants.Languages.English;
				importerAddTrans.OTA_CompanyName = "测试公司2";
				importerAddTrans.OTA_Address1 = "Test Address 2";
				importerAddTrans.OTA_Address2 = "测试地址2";
				importerAddTrans.City = "测试城市2";
				importerAddTrans.State = "美国";
				importerAddTrans.Postcode = "100002";
				Factory.Save();
				var wrapper = new CNJobDeclarationMessageSendingObjectParent(declaration);
				var helper = new CNJobDeclarationUniversalMessagingHelper(wrapper);
				foreach (JobDeclarationMessageSendingObject sendingObject in wrapper.SendingObjectsCollection)
				{
					sendingObject.ShouldSend = true;
				}

				helper.SendUniversalMessage();
				Factory.Save();
				var createdMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entry.PK));
				var interchange = createdMessage.Interchange;
				var expectedAddress1 = @"
			<LocalAddressCollection>
				<LocalAddress>
					<Address1>Test Address 1</Address1>
					<Address2>测试地址1</Address2>
					<City>测试城市1</City>
					<CompanyName>测试公司1</CompanyName>
					<Language>
						<Code>ZH-CN</Code>
						<Description>Chinese - Simplified</Description>
					</Language>
					<Postcode>100001</Postcode>
					<State>中国</State>
				</LocalAddress>
			</LocalAddressCollection>";
				var expectedAddress2 = @"
			<LocalAddressCollection>
				<LocalAddress>
					<Address1>Test Address 2</Address1>
					<Address2>测试地址2</Address2>
					<City>测试城市2</City>
					<CompanyName>测试公司2</CompanyName>
					<Language>
						<Code>EN</Code>
						<Description>English</Description>
					</Language>
					<Postcode>100002</Postcode>
					<State>美国</State>
				</LocalAddress>
			</LocalAddressCollection>";
				AssertTextContainsDispiteBlanks(expectedAddress1, interchange.EI_BodyText);
				AssertTextContainsDispiteBlanks(expectedAddress2, interchange.EI_BodyText);
			}
		}

		public void TestPackingLineCollection()
		{
			using (Factory.AddDisposableService())
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				var entry = declaration.ActiveEntryHeaders.AddNew();
				entry.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
				var container1 = declaration.CusContainers.AddNew();
				container1.CO_ContainerNumber = "CONT1";
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_Description = "DESC1";
				Factory.Save();
				var wrapper = new CNJobDeclarationMessageSendingObjectParent(declaration);
				var helper = new CNJobDeclarationUniversalMessagingHelper(wrapper);
				foreach (JobDeclarationMessageSendingObject sendingObject in wrapper.SendingObjectsCollection)
				{
					sendingObject.ShouldSend = true;
				}

				helper.SendUniversalMessage();
				Factory.Save();
				var expectedPackingData = @"
				<PackingLineCollection Content=""Complete"">
					<PackingLine>
						<ContainerNumber>CONT1</ContainerNumber>
						<PackedItemCollection>
							<PackedItem>
								<CommercialInvoiceLineLink>1</CommercialInvoiceLineLink>
								<GoodsValue>0</GoodsValue>
								<GrossWeight>0</GrossWeight>
								<GrossWeightUnit>
									<Code>KG</Code>
									<Description>Kilogram</Description>
								</GrossWeightUnit>
								<NetWeight>0</NetWeight>
								<NetWeightUnit>
									<Code>KG</Code>
									<Description>Kilogram</Description>
								</NetWeightUnit>
								<PackedQuantity>0</PackedQuantity>
							</PackedItem>
						</PackedItemCollection>
					</PackingLine>
				</PackingLineCollection>
";
				var createdMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entry.PK));
				var interchange = createdMessage.Interchange;
				AssertTextContainsDispiteBlanks(expectedPackingData, interchange.EI_BodyText);
			}
		}
	}
}
