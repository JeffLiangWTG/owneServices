using Enterprise.ZArchitecture.DataMapping.Testing;

namespace Enterprise.ServiceManager.Business.Testing
{
	class HostedServiceSerializableSettingsTest : XmlSerializableTestCase<HostedServiceSerializableSettings>
	{
		public void TestWriteXml()
		{
			var settings = new HostedServiceSerializableSettings();
			var result = Serialize(settings);
			AssertEquals("<HostedServiceSerializableSettings><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>", result);

			settings.SecondaryProcessesMaxCount = 5;
			result = Serialize(settings);
			AssertEquals("<HostedServiceSerializableSettings><SecondaryProcessesMaxCount>5</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>", result);

			settings.SecondaryProcessesMaxCount = 0;
			settings.ConfigString = "<TestConfigString />";
			result = Serialize(settings);
			AssertEquals("<HostedServiceSerializableSettings><ConfigString>&lt;TestConfigString /&gt;</ConfigString><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>", result);
		}

		public void TestErrorReportWithIncorrectXmlFormat()
		{
			var xml = "InvalidHostedServiceSerializableSettings";
			var settings = Deserialize(xml);

			AssertNotNull(CargoWise.Common.ErrorReporter.LastExceptionReported);
			AssertEquals(string.Format("XmlSerializableSetting.FromXml<HostedServiceSerializableSettings>{0}The Unicode-string value of the binary data was : [{1}]", System.Environment.NewLine, xml), CargoWise.Common.ErrorReporter.LastMessageReported);

			CargoWise.Common.ErrorReporter.Clear();
		}
	}
}