using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaManifestHeaderDataEventParentFinderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestAcknowledged_Cancellation_WI00210360()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <Workflow>
            <ActionPurpose Description=""SG ACCESS ACKNOWLEDGEMENT"">ACK</ActionPurpose>
          </Workflow>
          <DataSource>
            <DataProvider>SGA</DataProvider>
          </DataSource>
          <DataTargetCollection>
            <DataTarget>
              <Key>{0}</Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-08-23T16:29:48</EventTime>
        <EventType>MDL</EventType>
        <EventReference>MST=MGE</EventReference>
        <IsEstimate>false</IsEstimate>
        <ContextCollection>
          <Context>
            <Type>ManifestCountry</Type>
            <Value>SG</Value>
          </Context>
          <Context>
            <Type>MessageType</Type>
            <Value>AIRAEU</Value>
          </Context>
          <Context>
            <Type>UENNumber</Type>
            <Value>198801949D</Value>
          </Context>
          <Context>
            <Type>OriginalCreateDate</Type>
            <Value>20180823</Value>
          </Context>
          <Context>
            <Type>OriginalSerialNumber</Type>
            <Value>7127</Value>
          </Context>
          <Context>
            <Type>MasterBill</Type>
            <Value>36467467676</Value>
            <SubContextCollection>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>FDASFD</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>1</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>JHKHJKJLLL</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>2</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>3</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>JFJG</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>4</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>5</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>6</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>7</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
            </SubContextCollection>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Header
				var header =
					(AsycudaManifestHeader)Factory.BOFactory
						.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGE";
				header.AMA_MasterBill = "36467467676";

				var bill1 = header.Bills.AddNew();
				bill1.ABL_BillNumber = "JHKHJKJLLL";

				var bill1Pack1 = bill1.Packs.AddNew();
				bill1Pack1.ConsignmentReference = 2;
				var bill1Pack1PackedItem = bill1Pack1.PackedItemForTesting();
				bill1Pack1PackedItem.API_PackStatus = "CAN";

				var bill1Pack2 = bill1.Packs.AddNew();
				bill1Pack2.ConsignmentReference = 3;
				var bill1Pack2PackedItem = bill1Pack2.PackedItemForTesting();
				bill1Pack2PackedItem.API_PackStatus = "CAN";

				var bill2 = header.Bills.AddNew();
				bill2.ABL_BillNumber = "FDASFD";

				var bill2Pack1 = bill2.Packs.AddNew();
				bill2Pack1.ConsignmentReference = 1;
				var bill2Pack1PackedItem = bill2Pack1.PackedItemForTesting();
				bill2Pack1PackedItem.API_PackStatus = "CAN";

				var bill3 = header.Bills.AddNew();
				bill3.ABL_BillNumber = "JFJG";

				var bill3Pack1 = bill3.Packs.AddNew();
				bill3Pack1.ConsignmentReference = 4;
				var bill3Pack1PackedItem = bill3Pack1.PackedItemForTesting();
				bill3Pack1PackedItem.API_PackStatus = "CAN";
				var bill3Pack2 = bill3.Packs.AddNew();
				bill3Pack2.ConsignmentReference = 5;
				var bill3Pack2PackedItem = bill3Pack2.PackedItemForTesting();
				bill3Pack2PackedItem.API_PackStatus = "CAN";
				var bill3Pack3 = bill3.Packs.AddNew();
				bill3Pack3.ConsignmentReference = 6;
				var bill3Pack3PackedItem = bill3Pack3.PackedItemForTesting();
				bill3Pack3PackedItem.API_PackStatus = "CAN";

				Factory.SaveForTesting();

				var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference));
				subscriber.GetLogParentsForEvent(xmlEvent);

				CombineAssertions(() =>
				{
					AssertEquals("header.RegistrationStatus", "CN", header.RegistrationStatus);
					AssertEquals("bill1Bill.ABL_MessageStatus", "ACP", bill1.ABL_MessageStatus);
					AssertEquals("bill1Bill.ABL_BillStatus", "CN", bill1.ABL_BillStatus);
					AssertEquals("bill1Pack1PackedItem.API_MessageStatus", "ACP", bill1Pack1PackedItem.API_MessageStatus);
					AssertEquals("bill1Pack1PackedItem.API_PackStatus", "CAN", bill1Pack1PackedItem.API_PackStatus);
					AssertEquals("bill1Pack2PackedItem.API_MessageStatus", "ACP", bill1Pack2PackedItem.API_MessageStatus);
					AssertEquals("bill1Pack2PackedItem.API_PackStatus", "CAN", bill1Pack2PackedItem.API_PackStatus);

					AssertEquals("bill2Bill.ABL_MessageStatus", "ACP", bill2.ABL_MessageStatus);
					AssertEquals("bill2Bill.ABL_BillStatus", "CN", bill2.ABL_BillStatus);
					AssertEquals("bill2Pack1PackedItem.API_MessageStatus", "ACP", bill2Pack1PackedItem.API_MessageStatus);
					AssertEquals("bill2Pack1PackedItem.API_PackStatus", "CAN", bill2Pack1PackedItem.API_PackStatus);

					AssertEquals("bill3Bill.ABL_MessageStatus", "ACP", bill3.ABL_MessageStatus);
					AssertEquals("bill3Bill.ABL_BillStatus", "CN", bill3.ABL_BillStatus);
					AssertEquals("bill3Pack1PackedItem.API_MessageStatus", "ACP", bill3Pack1PackedItem.API_MessageStatus);
					AssertEquals("bill3Pack1PackedItem.API_PackStatus", "CAN", bill3Pack1PackedItem.API_PackStatus);
					AssertEquals("bill3Pack2PackedItem.API_MessageStatus", "ACP", bill3Pack2PackedItem.API_MessageStatus);
					AssertEquals("bill3Pack2PackedItem.API_PackStatus", "CAN", bill3Pack2PackedItem.API_PackStatus);
					AssertEquals("bill3Pack3PackedItem.API_MessageStatus", "ACP", bill3Pack3PackedItem.API_MessageStatus);
					AssertEquals("bill3Pack3PackedItem.API_PackStatus", "CAN", bill3Pack3PackedItem.API_PackStatus);
				});
			}
		}

		public void TestImportSGAEvent()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "9", "9 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			Factory.SaveForTesting();

			const string eventXmlText = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataProvider>SGA</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Key>{0}</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>MRR</EventType>
    <EventReference>MST=MGE</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101330</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101330</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>634</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ErrorCode</Type>
                    <Value>E3</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>REJ</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>8</Value>
              </SubContext>
              <SubContext>
                <Type>ConsignmentStatus</Type>
                <Value>CLR</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>MessageStatusCode</Type>
            <Value>9</Value>
          </SubContext>
          <SubContext>
            <Type>ConsignmentStatus</Type>
            <Value>FAL</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101350</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101350</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>323</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>9</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>A1</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>ErrorCode</Type>
                <Value>E3</Value>
              </SubContext>
              <SubContext>
                <Type>ConsignmentStatus</Type>
                <Value>K5</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>ErrorCode</Type>
            <Value>D43</Value>
          </SubContext>
          <SubContext>
            <Type>ConsignmentStatus</Type>
            <Value>K2</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var logger = new TestErrorLogger();
			var newFactory = new BusinessObjectFactory();
			var subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, "DJ32342"));
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			newFactory.Save();
			AssertNull(logParents);
			AssertEquals("", logger.Logs);

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var headerFJ = AsycudaManifestHeaderHelper.CreateNew(Factory.BOFactory, Core.Constants.CountryCodes.Fiji, "ASY");
				headerFJ.AMA_RN_NKCountry = Core.Constants.CountryCodes.Fiji;
				Factory.SaveForTesting();
				logger.ClearLogs();
				newFactory = new BusinessObjectFactory();
				subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, headerFJ.AMA_JobReference));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				newFactory.Save();
				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(headerFJ), GetHumanReadableID(logParents[0]));
				AssertEquals("Warning - No Master Bill matched Global Manifest Job 'MAN0000001' Master Bill ''.", logger.Logs);
				headerFJ.Reload();
				AssertEquals("headerFJ.AMA_MessageStatus", ZString.Empty, headerFJ.AMA_MessageStatus);

				headerFJ.AMA_MasterBill = "MB1708101350";
				var billFJ = headerFJ.Bills.AddNew();
				billFJ.ABL_BillNumber = "HB1708101330";

				var packFJ = billFJ.Packs.AddNew();
				packFJ.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
				packFJ.ConsignmentReference = 323;
				var packedItemFJ = packFJ.PackedItemForTesting();
				Factory.SaveForTesting();
				logger.ClearLogs();
				newFactory = new BusinessObjectFactory();
				subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				newFactory.Save();
				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(headerFJ), GetHumanReadableID(logParents[0]));
				AssertEquals("Warning - Global Manifest Job 'MAN0000001' does not have matching Manifest Country 'SG'.", logger.Logs);
				headerFJ.Reload();
				AssertEquals("headerFJ.AMA_MessageStatus", ZString.Empty, headerFJ.AMA_MessageStatus);
				AssertEquals("headerFJ.RegistrationStatus", ZString.Empty, headerFJ.RegistrationStatus);
				billFJ.Reload();
				AssertEquals("bill.ABL_BillStatus", ZString.Empty, billFJ.ABL_BillStatus);
				AssertEquals("bill.ABL_MessageStatus", ZString.Empty, billFJ.ABL_MessageStatus);
				packedItemFJ.Reload();
				AssertEquals("packedItem.API_MessageStatus", ZString.Empty, packedItemFJ.API_MessageStatus);

				var headerSG = AsycudaManifestHeaderHelper.CreateNew(Factory.BOFactory, Core.Constants.CountryCodes.Singapore, "MGI");
				headerSG.AMA_MasterBill = "MB1708101350";
				var billSG = headerSG.Bills.AddNew();
				billSG.ABL_BillNumber = "HB1708101330";
				var packSG = billSG.Packs.AddNew();
				packSG.ConsignmentReference = 323;
				var packedItemSG = packSG.PackedItemForTesting();
				Factory.SaveForTesting();
				logger.ClearLogs();
				newFactory = new BusinessObjectFactory();
				subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, headerSG.AMA_JobReference));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				newFactory.Save();
				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(new BusinessObjectFactory().Load<AsycudaManifestHeader>(headerSG.PK)), GetHumanReadableID(logParents[0]));
				AssertEquals("Warning - Global Manifest Job 'MAN0000002' does not have matching Bill 'HB1708101350'.", logger.Logs);
				billSG.Reload();
				AssertEquals("bill.ABL_BillStatus", ZString.Empty, billSG.ABL_BillStatus);
				AssertEquals("bill.ABL_MessageStatus", ZString.Empty, billSG.ABL_MessageStatus);
				packedItemSG.Reload();
				AssertEquals("packedItem.API_MessageStatus", ZString.Empty, packedItemSG.API_MessageStatus);

				headerSG.AMA_MasterBill = "MB1708101330";
				packSG = billSG.Packs.AddNew();
				packSG.ConsignmentReference = 323;
				packedItemSG = packSG.PackedItemForTesting();
				Factory.SaveForTesting();
				billSG.Packs.RemoveAndDelete(packSG);
				packSG = billSG.Packs.AddNew();
				packSG.ConsignmentReference = 323;
				packedItemSG = packSG.PackedItemForTesting();
				Factory.SaveForTesting();
				logger.ClearLogs();
				newFactory = new BusinessObjectFactory();
				subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				newFactory.Save();
				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(new BusinessObjectFactory().Load<AsycudaManifestHeader>(headerSG.PK)), GetHumanReadableID(logParents[0]));
				AssertEquals("Warning - Bill 'HB1708101330' on Global Manifest Job 'MAN0000002' does not have matching Pack Consignment Reference '634'.", logger.Logs);
				billSG.Reload();
				AssertEquals("bill.ABL_BillStatus", "CLR", billSG.ABL_BillStatus);
				AssertEquals("bill.ABL_MessageStatus", ZString.Empty, billSG.ABL_MessageStatus);
				packedItemSG.Reload();
				AssertEquals("packedItem.API_MessageStatus", ZString.Empty, packedItemSG.API_MessageStatus);

				packSG.ConsignmentReference = 634;
				Factory.SaveForTesting();
				logger.ClearLogs();
				newFactory = new BusinessObjectFactory();
				subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				newFactory.Save();
				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(new BusinessObjectFactory().Load<AsycudaManifestHeader>(headerSG.PK)), GetHumanReadableID(logParents[0]));
				AssertEquals("", logger.Logs);
				billSG.Reload();
				AssertEquals("bill.ABL_BillStatus", "CLR", billSG.ABL_BillStatus);
				AssertEquals("bill.ABL_MessageStatus", MessageStatusCodeList.Codes.Error, billSG.ABL_MessageStatus);
				packedItemSG.Reload();
				AssertEquals("packedItem.API_MessageStatus", MessageStatusCodeList.Codes.Error, packedItemSG.API_MessageStatus);
			}
		}

		public void TestImportSGAEvent_UpdateBill()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "9", "9 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			Factory.SaveForTesting();

			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Event>
      <DataContext>
		<Workflow>
		<ActionPurpose Description=""SG ACCESS ACKNOWLEDGEMENT"">ACK</ActionPurpose>
		</Workflow>
		<DataSource>
		<DataProvider>SGA</DataProvider>
		</DataSource>
      <DataTargetCollection>
        <DataTarget>
          <Key>{0}</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>MRR</EventType>
    <EventReference>MST=MGE</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101330</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101330</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>634</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ErrorCode</Type>
                    <Value>E3</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>REJ</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>8</Value>
              </SubContext>
              <SubContext>
                <Type>ConsignmentStatus</Type>
                <Value>CLR</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>MessageStatusCode</Type>
            <Value>9</Value>
          </SubContext>
          <SubContext>
            <Type>ConsignmentStatus</Type>
            <Value>FAL</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101350</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101350</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>323</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>9</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>A1</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>ErrorCode</Type>
                <Value>E3</Value>
              </SubContext>
              <SubContext>
                <Type>ConsignmentStatus</Type>
                <Value>K5</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>ErrorCode</Type>
            <Value>D43</Value>
          </SubContext>
          <SubContext>
            <Type>ConsignmentStatus</Type>
            <Value>K2</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var logger = new TestErrorLogger();
			var newFactory = new BusinessObjectFactory();
			var subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, "MAN0000002"));
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			newFactory.Save();
			AssertNull(logParents);
			AssertEquals("", logger.Logs);

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var headerSG = AsycudaManifestHeaderHelper.CreateNew(Factory.BOFactory, Core.Constants.CountryCodes.Singapore, "MGI");
				headerSG.AMA_MasterBill = "MB1708101330";
				headerSG.AMA_JobReference = "MAN0000002";
				var billSG = headerSG.Bills.AddNew();
				billSG.ABL_BillNumber = "HB1708101330";
				var packSG = billSG.Packs.AddNew();
				packSG.ConsignmentReference = 634;
				var packedItemSG = packSG.PackedItemForTesting();
				Factory.SaveForTesting();
				logger.ClearLogs();

				newFactory = new BusinessObjectFactory();
				subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				newFactory.Save();
				billSG.Reload();
				packedItemSG.Reload();
				AssertEquals("packedItem.API_MessageStatus", MessageStatusCodeList.Codes.Error, packedItemSG.API_MessageStatus);

				headerSG.AMA_MasterBill = "MB1708101350";
				billSG.ABL_BillNumber = "HB1708101350";
				packSG.ConsignmentReference = 323;
				Factory.SaveForTesting();
				logger.ClearLogs();
				newFactory = new BusinessObjectFactory();
				subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				newFactory.Save();
				billSG.Reload();
				packedItemSG.Reload();
				AssertEquals("packedItem.API_MessageStatus", MessageStatusCodeList.Codes.Error, packedItemSG.API_MessageStatus);
			}
		}

		public void TestImportSGAEvent_MRJ()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "9", "9 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			Factory.SaveForTesting();

			const string eventXmlText = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataProvider>SGA</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Key>{0}</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>MRJ</EventType>
    <EventReference>MST=MGE</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101330</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101330</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>634</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ErrorCode</Type>
                    <Value>E3</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>REJ</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>8</Value>
              </SubContext>
              <SubContext>
                <Type>ConsignmentStatus</Type>
                <Value>CLR</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>MessageStatusCode</Type>
            <Value>9</Value>
          </SubContext>
          <SubContext>
            <Type>ConsignmentStatus</Type>
            <Value>FAL</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var logger = new TestErrorLogger();
			var newFactory = new BusinessObjectFactory();
			var subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, "DJ32342"));
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
			AssertEquals("", logger.Logs);

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var headerFJ = AsycudaManifestHeaderHelper.CreateNew(Factory.BOFactory, Core.Constants.CountryCodes.Fiji, "ASY");
				headerFJ.AMA_RN_NKCountry = Core.Constants.CountryCodes.Fiji;
				Factory.SaveForTesting();
				logger.ClearLogs();
				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, headerFJ.AMA_JobReference));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				newFactory.Save();
				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(headerFJ), GetHumanReadableID(logParents[0]));
				AssertEquals("Warning - No Master Bill matched Global Manifest Job 'MAN0000001' Master Bill ''.", logger.Logs);
				headerFJ.Reload();
				AssertEquals("headerFJ.AMA_MessageStatus", ZString.Empty, headerFJ.AMA_MessageStatus);

				var headerSG = AsycudaManifestHeaderHelper.CreateNew(Factory.BOFactory, Core.Constants.CountryCodes.Singapore, "MGI");
				headerSG.AMA_MasterBill = "MB1708101330";
				var billSG = headerSG.Bills.AddNew();
				billSG.ABL_BillNumber = "HB1708101330";
				var packSG = billSG.Packs.AddNew();
				packSG.ConsignmentReference = 323;
				var packedItemSG = packSG.PackedItemForTesting();
				Factory.SaveForTesting();
				logger.ClearLogs();
				newFactory = new BusinessObjectFactory();
				subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, headerSG.AMA_JobReference));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				newFactory.Save();
				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(new BusinessObjectFactory().Load<AsycudaManifestHeader>(headerSG.PK)), GetHumanReadableID(logParents[0]));
				AssertEquals("Warning - Bill 'HB1708101330' on Global Manifest Job 'MAN0000002' does not have matching Pack Consignment Reference '634'.", logger.Logs);
				billSG.Reload();
				AssertEquals("bill.ABL_BillStatus", "CLR", billSG.ABL_BillStatus);
				AssertEquals("bill.ABL_MessageStatus", ZString.Empty, billSG.ABL_MessageStatus);
				packedItemSG.Reload();
				AssertEquals("packedItem.API_MessageStatus", ZString.Empty, packedItemSG.API_MessageStatus);

				packSG.ConsignmentReference = 634;
				Factory.SaveForTesting();
				logger.ClearLogs();
				newFactory = new BusinessObjectFactory();
				subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				newFactory.Save();
				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(new BusinessObjectFactory().Load<AsycudaManifestHeader>(headerSG.PK)), GetHumanReadableID(logParents[0]));
				AssertEquals("", logger.Logs);
				billSG.Reload();
				AssertEquals("bill.ABL_BillStatus", "CLR", billSG.ABL_BillStatus);
				AssertEquals("bill.ABL_MessageStatus", MessageStatusCodeList.Codes.Error, billSG.ABL_MessageStatus);
				packedItemSG.Reload();
				AssertEquals("packedItem.API_MessageStatus", MessageStatusCodeList.Codes.Error, packedItemSG.API_MessageStatus);
			}
		}

		public void TestImportSGA_MessageAndCustomsStatus()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "9", "9 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			Factory.SaveForTesting();

			const string eventXmlText = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataProvider>SGA</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Key>{0}</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>MRJ</EventType>
    <EventReference>MST=MGE</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101330</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101330</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>634</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ErrorCode</Type>
                    <Value>E3</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>CR</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>635</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>9</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>8</Value>
              </SubContext>
              <SubContext>
                <Type>ConsignmentStatus</Type>
                <Value>IP</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>MessageStatusCode</Type>
            <Value>8</Value>
          </SubContext>
          <SubContext>
            <Type>ConsignmentStatus</Type>
            <Value>FAL</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();
			AssertEquals("", logger.Logs);

			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_MasterBill = "MB1708101330";
			header.AMA_ManifestType = "MGI";
			Factory.SaveForTesting();

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "HB1708101330";
			var pack = bill.Packs.AddNew();
			pack.ConsignmentReference = 323;
			var packedItem634 = pack.PackedItemForTesting();
			pack.ConsignmentReference = 634;
			var pack2 = bill.Packs.AddNew();
			pack2.ConsignmentReference = 323;
			var packedItem635 = pack2.PackedItemForTesting();
			pack2.ConsignmentReference = 635;
			Factory.SaveForTesting();

			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference));
			subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals("Message Status should be ERR", "ERR", header.AMA_MessageStatus);
			AssertEquals("Customs Status should be ERR", "", header.RegistrationStatus);
		}

		public void TestImportSGAEvent_MRJ_Cancellation()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "9", "9 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			Factory.SaveForTesting();

			const string eventXmlText = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataProvider>SGA</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Key>{0}</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>MRJ</EventType>
    <EventReference>MST=MGE</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101330</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101330</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>634</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ErrorCode</Type>
                    <Value>E3</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>REJ</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>635</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>9</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>8</Value>
              </SubContext>
              <SubContext>
                <Type>ConsignmentStatus</Type>
                <Value>CLR</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>MessageStatusCode</Type>
            <Value>9</Value>
          </SubContext>
          <SubContext>
            <Type>ConsignmentStatus</Type>
            <Value>FAL</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var logger = new TestErrorLogger();
			var newFactory = new BusinessObjectFactory();
			var subscriber = new AsycudaManifestHeaderDataEventParentFinder(newFactory, new AsycudaManifestHeaderDataContextManager(), logger);
			var eventDeserializer = new XmlEventDeserializer();
			AssertEquals("", logger.Logs);

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGI";
				Factory.SaveForTesting();
				var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference));

				header.AMA_MasterBill = "MB1708101330";
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "HB1708101330";
				var pack = bill.Packs.AddNew();
				pack.ConsignmentReference = 323;
				var packedItem634 = pack.PackedItemForTesting();
				packedItem634.API_PackStatus = "CAN";
				pack.ConsignmentReference = 634;
				var pack2 = bill.Packs.AddNew();
				pack2.ConsignmentReference = 323;
				var packedItem635 = pack2.PackedItemForTesting();
				packedItem635.API_PackStatus = "CAN";
				pack2.ConsignmentReference = 635;
				Factory.SaveForTesting();
				AssertEquals("packedItem.API_PackStatus", "CAN", packedItem634.API_PackStatus);
				AssertEquals("packedItem.API_PackStatus", "CAN", packedItem635.API_PackStatus);

				var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				newFactory.Save();
				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK)), GetHumanReadableID(logParents[0]));
				AssertEquals("", logger.Logs);
				header.Reload();
				AssertEquals("header.AMA_MessageStatus", MessageStatusCodeList.Codes.Error, header.AMA_MessageStatus);
				bill.Reload();
				AssertEquals("bill.ABL_BillStatus", "CLR", bill.ABL_BillStatus);
				AssertEquals("bill.ABL_MessageStatus", MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);
				CombineAssertions(() =>
				{
					packedItem634.Reload();
					AssertEquals("packedItem634.API_PackStatus", "REJ", packedItem634.API_PackStatus);
					AssertEquals("packedItem634.API_MessageStatus", MessageStatusCodeList.Codes.Error, packedItem634.API_MessageStatus);
					packedItem635.Reload();
					AssertEquals("packedItem635.API_PackStatus", "", packedItem635.API_PackStatus);
					AssertEquals("packedItem635.API_MessageStatus", MessageStatusCodeList.Codes.Error, packedItem635.API_MessageStatus);
				});
			}
		}

		public void TestImportManifestPermitNumber()
		{
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			header.AMA_MasterBill = "MB1708101330";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "HB1708101330";

			var pack = bill.Packs.AddNew();
			pack.ConsignmentReference = 123;
			var packedItem = pack.PackedItemForTesting();

			Factory.SaveForTesting();

			const string eventXmlText = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataProvider>SGA</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Key>{0}</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>MRR</EventType>
    <EventReference>MST=MGI</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101330</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101330</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>123</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ManifestPermitNumber</Type>
                    <Value>PERT43533</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>ManifestPermitNumber</Type>
                <Value>PERT58345</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>ManifestPermitNumber</Type>
            <Value>PERT9325</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference));
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertCollectionContains(header, logParents);
			Factory.SaveForTesting();
			AssertManifestPermitNumber(header, "PERT9325");
			AssertManifestPermitNumber(bill, "PERT58345");
			AssertManifestPermitNumber(packedItem, "PERT43533");
		}

		public void TestImportManifestPermitNumberMatchingManifestNumberWithHyphen()
		{
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = "MGI";
			header.AMA_MasterBill = "081-08101330";

			Factory.SaveForTesting();

			const string eventXmlText = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataProvider>SGA</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Key>{0}</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>MRR</EventType>
    <EventReference>MST=MGI</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>{1}</Value>
        <SubContextCollection>
          <SubContext>
            <Type>ManifestPermitNumber</Type>
            <Value>{2}</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "08108101330", "PERT9325"));
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertCollectionContains(header, logParents);
			Factory.SaveForTesting();
			AssertManifestPermitNumber(header, "PERT9325");

			header.RegistrationNumber = ZString.Empty;
			header.AMA_MasterBill = "08108101330";
			Factory.SaveForTesting();
			xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "081-08101330", "PERT9545"));
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertCollectionContains(header, logParents);
			Factory.SaveForTesting();
			AssertManifestPermitNumber(header, "PERT9545");

			header.RegistrationNumber = ZString.Empty;
			header.AMA_MasterBill = "0-8108101330";
			Factory.SaveForTesting();
			xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "08108-101330", "PERT96923"));
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertCollectionContains(header, logParents);
			Factory.SaveForTesting();
			AssertManifestPermitNumber(header, "PERT96923");

			header.RegistrationNumber = ZString.Empty;
			header.AMA_MasterBill = "08108101330";
			Factory.SaveForTesting();
			xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "08108101330", "PERT8732"));
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertCollectionContains(header, logParents);
			Factory.SaveForTesting();
			AssertManifestPermitNumber(header, "PERT8732");
		}

		public void TestMessageAcceptedEventsAreCancelled()
		{
			const string AcceptMessageCode = "MSG1";
			const string ErrorMessageCode = "MSG2";
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, AcceptMessageCode, "ACCEPT DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, ErrorMessageCode, "ERROR DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGI";
				var headerLog1 = header.Logs.AddNew(Events.MessageAccepted, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGI"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "HERE")
				});
				var headerLog2 = header.Logs.AddNew(Events.MessageAccepted, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGE"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "MGE")
				});
				var headerLog3 = header.Logs.AddNew(Events.MessageAccepted, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, "HERE"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGI")
				});
				var headerLog4 = header.Logs.AddNew(Events.MessagePendingProcessing, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, "HERE"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGI")
				});
				header.AMA_MasterBill = "MB1708101330";
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "HB1708101330";
				var billLog1 = bill.Logs.AddNew(Events.MessageAccepted, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGI"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "HERE")
				});
				var billLog2 = bill.Logs.AddNew(Events.MessageAccepted, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGE"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "MGE")
				});
				var billLog3 = bill.Logs.AddNew(Events.MessageAccepted, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, "HERE"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGI")
				});
				var billLog4 = bill.Logs.AddNew(Events.MessagePendingProcessing, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, "HERE"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGI")
				});

				var pack = bill.Packs.AddNew();
				pack.ConsignmentReference = 123;
				var packedItem = pack.PackedItemForTesting();
				var packLog1 = packedItem.Logs.AddNew(Events.MessageAccepted, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGI"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "HERE")
				});
				var packLog2 = packedItem.Logs.AddNew(Events.MessageAccepted, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGE"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "MGE")
				});
				var packLog3 = packedItem.Logs.AddNew(Events.MessageAccepted, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, "HERE"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGI")
				});
				var packLog4 = packedItem.Logs.AddNew(Events.MessagePendingProcessing, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, "HERE"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "MGI")
				});

				Factory.SaveForTesting();

				const string eventXmlText = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataProvider>SGA</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Key>{0}</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>MRR</EventType>
    <EventReference>{1}</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101330</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101330</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>123</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>{2}</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>{3}</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>MessageStatusCode</Type>
            <Value>{4}</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
				var logger = new TestErrorLogger();
				var subscriber = GetNewEventParentFinder(logger);
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "MST=MGI", AcceptMessageCode, AcceptMessageCode, AcceptMessageCode));
				var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertCollectionContains(header, logParents);
				Factory.SaveForTesting();
				AssertEquals("headerLog1.SL_IsCancelled", false, headerLog1.SL_IsCancelled);
				AssertEquals("headerLog2.SL_IsCancelled", false, headerLog2.SL_IsCancelled);
				AssertEquals("headerLog3.SL_IsCancelled", false, headerLog3.SL_IsCancelled);
				AssertEquals("headerLog4.SL_IsCancelled", false, headerLog4.SL_IsCancelled);
				AssertEquals("billLog1.SL_IsCancelled", false, billLog1.SL_IsCancelled);
				AssertEquals("billLog2.SL_IsCancelled", false, billLog2.SL_IsCancelled);
				AssertEquals("billLog3.SL_IsCancelled", false, billLog3.SL_IsCancelled);
				AssertEquals("billLog4.SL_IsCancelled", false, billLog4.SL_IsCancelled);
				AssertEquals("packLog1.SL_IsCancelled", false, packLog1.SL_IsCancelled);
				AssertEquals("packLog2.SL_IsCancelled", false, packLog2.SL_IsCancelled);
				AssertEquals("packLog3.SL_IsCancelled", false, packLog3.SL_IsCancelled);
				AssertEquals("packLog4.SL_IsCancelled", false, packLog4.SL_IsCancelled);

				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "MST=MGI", ErrorMessageCode, AcceptMessageCode, AcceptMessageCode));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertCollectionContains(header, logParents);
				Factory.SaveForTesting();
				AssertEquals("headerLog1.SL_IsCancelled", true, headerLog1.SL_IsCancelled);
				AssertEquals("headerLog2.SL_IsCancelled", false, headerLog2.SL_IsCancelled);
				AssertEquals("headerLog3.SL_IsCancelled", true, headerLog3.SL_IsCancelled);
				AssertEquals("headerLog4.SL_IsCancelled", false, headerLog4.SL_IsCancelled);
				AssertEquals("billLog1.SL_IsCancelled", true, billLog1.SL_IsCancelled);
				AssertEquals("billLog2.SL_IsCancelled", false, billLog2.SL_IsCancelled);
				AssertEquals("billLog3.SL_IsCancelled", true, billLog3.SL_IsCancelled);
				AssertEquals("billLog4.SL_IsCancelled", false, billLog4.SL_IsCancelled);
				AssertEquals("packLog1.SL_IsCancelled", true, packLog1.SL_IsCancelled);
				AssertEquals("packLog2.SL_IsCancelled", false, packLog2.SL_IsCancelled);
				AssertEquals("packLog3.SL_IsCancelled", true, packLog3.SL_IsCancelled);
				AssertEquals("packLog4.SL_IsCancelled", false, packLog4.SL_IsCancelled);

				packLog1.Reactivate();
				packLog3.Reactivate();
				Factory.SaveForTesting();
				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "MST=MGI", AcceptMessageCode, ErrorMessageCode, AcceptMessageCode));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertCollectionContains(header, logParents);
				Factory.SaveForTesting();
				AssertEquals("headerLog1.SL_IsCancelled", true, headerLog1.SL_IsCancelled);
				AssertEquals("headerLog2.SL_IsCancelled", false, headerLog2.SL_IsCancelled);
				AssertEquals("headerLog3.SL_IsCancelled", true, headerLog3.SL_IsCancelled);
				AssertEquals("headerLog4.SL_IsCancelled", false, headerLog4.SL_IsCancelled);
				AssertEquals("billLog1.SL_IsCancelled", true, billLog1.SL_IsCancelled);
				AssertEquals("billLog2.SL_IsCancelled", false, billLog2.SL_IsCancelled);
				AssertEquals("billLog3.SL_IsCancelled", true, billLog3.SL_IsCancelled);
				AssertEquals("billLog4.SL_IsCancelled", false, billLog4.SL_IsCancelled);
				AssertEquals("packLog1.SL_IsCancelled", false, packLog1.SL_IsCancelled);
				AssertEquals("packLog2.SL_IsCancelled", false, packLog2.SL_IsCancelled);
				AssertEquals("packLog3.SL_IsCancelled", false, packLog3.SL_IsCancelled);
				AssertEquals("packLog4.SL_IsCancelled", false, packLog4.SL_IsCancelled);

				billLog1.Reactivate();
				billLog3.Reactivate();
				Factory.SaveForTesting();
				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "MST=MGI", AcceptMessageCode, AcceptMessageCode, ErrorMessageCode));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertCollectionContains(header, logParents);
				Factory.SaveForTesting();
				AssertEquals("headerLog1.SL_IsCancelled", true, headerLog1.SL_IsCancelled);
				AssertEquals("headerLog2.SL_IsCancelled", false, headerLog2.SL_IsCancelled);
				AssertEquals("headerLog3.SL_IsCancelled", true, headerLog3.SL_IsCancelled);
				AssertEquals("headerLog4.SL_IsCancelled", false, headerLog4.SL_IsCancelled);
				AssertEquals("billLog1.SL_IsCancelled", false, billLog1.SL_IsCancelled);
				AssertEquals("billLog2.SL_IsCancelled", false, billLog2.SL_IsCancelled);
				AssertEquals("billLog3.SL_IsCancelled", false, billLog3.SL_IsCancelled);
				AssertEquals("billLog4.SL_IsCancelled", false, billLog4.SL_IsCancelled);
				AssertEquals("packLog1.SL_IsCancelled", false, packLog1.SL_IsCancelled);
				AssertEquals("packLog2.SL_IsCancelled", false, packLog2.SL_IsCancelled);
				AssertEquals("packLog3.SL_IsCancelled", false, packLog3.SL_IsCancelled);
				AssertEquals("packLog4.SL_IsCancelled", false, packLog4.SL_IsCancelled);

				headerLog1.Reactivate();
				headerLog3.Reactivate();
				Factory.SaveForTesting();
				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "MST=DLD", ErrorMessageCode, ErrorMessageCode, ErrorMessageCode));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertCollectionContains(header, logParents);
				Factory.SaveForTesting();
				AssertEquals("headerLog1.SL_IsCancelled", false, headerLog1.SL_IsCancelled);
				AssertEquals("headerLog2.SL_IsCancelled", false, headerLog2.SL_IsCancelled);
				AssertEquals("headerLog3.SL_IsCancelled", false, headerLog3.SL_IsCancelled);
				AssertEquals("headerLog4.SL_IsCancelled", false, headerLog4.SL_IsCancelled);
				AssertEquals("billLog1.SL_IsCancelled", false, billLog1.SL_IsCancelled);
				AssertEquals("billLog2.SL_IsCancelled", false, billLog2.SL_IsCancelled);
				AssertEquals("billLog3.SL_IsCancelled", false, billLog3.SL_IsCancelled);
				AssertEquals("billLog4.SL_IsCancelled", false, billLog4.SL_IsCancelled);
				AssertEquals("packLog1.SL_IsCancelled", false, packLog1.SL_IsCancelled);
				AssertEquals("packLog2.SL_IsCancelled", false, packLog2.SL_IsCancelled);
				AssertEquals("packLog3.SL_IsCancelled", false, packLog3.SL_IsCancelled);
				AssertEquals("packLog4.SL_IsCancelled", false, packLog4.SL_IsCancelled);

				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "MBS=KDS", ErrorMessageCode, ErrorMessageCode, ErrorMessageCode));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertCollectionContains(header, logParents);
				Factory.SaveForTesting();
				AssertEquals("headerLog1.SL_IsCancelled", false, headerLog1.SL_IsCancelled);
				AssertEquals("headerLog2.SL_IsCancelled", false, headerLog2.SL_IsCancelled);
				AssertEquals("headerLog3.SL_IsCancelled", false, headerLog3.SL_IsCancelled);
				AssertEquals("headerLog4.SL_IsCancelled", false, headerLog4.SL_IsCancelled);
				AssertEquals("billLog1.SL_IsCancelled", false, billLog1.SL_IsCancelled);
				AssertEquals("billLog2.SL_IsCancelled", false, billLog2.SL_IsCancelled);
				AssertEquals("billLog3.SL_IsCancelled", false, billLog3.SL_IsCancelled);
				AssertEquals("billLog4.SL_IsCancelled", false, billLog4.SL_IsCancelled);
				AssertEquals("packLog1.SL_IsCancelled", false, packLog1.SL_IsCancelled);
				AssertEquals("packLog2.SL_IsCancelled", false, packLog2.SL_IsCancelled);
				AssertEquals("packLog3.SL_IsCancelled", false, packLog3.SL_IsCancelled);
				AssertEquals("packLog4.SL_IsCancelled", false, packLog4.SL_IsCancelled);

				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "MST=MGI|MBS=KDS", ErrorMessageCode, ErrorMessageCode, ErrorMessageCode));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertCollectionContains(header, logParents);
				Factory.SaveForTesting();
				AssertEquals("headerLog1.SL_IsCancelled", false, headerLog1.SL_IsCancelled);
				AssertEquals("headerLog2.SL_IsCancelled", false, headerLog2.SL_IsCancelled);
				AssertEquals("headerLog3.SL_IsCancelled", false, headerLog3.SL_IsCancelled);
				AssertEquals("headerLog4.SL_IsCancelled", false, headerLog4.SL_IsCancelled);
				AssertEquals("billLog1.SL_IsCancelled", false, billLog1.SL_IsCancelled);
				AssertEquals("billLog2.SL_IsCancelled", false, billLog2.SL_IsCancelled);
				AssertEquals("billLog3.SL_IsCancelled", false, billLog3.SL_IsCancelled);
				AssertEquals("billLog4.SL_IsCancelled", false, billLog4.SL_IsCancelled);
				AssertEquals("packLog1.SL_IsCancelled", false, packLog1.SL_IsCancelled);
				AssertEquals("packLog2.SL_IsCancelled", false, packLog2.SL_IsCancelled);
				AssertEquals("packLog3.SL_IsCancelled", false, packLog3.SL_IsCancelled);
				AssertEquals("packLog4.SL_IsCancelled", false, packLog4.SL_IsCancelled);

				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "M=KDS", ErrorMessageCode, ErrorMessageCode, ErrorMessageCode));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertCollectionContains(header, logParents);
				Factory.SaveForTesting();
				AssertEquals("headerLog1.SL_IsCancelled", false, headerLog1.SL_IsCancelled);
				AssertEquals("headerLog2.SL_IsCancelled", false, headerLog2.SL_IsCancelled);
				AssertEquals("headerLog3.SL_IsCancelled", false, headerLog3.SL_IsCancelled);
				AssertEquals("headerLog4.SL_IsCancelled", false, headerLog4.SL_IsCancelled);
				AssertEquals("billLog1.SL_IsCancelled", false, billLog1.SL_IsCancelled);
				AssertEquals("billLog2.SL_IsCancelled", false, billLog2.SL_IsCancelled);
				AssertEquals("billLog3.SL_IsCancelled", false, billLog3.SL_IsCancelled);
				AssertEquals("billLog4.SL_IsCancelled", false, billLog4.SL_IsCancelled);
				AssertEquals("packLog1.SL_IsCancelled", false, packLog1.SL_IsCancelled);
				AssertEquals("packLog2.SL_IsCancelled", false, packLog2.SL_IsCancelled);
				AssertEquals("packLog3.SL_IsCancelled", false, packLog3.SL_IsCancelled);
				AssertEquals("packLog4.SL_IsCancelled", false, packLog4.SL_IsCancelled);

				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "=KDS", ErrorMessageCode, ErrorMessageCode, ErrorMessageCode));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertCollectionContains(header, logParents);
				Factory.SaveForTesting();
				AssertEquals("headerLog1.SL_IsCancelled", false, headerLog1.SL_IsCancelled);
				AssertEquals("headerLog2.SL_IsCancelled", false, headerLog2.SL_IsCancelled);
				AssertEquals("headerLog3.SL_IsCancelled", false, headerLog3.SL_IsCancelled);
				AssertEquals("headerLog4.SL_IsCancelled", false, headerLog4.SL_IsCancelled);
				AssertEquals("billLog1.SL_IsCancelled", false, billLog1.SL_IsCancelled);
				AssertEquals("billLog2.SL_IsCancelled", false, billLog2.SL_IsCancelled);
				AssertEquals("billLog3.SL_IsCancelled", false, billLog3.SL_IsCancelled);
				AssertEquals("billLog4.SL_IsCancelled", false, billLog4.SL_IsCancelled);
				AssertEquals("packLog1.SL_IsCancelled", false, packLog1.SL_IsCancelled);
				AssertEquals("packLog2.SL_IsCancelled", false, packLog2.SL_IsCancelled);
				AssertEquals("packLog3.SL_IsCancelled", false, packLog3.SL_IsCancelled);
				AssertEquals("packLog4.SL_IsCancelled", false, packLog4.SL_IsCancelled);

				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "|CMP=KDS|MST=MGI", ErrorMessageCode, ErrorMessageCode, ErrorMessageCode));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertCollectionContains(header, logParents);
				Factory.SaveForTesting();
				AssertEquals("headerLog1.SL_IsCancelled", false, headerLog1.SL_IsCancelled);
				AssertEquals("headerLog2.SL_IsCancelled", false, headerLog2.SL_IsCancelled);
				AssertEquals("headerLog3.SL_IsCancelled", false, headerLog3.SL_IsCancelled);
				AssertEquals("headerLog4.SL_IsCancelled", false, headerLog4.SL_IsCancelled);
				AssertEquals("billLog1.SL_IsCancelled", false, billLog1.SL_IsCancelled);
				AssertEquals("billLog2.SL_IsCancelled", false, billLog2.SL_IsCancelled);
				AssertEquals("billLog3.SL_IsCancelled", false, billLog3.SL_IsCancelled);
				AssertEquals("billLog4.SL_IsCancelled", false, billLog4.SL_IsCancelled);
				AssertEquals("packLog1.SL_IsCancelled", false, packLog1.SL_IsCancelled);
				AssertEquals("packLog2.SL_IsCancelled", false, packLog2.SL_IsCancelled);
				AssertEquals("packLog3.SL_IsCancelled", false, packLog3.SL_IsCancelled);
				AssertEquals("packLog4.SL_IsCancelled", false, packLog4.SL_IsCancelled);

				xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference, "|LOC=HERE|MST=MGI", ErrorMessageCode, ErrorMessageCode, ErrorMessageCode));
				logParents = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertCollectionContains(header, logParents);
				Factory.SaveForTesting();
				AssertEquals("headerLog1.SL_IsCancelled", true, headerLog1.SL_IsCancelled);
				AssertEquals("headerLog2.SL_IsCancelled", false, headerLog2.SL_IsCancelled);
				AssertEquals("headerLog3.SL_IsCancelled", false, headerLog3.SL_IsCancelled);
				AssertEquals("headerLog4.SL_IsCancelled", false, headerLog4.SL_IsCancelled);
				AssertEquals("billLog1.SL_IsCancelled", true, billLog1.SL_IsCancelled);
				AssertEquals("billLog2.SL_IsCancelled", false, billLog2.SL_IsCancelled);
				AssertEquals("billLog3.SL_IsCancelled", false, billLog3.SL_IsCancelled);
				AssertEquals("billLog4.SL_IsCancelled", false, billLog4.SL_IsCancelled);
				AssertEquals("packLog1.SL_IsCancelled", true, packLog1.SL_IsCancelled);
				AssertEquals("packLog2.SL_IsCancelled", false, packLog2.SL_IsCancelled);
				AssertEquals("packLog3.SL_IsCancelled", false, packLog3.SL_IsCancelled);
				AssertEquals("packLog4.SL_IsCancelled", false, packLog4.SL_IsCancelled);
			}
		}

		public void TestErrorMessageRejected()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ERR", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, "ManifestType", "MGI");
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, "false");

			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <Workflow>
            <ActionPurpose Description=""AIRERR SG ACCESS"">ERR</ActionPurpose>
          </Workflow>
          <DataSource>
            <DataProvider>SGA</DataProvider>
          </DataSource>
          <DataTargetCollection>
            <DataTarget>
              <Key>{0}</Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-04-12T21:26:15</EventTime>
        <EventType>MRR</EventType>
        <EventReference>MST=MGI</EventReference>
        <IsEstimate>false</IsEstimate>
        <ContextCollection>
          <Context>
            <Type>ManifestCountry</Type>
            <Value>SG</Value>
          </Context>
          <Context>
            <Type>OriginalInterchangeNumber</Type>
            <Value>334</Value>
          </Context>
          <Context>
            <Type>MessageType</Type>
            <Value>AIRPCM</Value>
          </Context>
          <Context>
            <Type>UENNumber</Type>
            <Value>198801949D</Value>
          </Context>
          <Context>
            <Type>OriginalCreateDate</Type>
            <Value>20180412</Value>
          </Context>
          <Context>
            <Type>OriginalSerialNumber</Type>
            <Value>2149</Value>
          </Context>
          <Context>
            <Type>MasterBill</Type>
            <Value>15625632655</Value>
            <SubContextCollection>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>RATK65BILLO</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>9</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>07292</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>U00</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorDescription</Type>
                        <Value>CYCLE TIME PASSED</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentTag</Type>
                        <Value>DTM</Value>
                      </SubContext>
                      <SubContext>
                        <Type>OrdinalNumber1</Type>
                        <Value>6</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>ERR</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>RATK65BILLN</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>8</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>07293</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>U00</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorDescription</Type>
                        <Value>CYCLE TIME PASSED</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentTag</Type>
                        <Value>DTM</Value>
                      </SubContext>
                      <SubContext>
                        <Type>OrdinalNumber1</Type>
                        <Value>6</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>ERR</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>RATK65BILLH</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>6</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>07294</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>U00</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorDescription</Type>
                        <Value>CYCLE TIME PASSED</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentTag</Type>
                        <Value>DTM</Value>
                      </SubContext>
                      <SubContext>
                        <Type>OrdinalNumber1</Type>
                        <Value>6</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>7</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>07295</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>U00</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorDescription</Type>
                        <Value>CYCLE TIME PASSED</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentTag</Type>
                        <Value>DTM</Value>
                      </SubContext>
                      <SubContext>
                        <Type>OrdinalNumber1</Type>
                        <Value>6</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>ERR</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>RATK65BILLE</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>5</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>07296</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>U00</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorDescription</Type>
                        <Value>CYCLE TIME PASSED</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentTag</Type>
                        <Value>DTM</Value>
                      </SubContext>
                      <SubContext>
                        <Type>OrdinalNumber1</Type>
                        <Value>6</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>ERR</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>RATK65BILLD</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>3</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>07297</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>U00</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorDescription</Type>
                        <Value>CYCLE TIME PASSED</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentTag</Type>
                        <Value>DTM</Value>
                      </SubContext>
                      <SubContext>
                        <Type>OrdinalNumber1</Type>
                        <Value>6</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>4</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>07298</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>U00</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorDescription</Type>
                        <Value>CYCLE TIME PASSED</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentTag</Type>
                        <Value>DTM</Value>
                      </SubContext>
                      <SubContext>
                        <Type>OrdinalNumber1</Type>
                        <Value>6</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>ERR</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>RATK65BILLC</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>2</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>07299</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>U00</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorDescription</Type>
                        <Value>CYCLE TIME PASSED</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentTag</Type>
                        <Value>DTM</Value>
                      </SubContext>
                      <SubContext>
                        <Type>OrdinalNumber1</Type>
                        <Value>6</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>ERR</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>RATK65BILLB</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>1</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>ConsignmentNumber</Type>
                        <Value>07300</Value>
                      </SubContext>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ERR</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorCode</Type>
                        <Value>U00</Value>
                      </SubContext>
                      <SubContext>
                        <Type>ErrorDescription</Type>
                        <Value>CYCLE TIME PASSED</Value>
                      </SubContext>
                      <SubContext>
                        <Type>SegmentTag</Type>
                        <Value>DTM</Value>
                      </SubContext>
                      <SubContext>
                        <Type>OrdinalNumber1</Type>
                        <Value>6</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>ERR</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>ERR</Value>
              </SubContext>
            </SubContextCollection>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();

			// Header
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			header.AMA_MasterBill = "15625632655";
			// Bill
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "RATK65BILLO";
			// Pack
			var pack = bill.Packs.AddNew();
			pack.ConsignmentReference = 9;
			var packedItem = pack.PackedItemForTesting();
			Factory.SaveForTesting();

			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference));
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals(1, logParents.Length);
			AssertEquals("Should get best matching Header.", GetHumanReadableID(header), GetHumanReadableID(logParents[0]));
			AssertEquals("header.AMA_MessageStatus", MessageStatusCodeList.Codes.Error, header.AMA_MessageStatus);
			AssertEquals("bill.ABL_MessageStatus", MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);
			AssertEquals("packedItem.API_MessageStatus", MessageStatusCodeList.Codes.Error, packedItem.API_MessageStatus);
		}

		public void TestAIRPCMMessageTypeAcknowledged()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <Workflow>
            <ActionPurpose Description=""SG ACCESS ACKNOWLEDGEMENT"">ACK</ActionPurpose>
          </Workflow>
          <DataSource>
            <DataProvider>SGA</DataProvider>
          </DataSource>
          <DataTargetCollection>
            <DataTarget>
              <Key>{0}</Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-02-27T19:48:38</EventTime>
        <EventType>MDL</EventType>
        <EventReference>MST=MGE</EventReference>
        <IsEstimate>false</IsEstimate>
        <ContextCollection>
          <Context>
            <Type>ManifestCountry</Type>
            <Value>SG</Value>
          </Context>
          <Context>
            <Type>MessageType</Type>
            <Value>AIRPCM</Value>
          </Context>
          <Context>
            <Type>UENNumber</Type>
            <Value>198801949D</Value>
          </Context>
          <Context>
            <Type>OriginalCreateDate</Type>
            <Value>20180227</Value>
          </Context>
          <Context>
            <Type>OriginalSerialNumber</Type>
            <Value>0074</Value>
          </Context>
          <Context>
            <Type>MasterBill</Type>
            <Value>4467847667</Value>
            <SubContextCollection>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>BILLHJKY23</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>1</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>ACK</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>ACK</Value>
              </SubContext>
            </SubContextCollection>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Header
				var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGI";
				header.AMA_MasterBill = "4467847667";
				// Bill
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "BILLHJKY23";
				// Pack
				var pack = bill.Packs.AddNew();
				pack.ConsignmentReference = 1;
				var packedItem = pack.PackedItemForTesting();
				Factory.SaveForTesting();

				var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference));
				var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(header), GetHumanReadableID(logParents[0]));
				AssertEquals("header.AMA_MessageStatus", MessageStatusCodeList.Codes.Awaiting, header.AMA_MessageStatus);
				AssertEquals("bill.ABC_MessageStatus", MessageStatusCodeList.Codes.Awaiting, bill.ABL_MessageStatus);
				AssertEquals("packedItem.API_MessageStatus", MessageStatusCodeList.Codes.Awaiting, packedItem.API_MessageStatus);
			}
		}

		public void TestAIRAEDMessageTypeAcknowledged()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <Workflow>
            <ActionPurpose Description=""SG ACCESS ACKNOWLEDGEMENT"">ACK</ActionPurpose>
          </Workflow>
          <DataSource>
            <DataProvider>SGA</DataProvider>
          </DataSource>
          <DataTargetCollection>
            <DataTarget>
              <Key>{0}</Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-02-27T19:48:38</EventTime>
        <EventType>MDL</EventType>
        <EventReference>MST=MGE</EventReference>
        <IsEstimate>false</IsEstimate>
        <ContextCollection>
          <Context>
            <Type>ManifestCountry</Type>
            <Value>SG</Value>
          </Context>
          <Context>
            <Type>MessageType</Type>
            <Value>AIRAED</Value>
          </Context>
          <Context>
            <Type>UENNumber</Type>
            <Value>198801949D</Value>
          </Context>
          <Context>
            <Type>OriginalCreateDate</Type>
            <Value>20180227</Value>
          </Context>
          <Context>
            <Type>OriginalSerialNumber</Type>
            <Value>0074</Value>
          </Context>
          <Context>
            <Type>MasterBill</Type>
            <Value>4467847667</Value>
            <SubContextCollection>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>BILLHJKY23</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>1</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
            </SubContextCollection>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Header
				var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGI";
				header.AMA_MasterBill = "4467847667";
				// Bill
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "BILLHJKY23";
				// Pack
				var pack = bill.Packs.AddNew();
				pack.ConsignmentReference = 1;
				var packedItem = pack.PackedItemForTesting();
				Factory.SaveForTesting();

				var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference));
				var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(header), GetHumanReadableID(logParents[0]));
				AssertEquals("header.AMA_MessageStatus", MessageStatusCodeList.Codes.Awaiting, header.AMA_MessageStatus);
				AssertEquals("bill.ABC_MessageStatus", MessageStatusCodeList.Codes.Awaiting, bill.ABL_MessageStatus);
				AssertEquals("packedItem.API_MessageStatus", MessageStatusCodeList.Codes.Awaiting, packedItem.API_MessageStatus);
			}
		}

		public void TestAIRPCUMessageTypeAcknowledged()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <Workflow>
            <ActionPurpose Description=""SG ACCESS ACKNOWLEDGEMENT"">ACK</ActionPurpose>
          </Workflow>
          <DataSource>
            <DataProvider>SGA</DataProvider>
          </DataSource>
          <DataTargetCollection>
            <DataTarget>
              <Key>{0}</Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-02-27T19:48:38</EventTime>
        <EventType>MDL</EventType>
        <EventReference>MST=MGE</EventReference>
        <IsEstimate>false</IsEstimate>
        <ContextCollection>
          <Context>
            <Type>ManifestCountry</Type>
            <Value>SG</Value>
          </Context>
          <Context>
            <Type>MessageType</Type>
            <Value>AIRPCU</Value>
          </Context>
          <Context>
            <Type>UENNumber</Type>
            <Value>198801949D</Value>
          </Context>
          <Context>
            <Type>OriginalCreateDate</Type>
            <Value>20180227</Value>
          </Context>
          <Context>
            <Type>OriginalSerialNumber</Type>
            <Value>0074</Value>
          </Context>
          <Context>
            <Type>MasterBill</Type>
            <Value>4467847667</Value>
            <SubContextCollection>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>BILLHJKY23</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>1</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
            </SubContextCollection>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Header
				var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGI";
				header.AMA_MasterBill = "4467847667";
				// Bill
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "BILLHJKY23";
				// Pack
				var pack = bill.Packs.AddNew();
				pack.ConsignmentReference = 1;
				var packedItem = pack.PackedItemForTesting();
				Factory.SaveForTesting();

				var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference));
				var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(header), GetHumanReadableID(logParents[0]));
				AssertEquals("header.AMA_MessageStatus", MessageStatusCodeList.Codes.Accepted, header.AMA_MessageStatus);
				AssertEquals("bill.ABC_MessageStatus", MessageStatusCodeList.Codes.Accepted, bill.ABL_MessageStatus);
				AssertEquals("packedItem.API_MessageStatus", MessageStatusCodeList.Codes.Accepted, packedItem.API_MessageStatus);
			}
		}

		public void TestAIRAEUMessageTypeAcknowledged()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <Workflow>
            <ActionPurpose Description=""SG ACCESS ACKNOWLEDGEMENT"">ACK</ActionPurpose>
          </Workflow>
          <DataSource>
            <DataProvider>SGA</DataProvider>
          </DataSource>
          <DataTargetCollection>
            <DataTarget>
              <Key>{0}</Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-02-27T19:48:38</EventTime>
        <EventType>MDL</EventType>
        <EventReference>MST=MGE</EventReference>
        <IsEstimate>false</IsEstimate>
        <ContextCollection>
          <Context>
            <Type>ManifestCountry</Type>
            <Value>SG</Value>
          </Context>
          <Context>
            <Type>MessageType</Type>
            <Value>AIRAEU</Value>
          </Context>
          <Context>
            <Type>UENNumber</Type>
            <Value>198801949D</Value>
          </Context>
          <Context>
            <Type>OriginalCreateDate</Type>
            <Value>20180227</Value>
          </Context>
          <Context>
            <Type>OriginalSerialNumber</Type>
            <Value>0074</Value>
          </Context>
          <Context>
            <Type>MasterBill</Type>
            <Value>4467847667</Value>
            <SubContextCollection>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>BILLHJKY23</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>1</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
            </SubContextCollection>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Header
				var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_ManifestType = "MGI";
				header.AMA_MasterBill = "4467847667";
				// Bill
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "BILLHJKY23";
				// Pack
				var pack = bill.Packs.AddNew();
				pack.ConsignmentReference = 1;
				var packedItem = pack.PackedItemForTesting();
				Factory.SaveForTesting();

				var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference));
				var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

				AssertEquals(1, logParents.Length);
				AssertEquals("Should get best matching Header.", GetHumanReadableID(header), GetHumanReadableID(logParents[0]));
				AssertEquals("header.AMA_MessageStatus", MessageStatusCodeList.Codes.Accepted, header.AMA_MessageStatus);
				AssertEquals("bill.ABC_MessageStatus", MessageStatusCodeList.Codes.Accepted, bill.ABL_MessageStatus);
				AssertEquals("packedItem.API_MessageStatus", MessageStatusCodeList.Codes.Accepted, packedItem.API_MessageStatus);
			}
		}

		public void TestAcknowledged_Cancellation()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <Workflow>
            <ActionPurpose Description=""SG ACCESS ACKNOWLEDGEMENT"">ACK</ActionPurpose>
          </Workflow>
          <DataSource>
            <DataProvider>SGA</DataProvider>
          </DataSource>
          <DataTargetCollection>
            <DataTarget>
              <Key>{0}</Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-02-27T19:48:38</EventTime>
        <EventType>MDL</EventType>
        <EventReference>MST=MGE</EventReference>
        <IsEstimate>false</IsEstimate>
        <ContextCollection>
          <Context>
            <Type>ManifestCountry</Type>
            <Value>SG</Value>
          </Context>
          <Context>
            <Type>MessageType</Type>
            <Value>AIRPCU</Value>
          </Context>
          <Context>
            <Type>UENNumber</Type>
            <Value>198801949D</Value>
          </Context>
          <Context>
            <Type>OriginalCreateDate</Type>
            <Value>20180227</Value>
          </Context>
          <Context>
            <Type>OriginalSerialNumber</Type>
            <Value>0074</Value>
          </Context>
          <Context>
            <Type>MasterBill</Type>
            <Value>4467847667</Value>
            <SubContextCollection>
              <SubContext>
                <Type>HouseBill</Type>
                <Value>BILLHJKY23</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentReference</Type>
                    <Value>1</Value>
                    <SubContextCollection>
                      <SubContext>
                        <Type>MessageStatusCode</Type>
                        <Value>ACK</Value>
                      </SubContext>
                    </SubContextCollection>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
            </SubContextCollection>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();

			// Header
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			header.AMA_MasterBill = "4467847667";
			// Bill
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BILLHJKY23";
			// Pack
			var pack = bill.Packs.AddNew();
			pack.ConsignmentReference = 1;
			var packedItem = pack.PackedItemForTesting();
			packedItem.API_PackStatus = "CAN";

			var cusEntry = packedItem.CustomsEntryNumbers.AddNew();
			cusEntry.CE_EntryType = "ASY";
			cusEntry.CE_EntryNum = "1";

			var cusEntry1 = packedItem.CustomsEntryNumbers.AddNew();
			cusEntry1.CE_EntryType = "TNP";
			cusEntry1.CE_EntryNum = "2";

			Factory.SaveForTesting();

			AssertEquals(2, packedItem.CustomsEntryNumbers.Count);

			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference));
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			AssertEquals(1, logParents.Length);
			AssertEquals("Should get best matching Header.", GetHumanReadableID(header), GetHumanReadableID(logParents[0]));
			AssertEquals("header.RegistrationStatus", "CN", header.RegistrationStatus);
			AssertEquals("bill.ABL_BillStatus", "CN", bill.ABL_BillStatus);
			AssertEquals("packedItem.API_PackStatus", "CAN", packedItem.API_PackStatus);
			AssertEquals(1, packedItem.CustomsEntryNumbers.Count);
			AssertEquals("TNP", packedItem.CustomsEntryNumbers[0].CE_EntryType);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "CN");
			Assert("header has event which type is 'STC' and reference is 'CN'", header.Logs.HasLogWith(logQuery));
		}

		public void TestStatusEventsAreAddedFromUniversalEventResponse()
		{
			const string eventXmlText = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataProvider>SGA</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Key>MAN000001A</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>MRR</EventType>
    <EventReference>MST=MGE</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101600</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101600</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>1</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ErrorCode</Type>
                    <Value>E3</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>IP</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ManifestPermitNumber</Type>
                    <Value>PERMIT3342</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>MSG2</Value>
              </SubContext>
              <SubContext>
                <Type>ConsignmentStatus</Type>
                <Value>CL</Value>
              </SubContext>
              <SubContext>
                <Type>ManifestPermitNumber</Type>
                <Value>PERMIT6544</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>MessageStatusCode</Type>
            <Value>MSG1</Value>
          </SubContext>
          <SubContext>
            <Type>ConsignmentStatus</Type>
            <Value></Value>
          </SubContext>
          <SubContext>
            <Type>ManifestPermitNumber</Type>
            <Value>PERMIT8345</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

			const string AcceptMessageCode = "MSG1";
			const string ErrorMessageCode = "MSG2";
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, AcceptMessageCode, "ACCEPT DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, ErrorMessageCode, "ERROR DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "MGI");

			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
			AssertEquals("", logger.Logs);

			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_JobReference = "MAN000001A";
			header.AMA_ManifestType = "MGI";
			header.AMA_MasterBill = "MB1708101600";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "HB1708101600";
			var pack = bill.Packs.AddNew();
			pack.ConsignmentReference = 1;
			var packedItem = pack.PackedItemForTesting();
			Factory.SaveForTesting();
			logger.ClearLogs();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			AssertEquals("Should get best matching Header.", GetHumanReadableID(header), GetHumanReadableID(logParents[0]));
			AssertEquals("header.RegistrationNumber", "PERMIT8345", header.RegistrationNumber);
			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "ERR");
			Assert("header has event which type is 'MSC' and reference is 'ERR'", header.Logs.HasLogWith(logQuery));
			logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "IP");
			Assert("header has event which type is 'STC' and reference is 'IP'", header.Logs.HasLogWith(logQuery));

			AssertEquals("bill.RegistrationNumber", "PERMIT6544", bill.RegistrationNumber);
			logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "MSG2");
			Assert("bill has event which type is 'MSC' and reference is 'MSG2'", bill.Logs.HasLogWith(logQuery));
			logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "CL");
			Assert("bill has event which type is 'STC' and reference is 'CL'", bill.Logs.HasLogWith(logQuery));
			packedItem.CustomsEntryNumbers.Load();
			AssertEquals("packedItem.RegistrationNumber", "PERMIT3342", packedItem.RegistrationNumber);
			logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "E3");
			Assert("pack has event which type is 'MSC' and reference is 'E3'", pack.Logs.HasLogWith(logQuery));
			logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "IP");
			Assert("pack has event which type is 'STC' and reference is 'IP'", pack.Logs.HasLogWith(logQuery));
		}

		public void TestStatusEventsAreAddedFromUniversalEventResponse_EachStatusOnlyOnce()
		{
			const string eventXmlText = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataProvider>SGA</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>MAN000001A</Key>
					<Type>AsycudaManifest</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>

		<EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
		<EventType>MRR</EventType>
		<EventReference>MST=MGE</EventReference>

		<ContextCollection>
			<Context>
				<Type>MasterBill</Type>
				<Value>MB1708101600</Value>
				<SubContextCollection>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>HB1708101600</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ErrorCode</Type>
										<Value>E3</Value>
									</SubContext>
									<SubContext>
										<Type>ConsignmentStatus</Type>
										<Value>REJ</Value>
									</SubContext>
									<SubContext>
										<Type>ManifestPermitNumber</Type>
										<Value>PERMIT3342</Value>
									</SubContext>
								</SubContextCollection>
							</SubContext>
							<SubContext>
								<Type>MessageStatusCode</Type>
								<Value>MSG1</Value>
							</SubContext>
							<SubContext>
								<Type>ConsignmentStatus</Type>
								<Value>CLR</Value>
							</SubContext>
							<SubContext>
								<Type>ManifestPermitNumber</Type>
								<Value>PERMIT6544</Value>
							</SubContext>
						</SubContextCollection>
					</SubContext>
					<SubContext>
						<Type>HouseBill</Type>
						<Value>HB1708101600</Value>
						<SubContextCollection>
							<SubContext>
								<Type>ConsignmentReference</Type>
								<Value>1</Value>
								<SubContextCollection>
									<SubContext>
										<Type>ErrorCode</Type>
										<Value>E3</Value>
									</SubContext>
									<SubContext>
										<Type>ConsignmentStatus</Type>
										<Value>REJ</Value>
									</SubContext>
									<SubContext>
										<Type>ManifestPermitNumber</Type>
										<Value>PERMIT3342</Value>
									</SubContext>
								</SubContextCollection>
							</SubContext>
							<SubContext>
								<Type>MessageStatusCode</Type>
								<Value>MSG1</Value>
							</SubContext>
							<SubContext>
								<Type>ConsignmentStatus</Type>
								<Value>CLR</Value>
							</SubContext>
							<SubContext>
								<Type>ManifestPermitNumber</Type>
								<Value>PERMIT6544</Value>
							</SubContext>
						</SubContextCollection>
					</SubContext>
					<SubContext>
						<Type>MessageStatusCode</Type>
						<Value>MSG2</Value>
					</SubContext>
					<SubContext>
						<Type>ConsignmentStatus</Type>
						<Value>FAL</Value>
					</SubContext>
					<SubContext>
						<Type>ManifestPermitNumber</Type>
						<Value>PERMIT8345</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
			AssertEquals("", logger.Logs);

			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_JobReference = "MAN000001A";
			header.AMA_ManifestType = "MGI";
			header.AMA_MasterBill = "MB1708101600";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "HB1708101600";
			var pack = bill.Packs.AddNew();
			pack.ConsignmentReference = 1;
			var packedItem = pack.PackedItemForTesting();
			Factory.SaveForTesting();
			logger.ClearLogs();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "E3");
			AssertEquals("pack has event which type is 'MSC' and reference is 'E3'", 1, pack.Logs.Find(logQuery).Length);
		}

		public void TestTradeNetPermitNumberAndTradeNetStatusAdded()
		{
			const string eventXmlText = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataProvider>SGA</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Key>MAN000001A</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>MRR</EventType>
    <EventReference>MST=MGE</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>08199009900</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>PWS77</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>1</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>CR</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ManifestPermitNumber</Type>
                    <Value>PERMIT3341</Value>
                  </SubContext>
                  <SubContext>
                    <Type>TradeNetPermitNumber</Type>
                    <Value>64643342</Value>
                  </SubContext>
                  <SubContext>
                    <Type>TradeNetPermitStatus</Type>
                    <Value>XXXX</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>PW78</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>2</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>IP</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ManifestPermitNumber</Type>
                    <Value>PERMIT3342</Value>
                  </SubContext>
                  <SubContext>
                    <Type>TradeNetPermitNumber</Type>
                    <Value>53442444</Value>
                  </SubContext>
                  <SubContext>
                    <Type>TradeNetPermitStatus</Type>
                    <Value>YYYYYYYYY</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
            </SubContextCollection>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
			AssertEquals("", logger.Logs);

			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_JobReference = "MAN000001A";
			header.AMA_ManifestType = "MGI";
			header.AMA_MasterBill = "08199009900";
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "PWS77";
			var pack1 = bill1.Packs.AddNew();
			pack1.ConsignmentReference = 1;
			var packedItem1 = pack1.PackedItemForTesting();
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "PW78";
			var pack2 = bill2.Packs.AddNew();
			pack2.ConsignmentReference = 2;
			var packedItem2 = pack2.PackedItemForTesting();
			Factory.SaveForTesting();
			logger.ClearLogs();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			AssertEquals("Should get best matching Header.", GetHumanReadableID(header), GetHumanReadableID(logParents[0]));

			packedItem1.CustomsEntryNumbers.Load();
			AssertEquals("packedItem1.CustomsEntryNumbers.Count", 2, packedItem1.CustomsEntryNumbers.Count);
			AssertEquals("packedItem1.CustomsEntryNumbers[0].CE_EntryType", Constants.CustomsEntryType.ACCESSPermit, packedItem1.CustomsEntryNumbers[0].CE_EntryType);
			AssertEquals("packedItem1.CustomsEntryNumbers[0].CE_EntryNum", "PERMIT3341", packedItem1.CustomsEntryNumbers[0].CE_EntryNum);
			AssertEquals("packedItem1.CustomsEntryNumbers[1].CE_EntryType", Constants.CustomsEntryType.TradeNetPermit, packedItem1.CustomsEntryNumbers[1].CE_EntryType);
			AssertEquals("packedItem1.CustomsEntryNumbers[1].CE_EntryNum", "64643342", packedItem1.CustomsEntryNumbers[1].CE_EntryNum);
			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatusCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "XXXX");
			Assert("pack1 has event which type is 'CES' and reference is 'XXXX'", pack1.Logs.HasLogWith(logQuery));

			packedItem2.CustomsEntryNumbers.Load();
			AssertEquals("packedItem2.CustomsEntryNumbers.Count", 2, packedItem2.CustomsEntryNumbers.Count);
			AssertEquals("packedItem2.CustomsEntryNumbers[0].CE_EntryType", Constants.CustomsEntryType.ACCESSPermit, packedItem2.CustomsEntryNumbers[0].CE_EntryType);
			AssertEquals("packedItem2.CustomsEntryNumbers[0].CE_EntryNum", "PERMIT3342", packedItem2.CustomsEntryNumbers[0].CE_EntryNum);
			AssertEquals("packedItem2.CustomsEntryNumbers[1].CE_EntryType", Constants.CustomsEntryType.TradeNetPermit, packedItem2.CustomsEntryNumbers[1].CE_EntryType);
			AssertEquals("packedItem2.CustomsEntryNumbers[1].CE_EntryNum", "53442444", packedItem2.CustomsEntryNumbers[1].CE_EntryNum);
			logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatusCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "YYYYYYYYY");
			Assert("pack2 has event which type is 'CES' and reference is 'YYYYYYYYY'", pack2.Logs.HasLogWith(logQuery));
		}

		public void TestDuplicateEventReferenceKeyShouldNotBeThrown()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent>
  <Event>
    <DataContext>
      <DataProvider>SGA</DataProvider>
      <DataTargetCollection>
        <DataTarget>
          <Key>{0}</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>MRR</EventType>
    <EventReference>|MST=MGE|MST=MGI</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101330</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101330</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>634</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ErrorCode</Type>
                    <Value>E3</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>REJ</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>8</Value>
              </SubContext>
              <SubContext>
                <Type>ConsignmentStatus</Type>
                <Value>CLR</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>MessageStatusCode</Type>
            <Value>9</Value>
          </SubContext>
          <SubContext>
            <Type>ConsignmentStatus</Type>
            <Value>FAL</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
      <Context>
        <Type>MasterBill</Type>
        <Value>4467847667</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101350</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>323</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>MessageStatusCode</Type>
                    <Value>9</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>A1</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>ErrorCode</Type>
                <Value>E3</Value>
              </SubContext>
              <SubContext>
                <Type>ConsignmentStatus</Type>
                <Value>K5</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>ErrorCode</Type>
            <Value>D43</Value>
          </SubContext>
          <SubContext>
            <Type>ConsignmentStatus</Type>
            <Value>K2</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();

			// Header
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			header.AMA_MasterBill = "4467847667";
			// Bill
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BILLHJKY23";
			// Pack
			var pack = bill.Packs.AddNew();
			pack.ConsignmentReference = 1;
			var packedItem = pack.PackedItemForTesting();
			Factory.SaveForTesting();

			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText, header.AMA_JobReference));
			AssertNoExceptionThrown("The duplicated pairs are: MST=MGI|MST=MGE", () =>
			{
				subscriber.GetLogParentsForEvent(xmlEvent);
			});
		}

		void AssertManifestPermitNumber(BusinessObject bizObj, ZString number)
		{
			var entryNumber = CusEntryNumber.Load(bizObj, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Singapore);
			AssertEquals("entryNumber.CE_EntryNum", number, entryNumber.CE_EntryNum);
		}

		AsycudaManifestHeaderDataEventParentFinder GetNewEventParentFinder(TestErrorLogger logger) => new AsycudaManifestHeaderDataEventParentFinder(Factory.BOFactory, new AsycudaManifestHeaderDataContextManager(), logger);

		static string GetHumanReadableID(BusinessObject businessObject) => businessObject.HumanReadableName + " (" + businessObject.GetType().FullName + ") - PK: " + businessObject.PK;
	}
}
