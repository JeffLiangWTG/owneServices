namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZFindBoxLabelBaseTest : ZLookupLabelBaseTest
	{
		protected new ZFindBoxLabelBase ZLabelTestO
		{
			get { return (ZFindBoxLabelBase)base.ZLabelTestO; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			ZLabelTestO.BindToList = BindToListPropertyName;
		}
	}
}
