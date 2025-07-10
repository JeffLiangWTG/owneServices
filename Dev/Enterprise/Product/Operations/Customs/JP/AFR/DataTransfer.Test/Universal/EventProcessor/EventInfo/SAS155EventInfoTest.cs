using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class SAS155EventInfoTest : TestCase
	{
		public void TestProperties()
		{
			var sas155XML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NON</Code>
				<Description>SAS155</Description>
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
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var xmlEvent = new XmlEventDeserializer().Parse(sas155XML);

			var eventInfo = new SAS155EventInfo(xmlEvent);

			AssertEquals("MasterBillNumber ", "MB20170306", eventInfo.MasterBillNumber);
			AssertEquals("CarrierCode", "JEFF", eventInfo.CarrierCode);
			AssertEquals("VesselCallSign", "OVYQ2", eventInfo.VesselCallSign);
			AssertEquals("VoyageNumber", "12345678", eventInfo.VoyageNumber);
			AssertEquals("LoadingPortCode", "AUSYD", eventInfo.LoadingPortCode);
			AssertEquals("LoadingPortSuffix", "1", eventInfo.LoadingPortSuffix);

			sas155XML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NON</Code>
				<Description>SAS155</Description>
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
				<Type>PortOfDischargeCode</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>VesselCallSignNew</Type>
				<Value>OVYQ2</Value>
			</Context>
			<Context>
				<Type>VoyageNumberNew</Type>
				<Value>12345678</Value>
			</Context>
			<Context>
				<Type>CarrierCodeNew</Type>
				<Value>JEFF</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingUNLOCONew</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingSuffixNew</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>NON</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			xmlEvent = new XmlEventDeserializer().Parse(sas155XML);

			eventInfo = new SAS155EventInfo(xmlEvent);

			AssertEquals("MasterBillNumber ", "MB20170306", eventInfo.MasterBillNumber);
			AssertEquals("CarrierCode", "JEFF", eventInfo.CarrierCode);
			AssertEquals("VesselCallSign", "OVYQ2", eventInfo.VesselCallSign);
			AssertEquals("VoyageNumber", "12345678", eventInfo.VoyageNumber);
			AssertEquals("LoadingPortCode", "AUSYD", eventInfo.LoadingPortCode);
			AssertEquals("LoadingPortSuffix", "1", eventInfo.LoadingPortSuffix);
			AssertEquals("PortOfDischargeCode", "AUSYD", eventInfo.DischargeCode);
			AssertEquals("CarrierCodeNew", "JEFF", eventInfo.NewCarrierCode);
			AssertEquals("VesselCallSignNew", "OVYQ2", eventInfo.NewVesselCallSign);
			AssertEquals("VoyageNumberNew", "12345678", eventInfo.NewVoyageNumber);
			AssertEquals("LoadingPortCodeNew", "AUSYD", eventInfo.NewLoadingPortCode);
			AssertEquals("LoadingPortSuffixNew", "1", eventInfo.NewLoadingPortSuffix);
			AssertEquals("IsNON", true, eventInfo.IsNON);
			AssertEquals("Count of Bills to update", 1, eventInfo.BillsToUpdate.Count);
			var bill = eventInfo.BillsToUpdate[0];
			AssertEquals("Process Result Code of Bill 1", "", bill.ProcessResultCode);
			AssertEquals("Bill 1 number", "NON", bill.BillNo);

			sas155XML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NON</Code>
				<Description>SAS155</Description>
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
				<Type>PortOfDischargeCode</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>VesselCallSignNew</Type>
				<Value>OVYQ2</Value>
			</Context>
			<Context>
				<Type>VoyageNumberNew</Type>
				<Value>12345678</Value>
			</Context>
			<Context>
				<Type>CarrierCodeNew</Type>
				<Value>JEFF</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingUNLOCONew</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingSuffixNew</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>NACC1234567891</Value>
					</SubContext>
					<SubContext>
						<Type>ProcessResultCode</Type>
						<Value>E0006</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			xmlEvent = new XmlEventDeserializer().Parse(sas155XML);

			eventInfo = new SAS155EventInfo(xmlEvent);

			AssertEquals("MasterBillNumber ", "MB20170306", eventInfo.MasterBillNumber);
			AssertEquals("CarrierCode", "JEFF", eventInfo.CarrierCode);
			AssertEquals("VesselCallSign", "OVYQ2", eventInfo.VesselCallSign);
			AssertEquals("VoyageNumber", "12345678", eventInfo.VoyageNumber);
			AssertEquals("LoadingPortCode", "AUSYD", eventInfo.LoadingPortCode);
			AssertEquals("LoadingPortSuffix", "1", eventInfo.LoadingPortSuffix);
			AssertEquals("PortOfDischargeCode", "AUSYD", eventInfo.DischargeCode);
			AssertEquals("CarrierCodeNew", "JEFF", eventInfo.NewCarrierCode);
			AssertEquals("VesselCallSignNew", "OVYQ2", eventInfo.NewVesselCallSign);
			AssertEquals("VoyageNumberNew", "12345678", eventInfo.NewVoyageNumber);
			AssertEquals("LoadingPortCodeNew", "AUSYD", eventInfo.NewLoadingPortCode);
			AssertEquals("LoadingPortSuffixNew", "1", eventInfo.NewLoadingPortSuffix);
			AssertEquals("IsNON", false, eventInfo.IsNON);
			AssertEquals("Count of Bills to update", 1, eventInfo.BillsToUpdate.Count);
			bill = eventInfo.BillsToUpdate[0];
			AssertEquals("Process Result Code of Bill 1", "E0006", bill.ProcessResultCode);
			AssertEquals("Bill 1 number", "NACC1234567891", bill.BillNo);
		}
	}
}
