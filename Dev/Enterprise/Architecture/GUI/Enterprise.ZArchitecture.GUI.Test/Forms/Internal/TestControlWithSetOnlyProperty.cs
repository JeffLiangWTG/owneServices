using System.Windows.Forms;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	sealed class TestControlWithSetOnlyProperty : Control
	{
		public string Foo { set { } }
	}
}
