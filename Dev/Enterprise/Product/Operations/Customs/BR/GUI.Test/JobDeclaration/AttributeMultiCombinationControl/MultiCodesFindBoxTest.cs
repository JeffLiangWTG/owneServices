using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class MultiCodesFindBoxTest : BaseFindBoxTest
	{
		protected override ZFindBoxUserControl NewFindBoxTester => new MultiCodesFindBox();
	}
}
