using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(CGMManifestUserControl))]
sealed class CGMManifestUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new CGMManifestUserControl())
		{
			AssertEquals("DataSourceType", typeof(CGMAsycudaManifestHeader), control.DataSourceType);
		}
	}

	public void TestManifestQtyCalcDropEditBehaviour()
	{
		var header = Factory.NewWithValidTestData<CGMAsycudaManifestHeader>();
		header.AMA_TransportMode = TransportTypeList.Codes.Air;
		using var form = new ZForm(header);
		using var control = new CGMManifestUserControl();
		form.Controls.Add(control);
		form.Show();

		var manifestQtyCalcDropEdit = (ZCalcDropEdit)control.Controls.Find("ManifestQtyCalcDropEdit", true).Single();

		AssertEquals("MaxValue", 99999999m, manifestQtyCalcDropEdit.MaxValue);
		AssertEquals("Decimal", 0, manifestQtyCalcDropEdit.Decimals);
	}
}
