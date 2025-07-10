using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	class PremisesFilterStripTest : TestCaseWithFactory
	{
		public void TestControlsCaption()
		{
			using (var premisesFilterStrip = new PremisesFilterStrip())
			{
				var premisesTypeDropEdit = (ZDropEdit)premisesFilterStrip.Controls.Find("PremisesTypeDropEdit", true).Single();
				var premisesCodeDropEdit = (ZDropEdit)premisesFilterStrip.Controls.Find("PremisesCodeDropEdit", true).Single();
				var premisesCodeTextBox = (ZTextBox)premisesFilterStrip.Controls.Find("PremisesCodeTextBox", true).Single();
				var premisesLocationCodeFindBox = (ZCodeFindBox)premisesFilterStrip.Controls.Find("PremisesLocationCodeFindBox", true).Single();

				CombineAssertions(() =>
				{
					AssertEquals("PremisesTypeDropEdit Caption", "Premises Type", premisesTypeDropEdit.CaptionResourceString.Caption);
					AssertEquals("PremisesCodeDropEdit Caption", "Premises Code", premisesCodeDropEdit.CaptionResourceString.Caption);
					AssertEquals("PremisesCodeTextBox Caption", "Premises Code", premisesCodeTextBox.CaptionResourceString.Caption);
					AssertEquals("PremisesLocationCodeFindBox Caption", "Premises Location", premisesLocationCodeFindBox.CaptionResourceString.Caption);
				});
			}
		}
	}
}
