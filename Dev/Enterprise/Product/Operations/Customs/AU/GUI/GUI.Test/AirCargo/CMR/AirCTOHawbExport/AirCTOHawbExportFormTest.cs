using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	[TestedType(typeof(AirCTOHawbExportForm))]
	sealed class AirCTOHawbExportFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new AirCTOHawbExportForm(Factory.New<AirCTOExportCustomsManifestHeader>().Lines.AddNew());
	}
}
