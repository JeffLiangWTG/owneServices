using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZDescriptionPkFindBoxColumnStyleInfoTest : TestCaseWithDummy
	{
		public void TestZDescriptionCodeFindBoxColumnStyleInfo()
		{
			var propertyDescriptor = TypeDescriptor.GetProperties(Dummy)[DummyBizoSchema.Z0_Guid.Name];
			var columnStyleInfo = new ZDescriptionPkFindBoxColumnStyleInfo(propertyDescriptor);

			AssertEquals(typeof(ZDescriptionPkFindBoxColumnStyle), columnStyleInfo.ColumnStyleType);
			AssertEquals(CharacterCasing.Normal, columnStyleInfo.CharacterCasing);
			AssertEquals(propertyDescriptor, ((IOverridablePropertyDescriptor)columnStyleInfo).PropertyDescriptor);
		}
	}
}
