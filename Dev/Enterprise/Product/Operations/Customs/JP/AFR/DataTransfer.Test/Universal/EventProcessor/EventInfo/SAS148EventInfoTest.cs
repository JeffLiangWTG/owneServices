using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class SAS148EventInfoTest : TestCase
	{
		public void TestProperties()
		{
			var sas148XML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NRS</Code>
				<Description>SAS148</Description>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2013-12-04T04:37:00.0000000Z</EventTime>
		<EventType>MSC</EventType>
		<EventReference>TEST</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB20170306</Value>
			</Context>
			<Context>
				<Type>VesselCallSign</Type>
				<Value>OVYQ2</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>A P MOLLER</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>12345678</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>JEFF</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingSuffix</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>MasterBillIdentifier</Type>
				<Value>M</Value>
			</Context>
			<Context>
				<Type>DateTimeOfDeletion</Type>
				<Value>2013-03-25 20:00</Value>
			</Context>
			<Context>
				<Type>TimeOfArrival</Type>
				<Value>2013-03-24 20:00</Value>
			</Context>
			<Context>
				<Type>TimeOfDeparture</Type>
				<Value>2013-03-26 20:00</Value>
			</Context>
			<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>MB20170306-1</Value>
					</SubContext>
					<SubContext>
						<Type>DiscrepancyCode</Type>
						<Value>S</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>MB20170306-X</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var xmlEvent = new XmlEventDeserializer().Parse(sas148XML);

			var sAS148EventInfo = new SAS148EventInfo(xmlEvent);

			AssertEquals("MasterBillIdentifier", "M", sAS148EventInfo.MasterBillIdentifier);
			AssertEquals("MasterBillNumber ", "MB20170306", sAS148EventInfo.MasterBillNumber);
			AssertEquals("DateTimeOfDeletion ", "2013-03-25 20:00", sAS148EventInfo.DateTimeOfDeletion);
			AssertEquals("NewCarrierCode", "JEFF", sAS148EventInfo.NewCarrierCode);
			AssertEquals("NewVesselCallSign", "OVYQ2", sAS148EventInfo.NewVesselCallSign);
			AssertEquals("NewVoyageNumber", "12345678", sAS148EventInfo.NewVoyageNumber);
			AssertEquals("NewLoadingPortCode", "AUSYD", sAS148EventInfo.NewLoadingPortCode);
			AssertEquals("NewLoadingPortSuffix", "1", sAS148EventInfo.NewLoadingPortSuffix);
			AssertEquals("ETA ", new ZDateTime(2013, 3, 24, 20, 0, 0), sAS148EventInfo.ETA);
			AssertEquals("ETD ", new ZDateTime(2013, 3, 26, 20, 0, 0), sAS148EventInfo.ETD);
			AssertEquals("AreAllVesselDetailEmpty", false, sAS148EventInfo.AreAllVesselDetailEmpty);
			AssertEquals("AreAllHouseBillDiscrepancyCodeEmpty", false, sAS148EventInfo.AreAllHouseBillDiscrepancyCodeEmpty);
			AssertEquals("Count of BillsToUpdate", 2, sAS148EventInfo.BillsToUpdate.Count);
			var bill1 = sAS148EventInfo.BillsToUpdate[0];
			var bill2 = sAS148EventInfo.BillsToUpdate[1];
			AssertEquals("BillNo of bill1", "MB20170306-1", bill1.BillNo);
			AssertEquals("DiscrepancyCode of bill1", "S", bill1.DiscrepancyCode);
			AssertEquals("BillNo of bill2", "MB20170306-X", bill2.BillNo);
			AssertEquals("DiscrepancyCode of bill2", "", bill2.DiscrepancyCode);

			sas148XML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NRS</Code>
				<Description>SAS148</Description>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2013-12-04T04:37:00.0000000Z</EventTime>
		<EventType>MSC</EventType>
		<EventReference>TEST</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB20170306</Value>
			</Context>
			<Context>
				<Type>VesselCallSign</Type>
				<Value>OVYQ2</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>A P MOLLER</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>12345678</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>JEFF</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingSuffix</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>MasterBillIdentifier</Type>
				<Value>M</Value>
			</Context>
			<Context>
				<Type>DateTimeOfDeletion</Type>
				<Value>2013-03-25 20:00</Value>
			</Context>
			<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>MB20170306-1</Value>
					</SubContext>
					<SubContext>
						<Type>DiscrepancyCode</Type>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>MB20170306-X</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			xmlEvent = new XmlEventDeserializer().Parse(sas148XML);

			sAS148EventInfo = new SAS148EventInfo(xmlEvent);

			AssertEquals("MasterBillIdentifier", "M", sAS148EventInfo.MasterBillIdentifier);
			AssertEquals("MasterBillNumber ", "MB20170306", sAS148EventInfo.MasterBillNumber);
			AssertEquals("DateTimeOfDeletion ", "2013-03-25 20:00", sAS148EventInfo.DateTimeOfDeletion);
			AssertEquals("NewCarrierCode", "JEFF", sAS148EventInfo.NewCarrierCode);
			AssertEquals("NewVesselCallSign", "OVYQ2", sAS148EventInfo.NewVesselCallSign);
			AssertEquals("NewVoyageNumber", "12345678", sAS148EventInfo.NewVoyageNumber);
			AssertEquals("NewLoadingPortCode", "AUSYD", sAS148EventInfo.NewLoadingPortCode);
			AssertEquals("NewLoadingPortSuffix", "1", sAS148EventInfo.NewLoadingPortSuffix);
			AssertEquals("AreAllVesselDetailEmpty", false, sAS148EventInfo.AreAllVesselDetailEmpty);
			AssertEquals("AreAllHouseBillDiscrepancyCodeEmpty", true, sAS148EventInfo.AreAllHouseBillDiscrepancyCodeEmpty);
			AssertEquals("Count of BillsToUpdate", 2, sAS148EventInfo.BillsToUpdate.Count);
			bill1 = sAS148EventInfo.BillsToUpdate[0];
			bill2 = sAS148EventInfo.BillsToUpdate[1];
			AssertEquals("BillNo of bill1", "MB20170306-1", bill1.BillNo);
			AssertEquals("DiscrepancyCode of bill1", "", bill1.DiscrepancyCode);
			AssertEquals("BillNo of bill2", "MB20170306-X", bill2.BillNo);
			AssertEquals("DiscrepancyCode of bill2", "", bill2.DiscrepancyCode);

			sas148XML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NRS</Code>
				<Description>SAS148</Description>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2013-12-04T04:37:00.0000000Z</EventTime>
		<EventType>MSC</EventType>
		<EventReference>TEST</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB20170306</Value>
			</Context>
			<Context>
				<Type>MasterBillIdentifier</Type>
				<Value>M</Value>
			</Context>
			<Context>
				<Type>DateTimeOfDeletion</Type>
				<Value>2013-03-25 20:00</Value>
			</Context>
			<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>MB20170306-1</Value>
					</SubContext>
					<SubContext>
						<Type>DiscrepancyCode</Type>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>MB20170306-X</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			xmlEvent = new XmlEventDeserializer().Parse(sas148XML);

			sAS148EventInfo = new SAS148EventInfo(xmlEvent);

			AssertEquals("MasterBillIdentifier", "M", sAS148EventInfo.MasterBillIdentifier);
			AssertEquals("MasterBillNumber ", "MB20170306", sAS148EventInfo.MasterBillNumber);
			AssertEquals("DateTimeOfDeletion ", "2013-03-25 20:00", sAS148EventInfo.DateTimeOfDeletion);
			AssertEquals("ETA ", ZDateTime.Empty, sAS148EventInfo.ETA);
			AssertEquals("ETD ", ZDateTime.Empty, sAS148EventInfo.ETD);
			AssertEquals("NewCarrierCode", "", sAS148EventInfo.NewCarrierCode);
			AssertEquals("NewVesselCallSign", "", sAS148EventInfo.NewVesselCallSign);
			AssertEquals("NewVoyageNumber", "", sAS148EventInfo.NewVoyageNumber);
			AssertEquals("NewLoadingPortCode", "", sAS148EventInfo.NewLoadingPortCode);
			AssertEquals("NewLoadingPortSuffix", "", sAS148EventInfo.NewLoadingPortSuffix);
			AssertEquals("AreAllVesselDetailEmpty", true, sAS148EventInfo.AreAllVesselDetailEmpty);
			AssertEquals("AreAllHouseBillDiscrepancyCodeEmpty", true, sAS148EventInfo.AreAllHouseBillDiscrepancyCodeEmpty);
			AssertEquals("Count of BillsToUpdate", 2, sAS148EventInfo.BillsToUpdate.Count);
			bill1 = sAS148EventInfo.BillsToUpdate[0];
			bill2 = sAS148EventInfo.BillsToUpdate[1];
			AssertEquals("BillNo of bill1", "MB20170306-1", bill1.BillNo);
			AssertEquals("DiscrepancyCode of bill1", "", bill1.DiscrepancyCode);
			AssertEquals("BillNo of bill2", "MB20170306-X", bill2.BillNo);
			AssertEquals("DiscrepancyCode of bill2", "", bill2.DiscrepancyCode);

			sas148XML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NRS</Code>
				<Description>SAS148</Description>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2013-12-04T04:37:00.0000000Z</EventTime>
		<EventType>MSC</EventType>
		<EventReference>TEST</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB20170306</Value>
			</Context>
			<Context>
				<Type>VesselCallSign</Type>
				<Value>OVYQ2</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>A P MOLLER</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>12345678</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>JEFF</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingSuffix</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>MasterBillIdentifier</Type>
				<Value>M</Value>
			</Context>
			<Context>
				<Type>DateTimeOfDeletion</Type>
				<Value>2013-03-25 20:00</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			xmlEvent = new XmlEventDeserializer().Parse(sas148XML);

			sAS148EventInfo = new SAS148EventInfo(xmlEvent);

			AssertEquals("MasterBillIdentifier", "M", sAS148EventInfo.MasterBillIdentifier);
			AssertEquals("MasterBillNumber ", "MB20170306", sAS148EventInfo.MasterBillNumber);
			AssertEquals("DateTimeOfDeletion ", "2013-03-25 20:00", sAS148EventInfo.DateTimeOfDeletion);
			AssertEquals("NewCarrierCode", "JEFF", sAS148EventInfo.NewCarrierCode);
			AssertEquals("NewVesselCallSign", "OVYQ2", sAS148EventInfo.NewVesselCallSign);
			AssertEquals("NewVoyageNumber", "12345678", sAS148EventInfo.NewVoyageNumber);
			AssertEquals("NewLoadingPortCode", "AUSYD", sAS148EventInfo.NewLoadingPortCode);
			AssertEquals("NewLoadingPortSuffix", "1", sAS148EventInfo.NewLoadingPortSuffix);
			AssertEquals("AreAllVesselDetailEmpty", false, sAS148EventInfo.AreAllVesselDetailEmpty);
			AssertEquals("AreAllHouseBillDiscrepancyCodeEmpty", true, sAS148EventInfo.AreAllHouseBillDiscrepancyCodeEmpty);
			AssertEquals("Count of BillsToUpdate", 0, sAS148EventInfo.BillsToUpdate.Count);

			sas148XML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NRS</Code>
				<Description>SAS148</Description>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2013-12-04T04:37:00.0000000Z</EventTime>
		<EventType>MSC</EventType>
		<EventReference>TEST</EventReference>
	</Event>
</UniversalEvent>
";
			xmlEvent = new XmlEventDeserializer().Parse(sas148XML);

			sAS148EventInfo = new SAS148EventInfo(xmlEvent);

			AssertEquals("MasterBillIdentifier", "", sAS148EventInfo.MasterBillIdentifier);
			AssertEquals("MasterBillNumber ", "", sAS148EventInfo.MasterBillNumber);
			AssertEquals("DateTimeOfDeletion ", "", sAS148EventInfo.DateTimeOfDeletion);
			AssertEquals("NewCarrierCode", "", sAS148EventInfo.NewCarrierCode);
			AssertEquals("NewVesselCallSign", "", sAS148EventInfo.NewVesselCallSign);
			AssertEquals("NewVoyageNumber", "", sAS148EventInfo.NewVoyageNumber);
			AssertEquals("NewLoadingPortCode", "", sAS148EventInfo.NewLoadingPortCode);
			AssertEquals("NewLoadingPortSuffix", "", sAS148EventInfo.NewLoadingPortSuffix);
			AssertEquals("AreAllVesselDetailEmpty", true, sAS148EventInfo.AreAllVesselDetailEmpty);
			AssertEquals("AreAllHouseBillDiscrepancyCodeEmpty", true, sAS148EventInfo.AreAllHouseBillDiscrepancyCodeEmpty);
			AssertEquals("Count of BillsToUpdate", 0, sAS148EventInfo.BillsToUpdate.Count);
		}
	}
}
