using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZTimeEditHelperTest : ZTimeEditHelperTest2
	{
		public override void TestGetTextFromTime()
		{
			AssertGetTextFromTime("999:45", new ZDateTime(2006, 1, 1).AddHours(999).AddMinutes(45));
			AssertGetTextFromTime("999:03", new ZDateTime(2006, 1, 1).AddHours(999).AddMinutes(3));
			AssertGetTextFromTime("-999:03", new ZDateTime(2006, 1, 1).AddHours(-999).AddMinutes(-3));
		}

		protected override ZTimeEditHelper TimeHelper
		{
			get { return new ZTimeEditExHelper(); }
		}
	}
}
