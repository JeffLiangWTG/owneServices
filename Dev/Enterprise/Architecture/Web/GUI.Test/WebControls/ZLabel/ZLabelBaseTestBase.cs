using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZLabelBaseTestBase : TestCaseWithFactory
	{
		public virtual void TestTransformSpecialSpaceCharactersToSpaces()
		{
			ZLabelTestO.Text = "<script>Spaces. .\u00A0.\u2007.\u202F.</script>";
			AssertEquals("&lt;script&gt;Spaces. . . . .&lt;/script&gt;", ZLabelTestO.Text);

			ZLabelTestO.EnableHtmlEncoding = false;
			AssertEquals("<script>Spaces. .\u00A0.\u2007.\u202F.</script>", ZLabelTestO.Text);
		}

		protected DummyEnterpriseBusinessObject TestBizO;
		protected ZLabelBase ZLabelTestO;

		protected abstract ZLabelBase GetNewLabel();

		protected override void SetUp()
		{
			base.SetUp();
			TestBizO = GetNewDataSource();
			ZLabelTestO = GetNewLabel();
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected virtual DummyEnterpriseBusinessObject GetNewDataSource()
		{
			return Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
		}
	}
}
