using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZAddressFindBoxTest : BaseZGuidFindBoxTest
	{
		protected override ZFindBoxUserControl NewFindBoxTester
		{
			get { return new ZAddressFindBox(); }
		}
	}
}
