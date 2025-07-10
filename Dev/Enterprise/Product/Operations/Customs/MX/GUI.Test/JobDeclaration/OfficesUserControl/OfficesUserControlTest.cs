using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(CustomsAreaUserControl))]
	class CustomsAreaUserControlTest : TestCase
	{
		public void TestProperties()
		{
			using (var control = new CustomsAreaUserControl())
			{
				AssertType<ZCodeFindBox>("ClearanceAreaDropEdit type should be", control.ClearanceAreaFindBox);
				AssertType<ZCodeFindBox>("EntryAreaDropEdit type should be", control.EntryOrExitAreaFindBox);
			}
		}
	}
}
