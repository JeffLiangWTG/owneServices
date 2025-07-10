using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCusSCAOceanBillDataEventParentFinder))]
	sealed class CMRCusSCAOceanBillDataEventParentFinderTest : CusSCAOceanBillDataEventParentFinderAbstractTest<CMRCusSCAOceanBillDataEventParentFinder, CusSCAOceanBill>
	{
		protected override ZString CorrectApplicationCode
		{
			get { return Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages; }
		}

		protected override CMRCusSCAOceanBillDataEventParentFinder GetNewParentFinder(BusinessObjectFactory factory, CusSCAOceanBillDataContextManager manager, IXmlImportLogger logger)
		{
			return new CMRCusSCAOceanBillDataEventParentFinder(factory, manager, logger);
		}

		protected override string OceanBillEventXML
		{
			get { return FileReader.GetEmbeddedFileText(TestFilesPath, "OceanBillEvent.xml"); }
		}

		TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(CMRCusSCAOceanBillDataEventParentFinderTest)));
		TestFileReader fileReader;

		const string TestFilesPath = "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Xml.Universal.SeaCargo.TestFiles";
	}
}
