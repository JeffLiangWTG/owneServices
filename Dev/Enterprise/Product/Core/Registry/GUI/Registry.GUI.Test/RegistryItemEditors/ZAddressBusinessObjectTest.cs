using CargoWise.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ZAddressBusinessObject))]
	sealed class ZAddressBusinessObjectTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		protected override void MasterSetUp()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			base.MasterSetUp();
		}
	}
}
