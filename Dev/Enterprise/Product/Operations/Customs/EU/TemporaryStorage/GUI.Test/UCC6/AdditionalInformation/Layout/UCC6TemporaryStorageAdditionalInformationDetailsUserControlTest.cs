using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	public class UCC6TemporaryStorageAdditionalInformationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6TemporaryStorageAdditionalInformationDetailsUserControl())
			{
				AssertNotNull(control.FindSingle<ZCalcEdit>("AmountCalcEdit"));
				AssertNotNull(control.FindSingle<ZDropEdit>("CurrencyDropEdit"));
				AssertNotNull(control.FindSingle<ZTextBox>("DetailTextBox"));
				AssertNotNull(control.FindSingle<ZTextBox>("ReferenceTextBox"));
				AssertNotNull(control.FindSingle<ZDropEdit>("KindDropEdit"));
				AssertNotNull(control.FindSingle<ZCodeFindBox>("FullTypeCodeFindBox"));
				AssertNotNull(control.FindSingle<ZTextBox>("DescriptionTextBox"));
			}
		}
	}
}
