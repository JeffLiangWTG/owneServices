using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCusSCAOceanBillDataObjectWriter))]
	partial class CMRCusSCAOceanBillDataObjectWriterTest : CusSCAOceanBillDataObjectWriterTest<CusSCAOceanBill, CusSCAHouse, CusSCAContainer, CusSCAPivot>
	{
		protected override ITopLevelDataObjectWriter GetWriter(IDataWritingManager writeManager)
		{
			return new CMRCusSCAOceanBillDataObjectWriter(writeManager);
		}

		protected override CusSCAOceanBill CreateOceanBill()
		{
			var oceanBill = base.CreateOceanBill();
			oceanBill.CB_MessageReference = ZString.Empty;

			oceanBill.SetUserDefinedValue("Test1", (ZString)"Test1Value");
			oceanBill.SetUserDefinedValue("Test2", (ZInt)111111);
			return oceanBill;
		}

		protected override string OceanBillShipmentXML
		{
			get { return FileReader.GetEmbeddedFileText(TestFilesPath, "OceanBillShipment.xml"); }
		}

		TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(CMRCusSCAOceanBillDataObjectWriterTest)));
		TestFileReader fileReader;

		const string TestFilesPath = "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Xml.Universal.SeaCargo.TestFiles";
	}
}
