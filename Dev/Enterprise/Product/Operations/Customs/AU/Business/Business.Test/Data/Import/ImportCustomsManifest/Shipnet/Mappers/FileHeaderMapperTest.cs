using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FileHeaderMapperTest : BaseMapperTest
	{
		public void TestFromFileHeaderRow()
		{
			FileHeaderMapper mapper = new FileHeaderMapper();
			FileHeaderDataRow row = new FileHeaderDataRow("HDRIMM       HORIZON                            CRMIMM                             EDICMR    01   MORE      Cape Moreton                       8012695   AAL       AAL                                1105A5      YPGLAELae                                              AUBNEBrisbane                                                        13096334771");
			Xsd.CusImportManifest manifest = new Xsd.CusImportManifest();
			mapper.FromFileHeaderRow(row, manifest);

			AssertEquals("Cape Moreton", manifest.Vessel.Name.Trim());
			AssertEquals("8012695", manifest.Vessel.Lloyds.Trim());
			AssertEquals("05A5", manifest.VoyageNumber.Trim());
			AssertEquals("PGLAE", manifest.LastForeignPortOfDeparture.Port.Value.Trim());
		}
	}
}
