using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	public class AdditionalInfoDetailsControlTest : TestCaseWithFactory
	{
		public void TestFields()
		{
			using (var control = new AdditionalInfoDetailsControl())
			{
				AssertType<ZTextBox>("AddInfoDescriptionTextBox must be ZTextBox", control.AddInfoDescriptionTextBox);
				AssertType<ZDropEdit>("AddInfoTypeCodeDropEdit must be ZDropEdit", control.AddInfoTypeCodeDropEdit);
			}
		}
	}
}
