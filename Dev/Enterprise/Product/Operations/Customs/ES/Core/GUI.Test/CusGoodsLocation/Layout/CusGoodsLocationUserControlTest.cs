using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	public class CusGoodsLocationUserControlTest : TestCaseWithFactory
	{
		public void TestAuthorizationCodeFindBox()
		{
			var esAuthorizationCodeFindBox = control.ESAuthorizationCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", esAuthorizationCodeFindBox);
				AssertEquals("BindTo", "Address.AuthorisationNumber", esAuthorizationCodeFindBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CusGoodsLocationUserControl();
		}
		CusGoodsLocationUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
