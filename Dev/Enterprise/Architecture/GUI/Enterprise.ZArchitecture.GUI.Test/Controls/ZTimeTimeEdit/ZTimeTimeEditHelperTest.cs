using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZTimeTimeEditHelperTest : ZTimeTimeEditHelperTest2
	{
		public override void TestGetTextFromTime()
		{
			AssertGetTextFromTime("12:45", new ZTime(12, 45));
		}

		protected override ZTimeTimeEditHelper TimeHelper
		{
			get { return new ZTimeTimeEditExHelper(); }
		}
	}
}
