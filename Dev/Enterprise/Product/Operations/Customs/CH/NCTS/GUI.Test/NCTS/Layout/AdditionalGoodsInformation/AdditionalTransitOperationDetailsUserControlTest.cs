using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

class AdditionalTransitOperationDetailsUserControlTest : TestCase
{
	public void TestControlTypes()
	{
		using (var control = new AdditionalTransitOperationDetailsUserControl())
		{
			AssertType<ZCalcEdit>("LineNoCalcEdit", control.LineNoCalcEdit);
			AssertType<ZDropEdit>("TypeDropEdit", control.TypeDropEdit);
			AssertType<ZTextBox>("ReferenceTextBox", control.ReferenceTextBox);
			AssertType<ZTextBox>("DescriptionTextBox", control.DescriptionTextBox);
			AssertType<ZCalcDropEdit>("GrossMassDropEdit", control.GrossMassDropEdit);
			AssertType<ZCalcDropEdit>("PackagesCalcDropEdit", control.PackagesCalcDropEdit);
			AssertType<ZDropEdit>("StateOfSealsDropEdit", control.StateOfSealsDropEdit);
		}
	}

	public void TestDescriptionTextBox()
	{
		using (var control = new AdditionalTransitOperationDetailsUserControl())
		{
			AssertEquals("Multiline", true, control.DescriptionTextBox.Multiline);
		}
	}

	public void TestPackagesCalcDropEdit()
	{
		using (var control = new AdditionalTransitOperationDetailsUserControl())
		{
			AssertEquals("ShowDescriptionBox", true, control.PackagesCalcDropEdit.ShowDescriptionBox);
		}
	}
}
