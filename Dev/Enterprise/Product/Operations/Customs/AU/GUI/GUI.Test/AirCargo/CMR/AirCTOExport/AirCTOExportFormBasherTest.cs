using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	[TestedType(typeof(AirCTOExportForm))]
	sealed class AirCTOExportFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new AirCTOExportForm(header);

		AirCTOExportCustomsManifestHeader header;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AirCTOExportCustomsManifestHeader>();
			ExportCustomsManifestLines line = header.Lines.AddNew();
			Factory.Save();
		}
	}
}
