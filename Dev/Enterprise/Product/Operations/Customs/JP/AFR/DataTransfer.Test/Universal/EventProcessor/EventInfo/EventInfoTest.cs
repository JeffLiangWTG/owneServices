using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class EventInfoTest : TestCase
	{
		public void TestProperties()
		{
			var uxml = @"
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
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var xmlEvent = new XmlEventDeserializer().Parse(uxml);

			var eventInfo = new EventInfo(xmlEvent);

			AssertEquals("MasterBillNumber ", "MB20170306", eventInfo.MasterBillNumber);
			AssertEquals("CarrierCode", "JEFF", eventInfo.CarrierCode);
			AssertEquals("VesselCallSign", "OVYQ2", eventInfo.VesselCallSign);
			AssertEquals("VoyageNumber", "12345678", eventInfo.VoyageNumber);
			AssertEquals("LoadingPortCode", "AUSYD", eventInfo.LoadingPortCode);
			AssertEquals("LoadingPortSuffix", "1", eventInfo.LoadingPortSuffix);
		}
	}
}
