using System.IO;
using System.Reflection;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.DataTransfer.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageJobHeaderDataContextManager))]
	class ITCusTempStorageJobHeaderDataContextManagerTest : ShipmentDataContextManagerTestCase<CusTempStorageJobHeaderDataContextManager, CusTempStorageJobHeader>
	{
		protected override bool ManagerChecksDataTargetToImport => false;

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => SampleUxml;

		public static string SampleUxml => LoadSampleUxml("Enterprise.Customs.EU.DataTransfer.Testing.CusTempStorage.TestFiles.UniversalXmlTest1.xml");

		public static string LoadSampleUxml(string key)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(key))
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy);
		}
	}
}
