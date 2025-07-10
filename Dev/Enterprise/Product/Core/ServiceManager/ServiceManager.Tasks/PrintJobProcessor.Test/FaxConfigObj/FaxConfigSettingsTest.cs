using Enterprise.Faxing.Integration;
using Enterprise.ZArchitecture.DataMapping.Testing;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	sealed class FaxConfigSettingsTest : XmlSerializableTestCase<FaxConfigSettings>
	{
		public void TestWriteXml()
		{
			FaxConfigSettings settings = new FaxConfigSettings();
			string result = Serialize(settings);
			AssertEquals("<FaxConfigSettings><LocalCountry /><LocalAreaCode /><LocalID /><OutsideLinePrefix /><InternationalCallPrefix /><EnableLogging>False</EnableLogging></FaxConfigSettings>", result);

			settings.LocalCountry = "AU";
			settings.LocalAreaCode = "2";
			settings.LocalID = "1111-2222";
			settings.OutsideLinePrefix = "9";
			settings.InternationalCallPrefix = "0011";
			settings.Ports = new FaxPortConfigSettings[] { new FaxPortConfigSettings() { PortName = "COM1" }, new FaxPortConfigSettings() { PortName = "COM3" } };
			settings.EnableLogging = true;

			result = Serialize(settings);
			AssertEquals("<FaxConfigSettings><LocalCountry>AU</LocalCountry><LocalAreaCode>2</LocalAreaCode><LocalID>1111-2222</LocalID><OutsideLinePrefix>9</OutsideLinePrefix><InternationalCallPrefix>0011</InternationalCallPrefix><Port><Name>COM1</Name></Port><Port><Name>COM3</Name></Port><EnableLogging>True</EnableLogging></FaxConfigSettings>", result);
		}

		public void TestReadXml()
		{
			string xml = "<FaxConfigSettings />";
			FaxConfigSettings settings = Deserialize(xml);
			AssertEquals("", settings.LocalCountry);
			AssertEquals("", settings.LocalAreaCode);
			AssertEquals("", settings.LocalID);
			AssertEquals("", settings.OutsideLinePrefix);
			AssertEquals("", settings.InternationalCallPrefix);
			AssertEquals(0, settings.Ports.Count);
			AssertEquals(false, settings.EnableLogging);

			xml = "<FaxConfigSettings><LocalCountry>AU</LocalCountry><LocalAreaCode>2</LocalAreaCode><LocalID>1111-2222</LocalID><OutsideLinePrefix>9</OutsideLinePrefix><InternationalCallPrefix>0011</InternationalCallPrefix><Port><Name>COM1</Name></Port><Port><Name>COM3</Name></Port><EnableLogging>True</EnableLogging></FaxConfigSettings>";
			settings = Deserialize(xml);
			AssertEquals("AU", settings.LocalCountry);
			AssertEquals("2", settings.LocalAreaCode);
			AssertEquals("1111-2222", settings.LocalID);
			AssertEquals("9", settings.OutsideLinePrefix);
			AssertEquals("0011", settings.InternationalCallPrefix);
			AssertEquals(2, settings.Ports.Count);
			AssertEquals("COM1", settings.Ports[0].PortName);
			AssertEquals("COM3", settings.Ports[1].PortName);
			AssertEquals(true, settings.EnableLogging);
		}

		public void TestLogger()
		{
			FaxConfigSettings settings = new FaxConfigSettings();
			TestServiceLogger log = new TestServiceLogger();
			settings.SetLogger(log);

			IFaxLogger logger = ((IFaxConfig)settings).GetLogger();
			logger.Log(FaxLogType.Debug, "1");
			logger.Log(FaxLogType.Error, "2");
			logger.Log(FaxLogType.Information, "3");
			logger.Log(FaxLogType.Warning, "4");

			AssertEquals(4, log.Count);
			AssertEquals("Debug|1", log[0]);
			AssertEquals("Error|2", log[1]);
			AssertEquals("Information|3", log[2]);
			AssertEquals("Warning|4", log[3]);
		}
	}
}
