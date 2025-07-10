using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.GB.DataTransfer.Test.Universal
{
	public class CcsukAirManifestTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestCcsukAirManifestProcessing()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var mawbCount = Factory.Load<CusMAWB>(new ZDBOnlyQuery(typeof(CusMAWB))).Length;
				var hawbCount = Factory.Load<CusHAWB>(new ZDBOnlyQuery(typeof(CusHAWB))).Length;

				var helper = new TestCaseWithFactoryAndMessagingHelpers();
				var message = GetQueuedUniversalShipmentMessage(GetCcsukAirManifestXml());

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertContains("ERROR - AirManifest is not currently supported for country 'GB'.", serviceTaskLog.ToString());
				Assert(Factory.Load<CusMAWB>(new ZDBOnlyQuery(typeof(CusMAWB))).Length == mawbCount);
				Assert(Factory.Load<CusHAWB>(new ZDBOnlyQuery(typeof(CusHAWB))).Length == hawbCount);
			}
		}

		internal static string GetCcsukAirManifestXml() => EmbeddedResourceHelper.GetMessageXml("Universal.TestFiles.CcsukAirManifest.xml");
	}

	static class EmbeddedResourceHelper
	{
		public static ZString GetMessageXml(ZString path)
		{
			var assembly = typeof(EmbeddedResourceHelper).Assembly;
			var foundPath = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith("." + path));
			using (var inStream = assembly.GetManifestResourceStream(foundPath))
			{
				return new StreamReader(inStream).ReadToEnd();
			}
		}
	}
}
