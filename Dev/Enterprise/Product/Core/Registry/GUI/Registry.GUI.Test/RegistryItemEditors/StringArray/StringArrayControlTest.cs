using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(StringArrayControl))]
	sealed class StringArrayControlTest : Testing.RegistryZUserControlTestCase
	{
		public void TestGridDoesNotAllowSorting()
		{
			using (StringArrayControl control = new StringArrayControl())
			{
				AssertEquals("AllowSorting", false, control.StringGrid.AllowSorting);
			}
		}

		public void TestCharacterCasing()
		{
			using (StringArrayControl control = new StringArrayControl(CharacterCasing.Upper))
			{
				AssertEquals(CharacterCasing.Upper, control.zTextBoxColumnStyleInfo1.CharacterCasing);
			}
			using (StringArrayControl control = new StringArrayControl(CharacterCasing.Lower))
			{
				AssertEquals(CharacterCasing.Lower, control.zTextBoxColumnStyleInfo1.CharacterCasing);
			}
			using (StringArrayControl control = new StringArrayControl(CharacterCasing.Normal))
			{
				AssertEquals(CharacterCasing.Normal, control.zTextBoxColumnStyleInfo1.CharacterCasing);
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new StringLineCollection(new DelimitedStringArrayRegistryDataType());
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((StringArrayControl)control).StringGrid.ReadOnly;
		}
	}
}
