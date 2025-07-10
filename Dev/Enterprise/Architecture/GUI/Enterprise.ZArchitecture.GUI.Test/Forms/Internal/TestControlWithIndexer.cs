using System.Windows.Forms;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	sealed class TestControlWithIndexer : Control
	{
		public int this[int i] => i;
	}
}
