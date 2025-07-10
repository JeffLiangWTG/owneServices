using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	public class ImportOrganizationUserControlTest : TestCaseWithFactory
	{
		public void TestBuyerOrganisationGuidFindBoxVisibility()
		{
			using (var userControl = new ImportOrganizationUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				userControl.SetDataBinding(declaration, string.Empty);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				CombineAssertions(() =>
				{
					var buyerOrganisationGuidFindBox = userControl.FindSingle<ZGuidFindBox>("BuyerOrganisationGuidFindBox");
					AssertEquals("Buyer find box should not be visible on Delta G declarations.", false, buyerOrganisationGuidFindBox.Visible);
					AssertEquals("Buyer find box should bind to \"JE_OH_Buyer\".", "JE_OH_Buyer", buyerOrganisationGuidFindBox.BindTo);
				});

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				CombineAssertions(() =>
				{
					var buyerOrganisationGuidFindBox = userControl.FindSingle<ZGuidFindBox>("BuyerOrganisationGuidFindBox");
					AssertEquals("Buyer find box should be visible on Delta I/E declarations.", true, buyerOrganisationGuidFindBox.Visible);
					AssertEquals("Buyer find box should bind to \"JE_OH_Buyer\".", "JE_OH_Buyer", buyerOrganisationGuidFindBox.BindTo);
				});
			}
		}
	}
}
