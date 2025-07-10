using System;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.IO;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Testing.Management
{
	class GrEngineKeyGenHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestUniversalEventMessage()
		{
			var declaration = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "B" + 1.ToString().PadLeft(8, '0');
			declaration[JobDeclarationSchema.JE_TransportMode] = "SEA";
			declaration[JobDeclarationSchema.JE_MasterBill] = "CRACKER";
			declaration[JobDeclarationSchema.JE_HouseBill] = "CRACKER";
			Factory.SaveForTesting();

			#region 

			const string UniversalEventForMBOLTest = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
	    <Value>CRACKER</Value>
	  </Context>
	  <Context>
	    <Type>HBOLNumber</Type>
	    <Value>CRACKER</Value>
	  </Context>
	</ContextCollection>
</Event>
</UniversalEvent>";

			#endregion
			var message = GetQueuedUniversalEventMessage(UniversalEventForMBOLTest);
			message.EM_MessageNum = "TEST0001";

			var expectedTotalKeys = "Total Keys: 2";
			var expectedKeys = declaration.PK.ToStringKey() + ", CRACKER";
			var expectedKeysInfo = new[] { (declaration.PK.ToStringKey(), "JobDeclaration/PK"), ("CRACKER", "Context/HBOLNumber+Context/MBOLNumber") };
			var expectedLogs = new[] { "Provider type: DataMessageProcessor" };

			using (var tempDir = new TempDirectory())
			{
				GrEngineKeyGenHelper.GenerateKey(message, tempDir);
				var fileContent = File.ReadAllText(Path.Combine(tempDir, $"KeyInfo_{message.EM_MessageNum}.txt"));
				AssertContains(expectedTotalKeys, fileContent);
				AssertContains(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(expectedKeys)), fileContent);
				foreach (var pair in expectedKeysInfo)
				{
					AssertContains(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(pair.Item1)), fileContent);
					AssertContains(pair.Item2, fileContent);
				}
				foreach (var log in expectedLogs)
				{
					AssertContains(log, fileContent);
				}
			}
		}

		public void TestUniversalEventMessageWithNoMatch()
		{
			#region
			const string nonMatchingMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
	</Event>
</UniversalEvent>";
			#endregion

			var message = GetQueuedUniversalEventMessage(nonMatchingMessage);
			message.EM_MessageNum = "TEST0001";

			var expectedTotalKeys = "Total Keys: 0";
			using (var tempDir = new TempDirectory())
			{
				GrEngineKeyGenHelper.GenerateKey(message, tempDir);
				var fileContent = File.ReadAllText(Path.Combine(tempDir, $"KeyInfo_{message.EM_MessageNum}.txt"));
				AssertContains(expectedTotalKeys, fileContent);
			}
		}

		public void TestUniversalShipmentMessage()
		{
			var shipment = (Forwarding.IForwardingShipment)Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "C00001286";
			shipment.JS_GoodsDescription = "frozen spinach";

			Factory.SaveForTesting();
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
    <WayBillNumber>TestBill</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>
    <LocalProcessing>
      <ArrivalCartageRef>TestRef</ArrivalCartageRef>
      <DeliveryCartageAdvised>1788-01-26T00:00:00</DeliveryCartageAdvised>
      <DeliveryCartageCompleted>2188-01-26T00:00:00</DeliveryCartageCompleted>
      <EstimatedDelivery>2012-01-01T00:00:00</EstimatedDelivery>
    </LocalProcessing>

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
			message.EM_MessageNum = "TEST0001";

			var expectedTotalKeys = "Total Keys: 2";
			var expectedKeys = "TestBill, TestRef";
			var expectedKeysInfo = new[] { ("TestBill", "WayBillNumber"), ("TestRef", "LocalProcessing/ArrivalCartageRef") };
			var expectedLogs = new[] { "Provider type: DataMessageProcessor" };

			using (var tempDir = new TempDirectory())
			{
				GrEngineKeyGenHelper.GenerateKey(message, tempDir);
				var fileContent = File.ReadAllText(Path.Combine(tempDir, $"KeyInfo_{message.EM_MessageNum}.txt"));
				AssertContains(expectedTotalKeys, fileContent);
				AssertContains(expectedKeys, fileContent);
				foreach (var pair in expectedKeysInfo)
				{
					AssertContains(pair.Item1, fileContent);
					AssertContains(pair.Item2, fileContent);
				}
				foreach (var log in expectedLogs)
				{
					AssertContains(log, fileContent);
				}
			}
		}

		public void TestUniversalTransactionBatchMessage()
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
			message.EM_MessageNum = "TEST0001";

			var expectedTotalKeys = "Total Keys: 0";

			using (var tempDir = new TempDirectory())
			{
				GrEngineKeyGenHelper.GenerateKey(message, tempDir);
				var fileContent = File.ReadAllText(Path.Combine(tempDir, $"KeyInfo_{message.EM_MessageNum}.txt"));
				AssertContains(expectedTotalKeys, fileContent);
			}
		}

		public void TestMessageWithInvalidCharactersInKeyString()
		{
			var jobDeclaration = Factory.BOFactory.NewWithPrimaryKey<BaseJobDeclaration>(Guid.Parse("69a6c51a-4303-41ba-a86c-b9520b779cdd"));
			jobDeclaration[JobDeclarationSchema.JE_MasterBill] = "THENETHER";
			jobDeclaration[JobDeclarationSchema.JE_HouseBill] = "THENETHER";
			jobDeclaration[JobDeclarationSchema.JE_DataModel] = "TS";
			jobDeclaration[JobDeclarationSchema.JE_GC] = Env.CurrentCompany.PK;

			Factory.SaveForTesting();

			const string UniversalEventForMBOLTest = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
	    <Value>THENETHER</Value>
	  </Context>
	  <Context>
	    <Type>HBOLNumber</Type>
	    <Value>THENETHER</Value>
	  </Context>
	</ContextCollection>
</Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(UniversalEventForMBOLTest);
			message.EM_MessageNum = "TEST0001";

			var expectedBizoPkStringKey = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(jobDeclaration.PK.ToStringKey()));
			var expectedTotalKeys = "Total Keys: 2";
			var expectedKeys = expectedBizoPkStringKey + ", THENETHER";
			var expectedKeysInfo = new[] { (expectedBizoPkStringKey, "JobDeclaration/PK"), ("THENETHER", "Context/HBOLNumber+Context/MBOLNumber") };
			var expectedLogs = new[] { "Provider type: DataMessageProcessor" };

			using (var tempDir = new TempDirectory())
			{
				AssertNoExceptionThrown(() => GrEngineKeyGenHelper.GenerateKey(message, tempDir));
				var fileContent = File.ReadAllText(Path.Combine(tempDir, $"KeyInfo_{message.EM_MessageNum}.txt"));
				AssertContains(expectedTotalKeys, fileContent);
				AssertContains(expectedKeys, fileContent);
				foreach (var pair in expectedKeysInfo)
				{
					AssertContains(pair.Item1, fileContent);
					AssertContains(pair.Item2, fileContent);
				}
				foreach (var log in expectedLogs)
				{
					AssertContains(log, fileContent);
				}
			}
		}
	}
}
