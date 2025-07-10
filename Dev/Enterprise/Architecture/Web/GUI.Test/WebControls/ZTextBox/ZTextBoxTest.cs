using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZTextBoxTest : ZTextBoxBaseTest
	{
		#region setup

		protected override Control GetNewControl()
		{
			return new ZTextBox();
		}

		protected override IZType TestValue
		{
			get { return new ZString("sample1"); }
		}

		protected override IZType TestValue2
		{
			get { return new ZString("sample2"); }
		}

		protected override IZType TestValueTooLong => new ZString('a', TestBizO.Z0_DescriptionInfo.MaxLength + 1);

		protected override ZPropertyInfo BindToProperty
		{
			get { return TestBizO.Z0_DescriptionInfo; }
		}

		protected override bool BindToProperty_ReadOnly
		{
			get => TestBizO.Z0_Description_ReadOnly;
			set => TestBizO.Z0_Description_ReadOnly = value;
		}

		ZTextBox TestTextBox
		{
			get { return (ZTextBox)Control; }
		}

		#endregion

		public void TestCharacterCasing()
		{
			TestTextBox.CharacterCasing = CharacterCasing.Lower;
			AssertEquals(CharacterCasing.Lower, TestTextBox.CharacterCasing);

			TestTextBox.CharacterCasing = CharacterCasing.Upper;
			AssertEquals(CharacterCasing.Upper, TestTextBox.CharacterCasing);
		}
	}
}
