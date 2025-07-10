namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	public class HBLAndHAWBBrandingOptionEditorInfoTest : NUnit.Framework.TestCase
	{
		public void TestBaseDataTypeToBeEdited()
		{
			AssertEquals("BaseDataTypeToBeEdited", typeof(string), EditorInfo.BaseDataTypeToBeEdited);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			EditorInfo = new HBLAndHAWBBrandingOptionEditorInfo();
		}

		HBLAndHAWBBrandingOptionEditorInfo EditorInfo;

		#endregion
	}
}
