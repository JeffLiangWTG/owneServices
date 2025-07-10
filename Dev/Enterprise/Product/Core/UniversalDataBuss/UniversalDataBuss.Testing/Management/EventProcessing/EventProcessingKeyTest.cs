using System;
using System.Linq;
using CargoWise.Application;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	/// <summary>
	/// This class is for checking that messages generate keys for root jobs.
	/// This is necessary for GrEngine to safely process messages in parallel.
	/// </summary>
	class EventProcessingKeyTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			lazyServiceTaskLog = new Lazy<ServiceTaskLogForTesting>(() => new ServiceTaskLogForTesting());
		}

		Lazy<ServiceTaskLogForTesting> lazyServiceTaskLog;
		ServiceTaskLogForTesting ServiceTaskLog => lazyServiceTaskLog.Value;

		#endregion

		public void TestUniversalEventMessageProcessor()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var declaration = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "B" + 1.ToString().PadLeft(8, '0');
			declaration[JobDeclarationSchema.JE_TransportMode] = "SEA";
			declaration[JobDeclarationSchema.JE_MasterBill] = "CRACKERJACK";
			Factory.SaveForTesting();

			#region 

			const string UniversalEventForMBOLCrackerJack = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
	<EventTime>2016-05-12T12:03:04</EventTime>
	<EventType>ATH</EventType>
	<DataContext>
	  <DataProvider>CargoWise One</DataProvider>
	</DataContext>

	<ContextCollection>
	  <Context>
	    <Type>MBOLNumber</Type>
	    <Value>CRACKERJACK</Value>
	  </Context>
	</ContextCollection>
</Event>
</UniversalEvent>";

			#endregion

			var message = GetQueuedUniversalEventMessage(UniversalEventForMBOLCrackerJack);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertContainsExactElementsInAnyOrder(new[] { declaration.PK.ToStringKey(), "CRACKERJACK" }, manager.GetKeysForBlockingParallelImport(message).Keys.ToArray());
		}

		public void TestUniversalEventMessageProcessor_WithNoMatch()
		{
			#region
			const string nonMatchingMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
	</Event>
</UniversalEvent>";
			#endregion

			var message = GetQueuedUniversalEventMessage(nonMatchingMessage);
			var manager = new UniversalMessageProcessingManager(ServiceTaskLog);

			AssertEquals("Nothing matches this message.", true, manager.GetKeysForBlockingParallelImport(message).ShouldShortCircuit);
			AssertEquals("Nothing matches this message.", MessageStatus.Rejected, manager.GetKeysForBlockingParallelImport(message).Status);
		}

		public void TestUniversalShipmentMessageProcessor()
		{
			#region

			const string universalXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment>
    <DataContext>
			<DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001286</Key>
        </DataSource>
      </DataSourceCollection>
      <DataTargetCollection>
        <DataTarget>
          <Type>WarehouseOrder</Type>
        </DataTarget>
      </DataTargetCollection>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>FOR</Code>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <PortOfDischarge>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfLoading>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>BUNGA DELIMA</VesselName>
    <VoyageFlightNo>822</VoyageFlightNo>
    <WayBillNumber>FAT_TONY</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <SubShipmentCollection>
      <SubShipment>
        <ContainerMode>
          <Code>LCL</Code>
          <Description>Less Container Load</Description>
        </ContainerMode>
        <PortOfDestination>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>USSFO</Code>
          <Name>San Francisco</Name>
        </PortOfOrigin>
        <ShipmentType>
          <Code>STD</Code>
          <Description>Standard House</Description>
        </ShipmentType>
        <TransportMode>
          <Code>SEA</Code>
          <Description>Sea Freight</Description>
        </TransportMode>
        <WayBillNumber>JIMMY_THE_SNITCH</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";
			#endregion

			var message = GetQueuedUniversalShipmentMessage(universalXml);
			var manager = new UniversalMessageProcessingManager(ServiceTaskLog);

			var keys = manager.GetKeysForBlockingParallelImport(message);
			AssertEquals("No more short circuiting of universal shipments: Too much overhead?", false, keys.ShouldShortCircuit);
		}

		public void TestUniversalTransactionBatchMessageProcessor()
		{
			#region 
			const string universalXml = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<BatchType>
			<Code>TST</Code>
		</BatchType>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<CreateTime>2014-11-18T00:00:00</CreateTime>
				<PostDate>2014-11-17T00:00:00</PostDate>
				<PostingJournalCollection>
					<PostingJournal>
						<Description>TotalPaymentReceived</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";
			#endregion

			var message = GetQueuedUniversalTransactionBatchMessage(universalXml);

			var manager = new UniversalMessageProcessingManager(ServiceTaskLog);
			AssertContainsExactElementsInAnyOrder("Turns out that transactions don't ever generate keys.", Array.Empty<string>(), manager.GetKeysForBlockingParallelImport(message).Keys);
		}
	}
}
