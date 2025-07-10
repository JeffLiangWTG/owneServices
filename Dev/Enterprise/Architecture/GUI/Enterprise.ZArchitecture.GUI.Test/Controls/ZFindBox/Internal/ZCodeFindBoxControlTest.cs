using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZCodeFindBoxControlTest : ZControlBaseTestCase<ZCodeFindBox>
	{
		protected override string[] BindablePropertyNames
		{
			get { return new string[] { "ReadOnly", "MaxLength", "List", "IsVisibleForBinding" }; }
		}

		protected override void BindControl()
		{
			Control.BindToList = "Collection";
			Control.SetDataBinding(Dummy, DummyBizoSchema.Z0_Bool.Name);
		}
	}
}
