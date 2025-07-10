using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.ZLogging.StmALog.Internal.Module.Testing
{
	sealed class EventSourceInfoUserControlTestCase : TestCaseWithFactory
	{
		#region TestSetDataBinding

		public void TestSetDataBinding()
		{
			var businessObject = Factory.New<DummyEnterpriseBusinessObject>();
			businessObject.Logs.AddNew(Events.Arrival);

			var linkedLog = businessObject.Logs.AddNew(Events.Manifested);
			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.Content = XElement.Parse(@"
			<UniversalEvent>
				<Event>
				  <EventType>OCR</EventType>
				  <EventTime>10-JUL-2010 18:00</EventTime>
				  <EventDescription>Dummy Description</EventDescription>
				  <DataProvider>Dummy</DataProvider>
				  <ContextCollection>
					<Context>
					  <Type>MAWBNumber</Type>
					  <Value>020-12345675</Value>
					</Context>
					<Context>
					  <Type>MAWBOriginIATAAirportCode</Type>
					  <Value>JFK</Value>
					</Context>
					<Context>
					  <Type>MAWBDestinationIATAAirportCode</Type>
					  <Value>BKK</Value>
					</Context>
					<Context>
					  <Type>MAWBNumberOfPieces</Type>
					  <Value>20</Value>
					</Context>
					<Context>
					  <Type>NativeEventCode</Type>
					  <Value>RCS</Value>
					</Context>
					<Context>
					  <Type>NumberOfPieces</Type>
					  <Value>20</Value>
					</Context>
					<Context>
					  <Type>IATAAirportCode</Type>
					  <Value>JFK</Value>
					</Context>
					<Context>
					  <Type>ReceivedFromName</Type>
					  <Value>SHIPPERNAME</Value>
					</Context>
				  </ContextCollection>
				</Event>
			</UniversalEvent>
			");

			var pivot = Factory.New<IGenPivot>();
			pivot.XX_Relation1ID = linkedLog.PK;
			pivot.XX_Relation1TableCode = "SL";
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = "EM";
			pivot.XX_RelationType = "XEM";

			Factory.Save();

			var logs = businessObject.Logs.GetAllLogs();
			logs.Sort(StmALogSchema.SL_SE_NKEvent.Name, System.ComponentModel.ListSortDirection.Ascending);

			using (var form = new ZForm())
			{
				var filterControl = new ZGrid();
				var info1 = new ZTextBoxColumnStyleInfo();
				info1.ColumnName = StmALogSchema.Constants.SL_Reference;
				filterControl.ColumnStyles.Add(info1);

				var eventSourceInfoUserControl = new EventSourceInfoUserControl();

				form.Controls.Add(filterControl);
				form.Controls.Add(eventSourceInfoUserControl);

				filterControl.SetDataBinding(logs, "");
				eventSourceInfoUserControl.SetDataBinding(logs, "");

				form.Show();

				Assert("Precondition: Should not show Source Info", !eventSourceInfoUserControl.Visible);
				filterControl.ListManager.Position = 1;
				Assert("Should show Source Info for log that have linked message", eventSourceInfoUserControl.Visible);
			}
		}

		public void TestSetDataBinding_NoContextBoys()
		{
			var businessObject = Factory.New<DummyEnterpriseBusinessObject>();
			businessObject.Logs.AddNew(Events.Arrival);

			var linkedLog = businessObject.Logs.AddNew(Events.DataImport);
			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.Content = XElement.Parse(@"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <ActualChargeable>3</ActualChargeable>
    <AdditionalTerms></AdditionalTerms>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CartageWaybillNumber></CartageWaybillNumber>
    <CFSReference></CFSReference>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <DocumentedChargeable>3</DocumentedChargeable>
    <DocumentedVolume>0</DocumentedVolume>
    <DocumentedWeight>3000.000</DocumentedWeight>
    <FreightRate>0</FreightRate>
    <FreightRateCurrency>
      <Code></Code>
    </FreightRateCurrency>
    <GoodsDescription>Tables</GoodsDescription>
    <GoodsValue>150000.00</GoodsValue>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </GoodsValueCurrency>
    <HBLAWBChargesDisplay>
      <Code>SHW</Code>
      <Description>Show Collect Charges</Description>
    </HBLAWBChargesDisplay>
    <HBLContainerPackModeOverride></HBLContainerPackModeOverride>
    <InsuranceValue>150000.00</InsuranceValue>
    <InsuranceValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </InsuranceValueCurrency>
    <InterimReceiptNumber></InterimReceiptNumber>
    <IsBooking>false</IsBooking>
    <IsCancelled>false</IsCancelled>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsNeutralMaster>false</IsNeutralMaster>
    <IsShipping>false</IsShipping>
    <IsSplitShipment>false</IsSplitShipment>

    <ManifestedChargeable>3</ManifestedChargeable>
    <ManifestedVolume>0</ManifestedVolume>
    <ManifestedWeight>3000.000</ManifestedWeight>
    <NoCopyBills>1</NoCopyBills>
    <NoOriginalBills>0</NoOriginalBills>
    <OuterPacks>600</OuterPacks>
    <OuterPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </OuterPacksPackageType>
    <PackingOrder>0</PackingOrder>
    <PortOfDestination>
      <Code>AUMEL</Code>
      <Name>Melbourne</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfOrigin>
    <ReleaseType>
      <Code>EBL</Code>
      <Description>Express Bill of Lading</Description>
    </ReleaseType>
    <ScreeningStatus>
      <Code>UNK</Code>
      <Description>Unknown</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <ShipmentType>
      <Code>STD</Code>
      <Description>Standard House</Description>
    </ShipmentType>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <ShipperCODAmount>0</ShipperCODAmount>
    <ShipperCODPayMethod>
      <Code></Code>
    </ShipperCODPayMethod>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Metres</Description>
    </TotalVolumeUnit>
    <TotalWeight>3000.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WarehouseLocation></WarehouseLocation>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised></DeliveryCartageAdvised>
      <DeliveryCartageCompleted></DeliveryCartageCompleted>
      <DeliveryLabourCharge>0</DeliveryLabourCharge>
      <DeliveryLabourTime></DeliveryLabourTime>
      <DeliveryRequiredBy></DeliveryRequiredBy>
      <DemurrageOnDeliveryCharge>0</DemurrageOnDeliveryCharge>
      <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
      <DemurrageOnPickupCharge>0</DemurrageOnPickupCharge>
      <DemurrageOnPickupTime></DemurrageOnPickupTime>
      <EstimatedDelivery></EstimatedDelivery>
      <EstimatedPickup></EstimatedPickup>
      <ExportStatement>
        <Code></Code>
      </ExportStatement>
      <FCLAvailable></FCLAvailable>
      <FCLDeliveryEquipmentNeeded>
        <Code>WUP</Code>
        <Description>Wait for Pack/Unpack</Description>
      </FCLDeliveryEquipmentNeeded>
      <FCLPickupEquipmentNeeded>
        <Code>WUP</Code>
        <Description>Wait for Pack/Unpack</Description>
      </FCLPickupEquipmentNeeded>
      <FCLStorageCommences></FCLStorageCommences>
      <HasProhibitedPackaging>false</HasProhibitedPackaging>
      <InsuranceRequired>false</InsuranceRequired>
      <IsContingencyRelease>false</IsContingencyRelease>
      <LCLAirStorageCharge>0</LCLAirStorageCharge>
      <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
      <LCLAvailable></LCLAvailable>
      <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
      <LCLStorageCommences></LCLStorageCommences>
      <PickupCartageAdvised></PickupCartageAdvised>
      <PickupCartageCompleted></PickupCartageCompleted>
      <PickupLabourCharge>0</PickupLabourCharge>
      <PickupLabourTime></PickupLabourTime>
      <PickupRequiredBy></PickupRequiredBy>
      <PrintOptionForPackagesOnAWB>
        <Code>DEF</Code>
        <Description>Default (Dims, fallback to Vol)</Description>
      </PrintOptionForPackagesOnAWB>
    </LocalProcessing>

    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <MilestoneCollection>
      <Milestone>
        <Description>All Import Documents Received</Description>
        <EventCode>AID</EventCode>
        <Sequence>10</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
      <Milestone>
        <Description>Delivery Cartage Complete/Finalised</Description>
        <EventCode>DCF</EventCode>
        <Sequence>15</Sequence>
        <ActualDate></ActualDate>
        <ConditionReference></ConditionReference>
        <ConditionType></ConditionType>
        <EstimatedDate></EstimatedDate>
      </Milestone>
    </MilestoneCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <Address1>235 JOHNSON STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>235 JOHNSON STREET</AddressShortCode>
        <City>LOS ANGELES</City>
        <CompanyName>BOOKS GALORE PTY LTD</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>39-384844800</GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <OrganizationCode>BOOGALLAX</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>USLAX</Code>
          <Name>Los Angeles</Name>
        </Port>
        <Postcode>90019</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>CA</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <Address1>DOCK DOOR 88</Address1>
        <Address2>332 EDMONDS AVENUE</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>DOCK DOOR 88</AddressShortCode>
        <City>LOS ANGELES</City>
        <CompanyName>BOOKS GALORE PTY LTD</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax>+13108489383</Fax>
        <GovRegNum>39-384844800</GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <OrganizationCode>BOOGALLAX</OrganizationCode>
        <Phone>+13108483839</Phone>
        <Port>
          <Code>USLAX</Code>
          <Name>Los Angeles</Name>
        </Port>
        <Postcode>90019</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>CA</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>49-89 TURNER STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>16 TURNER STREET</AddressShortCode>
        <City>PORT MELBOURNE</City>
        <CompanyName>BBIT (AU) CORPORATION</CompanyName>
        <Contact>Operations</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>operations.aumel@bbit.com</Email>
        <Fax>+61333864512</Fax>
        <Mobile></Mobile>
        <OrganizationCode>BBITAUMEL</OrganizationCode>
        <Phone>+61333864512</Phone>
        <Port>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </Port>
        <Postcode>3207</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>VIC</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>BBITAU_AU</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <Address1>49-89 TURNER STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>16 TURNER STREET</AddressShortCode>
        <City>PORT MELBOURNE</City>
        <CompanyName>BBIT (AU) CORPORATION</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.aumel@bbit.com</Email>
        <Fax>+61333864512</Fax>
        <OrganizationCode>BBITAUMEL</OrganizationCode>
        <Phone>+61333864512</Phone>
        <Port>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </Port>
        <Postcode>3207</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>VIC</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>BBITAU_AU</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection Content=""Complete"">
      <PackingLine>
        <Commodity>
          <Code>GEN</Code>
          <Description>General</Description>
        </Commodity>
        <ContainerPackingOrder>0</ContainerPackingOrder>
        <CountryOfOrigin>
          <Code></Code>
        </CountryOfOrigin>
        <DetailedDescription></DetailedDescription>
        <EndItemNo>0</EndItemNo>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription>Tables</GoodsDescription>
        <HarmonisedCode></HarmonisedCode>
        <Height>0</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <ItemNo>0</ItemNo>
        <Length>0</Length>
        <LengthUnit>
          <Code>M</Code>
          <Description>Metres</Description>
        </LengthUnit>
        <LinePrice>0</LinePrice>
        <Link>1</Link>
        <LoadingMeters>0</LoadingMeters>
        <MarksAndNos></MarksAndNos>
        <OutturnComment></OutturnComment>
        <OutturnDamagedQty>0</OutturnDamagedQty>
        <OutturnedHeight>0</OutturnedHeight>
        <OutturnedLength>0</OutturnedLength>
        <OutturnedVolume>0</OutturnedVolume>
        <OutturnedWeight>0</OutturnedWeight>
        <OutturnedWidth>0</OutturnedWidth>
        <OutturnPillagedQty>0</OutturnPillagedQty>
        <OutturnQty>0</OutturnQty>
        <PackQty>600</PackQty>
        <PackType>
          <Code>PKG</Code>
          <Description>Package</Description>
        </PackType>
        <ReferenceNumber></ReferenceNumber>
        <Volume>0</Volume>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Metres</Description>
        </VolumeUnit>
        <Weight>3000.000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
        <Width>0</Width>

        <PackedItemCollection>
        </PackedItemCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>");

			var pivot = Factory.New<IGenPivot>();
			pivot.XX_Relation1ID = linkedLog.PK;
			pivot.XX_Relation1TableCode = "SL";
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = "EM";
			pivot.XX_RelationType = "XEM";

			Factory.Save();

			var logs = businessObject.Logs.GetAllLogs();
			logs.Sort(StmALogSchema.SL_SE_NKEvent.Name, System.ComponentModel.ListSortDirection.Ascending);

			using (var form = new ZForm())
			{
				var filterControl = new ZGrid();
				var info1 = new ZTextBoxColumnStyleInfo();
				info1.ColumnName = StmALogSchema.Constants.SL_Reference;
				filterControl.ColumnStyles.Add(info1);

				var eventSourceInfoUserControl = new EventSourceInfoUserControl();

				form.Controls.Add(filterControl);
				form.Controls.Add(eventSourceInfoUserControl);

				filterControl.SetDataBinding(logs, "");
				eventSourceInfoUserControl.SetDataBinding(logs, "");

				form.Show();

				Assert("Precondition: Should not show Source Info", !eventSourceInfoUserControl.Visible);
				filterControl.ListManager.Position = 1;
				Assert("Should show Source Info for log that have linked message", eventSourceInfoUserControl.Visible);
			}
		}

		#endregion

		#region TestShowMessageButtonClick_WithoutEDIMessagePermission

		public void TestShowMessageButtonClick_WithoutEDIMessagePermission()
		{
			// setup test user and deny EDIMessage permission
			var initialUserContext = Env.CurrentUserContext;

			var staff = (BusinessObject)Factory.New<IGlbStaff>();
			staff[GlbStaffSchema.GS_LoginName] = "TST";

			var createEDIMessageCheckPoint = Env.Security.EDIMessage;
			var securityRecord = (BusinessObject)Factory.New<IGlbSecurity>();
			securityRecord[GlbSecuritySchema.GU_SecurityRight] = createEDIMessageCheckPoint.Code;
			securityRecord[GlbSecuritySchema.GU_ItemGUID] = createEDIMessageCheckPoint.ItemGuid;
			securityRecord[GlbSecuritySchema.GU_SecurityItemIsAllowed] = false;
			securityRecord[GlbSecuritySchema.GU_GS] = staff.PK;
			var staffType = staff.GetType();
			var propertyInfo = staffType.GetProperty("GroupSecurityPermissionsCollectionForBinding");
			var securityPermissionCollection = (BusinessObjectCollection)propertyInfo.GetValue(staff, null);
			securityPermissionCollection.Add(securityRecord);
			Factory.Save();

			// Setup universal test data
			var businessObject = Factory.New<DummyEnterpriseBusinessObject>();
			businessObject.Logs.AddNew(Events.Arrival);

			var linkedLog = businessObject.Logs.AddNew(Events.Manifested);
			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.Content = XElement.Parse(universalXmlContent);

			var pivot = Factory.New<IGenPivot>();
			pivot.XX_Relation1ID = linkedLog.PK;
			pivot.XX_Relation1TableCode = "SL";
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = "EM";
			pivot.XX_RelationType = "XEM";
			Factory.Save();

			var logs = businessObject.Logs.GetAllLogs();
			logs.Sort(StmALogSchema.SL_SE_NKEvent.Name, ListSortDirection.Ascending);

			// test user without security rights to view EDIMessage
			using (Env.SetTemporaryUserContext((ZString)staff[GlbStaffSchema.GS_LoginName], Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var buffer = new NotificationBuffer();
				using (var form = new ZForm())
				{
					var filterControl = new ZGrid();
					var info = new ZTextBoxColumnStyleInfo();
					info.ColumnName = StmALogSchema.Constants.SL_Reference;
					filterControl.ColumnStyles.Add(info);

					var eventSourceInfoUserControl = new EventSourceInfoUserControlForTest();
					form.Controls.Add(filterControl);
					form.Controls.Add(eventSourceInfoUserControl);
					filterControl.SetDataBinding(logs, "");
					eventSourceInfoUserControl.SetDataBinding(logs, "");

					form.Show();
					filterControl.ListManager.Position = 1;
					AssertEquals("Precondition : Event source control should be visible.", true, eventSourceInfoUserControl.Visible);

					AssertNoExceptionThrown("No exception should be thrown when there are no EDIMessage security rights for the current user.", () => eventSourceInfoUserControl.ShowMessageButtonForTest.PerformClick());
					AssertEquals("You do not have the appropriate security rights to run this function.\r\n\r\n" +
						"If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\n" +
						"Maintain -> EDI Messaging -> EDI Message", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		#region TestShowMessageButtonClick_WhenRelatedEDIMessageIsNull

		public void TestShowMessageButtonClick_WhenRelatedEDIMessageIsNull()
		{
			var businessObject = Factory.New<DummyEnterpriseBusinessObject>();
			businessObject.Logs.AddNew(Events.Arrival);

			var linkedLog = businessObject.Logs.AddNew(Events.Manifested);

			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.Content = XElement.Parse(universalXmlContent);

			var pivot = Factory.New<IGenPivot>();
			pivot.XX_Relation1ID = linkedLog.PK;
			pivot.XX_Relation1TableCode = "SL";
			pivot.XX_Relation2ID = ZGuid.Empty;
			pivot.XX_Relation2TableCode = "EM";
			pivot.XX_RelationType = "XEM";

			Factory.Save();

			var logs = businessObject.Logs.GetAllLogs();
			logs.Sort(StmALogSchema.SL_SE_NKEvent.Name, ListSortDirection.Ascending);

			using (var form = new ZForm())
			{
				var filterControl = new ZGrid();
				var info = new ZTextBoxColumnStyleInfo();
				info.ColumnName = StmALogSchema.Constants.SL_Reference;
				filterControl.ColumnStyles.Add(info);

				var eventSourceInfoUserControl = new EventSourceInfoUserControlForTest();
				form.Controls.Add(filterControl);
				form.Controls.Add(eventSourceInfoUserControl);
				filterControl.SetDataBinding(logs, "");
				eventSourceInfoUserControl.SetDataBinding(logs, "");

				AssertNoExceptionThrown(() => eventSourceInfoUserControl.ShowMessageButton_Click(this, null));
			}
		}

		#endregion

		public void TestShowMessageButtonClick_WithJobReadOnlyAndEDIMessagePermissions()
		{
			var businessObject = Factory.New<DummyEnterpriseBusinessObject>();
			businessObject.Logs.AddNew(Events.Arrival);

			var linkedLog = businessObject.Logs.AddNew(Events.Manifested);
			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.Content = XElement.Parse(universalXmlContent);

			var pivot = Factory.New<IGenPivot>();
			pivot.XX_Relation1ID = linkedLog.PK;
			pivot.XX_Relation1TableCode = "SL";
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = "EM";
			pivot.XX_RelationType = "XEM";
			Factory.Save();

			var logs = businessObject.Logs.GetAllLogs();
			logs.Sort(StmALogSchema.SL_SE_NKEvent.Name, ListSortDirection.Ascending);

			using (var form = new ZForm(businessObject))
			using (var eventSourceInfoUserControl = new EventSourceInfoUserControlForTest())
			using (var filterControl = new ZGrid())
			{
				form.DisplayMode = Core.ODisplayMode.ReadOnly;
				var info = new ZTextBoxColumnStyleInfo();
				info.ColumnName = StmALogSchema.Constants.SL_Reference;
				filterControl.ColumnStyles.Add(info);

				form.Controls.Add(filterControl);
				form.Controls.Add(eventSourceInfoUserControl);
				filterControl.SetDataBinding(logs, "");
				eventSourceInfoUserControl.SetDataBinding(logs, "");

				form.Show();
				filterControl.ListManager.Position = 1;
				AssertEquals("Precondition : Event source control should be visible.", true, eventSourceInfoUserControl.Visible);

				eventSourceInfoUserControl.SetDataBinding(logs, "");
				Assert("Show message button should not be read only", !eventSourceInfoUserControl.ShowMessageButtonForTest.GetReadOnly());
				AssertNoExceptionThrown("No exception should be thrown when there are EDIMessage security rights and shipment read only rights for the current user.", () => eventSourceInfoUserControl.ShowMessageButtonForTest.PerformClick());
				using (var ediMessageForm = ZApplication.GetOpenForms().FirstOrDefault(f => f.Name == "EDIMessageForm"))
				{
					AssertNotNull("Message form should be open", ediMessageForm);
				}
			}
		}

		public void TestShowMessageButtonClick_WithJobEditAndEDIMessagePermissions()
		{
			var businessObject = Factory.New<DummyEnterpriseBusinessObject>();
			businessObject.Logs.AddNew(Events.Arrival);

			var linkedLog = businessObject.Logs.AddNew(Events.Manifested);
			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.Content = XElement.Parse(universalXmlContent);

			var pivot = Factory.New<IGenPivot>();
			pivot.XX_Relation1ID = linkedLog.PK;
			pivot.XX_Relation1TableCode = "SL";
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = "EM";
			pivot.XX_RelationType = "XEM";
			Factory.Save();

			var logs = businessObject.Logs.GetAllLogs();
			logs.Sort(StmALogSchema.SL_SE_NKEvent.Name, ListSortDirection.Ascending);

			using (var form = new ZForm())
			using (var eventSourceInfoUserControl = new EventSourceInfoUserControlForTest())
			using (var filterControl = new ZGrid())
			{
				var info = new ZTextBoxColumnStyleInfo();
				info.ColumnName = StmALogSchema.Constants.SL_Reference;
				filterControl.ColumnStyles.Add(info);

				form.Controls.Add(filterControl);
				form.Controls.Add(eventSourceInfoUserControl);
				filterControl.SetDataBinding(logs, "");
				eventSourceInfoUserControl.SetDataBinding(logs, "");

				form.Show();
				filterControl.ListManager.Position = 1;
				AssertEquals("Precondition : Event source control should be visible.", true, eventSourceInfoUserControl.Visible);

				Assert("Show message button should not be read only", !eventSourceInfoUserControl.ShowMessageButtonForTest.GetReadOnly());
				AssertNoExceptionThrown("No exception should be thrown when there are EDIMessage security rights and shipment edit rights for the current user.", () => eventSourceInfoUserControl.ShowMessageButtonForTest.PerformClick());
				using (var ediMessageForm = ZApplication.GetOpenForms().FirstOrDefault(f => f.Name == "EDIMessageForm"))
				{
					AssertNotNull("Message form should be open", ediMessageForm);
				}
			}
		}

		#region Implementation

		class EventSourceInfoUserControlForTest : EventSourceInfoUserControl
		{
			public Button ShowMessageButtonForTest { get { return showMessageButton; } }
		}

		const string universalXmlContent = @"
			<UniversalEvent>
				<Event>
				  <EventType>OCR</EventType>
				  <EventTime>10-JUL-2010 18:00</EventTime>
				  <EventDescription>Dummy Description</EventDescription>
				  <DataProvider>Dummy</DataProvider>
				  <ContextCollection>
					<Context>
					  <Type>MAWBNumber</Type>
					  <Value>020-12345675</Value>
					</Context>
					<Context>
					  <Type>MAWBOriginIATAAirportCode</Type>
					  <Value>JFK</Value>
					</Context>
					<Context>
					  <Type>MAWBDestinationIATAAirportCode</Type>
					  <Value>BKK</Value>
					</Context>
					<Context>
					  <Type>MAWBNumberOfPieces</Type>
					  <Value>20</Value>
					</Context>
					<Context>
					  <Type>NativeEventCode</Type>
					  <Value>RCS</Value>
					</Context>
					<Context>
					  <Type>NumberOfPieces</Type>
					  <Value>20</Value>
					</Context>
					<Context>
					  <Type>IATAAirportCode</Type>
					  <Value>JFK</Value>
					</Context>
					<Context>
					  <Type>ReceivedFromName</Type>
					  <Value>SHIPPERNAME</Value>
					</Context>
				  </ContextCollection>
				</Event>
			</UniversalEvent>";

		#endregion
	}
}
