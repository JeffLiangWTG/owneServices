using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZLookupLabelBaseTest : ZLabelBaseTest
	{
		public void TestDisplayStyle()
		{
			ZLabelTestO.DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription;
			AssertEquals("DisplayStyle", OComboBoxDropDownStyle.CodeAndDescription, ZLabelTestO.DisplayStyle);

			ZLabelTestO.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			AssertEquals("DisplayStyle", OComboBoxDropDownStyle.CodeOnly, ZLabelTestO.DisplayStyle);

			ZLabelTestO.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AssertEquals("DisplayStyle", OComboBoxDropDownStyle.DescriptionOnly, ZLabelTestO.DisplayStyle);
		}

		public sealed override void TestShouldEncodeTextToAvoidCrossSiteScripting()
		{
			Assert("Test not required for labels binding to a list, as CrossSite Scripting is not possible in this scenario", true);
		}

		public sealed override void TestDoNotEncodeHtmlTextWhenDisabled()
		{
			Assert("Test not required for labels binding to a list, as CrossSite Scripting is not possible in this scenario", true);
		}

		#region Setup

		protected new ZLookupLabelBase ZLabelTestO
		{
			get { return (ZLookupLabelBase)base.ZLabelTestO; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			ZLabelTestO.BindToList = BindToListPropertyName;
		}

		protected virtual string BindToListPropertyName { get { return "Collection"; } }

		#endregion
	}
}
