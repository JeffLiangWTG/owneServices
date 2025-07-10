using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZDescriptionCodeFindBoxColumnStyleInfoTest : TestCaseWithDummy
	{
		public void TestZDescriptionCodeFindBoxColumnStyleInfo()
		{
			var propertyDescriptor = TypeDescriptor.GetProperties(Dummy)[DummyBizoSchema.Z0_Code.Name];
			var columnStyleInfo = new ZDescriptionCodeFindBoxColumnStyleInfo(propertyDescriptor);

			AssertEquals(typeof(ZDescriptionCodeFindBoxColumnStyle), columnStyleInfo.ColumnStyleType);
			AssertEquals(CharacterCasing.Normal, columnStyleInfo.CharacterCasing);
			AssertEquals(propertyDescriptor, ((IOverridablePropertyDescriptor)columnStyleInfo).PropertyDescriptor);
		}
	}
}
